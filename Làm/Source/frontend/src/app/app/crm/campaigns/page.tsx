"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  closeCrmCampaign,
  fetchCrmCampaignExpenses,
  fetchCrmCampaigns,
  fetchCrmMarketingDashboard,
  fetchCrmWebLeads,
  syncCrmWebLead,
  upsertCrmCampaign,
  upsertCrmCampaignExpense,
  type CrmCampaignDto,
  type CrmCampaignExpenseDto,
  type CrmMarketingDashboardDto,
  type CrmWebLeadDto,
} from "@/shared/api/crm-marketing-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

type Tab = "campaigns" | "expenses" | "webleads" | "dashboard";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

function pillTone(status: string): "brand" | "success" | "warning" | "danger" | "muted" {
  const s = status.toLowerCase();
  if (s === "active" || s === "synced" || s === "used") return "success";
  if (s === "closed" || s === "expired" || s === "cancelled") return "danger";
  if (s === "draft" || s === "paused" || s === "pending") return "warning";
  return "muted";
}

export default function CrmCampaignsPage() {
  const { can } = usePermissions();
  const canRead = can("crm.campaign.read");
  const canManage = can("crm.campaign.manage");

  const [tab, setTab] = useState<Tab>("campaigns");
  const [list, setList] = useState<CrmCampaignDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [expenses, setExpenses] = useState<CrmCampaignExpenseDto[]>([]);
  const [webLeads, setWebLeads] = useState<CrmWebLeadDto[]>([]);
  const [dash, setDash] = useState<CrmMarketingDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [code, setCode] = useState("");
  const [name, setName] = useState("Summer Sale");
  const [channel, setChannel] = useState("Social");
  const [budget, setBudget] = useState("50000000");
  const [expType, setExpType] = useState("Ads");
  const [expAmount, setExpAmount] = useState("1000000");
  const [expNote, setExpNote] = useState("");
  const [wlName, setWlName] = useState("");
  const [wlPhone, setWlPhone] = useState("");
  const [wlEmail, setWlEmail] = useState("");
  const [wlLanding, setWlLanding] = useState("/landing/summer");

  const load = useCallback(async () => {
    const [c, w, d] = await Promise.all([
      fetchCrmCampaigns(),
      fetchCrmWebLeads(),
      fetchCrmMarketingDashboard(),
    ]);
    setList(c);
    setWebLeads(w);
    setDash(d);
    if (!selectedId && c[0]) setSelectedId(c[0].id);
  }, [selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchCrmCampaignExpenses(selectedId).then(setExpenses).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
  }

  async function onSaveCampaign(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    try {
      const row = await upsertCrmCampaign({
        code: code.trim(),
        name,
        channel,
        budgetAmount: Number(budget) || 0,
      });
      flash(`Đã lưu campaign ${row.code}`);
      setCode("");
      await load();
      setSelectedId(row.id);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi lưu campaign");
    }
  }

  async function onClose() {
    if (!canManage || !selectedId) return;
    try {
      const row = await closeCrmCampaign(selectedId, "Đóng từ UI");
      flash(`Đã đóng ${row.code}`);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi đóng campaign");
    }
  }

  async function onExpense(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !selectedId) return;
    try {
      await upsertCrmCampaignExpense(selectedId, {
        expenseType: expType,
        amount: Number(expAmount) || 0,
        description: expNote || undefined,
      });
      flash("Đã ghi chi phí");
      setExpenses(await fetchCrmCampaignExpenses(selectedId));
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi ghi chi phí");
    }
  }

  async function onSyncLead(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    try {
      const row = await syncCrmWebLead({
        contactName: wlName,
        phone: wlPhone || undefined,
        email: wlEmail || undefined,
        landingPage: wlLanding || undefined,
        campaignId: selectedId || undefined,
        utmSource: "web",
        utmMedium: "landing",
      });
      flash(`Đã sync web-lead → ${row.syncStatus}`);
      setWlName(""); setWlPhone(""); setWlEmail("");
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi sync web-lead");
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-rose-600">Thiếu quyền crm.campaign.read</div>;
  }

  const selected = list.find((x) => x.id === selectedId);

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold text-slate-800">Campaign marketing</h1>
          <p className="text-sm text-slate-500">UC_CRM_016 · 019 · 023 · 026 · 029 · 031</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["campaigns", "Campaign"],
            ["expenses", "Chi phí"],
            ["webleads", "Web lead"],
            ["dashboard", "Dashboard"],
          ] as const).map(([id, label]) => (
            <button key={id} type="button" className={tab === id ? btn.primary : btn.ghost} onClick={() => setTab(id)}>
              {label}
            </button>
          ))}
        </div>
      </div>

      {error && <div className="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</div>}
      {ok && <div className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-slate-500">Đang tải…</div>}

      {tab === "campaigns" && (
        <div className="grid gap-4 lg:grid-cols-2">
          <div className={panel}>
            <h2 className="mb-3 text-sm font-semibold text-slate-700">Danh sách</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Tên</th>
                    <th className={th}>TT</th>
                    <th className={th}>Chi</th>
                  </tr>
                </thead>
                <tbody>
                  {list.map((r) => (
                    <tr key={r.id} className="cursor-pointer hover:bg-slate-50" onClick={() => setSelectedId(r.id)}>
                      <td className={td}>{r.code}</td>
                      <td className={td}>{r.name}</td>
                      <td className={td}><span className={statusPill(pillTone(r.status))}>{r.status}</span></td>
                      <td className={`${td} text-right`}>{money(r.spentAmount)}</td>
                    </tr>
                  ))}
                  {list.length === 0 && (
                    <tr><td className={td} colSpan={4}>Chưa có campaign.</td></tr>
                  )}
                </tbody>
              </table>
            </div>
            {selected && canManage && selected.status !== "Closed" && (
              <button type="button" className={`${btn.ghost} mt-3`} onClick={onClose}>Đóng campaign</button>
            )}
          </div>
          <form className={`${panel} space-y-3`} onSubmit={onSaveCampaign}>
            <h2 className="text-sm font-semibold text-slate-700">Tạo / cập nhật</h2>
            <label className="block text-xs text-slate-500">Mã (để trống = tự sinh)
              <input className={field} value={code} onChange={(e) => setCode(e.target.value)} disabled={!canManage} />
            </label>
            <label className="block text-xs text-slate-500">Tên
              <input className={field} value={name} onChange={(e) => setName(e.target.value)} required disabled={!canManage} />
            </label>
            <label className="block text-xs text-slate-500">Kênh
              <select className={field} value={channel} onChange={(e) => setChannel(e.target.value)} disabled={!canManage}>
                {["Email", "Social", "SEM", "Event", "Other"].map((c) => <option key={c}>{c}</option>)}
              </select>
            </label>
            <label className="block text-xs text-slate-500">Ngân sách
              <input className={field} type="number" value={budget} onChange={(e) => setBudget(e.target.value)} disabled={!canManage} />
            </label>
            {canManage && <button type="submit" className={btn.primary}>Lưu campaign</button>}
          </form>
        </div>
      )}

      {tab === "expenses" && (
        <div className="grid gap-4 lg:grid-cols-2">
          <div className={panel}>
            <h2 className="mb-2 text-sm font-semibold">Chi phí — {selected?.code ?? "—"}</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead><tr><th className={th}>Loại</th><th className={th}>Số tiền</th><th className={th}>Mô tả</th></tr></thead>
                <tbody>
                  {expenses.map((x) => (
                    <tr key={x.id}>
                      <td className={td}>{x.expenseType}</td>
                      <td className={`${td} text-right`}>{money(x.amount)}</td>
                      <td className={td}>{x.description}</td>
                    </tr>
                  ))}
                  {expenses.length === 0 && <tr><td className={td} colSpan={3}>Chưa có chi phí.</td></tr>}
                </tbody>
              </table>
            </div>
          </div>
          <form className={`${panel} space-y-3`} onSubmit={onExpense}>
            <h2 className="text-sm font-semibold">Ghi nhận chi phí</h2>
            <label className="block text-xs text-slate-500">Loại
              <select className={field} value={expType} onChange={(e) => setExpType(e.target.value)}>
                {["Ads", "Media", "Event", "Agency", "Other"].map((c) => <option key={c}>{c}</option>)}
              </select>
            </label>
            <label className="block text-xs text-slate-500">Số tiền
              <input className={field} type="number" value={expAmount} onChange={(e) => setExpAmount(e.target.value)} />
            </label>
            <label className="block text-xs text-slate-500">Mô tả
              <input className={field} value={expNote} onChange={(e) => setExpNote(e.target.value)} />
            </label>
            {canManage && <button type="submit" className={btn.primary} disabled={!selectedId}>Lưu chi phí</button>}
          </form>
        </div>
      )}

      {tab === "webleads" && (
        <div className="grid gap-4 lg:grid-cols-2">
          <div className={panel}>
            <h2 className="mb-2 text-sm font-semibold">Web leads đã sync</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead><tr><th className={th}>Tên</th><th className={th}>SĐT/Email</th><th className={th}>TT</th></tr></thead>
                <tbody>
                  {webLeads.map((w) => (
                    <tr key={w.id}>
                      <td className={td}>{w.contactName}</td>
                      <td className={td}>{w.phone || w.email}</td>
                      <td className={td}><span className={statusPill(pillTone(w.syncStatus))}>{w.syncStatus}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
          <form className={`${panel} space-y-3`} onSubmit={onSyncLead}>
            <h2 className="text-sm font-semibold">Sync lead website / landing</h2>
            <label className="block text-xs text-slate-500">Tên liên hệ
              <input className={field} value={wlName} onChange={(e) => setWlName(e.target.value)} required />
            </label>
            <label className="block text-xs text-slate-500">SĐT
              <input className={field} value={wlPhone} onChange={(e) => setWlPhone(e.target.value)} />
            </label>
            <label className="block text-xs text-slate-500">Email
              <input className={field} value={wlEmail} onChange={(e) => setWlEmail(e.target.value)} />
            </label>
            <label className="block text-xs text-slate-500">Landing
              <input className={field} value={wlLanding} onChange={(e) => setWlLanding(e.target.value)} />
            </label>
            {canManage && <button type="submit" className={btn.primary}>Sync → Lead CRM</button>}
          </form>
        </div>
      )}

      {tab === "dashboard" && dash && (
        <div className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {[
              ["Campaign", dash.totalCampaigns],
              ["Đang chạy", dash.activeCampaigns],
              ["Chi tiêu", money(dash.totalSpent)],
              ["ROI %", dash.overallRoi],
            ].map(([k, v]) => (
              <div key={String(k)} className={panel}>
                <div className="text-xs text-slate-500">{k}</div>
                <div className="text-lg font-semibold text-slate-800">{v}</div>
              </div>
            ))}
          </div>
          <div className={panel}>
            <h2 className="mb-2 text-sm font-semibold">CPL / CAC / ROAS / ROI theo campaign</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Campaign</th>
                    <th className={th}>Lead</th>
                    <th className={th}>CPL</th>
                    <th className={th}>CAC</th>
                    <th className={th}>ROAS</th>
                    <th className={th}>ROI %</th>
                  </tr>
                </thead>
                <tbody>
                  {dash.campaignMetrics.map((m) => (
                    <tr key={m.campaignId}>
                      <td className={td}>{m.campaignName}</td>
                      <td className={td}>{m.leadCount}</td>
                      <td className={`${td} text-right`}>{money(m.costPerLead)}</td>
                      <td className={`${td} text-right`}>{money(m.customerAcquisitionCost)}</td>
                      <td className={td}>{m.roas}</td>
                      <td className={td}>{m.roiPercent}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
