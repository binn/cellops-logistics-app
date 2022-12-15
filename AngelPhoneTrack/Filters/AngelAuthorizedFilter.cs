using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using AngelPhoneTrack.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using AngelPhoneTrack.Controllers;

namespace AngelPhoneTrack.Filters
{
    public class AngelAuthorizedFilter : IAsyncActionFilter
    {
        private readonly bool _admin;
        private readonly bool _supervisor;

        public AngelAuthorizedFilter(bool admin, bool supervisor)
        {
            _admin = admin;
            _supervisor = supervisor;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            AngelControllerBase controller = (AngelControllerBase)context.Controller;
            using var ctx = context.HttpContext.RequestServices.GetRequiredService<AngelContext>();

            string? header = context.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(header)) // To prevent the database from being hit
            {
                string[] headerParts = header.Split(' '); // This is to give us an authentication scheme that's nice and neat
                if (headerParts.Length == 2 && headerParts[0] == "Bearer")
                {
                    var employee = await ctx.Employees
                        .Include(x => x.Department)
                        .FirstOrDefaultAsync(x => x.Token == headerParts[1]);

                    if (employee != null)
                    {
                        if (_admin && !employee.Admin)
                            goto Unauthorized;
                        else if (_supervisor && !employee.Supervisor)
                            goto Unauthorized; // i hate this implementation
                        else
                        {
                                controller.Employee = employee;
                                await next();
                                return;
                        }
                    }
                }
            }

            Unauthorized:
            context.HttpContext.Response.StatusCode = 401;
            context.Result = new JsonResult(new { error = "UNAUTHORIZED", reason = "credentials.invalid" });
        }
    }

    public class AngelAuthorized : TypeFilterAttribute
    {
        public AngelAuthorized(bool admin = false, bool supervisor = false) : base(typeof(AngelAuthorizedFilter)) 
        {
            Arguments = new object[] { admin, supervisor };
        }
    }
}
