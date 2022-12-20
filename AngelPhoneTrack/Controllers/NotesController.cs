using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [Route("/notes")]
    [AngelAuthorized]
    public class NotesController : AngelControllerBase
    {
        private AngelContext _ctx;

        public NotesController(AngelContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet("/_migrations/seed")]
        public async Task<IActionResult> SeedTasksAsync()
        {
            var lots = await _ctx.Lots.ToListAsync();
            var templates = await _ctx.Templates.ToListAsync();

            foreach (var lot in lots)
                foreach (var template in templates)
                    lot.CreateTask(template.Template, template.Category);

            await _ctx.SaveChangesAsync();
            return Ok("Done");
        }

        [HttpPost("/tasks/{taskId}/complete")]
        public async Task<IActionResult> ChangeTaskCompletedStatusAsync(Guid taskId, [FromQuery] bool completed)
        {
            var task = await _ctx.Tasks.Include(x => x.Lot).FirstOrDefaultAsync(x => x.Id == taskId);
            if (task == null)
                return BadRequest(new { error = "Task doesn't exist." });

            task.Completed = completed;
            task.Lot.CreateAudit(Employee!, Employee!.Department, completed ? "TASK_COMPLETED" : "TASK_UNCOMPLETED", "Task \"" + task.Name + "\" updated.");

            await _ctx.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("lots/{lotId}")]
        public async Task<IActionResult> AddNoteToLotAsync(Guid lotId, [FromBody] string data)
        {
            var lot = await _ctx.Lots.FindAsync(lotId);
            if (lot == null)
                return BadRequest(new { error = "Lot doesn't exist." });

            if (string.IsNullOrWhiteSpace(data))
                return BadRequest(new { error = "Note must not be blank." });

            lot.CreateNote(Employee!, Employee!.Department, data);
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpDelete("{noteId}")]
        [AngelAuthorized(supervisor: true)]
        public async Task<IActionResult> DeleteNoteAsync(Guid noteId)
        {
            var note = await _ctx.Notes.FindAsync(noteId);
            if (note == null)
                return BadRequest(new { error = "Note doesn't exist." });

            _ctx.Notes.Remove(note);
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // update note (maybe?)
    }
}
