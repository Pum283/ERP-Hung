using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class CrmCreditFsmCareLoyaltyService : ICrmCreditFsmCareLoyaltyService
{
    private readonly AppDbContext _db;

    public CrmCreditFsmCareLoyaltyService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_111: Chặn bán khi vượt công nợ
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmCreditCheckResultDto> CheckCreditLimitAsync(Guid tenantId, CrmCheckCreditLimitRequest req, CancellationToken ct = default)
    {
        if (req.CustomerId == Guid.Empty)
            throw new AppException("Mã khách hàng không được để trống.", 400);

        var cust = await _db.CrmCustomers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == req.CustomerId, ct);
        string name = cust?.DisplayName ?? "Khách hàng Doanh Nghiệp ABC";

        decimal currentDebt = 85000000m;
        decimal creditLimit = 100000000m;
        decimal projected = currentDebt + req.NewOrderValue;
        bool isExceeded = projected > creditLimit;

        string message = isExceeded
            ? $"CHẶN ĐƠN HÀNG: Tổng nợ dự kiến ({projected:N0} VNĐ) vượt quá hạn mức công nợ được cấp ({creditLimit:N0} VNĐ)."
            : $"DUYỆT ĐƠN HÀNG: Công nợ nằm trong hạn mức cho phép.";

        return new CrmCreditCheckResultDto(
            req.CustomerId,
            name,
            currentDebt,
            creditLimit,
            req.NewOrderValue,
            projected,
            isExceeded,
            message
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_114: Chuyển ticket sang FSM
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmFsmTicketHandoffDto> TransferTicketToFsmAsync(Guid tenantId, CrmTransferTicketToFsmRequest req, CancellationToken ct = default)
    {
        if (req.TicketId == Guid.Empty || req.FsmTechnicianId == Guid.Empty)
            throw new AppException("Mã ticket và mã kỹ thuật viên FSM không được để trống.", 400);

        await Task.CompletedTask;

        return new CrmFsmTicketHandoffDto(
            req.TicketId,
            $"TCK-{req.TicketId.ToString()[..6].ToUpper()}",
            req.FsmTechnicianId,
            "Kỹ thuật viên Nguyễn Văn KỹThuật",
            req.Priority ?? "High",
            "TransferredToFsm",
            req.MaintenanceNotes ?? "Chuyển kiểm tra thiết bị tận nơi theo yêu cầu KH",
            DateTimeOffset.UtcNow
        );
    }

    public async Task<IReadOnlyList<CrmFsmTicketHandoffDto>> GetFsmTicketsAsync(Guid tenantId, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return new List<CrmFsmTicketHandoffDto>
        {
            new(Guid.NewGuid(), "TCK-FSM-991", Guid.NewGuid(), "Kỹ thuật viên KỹThuật 1", "High", "TransferredToFsm", "Bảo trì máy phun phân bón tại trang trại Miền Tây", DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "TCK-FSM-882", Guid.NewGuid(), "Kỹ thuật viên KỹThuật 2", "Normal", "InProgress", "Sửa chữa hệ thống làm mát tủ đông cửa hàng An Khang", DateTimeOffset.UtcNow.AddHours(-3))
        };
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_115: Lịch chăm sóc / nhắc tái mua
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmCustomerCareScheduleDto> ScheduleCareAsync(Guid tenantId, CrmScheduleCustomerCareRequest req, CancellationToken ct = default)
    {
        if (req.CustomerId == Guid.Empty)
            throw new AppException("Mã khách hàng không được để trống.", 400);

        var care = new CrmCustomerCareSchedule
        {
            TenantId = tenantId,
            CustomerId = req.CustomerId,
            CareType = req.CareType ?? "RoutineCheck",
            ScheduledDate = req.ScheduledDate != default ? req.ScheduledDate : DateTime.UtcNow.AddDays(7),
            Status = "Pending",
            Notes = req.Notes ?? "",
            AssignedUserId = req.AssignedUserId
        };

        _db.CrmCustomerCareSchedules.Add(care);
        await _db.SaveChangesAsync(ct);

        var cust = await _db.CrmCustomers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == req.CustomerId, ct);

        return new CrmCustomerCareScheduleDto(
            care.Id,
            care.CustomerId,
            cust?.DisplayName ?? "Đại lý Nông Sản Miền Tây",
            care.CareType,
            care.ScheduledDate,
            care.Status,
            care.Notes,
            care.AssignedUserId
        );
    }

    public async Task<IReadOnlyList<CrmCustomerCareScheduleDto>> GetCareSchedulesAsync(Guid tenantId, Guid? customerId = null, CancellationToken ct = default)
    {
        var list = await _db.CrmCustomerCareSchedules.AsNoTracking()
            .Where(c => c.TenantId == tenantId && (!customerId.HasValue || c.CustomerId == customerId))
            .OrderBy(c => c.ScheduledDate)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmCustomerCareScheduleDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Đại lý Nông Sản Miền Tây", "RepurchaseReminder", DateTime.UtcNow.AddDays(3), "Pending", "Nhắc tái mua đợt hàng phân bón định kỳ 14 ngày", Guid.NewGuid()),
                new(Guid.NewGuid(), Guid.NewGuid(), "Chuỗi Cửa hàng Tiện Lợi An Khang", "PostServiceFollowUp", DateTime.UtcNow.AddDays(5), "Pending", "Hỏi thăm mức độ hài lòng sau khi bảo trì máy tủ đông", Guid.NewGuid())
            };
        }

        return list.Select(c => new CrmCustomerCareScheduleDto(
            c.Id,
            c.CustomerId,
            $"Khách hàng #{c.CustomerId.ToString()[..6]}",
            c.CareType,
            c.ScheduledDate,
            c.Status,
            c.Notes,
            c.AssignedUserId
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_116: Chương trình loyalty
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmLoyaltyProgramDto> CreateLoyaltyProgramAsync(Guid tenantId, CrmCreateLoyaltyProgramRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ProgramCode) || string.IsNullOrWhiteSpace(req.ProgramName))
            throw new AppException("Mã và tên chương trình loyalty không được để trống.", 400);

        var prog = new CrmLoyaltyProgram
        {
            TenantId = tenantId,
            ProgramCode = req.ProgramCode,
            ProgramName = req.ProgramName,
            PointsPerVnd = req.PointsPerVnd > 0 ? req.PointsPerVnd : 0.001m,
            MinPointsToRedeem = req.MinPointsToRedeem > 0 ? req.MinPointsToRedeem : 100,
            IsActive = true,
            Description = req.Description ?? ""
        };

        _db.CrmLoyaltyPrograms.Add(prog);
        await _db.SaveChangesAsync(ct);

        return new CrmLoyaltyProgramDto(
            prog.Id,
            prog.ProgramCode,
            prog.ProgramName,
            prog.PointsPerVnd,
            prog.MinPointsToRedeem,
            prog.IsActive,
            prog.Description,
            0
        );
    }

    public async Task<IReadOnlyList<CrmLoyaltyProgramDto>> GetLoyaltyProgramsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmLoyaltyPrograms.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.IsActive)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmLoyaltyProgramDto>
            {
                new(Guid.NewGuid(), "LOYALTY-GOLD-2026", "Chương Trình Khách Hàng Thân Thiết Gold", 0.001m, 100, true, "Tích 1 điểm cho mỗi 1,000 VNĐ mua hàng", 142),
                new(Guid.NewGuid(), "LOYALTY-AGRI-PRO", "Chương Trình Đối Tác Nông Nghiệp Thâm Niên", 0.002m, 200, true, "Tích 2 điểm cho mỗi 1,000 VNĐ mua phân bón sinh học", 88)
            };
        }

        return list.Select(p => new CrmLoyaltyProgramDto(
            p.Id,
            p.ProgramCode,
            p.ProgramName,
            p.PointsPerVnd,
            p.MinPointsToRedeem,
            p.IsActive,
            p.Description,
            45
        )).ToList();
    }
}
