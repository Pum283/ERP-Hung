using System.Security.Cryptography;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Prt;
using Erp.Application.Interfaces.Services.Prt;
using Erp.Domain.Base;
using Erp.Domain.Entities.Prt;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Prt;

public sealed class PrtPortalService : IPrtPortalService
{
    private static readonly HashSet<string> OrderStatuses =
        new(StringComparer.OrdinalIgnoreCase)
            { "Draft", "Confirmed", "Shipping", "Delivered", "Cancelled" };
    private static readonly HashSet<string> TicketStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Open", "InProgress", "Resolved", "Closed" };

    private readonly AppDbContext _db;
    public PrtPortalService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PrtAccountDto>> ListAccountsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.PrtAccounts.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Email.Contains(term) || x.DisplayName.Contains(term)
                || x.Code.Contains(term) || (x.CustomerCode != null && x.CustomerCode.Contains(term)));
        }
        var list = await query.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct);
        return await MapAccountsAsync(tenantId, list, ct);
    }

    public async Task<PrtAccountDto> UpsertAccountAsync(
        Guid tenantId, Guid userId, PrtAccountUpsertRequest req, CancellationToken ct = default)
    {
        var email = NormEmail(req.Email);
        var name = Req(req.DisplayName, 200, "Tên hiển thị");
        PrtAccount entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.PrtAccounts, tenantId, id, "tài khoản portal", ct);
        else
        {
            if (await _db.PrtAccounts.AnyAsync(x => x.TenantId == tenantId && x.Email == email && !x.IsDeleted, ct))
                throw new AppException("Email đã đăng ký.");
            entity = new PrtAccount
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextCodeAsync(tenantId, "PRT", ct) : NormCode(req.Code),
                PasswordHash = Hash(string.IsNullOrWhiteSpace(req.Password) ? "!Abc123" : req.Password),
                Status = "Active", CreatedBy = userId
            };
            _db.PrtAccounts.Add(entity);
        }
        entity.Email = email; entity.DisplayName = name;
        entity.CustomerCode = NullIfEmpty(req.CustomerCode)?.ToUpperInvariant();
        entity.CustomerName = NullIfEmpty(req.CustomerName);
        if (!string.IsNullOrWhiteSpace(req.Password))
        {
            ValidatePasswordPolicy(req.Password);
            entity.PasswordHash = Hash(req.Password);
        }
        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            var s = req.Status.Trim();
            if (s is not ("Pending" or "Active" or "Locked")) throw new AppException("TT: Pending|Active|Locked.");
            entity.Status = s;
        }
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAccountsAsync(tenantId, [entity], ct))[0];
    }

    public Task<PrtAccountDto> RegisterAsync(
        Guid tenantId, Guid userId, PrtRegisterRequest req, CancellationToken ct = default)
        => UpsertAccountAsync(tenantId, userId, new PrtAccountUpsertRequest(
            null, null, req.Email, req.DisplayName, req.Password, req.CustomerCode, null, "Pending"), ct);

    private static void ValidatePasswordPolicy(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new AppException("Mật khẩu tối thiểu 8 ký tự.");
        if (!password.Any(char.IsUpper))
            throw new AppException("Mật khẩu phải chứa ít nhất 1 chữ hoa (A-Z).");
        if (!password.Any(char.IsLower))
            throw new AppException("Mật khẩu phải chứa ít nhất 1 chữ thường (a-z).");
        if (!password.Any(char.IsDigit))
            throw new AppException("Mật khẩu phải chứa ít nhất 1 chữ số (0-9).");
    }

    public async Task<PrtLoginResultDto> LoginAsync(
        Guid tenantId, PrtLoginRequest req, CancellationToken ct = default)
    {
        var email = NormEmail(req.Email);
        var acc = await _db.PrtAccounts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email && !x.IsDeleted, ct)
            ?? throw new AppException("Email hoặc mật khẩu không đúng.", 401);

        if (acc.Status == "Locked")
            throw new AppException("Tài khoản đã bị khóa do vi phạm hoặc thử sai mật khẩu nhiều lần.", 401);

        var failedCount = await _db.IntegrationCallLogs.AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.Kind == "PRT_LOGIN_FAILED" && x.Target == email && x.CalledAt > DateTimeOffset.UtcNow.AddMinutes(-15), ct);

        if (failedCount >= 5)
        {
            acc.Status = "Locked";
            _db.IntegrationCallLogs.Add(new IntegrationCallLog
            {
                TenantId = tenantId, Kind = "PRT_ACCOUNT_LOCKED", Target = email,
                RequestSummary = "Auto-locked due to 5 consecutive failed login attempts",
                ResponseSummary = "Account status updated to Locked", StatusCode = 403, CalledAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            throw new AppException("Tài khoản bị tạm khóa do nhập sai mật khẩu quá 5 lần trong 15 phút.", 401);
        }

        if (acc.PasswordHash != Hash(req.Password ?? ""))
        {
            _db.IntegrationCallLogs.Add(new IntegrationCallLog
            {
                TenantId = tenantId, Kind = "PRT_LOGIN_FAILED", Target = email,
                RequestSummary = $"Failed attempt #{failedCount + 1}",
                ResponseSummary = "Invalid password hash match", StatusCode = 401, CalledAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            throw new AppException("Email hoặc mật khẩu không đúng.", 401);
        }

        acc.LastLoginAt = DateTimeOffset.UtcNow;
        if (acc.Status == "Pending") acc.Status = "Active";

        _db.IntegrationCallLogs.Add(new IntegrationCallLog
        {
            TenantId = tenantId, Kind = "PRT_LOGIN_SUCCESS", Target = email,
            RequestSummary = "User authentication successful",
            ResponseSummary = "JWT Session Token Issued", StatusCode = 200, CalledAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        var token = $"prt_token_{acc.Id:N}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var dto = (await MapAccountsAsync(tenantId, [acc], ct))[0];
        return new PrtLoginResultDto(dto, token, "Đăng nhập portal thành công");
    }

    public Task<PrtLoginResultDto> LoginStubAsync(
        Guid tenantId, PrtLoginRequest req, CancellationToken ct = default)
        => LoginAsync(tenantId, req, ct);

    public async Task<PrtAccountDto> ForgotPasswordAsync(
        Guid tenantId, PrtForgotPasswordRequest req, CancellationToken ct = default)
    {
        var email = NormEmail(req.Email);
        var acc = await _db.PrtAccounts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy email đăng ký portal.", 404);

        if (acc.Status == "Locked")
            throw new AppException("Tài khoản đang bị khóa, vui lòng liên hệ quản trị viên.", 400);

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        acc.ResetTokenStub = token;
        acc.ResetTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(2);

        _db.IntegrationCallLogs.Add(new IntegrationCallLog
        {
            TenantId = tenantId,
            Kind = "PRT_RESET_PASSWORD",
            Target = acc.Email,
            RequestSummary = $"Token:{token}",
            ResponseSummary = "Reset token generated and valid for 2 hours",
            StatusCode = 200,
            CalledAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return (await MapAccountsAsync(tenantId, [acc], ct))[0];
    }

    public Task<PrtAccountDto> ForgotPasswordStubAsync(
        Guid tenantId, PrtForgotPasswordRequest req, CancellationToken ct = default)
        => ForgotPasswordAsync(tenantId, req, ct);

    public async Task<PrtAccountDto> ResetPasswordAsync(
        Guid tenantId, PrtResetPasswordRequest req, CancellationToken ct = default)
    {
        var email = NormEmail(req.Email);
        var token = (req.ResetToken ?? "").Trim();
        var pass = (req.NewPassword ?? "").Trim();
        if (string.IsNullOrEmpty(token)) throw new AppException("Token không được để trống.");
        
        ValidatePasswordPolicy(pass);

        var acc = await _db.PrtAccounts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy tài khoản.", 404);

        if (string.IsNullOrEmpty(acc.ResetTokenStub) || !string.Equals(acc.ResetTokenStub, token, StringComparison.OrdinalIgnoreCase))
            throw new AppException("Reset token không hợp lệ hoặc đã được sử dụng.");
        if (acc.ResetTokenExpiresAt is DateTimeOffset exp && exp < DateTimeOffset.UtcNow)
            throw new AppException("Reset token đã hết hạn (chỉ có hiệu lực trong 2 giờ).");

        acc.PasswordHash = Hash(pass);
        acc.ResetTokenStub = null;
        acc.ResetTokenExpiresAt = null;
        if (acc.Status is "Pending" or "Locked") acc.Status = "Active";

        _db.IntegrationCallLogs.Add(new IntegrationCallLog
        {
            TenantId = tenantId,
            Kind = "PRT_RESET_PASSWORD_SUCCESS",
            Target = acc.Email,
            RequestSummary = "Password successfully reset via token",
            ResponseSummary = "Account status updated to Active and token cleared",
            StatusCode = 200,
            CalledAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return (await MapAccountsAsync(tenantId, [acc], ct))[0];
    }

    public async Task<PrtAccountDto> LinkCustomerAsync(
        Guid tenantId, Guid userId, PrtLinkCustomerRequest req, CancellationToken ct = default)
    {
        var acc = await RequireAsync(_db.PrtAccounts, tenantId, req.AccountId, "tài khoản portal", ct);
        var code = NormCode(req.CustomerCode);
        acc.CustomerCode = code;
        acc.CustomerName = NullIfEmpty(req.CustomerName) ?? code;
        if (acc.Status == "Pending") acc.Status = "Active";
        acc.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAccountsAsync(tenantId, [acc], ct))[0];
    }

    public async Task<IReadOnlyList<PrtOrderDto>> ListOrdersAsync(
        Guid tenantId, Guid? accountId, CancellationToken ct = default)
    {
        var q = _db.PrtOrders.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (accountId is Guid aid) q = q.Where(x => x.AccountId == aid);
        var list = await q.OrderByDescending(x => x.OrderDate).Take(200).ToListAsync(ct);
        return await MapOrdersAsync(tenantId, list, ct);
    }

    public async Task<PrtOrderDetailDto> GetOrderDetailAsync(
        Guid tenantId, Guid orderId, CancellationToken ct = default)
    {
        var o = await _db.PrtOrders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy đơn.", 404);
        var dto = (await MapOrdersAsync(tenantId, [o], ct))[0];
        var lines = await _db.PrtOrderLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OrderId == orderId && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        return new PrtOrderDetailDto(dto, lines.Select(l => new PrtOrderLineDto(
            l.Id, l.OrderId, l.ItemCode, l.ItemName, l.Quantity, l.UnitPrice, l.LineAmount, l.LineNo)).ToList());
    }

    public async Task<PrtOrderDto> UpsertOrderAsync(
        Guid tenantId, Guid userId, PrtOrderUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.PrtAccounts, tenantId, req.AccountId, "tài khoản portal", ct);
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Confirmed" : req.Status.Trim();
        if (!OrderStatuses.Contains(status)) throw new AppException("TT đơn: Draft|Confirmed|Shipping|Delivered|Cancelled.");

        PrtOrder entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.PrtOrders, tenantId, id, "đơn hàng", ct);
        else
        {
            entity = new PrtOrder
            {
                TenantId = tenantId, AccountId = req.AccountId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextCodeAsync(tenantId, "SO", ct) : NormCode(req.Code),
                CreatedBy = userId
            };
            _db.PrtOrders.Add(entity);
        }
        entity.AccountId = req.AccountId;
        entity.OrderDate = req.OrderDate ?? entity.OrderDate;
        entity.Status = OrderStatuses.First(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));
        entity.ShippingAddress = NullIfEmpty(req.ShippingAddress);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        if (req.Lines is { Count: > 0 })
        {
            var old = await _db.PrtOrderLines
                .Where(x => x.TenantId == tenantId && x.OrderId == entity.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var o in old) { o.IsDeleted = true; o.DeletedAt = DateTimeOffset.UtcNow; o.UpdatedBy = userId; }
            var n = 1; decimal total = 0;
            foreach (var l in req.Lines)
            {
                if (l.Quantity <= 0 || l.UnitPrice < 0) throw new AppException("SL > 0, đơn giá ≥ 0.");
                var amt = Math.Round(l.Quantity * l.UnitPrice, 2);
                _db.PrtOrderLines.Add(new PrtOrderLine
                {
                    TenantId = tenantId, OrderId = entity.Id,
                    ItemCode = NormCode(l.ItemCode), ItemName = Req(l.ItemName, 200, "Tên hàng"),
                    Quantity = l.Quantity, UnitPrice = l.UnitPrice, LineAmount = amt,
                    LineNo = n++, CreatedBy = userId
                });
                total += amt;
            }
            entity.TotalAmount = total;
            await _db.SaveChangesAsync(ct);
        }

        return (await MapOrdersAsync(tenantId, [entity], ct))[0];
    }

    public async Task<PrtArSummaryDto> GetArSummaryAsync(
        Guid tenantId, Guid accountId, CancellationToken ct = default)
    {
        var acc = await RequireAsync(_db.PrtAccounts, tenantId, accountId, "tài khoản portal", ct);
        var now = DateTimeOffset.UtcNow;
        var openPrt = await _db.PrtInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AccountId == accountId && !x.IsDeleted && x.OpenAmount > 0)
            .ToListAsync(ct);
        var year = DateTime.UtcNow.Year;
        var paidYtdPrt = await _db.PrtPayments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AccountId == accountId && !x.IsDeleted && x.PaidAt.Year == year)
            .SumAsync(x => (decimal?)x.Amount, ct) ?? 0;

        decimal finOpenAmount = 0;
        int finOpenCount = 0;
        decimal finPaidYtd = 0;
        decimal finOverdueAmount = 0;
        int finOverdueCount = 0;

        if (!string.IsNullOrWhiteSpace(acc.CustomerCode))
        {
            var custCode = acc.CustomerCode.Trim().ToUpperInvariant();
            var finInvoices = await _db.FinArInvoices.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.CustomerId == accountId || x.CustomerInvoiceNo == custCode))
                .ToListAsync(ct);

            var openFinInvoices = finInvoices.Where(x => x.Status != "Paid" && x.Status != "Void" && (x.TotalAmount - x.ReceivedAmount > 0)).ToList();
            finOpenAmount = openFinInvoices.Sum(x => x.TotalAmount - x.ReceivedAmount);
            finOpenCount = openFinInvoices.Count;

            var overdueFin = openFinInvoices.Where(x => x.DueDate < now).ToList();
            finOverdueAmount = overdueFin.Sum(x => x.TotalAmount - x.ReceivedAmount);
            finOverdueCount = overdueFin.Count;

            finPaidYtd = await _db.FinArReceipts.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.CustomerId == accountId && x.ReceiptDate.Year == year && x.Status != "Void")
                .SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
        }

        var overduePrt = openPrt.Where(x => x.DueDate.HasValue && x.DueDate.Value < now).ToList();
        var totalOverdueAmount = overduePrt.Sum(x => x.OpenAmount) + finOverdueAmount;
        var totalOverdueCount = overduePrt.Count + finOverdueCount;

        var totalOpenAmount = openPrt.Sum(x => x.OpenAmount) + finOpenAmount;
        var totalOpenCount = openPrt.Count + finOpenCount;
        var totalPaidYtd = paidYtdPrt + finPaidYtd;

        return new PrtArSummaryDto(accountId, totalOpenAmount, totalOpenCount, totalPaidYtd, totalOverdueAmount, totalOverdueCount);
    }

    public async Task<IReadOnlyList<PrtInvoiceDto>> ListInvoicesAsync(
        Guid tenantId, Guid accountId, bool openOnly, CancellationToken ct = default)
    {
        var q = _db.PrtInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AccountId == accountId && !x.IsDeleted);
        if (openOnly) q = q.Where(x => x.OpenAmount > 0);
        var list = await q.OrderByDescending(x => x.InvoiceDate).Take(100).ToListAsync(ct);
        return list.Select(MapInvoice).ToList();
    }

    public async Task<PrtInvoiceDto> UpsertInvoiceAsync(
        Guid tenantId, Guid userId, PrtInvoiceUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.PrtAccounts, tenantId, req.AccountId, "tài khoản portal", ct);
        if (req.Amount < 0) throw new AppException("Số tiền ≥ 0.");
        var paid = req.PaidAmount ?? 0;
        if (paid < 0 || paid > req.Amount) throw new AppException("PaidAmount không hợp lệ.");

        PrtInvoice entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.PrtInvoices, tenantId, id, "hóa đơn", ct);
        else
        {
            entity = new PrtInvoice
            {
                TenantId = tenantId, AccountId = req.AccountId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextCodeAsync(tenantId, "INV", ct) : NormCode(req.Code),
                CreatedBy = userId
            };
            _db.PrtInvoices.Add(entity);
        }
        entity.InvoiceDate = req.InvoiceDate ?? entity.InvoiceDate;
        entity.DueDate = req.DueDate;
        entity.Amount = req.Amount;
        entity.PaidAmount = paid;
        entity.OpenAmount = req.Amount - paid;
        entity.Status = entity.OpenAmount <= 0 ? "Paid" : paid > 0 ? "Partial" : "Open";
        if (!string.IsNullOrWhiteSpace(req.Status)) entity.Status = req.Status.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapInvoice(entity);
    }

    public async Task<IReadOnlyList<PrtPaymentDto>> ListPaymentsAsync(
        Guid tenantId, Guid accountId, CancellationToken ct = default)
    {
        var list = await _db.PrtPayments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AccountId == accountId && !x.IsDeleted)
            .OrderByDescending(x => x.PaidAt).Take(100).ToListAsync(ct);
        var invIds = list.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value).Distinct().ToList();
        var invs = invIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.PrtInvoices.AsNoTracking().Where(x => invIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        return list.Select(p => new PrtPaymentDto(
            p.Id, p.AccountId, p.InvoiceId,
            p.InvoiceId is Guid i ? invs.GetValueOrDefault(i) : null,
            p.Code, p.PaidAt, p.Amount, p.Method, p.Note)).ToList();
    }

    public async Task<PrtPaymentDto> UpsertPaymentAsync(
        Guid tenantId, Guid userId, PrtPaymentUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireAsync(_db.PrtAccounts, tenantId, req.AccountId, "tài khoản portal", ct);
        if (req.Amount <= 0) throw new AppException("Số tiền thanh toán > 0.");
        if (req.InvoiceId is Guid iid)
            _ = await RequireAsync(_db.PrtInvoices, tenantId, iid, "hóa đơn", ct);

        PrtPayment entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.PrtPayments, tenantId, id, "thanh toán", ct);
        else
        {
            entity = new PrtPayment
            {
                TenantId = tenantId, AccountId = req.AccountId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextCodeAsync(tenantId, "PAY", ct) : NormCode(req.Code),
                CreatedBy = userId
            };
            _db.PrtPayments.Add(entity);
        }
        entity.InvoiceId = req.InvoiceId;
        entity.PaidAt = req.PaidAt ?? DateTimeOffset.UtcNow;
        entity.Amount = req.Amount;
        entity.Method = string.IsNullOrWhiteSpace(req.Method) ? "Transfer" : req.Method.Trim();
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;

        if (req.InvoiceId is Guid invId)
        {
            var inv = await RequireAsync(_db.PrtInvoices, tenantId, invId, "hóa đơn", ct);
            inv.PaidAmount = Math.Min(inv.Amount, inv.PaidAmount + req.Amount);
            inv.OpenAmount = inv.Amount - inv.PaidAmount;
            inv.Status = inv.OpenAmount <= 0 ? "Paid" : "Partial";
            inv.UpdatedBy = userId;
        }

        await _db.SaveChangesAsync(ct);
        return new PrtPaymentDto(
            entity.Id, entity.AccountId, entity.InvoiceId, null, entity.Code,
            entity.PaidAt, entity.Amount, entity.Method, entity.Note);
    }

    public async Task<IReadOnlyList<PrtTicketDto>> ListTicketsAsync(
        Guid tenantId, Guid? accountId, CancellationToken ct = default)
    {
        var q = _db.PrtTickets.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (accountId is Guid aid) q = q.Where(x => x.AccountId == aid);
        var list = await q.OrderByDescending(x => x.OpenedAt).Take(200).ToListAsync(ct);
        var aids = list.Select(x => x.AccountId).Distinct().ToList();
        var emails = await _db.PrtAccounts.AsNoTracking()
            .Where(x => aids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Email, ct);
        return list.Select(t => new PrtTicketDto(
            t.Id, t.AccountId, emails.GetValueOrDefault(t.AccountId), t.Code, t.Subject,
            t.Description, t.Status, t.OpenedAt, t.ClosedAt)).ToList();
    }

    public async Task<PrtTicketDto> UpsertTicketAsync(
        Guid tenantId, Guid userId, PrtTicketUpsertRequest req, CancellationToken ct = default)
    {
        var acc = await RequireAsync(_db.PrtAccounts, tenantId, req.AccountId, "tài khoản portal", ct);
        var subject = Req(req.Subject, 200, "Tiêu đề");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Open" : req.Status.Trim();
        if (!TicketStatuses.Contains(status)) throw new AppException("TT ticket: Open|InProgress|Resolved|Closed.");

        PrtTicket entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.PrtTickets, tenantId, id, "ticket", ct);
        else
        {
            entity = new PrtTicket
            {
                TenantId = tenantId, AccountId = req.AccountId,
                Code = await NextCodeAsync(tenantId, "TKT", ct),
                OpenedAt = DateTimeOffset.UtcNow, CreatedBy = userId
            };
            _db.PrtTickets.Add(entity);
        }
        entity.Subject = subject;
        entity.Description = NullIfEmpty(req.Description);
        entity.Status = TicketStatuses.First(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));
        if (entity.Status is "Resolved" or "Closed") entity.ClosedAt ??= DateTimeOffset.UtcNow;
        else entity.ClosedAt = null;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new PrtTicketDto(
            entity.Id, entity.AccountId, acc.Email, entity.Code, entity.Subject,
            entity.Description, entity.Status, entity.OpenedAt, entity.ClosedAt);
    }

    private async Task<IReadOnlyList<PrtAccountDto>> MapAccountsAsync(
        Guid tenantId, List<PrtAccount> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var orderCounts = await _db.PrtOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.AccountId) && !x.IsDeleted)
            .GroupBy(x => x.AccountId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        var openAr = await _db.PrtInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.AccountId) && !x.IsDeleted)
            .GroupBy(x => x.AccountId).Select(g => new { g.Key, S = g.Sum(x => x.OpenAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.S, ct);
        return list.Select(a => new PrtAccountDto(
            a.Id, a.Code, a.Email, a.DisplayName, a.CustomerCode, a.CustomerName,
            a.Status, a.LastLoginAt, orderCounts.GetValueOrDefault(a.Id), openAr.GetValueOrDefault(a.Id))).ToList();
    }

    private async Task<IReadOnlyList<PrtOrderDto>> MapOrdersAsync(
        Guid tenantId, List<PrtOrder> list, CancellationToken ct)
    {
        var aids = list.Select(x => x.AccountId).Distinct().ToList();
        var ids = list.Select(x => x.Id).ToList();
        var emails = await _db.PrtAccounts.AsNoTracking()
            .Where(x => aids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Email, ct);
        var lineCounts = await _db.PrtOrderLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.OrderId) && !x.IsDeleted)
            .GroupBy(x => x.OrderId).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(o => new PrtOrderDto(
            o.Id, o.AccountId, emails.GetValueOrDefault(o.AccountId), o.Code, o.OrderDate,
            o.Status, o.TotalAmount, o.ShippingAddress, o.Note, lineCounts.GetValueOrDefault(o.Id))).ToList();
    }

    private static PrtInvoiceDto MapInvoice(PrtInvoice i)
    {
        var isOverdue = i.OpenAmount > 0 && i.DueDate.HasValue && i.DueDate.Value < DateTimeOffset.UtcNow;
        var overdueDays = isOverdue ? (int)Math.Max(0, (DateTimeOffset.UtcNow - i.DueDate!.Value).TotalDays) : 0;
        return new(i.Id, i.AccountId, i.Code, i.InvoiceDate, i.DueDate, i.Amount, i.PaidAmount, i.OpenAmount, i.Status, overdueDays, isOverdue);
    }

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var p = $"{prefix}-{DateTime.UtcNow:yyyyMM}-";
        // Check across relevant tables by prefix convention
        string? last = prefix switch
        {
            "PRT" => await _db.PrtAccounts.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(p) && !x.IsDeleted)
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            "SO" => await _db.PrtOrders.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(p) && !x.IsDeleted)
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            "INV" => await _db.PrtInvoices.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(p) && !x.IsDeleted)
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            "PAY" => await _db.PrtPayments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(p) && !x.IsDeleted)
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            _ => await _db.PrtTickets.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(p) && !x.IsDeleted)
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
        };
        var n = 1;
        if (last is not null && int.TryParse(last.AsSpan(p.Length), out var parsed)) n = parsed + 1;
        return $"{p}{n:D4}";
    }

    private static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("prt-stub|" + password));
        return Convert.ToHexString(bytes);
    }

    private static async Task<T> RequireAsync<T>(
        DbSet<T> set, Guid tenantId, Guid id, string label, CancellationToken ct)
        where T : TenantEntity
        => await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException($"Không tìm thấy {label}.", 404);

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string NormEmail(string? email)
    {
        var e = (email ?? "").Trim().ToLowerInvariant();
        if (e.Length is < 5 or > 200 || !e.Contains('@')) throw new AppException("Email không hợp lệ.");
        return e;
    }

    private static string Req(string? value, int max, string label)
    {
        var v = (value ?? "").Trim();
        if (v.Length is < 1 || v.Length > max) throw new AppException($"{label} 1–{max} ký tự.");
        return v;
    }

    private static string? NullIfEmpty(string? s)
    {
        var v = s?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }
}
