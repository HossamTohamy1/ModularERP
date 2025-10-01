using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations.Finance
{
    /// <inheritdoc />
    public partial class AddCompanyIdToProductAndUpdateProductStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_ApprovedBy",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_ConfirmedBy",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_ReversedBy",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_SubmittedBy",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Requisitions_ParentRequisitionId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Suppliers_SupplierId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Warehouses_WarehouseId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisition_Date",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisition_Status",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisition_Tenant_Number",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisition_Type",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_ApprovedBy",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_ConfirmedBy",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_ReversedBy",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_SubmittedBy",
                table: "Requisitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductStats",
                table: "ProductStats");

            migrationBuilder.RenameIndex(
                name: "IX_Requisition_Warehouse",
                table: "Requisitions",
                newName: "IX_Requisitions_WarehouseId");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Requisitions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Requisitions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldDefaultValue: "Draft");

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "Requisitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedByUserId",
                table: "Requisitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversedByUserId",
                table: "Requisitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                table: "Requisitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ProductStats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "ProductStats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductStats",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "ProductStats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProductStats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductStats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductStats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProductStats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "ProductStats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductStats",
                table: "ProductStats",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_ApprovedByUserId",
                table: "Requisitions",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_ConfirmedByUserId",
                table: "Requisitions",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_ReversedByUserId",
                table: "Requisitions",
                column: "ReversedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_SubmittedByUserId",
                table: "Requisitions",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStats_Company",
                table: "ProductStats",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStats_Product",
                table: "ProductStats",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_Company",
                table: "Products",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Companies_CompanyId",
                table: "Products",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStats_Companies_CompanyId",
                table: "ProductStats",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_ApprovedByUserId",
                table: "Requisitions",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_ConfirmedByUserId",
                table: "Requisitions",
                column: "ConfirmedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_ReversedByUserId",
                table: "Requisitions",
                column: "ReversedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_SubmittedByUserId",
                table: "Requisitions",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Requisitions_ParentRequisitionId",
                table: "Requisitions",
                column: "ParentRequisitionId",
                principalTable: "Requisitions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Suppliers_SupplierId",
                table: "Requisitions",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Warehouses_WarehouseId",
                table: "Requisitions",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Companies_CompanyId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductStats_Companies_CompanyId",
                table: "ProductStats");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_ApprovedByUserId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_ConfirmedByUserId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_ReversedByUserId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Requisitions_ParentRequisitionId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Suppliers_SupplierId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Warehouses_WarehouseId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_ApprovedByUserId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_ConfirmedByUserId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_ReversedByUserId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductStats",
                table: "ProductStats");

            migrationBuilder.DropIndex(
                name: "IX_ProductStats_Company",
                table: "ProductStats");

            migrationBuilder.DropIndex(
                name: "IX_ProductStats_Product",
                table: "ProductStats");

            migrationBuilder.DropIndex(
                name: "IX_Product_Company",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "ConfirmedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "ReversedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "ProductStats");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_Requisitions_WarehouseId",
                table: "Requisitions",
                newName: "IX_Requisition_Warehouse");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Requisitions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Requisitions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductStats",
                table: "ProductStats",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisition_Date",
                table: "Requisitions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Requisition_Status",
                table: "Requisitions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Requisition_Tenant_Number",
                table: "Requisitions",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requisition_Type",
                table: "Requisitions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_ApprovedBy",
                table: "Requisitions",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_ConfirmedBy",
                table: "Requisitions",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_ReversedBy",
                table: "Requisitions",
                column: "ReversedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_SubmittedBy",
                table: "Requisitions",
                column: "SubmittedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_ApprovedBy",
                table: "Requisitions",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_ConfirmedBy",
                table: "Requisitions",
                column: "ConfirmedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_ReversedBy",
                table: "Requisitions",
                column: "ReversedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_SubmittedBy",
                table: "Requisitions",
                column: "SubmittedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Requisitions_ParentRequisitionId",
                table: "Requisitions",
                column: "ParentRequisitionId",
                principalTable: "Requisitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Suppliers_SupplierId",
                table: "Requisitions",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Warehouses_WarehouseId",
                table: "Requisitions",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
