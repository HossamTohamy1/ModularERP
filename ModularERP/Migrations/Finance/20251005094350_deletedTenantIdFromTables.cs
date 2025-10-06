using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations.Finance
{
    /// <inheritdoc />
    public partial class deletedTenantIdFromTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vendors_TenantId_Code",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_UnitTemplate_Tenant",
                table: "UnitTemplates");

            migrationBuilder.DropIndex(
                name: "IX_UnitTemplate_Tenant_Name",
                table: "UnitTemplates");

            migrationBuilder.DropIndex(
                name: "IX_TaxProfile_Tenant",
                table: "TaxProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TaxProfile_Tenant_Name",
                table: "TaxProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TaxProfileComponent_Tenant",
                table: "TaxProfileComponents");

            migrationBuilder.DropIndex(
                name: "IX_TaxComponent_Tenant",
                table: "TaxComponents");

            migrationBuilder.DropIndex(
                name: "IX_TaxComponent_Tenant_Name",
                table: "TaxComponents");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_Tenant",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_Tenant_Name",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Stocktaking_Tenant_Number",
                table: "StocktakingHeaders");

            migrationBuilder.DropIndex(
                name: "IX_StockSnapshot_Tenant",
                table: "StockSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTaxProfile_Tenant",
                table: "ServiceTaxProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Service_Tenant_Code",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Requisition_Tenant_Number",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_ProductTaxProfile_Tenant",
                table: "ProductTaxProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Product_Tenant_Barcode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Product_Tenant_SKU",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PriceList_Tenant_Name",
                table: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_PriceList_Tenant_Type_Default",
                table: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_ItemGroup_Tenant",
                table: "ItemGroups");

            migrationBuilder.DropIndex(
                name: "IX_ItemGroup_Tenant_Name",
                table: "ItemGroups");

            migrationBuilder.DropIndex(
                name: "IX_CustomField_Tenant",
                table: "CustomFields");

            migrationBuilder.DropIndex(
                name: "IX_CustomField_Tenant_Name",
                table: "CustomFields");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId_Code",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Category_Tenant",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Category_Tenant_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Brand_Tenant",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Brand_Tenant_Name",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_BarcodeSettings_Tenant",
                table: "BarcodeSettings");

            migrationBuilder.DropIndex(
                name: "IX_BarcodeSettings_Tenant_Default",
                table: "BarcodeSettings");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "VoucherTaxes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UnitTemplates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UnitConversions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Treasuries");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TaxProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TaxProfileComponents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TaxComponents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StocktakingLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StocktakingHeaders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StocktakingAttachments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ServiceTaxProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RequisitionItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RequisitionApprovalLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RecurringSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductTaxProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductCustomFieldValues");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PriceListRules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PriceListAssignments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PriceCalculationLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ItemGroups");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ItemGroupItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GlAccounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CustomFields");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CategoryAttachments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BulkDiscounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BarcodeSettings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Code",
                table: "Vendors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Code",
                table: "Customers",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vendors_Code",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Code",
                table: "Customers");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "VoucherTaxes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Vouchers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Vendors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "UnitTemplates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "UnitConversions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Treasuries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "TaxProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "TaxProfileComponents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "TaxComponents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "StockTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "StocktakingLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "StocktakingHeaders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "StocktakingAttachments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "StockSnapshots",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ServiceTaxProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Requisitions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "RequisitionItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "RequisitionApprovalLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "RecurringSchedules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductTaxProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductStats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductCustomFieldValues",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PriceLists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PriceListRules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PriceListItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PriceListAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PriceCalculationLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "LedgerEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ItemGroups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ItemGroupItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "GlAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CustomFields",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Currencies",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Companies",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CategoryAttachments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BulkDiscounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Brands",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BarcodeSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BankAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Attachments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_TenantId_Code",
                table: "Vendors",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitTemplate_Tenant",
                table: "UnitTemplates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTemplate_Tenant_Name",
                table: "UnitTemplates",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfile_Tenant",
                table: "TaxProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfile_Tenant_Name",
                table: "TaxProfiles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfileComponent_Tenant",
                table: "TaxProfileComponents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxComponent_Tenant",
                table: "TaxComponents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxComponent_Tenant_Name",
                table: "TaxComponents",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_Tenant",
                table: "Suppliers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_Tenant_Name",
                table: "Suppliers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stocktaking_Tenant_Number",
                table: "StocktakingHeaders",
                columns: new[] { "TenantId", "Number" },
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
                name: "IX_Service_Tenant_Code",
                table: "Services",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Requisition_Tenant_Number",
                table: "Requisitions",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTaxProfile_Tenant",
                table: "ProductTaxProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Tenant_Barcode",
                table: "Products",
                columns: new[] { "TenantId", "Barcode" },
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Tenant_SKU",
                table: "Products",
                columns: new[] { "TenantId", "SKU" },
                unique: true,
                filter: "[SKU] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriceList_Tenant_Name",
                table: "PriceLists",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceList_Tenant_Type_Default",
                table: "PriceLists",
                columns: new[] { "TenantId", "Type", "IsDefault" },
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ItemGroup_Tenant",
                table: "ItemGroups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemGroup_Tenant_Name",
                table: "ItemGroups",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomField_Tenant",
                table: "CustomFields",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomField_Tenant_Name",
                table: "CustomFields",
                columns: new[] { "TenantId", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_Code",
                table: "Customers",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_Tenant",
                table: "Categories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_Tenant_Name",
                table: "Categories",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brand_Tenant",
                table: "Brands",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Brand_Tenant_Name",
                table: "Brands",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeSettings_Tenant",
                table: "BarcodeSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeSettings_Tenant_Default",
                table: "BarcodeSettings",
                columns: new[] { "TenantId", "IsDefault" },
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId");
        }
    }
}
