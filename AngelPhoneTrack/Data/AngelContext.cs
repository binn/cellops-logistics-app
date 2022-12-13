using Microsoft.EntityFrameworkCore;

namespace AngelPhoneTrack.Data
{
    public class AngelContext : DbContext
    {
        public AngelContext(DbContextOptions<AngelContext> opts) : base(opts) { }

        public DbSet<Lot> Lots { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
    }
}
