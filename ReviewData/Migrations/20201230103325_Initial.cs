using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReviewData.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reviews");

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "reviews",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerAuthId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                schema: "reviews",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => new { x.CustomerId, x.ProductId });
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                schema: "reviews",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    ReviewText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => new { x.CustomerId, x.ProductId });
                });

            migrationBuilder.InsertData(
                schema: "reviews",
                table: "Customers",
                columns: new[] { "CustomerId", "CustomerAuthId", "CustomerName" },
                values: new object[,]
                {
                    { 1, "f756701c-4336-47b1-8317-a16e84bd0059", "Chris Burrell" },
                    { 2, "07dc5dfc-9dad-408c-ba81-ff6a8dd3aec2", "Paul Mitchell" },
                    { 3, "1e3998f7-4ca6-42e0-9c78-8cb030f65f47", "Jack Ferguson" },
                    { 4, "bce3bb9c-5947-4265-8a7d-8588655bbabe", "Carter Ridgeway" },
                    { 5, "fb9e3941-6830-4387-be15-eeac14848c01", "Karl Hall" }
                });

            migrationBuilder.InsertData(
                schema: "reviews",
                table: "Purchases",
                columns: new[] { "CustomerId", "ProductId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 2, 3 },
                    { 2, 4 },
                    { 3, 5 }
                });

            migrationBuilder.InsertData(
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "CustomerId", "ProductId", "Rating", "ReviewText", "TimeStamp", "Visible" },
                values: new object[] { 1, 1, 3, "Good", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers",
                schema: "reviews");

            migrationBuilder.DropTable(
                name: "Purchases",
                schema: "reviews");

            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "reviews");
        }
    }
}
