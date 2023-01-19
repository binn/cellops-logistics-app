using AngelPhoneTrack.Data;
using AngelPhoneTrack.Filters;
using AngelPhoneTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
                FirstName = request.FirstName,
                LastName = request.LastName,
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
            employee.FirstName = incoming.FirstName;
            employee.LastName = incoming.LastName;

            if (incoming.Department != employee.Department.Id)
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

        public record EmployeeCreateRequest(string FirstName, string LastName, bool Admin, bool Supervisor, string Pin, int Department);

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
                Default = request.Default,
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

            var lots = await _ctx.Lots
                .Include(x => x.Assignments)
                .ThenInclude(x => x.Department)
                .Where(x => x.Assignments.Any(d => d.Department.Id == department.Id && d.Count > 0)).ToListAsync();

            foreach(var lot in lots)
            {
                var oldAssignments = lot.Assignments.Select(x => new { x.Department.Id, x.Count });

                var defaultDepartment = lot.Assignments.First(x => x.Department.Default);
                var match = lot.Assignments.First(x => x.Department.Id == department.Id);
                defaultDepartment.Count += match.Count;
                lot.Assignments.Remove(match);

                lot.CreateAudit(Employee!, Employee!.Department, "LOT_DEPARTMENT_DELETED",
                    JsonSerializer.Serialize(new
                    {
                        old = oldAssignments,
                        updated = lot.Assignments.Select(x => new { x.Department.Id, x.Count })
                    }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            }

            _ctx.Departments.Remove(department); // implement code for moving existing employees to another department
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }

        public record CreateDepartmentRequest(string Name, string Description, bool Default = false);
    }
}