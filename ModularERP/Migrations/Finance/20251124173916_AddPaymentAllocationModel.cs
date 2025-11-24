using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAllocationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Purchases");

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                schema: "Purchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AllocatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsVoided = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VoidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VoidedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VoidReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_AspNetUsers_VoidedBy",
                        column: x => x.VoidedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_PurchaseInvoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "PurchaseInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_SupplierPayments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "SupplierPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_AllocationDate",
                schema: "Purchases",
                table: "PaymentAllocations",
                column: "AllocationDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_InvoiceId",
                schema: "Purchases",
                table: "PaymentAllocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_IsVoided",
                schema: "Purchases",
                table: "PaymentAllocations",
                column: "IsVoided");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_Payment_Invoice",
                schema: "Purchases",
                table: "PaymentAllocations",
                columns: new[] { "PaymentId", "InvoiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_PaymentId",
                schema: "Purchases",
                table: "PaymentAllocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_VoidedBy",
                schema: "Purchases",
                table: "PaymentAllocations",
                column: "VoidedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentAllocations",
                schema: "Purchases");
        }
    }
}
