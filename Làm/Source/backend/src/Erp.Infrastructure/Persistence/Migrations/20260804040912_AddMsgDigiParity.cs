using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMsgDigiParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentStorageKey",
                schema: "erp_sys",
                table: "chat_message",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                schema: "erp_sys",
                table: "chat_message",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                schema: "erp_sys",
                table: "chat_message",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentMessageId",
                schema: "erp_sys",
                table: "chat_message",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentStorageKey",
                schema: "erp_sys",
                table: "chat_message");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                schema: "erp_sys",
                table: "chat_message");

            migrationBuilder.DropColumn(
                name: "IsEdited",
                schema: "erp_sys",
                table: "chat_message");

            migrationBuilder.DropColumn(
                name: "ParentMessageId",
                schema: "erp_sys",
                table: "chat_message");
        }
    }
}
