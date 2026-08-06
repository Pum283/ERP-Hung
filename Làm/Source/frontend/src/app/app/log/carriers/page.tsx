"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchLogCarriers,
  upsertLogCarrier,
  type LogCarrierDto,
} from "@/shared/api/log-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function LogCarriersPage() {
  const { can } = usePermissions();
  const canRead = can("log.carrier.read");
  const canManage = can("log.carrier.manage");

  const [list, setList] = useState<LogCarrierDto[]>([]);
  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [code, setCode] = useState("GHN");
  const [name, setName] = useState("");
  const [phone, setPhone] = useState("");
  const [contact, setContact] = useState("");

  const load = useCallback(async () => {
    setList(await fetchLogCarriers(q || undefined));
  }, [q]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    load()
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function onSave(e: FormEvent) {
    e.preventDefault();
    try {
      await upsertLogCarrier({ code, name, phone, contactName: contact, status: "Active" });
      setName("");
      setPhone("");
      setContact("");
      await load();
      flash("Đã lưu ĐVVC.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem ĐVVC.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Đơn vị vận chuyển</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">Danh mục ĐVVC (UC_LOG_001)</p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-[1fr_1.2fr]">
        {canManage && (
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Thêm ĐVVC</h2>
            <form onSubmit={onSave} className="grid gap-2">
              <input className={field} value={code} onChange={(e) => setCode(e.target.value)} placeholder="Mã" required />
              <input className={field} value={name} onChange={(e) => setName(e.target.value)} placeholder="Tên" required />
              <input className={field} value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="SĐT" />
              <input className={field} value={contact} onChange={(e) => setContact(e.target.value)} placeholder="Người liên hệ" />
              <button type="submit" className={btn.primary}>Lưu</button>
            </form>
          </section>
        )}

        <section className={panel}>
          <div className="mb-3 flex gap-2">
            <input className={`${field} w-48`} value={q} onChange={(e) => setQ(e.target.value)} placeholder="Tìm" />
            <button type="button" className={btn.ghost} onClick={() => load().catch((e: Error) => setError(e.message))}>
              Tìm
            </button>
          </div>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>Liên hệ</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {list.map((c) => (
                  <tr key={c.id}>
                    <td className={td}>{c.code}</td>
                    <td className={td}>{c.name}</td>
                    <td className={td}>{c.contactName || "—"} · {c.phone || "—"}</td>
                    <td className={td}>
                      <span className={statusPill(c.status === "Active" ? "success" : "muted")}>{c.status}</span>
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
