using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitorPhoneAndResidentName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Residents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ApartmentNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Residents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Visitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ApartmentNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IdNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PhotoPath = table.Column<string>(type: "TEXT", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResidentPhone = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    ResidentName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    VisitorPhone = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResidentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitors_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VisitorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VisitorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PerformedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitorLogs_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "LastLogin", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sistem Yöneticisi", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "$2a$11$rQzQPVqrst4wxyz123456uzQPVqrst4wxyz123456uzQPVqrst4wx", "Admin", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Residents_ApartmentNumber",
                table: "Residents",
                column: "ApartmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Residents_PhoneNumber",
                table: "Residents",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitorLogs_Timestamp",
                table: "VisitorLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_VisitorLogs_VisitorId",
                table: "VisitorLogs",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_ApartmentNumber",
                table: "Visitors",
                column: "ApartmentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_CheckInTime",
                table: "Visitors",
                column: "CheckInTime");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_ResidentId",
                table: "Visitors",
                column: "ResidentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VisitorLogs");

            migrationBuilder.DropTable(
                name: "Visitors");

            migrationBuilder.DropTable(
                name: "Residents");
        }
    }
}
