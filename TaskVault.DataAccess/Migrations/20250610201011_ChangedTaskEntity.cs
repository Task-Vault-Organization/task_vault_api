using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskVault.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTaskEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TaskId1",
                table: "TaskSubmissions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskSubmissions_TaskId1",
                table: "TaskSubmissions",
                column: "TaskId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskSubmissions_Tasks_TaskId1",
                table: "TaskSubmissions",
                column: "TaskId1",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskSubmissions_Tasks_TaskId1",
                table: "TaskSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_TaskSubmissions_TaskId1",
                table: "TaskSubmissions");

            migrationBuilder.DropColumn(
                name: "TaskId1",
                table: "TaskSubmissions");
        }
    }
}
