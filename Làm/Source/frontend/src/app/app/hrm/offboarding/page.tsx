"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  approveOffboarding,
  completeOffboarding,
  createOffboardingCase,
  fetchEmployees,
  fetchOffboardingCases,
  fetchOffboardingReport,
  fetchOffboardingSettings,
  rejectOffboarding,
  revokeOffboardingAccess,
  saveOffboardingInterview,
  settleOffboarding,
  submitOffboarding,
  updateOffboardingChecklist,
  upsertOffboardingSettings,
  type EmployeeDto,
  type OffboardingCaseDto,
  type OffboardingReportRowDto,
  type OffboardingSettingDto,
} from "@/shared/api/hrm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, td, th } from "@/shared/ui/field";
import { cn } from "@/shared/lib/cn";

function today() {
  return new Date().toISOString().slice(0, 10);
}

function money(n: number) {
  return n.toLocaleString("vi-VN");
}

function caseTone(status: string) {
  const s = status.toLowerCase();
  if (s.includes("complete")) return "success" as const;
  if (s.includes("reject")) return "danger" as const;
  if (s.includes("approv") || s.includes("progress")) return "brand" as const;
  if (s.includes("submit")) return "warning" as const;
  return "muted" as const;
}

export default function OffboardingPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.employee.read");
  const canManage = can("hrm.employee.manage");

  const [settings, setSettings] = useState<OffboardingSettingDto>({
    noticeDays: 30,
    requireChecklistComplete: true,
    autoRevokeAccessOnComplete: true,
  });
  const [cases, setCases] = useState<OffboardingCaseDto[]>([]);
  const [report, setReport] = useState<OffboardingReportRowDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [employeeId, setEmployeeId] = useState("");
  const [requestDate, setRequestDate] = useState(today());
  const [lastDay, setLastDay] = useState(today());
  const [reasonCode, setReasonCode] = useState("Personal");
  const [reasonDetail, setReasonDetail] = useState("");

  async function load() {
    setError(null);
    try {
      const [s, c, r, emp] = await Promise.all([
        fetchOffboardingSettings(),
        fetchOffboardingCases(),
        fetchOffboardingReport(),
        fetchEmployees(),
      ]);
      setSettings(s);
      setCases(c);
      setReport(r);
      setEmployees(emp);
      if (!employeeId && emp[0]) setEmployeeId(emp[0].id);
    } catch {
      setError("Không tải được dữ liệu nghỉ việc.");
    }
  }

  useEffect(() => {
    if (canRead) void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  async function onCreate(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    try {
      await createOffboardingCase({
        employeeId,
        requestDate,
        lastWorkingDay: lastDay,
        reasonCode,
        reasonDetail,
      });
      setOk("Đã tạo đơn nghỉ việc.");
      await load();
    } catch {
      setError("Tạo đơn thất bại.");
    }
  }

  if (!canRead) return <p className="text-body text-destructive">Không có quyền xem.</p>;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Nghỉ việc / Offboarding</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Đơn nghỉ · báo trước · duyệt · checklist · thu quyền · quyết toán · hoàn tất
          </p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void load()}>
          Làm mới
        </button>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-success">{ok}</p>}

      {canManage && (
        <section className={panel}>
          <h2 className="mb-3 font-display text-lead font-bold">Cấu hình báo trước</h2>
          <form
            onSubmit={async (e) => {
              e.preventDefault();
              try {
                await upsertOffboardingSettings(settings);
                setOk("Đã lưu cấu hình báo trước.");
              } catch {
                setError("Lưu cấu hình thất bại.");
              }
            }}
            className="flex flex-wrap items-end gap-4"
          >
            <div>
              <label className={field.label}>Ngày báo trước</label>
              <input
                type="number"
                className={cn(field.input, "w-24")}
                value={settings.noticeDays}
                onChange={(e) => setSettings({ ...settings, noticeDays: Number(e.target.value) })}
              />
            </div>
            <label className="flex items-center gap-2 text-body">
              <input
                type="checkbox"
                className={field.check}
                checked={settings.requireChecklistComplete}
                onChange={(e) => setSettings({ ...settings, requireChecklistComplete: e.target.checked })}
              />
              Bắt buộc checklist
            </label>
            <label className="flex items-center gap-2 text-body">
              <input
                type="checkbox"
                className={field.check}
                checked={settings.autoRevokeAccessOnComplete}
                onChange={(e) => setSettings({ ...settings, autoRevokeAccessOnComplete: e.target.checked })}
              />
              Auto thu quyền khi hoàn tất
            </label>
            <button type="submit" className={btn.primary}>
              Lưu cấu hình
            </button>
          </form>
        </section>
      )}

      {canManage && (
        <section className={panel}>
          <h2 className="mb-3 font-display text-lead font-bold">Tạo đơn nghỉ việc</h2>
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
              <label className={field.label}>Ngày yêu cầu</label>
              <input type="date" className={field.input} value={requestDate} onChange={(e) => setRequestDate(e.target.value)} />
            </div>
            <div>
              <label className={field.label}>Ngày làm cuối</label>
              <input type="date" className={field.input} value={lastDay} onChange={(e) => setLastDay(e.target.value)} />
            </div>
            <div>
              <label className={field.label}>Lý do</label>
              <select className={field.select} value={reasonCode} onChange={(e) => setReasonCode(e.target.value)}>
                <option value="Personal">Cá nhân</option>
                <option value="BetterOffer">Công việc mới</option>
                <option value="Relocation">Chuyển chỗ</option>
                <option value="Performance">Hiệu suất</option>
                <option value="Other">Khác</option>
              </select>
            </div>
            <div className="sm:col-span-2">
              <label className={field.label}>Chi tiết</label>
              <input className={field.input} value={reasonDetail} onChange={(e) => setReasonDetail(e.target.value)} placeholder="Chi tiết" />
            </div>
            <div className="flex items-end">
              <button type="submit" className={btn.primary}>
                Tạo đơn
              </button>
            </div>
          </form>
        </section>
      )}

      <div className="space-y-3">
        {cases.length === 0 ? (
          <p className="rounded-xl border border-border bg-surface py-8 text-center text-muted-foreground">
            Chưa có hồ sơ nghỉ việc.
          </p>
        ) : (
          cases.map((c) => (
            <article key={c.id} className={panel}>
              <header className="flex flex-wrap items-center gap-2">
                <strong className="text-foreground">
                  <span className="font-mono text-brand-strong">{c.employeeCode}</span> · {c.employeeName}
                </strong>
                <span className="text-meta text-muted-foreground">{c.orgUnitName}</span>
                <span className={statusPill(caseTone(c.status))}>{c.status}</span>
                <span className={statusPill(c.noticeSatisfied ? "success" : "warning")}>
                  Báo trước {c.noticeSatisfied ? "đủ" : "không đủ"} ({c.requiredNoticeDays} ngày)
                </span>
                <span className="text-meta text-muted-foreground">Nghỉ cuối: {c.lastWorkingDay}</span>
                <span className="text-meta text-muted-foreground">Lý do: {c.reasonCode}</span>
              </header>

              {canManage && (
                <div className="mt-3 flex flex-wrap gap-1.5">
                  {c.status === "Draft" && (
                    <button
                      type="button"
                      className={btn.soft}
                      onClick={async () => {
                        await submitOffboarding(c.id);
                        await load();
                      }}
                    >
                      Nộp duyệt
                    </button>
                  )}
                  {c.status === "Submitted" && (
                    <>
                      <button
                        type="button"
                        className={btn.primary}
                        onClick={async () => {
                          await approveOffboarding(c.id);
                          setOk("Đã duyệt.");
                          await load();
                        }}
                      >
                        Duyệt
                      </button>
                      <button
                        type="button"
                        className={btn.danger}
                        onClick={async () => {
                          await rejectOffboarding(c.id, "Từ chối");
                          await load();
                        }}
                      >
                        Từ chối
                      </button>
                    </>
                  )}
                  {(c.status === "Approved" || c.status === "InProgress") && (
                    <>
                      {!c.accessRevoked && (
                        <button
                          type="button"
                          className={btn.soft}
                          onClick={async () => {
                            await revokeOffboardingAccess(c.id);
                            setOk("Đã thu hồi quyền.");
                            await load();
                          }}
                        >
                          Thu quyền
                        </button>
                      )}
                      <button
                        type="button"
                        className={btn.soft}
                        onClick={async () => {
                          const leaveAmt = prompt("Tiền quyết toán phép", "0");
                          const finalPay = prompt("Ước tính lương cuối", "0");
                          if (leaveAmt == null) return;
                          await settleOffboarding(c.id, {
                            leaveSettlementAmount: Number(leaveAmt) || 0,
                            finalPayEstimate: Number(finalPay) || 0,
                            settlementNote: "Day-1 settle",
                          });
                          setOk("Đã quyết toán.");
                          await load();
                        }}
                      >
                        Quyết toán
                      </button>
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={async () => {
                          const notes = prompt("Phỏng vấn nghỉ việc", c.interviewNotes ?? "");
                          if (notes == null) return;
                          await saveOffboardingInterview(c.id, notes);
                          await load();
                        }}
                      >
                        PV nghỉ
                      </button>
                      <button
                        type="button"
                        className={btn.primary}
                        onClick={async () => {
                          try {
                            await completeOffboarding(c.id);
                            setOk("Hoàn tất offboarding.");
                            await load();
                          } catch {
                            setError("Hoàn tất thất bại (checklist?).");
                          }
                        }}
                      >
                        Hoàn tất
                      </button>
                    </>
                  )}
                </div>
              )}

              <ul className="mt-3 grid gap-1.5 sm:grid-cols-2">
                {c.checklist.map((item, idx) => (
                  <li key={item.key}>
                    <label className="flex items-center gap-2 rounded-md px-2 py-1.5 text-body hover:bg-muted/60">
                      <input
                        type="checkbox"
                        className={field.check}
                        checked={item.done}
                        disabled={!canManage || c.status === "Completed" || c.status === "Rejected"}
                        onChange={async (e) => {
                          const next = c.checklist.map((x, i) =>
                            i === idx ? { ...x, done: e.target.checked } : x
                          );
                          await updateOffboardingChecklist(c.id, next);
                          await load();
                        }}
                      />
                      <span className={item.done ? "text-muted-foreground line-through" : ""}>{item.label}</span>
                    </label>
                  </li>
                ))}
              </ul>

              {(c.leaveDaysRemaining != null || c.finalPayEstimate != null) && (
                <p className="mt-2 text-meta text-muted-foreground">
                  Phép còn: {c.leaveDaysRemaining ?? "—"} ngày
                  {c.leaveSettlementAmount != null ? ` · QT phép ${money(c.leaveSettlementAmount)}` : ""}
                  {c.finalPayEstimate != null ? ` · Lương cuối ~${money(c.finalPayEstimate)}` : ""}
                  {c.accessRevoked ? " · Đã thu quyền" : ""}
                </p>
              )}
              {c.interviewNotes && <p className="mt-1 text-meta text-foreground">PV: {c.interviewNotes}</p>}
            </article>
          ))
        )}
      </div>

      <section className={panel}>
        <h2 className="mb-3 font-display text-lead font-bold">Báo cáo theo lý do</h2>
        {report.length === 0 ? (
          <p className="text-muted-foreground">Chưa có dữ liệu.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-body">
              <thead className="border-b border-border bg-muted/50">
                <tr>
                  <th className={th}>Lý do</th>
                  <th className={th}>Số lượng</th>
                </tr>
              </thead>
              <tbody>
                {report.map((r) => (
                  <tr key={r.reasonCode} className="border-t border-border">
                    <td className={td}>{r.reasonCode}</td>
                    <td className={cn(td, "tabular-nums font-semibold")}>{r.count}</td>
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
