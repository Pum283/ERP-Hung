"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPrtEnabledFeatures,
  fetchPrtPackages,
  upsertPrtPackage,
  type PrtEnabledFeaturesDto,
  type PrtPortalPackageDto,
} from "@/shared/api/prt-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

const FEATURE_KEYS = ["orders", "ar", "tickets", "vendor", "docs"] as const;

export default function PrtPackagePage() {
  const { can } = usePermissions();
  const canRead = can("prt.portal.read");
  const canManage = can("prt.portal.manage");

  const [packages, setPackages] = useState<PrtPortalPackageDto[]>([]);
  const [enabled, setEnabled] = useState<PrtEnabledFeaturesDto | null>(null);
  const [edit, setEdit] = useState<PrtPortalPackageDto | null>(null);
  const [flags, setFlags] = useState<Record<string, boolean>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [p, e] = await Promise.all([fetchPrtPackages(), fetchPrtEnabledFeatures()]);
    setPackages(p);
    setEnabled(e);
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((err: Error) => setError(err.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  function openEdit(p: PrtPortalPackageDto) {
    setEdit(p);
    setFlags({ ...p.features });
  }

  async function onSave(e: FormEvent) {
    e.preventDefault();
    if (!edit) return;
    try {
      await upsertPrtPackage({
        id: edit.id, planCode: edit.planCode, name: edit.name,
        features: flags, isActive: edit.isActive, note: edit.note,
      });
      await load();
      setOk("Đã lưu gói"); setError(null);
      setTimeout(() => setOk(null), 2500);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem cấu hình portal.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Gói module portal</h1>
        <p className="text-sm text-[var(--muted)]">UC_PRT_037 · bật/tắt tính năng theo PlanCode (Starter / Standard / Enterprise).</p>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}

      {enabled && (
        <div className={panel}>
          <div className="text-sm font-semibold">Gói đang áp dụng: {enabled.planCode}</div>
          <div className="mt-1 text-sm text-[var(--muted)]">
            Bật: {enabled.enabledFeatures.length ? enabled.enabledFeatures.join(", ") : "—"}
          </div>
        </div>
      )}

      {loading ? (
        <p className="text-sm text-[var(--muted)]">Đang tải…</p>
      ) : (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Plan</th>
                <th className={th}>Tên</th>
                <th className={th}>Features</th>
                <th className={th}>TT</th>
                <th className={th} />
              </tr>
            </thead>
            <tbody>
              {packages.map((p) => (
                <tr key={p.id}>
                  <td className={td}>{p.planCode}</td>
                  <td className={td}>{p.name}</td>
                  <td className={td}>
                    {Object.entries(p.features).filter(([, v]) => v).map(([k]) => k).join(", ") || "—"}
                  </td>
                  <td className={td}>
                    <span className={statusPill(p.isActive ? "success" : "muted")}>
                      {p.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  <td className={td}>
                    {canManage && (
                      <button type="button" className={btn.ghost} onClick={() => openEdit(p)}>Sửa</button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {edit && canManage && (
        <form className={`${panel} space-y-3`} onSubmit={onSave}>
          <div className="text-sm font-semibold">Sửa {edit.planCode}</div>
          <div className="grid gap-2 sm:grid-cols-2 md:grid-cols-3">
            {FEATURE_KEYS.map((k) => (
              <label key={k} className="flex items-center gap-2 text-sm">
                <input type="checkbox" className={field.check} checked={!!flags[k]}
                  onChange={(e) => setFlags((f) => ({ ...f, [k]: e.target.checked }))} />
                {k}
              </label>
            ))}
          </div>
          <div className="flex gap-2">
            <button type="submit" className={btn.primary}>Lưu</button>
            <button type="button" className={btn.ghost} onClick={() => setEdit(null)}>Đóng</button>
          </div>
        </form>
      )}
    </div>
  );
}
