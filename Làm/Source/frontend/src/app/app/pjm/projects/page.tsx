"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  closePjmProject,
  createPjmAcceptance,
  createPjmMaterialIssue,
  fetchPjmProjectDetail,
  fetchPjmProjects,
  fetchPjmStatuses,
  fetchPjmTemplates,
  fetchPjmTypes,
  recognizePjmRevenue,
  signPjmAcceptance,
  upsertPjmExpense,
  upsertPjmMember,
  upsertPjmProject,
  upsertPjmWbs,
  type PjmProjectDetailDto,
  type PjmProjectDto,
  type PjmProjectStatusDto,
  type PjmProjectTypeDto,
  type PjmWbsTemplateDto,
} from "@/shared/api/pjm-api";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PjmProjectsPage() {
  const { can } = usePermissions();
  const canRead = can("pjm.project.read");
  const canManage = can("pjm.project.manage");

  const [list, setList] = useState<PjmProjectDto[]>([]);
  const [types, setTypes] = useState<PjmProjectTypeDto[]>([]);
  const [statuses, setStatuses] = useState<PjmProjectStatusDto[]>([]);
  const [templates, setTemplates] = useState<PjmWbsTemplateDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<PjmProjectDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [typeId, setTypeId] = useState("");
  const [customer, setCustomer] = useState("");
  const [contract, setContract] = useState("");
  const [opportunity, setOpportunity] = useState("");
  const [budget, setBudget] = useState("0");
  const [pmId, setPmId] = useState("");
  const [tplId, setTplId] = useState("");
  const [statusCode, setStatusCode] = useState("Draft");
  const [memberId, setMemberId] = useState("");
  const [memberRole, setMemberRole] = useState("Member");
  const [wbsCode, setWbsCode] = useState("1.1");
  const [wbsName, setWbsName] = useState("");
  const [wbsAssignee, setWbsAssignee] = useState("");
  const [wbsPct, setWbsPct] = useState("0");
  const [wbsDue, setWbsDue] = useState("");
  const [wbsMilestone, setWbsMilestone] = useState(false);
  const [allocPct, setAllocPct] = useState("100");
  const [expDesc, setExpDesc] = useState("");
  const [expAmount, setExpAmount] = useState("0");
  const [matCode, setMatCode] = useState("NVL-001");
  const [matName, setMatName] = useState("");
  const [matQty, setMatQty] = useState("1");
  const [matCost, setMatCost] = useState("0");
  const [accTitle, setAccTitle] = useState("Nghiệm thu giai đoạn");
  const [accKind, setAccKind] = useState("Phase");
  const [signer, setSigner] = useState("");
  const [revenue, setRevenue] = useState("0");

  const load = useCallback(async () => {
    const [p, t, s, tpl, u] = await Promise.all([
      fetchPjmProjects(),
      fetchPjmTypes().catch(() => [] as PjmProjectTypeDto[]),
      fetchPjmStatuses().catch(() => [] as PjmProjectStatusDto[]),
      fetchPjmTemplates().catch(() => [] as PjmWbsTemplateDto[]),
      fetchMsgDirectory().catch(() => [] as MsgDirectoryUserDto[]),
    ]);
    setList(p); setTypes(t); setStatuses(s); setTemplates(tpl); setUsers(u);
    if (!selectedId && p[0]) setSelectedId(p[0].id);
    if (!typeId && t[0]) setTypeId(t[0].id);
    if (!pmId && u[0]) setPmId(u[0].id);
    if (!memberId && u[0]) setMemberId(u[0].id);
    if (!wbsAssignee && u[0]) setWbsAssignee(u[0].id);
    if (!tplId && tpl[0]) setTplId(tpl[0].id);
  }, [selectedId, typeId, pmId, memberId, wbsAssignee, tplId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchPjmProjectDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchPjmProjectDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem dự án.</div>;
  }

  const p = detail?.project;

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Dự án</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Tạo thủ công / từ cơ hội · KH/HĐ · PM · NS · WBS · gán người (UC_PJM_005–009, 011–012)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-[1fr_1.4fr]">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách dự án</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>TT</th>
                  <th className={th}>WBS</th>
                </tr>
              </thead>
              <tbody>
                {list.map((x) => (
                  <tr
                    key={x.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedId === x.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedId(x.id)}
                  >
                    <td className={td}>{x.code}</td>
                    <td className={td}>
                      <div>{x.name}</div>
                      <div className="text-xs text-[var(--muted)]">{x.customerName || "—"}</div>
                    </td>
                    <td className={td}>
                      <span className={statusPill(x.statusCode === "Completed" ? "success" : "muted")}>
                        {x.statusName || x.statusCode}
                      </span>
                    </td>
                    <td className={td}>{x.wbsCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo dự án</h2>
              <form
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  const start = new Date();
                  const end = new Date(Date.now() + 90 * 86400000);
                  run(async () => {
                    const saved = await upsertPjmProject({
                      name,
                      projectTypeId: typeId || null,
                      statusCode: "Draft",
                      customerName: customer || null,
                      contractCode: contract || null,
                      sourceOpportunityCode: opportunity || null,
                      pmUserId: pmId || null,
                      budget: Number(budget) || 0,
                      startDate: start.toISOString(),
                      endDate: end.toISOString(),
                      applyTemplateId: tplId || null,
                    });
                    setSelectedId(saved.id);
                    setName(""); setCustomer(""); setContract(""); setOpportunity("");
                  }, "Đã tạo dự án.");
                }}
                className="grid gap-2 sm:grid-cols-2"
              >
                <input className={`${field} sm:col-span-2`} value={name} onChange={(e) => setName(e.target.value)} placeholder="Tên dự án" required />
                <select className={field} value={typeId} onChange={(e) => setTypeId(e.target.value)}>
                  <option value="">— Loại —</option>
                  {types.map((t) => <option key={t.id} value={t.id}>{t.code}</option>)}
                </select>
                <select className={field} value={tplId} onChange={(e) => setTplId(e.target.value)}>
                  <option value="">— Mẫu WBS —</option>
                  {templates.map((t) => <option key={t.id} value={t.id}>{t.code}</option>)}
                </select>
                <input className={field} value={customer} onChange={(e) => setCustomer(e.target.value)} placeholder="Khách hàng" />
                <input className={field} value={contract} onChange={(e) => setContract(e.target.value)} placeholder="Mã HĐ" />
                <input className={field} value={opportunity} onChange={(e) => setOpportunity(e.target.value)} placeholder="Cơ hội CRM" />
                <input className={field} value={budget} onChange={(e) => setBudget(e.target.value)} placeholder="Ngân sách" />
                <select className={`${field} sm:col-span-2`} value={pmId} onChange={(e) => setPmId(e.target.value)}>
                  {users.map((u) => <option key={u.id} value={u.id}>PM: {u.displayName || u.username}</option>)}
                </select>
                <button type="submit" className={`${btn.primary} sm:col-span-2`}>Tạo dự án</button>
              </form>
            </section>
          )}

          {detail && p && (
            <section className={panel}>
              <h2 className="mb-1 text-sm font-semibold">{p.code} · {p.name}</h2>
              <p className="mb-3 text-xs text-[var(--muted)]">
                {p.projectTypeName || "—"} · KH {p.customerName || "—"} · HĐ {p.contractCode || "—"}
                {p.sourceOpportunityCode ? ` · Opp ${p.sourceOpportunityCode}` : ""}
                {" · "}NS {p.budget.toLocaleString()} · PM {p.pmName || "—"}
              </p>

              {canManage && (
                <div className="mb-4 flex flex-wrap gap-2">
                  <select className={`${field} w-40`} value={statusCode} onChange={(e) => setStatusCode(e.target.value)}>
                    {statuses.map((s) => <option key={s.id} value={s.code}>{s.name}</option>)}
                  </select>
                  <button
                    type="button"
                    className={btn.ghost}
                    onClick={() =>
                      run(
                        () => upsertPjmProject({
                          id: p.id, name: p.name, projectTypeId: p.projectTypeId,
                          statusCode, customerName: p.customerName, contractCode: p.contractCode,
                          sourceOpportunityCode: p.sourceOpportunityCode, pmUserId: p.pmUserId,
                          budget: p.budget, startDate: p.startDate, endDate: p.endDate, note: p.note,
                        }),
                        "Đã cập nhật trạng thái.",
                      )
                    }
                  >
                    Đổi TT
                  </button>
                </div>
              )}

              <h3 className="mb-2 text-xs font-semibold uppercase text-[var(--muted)]">Thành viên / phân công</h3>
              <ul className="mb-2 space-y-1 text-sm">
                {detail.members.map((m) => (
                  <li key={m.id}>{m.userName} · {m.role} · {m.allocationPct}%</li>
                ))}
              </ul>
              {canManage && (
                <form
                  onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    run(
                      () => upsertPjmMember(p.id, {
                        userId: memberId, role: memberRole, allocationPct: Number(allocPct) || 100,
                      }),
                      "Đã gán thành viên.",
                    );
                  }}
                  className="mb-4 flex flex-wrap gap-2"
                >
                  <select className={field} value={memberId} onChange={(e) => setMemberId(e.target.value)}>
                    {users.map((u) => <option key={u.id} value={u.id}>{u.displayName || u.username}</option>)}
                  </select>
                  <select className={field} value={memberRole} onChange={(e) => setMemberRole(e.target.value)}>
                    <option value="PM">PM</option>
                    <option value="Member">Member</option>
                    <option value="Viewer">Viewer</option>
                  </select>
                  <input className={`${field} w-24`} type="number" min={0} max={100} value={allocPct} onChange={(e) => setAllocPct(e.target.value)} title="% phân bổ" />
                  <button type="submit" className={btn.ghost}>Gán</button>
                </form>
              )}

              <h3 className="mb-2 text-xs font-semibold uppercase text-[var(--muted)]">WBS</h3>
              <ul className="mb-2 space-y-1 text-sm">
                {detail.wbsItems.map((w) => (
                  <li key={w.id}>
                    {w.code} · {w.name}
                    {w.isMilestone ? " ★" : ""}
                    {` · ${w.percentComplete}%`}
                    {w.dueDate ? ` · hạn ${w.dueDate.slice(0, 10)}` : ""}
                    {w.assigneeName ? ` → ${w.assigneeName}` : ""}{" "}
                    <span className={statusPill(w.isOverdue ? "danger" : w.status === "Done" ? "success" : "muted")}>
                      {w.isOverdue ? "Trễ" : w.status}
                    </span>
                  </li>
                ))}
                {detail.wbsItems.length === 0 && <li className="text-[var(--muted)]">Chưa có hạng mục</li>}
              </ul>
              {canManage && (
                <form
                  onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    run(
                      () => upsertPjmWbs(p.id, {
                        code: wbsCode, name: wbsName, assigneeUserId: wbsAssignee || null,
                        percentComplete: Number(wbsPct) || 0,
                        isMilestone: wbsMilestone,
                        dueDate: wbsDue ? new Date(wbsDue + "T23:59:59").toISOString() : null,
                      }),
                      "Đã thêm / cập nhật WBS.",
                    );
                    setWbsName("");
                    setWbsPct("0");
                    setWbsDue("");
                    setWbsMilestone(false);
                  }}
                  className="grid gap-2 sm:grid-cols-3"
                >
                  <input className={field} value={wbsCode} onChange={(e) => setWbsCode(e.target.value)} placeholder="Mã" required />
                  <input className={field} value={wbsName} onChange={(e) => setWbsName(e.target.value)} placeholder="Tên HM" required />
                  <select className={field} value={wbsAssignee} onChange={(e) => setWbsAssignee(e.target.value)}>
                    {users.map((u) => <option key={u.id} value={u.id}>{u.displayName || u.username}</option>)}
                  </select>
                  <input className={field} type="number" min={0} max={100} value={wbsPct} onChange={(e) => setWbsPct(e.target.value)} placeholder="% HT" />
                  <input className={field} type="date" value={wbsDue} onChange={(e) => setWbsDue(e.target.value)} />
                  <label className="flex items-center gap-2 text-sm">
                    <input type="checkbox" checked={wbsMilestone} onChange={(e) => setWbsMilestone(e.target.checked)} />
                    Milestone
                  </label>
                  <button type="submit" className={`${btn.ghost} sm:col-span-3`}>Thêm / cập nhật WBS</button>
                </form>
              )}

              <h3 className="mb-2 mt-4 text-xs font-semibold uppercase text-[var(--muted)]">Chi phí · NVL · NT · đóng</h3>
              <div className="mb-3 grid gap-2 sm:grid-cols-4 text-sm">
                <div className={panel}>NS: {detail.costSummary.budget}</div>
                <div className={panel}>Thực tế: {detail.costSummary.actualCost}</div>
                <div className={panel}>DT: {detail.costSummary.recognizedRevenue}</div>
                <div className={panel}>Margin: {detail.costSummary.margin}</div>
              </div>
              {canManage && p.statusCode !== "Closed" && p.statusCode !== "Completed" && (
                <div className="space-y-2">
                  <form
                    className="grid gap-2 sm:grid-cols-4"
                    onSubmit={(e: FormEvent) => {
                      e.preventDefault();
                      run(
                        () => upsertPjmExpense(p.id, {
                          category: "Other", description: expDesc, amount: Number(expAmount) || 0, post: true,
                        }),
                        "Đã ghi chi phí.",
                      );
                      setExpDesc("");
                    }}
                  >
                    <input className={`${field} sm:col-span-2`} value={expDesc} onChange={(e) => setExpDesc(e.target.value)} placeholder="Mô tả chi phí" required />
                    <input className={field} type="number" min={0} value={expAmount} onChange={(e) => setExpAmount(e.target.value)} />
                    <button type="submit" className={btn.ghost}>Ghi CP</button>
                  </form>
                  <form
                    className="grid gap-2 sm:grid-cols-5"
                    onSubmit={(e: FormEvent) => {
                      e.preventDefault();
                      run(
                        () => createPjmMaterialIssue(p.id, {
                          post: true,
                          lines: [{ productCode: matCode, productName: matName || matCode, qty: Number(matQty) || 1, unitCost: Number(matCost) || 0 }],
                        }),
                        "Đã xuất NVL.",
                      );
                      setMatName("");
                    }}
                  >
                    <input className={field} value={matCode} onChange={(e) => setMatCode(e.target.value)} placeholder="Mã NVL" />
                    <input className={field} value={matName} onChange={(e) => setMatName(e.target.value)} placeholder="Tên NVL" />
                    <input className={field} type="number" value={matQty} onChange={(e) => setMatQty(e.target.value)} />
                    <input className={field} type="number" value={matCost} onChange={(e) => setMatCost(e.target.value)} />
                    <button type="submit" className={btn.ghost}>Xuất NVL</button>
                  </form>
                  <div className="flex flex-wrap gap-2">
                    <select className={field} value={accKind} onChange={(e) => setAccKind(e.target.value)}>
                      <option value="Phase">Phase</option>
                      <option value="Final">Final</option>
                    </select>
                    <input className={field} value={accTitle} onChange={(e) => setAccTitle(e.target.value)} />
                    <button
                      type="button"
                      className={btn.ghost}
                      onClick={() => run(() => createPjmAcceptance(p.id, { kind: accKind, title: accTitle }), "Đã tạo BBNT.")}
                    >
                      Tạo BBNT
                    </button>
                    <input className={field} value={signer} onChange={(e) => setSigner(e.target.value)} placeholder="Khách ký" />
                    <button
                      type="button"
                      className={btn.ghost}
                      disabled={!signer || detail.acceptances.length === 0}
                      onClick={() => {
                        const draft = detail.acceptances.find((a) => a.status === "Draft") ?? detail.acceptances[0];
                        if (!draft) return;
                        run(() => signPjmAcceptance(p.id, draft.id, { signerName: signer }), "Đã ký NT.");
                      }}
                    >
                      Ký NT
                    </button>
                    <input className={`${field} w-28`} type="number" value={revenue} onChange={(e) => setRevenue(e.target.value)} />
                    <button
                      type="button"
                      className={btn.ghost}
                      onClick={() => run(() => recognizePjmRevenue(p.id, { amount: Number(revenue) || 0 }), "Đã ghi DT.")}
                    >
                      Ghi DT
                    </button>
                    <button
                      type="button"
                      className={btn.primary}
                      onClick={() => run(() => closePjmProject(p.id), "Đã đóng dự án.")}
                    >
                      Đóng DA
                    </button>
                  </div>
                </div>
              )}
              <ul className="mt-2 space-y-1 text-xs text-[var(--muted)]">
                {detail.expenses.slice(0, 5).map((x) => (
                  <li key={x.id}>CP {x.code}: {x.description} · {x.amount} ({x.status})</li>
                ))}
                {detail.materialIssues.slice(0, 5).map((x) => (
                  <li key={x.id}>NVL {x.code}: {x.totalAmount} ({x.status})</li>
                ))}
                {detail.acceptances.map((a) => (
                  <li key={a.id}>NT {a.code} · {a.kind} · {a.status}{a.signerName ? ` · ${a.signerName}` : ""}</li>
                ))}
              </ul>
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
