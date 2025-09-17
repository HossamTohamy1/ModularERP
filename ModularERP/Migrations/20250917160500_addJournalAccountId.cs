using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class addJournalAccountId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JournalAccountId",
                table: "Treasuries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JournalAccountId",
                table: "BankAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Treasuries_JournalAccountId",
                table: "Treasuries",
                column: "JournalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_JournalAccountId",
                table: "BankAccounts",
                column: "JournalAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_GlAccounts_JournalAccountId",
                table: "BankAccounts",
                column: "JournalAccountId",
                principalTable: "GlAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Treasuries_GlAccounts_JournalAccountId",
                table: "Treasuries",
                column: "JournalAccountId",
                principalTable: "GlAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_GlAccounts_JournalAccountId",
                table: "BankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Treasuries_GlAccounts_JournalAccountId",
                table: "Treasuries");

            migrationBuilder.DropIndex(
                name: "IX_Treasuries_JournalAccountId",
                table: "Treasuries");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_JournalAccountId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "JournalAccountId",
                table: "Treasuries");

            migrationBuilder.DropColumn(
                name: "JournalAccountId",
                table: "BankAccounts");
        }
    }
}
