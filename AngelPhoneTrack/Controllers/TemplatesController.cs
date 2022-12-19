using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [AngelAuthorized]
    [Route("/tasks/templates")]
    public class TemplatesController : AngelControllerBase
    {
        private readonly AngelContext _ctx;

        public TemplatesController(AngelContext ctx) =>
            _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetTaskTemplatesAsync()
        {
            return Ok(await _ctx.Templates.ToListAsync());
        }

        [HttpPost]
        [AngelAuthorized(admin: true)]
        public async Task<IActionResult> CreateTaskTemplateAsync([FromBody] [Required(AllowEmptyStrings = false)] [MinLength(3)] string template, [FromBody] string category)
        {
            if (category != "TESTING" || category != "GRADING")
                return BadRequest(new { error = "Bad category. Category doesn't exist." });
            var nt = new TaskTemplate()
            {
                Template = template
            };

            await _ctx.Templates.AddAsync(nt);
            await _ctx.SaveChangesAsync();

            return Ok(nt);
        }


        [HttpDelete("{id}")]
        [AngelAuthorized(admin: true)]
        public async Task<IActionResult> DeleteTaskTemplateAsync(int id)
        {
            var nt = await _ctx.Templates.FindAsync(id);
            if (nt == null)
                return BadRequest(new { error = "Task template doesn't exist." });

            _ctx.Templates.Remove(nt);
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
