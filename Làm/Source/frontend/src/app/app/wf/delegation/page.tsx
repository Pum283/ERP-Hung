"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  deactivateWfDelegation,
  fetchWfDelegations,
  upsertWfDelegation,
  type WfDelegationDto,
} from "@/shared/api/wf-api";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

function today() {
  return new Date().toISOString().slice(0, 10);
}

function addDays(iso: string, n: number) {
  const d = new Date(iso + "T00:00:00");
  d.setDate(d.getDate() + n);
  return d.toISOString().slice(0, 10);
}

export default function WfDelegationPage() {
  const { can } = usePermissions();
  const canRead = can("wf.task.read");
  const canAct = can("wf.task.act");

  const [rows, setRows] = useState<WfDelegationDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [toUserId, setToUserId] = useState("");
  const [startDate, setStartDate] = useState(today);
  const [endDate, setEndDate] = useState(() => addDays(today(), 7));
  const [moduleCode, setModuleCode] = useState("");
  const [note, setNote] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const [d, u] = await Promise.all([fetchWfDelegations(), fetchMsgDirectory()]);
      setRows(d);
      setUsers(u);
      if (!toUserId && u[0]) setToUserId(u[0].id);
    } catch {
      setError("Không tải được ủy quyền.");
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền wf.task.read</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Ủy quyền duyệt</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Ủy quyền tạm thời — người nhận thấy &amp; xử lý task của bạn trong kỳ hiệu lực
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      {canAct && (
        <form
          className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm max-w-xl"
          onSubmit={async (e: FormEvent) => {
            e.preventDefault();
            setError(null);
            setOk(null);
            try {
              await upsertWfDelegation({
                toUserId,
                startDate,
                endDate,
                moduleCode: moduleCode || null,
                isActive: true,
                note: note || null,
              });
              setOk("Đã tạo ủy quyền.");
              setNote("");
              await load();
            } catch {
              setError("Tạo ủy quyền thất bại.");
            }
          }}
        >
          <h2 className="text-lead font-bold">Tạo ủy quyền</h2>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Ủy quyền cho</span>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={toUserId}
              onChange={(e) => setToUserId(e.target.value)}
              required
            >
              {users.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.displayName || u.username}
                </option>
              ))}
            </select>
          </label>
          <div className="grid grid-cols-2 gap-2">
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Từ ngày</span>
              <input
                type="date"
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                required
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Đến ngày</span>
              <input
                type="date"
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                required
              />
            </label>
          </div>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Module (để trống = tất cả)</span>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={moduleCode}
              onChange={(e) => setModuleCode(e.target.value)}
            >
              <option value="">Tất cả</option>
              <option value="HRM">HRM</option>
              <option value="WF">WF</option>
              <option value="FIN">FIN</option>
              <option value="PUR">PUR</option>
            </select>
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Ghi chú</span>
            <input
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={note}
              onChange={(e) => setNote(e.target.value)}
            />
          </label>
          <button type="submit" className={btn.primary}>
            Lưu ủy quyền
          </button>
        </form>
      )}

      <section className="rounded-xl border border-border bg-surface p-4 shadow-sm">
        <h2 className="text-lead font-bold">Danh sách liên quan</h2>
        {rows.length === 0 ? (
          <p className="mt-2 text-body text-muted-foreground">Chưa có ủy quyền.</p>
        ) : (
          <div className="mt-3 overflow-x-auto">
            <table className="w-full text-left text-body">
              <thead>
                <tr className="border-b border-border text-muted-foreground">
                  <th className="py-2 pr-2">Từ</th>
                  <th className="py-2 pr-2">Cho</th>
                  <th className="py-2 pr-2">Kỳ</th>
                  <th className="py-2 pr-2">Module</th>
                  <th className="py-2 pr-2">TT</th>
                  <th className="py-2">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r) => (
                  <tr key={r.id} className="border-b border-border/60">
                    <td className="py-2 pr-2">{r.fromUserName}</td>
                    <td className="py-2 pr-2">{r.toUserName}</td>
                    <td className="py-2 pr-2">
                      {r.startDate.slice(0, 10)} → {r.endDate.slice(0, 10)}
                    </td>
                    <td className="py-2 pr-2">{r.moduleCode || "Tất cả"}</td>
                    <td className="py-2 pr-2">{r.isActive ? "Active" : "Off"}</td>
                    <td className="py-2">
                      {canAct && r.isActive && (
                        <button
                          type="button"
                          className={btn.ghost}
                          onClick={async () => {
                            try {
                              await deactivateWfDelegation(r.id);
                              setOk("Đã tắt ủy quyền.");
                              await load();
                            } catch {
                              setError("Tắt ủy quyền thất bại.");
                            }
                          }}
                        >
                          Tắt
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
