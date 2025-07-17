using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ResidentManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Residents_PhoneNumber",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Residents");

            migrationBuilder.AddColumn<string>(
                name: "Block",
                table: "Residents",
                type: "TEXT",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoorNumber",
                table: "Residents",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Residents",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubBlock",
                table: "Residents",
                type: "TEXT",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Residents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResidentContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ResidentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ContactValue = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidentContacts_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResidentVehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ResidentId = table.Column<int>(type: "INTEGER", nullable: false),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Year = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    VehicleType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidentVehicles_Residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "Residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Residents_Block_SubBlock_DoorNumber",
                table: "Residents",
                columns: new[] { "Block", "SubBlock", "DoorNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ResidentContacts_ContactValue",
                table: "ResidentContacts",
                column: "ContactValue");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentContacts_ResidentId_ContactType_Priority",
                table: "ResidentContacts",
                columns: new[] { "ResidentId", "ContactType", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_ResidentVehicles_LicensePlate",
                table: "ResidentVehicles",
                column: "LicensePlate");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentVehicles_ResidentId",
                table: "ResidentVehicles",
                column: "ResidentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResidentContacts");

            migrationBuilder.DropTable(
                name: "ResidentVehicles");

            migrationBuilder.DropIndex(
                name: "IX_Residents_Block_SubBlock_DoorNumber",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "Block",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "DoorNumber",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "SubBlock",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Residents");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Residents",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Residents",
                type: "TEXT",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Residents_PhoneNumber",
                table: "Residents",
                column: "PhoneNumber");
        }
    }
}
