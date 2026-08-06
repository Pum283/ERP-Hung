"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  assignShift,
  assignShiftRange,
  cancelShiftAssignment,
  copyShiftAssignments,
  exportShiftScheduleCsv,
  fetchEmployees,
  fetchMyShiftAssignments,
  fetchShiftAssignments,
  fetchShiftLocks,
  fetchWorkShifts,
  lockShiftPeriod,
  swapShifts,
  upsertWorkShift,
  type EmployeeDto,
  type ShiftAssignmentDto,
  type ShiftPeriodLockDto,
  type WorkShiftDto,
} from "@/shared/api/hrm-api";
import { fetchOrgUnits, type OrgUnitDto } from "@/shared/api/sys-api";
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

export default function ShiftsPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.employee.read");
  const canManage = can("hrm.employee.manage");

  const [templates, setTemplates] = useState<WorkShiftDto[]>([]);
  const [assignments, setAssignments] = useState<ShiftAssignmentDto[]>([]);
  const [mine, setMine] = useState<ShiftAssignmentDto[]>([]);
  const [locks, setLocks] = useState<ShiftPeriodLockDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState<"org" | "mine">("org");

  const [code, setCode] = useState("CA1");
  const [name, setName] = useState("Ca sáng");
  const [startTime, setStartTime] = useState("08:00:00");
  const [endTime, setEndTime] = useState("16:00:00");
  const [breakMinutes, setBreakMinutes] = useState("60");

  const [orgUnitId, setOrgUnitId] = useState("");
  const [employeeId, setEmployeeId] = useState("");
  const [workShiftId, setWorkShiftId] = useState("");
  const [workDate, setWorkDate] = useState(today);
  const [rangeFrom, setRangeFrom] = useState(today);
  const [rangeTo, setRangeTo] = useState(() => addDays(today(), 6));
  const [filterFrom, setFilterFrom] = useState(today);
  const [filterTo, setFilterTo] = useState(() => addDays(today(), 30));
  const [swapA, setSwapA] = useState("");
  const [swapB, setSwapB] = useState("");
  const [copyTarget, setCopyTarget] = useState(() => addDays(today(), 7));
  const [lockPeriod, setLockPeriod] = useState(() => today().slice(0, 7));

  const empInOrg = useMemo(
    () => (orgUnitId ? employees.filter((e) => e.orgUnitId === orgUnitId) : employees),
    [employees, orgUnitId],
  );

  const scheduled = useMemo(
    () => assignments.filter((a) => a.status === "Scheduled"),
    [assignments],
  );

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [t, a, m, l, e, o] = await Promise.all([
        fetchWorkShifts(),
        fetchShiftAssignments({
          orgUnitId: orgUnitId || undefined,
          from: filterFrom,
          to: filterTo,
        }),
        fetchMyShiftAssignments({ from: filterFrom, to: filterTo }).catch(() => [] as ShiftAssignmentDto[]),
        fetchShiftLocks(),
        fetchEmployees(),
        fetchOrgUnits(),
      ]);
      setTemplates(t);
      setAssignments(a);
      setMine(m);
      setLocks(l);
      setEmployees(e);
      setOrgs(o);
      if (!orgUnitId && o[0]) setOrgUnitId(o[0].id);
      if (!workShiftId && t.find((x) => x.isActive)) setWorkShiftId(t.find((x) => x.isActive)!.id);
      if (!employeeId && e[0]) setEmployeeId(e[0].id);
    } catch {
      setError("Không tải được ca làm việc.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  async function onSaveTemplate(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    setError(null);
    setOk(null);
    try {
      await upsertWorkShift({
        code,
        name,
        startTime: startTime.length === 5 ? `${startTime}:00` : startTime,
        endTime: endTime.length === 5 ? `${endTime}:00` : endTime,
        breakMinutes: Number(breakMinutes),
        isActive: true,
      });
      setOk("Đã lưu mẫu ca.");
      await load();
    } catch {
      setError("Lưu mẫu ca thất bại.");
    }
  }

  async function onAssignOne(e: FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    try {
      await assignShift({ employeeId, workShiftId, workDate });
      setOk("Đã xếp ca.");
      await load();
    } catch {
      setError("Xếp ca thất bại (có thể kỳ đã khóa).");
    }
  }

  async function onAssignRange(e: FormEvent) {
    e.preventDefault();
    if (!canManage || !employeeId) return;
    try {
      await assignShiftRange({
        employeeIds: [employeeId],
        workShiftId,
        from: rangeFrom,
        to: rangeTo,
        weekdays: [1, 2, 3, 4, 5],
      });
      setOk("Đã xếp ca theo tuần/tháng (T2–T6).");
      await load();
    } catch {
      setError("Xếp ca theo khoảng thất bại.");
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.employee.read</p>;
  }

  const rows = tab === "mine" ? mine : assignments;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Ca làm việc</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Mẫu ca · xếp lịch · đổi/hủy · sao chép · khóa kỳ · xuất CSV
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      <section className="grid gap-4 lg:grid-cols-2">
        <form
          onSubmit={(e) => void onSaveTemplate(e)}
          className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
        >
          <h2 className="text-lead font-bold">Mẫu ca</h2>
          <div className="grid grid-cols-2 gap-2">
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Mã</span>
              <input
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                disabled={!canManage}
                required
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Tên</span>
              <input
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={name}
                onChange={(e) => setName(e.target.value)}
                disabled={!canManage}
                required
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Bắt đầu</span>
              <input
                type="time"
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={startTime.slice(0, 5)}
                onChange={(e) => setStartTime(e.target.value)}
                disabled={!canManage}
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Kết thúc</span>
              <input
                type="time"
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={endTime.slice(0, 5)}
                onChange={(e) => setEndTime(e.target.value)}
                disabled={!canManage}
              />
            </label>
          </div>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Nghỉ giữa ca (phút)</span>
            <input
              type="number"
              min={0}
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={breakMinutes}
              onChange={(e) => setBreakMinutes(e.target.value)}
              disabled={!canManage}
            />
          </label>
          {canManage && (
            <button type="submit" className={btn.primary}>
              Lưu mẫu ca
            </button>
          )}
          <ul className="mt-2 space-y-1 text-body text-muted-foreground">
            {templates.map((t) => (
              <li key={t.id}>
                {t.code} — {t.name} ({t.startTime.slice(0, 5)}–{t.endTime.slice(0, 5)})
                {!t.isActive ? " · ngưng" : ""}
              </li>
            ))}
          </ul>
        </form>

        <div className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm">
          <h2 className="text-lead font-bold">Xếp lịch</h2>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Đơn vị (lọc)</span>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={orgUnitId}
              onChange={(e) => setOrgUnitId(e.target.value)}
            >
              {orgs.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.code} — {o.name}
                </option>
              ))}
            </select>
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Nhân viên</span>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={employeeId}
              onChange={(e) => setEmployeeId(e.target.value)}
              disabled={!canManage}
            >
              {empInOrg.map((e) => (
                <option key={e.id} value={e.id}>
                  {e.employeeCode} — {e.fullName}
                </option>
              ))}
            </select>
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Ca</span>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={workShiftId}
              onChange={(e) => setWorkShiftId(e.target.value)}
              disabled={!canManage}
            >
              {templates
                .filter((t) => t.isActive)
                .map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.code} — {t.name}
                  </option>
                ))}
            </select>
          </label>
          {canManage && (
            <>
              <form onSubmit={(e) => void onAssignOne(e)} className="flex flex-wrap items-end gap-2">
                <label className="block space-y-1 text-body">
                  <span className="text-muted-foreground">Ngày</span>
                  <input
                    type="date"
                    className="rounded-lg border border-border bg-background px-3 py-2"
                    value={workDate}
                    onChange={(e) => setWorkDate(e.target.value)}
                  />
                </label>
                <button type="submit" className={btn.primary}>
                  Xếp 1 ngày
                </button>
              </form>
              <form onSubmit={(e) => void onAssignRange(e)} className="flex flex-wrap items-end gap-2">
                <label className="block space-y-1 text-body">
                  <span className="text-muted-foreground">Từ</span>
                  <input
                    type="date"
                    className="rounded-lg border border-border bg-background px-3 py-2"
                    value={rangeFrom}
                    onChange={(e) => setRangeFrom(e.target.value)}
                  />
                </label>
                <label className="block space-y-1 text-body">
                  <span className="text-muted-foreground">Đến</span>
                  <input
                    type="date"
                    className="rounded-lg border border-border bg-background px-3 py-2"
                    value={rangeTo}
                    onChange={(e) => setRangeTo(e.target.value)}
                  />
                </label>
                <button type="submit" className={btn.secondary}>
                  Xếp T2–T6
                </button>
              </form>
            </>
          )}
        </div>
      </section>

      <section className="rounded-xl border border-border bg-surface p-4 shadow-sm space-y-3">
        <div className="flex flex-wrap items-center gap-2">
          <h2 className="text-lead font-bold mr-auto">Lịch ca</h2>
          <button
            type="button"
            className={tab === "org" ? btn.primary : btn.ghost}
            onClick={() => setTab("org")}
          >
            Theo đơn vị
          </button>
          <button
            type="button"
            className={tab === "mine" ? btn.primary : btn.ghost}
            onClick={() => setTab("mine")}
          >
            Cá nhân
          </button>
          <label className="text-body">
            Từ{" "}
            <input
              type="date"
              className="rounded-lg border border-border bg-background px-2 py-1"
              value={filterFrom}
              onChange={(e) => setFilterFrom(e.target.value)}
            />
          </label>
          <label className="text-body">
            Đến{" "}
            <input
              type="date"
              className="rounded-lg border border-border bg-background px-2 py-1"
              value={filterTo}
              onChange={(e) => setFilterTo(e.target.value)}
            />
          </label>
          <button type="button" className={btn.ghost} onClick={() => void load()}>
            Lọc
          </button>
          <button
            type="button"
            className={btn.secondary}
            onClick={async () => {
              try {
                const blob = await exportShiftScheduleCsv({
                  orgUnitId: orgUnitId || undefined,
                  from: filterFrom,
                  to: filterTo,
                });
                const url = URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = "shift-schedule.csv";
                a.click();
                URL.revokeObjectURL(url);
                setOk("Đã xuất CSV.");
              } catch {
                setError("Xuất CSV thất bại.");
              }
            }}
          >
            Xuất CSV
          </button>
        </div>

        {loading ? (
          <p className="text-body text-muted-foreground">Đang tải…</p>
        ) : rows.length === 0 ? (
          <p className="text-body text-muted-foreground">Chưa có lịch trong khoảng.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-body">
              <thead>
                <tr className="border-b border-border text-muted-foreground">
                  <th className="py-2 pr-2">Ngày</th>
                  <th className="py-2 pr-2">NV</th>
                  <th className="py-2 pr-2">Đơn vị</th>
                  <th className="py-2 pr-2">Ca</th>
                  <th className="py-2 pr-2">TT</th>
                  <th className="py-2">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r) => (
                  <tr key={r.id} className="border-b border-border/60">
                    <td className="py-2 pr-2">{r.workDate.slice(0, 10)}</td>
                    <td className="py-2 pr-2">
                      {r.employeeCode} — {r.employeeName}
                    </td>
                    <td className="py-2 pr-2">{r.orgUnitName}</td>
                    <td className="py-2 pr-2">
                      {r.shiftCode} ({r.startTime.slice(0, 5)}–{r.endTime.slice(0, 5)})
                    </td>
                    <td className="py-2 pr-2">{r.status}</td>
                    <td className="py-2">
                      {canManage && r.status === "Scheduled" && tab === "org" && (
                        <button
                          type="button"
                          className={btn.ghost}
                          onClick={async () => {
                            try {
                              await cancelShiftAssignment(r.id);
                              setOk("Đã hủy lịch ca.");
                              await load();
                            } catch {
                              setError("Hủy thất bại.");
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
        )}
      </section>

      {canManage && (
        <section className="grid gap-4 lg:grid-cols-3">
          <div className="space-y-2 rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Đổi ca</h2>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2 text-body"
              value={swapA}
              onChange={(e) => setSwapA(e.target.value)}
            >
              <option value="">— Ca A —</option>
              {scheduled.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.workDate.slice(0, 10)} · {a.employeeCode} · {a.shiftCode}
                </option>
              ))}
            </select>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2 text-body"
              value={swapB}
              onChange={(e) => setSwapB(e.target.value)}
            >
              <option value="">— Ca B —</option>
              {scheduled.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.workDate.slice(0, 10)} · {a.employeeCode} · {a.shiftCode}
                </option>
              ))}
            </select>
            <button
              type="button"
              className={btn.secondary}
              onClick={async () => {
                try {
                  await swapShifts(swapA, swapB);
                  setOk("Đã đổi ca.");
                  await load();
                } catch {
                  setError("Đổi ca thất bại (cần cùng ngày).");
                }
              }}
            >
              Đổi
            </button>
          </div>

          <div className="space-y-2 rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Sao chép lịch</h2>
            <p className="text-body text-muted-foreground">
              Nguồn {filterFrom} → {filterTo}
            </p>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Ngày bắt đầu đích</span>
              <input
                type="date"
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={copyTarget}
                onChange={(e) => setCopyTarget(e.target.value)}
              />
            </label>
            <button
              type="button"
              className={btn.secondary}
              onClick={async () => {
                try {
                  const r = await copyShiftAssignments({
                    sourceFrom: filterFrom,
                    sourceTo: filterTo,
                    targetStart: copyTarget,
                    orgUnitId: orgUnitId || null,
                  });
                  setOk(`Đã sao chép ${r.copied} dòng.`);
                  await load();
                } catch {
                  setError("Sao chép thất bại.");
                }
              }}
            >
              Sao chép
            </button>
          </div>

          <div className="space-y-2 rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Khóa sổ kỳ</h2>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Kỳ (yyyy-MM)</span>
              <input
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={lockPeriod}
                onChange={(e) => setLockPeriod(e.target.value)}
                placeholder="2026-08"
              />
            </label>
            <button
              type="button"
              className={btn.primary}
              onClick={async () => {
                try {
                  await lockShiftPeriod({ orgUnitId, periodKey: lockPeriod });
                  setOk("Đã khóa kỳ.");
                  await load();
                } catch {
                  setError("Khóa kỳ thất bại.");
                }
              }}
            >
              Khóa
            </button>
            <ul className="text-body text-muted-foreground">
              {locks.slice(0, 5).map((l) => (
                <li key={l.id}>
                  {l.orgUnitName} · {l.periodKey} · {l.lockerName}
                </li>
              ))}
            </ul>
          </div>
        </section>
      )}
    </div>
  );
}
