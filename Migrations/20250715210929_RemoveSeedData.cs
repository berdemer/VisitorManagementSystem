using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisitorManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "LastLogin", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sistem Yöneticisi", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "$2a$11$rQzQPVqrst4wxyz123456uzQPVqrst4wxyz123456uzQPVqrst4wx", "Admin", "admin" });
        }
    }
}
