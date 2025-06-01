using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskVault.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInFileSystemLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "Index",
                table: "Files");

            migrationBuilder.CreateTable(
                name: "DirectoryEntries",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DirectoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryEntries", x => new { x.UserId, x.DirectoryId, x.FileId });
                    table.ForeignKey(
                        name: "FK_DirectoryEntries_Files_DirectoryId",
                        column: x => x.DirectoryId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DirectoryEntries_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryEntries_DirectoryId",
                table: "DirectoryEntries",
                column: "DirectoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryEntries_FileId",
                table: "DirectoryEntries",
                column: "FileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectoryEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "DirectoryId",
                table: "Files",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Files",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

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
    }
}
