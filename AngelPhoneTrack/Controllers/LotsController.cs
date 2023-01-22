using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using AngelPhoneTrack.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Telegram.Bot;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [Route("/lots")]
    [AngelAuthorized(admin: false, supervisor: false)]
    public class LotsController : AngelControllerBase
    {
        private readonly AngelContext _ctx;
        private readonly TelegramBotClient _bot;

        public LotsController(AngelContext ctx, TelegramBotClient bot)
        {
            _ctx = ctx;
            _bot = bot;
        }

        [HttpGet]
        public async Task<IActionResult> GetLotsAsync([FromQuery] int page = 1, [FromQuery] string? lotNo = null, [FromQuery] bool archived = false)
        {
            page = page < 1 ? 1 : page;
            var query = _ctx.Lots
                .OrderByDescending(x => x.Audits.OrderByDescending(x => x.Timestamp).FirstOrDefault()!.Timestamp) // at least one audit should exist for lot creation
                .Where(x => x.Archived == archived)
                .Select(lot => new
                {
                    lot.Id,
                    lot.LotNo,
                    lot.Count,
                    lot.Model,
                    lot.Grade,
                    lot.Timestamp,
                    lot.Archived,
                    lot.ArchivedAt,
                    lot.Priority,
                    lot.Expiration,
                    New = lot.Timestamp.AddHours(1) >= DateTimeOffset.UtcNow,
                    DueSoon = DateTimeOffset.UtcNow >= lot.Expiration.AddHours(-1) && DateTimeOffset.UtcNow < lot.Expiration,
                    Late = lot.Expiration <= DateTimeOffset.UtcNow,
                    Assignments = lot.Assignments.Select(x => new
                    {
                        x.Department.Id,
                        x.Count
                    })
                });

            if (!string.IsNullOrWhiteSpace(lotNo))
                query = query.Where(x => x.LotNo.Contains(lotNo.ToUpper())); // updated this logic cuz why not

            var results = await query.ToPagedListAsync(page, 25);

            return Ok(new
            {
                results.TotalCount,
                results.TotalPages,
                results.CurrentPage,
                results
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateLotAsync([FromBody] CreateLotRequest request)
        {
            if (request.Count < 1)
                return BadRequest(new { error = "Lot requires at least one item." });

            if (string.IsNullOrWhiteSpace(request.LotNo))
                return BadRequest(new { error = "Lot number must not be blank." });

            bool existsAlready = await _ctx.Lots.AnyAsync(x => x.LotNo == request.LotNo);
            if (existsAlready)
                return BadRequest(new { error = "Lot already exists." });

            if (request.Expiration != "1h" && request.Expiration != "24h" && request.Expiration != "48h" && request.Expiration != "72h" && request.Expiration != "1w")
                return BadRequest(new { error = "Bad expiration time." });

            Department? assignedDepartment = await _ctx.Departments.FindAsync(request.Department);
            if (assignedDepartment == null)
                return BadRequest(new { error = "Department doesn't exist." });

            var lot = new Lot(request.LotNo.Trim(), request.Count);
            var tasks = request.Tasks.Where(x => x > 0).ToList();
            var taskTemplates = await _ctx.Templates.Where(x => tasks.Contains(x.Id)).ToListAsync();
            var departments = await _ctx.Departments.Where(x => x.IsAssignable).ToListAsync();
            lot.Model = request.Model;

            if (request.Grade != "NEW" && request.Grade != "CPO" && request.Grade != "UNKNOWN")
            {
                if (request.Grade.Length > 2)
                    return BadRequest(new { error = "Invalid grade!" });

                char[] grades = { 'A', 'B', 'C', 'D' };
                if (!grades.Contains(request.Grade[0]))
                    return BadRequest(new { error = "Invalid grade!" });

                if (request.Grade.Length == 2)
                {
                    if ((request.Grade[1] != '+' && request.Grade[1] != '-') && request.Grade[0] != 'D')
                        return BadRequest(new { error = "Invalid grade!" });
                }
            }

            DateTimeOffset expiration;

            expiration = request.Expiration switch
            {
                "1h" => DateTimeOffset.UtcNow.AddMinutes(75),
                "24h" => DateTimeOffset.UtcNow.AddDays(1),
                "48h" => DateTimeOffset.UtcNow.AddDays(2),
                "72h" => DateTimeOffset.UtcNow.AddDays(3),
                "1w" => DateTimeOffset.UtcNow.AddDays(5),
                _ => DateTimeOffset.UtcNow.AddMinutes(75),
            };

            if (expiration.DayOfWeek == DayOfWeek.Saturday)
                expiration = expiration.AddDays(2);

            if (expiration.DayOfWeek == DayOfWeek.Sunday)
                expiration = expiration.AddDays(1);

            if (request.Priority == Priority.Immediate)
                expiration = DateTimeOffset.UtcNow.Date.AddHours(22);

            lot.Expiration = expiration;
            lot.Priority = request.Priority;
            lot.Grade = request.Grade;
            foreach (var task in taskTemplates)
                lot.CreateTask(task.Template, task.Category, task.Id);

            foreach (var department in departments)
            {
                var lotAssignment = new LotAssignment()
                {
                    Lot = lot,
                    Count = department.Id == assignedDepartment.Id ? lot.Count : 0,
                    Department = department,
                };

                lot.Assignments.Add(lotAssignment);
            }

            lot.CreateAudit(Employee!, Employee!.Department, "LOT_CREATED");
            await _ctx.Lots.AddAsync(lot);
            await _ctx.SaveChangesAsync();

            await _bot.SendTextMessageAsync(-1001590253664, $"Lot {lot.LotNo} recieved in {assignedDepartment.Name} with {lot.Count} of {lot.Model}.\n(triggered by: {Employee!.Name})");

            return Ok(new
            {
                lot.Id,
                lot.LotNo,
                lot.Count,
                lot.Model,
                lot.Grade,
                lot.Timestamp,
                Assignments = lot.Assignments.Select(x => new
                {
                    x.Department.Id,
                    x.Count
                })
            });
        }

        [HttpPost("{id}/archive")]
        [AngelAuthorized(supervisor: true)]
        public async Task<IActionResult> ArchiveLotAsync(Guid id)
        {
            var lot = await _ctx.Lots.Include(x => x.Tasks).FirstOrDefaultAsync(x => x.Id == id);
            if (lot == null)
                return BadRequest(new { error = "Lot doesn't exist." });

            if (!lot.Tasks.All(x => x.Completed))
                return BadRequest(new { error = "All tasks must be completed before archival" });

            if (lot.Archived)
                return BadRequest(new { error = "Lot is already archived" });

            lot.Archived = true;
            lot.ArchivedAt = DateTimeOffset.UtcNow;

            await _ctx.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("{id}/unarchive")]
        [AngelAuthorized(supervisor: true)]
        public async Task<IActionResult> UnarchiveLotAsync(Guid id)
        {
            var lot = await _ctx.Lots.Include(x => x.Tasks).FirstOrDefaultAsync(x => x.Id == id);
            if (lot == null)
                return BadRequest(new { error = "Lot doesn't exist." });

            if (!lot.Archived)
                return BadRequest(new { error = "Lot is not archived." });

            lot.Archived = false;
            lot.ArchivedAt = null;

            await _ctx.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("{id}/assignments")]
        public async Task<IActionResult> UpdateLotAssignmentsAsync(Guid id, [FromBody] LotAssignmentRequest[] assignments)
        {
            var lot = await _ctx.Lots.Include(x => x.Assignments).ThenInclude(x => x.Department).FirstOrDefaultAsync(x => x.Id == id);
            if (lot == null)
                return BadRequest(new { error = "Lot doesn't exist." });

            if (!assignments.All(x => lot.Assignments.Any(a => a.Department.Id == x.Id)))
                return BadRequest(new { error = "Missing departments." });

            if (assignments.Sum(x => x.Count) != lot.Count)
                return BadRequest(new { error = "Mismatch of reassignment count." });

            if (assignments.Any(x => x.Count < 0))
                return BadRequest(new { error = "Cannot introduce negative." });

            if (assignments.All(x => lot.Assignments.Any(a => a.Department.Id == x.Id && a.Count == x.Count)))
                return BadRequest(new { error = "No change detected." });

            var oldAssignments = lot.Assignments.Select(x => new { x.Department.Id, x.Count }).ToArray();
            foreach (var assignment in lot.Assignments)
            {
                var newAssignment = assignments.FirstOrDefault(x => x.Id == assignment.Department.Id);
                assignment.Count = newAssignment!.Count;
            }

            lot.CreateAudit(Employee!, Employee!.Department, "LOT_REASSIGNED",
                JsonSerializer.Serialize(new
                {
                    old = oldAssignments,
                    updated = lot.Assignments.Select(x => new { x.Department.Id, x.Count })
                }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            await _ctx.SaveChangesAsync();
            return Ok(new
            {
                lot.Id,
                lot.LotNo,
                lot.Count,
                lot.Model,
                lot.Grade,
                lot.Timestamp,
                Assignments = lot.Assignments.Select(x => new
                {
                    x.Department.Id,
                    x.Count
                })
            });
        }

        [HttpGet("/search/{lotNo}")]
        public async Task<IActionResult> GetLotByLotNoAsync(string lotNo)
        {
            Guid? lot = await _ctx.Lots.Where(x => x.LotNo == lotNo).Select(x => x.Id).FirstOrDefaultAsync();
            if (lot == null)
                return BadRequest(new { error = "Lot doesn't exist. " });

            return await GetLotAsync(lot.GetValueOrDefault());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLotAsync(Guid id)
        {
            var lot = await _ctx.Lots
                .Include(x => x.Assignments)
                    .ThenInclude(x => x.Department)
                .Include(x => x.Audits)
                    .ThenInclude(x => x.Department)
                .Include(x => x.Notes)
                    .ThenInclude(x => x.Department)
                .Select(x => new
                {
                    x.Id,
                    x.LotNo,
                    x.Count,
                    x.Model,
                    x.Grade,
                    x.Priority,
                    x.Archived,
                    x.Timestamp,
                    x.ArchivedAt,
                    x.Expiration,
                    x.Tasks,
                    Assignments = x.Assignments.Select(x => new
                    {
                        x.Department.Id,
                        x.Count
                    }),
                    Notes = x.Notes.OrderByDescending(x => x.Timestamp).Select(x => new
                    {
                        x.Id,
                        Department = x.Department.Id,
                        x.CreatedBy,
                        x.Data,
                        x.Timestamp
                    }),
                    Audits = x.Audits.OrderByDescending(x => x.Timestamp).Select(x => new
                    {
                        x.Id,
                        x.CreatedBy,
                        Department = x.Department.Id,
                        x.Type,
                        x.Data,
                        x.Timestamp,
                    })
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lot == null)
                return BadRequest(new { error = "Lot doesn't exist." });

            return Ok(lot);
        }

        [HttpDelete("{id}")]
        [AngelAuthorized(supervisor: true)]
        public async Task<IActionResult> DeleteLotAsync(Guid id)
        {
            var lot = await _ctx.Lots.FirstOrDefaultAsync(x => x.Id == id);
            if (lot == null)
                return BadRequest(new { error = "Lot doesn't exist. " });

            _ctx.Lots.Remove(lot);
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }

        public record LotAssignmentRequest(int Count, int Id);
        public record CreateLotRequest(string LotNo, int Count, int Department, int[] Tasks, string Model, string Grade, Priority Priority, string Expiration);
    }
}
