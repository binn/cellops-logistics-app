using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [AngelAuthorized]
    [Route("/departments")]
    public class DepartmentsController : AngelControllerBase
    {
        private readonly AngelContext _ctx;

        public DepartmentsController(AngelContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetDepartmentsAsync()
        {
            return Ok(await _ctx.Departments
                .Where(x => x.IsAssignable)
                .Select(x => new { x.Id, x.Name })
                .ToListAsync());
        }
    }
}
