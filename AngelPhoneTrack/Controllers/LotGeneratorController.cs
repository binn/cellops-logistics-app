using AngelPhoneTrack.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [Route("/lots")]
    public class LotGeneratorController : AngelControllerBase
    {
        private readonly AngelContext _ctx;

        public LotGeneratorController(AngelContext ctx) =>
            _ctx = ctx;

        [HttpGet("{id}/report")]
        public async Task<IActionResult> GetReportAsync(Guid id, [FromQuery] string name)
        {
            var lot = await _ctx.Lots
                .Include(x => x.Audits)
                .Include(x => x.Assignments)
                    .ThenInclude(x => x.Department)
                .Include(x => x.Tasks)
                .Include(x => x.Notes)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lot == null)
                return NotFound();

            var data = new
            {
                lot.Id,
                lot.LotNo,
                CreatedAt = lot.Timestamp,
                lot.Audits.FirstOrDefault(x => x.Type == "LOT_CREATED")!.CreatedBy,
                Assignments = lot.Assignments.Select(x => new
                {
                    x.Department.Name,
                    x.Count
                }),
                Tasks = lot.Tasks.Select(x => new
                {
                    x.Name,
                    x.Completed,
                    x.Category
                }),
                Notes = lot.Notes.Select(x => new
                {
                    x.Data,
                    x.CreatedBy,
                    x.Timestamp
                }),
                lot.Priority,
                lot.Expiration,
                lot.Grade,
                lot.Model,
                lot.Count,
                PrintedBy = Employee?.Name ?? name ?? "Angel Cellular LLC",
                DueSoon = DateTimeOffset.UtcNow >= lot.Expiration.AddHours(-1) && DateTimeOffset.UtcNow < lot.Expiration,
                Late = lot.Expiration <= DateTimeOffset.UtcNow
            };

            var dataSerialized = JsonSerializer.Serialize(data, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return Redirect("https://angelpt-reports-rqed7.ondigitalocean.app/?incoming=" + dataSerialized);
        }
    }
}
