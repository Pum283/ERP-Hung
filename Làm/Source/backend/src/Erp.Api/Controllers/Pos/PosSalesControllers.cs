using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Pos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Pos;

[ApiController]
[Authorize]
[Route("api/pos/shifts")]
public sealed class PosShiftController : ControllerBase
{
    private readonly IPosSalesService _svc;
    public PosShiftController(IPosSalesService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.shift.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosShiftDto>>>> List(
        [FromQuery] Guid? storeId, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosShiftDto>>.Ok(await _svc.ListShiftsAsync(TenantId, storeId, status, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pos.shift.read")]
    public async Task<ActionResult<ApiResponse<PosShiftDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PosShiftDetailDto>.Ok(await _svc.GetShiftDetailAsync(TenantId, id, ct)));

    [HttpPost("open")]
    [AuthorizePermission("pos.shift.manage")]
    public async Task<ActionResult<ApiResponse<PosShiftDto>>> Open(
        [FromBody] PosShiftOpenRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosShiftDto>.Ok(await _svc.OpenShiftAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("pos.shift.manage")]
    public async Task<ActionResult<ApiResponse<PosShiftDto>>> Close(
        Guid id, [FromBody] PosShiftCloseRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosShiftDto>.Ok(await _svc.CloseShiftAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/print-report")]
    [AuthorizePermission("pos.shift.manage")]
    public async Task<ActionResult<ApiResponse<PosShiftDto>>> PrintReport(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PosShiftDto>.Ok(await _svc.PrintShiftReportAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/pos/sales")]
public sealed class PosSaleController : ControllerBase
{
    private readonly IPosSalesService _svc;
    public PosSaleController(IPosSalesService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosSaleDto>>>> List(
        [FromQuery] Guid? shiftId, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosSaleDto>>.Ok(await _svc.ListSalesAsync(TenantId, shiftId, status, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<PosSaleDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDetailDto>.Ok(await _svc.GetSaleDetailAsync(TenantId, id, ct)));

    [HttpPost("open")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> Open(
        [FromBody] PosSaleOpenRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.OpenSaleAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleLineDto>>> UpsertLine(
        Guid id, [FromBody] PosSaleLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSaleLineDto>.Ok(await _svc.UpsertSaleLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/hold")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> Hold(
        Guid id, [FromBody] PosSaleHoldRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.HoldSaleAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/resume")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> Resume(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.ResumeSaleAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/lines/{lineId:guid}/cancel")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleLineDto>>> CancelLine(
        Guid id, Guid lineId, CancellationToken ct)
        => Ok(ApiResponse<PosSaleLineDto>.Ok(await _svc.CancelSaleLineAsync(TenantId, UserId, id, lineId, ct)));

    [HttpPost("{id:guid}/cancel")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> Cancel(
        Guid id, [FromBody] PosSaleHoldRequest? req, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.CancelSaleAsync(TenantId, UserId, id, req?.Note, ct)));

    [HttpPost("{id:guid}/pay")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSalePaymentDto>>> Pay(
        Guid id, [FromBody] PosSalePayRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSalePaymentDto>.Ok(await _svc.PaySaleAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/print-receipt")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> PrintReceipt(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.PrintReceiptAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/pos/returns")]
public sealed class PosReturnController : ControllerBase
{
    private readonly IPosSalesService _svc;
    public PosReturnController(IPosSalesService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosReturnDto>>>> List(
        [FromQuery] Guid? saleId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosReturnDto>>.Ok(await _svc.ListReturnsAsync(TenantId, saleId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<PosReturnDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PosReturnDetailDto>.Ok(await _svc.GetReturnDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosReturnDto>>> Create(
        [FromBody] PosReturnCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosReturnDto>.Ok(await _svc.CreateReturnAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosReturnLineDto>>> AddLine(
        Guid id, [FromBody] PosReturnLineRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosReturnLineDto>.Ok(await _svc.AddReturnLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/complete")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosReturnDto>>> Complete(
        Guid id, [FromBody] PosReturnCompleteRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosReturnDto>.Ok(await _svc.CompleteReturnAsync(TenantId, UserId, id, req, ct)));
}
