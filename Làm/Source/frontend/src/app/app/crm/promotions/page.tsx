"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchCrmQuotes, type CrmQuoteDto } from "@/shared/api/crm-sales-api";
import {
  applyCrmPromotionOnQuote,
  calcPromoDiscount,
  canSyncPromoToPos,
  fetchCrmPromotions,
  fetchCrmVoucherUsageReport,
  fetchCrmVouchers,
  formatSyncToPosMessage,
  generateCrmVouchers,
  summarizeVoucherUsageReport,
  syncCrmPromotionToPos,
  upsertCrmPromotion,
  type CrmPromotionDto,
  type CrmVoucherDto,
  type CrmVoucherUsageReportRowDto,
} from "@/shared/api/crm-marketing-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

function pillTone(status: string): "brand" | "success" | "warning" | "danger" | "muted" {
  const s = status.toLowerCase();
  if (s === "active" || s === "synced") return "success";
  if (s === "expired" || s === "cancelled" || s === "used") return "danger";
  if (s === "draft") return "warning";
  return "muted";
}

export default function CrmPromotionsPage() {
  const { can } = usePermissions();
  const canRead = can("crm.promotion.read");
  const canManage = can("crm.promotion.manage");

  const [list, setList] = useState<CrmPromotionDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [vouchers, setVouchers] = useState<CrmVoucherDto[]>([]);
  const [usageRows, setUsageRows] = useState<CrmVoucherUsageReportRowDto[]>([]);
  const [quotes, setQuotes] = useState<CrmQuoteDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [code, setCode] = useState("");
  const [name, setName] = useState("Giảm 10%");
  const [discountType, setDiscountType] = useState("Percentage");
  const [discountValue, setDiscountValue] = useState("10");
  const [minOrder, setMinOrder] = useState("0");
  const [maxUsage, setMaxUsage] = useState("100");
  const [maxPerCust, setMaxPerCust] = useState("1");
  const [condType, setCondType] = useState("MinAmount");
  const [condValue, setCondValue] = useState("0");
  const [voucherQty, setVoucherQty] = useState("5");
  const [voucherPrefix, setVoucherPrefix] = useState("SUMMER");
  const [quoteId, setQuoteId] = useState("");
  const [previewSub, setPreviewSub] = useState("1000000");

  const selected = list.find((x) => x.id === selectedId);

  const load = useCallback(async () => {
    const [p, q] = await Promise.all([
      fetchCrmPromotions(),
      fetchCrmQuotes().catch(() => [] as CrmQuoteDto[]),
    ]);
    setList(p);
    setQuotes(q);
    if (!selectedId && p[0]) setSelectedId(p[0].id);
    if (!quoteId && q[0]) setQuoteId(q[0].id);
  }, [selectedId, quoteId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchCrmVouchers(selectedId).then(setVouchers).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
  }

  async function onSave(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    try {
      const row = await upsertCrmPromotion({
        code: code.trim(),
        name,
        discountType,
        discountValue: Number(discountValue) || 0,
        minOrderValue: Number(minOrder) || undefined,
        maxUsageTotal: Number(maxUsage) || undefined,
        maxUsagePerCustomer: Number(maxPerCust) || undefined,
        conditions: [
          { conditionType: condType, conditionValue: condValue || "0", operator: "GreaterThan" },
        ],
      });
      flash(`Đã lưu CTKM ${row.code}`);
      setCode("");
      await load();
      setSelectedId(row.id);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi lưu CTKM");
    }
  }

  async function onGenerate(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !selectedId) return;
    try {
      const rows = await generateCrmVouchers(selectedId, {
        quantity: Number(voucherQty) || 1,
        prefix: voucherPrefix,
        maxUsagePerVoucher: 1,
      });
      flash(`Đã sinh ${rows.length} voucher`);
      setVouchers(await fetchCrmVouchers(selectedId));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi sinh voucher");
    }
  }

  async function onApply() {
    if (!canManage || !quoteId || !selectedId) return;
    try {
      const r = await applyCrmPromotionOnQuote({ quoteId, promotionId: selectedId });
      flash(r.message || (r.applied ? `Đã giảm ${money(r.discountAmount)}` : "Không áp được"));
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi áp CTKM");
    }
  }

  async function onSyncPos() {
    if (!canManage || !selectedId || !selected) return;
    if (!canSyncPromoToPos(selected.discountType, selected.discountValue)) {
      setError("Chỉ sync Percentage/FixedAmount với giá trị > 0");
      return;
    }
    try {
      const r = await syncCrmPromotionToPos(selectedId);
      flash(formatSyncToPosMessage(r));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi sync POS");
    }
  }

  async function onLoadUsage() {
    if (!canRead) return;
    try {
      const rows = await fetchCrmVoucherUsageReport(
        selectedId ? { promotionId: selectedId } : undefined,
      );
      setUsageRows(rows);
      const s = summarizeVoucherUsageReport(rows);
      flash(`BC voucher: ${s.voucherCount} mã · ${s.redeemTotal} lượt · giảm ${money(s.discountTotal)}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi BC voucher");
    }
  }

  const preview = selected
    ? calcPromoDiscount({
        discountType: selected.discountType,
        discountValue: selected.discountValue,
        maxDiscountAmount: selected.maxDiscountAmount,
        minOrderValue: selected.minOrderValue,
        subTotal: Number(previewSub) || 0,
      })
    : 0;

  if (!canRead) {
    return <div className="p-6 text-sm text-rose-600">Thiếu quyền crm.promotion.read</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div>
        <h1 className="text-xl font-semibold text-slate-800">Khuyến mại & voucher</h1>
        <p className="text-sm text-slate-500">UC_CRM_032–038 · sync POS · BC sử dụng voucher</p>
      </div>

      {error && <div className="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</div>}
      {ok && <div className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-slate-500">Đang tải…</div>}

      <div className="grid gap-4 lg:grid-cols-2">
        <div className={panel}>
          <h2 className="mb-3 text-sm font-semibold text-slate-700">Danh sách CTKM</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>Giảm</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {list.map((r) => (
                  <tr key={r.id} className="cursor-pointer hover:bg-slate-50" onClick={() => setSelectedId(r.id)}>
                    <td className={td}>{r.code}</td>
                    <td className={td}>{r.name}</td>
                    <td className={td}>
                      {r.discountType === "Percentage" ? `${r.discountValue}%` : money(r.discountValue)}
                    </td>
                    <td className={td}><span className={statusPill(pillTone(r.status))}>{r.status}</span></td>
                  </tr>
                ))}
                {list.length === 0 && <tr><td className={td} colSpan={4}>Chưa có CTKM.</td></tr>}
              </tbody>
            </table>
          </div>
          {selected && (
            <div className="mt-3 space-y-1 text-xs text-slate-600">
              <div>Điều kiện: {selected.conditions.map((c) => `${c.conditionType} ${c.operator} ${c.conditionValue}`).join("; ") || "—"}</div>
              <div>Đã dùng {selected.currentUsageCount}/{selected.maxUsageTotal ?? "∞"} · mỗi KH tối đa {selected.maxUsagePerCustomer ?? "∞"}</div>
              <div className="flex flex-wrap items-end gap-2 pt-2">
                <label className="text-xs">Preview SubTotal
                  <input className={field} type="number" value={previewSub} onChange={(e) => setPreviewSub(e.target.value)} />
                </label>
                <div className="pb-2 font-medium">→ giảm {money(preview)}</div>
              </div>
              {canManage && (
                <button type="button" className={`${btn.ghost} mt-2`} disabled={!selectedId} onClick={() => void onSyncPos()}>
                  Đồng bộ sang POS
                </button>
              )}
            </div>
          )}
        </div>

        <form className={`${panel} space-y-3`} onSubmit={onSave}>
          <h2 className="text-sm font-semibold text-slate-700">Tạo CTKM + điều kiện</h2>
          <label className="block text-xs text-slate-500">Mã
            <input className={field} value={code} onChange={(e) => setCode(e.target.value)} disabled={!canManage} />
          </label>
          <label className="block text-xs text-slate-500">Tên
            <input className={field} value={name} onChange={(e) => setName(e.target.value)} required disabled={!canManage} />
          </label>
          <div className="grid grid-cols-2 gap-2">
            <label className="block text-xs text-slate-500">Loại giảm
              <select className={field} value={discountType} onChange={(e) => setDiscountType(e.target.value)} disabled={!canManage}>
                {["Percentage", "FixedAmount", "FreeShipping"].map((c) => <option key={c}>{c}</option>)}
              </select>
            </label>
            <label className="block text-xs text-slate-500">Giá trị
              <input className={field} type="number" value={discountValue} onChange={(e) => setDiscountValue(e.target.value)} disabled={!canManage} />
            </label>
          </div>
          <div className="grid grid-cols-3 gap-2">
            <label className="block text-xs text-slate-500">Min đơn
              <input className={field} type="number" value={minOrder} onChange={(e) => setMinOrder(e.target.value)} disabled={!canManage} />
            </label>
            <label className="block text-xs text-slate-500">Max tổng
              <input className={field} type="number" value={maxUsage} onChange={(e) => setMaxUsage(e.target.value)} disabled={!canManage} />
            </label>
            <label className="block text-xs text-slate-500">Max / KH
              <input className={field} type="number" value={maxPerCust} onChange={(e) => setMaxPerCust(e.target.value)} disabled={!canManage} />
            </label>
          </div>
          <div className="grid grid-cols-2 gap-2">
            <label className="block text-xs text-slate-500">Điều kiện
              <select className={field} value={condType} onChange={(e) => setCondType(e.target.value)} disabled={!canManage}>
                {["MinAmount", "Product", "Category", "CustomerSegment", "MinQty"].map((c) => <option key={c}>{c}</option>)}
              </select>
            </label>
            <label className="block text-xs text-slate-500">Giá trị ĐK
              <input className={field} value={condValue} onChange={(e) => setCondValue(e.target.value)} disabled={!canManage} />
            </label>
          </div>
          {canManage && <button type="submit" className={btn.primary}>Lưu CTKM</button>}
        </form>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <form className={`${panel} space-y-3`} onSubmit={onGenerate}>
          <h2 className="text-sm font-semibold">Sinh voucher — {selected?.code ?? "—"}</h2>
          <div className="grid grid-cols-2 gap-2">
            <label className="block text-xs text-slate-500">Số lượng
              <input className={field} type="number" value={voucherQty} onChange={(e) => setVoucherQty(e.target.value)} />
            </label>
            <label className="block text-xs text-slate-500">Prefix
              <input className={field} value={voucherPrefix} onChange={(e) => setVoucherPrefix(e.target.value)} />
            </label>
          </div>
          {canManage && <button type="submit" className={btn.primary} disabled={!selectedId}>Sinh mã</button>}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>TT</th><th className={th}>Lượt</th></tr></thead>
              <tbody>
                {vouchers.slice(0, 20).map((v) => (
                  <tr key={v.id}>
                    <td className={`${td} font-mono text-xs`}>{v.voucherCode}</td>
                    <td className={td}><span className={statusPill(pillTone(v.status))}>{v.status}</span></td>
                    <td className={td}>{v.usageCount}/{v.maxUsage}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </form>

        <div className={`${panel} space-y-3`}>
          <h2 className="text-sm font-semibold">Áp CTKM lên báo giá</h2>
          <label className="block text-xs text-slate-500">Báo giá
            <select className={field} value={quoteId} onChange={(e) => setQuoteId(e.target.value)}>
              <option value="">— chọn —</option>
              {quotes.map((q) => (
                <option key={q.id} value={q.id}>{q.code} · {money(q.subTotal ?? q.totalAmount ?? 0)}</option>
              ))}
            </select>
          </label>
          {canManage && (
            <button type="button" className={btn.primary} disabled={!selectedId || !quoteId} onClick={onApply}>
              Áp khuyến mại
            </button>
          )}
        </div>
      </div>

      <div className={panel}>
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <h2 className="text-sm font-semibold">Báo cáo sử dụng voucher</h2>
          <button type="button" className={btn.ghost} onClick={() => void onLoadUsage()}>
            Tải BC {selected ? `(${selected.code})` : "(tất cả)"}
          </button>
        </div>
        <div className={tableWrap}>
          <table className="w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Voucher</th>
                <th className={th}>CTKM</th>
                <th className={th}>Lượt</th>
                <th className={th}>Giảm</th>
                <th className={th}>Gần nhất</th>
              </tr>
            </thead>
            <tbody>
              {usageRows.map((r) => (
                <tr key={r.voucherId}>
                  <td className={`${td} font-mono text-xs`}>{r.voucherCode}</td>
                  <td className={td}>{r.promotionCode}</td>
                  <td className={td}>{r.redeemCount}</td>
                  <td className={td}>{money(r.totalDiscount)}</td>
                  <td className={td}>{r.lastUsedAt ? new Date(r.lastUsedAt).toLocaleString("vi-VN") : "—"}</td>
                </tr>
              ))}
              {usageRows.length === 0 && (
                <tr><td className={td} colSpan={5}>Chưa tải hoặc chưa có lượt dùng.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
