using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmCreditFsmCareLoyaltyService
{
    // UC_CRM_111: Chặn bán khi vượt công nợ
    Task<CrmCreditCheckResultDto> CheckCreditLimitAsync(Guid tenantId, CrmCheckCreditLimitRequest req, CancellationToken ct = default);

    // UC_CRM_114: Chuyển ticket sang FSM
    Task<CrmFsmTicketHandoffDto> TransferTicketToFsmAsync(Guid tenantId, CrmTransferTicketToFsmRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmFsmTicketHandoffDto>> GetFsmTicketsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_115: Lịch chăm sóc / nhắc tái mua
    Task<CrmCustomerCareScheduleDto> ScheduleCareAsync(Guid tenantId, CrmScheduleCustomerCareRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmCustomerCareScheduleDto>> GetCareSchedulesAsync(Guid tenantId, Guid? customerId = null, CancellationToken ct = default);

    // UC_CRM_116: Chương trình loyalty
    Task<CrmLoyaltyProgramDto> CreateLoyaltyProgramAsync(Guid tenantId, CrmCreateLoyaltyProgramRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmLoyaltyProgramDto>> GetLoyaltyProgramsAsync(Guid tenantId, CancellationToken ct = default);
}
