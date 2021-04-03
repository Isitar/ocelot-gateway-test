using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Auth.Identity.Migrations
{
    public partial class DefaultAzConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AzureAdConfigs",
                columns: new[] { "Id", "ClientId", "Domain", "RedirectUri", "TenantId" },
                values: new object[] { new Guid("b08585db-e0ac-4320-b076-476b79d30e2d"), "b08585db-e0ac-4320-ffff-476b79d30e2d", "bbb.ch", "http://localhost:4200/login-azuread", "b08585db-e0ac-4320-ffff-476b79d30e2d" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AzureAdConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b08585db-e0ac-4320-b076-476b79d30e2d"));
        }
    }
}
