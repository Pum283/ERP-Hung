"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  applyRewardToPayroll,
  attachRewardDecision,
  createRewardDecision,
  fetchEmployees,
  fetchRewardDecisions,
  fetchRewardReport,
  uploadHrmFile,
  type EmployeeDto,
  type RewardDisciplineDto,
  type RewardDisciplineReportRowDto,
} from "@/shared/api/hrm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";
import { cn } from "@/shared/lib/cn";

function today() {
  return new Date().toISOString().slice(0, 10);
}

function money(n: number) {
  return n.toLocaleString("vi-VN");
}

export default function RewardsPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.employee.read");
  const canManage = can("hrm.employee.manage");

  const [rows, setRows] = useState<RewardDisciplineDto[]>([]);
  const [report, setReport] = useState<RewardDisciplineReportRowDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [kindFilter, setKindFilter] = useState("");

  const [employeeId, setEmployeeId] = useState("");
  const [kind, setKind] = useState("Reward");
  const [title, setTitle] = useState("");
  const [decisionDate, setDecisionDate] = useState(today());
  const [reason, setReason] = useState("");
  const [impactAmt, setImpactAmt] = useState("0");
  const [impactKind, setImpactKind] = useState("None");

  async function load() {
    setError(null);
    try {
      const [r, rep, emp] = await Promise.all([
        fetchRewardDecisions(kindFilter || undefined),
        fetchRewardReport(),
        fetchEmployees(),
      ]);
      setRows(r);
      setReport(rep);
      setEmployees(emp);
      if (!employeeId && emp[0]) setEmployeeId(emp[0].id);
    } catch {
      setError("Không tải được dữ liệu khen thưởng / kỷ luật.");
    }
  }

  useEffect(() => {
    if (canRead) void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead, kindFilter]);

  async function onCreate(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    try {
      await createRewardDecision({
        employeeId,
        kind,
        title,
        decisionDate,
        reason,
        payrollImpactAmount: Number(impactAmt) || 0,
        payrollImpactKind: impactKind,
      });
      setTitle("");
      setReason("");
      setOk("Đã ghi nhận quyết định.");
      await load();
    } catch {
      setError("Tạo quyết định thất bại.");
    }
  }

  if (!canRead) return <p className="text-body text-destructive">Không có quyền xem.</p>;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Khen thưởng / Kỷ luật</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Ghi nhận quyết định · đính kèm file · áp dụng ảnh hưởng lương · báo cáo
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <select className={cn(field.select, "w-44")} value={kindFilter} onChange={(e) => setKindFilter(e.target.value)}>
            <option value="">Tất cả</option>
            <option value="Reward">Khen thưởng</option>
            <option value="Discipline">Kỷ luật</option>
          </select>
          <button type="button" className={btn.ghost} onClick={() => void load()}>
            Làm mới
          </button>
        </div>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-success">{ok}</p>}

      {canManage && (
        <section className={panel}>
          <h2 className="mb-3 font-display text-lead font-bold">Ghi nhận quyết định</h2>
          <form onSubmit={onCreate} className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            <div>
              <label className={field.label}>Nhân viên</label>
              <select className={field.select} value={employeeId} onChange={(e) => setEmployeeId(e.target.value)}>
                {employees.map((e) => (
                  <option key={e.id} value={e.id}>
                    {e.employeeCode} — {e.fullName}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className={field.label}>Loại</label>
              <select className={field.select} value={kind} onChange={(e) => setKind(e.target.value)}>
                <option value="Reward">Khen thưởng</option>
                <option value="Discipline">Kỷ luật</option>
              </select>
            </div>
            <div>
              <label className={field.label}>Ngày quyết định</label>
              <input type="date" className={field.input} value={decisionDate} onChange={(e) => setDecisionDate(e.target.value)} />
            </div>
            <div className="sm:col-span-2">
              <label className={field.label}>Tiêu đề</label>
              <input className={field.input} value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Tiêu đề" required />
            </div>
            <div>
              <label className={field.label}>Lý do</label>
              <input className={field.input} value={reason} onChange={(e) => setReason(e.target.value)} placeholder="Lý do" />
            </div>
            <div>
              <label className={field.label}>Ảnh hưởng lương</label>
              <select className={field.select} value={impactKind} onChange={(e) => setImpactKind(e.target.value)}>
                <option value="None">Không ảnh hưởng lương</option>
                <option value="Bonus">Thưởng (+)</option>
                <option value="Allowance">Phụ cấp (+)</option>
                <option value="Deduction">Khấu trừ (−)</option>
              </select>
            </div>
            <div>
              <label className={field.label}>Số tiền</label>
              <input className={field.input} value={impactAmt} onChange={(e) => setImpactAmt(e.target.value)} placeholder="Số tiền" />
            </div>
            <div className="flex items-end">
              <button type="submit" className={btn.primary}>
                Ghi nhận
              </button>
            </div>
          </form>
        </section>
      )}

      <div className={tableWrap}>
        <table className="w-full text-body">
          <thead className="border-b border-border bg-muted">
            <tr>
              {["Loại", "NV", "Tiêu đề", "Ngày", "Ảnh hưởng", "File", "TT", ""].map((h) => (
                <th key={h || "act"} className={th}>
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.length === 0 ? (
              <tr>
                <td colSpan={8} className="px-3 py-8 text-center text-muted-foreground">
                  Chưa có quyết định.
                </td>
              </tr>
            ) : (
              rows.map((r) => (
                <tr key={r.id} className="border-t border-border">
                  <td className={td}>
                    <span className={statusPill(r.kind === "Reward" ? "success" : "danger")}>
                      {r.kind === "Reward" ? "Khen thưởng" : "Kỷ luật"}
                    </span>
                  </td>
                  <td className={td}>
                    <span className="font-mono text-brand-strong">{r.employeeCode}</span>
                    <span className="text-muted-foreground"> · {r.employeeName}</span>
                  </td>
                  <td className={td}>{r.title}</td>
                  <td className={cn(td, "tabular-nums text-muted-foreground")}>{r.decisionDate}</td>
                  <td className={td}>
                    {r.payrollImpactKind === "None" ? (
                      <span className="text-muted-foreground">—</span>
                    ) : (
                      <span className="tabular-nums">
                        {r.payrollImpactKind} {money(r.payrollImpactAmount)}
                      </span>
                    )}
                  </td>
                  <td className={td}>{r.decisionStorageKey ? "✓" : "—"}</td>
                  <td className={td}>
                    <span className={statusPill(r.status === "Applied" ? "success" : "brand")}>{r.status}</span>
                  </td>
                  <td className={cn(td, "whitespace-nowrap")}>
                    {canManage && (
                      <div className="flex flex-wrap gap-1.5">
                        <label className={cn(btn.soft, "cursor-pointer")}>
                          File
                          <input
                            type="file"
                            hidden
                            onChange={async (ev) => {
                              const f = ev.target.files?.[0];
                              if (!f) return;
                              try {
                                const up = await uploadHrmFile(f);
                                await attachRewardDecision(r.id, up.storageKey);
                                setOk("Đã đính kèm.");
                                await load();
                              } catch {
                                setError("Upload thất bại.");
                              }
                            }}
                          />
                        </label>
                        {r.payrollImpactKind !== "None" && r.status !== "Applied" && (
                          <button
                            type="button"
                            className={btn.soft}
                            onClick={async () => {
                              try {
                                await applyRewardToPayroll(r.id);
                                setOk("Đã áp dụng vào kỳ lương mở.");
                                await load();
                              } catch {
                                setError("Áp dụng lương thất bại (cần kỳ mở).");
                              }
                            }}
                          >
                            → Lương
                          </button>
                        )}
                      </div>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <section className={panel}>
        <h2 className="mb-3 font-display text-lead font-bold">Báo cáo năm</h2>
        {report.length === 0 ? (
          <p className="text-body text-muted-foreground">Chưa có dữ liệu báo cáo.</p>
        ) : (
          <div className="grid gap-2 sm:grid-cols-2">
            {report.map((x) => (
              <div key={x.kind} className="rounded-lg border border-border/70 px-3 py-2">
                <div className="font-semibold">{x.kind}</div>
                <div className="text-meta text-muted-foreground">
                  {x.count} quyết định · tổng ảnh hưởng {money(x.totalImpact)}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
