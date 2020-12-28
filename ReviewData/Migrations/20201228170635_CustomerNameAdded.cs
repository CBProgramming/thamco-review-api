using Microsoft.EntityFrameworkCore.Migrations;

namespace ReviewData.Migrations
{
    public partial class CustomerNameAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "reviews",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "reviews",
                table: "Reviews");
        }
    }
}
