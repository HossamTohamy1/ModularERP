using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_CreatedById",
                table: "Requisitions",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_CreatedById",
                table: "Requisitions",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_CreatedById",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_CreatedById",
                table: "Requisitions");

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                table: "Requisitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_SubmittedByUserId",
                table: "Requisitions",
                column: "SubmittedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_AspNetUsers_SubmittedByUserId",
                table: "Requisitions",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
