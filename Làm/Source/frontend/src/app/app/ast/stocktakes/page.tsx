"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  closeAstStocktake,
  countAstStocktakeLine,
  createAstStocktake,
  fetchAstLocations,
  fetchAstStocktakeDetail,
  fetchAstStocktakeVariances,
  fetchAstStocktakes,
  reviewAstStocktake,
  type AstLocationDto,
  type AstStocktakeDetailDto,
  type AstStocktakeDto,
  type AstStocktakeLineDto,
} from "@/shared/api/ast-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function AstStocktakesPage() {
  const { can } = usePermissions();
  const canRead = can("ast.asset.read");
  const canManage = can("ast.asset.manage");

  const [list, setList] = useState<AstStocktakeDto[]>([]);
  const [locations, setLocations] = useState<AstLocationDto[]>([]);
  const [detail, setDetail] = useState<AstStocktakeDetailDto | null>(null);
  const [variances, setVariances] = useState<AstStocktakeLineDto[]>([]);
  const [locationId, setLocationId] = useState("");
  const [note, setNote] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const loadList = useCallback(async () => {
    const [s, l] = await Promise.all([fetchAstStocktakes(), fetchAstLocations()]);
    setList(s);
    setLocations(l.filter((x) => x.status === "Active"));
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    loadList().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, loadList]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await loadList();
      if (detail) {
        const d = await fetchAstStocktakeDetail(detail.header.id);
        setDetail(d);
        setVariances(await fetchAstStocktakeVariances(detail.header.id));
      }
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  async function openDetail(id: string) {
    try {
      const d = await fetchAstStocktakeDetail(id);
      setDetail(d);
      setVariances(await fetchAstStocktakeVariances(id));
      setError(null);
    } catch (err) { setError((err as Error).message); }
  }

  function onCreate(e: FormEvent) {
    e.preventDefault();
    void run(() => createAstStocktake({ locationId: locationId || null, note: note || null }), "Đã tạo đợt KK");
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem kiểm kê TSCĐ.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Kiểm kê TSCĐ</h1>
        <p className="text-sm text-[var(--muted)]">UC_AST_021–022 · snapshot Active · đếm có/không · đối chiếu thiếu/thừa.</p>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}

      {canManage && (
        <form className={`${panel} flex flex-wrap items-end gap-3`} onSubmit={onCreate}>
          <label className={field.label}>
            Phạm vi vị trí
            <select className={field.input} value={locationId} onChange={(e) => setLocationId(e.target.value)}>
              <option value="">Tất cả vị trí</option>
              {locations.map((l) => <option key={l.id} value={l.id}>{l.code} · {l.name}</option>)}
            </select>
          </label>
          <label className={field.label}>
            Ghi chú
            <input className={field.input} value={note} onChange={(e) => setNote(e.target.value)} />
          </label>
          <button type="submit" className={btn.primary}>Tạo đợt KK</button>
        </form>
      )}

      {loading ? (
        <p className="text-sm text-[var(--muted)]">Đang tải…</p>
      ) : (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Mã</th>
                <th className={th}>Phạm vi</th>
                <th className={th}>Đếm</th>
                <th className={th}>Lệch</th>
                <th className={th}>TT</th>
                <th className={th} />
              </tr>
            </thead>
            <tbody>
              {list.length === 0 ? (
                <tr><td className={td} colSpan={6}>Chưa có đợt kiểm kê.</td></tr>
              ) : list.map((s) => (
                <tr key={s.id}>
                  <td className={td}>{s.code}</td>
                  <td className={td}>{s.locationName ?? "—"}</td>
                  <td className={td}>{s.countedCount}/{s.lineCount}</td>
                  <td className={td}>{s.varianceCount}</td>
                  <td className={td}>
                    <span className={statusPill(s.status === "Closed" ? "muted" : s.status === "Reviewed" ? "success" : "brand")}>
                      {s.status}
                    </span>
                  </td>
                  <td className={td}>
                    <button type="button" className={btn.ghost} onClick={() => void openDetail(s.id)}>Chi tiết</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {detail && (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h2 className="text-lg font-semibold">{detail.header.code} · {detail.header.status}</h2>
            <div className="flex gap-2">
              {canManage && detail.header.status === "Counting" && (
                <button type="button" className={btn.soft}
                  onClick={() => void run(() => reviewAstStocktake(detail.header.id), "Đã đối chiếu")}>
                  Duyệt đối chiếu
                </button>
              )}
              {canManage && detail.header.status === "Reviewed" && (
                <button type="button" className={btn.primary}
                  onClick={() => void run(() => closeAstStocktake(detail.header.id), "Đã đóng")}>
                  Đóng đợt
                </button>
              )}
            </div>
          </div>

          {variances.length > 0 && (
            <div className={panel}>
              <div className="mb-2 text-sm font-semibold text-amber-800">Thiếu / thừa ({variances.length})</div>
              <ul className="space-y-1 text-sm">
                {variances.map((v) => (
                  <li key={v.id}>
                    {v.assetCode} · {v.assetName} — {v.variance < 0 ? "Thiếu" : "Thừa"} (Δ {v.variance})
                  </li>
                ))}
              </ul>
            </div>
          )}

          <div className={tableWrap}>
            <table className="min-w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã TS</th>
                  <th className={th}>Tên</th>
                  <th className={th}>Vị trí</th>
                  <th className={th}>Sổ</th>
                  <th className={th}>Đếm</th>
                  <th className={th}>Δ</th>
                  <th className={th} />
                </tr>
              </thead>
              <tbody>
                {detail.lines.map((line) => (
                  <tr key={line.id}>
                    <td className={td}>{line.assetCode}</td>
                    <td className={td}>{line.assetName}</td>
                    <td className={td}>{line.locationName ?? "—"}</td>
                    <td className={td}>{line.expectedPresent ? "Có" : "Không"}</td>
                    <td className={td}>
                      {line.countedPresent == null ? "—" : line.countedPresent ? "Có" : "Không"}
                    </td>
                    <td className={td}>{line.countedPresent == null ? "—" : line.variance}</td>
                    <td className={td}>
                      {canManage && detail.header.status === "Counting" && (
                        <div className="flex gap-1">
                          <button type="button" className={btn.soft}
                            onClick={() => void run(() => countAstStocktakeLine(detail.header.id, {
                              lineId: line.id, countedPresent: true,
                            }), "Đã đếm Có")}>Có</button>
                          <button type="button" className={btn.ghost}
                            onClick={() => void run(() => countAstStocktakeLine(detail.header.id, {
                              lineId: line.id, countedPresent: false,
                            }), "Đã đếm Không")}>Không</button>
                        </div>
                      )}
                    </td>
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
