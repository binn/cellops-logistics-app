using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngelPhoneTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeModelNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "Lots",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Lots",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Lots");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Lots");
        }
    }
}
