"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchAstAssets,
  fetchAstLocations,
  fetchAstMovements,
  postAstMovement,
  upsertAstMovement,
  voidAstMovement,
  type AstAssetDto,
  type AstLocationDto,
  type AstMovementDocDto,
} from "@/shared/api/ast-api";
import { fetchEmployees, type EmployeeDto } from "@/shared/api/hrm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

type DocType = "Transfer" | "Handover" | "Disposal";

export default function AstMovementsPage() {
  const { can } = usePermissions();
  const canRead = can("ast.asset.read");
  const canManage = can("ast.asset.manage");

  const [docs, setDocs] = useState<AstMovementDocDto[]>([]);
  const [assets, setAssets] = useState<AstAssetDto[]>([]);
  const [locations, setLocations] = useState<AstLocationDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [filterType, setFilterType] = useState<DocType | "">("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [docType, setDocType] = useState<DocType>("Transfer");
  const [assetId, setAssetId] = useState("");
  const [toLocationId, setToLocationId] = useState("");
  const [toEmployeeId, setToEmployeeId] = useState("");
  const [disposalKind, setDisposalKind] = useState("Scrap");
  const [disposalAmount, setDisposalAmount] = useState("0");
  const [note, setNote] = useState("");

  const load = useCallback(async () => {
    const [d, a, l, e] = await Promise.all([
      fetchAstMovements(filterType ? { docType: filterType } : undefined),
      fetchAstAssets(),
      fetchAstLocations(),
      fetchEmployees().catch(() => [] as EmployeeDto[]),
    ]);
    setDocs(d);
    setAssets(a.filter((x) => x.status !== "Disposed" || filterType === "Disposal"));
    setLocations(l.filter((x) => x.status === "Active"));
    setEmployees(e.filter((x) => x.status === "Active"));
    if (!assetId && a[0]) setAssetId(a.find((x) => x.status === "Active")?.id ?? a[0].id);
  }, [filterType, assetId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((err: Error) => setError(err.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  function onCreate(e: FormEvent) {
    e.preventDefault();
    if (!assetId) { setError("Chọn tài sản."); return; }
    void run(async () => {
      await upsertAstMovement({
        docType,
        assetId,
        toLocationId: docType === "Transfer" || docType === "Handover" ? toLocationId || null : null,
        toEmployeeId: docType === "Handover" ? toEmployeeId || null : null,
        disposalKind: docType === "Disposal" ? disposalKind : null,
        disposalAmount: docType === "Disposal" ? Number(disposalAmount) || 0 : null,
        note: note || null,
      });
    }, "Đã tạo Draft");
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem chứng từ tài sản.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Điều chuyển · bàn giao · thanh lý</h1>
        <p className="text-sm text-[var(--muted)]">UC_AST_016–018 · Draft → ghi sổ cập nhật thẻ TS.</p>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}

      {canManage && (
        <form className={`${panel} grid gap-3 md:grid-cols-2 lg:grid-cols-3`} onSubmit={onCreate}>
          <label className={field.label}>
            Loại
            <select className={field.input} value={docType} onChange={(e) => setDocType(e.target.value as DocType)}>
              <option value="Transfer">Điều chuyển nội bộ</option>
              <option value="Handover">Bàn giao NV</option>
              <option value="Disposal">Thanh lý / nhượng bán</option>
            </select>
          </label>
          <label className={field.label}>
            Tài sản
            <select className={field.input} value={assetId} onChange={(e) => setAssetId(e.target.value)}>
              {assets.map((a) => (
                <option key={a.id} value={a.id}>{a.code} · {a.name} ({a.status})</option>
              ))}
            </select>
          </label>
          {(docType === "Transfer" || docType === "Handover") && (
            <label className={field.label}>
              Vị trí đích {docType === "Handover" ? "(tuỳ chọn)" : ""}
              <select className={field.input} value={toLocationId} onChange={(e) => setToLocationId(e.target.value)} required={docType === "Transfer"}>
                <option value="">— Chọn —</option>
                {locations.map((l) => <option key={l.id} value={l.id}>{l.code} · {l.name}</option>)}
              </select>
            </label>
          )}
          {docType === "Handover" && (
            <label className={field.label}>
              NV nhận
              <select className={field.input} value={toEmployeeId} onChange={(e) => setToEmployeeId(e.target.value)} required>
                <option value="">— Chọn —</option>
                {employees.map((emp) => (
                  <option key={emp.id} value={emp.id}>{emp.employeeCode} · {emp.fullName}</option>
                ))}
              </select>
            </label>
          )}
          {docType === "Disposal" && (
            <>
              <label className={field.label}>
                Hình thức
                <select className={field.input} value={disposalKind} onChange={(e) => setDisposalKind(e.target.value)}>
                  <option value="Scrap">Thanh lý</option>
                  <option value="Sale">Nhượng bán</option>
                </select>
              </label>
              <label className={field.label}>
                Số tiền
                <input className={field.input} value={disposalAmount} onChange={(e) => setDisposalAmount(e.target.value)} />
              </label>
            </>
          )}
          <label className={`${field.label} md:col-span-2`}>
            Ghi chú
            <input className={field.input} value={note} onChange={(e) => setNote(e.target.value)} />
          </label>
          <div className="flex items-end">
            <button type="submit" className={btn.primary}>Tạo Draft</button>
          </div>
        </form>
      )}

      <div className="flex flex-wrap gap-2">
        {(["", "Transfer", "Handover", "Disposal"] as const).map((t) => (
          <button key={t || "all"} type="button" className={filterType === t ? btn.primary : btn.ghost}
            onClick={() => setFilterType(t)}>
            {t === "" ? "Tất cả" : t === "Transfer" ? "Điều chuyển" : t === "Handover" ? "Bàn giao" : "Thanh lý"}
          </button>
        ))}
      </div>

      {loading ? (
        <p className="text-sm text-[var(--muted)]">Đang tải…</p>
      ) : (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Mã</th>
                <th className={th}>Loại</th>
                <th className={th}>TS</th>
                <th className={th}>Chi tiết</th>
                <th className={th}>Ngày</th>
                <th className={th}>TT</th>
                <th className={th} />
              </tr>
            </thead>
            <tbody>
              {docs.length === 0 ? (
                <tr><td className={td} colSpan={7}>Chưa có chứng từ.</td></tr>
              ) : docs.map((d) => (
                <tr key={d.id}>
                  <td className={td}>{d.code}</td>
                  <td className={td}>{d.docType}</td>
                  <td className={td}>{d.assetCode} · {d.assetName}</td>
                  <td className={td}>
                    {d.docType === "Transfer" && <>{d.fromLocationName ?? "—"} → {d.toLocationName ?? "—"}</>}
                    {d.docType === "Handover" && <>{d.fromEmployeeName ?? "—"} → {d.toEmployeeName ?? "—"}</>}
                    {d.docType === "Disposal" && (
                      <>{d.disposalKind} · {money(d.disposalAmount ?? 0)} · GTCL {money(d.bookValueSnapshot ?? 0)}</>
                    )}
                  </td>
                  <td className={td}>{new Date(d.docDate).toLocaleDateString("vi-VN")}</td>
                  <td className={td}>
                    <span className={statusPill(d.status === "Posted" ? "success" : d.status === "Void" ? "danger" : "brand")}>
                      {d.status}
                    </span>
                  </td>
                  <td className={td}>
                    {canManage && d.status === "Draft" && (
                      <div className="flex gap-1">
                        <button type="button" className={btn.soft}
                          onClick={() => void run(() => postAstMovement(d.id), "Đã ghi sổ")}>Ghi sổ</button>
                        <button type="button" className={btn.ghost}
                          onClick={() => void run(() => voidAstMovement(d.id), "Đã hủy")}>Hủy</button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
