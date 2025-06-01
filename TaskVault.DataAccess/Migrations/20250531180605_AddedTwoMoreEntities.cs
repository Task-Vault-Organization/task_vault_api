using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskVault.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedTwoMoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileShareRequestStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileShareRequestStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileShareRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileShareRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileShareRequests_FileShareRequestStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "FileShareRequestStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileShareRequests_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileShareRequests_Users_FromId",
                        column: x => x.FromId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileShareRequests_Users_ToId",
                        column: x => x.ToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileShareRequests_FileId",
                table: "FileShareRequests",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileShareRequests_FromId",
                table: "FileShareRequests",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_FileShareRequests_StatusId",
                table: "FileShareRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FileShareRequests_ToId",
                table: "FileShareRequests",
                column: "ToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileShareRequests");

            migrationBuilder.DropTable(
                name: "FileShareRequestStatuses");
        }
    }
}
