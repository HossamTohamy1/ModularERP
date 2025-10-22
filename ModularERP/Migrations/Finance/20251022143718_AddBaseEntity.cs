using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockSnapshots_StocktakingHeaders_StocktakingId",
                table: "StockSnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockSnapshots",
                table: "StockSnapshots");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "StockSnapshots",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "StockSnapshots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StockSnapshots",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StocktakingHeaderId",
                table: "StockSnapshots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StockSnapshots",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "StockSnapshots",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockSnapshots",
                table: "StockSnapshots",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StockSnapshot_IsDeleted",
                table: "StockSnapshots",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_StockSnapshot_Stocktaking",
                table: "StockSnapshots",
                column: "StocktakingId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSnapshots_StocktakingHeaderId",
                table: "StockSnapshots",
                column: "StocktakingHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockSnapshots_StocktakingHeaders_StocktakingHeaderId",
                table: "StockSnapshots",
                column: "StocktakingHeaderId",
                principalTable: "StocktakingHeaders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockSnapshots_StocktakingHeaders_StocktakingId",
                table: "StockSnapshots",
                column: "StocktakingId",
                principalTable: "StocktakingHeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockSnapshots_StocktakingHeaders_StocktakingHeaderId",
                table: "StockSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_StockSnapshots_StocktakingHeaders_StocktakingId",
                table: "StockSnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockSnapshots",
                table: "StockSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_StockSnapshot_IsDeleted",
                table: "StockSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_StockSnapshot_Stocktaking",
                table: "StockSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_StockSnapshots_StocktakingHeaderId",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "StocktakingHeaderId",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StockSnapshots");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "StockSnapshots");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockSnapshots",
                table: "StockSnapshots",
                column: "SnapshotId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockSnapshots_StocktakingHeaders_StocktakingId",
                table: "StockSnapshots",
                column: "StocktakingId",
                principalTable: "StocktakingHeaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
