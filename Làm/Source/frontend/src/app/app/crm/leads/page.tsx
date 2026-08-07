"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  addCrmLeadActivity,
  assignCrmLead,
  autoIntakeCrmLead,
  convertCrmLead,
  fetchCrmLeadConversionReport,
  fetchCrmLeadDetail,
  fetchCrmLeadSources,
  fetchCrmLeads,
  importCrmLeadsCsv,
  markCrmLeadLost,
  setCrmLeadStatus,
  upsertCrmLead,
  upsertCrmLeadSource,
  upsertCrmLeadTask,
  type CrmLeadConversionReportDto,
  type CrmLeadDetailDto,
  type CrmLeadDto,
  type CrmLeadSourceDto,
} from "@/shared/api/crm-lead-api";
import { canAutoIntake } from "@/shared/api/crm-hrm-intake-helpers";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function CrmLeadsPage() {
  const { can } = usePermissions();
  const canRead = can("crm.lead.read");
  const canManage = can("crm.lead.manage");

  const [leads, setLeads] = useState<CrmLeadDto[]>([]);
  const [sources, setSources] = useState<CrmLeadSourceDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [report, setReport] = useState<CrmLeadConversionReportDto | null>(null);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<CrmLeadDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [phone, setPhone] = useState("");
  const [sourceId, setSourceId] = useState("");
  const [ownerId, setOwnerId] = useState("");
  const [srcCode, setSrcCode] = useState("WEB");
  const [srcName, setSrcName] = useState("Website");
  const [taskTitle, setTaskTitle] = useState("Gọi follow-up");
  const [activity, setActivity] = useState("");
  const [csv, setCsv] = useState("Name,Phone,Email,Company\nLead Demo,0901234567,a@b.com,ABC");
  const [lostReason, setLostReason] = useState("Không liên lạc được");

  const load = useCallback(async () => {
    const [l, s, u, r] = await Promise.all([
      fetchCrmLeads(),
      fetchCrmLeadSources(),
      fetchMsgDirectory().catch(() => [] as MsgDirectoryUserDto[]),
      fetchCrmLeadConversionReport(),
    ]);
    setLeads(l); setSources(s); setUsers(u); setReport(r);
    if (!selectedId && l[0]) setSelectedId(l[0].id);
    if (!sourceId && s[0]) setSourceId(s[0].id);
    if (!ownerId && u[0]) setOwnerId(u[0].id);
  }, [selectedId, sourceId, ownerId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchCrmLeadDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchCrmLeadDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem lead.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Lead</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Nguồn · tạo/auto · phân bổ · pipeline · task/nhắc · nhật ký · convert · import · BC (UC_CRM_024, 049–051, 053–058, 060–061)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      {report && (
        <div className="text-sm text-[var(--muted)]">
          Tổng {report.totalLeads} · Converted {report.converted} · Lost {report.lost} ·
          Tỷ lệ {report.conversionRatePercent}%
        </div>
      )}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách lead</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Lead</th><th className={th}>Sales</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {leads.map((l) => (
                  <tr key={l.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(l.id)}>
                    <td className={td}>
                      <div className="font-medium">{l.code} · {l.name}</div>
                      <div className="text-xs text-[var(--muted)]">{l.sourceName ?? "—"} · {l.intakeChannel}</div>
                    </td>
                    <td className={td}>{l.ownerName ?? "—"}</td>
                    <td className={td}>
                      <span className={statusPill(
                        l.pipelineStatus === "Converted" ? "success"
                          : l.pipelineStatus === "Lost" ? "danger" : "warning",
                      )}>{l.pipelineStatus}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {canManage && (
            <div className="mt-3 space-y-2 border-t border-black/10 pt-3">
              <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => upsertCrmLead({
                  name: name || "Lead mới", phone, sourceId: sourceId || null, ownerUserId: ownerId || null,
                }), "Đã tạo lead");
              }}>
                <input className={field} placeholder="Tên" value={name} onChange={(e) => setName(e.target.value)} />
                <input className={field} placeholder="SĐT" value={phone} onChange={(e) => setPhone(e.target.value)} />
                <select className={field} value={sourceId} onChange={(e) => setSourceId(e.target.value)}>
                  <option value="">— Nguồn —</option>
                  {sources.map((s) => <option key={s.id} value={s.id}>{s.code}</option>)}
                </select>
                <button className={btn.primary} type="submit">Tạo thủ công</button>
              </form>
              <button type="button" className={btn.ghost} disabled={!canAutoIntake(name || "Lead website", phone)} onClick={() => void run(async () => {
                const lead = await autoIntakeCrmLead({
                  name: name || "Lead website", phone, sourceCode: srcCode || "WEBSITE", note: "Auto intake",
                });
                return lead;
              }, "Đã tiếp nhận auto-intake (dedup SĐT/Email · activity).")}>
                Auto intake (website)
              </button>
              <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => upsertCrmLeadSource({
                  code: srcCode, name: srcName, channelType: "Website",
                }), "Đã lưu nguồn");
              }}>
                <input className={field} value={srcCode} onChange={(e) => setSrcCode(e.target.value)} />
                <input className={field} value={srcName} onChange={(e) => setSrcName(e.target.value)} />
                <button className={btn.ghost} type="submit">Thêm nguồn</button>
              </form>
              <textarea className={field} rows={3} value={csv} onChange={(e) => setCsv(e.target.value)} />
              <button type="button" className={btn.ghost} onClick={() => void run(
                () => importCrmLeadsCsv(csv), "Đã import CSV",
              )}>
                Import CSV
              </button>
            </div>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết / thao tác</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.lead.code}</b> — {detail.lead.name}
                <div className="text-xs text-[var(--muted)]">
                  Score {detail.lead.score} · Task mở {detail.lead.openTaskCount} ·
                  Follow-up {detail.lead.nextFollowUpAt ? new Date(detail.lead.nextFollowUpAt).toLocaleString() : "—"}
                </div>
              </div>
              {canManage && detail.lead.pipelineStatus !== "Converted" && detail.lead.pipelineStatus !== "Lost" && (
                <div className="flex flex-wrap gap-2">
                  <select className={field} value={ownerId} onChange={(e) => setOwnerId(e.target.value)}>
                    {users.map((u) => <option key={u.id} value={u.id}>{u.displayName || u.username}</option>)}
                  </select>
                  <button type="button" className={btn.ghost} onClick={() => void run(
                    () => assignCrmLead(detail.lead.id, ownerId), "Đã phân bổ",
                  )}>
                    Phân bổ
                  </button>
                  {["Contacted", "Qualified"].map((st) => (
                    <button key={st} type="button" className={btn.ghost} onClick={() => void run(
                      () => setCrmLeadStatus(detail.lead.id, st), `→ ${st}`,
                    )}>{st}</button>
                  ))}
                  <button type="button" className={btn.primary} onClick={() => void run(
                    () => convertCrmLead(detail.lead.id), "Đã convert → cơ hội",
                  )}>
                    Convert
                  </button>
                  <input className={field} value={lostReason} onChange={(e) => setLostReason(e.target.value)} />
                  <button type="button" className={btn.ghost} onClick={() => void run(
                    () => markCrmLeadLost(detail.lead.id, lostReason), "Đã mark Lost",
                  )}>
                    Lost
                  </button>
                </div>
              )}
              {canManage && (
                <>
                  <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    const due = new Date(Date.now() + 86400000).toISOString();
                    void run(() => upsertCrmLeadTask({
                      leadId: detail.lead.id, title: taskTitle, dueAt: due,
                      assigneeUserId: ownerId || null, isReminder: true,
                    }), "Đã tạo task/nhắc");
                  }}>
                    <input className={field} value={taskTitle} onChange={(e) => setTaskTitle(e.target.value)} />
                    <button className={btn.ghost} type="submit">Task follow-up</button>
                  </form>
                  <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    void run(() => addCrmLeadActivity({
                      leadId: detail.lead.id, activityType: "Call", content: activity || "Gọi chăm sóc",
                    }), "Đã ghi nhật ký");
                  }}>
                    <input className={field} placeholder="Nhật ký" value={activity} onChange={(e) => setActivity(e.target.value)} />
                    <button className={btn.ghost} type="submit">Ghi hoạt động</button>
                  </form>
                </>
              )}
              <div className="text-xs">
                <div className="font-medium mb-1">Tasks</div>
                {detail.tasks.map((t) => (
                  <div key={t.id}>{t.title} · {new Date(t.dueAt).toLocaleDateString()} · {t.status}
                    {t.isReminder ? " · nhắc" : ""}</div>
                ))}
                <div className="font-medium mt-2 mb-1">Nhật ký</div>
                {detail.activities.slice(0, 8).map((a) => (
                  <div key={a.id}>{a.activityType}: {a.content}</div>
                ))}
              </div>
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chọn một lead.</p>
          )}
        </section>
      </div>
    </div>
  );
}
