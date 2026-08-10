"use client";

import { useCallback, useEffect, useState } from "react";
import {
  fetchPrtAccounts,
  fetchPrtArSummary,
  fetchPrtInvoices,
  type PrtAccountDto,
  type PrtArSummaryDto,
  type PrtInvoiceDto,
} from "@/shared/api/prt-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, tableWrap, td, th } from "@/shared/ui/field";

export default function PrtInvoicesPage() {
  const { can } = usePermissions();
  const canRead = can("prt.portal.read");

  const [accounts, setAccounts] = useState<PrtAccountDto[]>([]);
  const [accountId, setAccountId] = useState("");
  const [arSummary, setArSummary] = useState<PrtArSummaryDto | null>(null);
  const [invoices, setInvoices] = useState<PrtInvoiceDto[]>([]);
  const [filterOverdueOnly, setFilterOverdueOnly] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const accs = await fetchPrtAccounts().catch(() => [] as PrtAccountDto[]);
      setAccounts(accs);
      const aid = accountId || accs[0]?.id || "";
      if (!accountId && aid) setAccountId(aid);
      if (!aid) {
        setArSummary(null);
        setInvoices([]);
        return;
      }
      const [summary, invList] = await Promise.all([
        fetchPrtArSummary(aid),
        fetchPrtInvoices(aid, true),
      ]);
      setArSummary(summary);
      setInvoices(invList);
    } catch (e) {
      setError((e as Error).message);
    }
  }, [accountId]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền truy cập trang Hóa đơn Portal.</div>;
  }

  const displayedInvoices = filterOverdueOnly
    ? invoices.filter((i) => i.isOverdue)
    : invoices;

  return (
    <div className="space-y-5 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Trang Công Nợ & Hóa Đơn Portal (UC_PRT_014–016)</h1>
          <p className="mt-1 text-sm text-slate-500">
            Theo dõi tổng dư nợ, cảnh báo nợ quá hạn và lịch sử hóa đơn tài chính của khách hàng.
          </p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void loadData()}>
          🔄 Tải lại dữ liệu
        </button>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}

      {/* Selector & Overview Cards */}
      <div className="flex flex-wrap items-center gap-4">
        <label className="text-sm font-medium text-slate-700">
          Khách hàng / Tài khoản Portal:
          <select
            className={`${field} ml-2 min-w-[280px] font-semibold text-slate-900`}
            value={accountId}
            onChange={(e) => setAccountId(e.target.value)}
          >
            <option value="">— Chọn tài khoản khách hàng —</option>
            {accounts.map((a) => (
              <option key={a.id} value={a.id}>
                {a.displayName || a.email} ({a.customerCode ?? "Không mã"})
              </option>
            ))}
          </select>
        </label>
      </div>

      {arSummary && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className="text-xs font-semibold uppercase tracking-wider text-slate-500">Tổng Nợ Mở (Open AR)</div>
            <div className="mt-2 text-2xl font-bold text-slate-900">{arSummary.openAmount.toLocaleString("vi-VN")} ₫</div>
            <div className="mt-1 text-xs text-slate-500">{arSummary.openInvoiceCount} hóa đơn chưa thanh toán</div>
          </div>

          <div className={`rounded-xl border p-5 shadow-sm ${arSummary.overdueAmount > 0 ? "border-red-200 bg-red-50/60" : "border-emerald-200 bg-emerald-50/60"}`}>
            <div className="text-xs font-semibold uppercase tracking-wider text-slate-600">Nợ Quá Hạn (Overdue)</div>
            <div className={`mt-2 text-2xl font-bold ${arSummary.overdueAmount > 0 ? "text-red-700" : "text-emerald-700"}`}>
              {arSummary.overdueAmount.toLocaleString("vi-VN")} ₫
            </div>
            <div className="mt-1 text-xs font-medium text-slate-600">
              {arSummary.overdueAmount > 0 ? `⚠️ ${arSummary.overdueInvoiceCount} HĐ quá hạn` : "✓ Không có nợ quá hạn"}
            </div>
          </div>

          <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className="text-xs font-semibold uppercase tracking-wider text-slate-500">Đã Thanh Toán YTD</div>
            <div className="mt-2 text-2xl font-bold text-emerald-600">{arSummary.paidYtd.toLocaleString("vi-VN")} ₫</div>
            <div className="mt-1 text-xs text-slate-500">Tổng doanh thu từ đầu năm</div>
          </div>

          <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className="text-xs font-semibold uppercase tracking-wider text-slate-500">Trạng Thái Hạn Mức</div>
            <div className="mt-2 text-2xl font-bold text-indigo-600">Hoạt động</div>
            <div className="mt-1 text-xs text-slate-500">Mã KH: {arSummary.customerCode || "—"}</div>
          </div>
        </div>
      )}

      {/* Invoices List Panel */}
      <section className={panel}>
        <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
          <h2 className="text-lg font-bold text-slate-900">Danh Sách Hóa Đơn Chi Tiết</h2>
          <div className="flex items-center gap-2">
            <button
              type="button"
              className={filterOverdueOnly ? btn.primary : btn.ghost}
              onClick={() => setFilterOverdueOnly(!filterOverdueOnly)}
            >
              {filterOverdueOnly ? "Hiển thị tất cả HĐ" : "⚠️ Chỉ xem HĐ quá hạn"}
            </button>
          </div>
        </div>

        {loading ? (
          <div className="p-8 text-center text-sm text-slate-500">Đang tải dữ liệu hóa đơn...</div>
        ) : displayedInvoices.length === 0 ? (
          <div className="p-8 text-center text-sm text-slate-500">Không tìm thấy hóa đơn nào.</div>
        ) : (
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã Hóa Đơn</th>
                  <th className={th}>Nguồn Chứng Từ</th>
                  <th className={th}>Tổng Tiền</th>
                  <th className={th}>Còn Nợ</th>
                  <th className={th}>Hạn Thanh Toán</th>
                  <th className={th}>Trạng Thái & Cảnh Báo</th>
                </tr>
              </thead>
              <tbody>
                {displayedInvoices.map((inv) => (
                  <tr key={inv.id} className={inv.isOverdue ? "bg-red-50/40 hover:bg-red-50/70" : "hover:bg-slate-50"}>
                    <td className={`${td} font-bold text-slate-900`}>{inv.code}</td>
                    <td className={td}>
                      <span className="rounded bg-slate-100 px-2 py-0.5 text-xs font-semibold text-slate-700">
                        {inv.sourceModule || "PRT"}
                      </span>
                    </td>
                    <td className={`${td} font-semibold text-slate-800`}>{inv.totalAmount.toLocaleString("vi-VN")} ₫</td>
                    <td className={`${td} font-bold ${inv.openAmount > 0 ? "text-amber-700" : "text-slate-500"}`}>
                      {inv.openAmount.toLocaleString("vi-VN")} ₫
                    </td>
                    <td className={td}>
                      {inv.dueDate ? new Date(inv.dueDate).toLocaleDateString("vi-VN") : "—"}
                    </td>
                    <td className={td}>
                      {inv.isOverdue ? (
                        <span className="inline-flex items-center gap-1 rounded-full bg-red-100 px-2.5 py-1 text-xs font-bold text-red-800">
                          🚨 Quá hạn ({inv.overdueDays} ngày)
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 rounded-full bg-emerald-100 px-2.5 py-1 text-xs font-medium text-emerald-800">
                          ✓ {inv.status}
                        </span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
