"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchPosStores, type PosStoreDto } from "@/shared/api/pos-api";
import {
  downloadPosReportCsv,
  fetchPosCancelDiscount,
  fetchPosRevenueByCashier,
  fetchPosRevenueByProduct,
  fetchPosRevenueByTime,
  type PosCancelDiscountReportDto,
  type PosRevenueByCashierRowDto,
  type PosRevenueByProductRowDto,
  type PosRevenueByTimeRowDto,
} from "@/shared/api/pos-report-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

function isoDay(d: Date) {
  return d.toISOString().slice(0, 10);
}

type Tab = "time" | "product" | "cashier" | "cancel";

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
  const [timeRows, setTimeRows] = useState<PosRevenueByTimeRowDto[]>([]);
  const [productRows, setProductRows] = useState<PosRevenueByProductRowDto[]>([]);
  const [cashierRows, setCashierRows] = useState<PosRevenueByCashierRowDto[]>([]);
  const [cancel, setCancel] = useState<PosCancelDiscountReportDto | null>(null);
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
    else setCancel(await fetchPosCancelDiscount(r));
  }, [tab, grain, range]);

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
      const report = tab === "cancel" ? "cancel-discount" : tab;
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
          <p className="text-sm text-[var(--muted)]">UC_POS_061–064 · 068 · DT theo giờ/ngày/ca · SP · thu ngân · hủy/giảm · CSV.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["time", "Theo thời gian"],
            ["product", "Theo SP"],
            ["cashier", "Theo thu ngân"],
            ["cancel", "Hủy / giảm giá"],
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
