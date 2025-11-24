using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierPaymentEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBalance",
                table: "Suppliers",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalance",
                table: "Suppliers",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpeningBalanceDate",
                table: "Suppliers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaid",
                table: "Suppliers",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPurchases",
                table: "Suppliers",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AllocatedAmount",
                table: "SupplierPayments",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoid",
                table: "SupplierPayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNumber",
                table: "SupplierPayments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "SupplierPayments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "AgainstInvoice");

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseOrderId",
                table: "SupplierPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SupplierPayments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<decimal>(
                name: "UnallocatedAmount",
                table: "SupplierPayments",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "SupplierPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAt",
                table: "SupplierPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoidedBy",
                table: "SupplierPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaid",
                table: "PurchaseOrders",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_CurrentBalance",
                table: "Suppliers",
                column: "CurrentBalance");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayment_Number",
                table: "SupplierPayments",
                column: "PaymentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayment_PurchaseOrder",
                table: "SupplierPayments",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayment_Status",
                table: "SupplierPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayment_Type",
                table: "SupplierPayments",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_VoidedBy",
                table: "SupplierPayments",
                column: "VoidedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierPayments_AspNetUsers_VoidedBy",
                table: "SupplierPayments",
                column: "VoidedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierPayments_PurchaseOrders_PurchaseOrderId",
                table: "SupplierPayments",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierPayments_AspNetUsers_VoidedBy",
                table: "SupplierPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierPayments_PurchaseOrders_PurchaseOrderId",
                table: "SupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_CurrentBalance",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPayment_Number",
                table: "SupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPayment_PurchaseOrder",
                table: "SupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPayment_Status",
                table: "SupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPayment_Type",
                table: "SupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPayments_VoidedBy",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "CurrentBalance",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "OpeningBalance",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "OpeningBalanceDate",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TotalPaid",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TotalPurchases",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "AllocatedAmount",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "IsVoid",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "PaymentNumber",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "UnallocatedAmount",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "VoidedAt",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "VoidedBy",
                table: "SupplierPayments");

            migrationBuilder.DropColumn(
                name: "TotalPaid",
                table: "PurchaseOrders");
        }
    }
}
