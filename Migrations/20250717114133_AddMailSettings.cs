using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMailSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MailSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SenderEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SmtpServer = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    SecurityType = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MailSettings");
        }
    }
}
