using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations.Finance
{
    /// <inheritdoc />
    public partial class addTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaxProfile_Active",
                table: "TaxProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TaxComponent_Active",
                table: "TaxComponents");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "TaxProfiles");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "TaxComponents");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TaxProfileComponents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "TaxProfileComponents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StockSnapshots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "StockSnapshots",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ServiceTaxProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ServiceTaxProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductTaxProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductTaxProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfileComponent_Tenant",
                table: "TaxProfileComponents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_Company_Name",
                table: "Suppliers",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockSnapshot_Tenant",
                table: "StockSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTaxProfile_Tenant",
                table: "ServiceTaxProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_Company_Code",
                table: "Services",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTaxProfile_Tenant",
                table: "ProductTaxProfiles",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Companies_CompanyId",
                table: "Services",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Companies_CompanyId",
                table: "Suppliers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Companies_CompanyId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Companies_CompanyId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_TaxProfileComponent_Tenant",
                table: "TaxProfileComponents");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_Company_Name",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_StockSnapshot_Tenant",
                table: "StockSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTaxProfile_Tenant",
                table: "ServiceTaxProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Service_Company_Code",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ProductTaxProfile_Tenant",
                table: "ProductTaxProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TaxProfileComponents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TaxProfileComponents");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ServiceTaxProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ServiceTaxProfiles");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductTaxProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductTaxProfiles");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "TaxProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "TaxComponents",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfile_Active",
                table: "TaxProfiles",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_TaxComponent_Active",
                table: "TaxComponents",
                column: "Active");
        }
    }
}
