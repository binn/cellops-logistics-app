using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngelPhoneTrack.Migrations
{
    /// <inheritdoc />
    public partial class Acknowledgement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Received",
                table: "Assignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Received",
                table: "Assignments");
        }
    }
}
