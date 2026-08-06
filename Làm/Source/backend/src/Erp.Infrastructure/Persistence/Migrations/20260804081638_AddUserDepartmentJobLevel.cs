using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDepartmentJobLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobLevelId",
                schema: "erp_sys",
                table: "user_department",
                type: "uniqueidentifier",
                nullable: true);

            // Backfill job level từ AppUser (denorm primary) nếu membership chưa có
            migrationBuilder.Sql("""
                UPDATE ud
                SET ud.JobLevelId = u.JobLevelId
                FROM erp_sys.user_department ud
                INNER JOIN erp_sys.app_user u ON u.Id = ud.UserId
                WHERE ud.JobLevelId IS NULL AND u.JobLevelId IS NOT NULL AND ud.IsDeleted = 0;
                """);

            // Tạo membership primary nếu user có DepartmentId mà chưa có UserDepartment
            migrationBuilder.Sql("""
                INSERT INTO erp_sys.user_department
                    (Id, TenantId, UserId, DepartmentId, JobLevelId, IsPrimary, ValidFrom,
                     CreatedAt, UpdatedAt, IsDeleted, RowVersion)
                SELECT NEWID(), u.TenantId, u.Id, u.DepartmentId, u.JobLevelId, 1, CAST(GETUTCDATE() AS date),
                       SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET(), 0, 0
                FROM erp_sys.app_user u
                WHERE u.DepartmentId IS NOT NULL AND u.IsDeleted = 0
                  AND NOT EXISTS (
                      SELECT 1 FROM erp_sys.user_department ud
                      WHERE ud.UserId = u.Id AND ud.DepartmentId = u.DepartmentId AND ud.IsDeleted = 0);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_user_department_UserId_IsPrimary",
                schema: "erp_sys",
                table: "user_department",
                columns: new[] { "UserId", "IsPrimary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_department_UserId_IsPrimary",
                schema: "erp_sys",
                table: "user_department");

            migrationBuilder.DropColumn(
                name: "JobLevelId",
                schema: "erp_sys",
                table: "user_department");
        }
    }
}
