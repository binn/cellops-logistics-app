using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngelPhoneTrack.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAdminName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSupervisor",
                table: "Employees",
                newName: "Supervisor");

            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "Employees",
                newName: "Admin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Supervisor",
                table: "Employees",
                newName: "IsSupervisor");

            migrationBuilder.RenameColumn(
                name: "Admin",
                table: "Employees",
                newName: "IsAdmin");
        }
    }
}
