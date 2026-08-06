"use client";

import { useCallback, useEffect, useState } from "react";
import {
  downloadAstReportCsv,
  fetchAstByLocation,
  fetchAstDepreciationReport,
  fetchAstLocations,
  fetchAstRegister,
  type AstByLocationRowDto,
  type AstDepreciationReportDto,
  type AstLocationDto,
  type AstRegisterRowDto,
} from "@/shared/api/ast-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

type Tab = "register" | "depreciation" | "location";

export default function AstReportsPage() {
  const { can } = usePermissions();
  const canRead = can("ast.asset.read");

  const [tab, setTab] = useState<Tab>("register");
  const [register, setRegister] = useState<AstRegisterRowDto[]>([]);
  const [byLoc, setByLoc] = useState<AstByLocationRowDto[]>([]);
  const [dep, setDep] = useState<AstDepreciationReportDto | null>(null);
  const [locations, setLocations] = useState<AstLocationDto[]>([]);
  const [status, setStatus] = useState("");
  const [locationId, setLocationId] = useState("");
  const [year, setYear] = useState(String(new Date().getFullYear()));
  const [month, setMonth] = useState(String(new Date().getMonth() + 1));
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    const locs = await fetchAstLocations().catch(() => [] as AstLocationDto[]);
    setLocations(locs);
    if (tab === "register") {
      setRegister(await fetchAstRegister({
        ...(status ? { status } : {}),
        ...(locationId ? { locationId } : {}),
      }));
    } else if (tab === "location") {
      setByLoc(await fetchAstByLocation(locationId ? { locationId } : undefined));
    } else {
      setDep(await fetchAstDepreciationReport(Number(year), Number(month)));
    }
  }, [tab, status, locationId, year, month]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  async function exportCsv() {
    try {
      setError(null);
      if (tab === "register") {
        await downloadAstReportCsv({
          report: "register",
          ...(status ? { status } : {}),
          ...(locationId ? { locationId } : {}),
        });
      } else if (tab === "location") {
        await downloadAstReportCsv({
          report: "by-location",
          ...(locationId ? { locationId } : {}),
        });
      } else {
        await downloadAstReportCsv({
          report: "depreciation",
          year: Number(year),
          month: Number(month),
        });
      }
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo cáo TSCĐ.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Báo cáo TSCĐ</h1>
          <p className="text-sm text-[var(--muted)]">UC_AST_030–032 · 034 · sổ · KH kỳ · theo vị trí · xuất CSV.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["register", "Sổ TSCĐ"],
            ["depreciation", "KH theo kỳ"],
            ["location", "Theo vị trí"],
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
        {tab === "register" && (
          <label className={field.label}>
            Trạng thái
            <select className={field.input} value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">Tất cả</option>
              <option value="Active">Active</option>
              <option value="Draft">Draft</option>
              <option value="Disposed">Disposed</option>
            </select>
          </label>
        )}
        {(tab === "register" || tab === "location") && (
          <label className={field.label}>
            Vị trí
            <select className={field.input} value={locationId} onChange={(e) => setLocationId(e.target.value)}>
              <option value="">Tất cả</option>
              {locations.map((l) => <option key={l.id} value={l.id}>{l.code} · {l.name}</option>)}
            </select>
          </label>
        )}
        {tab === "depreciation" && (
          <>
            <label className={field.label}>
              Năm
              <input className={field.input} value={year} onChange={(e) => setYear(e.target.value)} />
            </label>
            <label className={field.label}>
              Tháng
              <input className={field.input} value={month} onChange={(e) => setMonth(e.target.value)} />
            </label>
          </>
        )}
      </div>

      {loading ? (
        <p className="text-sm text-[var(--muted)]">Đang tải…</p>
      ) : tab === "register" ? (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Mã</th>
                <th className={th}>Tên</th>
                <th className={th}>Nhóm</th>
                <th className={th}>Vị trí</th>
                <th className={th}>Nguyên giá</th>
                <th className={th}>KH LK</th>
                <th className={th}>GTCL</th>
                <th className={th}>TT</th>
              </tr>
            </thead>
            <tbody>
              {register.length === 0 ? (
                <tr><td className={td} colSpan={8}>Không có dữ liệu.</td></tr>
              ) : register.map((r) => (
                <tr key={r.id}>
                  <td className={td}>{r.code}</td>
                  <td className={td}>{r.name}</td>
                  <td className={td}>{r.groupName ?? "—"}</td>
                  <td className={td}>{r.locationName ?? "—"}</td>
                  <td className={td}>{money(r.originalCost)}</td>
                  <td className={td}>{money(r.accumulatedDepreciation)}</td>
                  <td className={td}>{money(r.bookValue)}</td>
                  <td className={td}>{r.status}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : tab === "location" ? (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Vị trí</th>
                <th className={th}>SL TS</th>
                <th className={th}>Nguyên giá</th>
                <th className={th}>KH LK</th>
                <th className={th}>GTCL</th>
              </tr>
            </thead>
            <tbody>
              {byLoc.length === 0 ? (
                <tr><td className={td} colSpan={5}>Không có dữ liệu.</td></tr>
              ) : byLoc.map((r) => (
                <tr key={r.locationId ?? r.locationName}>
                  <td className={td}>{r.locationName}</td>
                  <td className={td}>{r.assetCount}</td>
                  <td className={td}>{money(r.originalCost)}</td>
                  <td className={td}>{money(r.accumulatedDepreciation)}</td>
                  <td className={td}>{money(r.bookValue)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="space-y-3">
          <div className={panel}>
            <div className="text-sm text-[var(--muted)]">
              Run {dep?.runCode ?? "—"} · {dep?.status ?? "chưa tính"} · tổng {money(dep?.totalAmount ?? 0)} · {dep?.lineCount ?? 0} dòng
            </div>
          </div>
          <div className={tableWrap}>
            <table className="min-w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>#</th>
                  <th className={th}>TS</th>
                  <th className={th}>Số tiền KH</th>
                  <th className={th}>GTCL trước</th>
                  <th className={th}>GTCL sau</th>
                </tr>
              </thead>
              <tbody>
                {!dep?.lines?.length ? (
                  <tr><td className={td} colSpan={5}>Chưa có run KH kỳ này — tính ở Thẻ TS / khấu hao.</td></tr>
                ) : dep.lines.map((l) => (
                  <tr key={l.id}>
                    <td className={td}>{l.lineNo}</td>
                    <td className={td}>{l.assetCode} · {l.assetName}</td>
                    <td className={td}>{money(l.amount)}</td>
                    <td className={td}>{money(l.bookValueBefore)}</td>
                    <td className={td}>{money(l.bookValueAfter)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
