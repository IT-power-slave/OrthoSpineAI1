using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrthoSpineAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedTestStageSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "MedTestStages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "MedTestStages");
        }
    }
}
