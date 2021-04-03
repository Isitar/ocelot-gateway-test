using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Auth.Identity.Migrations
{
    public partial class PermissionTb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 3, "isitar.permission", "Permission.TaskBoard", new Guid("79690aed-86b2-4480-a726-9fe5f0d8b35b") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
