using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class AddPurcheseOrderToPurcheseRefund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseInvoiceId",
                table: "PurchaseRefunds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRefund_PurchaseInvoice",
                table: "PurchaseRefunds",
                column: "PurchaseInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRefunds_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseRefunds",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRefunds_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseRefunds");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRefund_PurchaseInvoice",
                table: "PurchaseRefunds");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceId",
                table: "PurchaseRefunds");
        }
    }
}
