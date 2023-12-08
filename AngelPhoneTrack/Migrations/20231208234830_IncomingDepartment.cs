using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngelPhoneTrack.Migrations
{
    /// <inheritdoc />
    public partial class IncomingDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IncomingDepartmentId",
                table: "Assignments",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncomingDepartmentId",
                table: "Assignments");
        }
    }
}
