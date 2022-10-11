using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreeStructure.Migrations
{
    public partial class AddDisplayIdex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayIndex",
                table: "Nodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayIndex",
                table: "Nodes");
        }
    }
}
