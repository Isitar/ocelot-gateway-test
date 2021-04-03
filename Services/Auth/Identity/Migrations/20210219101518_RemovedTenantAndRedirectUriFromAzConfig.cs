using Microsoft.EntityFrameworkCore.Migrations;

namespace Auth.Identity.Migrations
{
    public partial class RemovedTenantAndRedirectUriFromAzConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectUri",
                table: "AzureAdConfigs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AzureAdConfigs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RedirectUri",
                table: "AzureAdConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AzureAdConfigs",
                type: "text",
                nullable: true);
        }
    }
}
