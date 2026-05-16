using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrthoSpineAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedTestHs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Hs",
                table: "MedTests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hs",
                table: "MedTests");
        }
    }
}
