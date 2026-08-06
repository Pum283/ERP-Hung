"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchInvSkus, fetchInvWarehouses, type InvSkuDto, type InvWarehouseDto } from "@/shared/api/inv-api";
import {
  downloadInvReportCsv,
  fetchInvDashboard,
  fetchInvMinMax,
  fetchInvMovement,
  fetchInvNearExpiry,
  fetchInvSkuCard,
  fetchInvStocktakeReport,
  fetchInvStockValue,
  type InvDashboardDto,
  type InvMinMaxAlertRowDto,
  type InvMovementPeriodRowDto,
  type InvNearExpiryRowDto,
  type InvSkuCardLineDto,
  type InvStocktakeReportRowDto,
  type InvStockValueRowDto,
} from "@/shared/api/inv-report-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}
function qty(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 2 });
}
function isoDay(d: Date) {
  return d.toISOString().slice(0, 10);
}

type Tab = "dashboard" | "value" | "movement" | "card" | "minmax" | "stocktake" | "expiry";

export default function InvReportsPage() {
  const { can } = usePermissions();
  const canRead = can("inv.stock.read") || can("inv.stocktake.read");

  const [tab, setTab] = useState<Tab>("dashboard");
  const [warehouses, setWarehouses] = useState<InvWarehouseDto[]>([]);
  const [skus, setSkus] = useState<InvSkuDto[]>([]);
  const [warehouseId, setWarehouseId] = useState("");
  const [skuId, setSkuId] = useState("");
  const [from, setFrom] = useState(() => {
    const d = new Date(); d.setMonth(d.getMonth() - 1); return isoDay(d);
  });
  const [to, setTo] = useState(() => isoDay(new Date()));
  const [dashboard, setDashboard] = useState<InvDashboardDto | null>(null);
  const [valueRows, setValueRows] = useState<InvStockValueRowDto[]>([]);
  const [moveRows, setMoveRows] = useState<InvMovementPeriodRowDto[]>([]);
  const [cardRows, setCardRows] = useState<InvSkuCardLineDto[]>([]);
  const [alertRows, setAlertRows] = useState<InvMinMaxAlertRowDto[]>([]);
  const [stRows, setStRows] = useState<InvStocktakeReportRowDto[]>([]);
  const [expiryRows, setExpiryRows] = useState<InvNearExpiryRowDto[]>([]);
  const [withinDays, setWithinDays] = useState("30");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const whParam = warehouseId ? { warehouseId } : {};
  const range = useCallback(() => ({
    from: new Date(from + "T00:00:00").toISOString(),
    to: new Date(to + "T23:59:59").toISOString(),
    ...whParam,
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }), [from, to, warehouseId]);

  const load = useCallback(async () => {
    if (tab === "dashboard") setDashboard(await fetchInvDashboard(whParam));
    else if (tab === "value") setValueRows(await fetchInvStockValue(whParam));
    else if (tab === "movement") setMoveRows(await fetchInvMovement(range()));
    else if (tab === "card") {
      if (!skuId) { setCardRows([]); return; }
      setCardRows(await fetchInvSkuCard({ skuId, ...range() }));
    } else if (tab === "minmax") setAlertRows(await fetchInvMinMax(whParam));
    else if (tab === "stocktake") setStRows(await fetchInvStocktakeReport(whParam));
    else setExpiryRows(await fetchInvNearExpiry({ ...whParam, withinDays: Number(withinDays) || 30 }));
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tab, range, skuId, warehouseId, withinDays]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    setError(null);
    Promise.all([
      fetchInvWarehouses().catch(() => [] as InvWarehouseDto[]),
      fetchInvSkus().catch(() => [] as InvSkuDto[]),
      load(),
    ])
      .then(([w, s]) => {
        setWarehouses(w);
        setSkus(s);
        setSkuId((prev) => prev || s[0]?.id || "");
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  async function exportCsv() {
    try {
      setError(null);
      const reportMap: Record<Tab, string> = {
        dashboard: "dashboard", value: "stock-value", movement: "movement",
        card: "sku-card", minmax: "min-max", stocktake: "stocktake", expiry: "near-expiry",
      };
      const r = range();
      await downloadInvReportCsv({
        report: reportMap[tab],
        ...whParam,
        ...(tab === "card" && skuId ? { skuId } : {}),
        ...(tab === "movement" || tab === "card" ? { from: r.from, to: r.to } : {}),
        ...(tab === "expiry" ? { withinDays: Number(withinDays) || 30 } : {}),
      });
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo cáo kho.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Báo cáo kho</h1>
          <p className="text-sm text-[var(--muted)]">UC_INV_044 · 048 · 055 · 060 · 063–065 · 067 · 069–070 · HSD · min/max · CSV.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["dashboard", "Dashboard"],
            ["value", "Giá trị tồn"],
            ["movement", "XNT kỳ"],
            ["card", "Thẻ kho"],
            ["minmax", "Min / Max"],
            ["stocktake", "Kiểm kê"],
            ["expiry", "Cận / quá HSD"],
          ] as [Tab, string][]).map(([k, label]) => (
            <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
              {label}
            </button>
          ))}
          <button type="button" className={btn.soft} onClick={() => void exportCsv()}>Xuất CSV</button>
        </div>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}

      <div className={`${panel} flex flex-wrap gap-3`}>
        <label className={field.label}>
          Kho
          <select className={field.input} value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)}>
            <option value="">Tất cả</option>
            {warehouses.map((w) => <option key={w.id} value={w.id}>{w.code} · {w.name}</option>)}
          </select>
        </label>
        {(tab === "movement" || tab === "card") && (
          <>
            <label className={field.label}>Từ<input className={field.input} type="date" value={from} onChange={(e) => setFrom(e.target.value)} /></label>
            <label className={field.label}>Đến<input className={field.input} type="date" value={to} onChange={(e) => setTo(e.target.value)} /></label>
          </>
        )}
        {tab === "card" && (
          <label className={field.label}>
            SKU
            <select className={field.input} value={skuId} onChange={(e) => setSkuId(e.target.value)}>
              {skus.map((s) => <option key={s.id} value={s.id}>{s.code} · {s.name}</option>)}
            </select>
          </label>
        )}
        <button type="button" className={btn.primary} disabled={loading} onClick={() => void load().catch((e: Error) => setError(e.message))}>
          {loading ? "Đang tải…" : "Làm mới"}
        </button>
      </div>

      {tab === "dashboard" && dashboard && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {[
            ["SKU active", String(dashboard.skuCount)],
            ["Kho active", String(dashboard.warehouseCount)],
            ["Tổng SL tồn", qty(dashboard.totalQtyOnHand)],
            ["Giá trị tồn", money(dashboard.totalStockValue)],
            ["Dưới min", String(dashboard.belowMinCount)],
            ["Trên max", String(dashboard.aboveMaxCount)],
            ["KK đang mở", String(dashboard.openStocktakeCount)],
            ["Cận HSD", String(dashboard.nearExpiryCount)],
            ["Quá HSD", String(dashboard.expiredCount)],
            ["ATP thiếu", String(dashboard.insufficientAtpCount)],
          ].map(([label, val]) => (
            <div key={label} className={panel}>
              <div className="text-xs text-[var(--muted)]">{label}</div>
              <div className="mt-1 text-lg font-semibold">{val}</div>
            </div>
          ))}
        </div>
      )}

      <div className={tableWrap}>
        {tab === "dashboard" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>Cảnh báo</th><th className={th}>SKU</th><th className={th}>Kho</th><th className={th}>Tồn</th><th className={th}>Min</th><th className={th}>Max</th></tr></thead>
            <tbody>
              {(dashboard?.topAlerts ?? []).map((r) => (
                <tr key={`${r.skuId}-${r.warehouseId}-${r.alertType}`}>
                  <td className={td}>{r.alertType}</td>
                  <td className={td}>{r.skuCode} · {r.skuName}</td>
                  <td className={td}>{r.warehouseName}</td>
                  <td className={td}>{qty(r.qtyOnHand)}</td>
                  <td className={td}>{r.minQty ?? "—"}</td>
                  <td className={td}>{r.maxQty ?? "—"}</td>
                </tr>
              ))}
              {!loading && (dashboard?.topAlerts?.length ?? 0) === 0 && <tr><td className={td} colSpan={6}>Không có cảnh báo min/max.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "value" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>SKU</th><th className={th}>Kho</th><th className={th}>Tồn</th><th className={th}>ĐG</th><th className={th}>Giá trị</th></tr></thead>
            <tbody>
              {valueRows.map((r) => (
                <tr key={`${r.skuId}-${r.warehouseId}`}>
                  <td className={td}>{r.skuCode} · {r.skuName}</td>
                  <td className={td}>{r.warehouseName}</td>
                  <td className={td}>{qty(r.qtyOnHand)}</td>
                  <td className={td}>{money(r.standardCost)}</td>
                  <td className={td}>{money(r.stockValue)}</td>
                </tr>
              ))}
              {!loading && valueRows.length === 0 && <tr><td className={td} colSpan={5}>Không có tồn.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "movement" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>SKU</th><th className={th}>Nhập</th><th className={th}>Xuất</th><th className={th}>Net</th><th className={th}>GT nhập</th><th className={th}>GT xuất</th></tr></thead>
            <tbody>
              {moveRows.map((r) => (
                <tr key={r.skuId}>
                  <td className={td}>{r.skuCode} · {r.skuName}</td>
                  <td className={td}>{qty(r.qtyIn)}</td>
                  <td className={td}>{qty(r.qtyOut)}</td>
                  <td className={td}>{qty(r.qtyNet)}</td>
                  <td className={td}>{money(r.valueIn)}</td>
                  <td className={td}>{money(r.valueOut)}</td>
                </tr>
              ))}
              {!loading && moveRows.length === 0 && <tr><td className={td} colSpan={6}>Không có phát sinh kỳ.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "card" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>Thời điểm</th><th className={th}>Phiếu</th><th className={th}>Loại</th><th className={th}>Kho</th><th className={th}>SL</th><th className={th}>ĐG</th><th className={th}>GT</th></tr></thead>
            <tbody>
              {cardRows.map((r, i) => (
                <tr key={`${r.docCode}-${i}`}>
                  <td className={td}>{r.at.slice(0, 16).replace("T", " ")}</td>
                  <td className={td}>{r.docCode}</td>
                  <td className={td}>{r.docType}/{r.sourceType}</td>
                  <td className={td}>{r.warehouseName}</td>
                  <td className={td}>{qty(r.qtySigned)}</td>
                  <td className={td}>{money(r.unitCost)}</td>
                  <td className={td}>{money(r.amount)}</td>
                </tr>
              ))}
              {!loading && cardRows.length === 0 && <tr><td className={td} colSpan={7}>{skuId ? "Không có lịch sử." : "Chọn SKU."}</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "minmax" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>Loại</th><th className={th}>SKU</th><th className={th}>Kho</th><th className={th}>Tồn</th><th className={th}>Min</th><th className={th}>Max</th></tr></thead>
            <tbody>
              {alertRows.map((r) => (
                <tr key={`${r.skuId}-${r.warehouseId}-${r.alertType}`}>
                  <td className={td}>{r.alertType}</td>
                  <td className={td}>{r.skuCode} · {r.skuName}</td>
                  <td className={td}>{r.warehouseName}</td>
                  <td className={td}>{qty(r.qtyOnHand)}</td>
                  <td className={td}>{r.minQty ?? "—"}</td>
                  <td className={td}>{r.maxQty ?? "—"}</td>
                </tr>
              ))}
              {!loading && alertRows.length === 0 && <tr><td className={td} colSpan={6}>Không có cảnh báo.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "stocktake" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>KK</th><th className={th}>Kho</th><th className={th}>TT</th><th className={th}>SKU</th><th className={th}>Hệ thống</th><th className={th}>Đếm</th><th className={th}>Lệch</th></tr></thead>
            <tbody>
              {stRows.map((r, i) => (
                <tr key={`${r.stocktakeId}-${r.skuCode}-${i}`}>
                  <td className={td}>{r.stocktakeCode}</td>
                  <td className={td}>{r.warehouseName}</td>
                  <td className={td}>{r.status}</td>
                  <td className={td}>{r.skuCode} · {r.skuName}</td>
                  <td className={td}>{qty(r.systemQty)}</td>
                  <td className={td}>{r.countedQty == null ? "—" : qty(r.countedQty)}</td>
                  <td className={td}>{qty(r.varianceQty)}</td>
                </tr>
              ))}
              {!loading && stRows.length === 0 && <tr><td className={td} colSpan={7}>Không có dòng kiểm kê.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "expiry" && (
          <>
            <div className="mb-2 flex items-center gap-2 text-sm">
              <label className="text-[var(--muted)]">Trong</label>
              <input className={`${field} w-20`} value={withinDays} onChange={(e) => setWithinDays(e.target.value)} />
              <span className="text-[var(--muted)]">ngày</span>
            </div>
            <table className="min-w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Loại</th><th className={th}>SKU</th><th className={th}>Lô</th>
                  <th className={th}>HSD</th><th className={th}>Ngày còn</th><th className={th}>Tồn</th><th className={th}>Avail</th>
                </tr>
              </thead>
              <tbody>
                {expiryRows.map((r) => (
                  <tr key={`${r.skuId}-${r.warehouseId}-${r.lotCode}-${r.expiryDate}`}>
                    <td className={td}>
                      <span className={statusPill(r.alertType === "Expired" ? "danger" : "warning")}>{r.alertType}</span>
                    </td>
                    <td className={td}>{r.skuCode} · {r.skuName}</td>
                    <td className={td}>{r.lotCode ?? "—"}</td>
                    <td className={td}>{r.expiryDate}</td>
                    <td className={td}>{r.daysToExpiry}</td>
                    <td className={td}>{qty(r.qtyOnHand)}</td>
                    <td className={td}>{qty(r.qtyAvailable)}</td>
                  </tr>
                ))}
                {!loading && expiryRows.length === 0 && <tr><td className={td} colSpan={7}>Không có lô cận/quá HSD.</td></tr>}
              </tbody>
            </table>
          </>
        )}
      </div>
    </div>
  );
}
