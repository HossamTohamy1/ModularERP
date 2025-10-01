using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations.Finance
{
    /// <inheritdoc />
    public partial class AddCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.RenameIndex(
                name: "IX_Requisitions_WarehouseId",
                table: "Requisitions",
                newName: "IX_Requisition_Warehouse");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "StockTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "StocktakingHeaders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Requisitions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "PriceLists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_CompanyId",
                table: "StockTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_StocktakingHeaders_CompanyId",
                table: "StocktakingHeaders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisition_Company",
                table: "Requisitions",
                column: "CompanyId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_CompanyId",
                table: "PriceLists",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceLists_Companies_CompanyId",
                table: "PriceLists",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Requisitions_Companies_CompanyId",
                table: "Requisitions",
                column: "CompanyId",
                principalTable: "Companies",
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

            migrationBuilder.AddForeignKey(
                name: "FK_StocktakingHeaders_Companies_CompanyId",
                table: "StocktakingHeaders",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Companies_CompanyId",
                table: "StockTransactions",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceLists_Companies_CompanyId",
                table: "PriceLists");

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
                name: "FK_Requisitions_Companies_CompanyId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_StocktakingHeaders_Companies_CompanyId",
                table: "StocktakingHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Companies_CompanyId",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_CompanyId",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StocktakingHeaders_CompanyId",
                table: "StocktakingHeaders");

            migrationBuilder.DropIndex(
                name: "IX_Requisition_Company",
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

            migrationBuilder.DropIndex(
                name: "IX_PriceLists_CompanyId",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "StocktakingHeaders");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PriceLists");

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
    }
}
