using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrthoSpineAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedTestDefinitionKeyFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_MedTestDefinitions_Key",
                table: "MedTestDefinitions",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_MedTests_MedTestDefinitionKey",
                table: "MedTests",
                column: "MedTestDefinitionKey");

            migrationBuilder.AddForeignKey(
                name: "FK_MedTests_MedTestDefinitions_MedTestDefinitionKey",
                table: "MedTests",
                column: "MedTestDefinitionKey",
                principalTable: "MedTestDefinitions",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedTests_MedTestDefinitions_MedTestDefinitionKey",
                table: "MedTests");

            migrationBuilder.DropIndex(
                name: "IX_MedTests_MedTestDefinitionKey",
                table: "MedTests");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_MedTestDefinitions_Key",
                table: "MedTestDefinitions");
        }
    }
}
