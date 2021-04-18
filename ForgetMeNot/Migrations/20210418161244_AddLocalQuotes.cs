using Microsoft.EntityFrameworkCore.Migrations;

namespace ForgetMeNot.Migrations
{
    public partial class AddLocalQuotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LocalQuotes",
                table: "GuildSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalQuotes",
                table: "GuildSettings");
        }
    }
}
