using AngelPhoneTrack.Data;
using Microsoft.AspNetCore.Mvc;

namespace AngelPhoneTrack.Controllers
{
    public class NotesController : AngelControllerBase
    {
        private AngelContext _ctx;

        public NotesController(AngelContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IActionResult> AddNoteToLotAsync(Guid lotId)
        {
            return Ok();
        }

        // remove note from lot (delete note)
        // update note
        // audits for note
    }
}
