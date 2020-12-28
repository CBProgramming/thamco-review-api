using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReviewData.Migrations
{
    public partial class SeedDataAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "CustomerId", "ProductId", "CustomerName", "Rating", "ReviewText", "TimeStamp", "Visible" },
                values: new object[] { 1, 1, "CustomerName", 3, "Good", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "reviews",
                table: "Purchases",
                keyColumns: new[] { "CustomerId", "ProductId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                schema: "reviews",
                table: "Purchases",
                keyColumns: new[] { "CustomerId", "ProductId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                schema: "reviews",
                table: "Purchases",
                keyColumns: new[] { "CustomerId", "ProductId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                schema: "reviews",
                table: "Purchases",
                keyColumns: new[] { "CustomerId", "ProductId" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                schema: "reviews",
                table: "Purchases",
                keyColumns: new[] { "CustomerId", "ProductId" },
                keyValues: new object[] { 2, 4 });

            migrationBuilder.DeleteData(
                schema: "reviews",
                table: "Purchases",
                keyColumns: new[] { "CustomerId", "ProductId" },
                keyValues: new object[] { 3, 5 });

            migrationBuilder.DeleteData(
                schema: "reviews",
                table: "Reviews",
                keyColumns: new[] { "CustomerId", "ProductId" },
                keyValues: new object[] { 1, 1 });
        }
    }
}
