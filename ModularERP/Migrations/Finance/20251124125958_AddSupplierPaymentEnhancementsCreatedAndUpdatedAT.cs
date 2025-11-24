using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierPaymentEnhancementsCreatedAndUpdatedAT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SupplierPayments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SupplierPayments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SupplierPayments");
        }
    }
}
