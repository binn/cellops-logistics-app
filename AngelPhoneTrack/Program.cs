using AngelPhoneTrack.Data;
using Microsoft.EntityFrameworkCore;

namespace AngelPhoneTrack
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddDbContext<AngelContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("MAIN")));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            await MigrateAndSeedDatabaseAsync(app);

            app.UseSwagger(); // originally behind dev environments only
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }

        private static async Task MigrateAndSeedDatabaseAsync(WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            using var ctx = app.Services.GetRequiredService<AngelContext>();

            logger.LogInformation("Migrating database...");
            await ctx.Database.MigrateAsync();

            logger.LogInformation("Validating database seed...");
            Department? hr = await ctx.Departments.FirstOrDefaultAsync(d => d.Name == "HR");
            Employee? superuser = await ctx.Employees.FirstOrDefaultAsync(x => x.Name == "Superuser");

            if (hr == null)
            {
                logger.LogInformation("HR department missing, creating one now...");
                hr = new Department()
                {
                    Name = "HR",
                    Description = "Department representing Human Resources (HR) team, for managing the application.",
                    IsAssignable = false
                };

                await ctx.Departments.AddAsync(hr);
                await ctx.SaveChangesAsync();
                logger.LogInformation("Created HR department: {hr}", hr);
            }

            if (superuser == null)
            {
                logger.LogInformation("Superuser missing, creating one now.");
                superuser = new Employee()
                {
                    IsAdmin = true,
                    Name = "Superuser",
                    IsSupervisor = true,
                    Pin = app.Configuration["SuperuserPin"]!
                };

                hr.Employees.Add(superuser);
                await ctx.SaveChangesAsync();
                logger.LogInformation("Created superuser: {superuser}", superuser);
            }
        }
    }
}