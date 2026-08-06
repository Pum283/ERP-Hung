using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContractSalaryScanParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                schema: "hrm",
                table: "contract",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentContractId",
                schema: "hrm",
                table: "contract",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScanFileId",
                schema: "hrm",
                table: "contract",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseSalary",
                schema: "hrm",
                table: "contract");

            migrationBuilder.DropColumn(
                name: "ParentContractId",
                schema: "hrm",
                table: "contract");

            migrationBuilder.DropColumn(
                name: "ScanFileId",
                schema: "hrm",
                table: "contract");
        }
    }
}
