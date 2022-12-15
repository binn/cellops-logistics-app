using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using AngelPhoneTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngelPhoneTrack.Controllers
{
    [ApiController]
    [Route("/admin")]
    [AngelAuthorized(admin: true)]
    public class AdministrationController : AngelControllerBase
    {
        private readonly AngelContext _ctx;
        private readonly ILogger<AdministrationController> _logger;

        public AdministrationController(ILogger<AdministrationController> logger, AngelContext ctx)
        {
            _ctx = ctx;
            _logger = logger;
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            return Ok(await _ctx.Employees
                .Where(x => x.Name != "Superuser")
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.Pin,
                    x.Name,
                    x.Admin,
                    x.Supervisor,
                    x.Timestamp,
                    Department = new
                    {
                        x.Department.Id,
                        x.Department.Name
                    }
                })
                .ToListAsync());
        }

        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployeeAsync([FromBody] EmployeeCreateRequest request)
        {
            Department? dept = await _ctx.Departments.FindAsync(request.Department);
            if (dept == null)
                return BadRequest(new { error = "Department doesn't exist." });

            var employee = new Employee()
            {
                Name = request.Name,
                Supervisor = request.Supervisor,
                Admin = request.Admin,
                Pin = request.Pin,
                Token = Guid.NewGuid().ToString()
            };

            dept.Employees.Add(employee);
            await _ctx.SaveChangesAsync();

            return Ok(new EmployeeResponse(employee));
        }

        [HttpDelete("employees/{id}")]
        public async Task<IActionResult> DeleteEmployeeAsync(int id)
        {
            Employee? employee = await _ctx.Employees.FindAsync(id);
            if(employee == null)
                return BadRequest(new { error = "Employee doesn't exist." });

            if (employee.Name == "Superuser")
                return BadRequest(new { error = "Cannot delete superuser account." });

            if (employee.Id == Employee!.Id)
                return BadRequest(new { error = "Cannot delete self." });

            _ctx.Employees.Remove(employee);
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("employees/{id}")]
        public async Task<IActionResult> UpdateEmployeeAsync(int id, [FromBody] EmployeeCreateRequest incoming)
        {
            Employee? employee = await _ctx.Employees.Include(x => x.Department).FirstOrDefaultAsync(x => x.Id == id);
            if (employee == null)
                return BadRequest(new { error = "Employee doesn't exist." });

            if (employee.Name == "Superuser")
                return BadRequest(new { error = "Cannot update superuser account." });

            employee.Supervisor = incoming.Supervisor;
            employee.Admin = incoming.Admin;
            employee.Name = incoming.Name;
            
            if(incoming.Department != employee.Department.Id)
            {
                Department? department = await _ctx.Departments.FindAsync(incoming.Department);
                if (department == null)
                    return BadRequest(new { error = "Updated department doesn't exist. " });

                employee.Department = department;
            }

            if(incoming.Pin != employee.Pin)
            {
                employee.Pin = incoming.Pin;
                employee.Token = Guid.NewGuid().ToString();
            }

            await _ctx.SaveChangesAsync();
            return Ok(new EmployeeResponse(employee));
        }

        public record EmployeeCreateRequest(string Name, bool Admin, bool Supervisor, string Pin, int Department);

        [HttpGet("departments")]
        public async Task<IActionResult> GetAllDepartments()
        {
            return Ok(await _ctx.Departments
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    Employees = x.Employees.Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.Admin,
                        e.Supervisor
                    })
                })
                .ToListAsync());
        }

        [HttpPost("departments")]
        public async Task<IActionResult> CreateDepartmentAsync([FromBody] CreateDepartmentRequest request)
        {
            var existingDepartment = await _ctx.Departments.FirstOrDefaultAsync(x => x.Name == request.Name);
            if (existingDepartment != null)
                return BadRequest(new { error = "Department already exists." });

            var department = new Department()
            {
                Name = request.Name,
                Description = request.Description,
                IsAssignable = true
            };

            await _ctx.Departments.AddAsync(department);
            await _ctx.SaveChangesAsync();

            return Ok(new DepartmentResponseWithDescription(department));
        }

        [HttpDelete("departments/{id}")]
        public async Task<IActionResult> DeleteDepartmentAsync(int id)
        {
            Department? department = await _ctx.Departments.FindAsync(id);
            if (department == null)
                return BadRequest(new { error = "Department doesn't exist. " });

            if (department.Name == "HR")
                return BadRequest(new { error = "Cannot delete HR department." });

            _ctx.Departments.Remove(department); // implement code for moving existing employees to another department
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }

        public record CreateDepartmentRequest(string Name, string Description);
    }
}