using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propnex.Poster.Migrations
{
    public partial class add_pntask_clientId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "PropnexPosterPnTasks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "PropnexPosterPnTasks");
        }
    }
}
