"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchPurVendors, type PurVendorDto } from "@/shared/api/pur-api";
import {
  downloadPurReportCsv,
  fetchPurOpenPoAging,
  fetchPurOpenPrAging,
  fetchPurPurchaseByProduct,
  fetchPurPurchaseByVendor,
  type PurOpenPoAgingRowDto,
  type PurOpenPrAgingRowDto,
  type PurPurchaseByProductRowDto,
  type PurPurchaseByVendorRowDto,
} from "@/shared/api/pur-report-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}
function qty(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 2 });
}
function isoDay(d: Date) {
  return d.toISOString().slice(0, 10);
}

type Tab = "vendor" | "product" | "open-pr" | "open-po";

export default function PurReportsPage() {
  const { can } = usePermissions();
  const canRead = can("pur.grn.read") || can("pur.pr.read") || can("pur.po.read");

  const [tab, setTab] = useState<Tab>("vendor");
  const [vendors, setVendors] = useState<PurVendorDto[]>([]);
  const [vendorId, setVendorId] = useState("");
  const [from, setFrom] = useState(() => {
    const d = new Date(); d.setMonth(d.getMonth() - 1); return isoDay(d);
  });
  const [to, setTo] = useState(() => isoDay(new Date()));
  const [vendorRows, setVendorRows] = useState<PurPurchaseByVendorRowDto[]>([]);
  const [productRows, setProductRows] = useState<PurPurchaseByProductRowDto[]>([]);
  const [prRows, setPrRows] = useState<PurOpenPrAgingRowDto[]>([]);
  const [poRows, setPoRows] = useState<PurOpenPoAgingRowDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const range = useCallback(() => ({
    from: new Date(from + "T00:00:00").toISOString(),
    to: new Date(to + "T23:59:59").toISOString(),
    ...(vendorId ? { vendorId } : {}),
  }), [from, to, vendorId]);

  const load = useCallback(async () => {
    const r = range();
    if (tab === "vendor") setVendorRows(await fetchPurPurchaseByVendor(r));
    else if (tab === "product") setProductRows(await fetchPurPurchaseByProduct(r));
    else if (tab === "open-pr") setPrRows(await fetchPurOpenPrAging());
    else setPoRows(await fetchPurOpenPoAging(vendorId ? { vendorId } : undefined));
  }, [tab, range, vendorId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    Promise.all([fetchPurVendors().catch(() => [] as PurVendorDto[]), load()])
      .then(([v]) => setVendors(v))
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  async function exportCsv() {
    try {
      setError(null);
      const r = range();
      const report =
        tab === "vendor" ? "by-vendor"
          : tab === "product" ? "by-product"
            : tab === "open-pr" ? "open-pr" : "open-po";
      await downloadPurReportCsv({
        report,
        ...(tab === "vendor" || tab === "product" ? { from: r.from, to: r.to } : {}),
        ...(vendorId ? { vendorId } : {}),
      });
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo cáo mua hàng.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Báo cáo mua hàng</h1>
          <p className="text-sm text-[var(--muted)]">UC_PUR_048 · 051 · 052 · theo NCC/SP · Open PR/PO aging · CSV.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["vendor", "Theo NCC"],
            ["product", "Theo SP"],
            ["open-pr", "Open PR"],
            ["open-po", "Open PO"],
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
        {(tab === "vendor" || tab === "product") && (
          <>
            <label className={field.label}>Từ<input className={field.input} type="date" value={from} onChange={(e) => setFrom(e.target.value)} /></label>
            <label className={field.label}>Đến<input className={field.input} type="date" value={to} onChange={(e) => setTo(e.target.value)} /></label>
          </>
        )}
        {(tab === "vendor" || tab === "product" || tab === "open-po") && (
          <label className={field.label}>
            NCC
            <select className={field.input} value={vendorId} onChange={(e) => setVendorId(e.target.value)}>
              <option value="">Tất cả</option>
              {vendors.map((v) => <option key={v.id} value={v.id}>{v.code} · {v.name}</option>)}
            </select>
          </label>
        )}
        <button type="button" className={btn.primary} disabled={loading} onClick={() => void load().catch((e: Error) => setError(e.message))}>
          {loading ? "Đang tải…" : "Làm mới"}
        </button>
      </div>

      <div className={tableWrap}>
        {tab === "vendor" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>NCC</th><th className={th}>GRN</th><th className={th}>SL nhận</th><th className={th}>Giá trị</th></tr></thead>
            <tbody>
              {vendorRows.map((r) => (
                <tr key={r.vendorId}>
                  <td className={td}>{r.vendorCode} · {r.vendorName}</td>
                  <td className={td}>{r.grnCount}</td>
                  <td className={td}>{qty(r.acceptedQty)}</td>
                  <td className={td}>{money(r.amount)}</td>
                </tr>
              ))}
              {!loading && vendorRows.length === 0 && <tr><td className={td} colSpan={4}>Không có dữ liệu GRN Posted trong kỳ.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "product" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>SP</th><th className={th}>SL</th><th className={th}>Giá trị</th><th className={th}>Dòng</th></tr></thead>
            <tbody>
              {productRows.map((r) => (
                <tr key={r.productCode}>
                  <td className={td}>{r.productCode} · {r.productName}</td>
                  <td className={td}>{qty(r.acceptedQty)}</td>
                  <td className={td}>{money(r.amount)}</td>
                  <td className={td}>{r.lineCount}</td>
                </tr>
              ))}
              {!loading && productRows.length === 0 && <tr><td className={td} colSpan={4}>Không có dữ liệu.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "open-pr" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>PR</th><th className={th}>TT</th><th className={th}>Ngày</th><th className={th}>Tuổi (ngày)</th><th className={th}>Dòng</th><th className={th}>SL</th></tr></thead>
            <tbody>
              {prRows.map((r) => (
                <tr key={r.id}>
                  <td className={td}>{r.code}</td>
                  <td className={td}>{r.status}</td>
                  <td className={td}>{r.createdAt.slice(0, 10)}</td>
                  <td className={td}>{r.ageDays}</td>
                  <td className={td}>{r.lineCount}</td>
                  <td className={td}>{qty(r.totalQty)}</td>
                </tr>
              ))}
              {!loading && prRows.length === 0 && <tr><td className={td} colSpan={6}>Không có PR mở.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "open-po" && (
          <table className="min-w-full text-sm">
            <thead><tr><th className={th}>PO</th><th className={th}>NCC</th><th className={th}>TT</th><th className={th}>Tuổi</th><th className={th}>SL mở</th><th className={th}>GT mở</th></tr></thead>
            <tbody>
              {poRows.map((r) => (
                <tr key={r.id}>
                  <td className={td}>{r.code}</td>
                  <td className={td}>{r.vendorCode} · {r.vendorName}</td>
                  <td className={td}>{r.status}</td>
                  <td className={td}>{r.ageDays}</td>
                  <td className={td}>{qty(r.openQty)}</td>
                  <td className={td}>{money(r.openAmount)}</td>
                </tr>
              ))}
              {!loading && poRows.length === 0 && <tr><td className={td} colSpan={6}>Không có PO mở.</td></tr>}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
