using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [AngelAuthorized]
    [Route("/notes/templates")]
    public class TemplatesController : AngelControllerBase
    {
        private readonly AngelContext _ctx;

        public TemplatesController(AngelContext ctx) =>
            _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetNoteTemplatesAsync()
        {
            return Ok(await _ctx.Templates.ToListAsync());
        }

        [HttpPost]
        [AngelAuthorized(admin: true)]
        public async Task<IActionResult> CreateNoteTemplateAsync([FromBody] [Required(AllowEmptyStrings = false)] [MinLength(3)] string template)
        {
            var nt = new NoteTemplate()
            {
                Template = template
            };

            await _ctx.Templates.AddAsync(nt);
            await _ctx.SaveChangesAsync();

            return Ok(nt);
        }


        [HttpDelete("{id}")]
        [AngelAuthorized(admin: true)]
        public async Task<IActionResult> DeleteNoteTemplateAsync(int id)
        {
            var nt = await _ctx.Templates.FindAsync(id);
            if (nt == null)
                return BadRequest(new { error = "Note template doesn't exist." });

            _ctx.Templates.Remove(nt);
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
