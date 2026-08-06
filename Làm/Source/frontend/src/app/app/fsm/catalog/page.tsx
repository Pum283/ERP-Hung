"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchFsmFaultCodes,
  fetchFsmParts,
  fetchFsmServiceTypes,
  fetchFsmSlaPolicies,
  upsertFsmFaultCode,
  upsertFsmPart,
  upsertFsmServiceType,
  upsertFsmSlaPolicy,
  type FsmFaultCodeDto,
  type FsmPartDto,
  type FsmServiceTypeDto,
  type FsmSlaPolicyDto,
} from "@/shared/api/fsm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function FsmCatalogPage() {
  const { can } = usePermissions();
  const canRead = can("fsm.master.read");
  const canManage = can("fsm.master.manage");

  const [types, setTypes] = useState<FsmServiceTypeDto[]>([]);
  const [faults, setFaults] = useState<FsmFaultCodeDto[]>([]);
  const [parts, setParts] = useState<FsmPartDto[]>([]);
  const [slas, setSlas] = useState<FsmSlaPolicyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [stCode, setStCode] = useState("SV-INSTALL");
  const [stName, setStName] = useState("");
  const [fcCode, setFcCode] = useState("E001");
  const [fcName, setFcName] = useState("");
  const [fcSev, setFcSev] = useState("Medium");
  const [pCode, setPCode] = useState("LK-01");
  const [pName, setPName] = useState("");
  const [slaCode, setSlaCode] = useState("SLA-NORMAL");
  const [slaName, setSlaName] = useState("SLA thường");
  const [slaPri, setSlaPri] = useState("Normal");
  const [respH, setRespH] = useState("8");
  const [resH, setResH] = useState("48");

  const load = useCallback(async () => {
    const [t, f, p, s] = await Promise.all([
      fetchFsmServiceTypes(), fetchFsmFaultCodes(), fetchFsmParts(), fetchFsmSlaPolicies(),
    ]);
    setTypes(t); setFaults(f); setParts(p); setSlas(s);
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem danh mục FSM.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Danh mục FSM</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Loại DV · mã lỗi · linh kiện · SLA theo ưu tiên (UC_FSM_001–003, 005)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Loại dịch vụ</h2>
          {canManage && (
            <form
              onSubmit={async (e: FormEvent) => {
                e.preventDefault();
                try {
                  await upsertFsmServiceType({ code: stCode, name: stName });
                  setStName(""); await load(); flash("Đã lưu loại DV.");
                } catch (err) { setError((err as Error).message); }
              }}
              className="mb-3 grid gap-2 sm:grid-cols-3"
            >
              <input className={field} value={stCode} onChange={(e) => setStCode(e.target.value)} required />
              <input className={field} value={stName} onChange={(e) => setStName(e.target.value)} placeholder="Tên" required />
              <button type="submit" className={btn.ghost}>Lưu</button>
            </form>
          )}
          <ul className="space-y-1 text-sm">
            {types.map((t) => <li key={t.id}>{t.code} · {t.name}</li>)}
          </ul>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Mã lỗi</h2>
          {canManage && (
            <form
              onSubmit={async (e: FormEvent) => {
                e.preventDefault();
                try {
                  await upsertFsmFaultCode({ code: fcCode, name: fcName, severity: fcSev });
                  setFcName(""); await load(); flash("Đã lưu mã lỗi.");
                } catch (err) { setError((err as Error).message); }
              }}
              className="mb-3 grid gap-2 sm:grid-cols-4"
            >
              <input className={field} value={fcCode} onChange={(e) => setFcCode(e.target.value)} required />
              <input className={field} value={fcName} onChange={(e) => setFcName(e.target.value)} placeholder="Tên" required />
              <select className={field} value={fcSev} onChange={(e) => setFcSev(e.target.value)}>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
              <button type="submit" className={btn.ghost}>Lưu</button>
            </form>
          )}
          <ul className="space-y-1 text-sm">
            {faults.map((f) => <li key={f.id}>{f.code} · {f.name} ({f.severity})</li>)}
          </ul>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Linh kiện</h2>
          {canManage && (
            <form
              onSubmit={async (e: FormEvent) => {
                e.preventDefault();
                try {
                  await upsertFsmPart({ code: pCode, name: pName });
                  setPName(""); await load(); flash("Đã lưu linh kiện.");
                } catch (err) { setError((err as Error).message); }
              }}
              className="mb-3 grid gap-2 sm:grid-cols-3"
            >
              <input className={field} value={pCode} onChange={(e) => setPCode(e.target.value)} required />
              <input className={field} value={pName} onChange={(e) => setPName(e.target.value)} placeholder="Tên" required />
              <button type="submit" className={btn.ghost}>Lưu</button>
            </form>
          )}
          <ul className="space-y-1 text-sm">
            {parts.map((p) => <li key={p.id}>{p.code} · {p.name}</li>)}
          </ul>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">SLA theo ưu tiên</h2>
          {canManage && (
            <form
              onSubmit={async (e: FormEvent) => {
                e.preventDefault();
                try {
                  await upsertFsmSlaPolicy({
                    code: slaCode, name: slaName, priority: slaPri,
                    responseHours: Number(respH) || 8, resolveHours: Number(resH) || 48,
                  });
                  await load(); flash("Đã lưu SLA.");
                } catch (err) { setError((err as Error).message); }
              }}
              className="mb-3 grid gap-2 sm:grid-cols-3"
            >
              <input className={field} value={slaCode} onChange={(e) => setSlaCode(e.target.value)} required />
              <input className={field} value={slaName} onChange={(e) => setSlaName(e.target.value)} required />
              <select className={field} value={slaPri} onChange={(e) => setSlaPri(e.target.value)}>
                <option value="Low">Low</option>
                <option value="Normal">Normal</option>
                <option value="High">High</option>
                <option value="Critical">Critical</option>
              </select>
              <input className={field} value={respH} onChange={(e) => setRespH(e.target.value)} placeholder="Giờ PH" />
              <input className={field} value={resH} onChange={(e) => setResH(e.target.value)} placeholder="Giờ XL" />
              <button type="submit" className={btn.primary}>Lưu SLA</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Ưu tiên</th>
                  <th className={th}>PH / XL</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {slas.map((s) => (
                  <tr key={s.id}>
                    <td className={td}>{s.code}</td>
                    <td className={td}>{s.priority}</td>
                    <td className={td}>{s.responseHours}h / {s.resolveHours}h</td>
                    <td className={td}>
                      <span className={statusPill(s.isActive ? "success" : "muted")}>
                        {s.isActive ? "Active" : "Off"}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </div>
  );
}
