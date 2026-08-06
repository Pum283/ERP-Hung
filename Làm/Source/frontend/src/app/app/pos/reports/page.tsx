"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchPosStores, type PosStoreDto } from "@/shared/api/pos-api";
import {
  downloadPosReportCsv,
  fetchPosCancelDiscount,
  fetchPosChainLive,
  fetchPosCostVariance,
  fetchPosRevenueByCashier,
  fetchPosRevenueByProduct,
  fetchPosRevenueByTime,
  fetchPosStoreCompare,
  fetchPosTopProducts,
  type PosCancelDiscountReportDto,
  type PosChainLiveReportDto,
  type PosCostVarianceReportDto,
  type PosRevenueByCashierRowDto,
  type PosRevenueByProductRowDto,
  type PosRevenueByTimeRowDto,
  type PosStoreCompareRowDto,
  type PosTopProductRowDto,
} from "@/shared/api/pos-report-api";
import { paceStatus, varianceTone } from "@/shared/api/pos-report-helpers";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

function isoDay(d: Date) {
  return d.toISOString().slice(0, 10);
}

type Tab = "time" | "product" | "cashier" | "cancel" | "top" | "stores" | "cost" | "live";

export default function PosReportsPage() {
  const { can } = usePermissions();
  const canRead = can("pos.sale.read");

  const [tab, setTab] = useState<Tab>("time");
  const [stores, setStores] = useState<PosStoreDto[]>([]);
  const [storeId, setStoreId] = useState("");
  const [grain, setGrain] = useState("day");
  const [from, setFrom] = useState(() => {
    const d = new Date(); d.setDate(d.getDate() - 7); return isoDay(d);
  });
  const [to, setTo] = useState(() => isoDay(new Date()));
  const [topBy, setTopBy] = useState<"qty" | "revenue">("qty");
  const [timeRows, setTimeRows] = useState<PosRevenueByTimeRowDto[]>([]);
  const [productRows, setProductRows] = useState<PosRevenueByProductRowDto[]>([]);
  const [cashierRows, setCashierRows] = useState<PosRevenueByCashierRowDto[]>([]);
  const [cancel, setCancel] = useState<PosCancelDiscountReportDto | null>(null);
  const [topRows, setTopRows] = useState<PosTopProductRowDto[]>([]);
  const [storeRows, setStoreRows] = useState<PosStoreCompareRowDto[]>([]);
  const [cost, setCost] = useState<PosCostVarianceReportDto | null>(null);
  const [live, setLive] = useState<PosChainLiveReportDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const range = useCallback(() => ({
    from: new Date(from + "T00:00:00").toISOString(),
    to: new Date(to + "T23:59:59").toISOString(),
    ...(storeId ? { storeId } : {}),
  }), [from, to, storeId]);

  const load = useCallback(async () => {
    const r = range();
    if (tab === "time") setTimeRows(await fetchPosRevenueByTime({ ...r, grain }));
    else if (tab === "product") setProductRows(await fetchPosRevenueByProduct(r));
    else if (tab === "cashier") setCashierRows(await fetchPosRevenueByCashier(r));
    else if (tab === "top") setTopRows(await fetchPosTopProducts({ ...r, top: 20, by: topBy }));
    else if (tab === "stores") setStoreRows(await fetchPosStoreCompare({ from: r.from, to: r.to }));
    else if (tab === "cost") setCost(await fetchPosCostVariance(r));
    else if (tab === "live") setLive(await fetchPosChainLive());
    else setCancel(await fetchPosCancelDiscount(r));
  }, [tab, grain, topBy, range]);

  useEffect(() => {
    if (tab !== "live" || !canRead) return;
    const t = setInterval(() => { fetchPosChainLive().then(setLive).catch(() => {}); }, 30_000);
    return () => clearInterval(t);
  }, [tab, canRead]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    Promise.all([fetchPosStores().catch(() => [] as PosStoreDto[]), load()])
      .then(([s]) => setStores(s))
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  async function exportCsv() {
    try {
      setError(null);
      const r = range();
      const report = tab === "cancel" ? "cancel-discount"
        : tab === "top" ? "top-products"
        : tab === "stores" ? "stores"
        : tab === "cost" ? "cost-variance"
        : tab;
      await downloadPosReportCsv({ report, ...r, ...(tab === "time" ? { grain } : {}) });
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo cáo POS.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Báo cáo POS</h1>
          <p className="text-sm text-[var(--muted)]">UC_POS_061–069 · 072 · DT thời gian/SP/thu ngân · hủy/giảm · top SP · so sánh điểm bán · cost variance · chuỗi live vs target · CSV.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["time", "Theo thời gian"],
            ["product", "Theo SP"],
            ["cashier", "Theo thu ngân"],
            ["cancel", "Hủy / giảm giá"],
            ["top", "Top SP"],
            ["stores", "So sánh điểm bán"],
            ["cost", "Cost variance"],
            ["live", "Chuỗi live"],
          ] as [Tab, string][]).map(([k, label]) => (
            <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
              {label}
            </button>
          ))}
          {tab !== "live" && (
            <button type="button" className={btn.soft} onClick={() => void exportCsv()}>Xuất CSV</button>
          )}
        </div>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}

      <div className={`${panel} flex flex-wrap gap-3`}>
        <label className={field.label}>Từ<input className={field.input} type="date" value={from} onChange={(e) => setFrom(e.target.value)} /></label>
        <label className={field.label}>Đến<input className={field.input} type="date" value={to} onChange={(e) => setTo(e.target.value)} /></label>
        <label className={field.label}>
          Cửa hàng
          <select className={field.input} value={storeId} onChange={(e) => setStoreId(e.target.value)}>
            <option value="">Tất cả</option>
            {stores.map((s) => <option key={s.id} value={s.id}>{s.code} · {s.name}</option>)}
          </select>
        </label>
        {tab === "time" && (
          <label className={field.label}>
            Grain
            <select className={field.input} value={grain} onChange={(e) => setGrain(e.target.value)}>
              <option value="hour">Giờ</option>
              <option value="day">Ngày</option>
              <option value="shift">Ca</option>
            </select>
          </label>
        )}
        {tab === "top" && (
          <label className={field.label}>
            Xếp theo
            <select className={field.input} value={topBy} onChange={(e) => setTopBy(e.target.value as "qty" | "revenue")}>
              <option value="qty">Số lượng</option>
              <option value="revenue">Doanh thu</option>
            </select>
          </label>
        )}
      </div>

      {loading ? (
        <p className="text-sm text-[var(--muted)]">Đang tải…</p>
      ) : tab === "time" ? (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Bucket</th><th className={th}>SL đơn</th>
                <th className={th}>DT</th><th className={th}>Giảm</th>
              </tr>
            </thead>
            <tbody>
              {timeRows.length === 0 ? (
                <tr><td className={td} colSpan={4}>Không có dữ liệu.</td></tr>
              ) : timeRows.map((r) => (
                <tr key={r.bucket + (r.shiftId ?? "")}>
                  <td className={td}>{r.bucket}</td>
                  <td className={td}>{r.saleCount}</td>
                  <td className={td}>{money(r.revenue)}</td>
                  <td className={td}>{money(r.discount)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : tab === "product" ? (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>SP</th><th className={th}>SL</th>
                <th className={th}>DT</th><th className={th}>Dòng</th>
              </tr>
            </thead>
            <tbody>
              {productRows.length === 0 ? (
                <tr><td className={td} colSpan={4}>Không có dữ liệu.</td></tr>
              ) : productRows.map((r) => (
                <tr key={r.productCode}>
                  <td className={td}>{r.productCode} · {r.productName}</td>
                  <td className={td}>{r.qty}</td>
                  <td className={td}>{money(r.revenue)}</td>
                  <td className={td}>{r.lineCount}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : tab === "cashier" ? (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Thu ngân</th><th className={th}>SL đơn</th>
                <th className={th}>DT</th><th className={th}>Giảm</th>
              </tr>
            </thead>
            <tbody>
              {cashierRows.length === 0 ? (
                <tr><td className={td} colSpan={4}>Không có dữ liệu.</td></tr>
              ) : cashierRows.map((r) => (
                <tr key={r.cashierUserId}>
                  <td className={td}>{r.cashierName}</td>
                  <td className={td}>{r.saleCount}</td>
                  <td className={td}>{money(r.revenue)}</td>
                  <td className={td}>{money(r.discount)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : tab === "top" ? (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>#</th><th className={th}>SP</th>
                <th className={th}>SL</th><th className={th}>DT</th><th className={th}>Dòng</th>
              </tr>
            </thead>
            <tbody>
              {topRows.length === 0 ? (
                <tr><td className={td} colSpan={5}>Không có dữ liệu.</td></tr>
              ) : topRows.map((r) => (
                <tr key={r.productCode}>
                  <td className={td}>{r.rank}</td>
                  <td className={td}>{r.productCode} · {r.productName}</td>
                  <td className={td}>{r.qty}</td>
                  <td className={td}>{money(r.revenue)}</td>
                  <td className={td}>{r.lineCount}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : tab === "stores" ? (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Điểm bán</th><th className={th}>SL đơn</th>
                <th className={th}>DT</th><th className={th}>Giảm</th>
                <th className={th}>TB/đơn</th><th className={th}>Tỷ trọng</th>
              </tr>
            </thead>
            <tbody>
              {storeRows.length === 0 ? (
                <tr><td className={td} colSpan={6}>Không có dữ liệu.</td></tr>
              ) : storeRows.map((r) => (
                <tr key={r.storeId}>
                  <td className={td}>{r.storeCode} · {r.storeName}</td>
                  <td className={td}>{r.saleCount}</td>
                  <td className={td}>{money(r.revenue)}</td>
                  <td className={td}>{money(r.discount)}</td>
                  <td className={td}>{money(r.avgTicket)}</td>
                  <td className={td}>{r.revenueSharePercent}%</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : tab === "cost" ? (
        cost && (
          <div className="space-y-3">
            <div className={`${panel} grid gap-3 sm:grid-cols-2 lg:grid-cols-4`}>
              <Stat label="Cost lý thuyết" value={cost.totalTheoreticalCost} money />
              <Stat label="Cost thực tế" value={cost.totalActualCost} money />
              <Stat label="Chênh lệch" value={cost.totalVarianceCost} money hint={`${cost.totalVariancePercent}%`} />
              <Stat label="Số NVL" value={cost.rows.length} />
            </div>
            <div className={tableWrap}>
              <table className="min-w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>NVL</th><th className={th}>SL LT</th><th className={th}>SL TT</th>
                    <th className={th}>Cost LT</th><th className={th}>Cost TT</th><th className={th}>Chênh</th>
                  </tr>
                </thead>
                <tbody>
                  {cost.rows.length === 0 ? (
                    <tr><td className={td} colSpan={6}>Không có dữ liệu BOM / phiếu xuất trong kỳ.</td></tr>
                  ) : cost.rows.map((r) => (
                    <tr key={r.materialCode}>
                      <td className={td}>{r.materialCode} · {r.materialName}</td>
                      <td className={td}>{r.theoreticalQty}</td>
                      <td className={td}>{r.actualQty}</td>
                      <td className={td}>{money(r.theoreticalCost)}</td>
                      <td className={td}>{money(r.actualCost)}</td>
                      <td className={`${td} ${
                        varianceTone(r.variancePercent) === "danger" ? "text-red-600"
                          : varianceTone(r.variancePercent) === "success" ? "text-emerald-600"
                          : "text-[var(--muted)]"
                      }`}>
                        {money(r.varianceCost)} ({r.variancePercent}%)
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )
      ) : tab === "live" ? (
        live && (
          <div className="space-y-3">
            <div className={`${panel} grid gap-3 sm:grid-cols-2 lg:grid-cols-4`}>
              <Stat label="DT hôm nay" value={live.totalTodayRevenue} money />
              <Stat label="DT tháng" value={live.totalMonthRevenue} money hint={live.totalTarget > 0 ? `${live.totalAttainmentPercent}% target` : "chưa đặt target"} />
              <Stat label="Ca đang mở" value={live.openShiftCount} />
              <Stat label="Điểm bán" value={live.storeCount} hint={`cập nhật ${new Date(live.asOf).toLocaleTimeString("vi-VN")} · 30s/lần`} />
            </div>
            <div className={tableWrap}>
              <table className="min-w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Điểm bán</th><th className={th}>Ca mở</th>
                    <th className={th}>Đơn hôm nay</th><th className={th}>DT hôm nay</th>
                    <th className={th}>DT tháng</th><th className={th}>Target</th><th className={th}>Đạt / nhịp</th>
                  </tr>
                </thead>
                <tbody>
                  {live.rows.length === 0 ? (
                    <tr><td className={td} colSpan={7}>Chưa có điểm bán Active.</td></tr>
                  ) : live.rows.map((r) => {
                    const pace = paceStatus(r.targetAttainmentPercent, r.monthElapsedPercent, r.monthlyTarget > 0);
                    return (
                      <tr key={r.storeId}>
                        <td className={td}>{r.storeCode} · {r.storeName}</td>
                        <td className={td}>{r.openShiftCount}</td>
                        <td className={td}>{r.todaySaleCount}</td>
                        <td className={td}>{money(r.todayRevenue)}</td>
                        <td className={td}>{money(r.monthRevenue)}</td>
                        <td className={td}>{r.monthlyTarget > 0 ? money(r.monthlyTarget) : "—"}</td>
                        <td className={`${td} ${
                          pace === "ahead" ? "text-emerald-600"
                            : pace === "behind" ? "text-red-600"
                            : "text-[var(--muted)]"
                        }`}>
                          {pace === "none" ? "chưa đặt target"
                            : `${r.targetAttainmentPercent}% / ${r.monthElapsedPercent}% tháng`}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        )
      ) : cancel ? (
        <div className={`${panel} grid gap-3 sm:grid-cols-2 lg:grid-cols-4`}>
          <Stat label="Tổng đơn" value={cancel.totalSales} />
          <Stat label="Đã TT" value={cancel.paidSales} />
          <Stat label="Hủy" value={cancel.cancelledSales} hint={`${cancel.cancelRatePercent}%`} />
          <Stat label="Có giảm giá" value={cancel.discountedSales} hint={`${cancel.discountRatePercent}%`} />
          <Stat label="Doanh thu" value={cancel.totalRevenue} money />
          <Stat label="Tổng giảm" value={cancel.totalDiscount} money />
        </div>
      ) : null}
    </div>
  );
}

function Stat({ label, value, hint, money: asMoney }: {
  label: string; value: number; hint?: string; money?: boolean;
}) {
  return (
    <div>
      <div className="text-xs uppercase tracking-wide text-[var(--muted)]">{label}</div>
      <div className="text-lg font-semibold">{asMoney ? money(value) : value}</div>
      {hint && <div className="text-xs text-[var(--muted)]">{hint}</div>}
    </div>
  );
}
