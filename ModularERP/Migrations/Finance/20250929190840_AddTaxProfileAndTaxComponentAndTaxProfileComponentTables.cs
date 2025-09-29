using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations.Finance
{
    /// <inheritdoc />
    public partial class AddTaxProfileAndTaxComponentAndTaxProfileComponentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RateValue = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    IncludedType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppliesOn = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "Both"),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxProfileComponents",
                columns: table => new
                {
                    TaxProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxProfileComponents", x => new { x.TaxProfileId, x.TaxComponentId });
                    table.ForeignKey(
                        name: "FK_TaxProfileComponents_TaxComponents_TaxComponentId",
                        column: x => x.TaxComponentId,
                        principalTable: "TaxComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxProfileComponents_TaxProfiles_TaxProfileId",
                        column: x => x.TaxProfileId,
                        principalTable: "TaxProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxComponent_Active",
                table: "TaxComponents",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_TaxComponent_AppliesOn",
                table: "TaxComponents",
                column: "AppliesOn");

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
                name: "IX_TaxProfileComponent_Priority",
                table: "TaxProfileComponents",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfileComponents_TaxComponentId",
                table: "TaxProfileComponents",
                column: "TaxComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfile_Active",
                table: "TaxProfiles",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfile_Tenant",
                table: "TaxProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxProfile_Tenant_Name",
                table: "TaxProfiles",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxProfileComponents");

            migrationBuilder.DropTable(
                name: "TaxComponents");

            migrationBuilder.DropTable(
                name: "TaxProfiles");
        }
    }
}
