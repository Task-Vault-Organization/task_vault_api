using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskVault.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewCommentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskSubmissionTaskItemFileComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskSubmissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskSubmissionTaskItemFileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommentHtml = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSubmissionTaskItemFileComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSubmissionTaskItemFileComments_TaskSubmissionTaskItemFiles_TaskSubmissionId_TaskSubmissionTaskItemFileId_FromUserId",
                        columns: x => new { x.TaskSubmissionId, x.TaskSubmissionTaskItemFileId, x.FromUserId },
                        principalTable: "TaskSubmissionTaskItemFiles",
                        principalColumns: new[] { "TaskSubmissionId", "TaskItemId", "FileId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSubmissionTaskItemFileComments_TaskSubmissions_TaskSubmissionId",
                        column: x => x.TaskSubmissionId,
                        principalTable: "TaskSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSubmissionTaskItemFileComments_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskSubmissionTaskItemFileComments_FromUserId",
                table: "TaskSubmissionTaskItemFileComments",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSubmissionTaskItemFileComments_TaskSubmissionId_TaskSubmissionTaskItemFileId_FromUserId",
                table: "TaskSubmissionTaskItemFileComments",
                columns: new[] { "TaskSubmissionId", "TaskSubmissionTaskItemFileId", "FromUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskSubmissionTaskItemFileComments");
        }
    }
}
