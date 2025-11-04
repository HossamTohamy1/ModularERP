using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdForGoodReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "GoodsReceiptNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNote_Company",
                table: "GoodsReceiptNotes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_CreatedById",
                table: "GoodsReceiptNotes",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceiptNotes_AspNetUsers_CreatedById",
                table: "GoodsReceiptNotes",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceiptNotes_Companies_CompanyId",
                table: "GoodsReceiptNotes",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceiptNotes_AspNetUsers_CreatedById",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceiptNotes_Companies_CompanyId",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceiptNote_Company",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceiptNotes_CreatedById",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "GoodsReceiptNotes");
        }
    }
}
