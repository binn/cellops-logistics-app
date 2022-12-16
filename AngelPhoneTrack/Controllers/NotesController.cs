using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using Microsoft.AspNetCore.Mvc;

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
