using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations.Finance
{
    /// <inheritdoc />
    public partial class AddWarehouseIdForProductAndServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Services");

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Service_Company",
                table: "Services",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_Warehouse",
                table: "Services",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Warehouse",
                table: "Products",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Warehouses_WarehouseId",
                table: "Products",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Warehouses_WarehouseId",
                table: "Services",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Warehouses_WarehouseId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Warehouses_WarehouseId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Service_Company",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Service_Warehouse",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Product_Warehouse",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Photo",
                table: "Services",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
