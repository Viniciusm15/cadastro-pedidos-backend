using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class AddUniqueConstraintsToNameTelefoneAndEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_IsActive",
                table: "Products",
                columns: new[] { "Name", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email_IsActive",
                table: "Clients",
                columns: new[] { "Email", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Telephone_IsActive",
                table: "Clients",
                columns: new[] { "Telephone", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name_IsActive",
                table: "Categories",
                columns: new[] { "Name", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Name_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Email_IsActive",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Telephone_IsActive",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name_IsActive",
                table: "Categories");
        }
    }
}
