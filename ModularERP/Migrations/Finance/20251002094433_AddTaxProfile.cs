using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations.Finance
{
    /// <inheritdoc />
    public partial class AddTaxProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTaxes_Taxes_TaxId",
                table: "VoucherTaxes");

            migrationBuilder.DropTable(
                name: "Taxes");

            migrationBuilder.RenameColumn(
                name: "TaxId",
                table: "VoucherTaxes",
                newName: "TaxProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_VoucherTaxes_TaxId",
                table: "VoucherTaxes",
                newName: "IX_VoucherTaxes_TaxProfileId");

            migrationBuilder.AddColumn<decimal>(
                name: "AppliedRate",
                table: "VoucherTaxes",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxComponentId",
                table: "VoucherTaxes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VoucherTaxes_TaxComponentId",
                table: "VoucherTaxes",
                column: "TaxComponentId");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTaxes_TaxComponents_TaxComponentId",
                table: "VoucherTaxes",
                column: "TaxComponentId",
                principalTable: "TaxComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTaxes_TaxProfiles_TaxProfileId",
                table: "VoucherTaxes",
                column: "TaxProfileId",
                principalTable: "TaxProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTaxes_TaxComponents_TaxComponentId",
                table: "VoucherTaxes");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherTaxes_TaxProfiles_TaxProfileId",
                table: "VoucherTaxes");

            migrationBuilder.DropIndex(
                name: "IX_VoucherTaxes_TaxComponentId",
                table: "VoucherTaxes");

            migrationBuilder.DropColumn(
                name: "AppliedRate",
                table: "VoucherTaxes");

            migrationBuilder.DropColumn(
                name: "TaxComponentId",
                table: "VoucherTaxes");

            migrationBuilder.RenameColumn(
                name: "TaxProfileId",
                table: "VoucherTaxes",
                newName: "TaxId");

            migrationBuilder.RenameIndex(
                name: "IX_VoucherTaxes_TaxProfileId",
                table: "VoucherTaxes",
                newName: "IX_VoucherTaxes_TaxId");

            migrationBuilder.CreateTable(
                name: "Taxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_TenantId_Code",
                table: "Taxes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherTaxes_Taxes_TaxId",
                table: "VoucherTaxes",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
