using AngelPhoneTrack.Data;
using AngelPhoneTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [Route("/authorization/login")]
    public class AuthorizationController : AngelControllerBase
    {
        private readonly AngelContext _ctx;

        public AuthorizationController(AngelContext ctx) =>
            _ctx = ctx;

        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            Employee? employee = await _ctx.Employees.Include(x => x.Department).FirstOrDefaultAsync(x => x.Pin == request.Pin);
            if (employee == null)
                return Unauthorized(new { success = false, error = "Credentials invalid.", reason = "credentials.invalid" });

            employee.Token = Guid.NewGuid().ToString("N");
            await _ctx.SaveChangesAsync();

            var resp = new EmployeeResponse(employee)
            { Pin = "****" };

            return Ok(new { success = true, token = employee.Token, employee = resp });
        }
        
        public record LoginRequest(string Pin);
    }
}
