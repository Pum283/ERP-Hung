"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPjmStatuses,
  fetchPjmTemplateDetail,
  fetchPjmTemplates,
  fetchPjmTypes,
  upsertPjmStatus,
  upsertPjmTemplate,
  upsertPjmTemplateItem,
  upsertPjmType,
  type PjmProjectStatusDto,
  type PjmProjectTypeDto,
  type PjmWbsTemplateDetailDto,
  type PjmWbsTemplateDto,
} from "@/shared/api/pjm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PjmCatalogPage() {
  const { can } = usePermissions();
  const canRead = can("pjm.master.read");
  const canManage = can("pjm.master.manage");

  const [types, setTypes] = useState<PjmProjectTypeDto[]>([]);
  const [statuses, setStatuses] = useState<PjmProjectStatusDto[]>([]);
  const [templates, setTemplates] = useState<PjmWbsTemplateDto[]>([]);
  const [selectedTplId, setSelectedTplId] = useState("");
  const [tplDetail, setTplDetail] = useState<PjmWbsTemplateDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [typeCode, setTypeCode] = useState("IMPL");
  const [typeName, setTypeName] = useState("");
  const [stCode, setStCode] = useState("Review");
  const [stName, setStName] = useState("");
  const [tplCode, setTplCode] = useState("WBS-STD");
  const [tplName, setTplName] = useState("");
  const [itemCode, setItemCode] = useState("1.0");
  const [itemName, setItemName] = useState("");

  const load = useCallback(async () => {
    const [t, s, tpl] = await Promise.all([fetchPjmTypes(), fetchPjmStatuses(), fetchPjmTemplates()]);
    setTypes(t); setStatuses(s); setTemplates(tpl);
    if (!selectedTplId && tpl[0]) setSelectedTplId(tpl[0].id);
  }, [selectedTplId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedTplId || !canRead) return;
    fetchPjmTemplateDetail(selectedTplId).then(setTplDetail).catch((e: Error) => setError(e.message));
  }, [selectedTplId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem danh mục dự án.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Danh mục dự án</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Loại DA · trạng thái chuẩn · mẫu WBS (UC_PJM_001, 002, 004)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-3">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Loại dự án</h2>
          {canManage && (
            <form
              onSubmit={async (e: FormEvent) => {
                e.preventDefault();
                try {
                  await upsertPjmType({ code: typeCode, name: typeName });
                  setTypeName(""); await load(); flash("Đã lưu loại DA.");
                } catch (err) { setError((err as Error).message); }
              }}
              className="mb-3 space-y-2"
            >
              <input className={field} value={typeCode} onChange={(e) => setTypeCode(e.target.value)} required />
              <input className={field} value={typeName} onChange={(e) => setTypeName(e.target.value)} placeholder="Tên" required />
              <button type="submit" className={btn.primary}>Lưu</button>
            </form>
          )}
          <ul className="space-y-1 text-sm">
            {types.map((t) => <li key={t.id}>{t.code} · {t.name}</li>)}
          </ul>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Trạng thái chuẩn</h2>
          {canManage && (
            <form
              onSubmit={async (e: FormEvent) => {
                e.preventDefault();
                try {
                  await upsertPjmStatus({ code: stCode, name: stName || stCode, sortOrder: 25 });
                  setStName(""); await load(); flash("Đã lưu trạng thái.");
                } catch (err) { setError((err as Error).message); }
              }}
              className="mb-3 space-y-2"
            >
              <input className={field} value={stCode} onChange={(e) => setStCode(e.target.value)} required />
              <input className={field} value={stName} onChange={(e) => setStName(e.target.value)} placeholder="Tên" />
              <button type="submit" className={btn.ghost}>Thêm TT</button>
            </form>
          )}
          <ul className="space-y-1 text-sm">
            {statuses.map((s) => (
              <li key={s.id}>
                {s.code} · {s.name}{" "}
                <span className={statusPill(s.isActive ? "success" : "muted")}>
                  {s.isTerminal ? "Terminal" : "Open"}
                </span>
              </li>
            ))}
          </ul>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Mẫu WBS</h2>
          {canManage && (
            <form
              onSubmit={async (e: FormEvent) => {
                e.preventDefault();
                try {
                  const saved = await upsertPjmTemplate({ code: tplCode, name: tplName });
                  setTplName(""); await load(); setSelectedTplId(saved.id); flash("Đã tạo mẫu.");
                } catch (err) { setError((err as Error).message); }
              }}
              className="mb-3 space-y-2"
            >
              <input className={field} value={tplCode} onChange={(e) => setTplCode(e.target.value)} required />
              <input className={field} value={tplName} onChange={(e) => setTplName(e.target.value)} placeholder="Tên mẫu" required />
              <button type="submit" className={btn.primary}>Tạo mẫu</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>HM</th>
                </tr>
              </thead>
              <tbody>
                {templates.map((t) => (
                  <tr
                    key={t.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedTplId === t.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedTplId(t.id)}
                  >
                    <td className={td}>{t.code}</td>
                    <td className={td}>{t.name}</td>
                    <td className={td}>{t.itemCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </div>

      {tplDetail && (
        <section className={panel}>
          <h2 className="mb-2 text-sm font-semibold">Hạng mục mẫu · {tplDetail.template.code}</h2>
          <ul className="mb-3 space-y-1 text-sm">
            {tplDetail.items.map((i) => (
              <li key={i.id}>{i.code} · {i.name}</li>
            ))}
            {tplDetail.items.length === 0 && <li className="text-[var(--muted)]">Chưa có hạng mục</li>}
          </ul>
          {canManage && (
            <form
              onSubmit={async (e: FormEvent) => {
                e.preventDefault();
                try {
                  await upsertPjmTemplateItem(tplDetail.template.id, { code: itemCode, name: itemName });
                  setItemName("");
                  setTplDetail(await fetchPjmTemplateDetail(tplDetail.template.id));
                  await load();
                  flash("Đã thêm hạng mục mẫu.");
                } catch (err) { setError((err as Error).message); }
              }}
              className="grid gap-2 sm:grid-cols-3"
            >
              <input className={field} value={itemCode} onChange={(e) => setItemCode(e.target.value)} required />
              <input className={field} value={itemName} onChange={(e) => setItemName(e.target.value)} placeholder="Tên HM" required />
              <button type="submit" className={btn.ghost}>Thêm HM</button>
            </form>
          )}
        </section>
      )}
    </div>
  );
}
