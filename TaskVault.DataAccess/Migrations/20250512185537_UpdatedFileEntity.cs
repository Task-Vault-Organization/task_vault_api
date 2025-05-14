using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskVault.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DirectoryId",
                table: "Files",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDirectory",
                table: "Files",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Files_DirectoryId",
                table: "Files",
                column: "DirectoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Files_DirectoryId",
                table: "Files",
                column: "DirectoryId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Files_DirectoryId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_DirectoryId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "DirectoryId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "IsDirectory",
                table: "Files");
        }
    }
}
