using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Auth.Identity.Migrations
{
    public partial class AzureAdUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AzureAdConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b08585db-e0ac-4320-b076-476b79d30e2d"),
                columns: new[] { "ClientId", "RedirectUri" },
                values: new object[] { "e663215e-5687-4c3f-b5ae-c2dbefb5e51c", "http://localhost:8080/login" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AzureAdConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b08585db-e0ac-4320-b076-476b79d30e2d"),
                columns: new[] { "ClientId", "RedirectUri" },
                values: new object[] { "5239a17a-0112-4bba-becb-61e13ac258eb", "http://localhost:4200/login-azuread" });
        }
    }
}
