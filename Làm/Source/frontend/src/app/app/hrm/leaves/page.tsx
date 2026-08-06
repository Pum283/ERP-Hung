"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  allocateLeaveYear,
  cancelLeaveRequest,
  createLeaveRequest,
  fetchEmployeeTypes,
  fetchEmployees,
  fetchHolidays,
  fetchLeaveBalances,
  fetchLeaveCalendar,
  fetchLeaveEntitlements,
  fetchLeaveReport,
  fetchLeaveRequests,
  fetchLeaveTypes,
  importHolidays,
  upsertHoliday,
  upsertLeaveEntitlement,
  type EmployeeDto,
  type HolidayDto,
  type LeaveBalanceDto,
  type LeaveCalendarItemDto,
  type LeaveEntitlementRuleDto,
  type LeaveReportRowDto,
  type LeaveRequestDto,
  type LeaveTypeDto,
} from "@/shared/api/hrm-api";
import { fetchOrgUnits, type OrgUnitDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

function yearNow() {
  return new Date().getFullYear();
}

export default function LeavesPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.leave.read");
  const canManage = can("hrm.leave.manage");

  const [tab, setTab] = useState<"mine" | "admin" | "calendar" | "report">("mine");
  const [types, setTypes] = useState<LeaveTypeDto[]>([]);
  const [empTypes, setEmpTypes] = useState<{ id: string; code: string; name: string }[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [balances, setBalances] = useState<LeaveBalanceDto[]>([]);
  const [rows, setRows] = useState<LeaveRequestDto[]>([]);
  const [rules, setRules] = useState<LeaveEntitlementRuleDto[]>([]);
  const [calendar, setCalendar] = useState<LeaveCalendarItemDto[]>([]);
  const [holidays, setHolidays] = useState<HolidayDto[]>([]);
  const [report, setReport] = useState<LeaveReportRowDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const [leaveTypeId, setLeaveTypeId] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [days, setDays] = useState("1");
  const [reason, setReason] = useState("");
  const [ruleTypeId, setRuleTypeId] = useState("");
  const [ruleEmpTypeId, setRuleEmpTypeId] = useState("");
  const [ruleDays, setRuleDays] = useState("12");
  const [holidayDate, setHolidayDate] = useState("");
  const [holidayName, setHolidayName] = useState("");
  const [orgUnitId, setOrgUnitId] = useState("");
  const [year, setYear] = useState(yearNow);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [t, et, emp, o, b, r, rulesR, cal, hol, rep] = await Promise.all([
        fetchLeaveTypes(),
        fetchEmployeeTypes(),
        fetchEmployees(),
        fetchOrgUnits(),
        fetchLeaveBalances(),
        fetchLeaveRequests(),
        fetchLeaveEntitlements(),
        fetchLeaveCalendar(),
        fetchHolidays(year),
        fetchLeaveReport({ year }),
      ]);
      setTypes(t);
      setEmpTypes(et);
      setEmployees(emp);
      setOrgs(o);
      setBalances(b);
      setRows(r);
      setRules(rulesR);
      setCalendar(cal);
      setHolidays(hol);
      setReport(rep);
      if (!leaveTypeId && t[0]) setLeaveTypeId(t[0].id);
      if (!ruleTypeId && t[0]) setRuleTypeId(t[0].id);
    } catch {
      setError("Không tải được dữ liệu nghỉ phép.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  async function onSubmit(e: FormEvent, submit: boolean) {
    e.preventDefault();
    if (!canManage) return;
    setError(null);
    setOk(null);
    try {
      await createLeaveRequest({
        leaveTypeId,
        fromDate,
        toDate,
        days: Number(days),
        reason: reason || null,
        submit,
      });
      setOk(submit ? "Đã gửi duyệt." : "Đã lưu nháp.");
      setReason("");
      await load();
    } catch {
      setError("Không tạo/gửi được đơn nghỉ.");
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.leave.read</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Nghỉ phép</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Quỹ phép · đơn nghỉ · rule theo loại NS · lịch · ngày lễ · báo cáo
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      <div className="flex flex-wrap gap-2">
        {(
          [
            ["mine", "Cá nhân / đơn"],
            ["admin", "Cấu hình quỹ"],
            ["calendar", "Lịch & ngày lễ"],
            ["report", "Báo cáo"],
          ] as const
        ).map(([k, label]) => (
          <button
            key={k}
            type="button"
            className={tab === k ? btn.primary : btn.ghost}
            onClick={() => setTab(k)}
          >
            {label}
          </button>
        ))}
        <button type="button" className={`${btn.ghost} ml-auto`} onClick={() => void load()}>
          Làm mới
        </button>
      </div>

      {tab === "mine" && (
        <>
          <section className="space-y-2">
            <h2 className="text-lead font-bold">Quỹ phép năm</h2>
            <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
              <table className="w-full text-body">
                <thead className="border-b border-border bg-muted text-left text-muted-foreground">
                  <tr>
                    <th className="px-4 py-2.5">Loại</th>
                    <th className="px-4 py-2.5">Năm</th>
                    <th className="px-4 py-2.5">Được hưởng</th>
                    <th className="px-4 py-2.5">Đã dùng</th>
                    <th className="px-4 py-2.5">Còn lại</th>
                  </tr>
                </thead>
                <tbody>
                  {balances.map((b) => (
                    <tr key={b.id} className="border-t border-border">
                      <td className="px-4 py-2.5 font-medium">{b.leaveTypeName}</td>
                      <td className="px-4 py-2.5">{b.year}</td>
                      <td className="px-4 py-2.5">{b.entitled}</td>
                      <td className="px-4 py-2.5">{b.used}</td>
                      <td className="px-4 py-2.5 font-semibold text-brand-strong">{b.remaining}</td>
                    </tr>
                  ))}
                  {!loading && balances.length === 0 && (
                    <tr>
                      <td colSpan={5} className="px-4 py-6 text-center text-muted-foreground">
                        Chưa có quỹ phép.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </section>

          {canManage && (
            <form
              className="grid max-w-2xl gap-3 rounded-xl border border-border bg-surface p-4 shadow-sm sm:grid-cols-2"
              onSubmit={(e) => void onSubmit(e, false)}
            >
              <h2 className="text-lead font-bold sm:col-span-2">Tạo đơn nghỉ</h2>
              <label className="space-y-1 text-body">
                <span className="text-muted-foreground">Loại nghỉ</span>
                <select
                  value={leaveTypeId}
                  onChange={(e) => setLeaveTypeId(e.target.value)}
                  className="h-9 w-full rounded-md border border-border bg-background px-2"
                >
                  {types.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name}
                    </option>
                  ))}
                </select>
              </label>
              <label className="space-y-1 text-body">
                <span className="text-muted-foreground">Số ngày</span>
                <input
                  type="number"
                  min="0.5"
                  step="0.5"
                  value={days}
                  onChange={(e) => setDays(e.target.value)}
                  className="h-9 w-full rounded-md border border-border bg-background px-2"
                />
              </label>
              <label className="space-y-1 text-body">
                <span className="text-muted-foreground">Từ ngày</span>
                <input
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                  className="h-9 w-full rounded-md border border-border bg-background px-2"
                  required
                />
              </label>
              <label className="space-y-1 text-body">
                <span className="text-muted-foreground">Đến ngày</span>
                <input
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                  className="h-9 w-full rounded-md border border-border bg-background px-2"
                  required
                />
              </label>
              <label className="space-y-1 text-body sm:col-span-2">
                <span className="text-muted-foreground">Lý do</span>
                <input
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  className="h-9 w-full rounded-md border border-border bg-background px-2"
                />
              </label>
              <div className="flex gap-2 sm:col-span-2">
                <button type="submit" className={btn.secondary}>
                  Lưu nháp
                </button>
                <button
                  type="button"
                  className={btn.primary}
                  onClick={(e) => void onSubmit(e as unknown as FormEvent, true)}
                >
                  Gửi duyệt
                </button>
              </div>
            </form>
          )}

          <section className="space-y-2">
            <h2 className="text-lead font-bold">Đơn nghỉ</h2>
            <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
              <table className="w-full text-body">
                <thead className="border-b border-border bg-muted text-left text-muted-foreground">
                  <tr>
                    <th className="px-4 py-2.5">NV</th>
                    <th className="px-4 py-2.5">Loại</th>
                    <th className="px-4 py-2.5">Từ–Đến</th>
                    <th className="px-4 py-2.5">Ngày</th>
                    <th className="px-4 py-2.5">Status</th>
                    <th className="px-4 py-2.5">Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((r) => (
                    <tr key={r.id} className="border-t border-border">
                      <td className="px-4 py-2.5 font-medium">{r.employeeName}</td>
                      <td className="px-4 py-2.5">{r.leaveTypeName}</td>
                      <td className="px-4 py-2.5 text-meta">
                        {r.fromDate} → {r.toDate}
                      </td>
                      <td className="px-4 py-2.5">{r.days}</td>
                      <td className="px-4 py-2.5">{r.status}</td>
                      <td className="px-4 py-2.5">
                        {canManage &&
                          (r.status === "Draft" ||
                            r.status === "Pending" ||
                            r.status === "Approved") && (
                            <button
                              type="button"
                              className={btn.ghost}
                              onClick={async () => {
                                try {
                                  await cancelLeaveRequest(r.id);
                                  setOk("Đã hủy đơn.");
                                  await load();
                                } catch {
                                  setError("Hủy đơn thất bại.");
                                }
                              }}
                            >
                              Hủy
                            </button>
                          )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        </>
      )}

      {tab === "admin" && (
        <section className="grid gap-4 lg:grid-cols-2">
          <form
            className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
            onSubmit={async (e) => {
              e.preventDefault();
              if (!canManage) return;
              try {
                await upsertLeaveEntitlement({
                  leaveTypeId: ruleTypeId,
                  employeeTypeId: ruleEmpTypeId || null,
                  daysPerYear: Number(ruleDays),
                  isActive: true,
                });
                setOk("Đã lưu rule quỹ phép.");
                await load();
              } catch {
                setError("Lưu rule thất bại.");
              }
            }}
          >
            <h2 className="text-lead font-bold">Rule quỹ theo loại NS</h2>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Loại nghỉ</span>
              <select
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={ruleTypeId}
                onChange={(e) => setRuleTypeId(e.target.value)}
                disabled={!canManage}
              >
                {types.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
              </select>
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Loại NS (trống = tất cả)</span>
              <select
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={ruleEmpTypeId}
                onChange={(e) => setRuleEmpTypeId(e.target.value)}
                disabled={!canManage}
              >
                <option value="">Tất cả</option>
                {empTypes.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
              </select>
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Ngày / năm</span>
              <input
                type="number"
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={ruleDays}
                onChange={(e) => setRuleDays(e.target.value)}
                disabled={!canManage}
              />
            </label>
            {canManage && (
              <button type="submit" className={btn.primary}>
                Lưu rule
              </button>
            )}
            <ul className="text-body text-muted-foreground">
              {rules.map((r) => (
                <li key={r.id}>
                  {r.leaveTypeName} · {r.employeeTypeName ?? "Tất cả"}: {r.daysPerYear}d
                </li>
              ))}
            </ul>
          </form>

          <div className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Cấp phát quỹ năm</h2>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Năm</span>
              <input
                type="number"
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={year}
                onChange={(e) => setYear(Number(e.target.value))}
                disabled={!canManage}
              />
            </label>
            {canManage && (
              <button
                type="button"
                className={btn.secondary}
                onClick={async () => {
                  try {
                    const r = await allocateLeaveYear({ year });
                    setOk(`Đã cấp phát ${r.allocated} dòng quỹ.`);
                    await load();
                  } catch {
                    setError("Cấp phát thất bại.");
                  }
                }}
              >
                Cấp phát theo rule
              </button>
            )}
            <p className="text-body text-muted-foreground">
              Áp dụng rule loại NS; không có rule thì dùng DefaultDaysPerYear của loại nghỉ.
              NV mẫu: {employees.length}.
            </p>
          </div>
        </section>
      )}

      {tab === "calendar" && (
        <section className="grid gap-4 lg:grid-cols-2">
          <div className="rounded-xl border border-border bg-surface p-4 shadow-sm">
            <div className="mb-2 flex flex-wrap gap-2">
              <h2 className="text-lead font-bold mr-auto">Lịch nghỉ theo đơn vị</h2>
              <select
                className="rounded-lg border border-border bg-background px-2 py-1 text-body"
                value={orgUnitId}
                onChange={(e) => setOrgUnitId(e.target.value)}
              >
                <option value="">Tất cả</option>
                {orgs.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.code}
                  </option>
                ))}
              </select>
              <button
                type="button"
                className={btn.ghost}
                onClick={async () => {
                  setCalendar(
                    await fetchLeaveCalendar({ orgUnitId: orgUnitId || undefined }),
                  );
                }}
              >
                Lọc
              </button>
            </div>
            <ul className="max-h-80 space-y-1 overflow-y-auto text-body">
              {calendar.map((c) => (
                <li key={c.requestId}>
                  {c.fromDate.slice(0, 10)}–{c.toDate.slice(0, 10)} · {c.employeeCode} ·{" "}
                  {c.leaveTypeName} ({c.status})
                </li>
              ))}
              {calendar.length === 0 && (
                <li className="text-muted-foreground">Không có lịch nghỉ.</li>
              )}
            </ul>
          </div>

          <div className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Ngày nghỉ lễ {year}</h2>
            {canManage && (
              <form
                className="flex flex-wrap gap-2"
                onSubmit={async (e) => {
                  e.preventDefault();
                  try {
                    await upsertHoliday({
                      date: holidayDate,
                      name: holidayName,
                      isPaid: true,
                    });
                    setOk("Đã thêm ngày lễ.");
                    setHolidayName("");
                    await load();
                  } catch {
                    setError("Thêm ngày lễ thất bại.");
                  }
                }}
              >
                <input
                  type="date"
                  className="rounded-lg border border-border bg-background px-2 py-1"
                  value={holidayDate}
                  onChange={(e) => setHolidayDate(e.target.value)}
                  required
                />
                <input
                  className="rounded-lg border border-border bg-background px-2 py-1"
                  value={holidayName}
                  onChange={(e) => setHolidayName(e.target.value)}
                  placeholder="Tên lễ"
                  required
                />
                <button type="submit" className={btn.secondary}>
                  Thêm
                </button>
                <button
                  type="button"
                  className={btn.ghost}
                  onClick={async () => {
                    try {
                      const r = await importHolidays([
                        { date: `${year}-01-01`, name: "Tết Dương lịch", isPaid: true },
                        { date: `${year}-04-30`, name: "30/4", isPaid: true },
                        { date: `${year}-05-01`, name: "1/5", isPaid: true },
                        { date: `${year}-09-02`, name: "Quốc khánh", isPaid: true },
                      ]);
                      setOk(`Đã import ${r.imported} ngày lễ mẫu.`);
                      await load();
                    } catch {
                      setError("Import thất bại.");
                    }
                  }}
                >
                  Import mẫu VN
                </button>
              </form>
            )}
            <ul className="text-body">
              {holidays.map((h) => (
                <li key={h.id}>
                  {h.date.slice(0, 10)} — {h.name}
                  {h.isPaid ? "" : " (không lương)"}
                </li>
              ))}
            </ul>
          </div>
        </section>
      )}

      {tab === "report" && (
        <section className="rounded-xl border border-border bg-surface p-4 shadow-sm">
          <div className="mb-3 flex flex-wrap items-end gap-2">
            <h2 className="text-lead font-bold mr-auto">Báo cáo quỹ phép</h2>
            <input
              type="number"
              className="rounded-lg border border-border bg-background px-2 py-1 text-body"
              value={year}
              onChange={(e) => setYear(Number(e.target.value))}
            />
            <button
              type="button"
              className={btn.secondary}
              onClick={async () => setReport(await fetchLeaveReport({ year }))}
            >
              Xem
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-body">
              <thead>
                <tr className="border-b border-border text-muted-foreground">
                  <th className="py-2 pr-2">NV</th>
                  <th className="py-2 pr-2">ĐV</th>
                  <th className="py-2 pr-2">Loại</th>
                  <th className="py-2 pr-2">Entitled</th>
                  <th className="py-2 pr-2">Used</th>
                  <th className="py-2 pr-2">Còn</th>
                  <th className="py-2">Đơn duyệt</th>
                </tr>
              </thead>
              <tbody>
                {report.map((r, i) => (
                  <tr key={`${r.employeeId}-${r.leaveTypeId}-${i}`} className="border-b border-border/60">
                    <td className="py-2 pr-2">
                      {r.employeeCode} — {r.employeeName}
                    </td>
                    <td className="py-2 pr-2">{r.orgUnitName}</td>
                    <td className="py-2 pr-2">{r.leaveTypeName}</td>
                    <td className="py-2 pr-2">{r.entitled}</td>
                    <td className="py-2 pr-2">{r.used}</td>
                    <td className="py-2 pr-2">{r.remaining}</td>
                    <td className="py-2">{r.approvedRequests}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}
    </div>
  );
}
