using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pur/advance-blanket-contract-expiration")]
public sealed class PurAdvanceBlanketContractExpirationController : ControllerBase
{
    private readonly IPurAdvanceBlanketContractExpirationService _svc;

    public PurAdvanceBlanketContractExpirationController(IPurAdvanceBlanketContractExpirationService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_PUR_044: Tạm ứng nhà cung cấp
    [HttpPost("advance-payments")]
    [AuthorizePermission("pur.advance.write")]
    public async Task<ActionResult<ApiResponse<PurVendorAdvancePaymentDto>>> CreateAdvancePayment([FromBody] PurCreateVendorAdvancePaymentRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurVendorAdvancePaymentDto>.Ok(await _svc.CreateAdvancePaymentAsync(TenantId, req, ct)));

    // UC_PUR_045 & UC_PUR_046: Hợp đồng mua khung & Theo dõi sản lượng/giá trị còn lại
    [HttpPost("blanket-contracts")]
    [AuthorizePermission("pur.contract.write")]
    public async Task<ActionResult<ApiResponse<PurBlanketContractDto>>> CreateBlanketContract([FromBody] PurCreateBlanketContractRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurBlanketContractDto>.Ok(await _svc.CreateBlanketContractAsync(TenantId, req, ct)));

    [HttpGet("blanket-contracts")]
    [AuthorizePermission("pur.contract.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurBlanketContractDto>>>> GetBlanketContracts(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurBlanketContractDto>>.Ok(await _svc.GetBlanketContractsAsync(TenantId, ct)));

    // UC_PUR_047: Cảnh báo hết hạn hợp đồng
    [HttpGet("blanket-contracts/expiring-alerts")]
    [AuthorizePermission("pur.contract.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurBlanketContractDto>>>> GetExpiringContractsAlerts([FromQuery] int warningDaysThreshold = 30, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PurBlanketContractDto>>.Ok(await _svc.GetExpiringContractsAlertsAsync(TenantId, warningDaysThreshold, ct)));
}
