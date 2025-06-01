using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskVault.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUserEntityToIncludeRootDirectoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RootDirectoryId",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RootDirectoryId",
                table: "Users");
        }
    }
}
