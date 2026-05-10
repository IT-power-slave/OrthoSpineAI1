using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrthoSpineAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.ClinicId);
                });

            migrationBuilder.CreateTable(
                name: "MedTestDefinitions",
                columns: table => new
                {
                    MedTestDefinitionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedTestDefinitions", x => x.MedTestDefinitionId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PESEL = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    Sex = table.Column<int>(type: "INTEGER", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AddressSt = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AddressCity = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                    table.ForeignKey(
                        name: "FK_Patients_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "ClinicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemUsers",
                columns: table => new
                {
                    SystemUserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Login = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ClinicId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUsers", x => x.SystemUserId);
                    table.ForeignKey(
                        name: "FK_SystemUsers_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "ClinicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedTestStages",
                columns: table => new
                {
                    MedTestStageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tip = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TipControl = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MainSurveyControl = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BodyPlaneName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Plane = table.Column<int>(type: "INTEGER", nullable: false),
                    OrtMeas = table.Column<int>(type: "INTEGER", nullable: false),
                    OrtState = table.Column<int>(type: "INTEGER", nullable: false),
                    OrtNextStepButton = table.Column<int>(type: "INTEGER", nullable: false),
                    OrtMode = table.Column<int>(type: "INTEGER", nullable: false),
                    OrtResetFlag = table.Column<int>(type: "INTEGER", nullable: false),
                    OrtContinousMeas = table.Column<bool>(type: "INTEGER", nullable: false),
                    ValueISOM1 = table.Column<double>(type: "REAL", nullable: true),
                    ValueISOM3 = table.Column<double>(type: "REAL", nullable: true),
                    MedTestDefinitionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedTestStages", x => x.MedTestStageId);
                    table.ForeignKey(
                        name: "FK_MedTestStages_MedTestDefinitions_MedTestDefinitionId",
                        column: x => x.MedTestDefinitionId,
                        principalTable: "MedTestDefinitions",
                        principalColumn: "MedTestDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedTests",
                columns: table => new
                {
                    MedTestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExaminationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    MedTestDefinitionKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    Growth = table.Column<double>(type: "REAL", nullable: false),
                    Beighton = table.Column<int>(type: "INTEGER", nullable: false),
                    TestPP = table.Column<bool>(type: "INTEGER", nullable: false),
                    KneeValgus = table.Column<bool>(type: "INTEGER", nullable: false),
                    TarsalValgus = table.Column<bool>(type: "INTEGER", nullable: false),
                    GaitDisturbance = table.Column<bool>(type: "INTEGER", nullable: false),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    SystemUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedTests", x => x.MedTestId);
                    table.ForeignKey(
                        name: "FK_MedTests_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedTests_SystemUsers_SystemUserId",
                        column: x => x.SystemUserId,
                        principalTable: "SystemUsers",
                        principalColumn: "SystemUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AwwsResults",
                columns: table => new
                {
                    AwwsResultId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MedTestId = table.Column<int>(type: "INTEGER", nullable: false),
                    PilsVariant = table.Column<int>(type: "INTEGER", nullable: false),
                    PilsControlKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Conclusion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ControlRecommendation = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    GroupResultsJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AwwsResults", x => x.AwwsResultId);
                    table.ForeignKey(
                        name: "FK_AwwsResults_MedTests_MedTestId",
                        column: x => x.MedTestId,
                        principalTable: "MedTests",
                        principalColumn: "MedTestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedTestContinuousResults",
                columns: table => new
                {
                    MedTestContinuousResultId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Signal = table.Column<int>(type: "INTEGER", nullable: false),
                    Battery = table.Column<double>(type: "REAL", nullable: false),
                    Shake = table.Column<double>(type: "REAL", nullable: false),
                    Roll = table.Column<double>(type: "REAL", nullable: false),
                    RollOffset = table.Column<double>(type: "REAL", nullable: false),
                    Tilt = table.Column<double>(type: "REAL", nullable: false),
                    Way = table.Column<int>(type: "INTEGER", nullable: false),
                    Space = table.Column<int>(type: "INTEGER", nullable: false),
                    Force1 = table.Column<double>(type: "REAL", nullable: false),
                    Force2 = table.Column<double>(type: "REAL", nullable: false),
                    OrtMeas = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MedTestId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedTestContinuousResults", x => x.MedTestContinuousResultId);
                    table.ForeignKey(
                        name: "FK_MedTestContinuousResults_MedTests_MedTestId",
                        column: x => x.MedTestId,
                        principalTable: "MedTests",
                        principalColumn: "MedTestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedTestResults",
                columns: table => new
                {
                    MedTestResultId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Plane = table.Column<int>(type: "INTEGER", nullable: false),
                    OrtMeas = table.Column<int>(type: "INTEGER", nullable: false),
                    PhysicalValue = table.Column<double>(type: "REAL", nullable: false),
                    PhysicalUnit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Side = table.Column<int>(type: "INTEGER", nullable: false),
                    MedTestId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedTestResults", x => x.MedTestResultId);
                    table.ForeignKey(
                        name: "FK_MedTestResults_MedTests_MedTestId",
                        column: x => x.MedTestId,
                        principalTable: "MedTests",
                        principalColumn: "MedTestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AwwsResults_MedTestId",
                table: "AwwsResults",
                column: "MedTestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedTestContinuousResults_MedTestId",
                table: "MedTestContinuousResults",
                column: "MedTestId");

            migrationBuilder.CreateIndex(
                name: "IX_MedTestDefinitions_Key",
                table: "MedTestDefinitions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedTestResults_MedTestId",
                table: "MedTestResults",
                column: "MedTestId");

            migrationBuilder.CreateIndex(
                name: "IX_MedTests_PatientId",
                table: "MedTests",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedTests_SystemUserId",
                table: "MedTests",
                column: "SystemUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedTestStages_MedTestDefinitionId",
                table: "MedTestStages",
                column: "MedTestDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ClinicId",
                table: "Patients",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PESEL",
                table: "Patients",
                column: "PESEL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsers_ClinicId",
                table: "SystemUsers",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsers_Login",
                table: "SystemUsers",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AwwsResults");

            migrationBuilder.DropTable(
                name: "MedTestContinuousResults");

            migrationBuilder.DropTable(
                name: "MedTestResults");

            migrationBuilder.DropTable(
                name: "MedTestStages");

            migrationBuilder.DropTable(
                name: "MedTests");

            migrationBuilder.DropTable(
                name: "MedTestDefinitions");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "SystemUsers");

            migrationBuilder.DropTable(
                name: "Clinics");
        }
    }
}
