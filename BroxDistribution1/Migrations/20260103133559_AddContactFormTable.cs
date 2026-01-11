using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BroxDistribution1.Migrations
{
    /// <inheritdoc />
    public partial class AddContactFormTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WineBrand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WineCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WineYear = table.Column<int>(type: "int", nullable: true),
                    WineCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactForms", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactForms");
        }
    }
}
