"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  addPayrollAdjustment,
  calculatePayrollPeriod,
  confirmPayrollPeriod,
  createPayrollPeriod,
  fetchAllowanceRules,
  fetchAllowanceTypes,
  fetchEmployeeSalaries,
  fetchEmployees,
  fetchMyPayslip,
  fetchPayrollAdjustments,
  fetchPayrollCompare,
  fetchPayrollCostByOrg,
  fetchPayrollLines,
  fetchPayrollPeriods,
  fetchPayrollPolicy,
  fetchSalaryGrades,
  lockPayrollPeriod,
  patchPayrollLine,
  payrollExportBankUrl,
  payrollExportUrl,
  upsertAllowanceRule,
  upsertAllowanceType,
  upsertEmployeeSalary,
  upsertPayrollPolicy,
  upsertSalaryGrade,
  type AllowanceRuleDto,
  type AllowanceTypeDto,
  type EmployeeDto,
  type EmployeeSalaryDto,
  type PayrollAdjustmentDto,
  type PayrollCompareDto,
  type PayrollCostByOrgDto,
  type PayrollLineDto,
  type PayrollPeriodDto,
  type PayrollPolicyDto,
  type SalaryGradeDto,
} from "@/shared/api/hrm-api";
import { api } from "@/shared/api/client";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";
import { cn } from "@/shared/lib/cn";

function monthKey() {
  return new Date().toISOString().slice(0, 7);
}

function money(n: number) {
  return n.toLocaleString("vi-VN");
}

function periodTone(status: string) {
  const s = status.toLowerCase();
  if (s.includes("lock")) return "muted" as const;
  if (s.includes("confirm")) return "success" as const;
  if (s.includes("calc") || s.includes("draft") || s.includes("open")) return "brand" as const;
  return "warning" as const;
}

function Panel({
  title,
  hint,
  children,
}: {
  title: string;
  hint?: string;
  children: React.ReactNode;
}) {
  return (
    <section className={panel}>
      <div className="mb-3 flex items-baseline justify-between gap-2">
        <h2 className="font-display text-lead font-bold text-foreground">{title}</h2>
        {hint ? <span className="text-meta text-muted-foreground">{hint}</span> : null}
      </div>
      {children}
    </section>
  );
}

export default function PayrollPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.payroll.read");
  const canManage = can("hrm.payroll.manage");

  const [tab, setTab] = useState<"run" | "config" | "mine" | "report">("run");
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const [grades, setGrades] = useState<SalaryGradeDto[]>([]);
  const [salaries, setSalaries] = useState<EmployeeSalaryDto[]>([]);
  const [allowTypes, setAllowTypes] = useState<AllowanceTypeDto[]>([]);
  const [allowRules, setAllowRules] = useState<AllowanceRuleDto[]>([]);
  const [policy, setPolicy] = useState<PayrollPolicyDto | null>(null);
  const [periods, setPeriods] = useState<PayrollPeriodDto[]>([]);
  const [periodId, setPeriodId] = useState("");
  const [lines, setLines] = useState<PayrollLineDto[]>([]);
  const [adjs, setAdjs] = useState<PayrollAdjustmentDto[]>([]);
  const [mine, setMine] = useState<PayrollLineDto[]>([]);
  const [cost, setCost] = useState<PayrollCostByOrgDto[]>([]);
  const [compare, setCompare] = useState<PayrollCompareDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);

  const [newPeriodKey, setNewPeriodKey] = useState(monthKey());
  const [gradeCode, setGradeCode] = useState("");
  const [gradeName, setGradeName] = useState("");
  const [gradeLevel, setGradeLevel] = useState("1");
  const [gradeAmount, setGradeAmount] = useState("10000000");
  const [salEmpId, setSalEmpId] = useState("");
  const [salGradeId, setSalGradeId] = useState("");
  const [salBase, setSalBase] = useState("10000000");
  const [salStatus, setSalStatus] = useState("");
  const [salFrom, setSalFrom] = useState(monthKey() + "-01");
  const [allowCode, setAllowCode] = useState("");
  const [allowName, setAllowName] = useState("");
  const [allowAmt, setAllowAmt] = useState("500000");
  const [ruleTypeId, setRuleTypeId] = useState("");
  const [ruleShift, setRuleShift] = useState("");
  const [ruleAmt, setRuleAmt] = useState("100000");
  const [adjEmpId, setAdjEmpId] = useState("");
  const [adjKind, setAdjKind] = useState("Bonus");
  const [adjTitle, setAdjTitle] = useState("");
  const [adjAmt, setAdjAmt] = useState("0");

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [g, s, at, ar, pol, per, emp, my] = await Promise.all([
        fetchSalaryGrades(),
        fetchEmployeeSalaries(),
        fetchAllowanceTypes(),
        fetchAllowanceRules(),
        fetchPayrollPolicy(),
        fetchPayrollPeriods(),
        fetchEmployees(),
        fetchMyPayslip(),
      ]);
      setGrades(g);
      setSalaries(s);
      setAllowTypes(at);
      setAllowRules(ar);
      setPolicy(pol);
      setPeriods(per);
      setEmployees(emp);
      setMine(my);
      if (!periodId && per[0]) setPeriodId(per[0].id);
      if (!salEmpId && emp[0]) setSalEmpId(emp[0].id);
      if (!adjEmpId && emp[0]) setAdjEmpId(emp[0].id);
      if (!salGradeId && g[0]) setSalGradeId(g[0].id);
      if (!ruleTypeId && at[0]) setRuleTypeId(at[0].id);
    } catch {
      setError("Không tải được dữ liệu lương kỳ.");
    } finally {
      setLoading(false);
    }
  }

  async function loadPeriodDetail(id: string) {
    if (!id) {
      setLines([]);
      setAdjs([]);
      setCost([]);
      setCompare([]);
      return;
    }
    try {
      const per = periods.find((p) => p.id === id);
      const [l, a, c, cmp] = await Promise.all([
        fetchPayrollLines(id),
        fetchPayrollAdjustments(id),
        fetchPayrollCostByOrg(id),
        per ? fetchPayrollCompare(per.periodKey) : Promise.resolve([] as PayrollCompareDto[]),
      ]);
      setLines(l);
      setAdjs(a);
      setCost(c);
      setCompare(cmp);
    } catch {
      setError("Không tải được bảng lương kỳ.");
    }
  }

  useEffect(() => {
    if (canRead) void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  useEffect(() => {
    if (periodId) void loadPeriodDetail(periodId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [periodId, periods]);

  async function onCreatePeriod(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    setError(null);
    try {
      const p = await createPayrollPeriod(newPeriodKey);
      setOk(`Đã tạo kỳ ${p.periodKey}`);
      await load();
      setPeriodId(p.id);
    } catch {
      setError("Tạo kỳ lương thất bại.");
    }
  }

  async function onCalculate() {
    if (!canManage || !periodId) return;
    setError(null);
    try {
      await calculatePayrollPeriod(periodId);
      setOk("Đã tính lương.");
      await load();
    } catch {
      setError("Tính lương thất bại.");
    }
  }

  async function onConfirm() {
    if (!canManage || !periodId) return;
    try {
      await confirmPayrollPeriod(periodId);
      setOk("Đã xác nhận bảng lương.");
      await load();
    } catch {
      setError("Xác nhận thất bại.");
    }
  }

  async function onLock() {
    if (!canManage || !periodId) return;
    try {
      await lockPayrollPeriod(periodId);
      setOk("Đã khóa kỳ lương.");
      await load();
    } catch {
      setError("Khóa kỳ thất bại.");
    }
  }

  async function download(url: string, name: string) {
    try {
      const res = await api.get(url, { responseType: "blob" });
      const blob = new Blob([res.data], { type: "text/csv" });
      const a = document.createElement("a");
      a.href = URL.createObjectURL(blob);
      a.download = name;
      a.click();
      URL.revokeObjectURL(a.href);
    } catch {
      setError("Xuất file thất bại.");
    }
  }

  const tabs = [
    { id: "run" as const, label: "Kỳ lương" },
    { id: "config" as const, label: "Cấu hình" },
    { id: "mine" as const, label: "Phiếu của tôi" },
    { id: "report" as const, label: "Báo cáo" },
  ];

  if (!canRead) {
    return <p className="text-body text-destructive">Bạn không có quyền xem lương kỳ.</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Lương kỳ</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Thang bậc · phụ cấp · BH/thuế · tính kỳ · phiếu lương · xuất CSV
          </p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void load()}>
          Làm mới
        </button>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-success">{ok}</p>}
      {loading && <p className="text-body text-muted-foreground">Đang tải…</p>}

      <div className="flex flex-wrap gap-1 border-b border-border">
        {tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={cn(
              "relative -mb-px px-3 py-2 text-body font-semibold transition-colors",
              tab === t.id
                ? "border-b-2 border-brand text-brand-strong"
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === "run" && (
        <div className="space-y-4">
          <Panel title="Chạy kỳ lương" hint={periods.length ? `${periods.length} kỳ` : undefined}>
            <div className="flex flex-wrap items-end gap-2">
              {canManage && (
                <form onSubmit={onCreatePeriod} className="flex flex-wrap items-end gap-2">
                  <div>
                    <label className={field.label}>Kỳ (yyyy-MM)</label>
                    <input
                      className={cn(field.input, "w-36")}
                      value={newPeriodKey}
                      onChange={(e) => setNewPeriodKey(e.target.value)}
                      placeholder="yyyy-MM"
                    />
                  </div>
                  <button type="submit" className={btn.primary}>
                    Tạo kỳ
                  </button>
                </form>
              )}
              <div className="min-w-[220px] flex-1">
                <label className={field.label}>Chọn kỳ</label>
                <select
                  className={field.select}
                  value={periodId}
                  onChange={(e) => setPeriodId(e.target.value)}
                >
                  <option value="">— chọn kỳ —</option>
                  {periods.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.periodKey} · {p.status} · {p.lineCount} dòng · net {money(p.totalNet)}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className="mt-3 flex flex-wrap gap-2">
              {canManage && (
                <>
                  <button type="button" className={btn.primary} onClick={() => void onCalculate()}>
                    Tính lương
                  </button>
                  <button type="button" className={btn.secondary} onClick={() => void onConfirm()}>
                    Xác nhận
                  </button>
                  <button type="button" className={btn.ghost} onClick={() => void onLock()}>
                    Khóa kỳ
                  </button>
                </>
              )}
              {periodId && (
                <>
                  <button
                    type="button"
                    className={btn.soft}
                    onClick={() => void download(payrollExportUrl(periodId), "payroll.csv")}
                  >
                    Xuất bảng lương
                  </button>
                  <button
                    type="button"
                    className={btn.soft}
                    onClick={() => void download(payrollExportBankUrl(periodId), "payroll-bank.csv")}
                  >
                    Xuất ngân hàng
                  </button>
                </>
              )}
            </div>

            {periodId && periods.find((p) => p.id === periodId) && (
              <div className="mt-3">
                <span className={statusPill(periodTone(periods.find((p) => p.id === periodId)!.status))}>
                  {periods.find((p) => p.id === periodId)!.status}
                </span>
              </div>
            )}
          </Panel>

          {canManage && periodId && (
            <Panel title="Điều chỉnh kỳ">
              <form
                onSubmit={async (e) => {
                  e.preventDefault();
                  try {
                    await addPayrollAdjustment({
                      payrollPeriodId: periodId,
                      employeeId: adjEmpId,
                      kind: adjKind,
                      title: adjTitle,
                      amount: Number(adjAmt) || 0,
                    });
                    setAdjTitle("");
                    setOk("Đã thêm điều chỉnh.");
                    await load();
                  } catch {
                    setError("Thêm điều chỉnh thất bại.");
                  }
                }}
                className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5"
              >
                <div>
                  <label className={field.label}>Nhân viên</label>
                  <select className={field.select} value={adjEmpId} onChange={(e) => setAdjEmpId(e.target.value)}>
                    {employees.map((e) => (
                      <option key={e.id} value={e.id}>
                        {e.employeeCode} — {e.fullName}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={field.label}>Loại</label>
                  <select className={field.select} value={adjKind} onChange={(e) => setAdjKind(e.target.value)}>
                    <option value="Bonus">Thưởng</option>
                    <option value="Allowance">Phụ cấp PS</option>
                    <option value="Deduction">Khấu trừ</option>
                    <option value="Advance">Tạm ứng</option>
                  </select>
                </div>
                <div>
                  <label className={field.label}>Tiêu đề</label>
                  <input
                    className={field.input}
                    value={adjTitle}
                    onChange={(e) => setAdjTitle(e.target.value)}
                    placeholder="Tiêu đề"
                    required
                  />
                </div>
                <div>
                  <label className={field.label}>Số tiền</label>
                  <input className={field.input} value={adjAmt} onChange={(e) => setAdjAmt(e.target.value)} />
                </div>
                <div className="flex items-end">
                  <button type="submit" className={btn.primary}>
                    Thêm ĐC
                  </button>
                </div>
              </form>

              {adjs.length > 0 && (
                <ul className="mt-3 space-y-1 text-body text-muted-foreground">
                  {adjs.map((a) => (
                    <li key={a.id}>
                      {a.employeeName} · {a.kind} · {a.title}:{" "}
                      <span className="font-semibold tabular-nums text-foreground">{money(a.amount)}</span>
                    </li>
                  ))}
                </ul>
              )}
            </Panel>
          )}

          <div className={tableWrap}>
            <table className="w-full min-w-[960px] text-body">
              <thead className="border-b border-border bg-muted">
                <tr>
                  {[
                    "Mã",
                    "Họ tên",
                    "Công",
                    "OT'",
                    "LCB",
                    "Công TT",
                    "OT",
                    "PC",
                    "Thưởng",
                    "Trừ",
                    "BH",
                    "Thuế",
                    "Gross",
                    "Net",
                    "",
                  ].map((h) => (
                    <th key={h || "act"} className={th}>
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {lines.length === 0 ? (
                  <tr>
                    <td colSpan={15} className="px-3 py-8 text-center text-muted-foreground">
                      Chưa có dòng lương — chọn kỳ và tính lương.
                    </td>
                  </tr>
                ) : (
                  lines.map((l) => (
                    <tr key={l.id} className="border-t border-border">
                      <td className={cn(td, "font-mono text-brand-strong")}>{l.employeeCode}</td>
                      <td className={td}>{l.employeeName}</td>
                      <td className={cn(td, "tabular-nums")}>{l.workUnits}</td>
                      <td className={cn(td, "tabular-nums")}>{l.otMinutes}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.baseSalary)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.attendancePay)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.otPay)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.allowanceTotal)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.bonus)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.deductionTotal)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.insuranceEmployee)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.tax)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.grossPay)}</td>
                      <td className={cn(td, "font-semibold tabular-nums")}>{money(l.netPay)}</td>
                      <td className={td}>
                        {canManage && !l.isConfirmed && (
                          <button
                            type="button"
                            className={btn.soft}
                            onClick={async () => {
                              const bonus = prompt("Thưởng", String(l.bonus));
                              if (bonus == null) return;
                              try {
                                await patchPayrollLine(l.id, { bonus: Number(bonus) || 0 });
                                await loadPeriodDetail(periodId);
                              } catch {
                                setError("Sửa dòng thất bại.");
                              }
                            }}
                          >
                            Sửa
                          </button>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {tab === "config" && (
        <div className="grid gap-4 lg:grid-cols-2">
          {policy && canManage && (
            <Panel title="BH · thuế · ngày công">
              <form
                onSubmit={async (e) => {
                  e.preventDefault();
                  try {
                    await upsertPayrollPolicy(policy);
                    setOk("Đã lưu chính sách BH/thuế.");
                  } catch {
                    setError("Lưu policy thất bại.");
                  }
                }}
                className="grid gap-3 sm:grid-cols-2"
              >
                {(
                  [
                    ["BHXH NV", "socialInsuranceEmpRate", 0.001],
                    ["BHYT NV", "healthInsuranceEmpRate", 0.001],
                    ["BHTN NV", "unemploymentEmpRate", 0.001],
                    ["Giảm trừ bản thân", "personalDeduction", 1],
                    ["Thuế flat", "flatTaxRate", 0.01],
                    ["Ngày công chuẩn", "standardWorkDays", 1],
                    ["Hệ số OT", "otMultiplier", 0.1],
                  ] as const
                ).map(([label, key, step]) => (
                  <div key={key}>
                    <label className={field.label}>{label}</label>
                    <input
                      type="number"
                      step={step}
                      className={field.input}
                      value={policy[key] as number}
                      onChange={(e) => setPolicy({ ...policy, [key]: Number(e.target.value) })}
                    />
                  </div>
                ))}
                <div className="sm:col-span-2">
                  <button type="submit" className={btn.primary}>
                    Lưu policy
                  </button>
                </div>
              </form>
            </Panel>
          )}

          <Panel title="Thang bậc">
            {canManage && (
              <form
                onSubmit={async (e) => {
                  e.preventDefault();
                  try {
                    await upsertSalaryGrade({
                      code: gradeCode,
                      name: gradeName,
                      level: Number(gradeLevel) || 1,
                      baseAmount: Number(gradeAmount) || 0,
                      isActive: true,
                    });
                    setGradeCode("");
                    setGradeName("");
                    setOk("Đã lưu bậc lương.");
                    await load();
                  } catch {
                    setError("Lưu bậc thất bại.");
                  }
                }}
                className="mb-3 grid gap-2 sm:grid-cols-2"
              >
                <input className={field.input} value={gradeCode} onChange={(e) => setGradeCode(e.target.value)} placeholder="Mã" required />
                <input className={field.input} value={gradeName} onChange={(e) => setGradeName(e.target.value)} placeholder="Tên" required />
                <input className={field.input} value={gradeLevel} onChange={(e) => setGradeLevel(e.target.value)} placeholder="Bậc" />
                <input className={field.input} value={gradeAmount} onChange={(e) => setGradeAmount(e.target.value)} placeholder="Mức" />
                <button type="submit" className={btn.primary}>
                  Thêm bậc
                </button>
              </form>
            )}
            <ul className="space-y-1.5 text-body">
              {grades.map((g) => (
                <li key={g.id} className="flex justify-between gap-2 border-b border-border/60 py-1.5 last:border-0">
                  <span>
                    <span className="font-mono text-brand-strong">{g.code}</span> · {g.name} · L{g.level}
                  </span>
                  <span className="tabular-nums text-muted-foreground">{money(g.baseAmount)}</span>
                </li>
              ))}
            </ul>
          </Panel>

          <Panel title="Lương thực tế NV">
            {canManage && (
              <form
                onSubmit={async (e) => {
                  e.preventDefault();
                  try {
                    await upsertEmployeeSalary({
                      employeeId: salEmpId,
                      salaryGradeId: salGradeId || null,
                      baseSalary: Number(salBase) || 0,
                      appliesToStatus: salStatus || null,
                      effectiveFrom: salFrom,
                      isActive: true,
                    });
                    setOk("Đã gán lương NV.");
                    await load();
                  } catch {
                    setError("Gán lương thất bại.");
                  }
                }}
                className="mb-3 grid gap-2 sm:grid-cols-2"
              >
                <select className={field.select} value={salEmpId} onChange={(e) => setSalEmpId(e.target.value)}>
                  {employees.map((e) => (
                    <option key={e.id} value={e.id}>
                      {e.employeeCode} — {e.fullName}
                    </option>
                  ))}
                </select>
                <select className={field.select} value={salGradeId} onChange={(e) => setSalGradeId(e.target.value)}>
                  <option value="">— bậc —</option>
                  {grades.map((g) => (
                    <option key={g.id} value={g.id}>
                      {g.code}
                    </option>
                  ))}
                </select>
                <input className={field.input} value={salBase} onChange={(e) => setSalBase(e.target.value)} placeholder="Lương CB" />
                <input className={field.input} value={salStatus} onChange={(e) => setSalStatus(e.target.value)} placeholder="TT (Active/Probation…)" />
                <input type="date" className={field.input} value={salFrom} onChange={(e) => setSalFrom(e.target.value)} />
                <button type="submit" className={btn.primary}>
                  Gán
                </button>
              </form>
            )}
            <ul className="max-h-64 space-y-1.5 overflow-y-auto text-body">
              {salaries.slice(0, 20).map((s) => (
                <li key={s.id} className="border-b border-border/60 py-1.5 last:border-0">
                  <span className="font-mono text-brand-strong">{s.employeeCode}</span> · {s.employeeName}:{" "}
                  <span className="tabular-nums font-semibold">{money(s.baseSalary)}</span>
                  {s.appliesToStatus ? ` [${s.appliesToStatus}]` : ""} từ {s.effectiveFrom}
                </li>
              ))}
            </ul>
          </Panel>

          <Panel title="Phụ cấp & rule theo ca">
            {canManage && (
              <form
                onSubmit={async (e) => {
                  e.preventDefault();
                  try {
                    await upsertAllowanceType({
                      code: allowCode,
                      name: allowName,
                      defaultAmount: Number(allowAmt) || 0,
                      isTaxable: true,
                      isActive: true,
                    });
                    setAllowCode("");
                    setAllowName("");
                    setOk("Đã thêm phụ cấp.");
                    await load();
                  } catch {
                    setError("Thêm phụ cấp thất bại.");
                  }
                }}
                className="mb-3 flex flex-wrap gap-2"
              >
                <input className={cn(field.input, "w-28")} value={allowCode} onChange={(e) => setAllowCode(e.target.value)} placeholder="Mã" required />
                <input className={cn(field.input, "w-40")} value={allowName} onChange={(e) => setAllowName(e.target.value)} placeholder="Tên" required />
                <input className={cn(field.input, "w-28")} value={allowAmt} onChange={(e) => setAllowAmt(e.target.value)} placeholder="Mặc định" />
                <button type="submit" className={btn.primary}>
                  Thêm PC
                </button>
              </form>
            )}
            <ul className="mb-4 space-y-1 text-body">
              {allowTypes.map((t) => (
                <li key={t.id}>
                  <span className="font-mono text-brand-strong">{t.code}</span> · {t.name}: {money(t.defaultAmount)}
                </li>
              ))}
            </ul>

            {canManage && (
              <form
                onSubmit={async (e) => {
                  e.preventDefault();
                  try {
                    await upsertAllowanceRule({
                      allowanceTypeId: ruleTypeId,
                      shiftCode: ruleShift || null,
                      amount: Number(ruleAmt) || 0,
                      isActive: true,
                    });
                    setOk("Đã thêm rule phụ cấp.");
                    await load();
                  } catch {
                    setError("Thêm rule thất bại.");
                  }
                }}
                className="mb-3 flex flex-wrap gap-2"
              >
                <select className={cn(field.select, "w-36")} value={ruleTypeId} onChange={(e) => setRuleTypeId(e.target.value)}>
                  {allowTypes.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.code}
                    </option>
                  ))}
                </select>
                <input className={cn(field.input, "w-40")} value={ruleShift} onChange={(e) => setRuleShift(e.target.value)} placeholder="Mã ca (trống = mọi)" />
                <input className={cn(field.input, "w-28")} value={ruleAmt} onChange={(e) => setRuleAmt(e.target.value)} placeholder="Số tiền" />
                <button type="submit" className={btn.primary}>
                  Thêm rule
                </button>
              </form>
            )}
            <ul className="space-y-1 text-body text-muted-foreground">
              {allowRules.map((r) => (
                <li key={r.id}>
                  {r.allowanceTypeName} · ca {r.shiftCode || "*"}: {money(r.amount)}
                </li>
              ))}
            </ul>
          </Panel>
        </div>
      )}

      {tab === "mine" && (
        <Panel title="Phiếu lương của tôi">
          {mine.length === 0 ? (
            <p className="py-6 text-center text-body text-muted-foreground">
              Chưa có phiếu (cần kỳ Confirmed/Locked).
            </p>
          ) : (
            <div className={tableWrap}>
              <table className="w-full text-body">
                <thead className="border-b border-border bg-muted">
                  <tr>
                    <th className={th}>Công</th>
                    <th className={th}>Gross</th>
                    <th className={th}>BH</th>
                    <th className={th}>Thuế</th>
                    <th className={th}>Net</th>
                  </tr>
                </thead>
                <tbody>
                  {mine.map((l) => (
                    <tr key={l.id} className="border-t border-border">
                      <td className={cn(td, "tabular-nums")}>{l.workUnits}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.grossPay)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.insuranceEmployee)}</td>
                      <td className={cn(td, "tabular-nums")}>{money(l.tax)}</td>
                      <td className={cn(td, "font-semibold tabular-nums text-brand-strong")}>{money(l.netPay)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Panel>
      )}

      {tab === "report" && (
        <div className="grid gap-4 lg:grid-cols-2">
          <Panel title="Chi phí theo đơn vị">
            {cost.length === 0 ? (
              <p className="py-4 text-center text-muted-foreground">Chọn kỳ để xem báo cáo.</p>
            ) : (
              <ul className="space-y-2">
                {cost.map((c) => (
                  <li key={c.orgUnitId} className="rounded-lg border border-border/70 px-3 py-2">
                    <div className="font-semibold text-foreground">{c.orgUnitName}</div>
                    <div className="mt-0.5 text-meta text-muted-foreground">
                      {c.headcount} NV · Gross {money(c.gross)} · Net {money(c.net)} · BH {money(c.insurance)}
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </Panel>
          <Panel title="So sánh kỳ">
            {compare.length === 0 ? (
              <p className="py-4 text-center text-muted-foreground">Chưa có dữ liệu so sánh.</p>
            ) : (
              <ul className="space-y-2">
                {compare.map((c) => (
                  <li key={c.periodKey} className="flex justify-between gap-2 border-b border-border/60 py-2 last:border-0">
                    <span className="font-mono text-brand-strong">{c.periodKey}</span>
                    <span className="text-meta text-muted-foreground">
                      {c.lineCount} dòng · Gross {money(c.totalGross)} · Net {money(c.totalNet)}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </Panel>
        </div>
      )}
    </div>
  );
}
