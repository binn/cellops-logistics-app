using AngelPhoneTrack.Data;
using AngelPhoneTrack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;

namespace AngelPhoneTrack
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var bot = new TelegramBotClient(builder.Configuration["TelegramToken"]!);
            builder.Services.AddSingleton(bot);
            
            builder.Services.AddControllers();
            builder.Services.AddDbContext<AngelContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("MAIN")));

            builder.Services.AddCors();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", new { context.HostingEnvironment.ApplicationName, context.HostingEnvironment.EnvironmentName }, true)
                    .Destructure.With<JsonDocumentDestructuringPolicy>()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {SourceContext} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

            builder.Services.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = (ctx) =>
                    new BadRequestObjectResult(new { error = ctx.ModelState.SelectMany(x => x.Value!.Errors!.Select(e => e!.ErrorMessage)).FirstOrDefault() });
            });

            var app = builder.Build();
            await MigrateAndSeedDatabaseAsync(app);

            app.UseSwagger(); // originally behind dev environments only
            app.UseSwaggerUI();

            // app.UseHttpsRedirection();
            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }

        private static async Task MigrateAndSeedDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<AngelContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Migrating database...");
            await ctx.Database.MigrateAsync();

            logger.LogInformation("Validating database seed...");
            Department? hr = await ctx.Departments.FirstOrDefaultAsync(d => d.Name == "HR");
            Employee? superuser = await ctx.Employees.FirstOrDefaultAsync(x => x.FirstName == "Superuser");

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
                logger.LogInformation("Created HR department: {@hr}", hr);
            }

            if (superuser == null)
            {
                logger.LogInformation("Superuser missing, creating one now.");
                superuser = new Employee()
                {
                    Admin = true,
                    FirstName = "Superuser",
                    LastName = "",
                    Supervisor = true,
                    Pin = app.Configuration["SuperuserPin"]!,
                    Token = Guid.NewGuid().ToString()
                };

                hr.Employees.Add(superuser);
                await ctx.SaveChangesAsync();
                logger.LogInformation("Created superuser: {@superuser}", superuser);
            }
        }
    }
}