using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularERP.Migrations
{
    /// <inheritdoc />
    public partial class addCreateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Requisitions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Requisitions",
                type: "uniqueidentifier",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_AspNetUsers_SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.DropIndex(
                name: "IX_Requisitions_SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Requisitions");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Requisitions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Requisitions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
