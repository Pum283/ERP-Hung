using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFsmCap2TicketCloseReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcceptanceNote",
                schema: "fsm",
                table: "ticket",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AcceptanceSignedAt",
                schema: "fsm",
                table: "ticket",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptanceSignerName",
                schema: "fsm",
                table: "ticket",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AppointmentAt",
                schema: "fsm",
                table: "ticket",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppointmentNote",
                schema: "fsm",
                table: "ticket",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CheckedOutAt",
                schema: "fsm",
                table: "ticket",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClosedAt",
                schema: "fsm",
                table: "ticket",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNote",
                schema: "fsm",
                table: "ticket",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResolvedAt",
                schema: "fsm",
                table: "ticket",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RootCause",
                schema: "fsm",
                table: "ticket",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SlaResolveMet",
                schema: "fsm",
                table: "ticket",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SlaResponseMet",
                schema: "fsm",
                table: "ticket",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticket_TenantId_Status_DueResolveAt",
                schema: "fsm",
                table: "ticket",
                columns: new[] { "TenantId", "Status", "DueResolveAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ticket_TenantId_Status_DueResolveAt",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "AcceptanceNote",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "AcceptanceSignedAt",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "AcceptanceSignerName",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "AppointmentAt",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "AppointmentNote",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "CheckedOutAt",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "ResolutionNote",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "RootCause",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "SlaResolveMet",
                schema: "fsm",
                table: "ticket");

            migrationBuilder.DropColumn(
                name: "SlaResponseMet",
                schema: "fsm",
                table: "ticket");
        }
    }
}
