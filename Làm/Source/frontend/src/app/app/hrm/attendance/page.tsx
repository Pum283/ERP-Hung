"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  approveAttendanceAdjust,
  attendanceCheckIn,
  attendanceCheckOut,
  confirmAttendanceRecord,
  createAttendanceAdjust,
  fetchAttendanceAdjusts,
  fetchAttendanceAlerts,
  fetchAttendanceBoard,
  fetchAttendanceDevices,
  fetchAttendanceGeoFences,
  fetchAttendanceLocks,
  fetchAttendancePolicy,
  fetchEmployees,
  fetchMyAttendance,
  lockAttendancePeriod,
  markAttendanceMissing,
  recalcAttendanceOt,
  rejectAttendanceAdjust,
  syncAttendanceDevice,
  unlockAttendancePeriod,
  upsertAttendanceDevice,
  upsertAttendanceGeoFence,
  upsertAttendancePolicy,
  type AttendanceAdjustDto,
  type AttendanceDeviceDto,
  type AttendanceGeoFenceDto,
  type AttendanceMissingAlertDto,
  type AttendancePeriodLockDto,
  type AttendancePolicyDto,
  type AttendanceRecordDto,
  type EmployeeDto,
} from "@/shared/api/hrm-api";
import { fetchOrgUnits, type OrgUnitDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

function today() {
  return new Date().toISOString().slice(0, 10);
}

function monthKey() {
  return today().slice(0, 7);
}

const defaultPolicy = (): AttendancePolicyDto => ({
  enableFingerprint: false,
  enableApp: true,
  enableQr: true,
  enableGeoFence: false,
  lateGraceMinutes: 5,
  lateDeductEveryMinutes: 30,
  lateDeductWorkUnit: 0.25,
  forgotCheckoutHours: 14,
  adjustDeadlineDays: 3,
  enableOt: true,
  otAfterMinutes: 30,
  enableNightShiftRule: false,
  enableHolidayRule: false,
  defaultShiftStart: "08:00:00",
  defaultShiftEnd: "17:00:00",
});

export default function AttendancePage() {
  const { can } = usePermissions();
  const canRead = can("hrm.employee.read");
  const canManage = can("hrm.employee.manage");

  const [policy, setPolicy] = useState<AttendancePolicyDto>(defaultPolicy);
  const [devices, setDevices] = useState<AttendanceDeviceDto[]>([]);
  const [fences, setFences] = useState<AttendanceGeoFenceDto[]>([]);
  const [mine, setMine] = useState<AttendanceRecordDto[]>([]);
  const [board, setBoard] = useState<AttendanceRecordDto[]>([]);
  const [alerts, setAlerts] = useState<AttendanceMissingAlertDto[]>([]);
  const [adjusts, setAdjusts] = useState<AttendanceAdjustDto[]>([]);
  const [locks, setLocks] = useState<AttendancePeriodLockDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [orgUnitId, setOrgUnitId] = useState("");
  const [from, setFrom] = useState(() => today().slice(0, 8) + "01");
  const [to, setTo] = useState(today);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [tab, setTab] = useState<"punch" | "board" | "config" | "adjust">("punch");

  const [devCode, setDevCode] = useState("MAY01");
  const [devName, setDevName] = useState("Máy chấm cổng");
  const [fenceName, setFenceName] = useState("Văn phòng");
  const [fenceLat, setFenceLat] = useState("21.0285");
  const [fenceLng, setFenceLng] = useState("105.8542");
  const [lockPeriod, setLockPeriod] = useState(monthKey);
  const [adjEmp, setAdjEmp] = useState("");
  const [adjDate, setAdjDate] = useState(today);
  const [adjReason, setAdjReason] = useState("");
  const [syncCode, setSyncCode] = useState("");

  async function load() {
    setError(null);
    try {
      const [p, d, g, m, b, a, adj, l, e, o] = await Promise.all([
        fetchAttendancePolicy(),
        fetchAttendanceDevices(),
        fetchAttendanceGeoFences(),
        fetchMyAttendance({ from, to }).catch(() => [] as AttendanceRecordDto[]),
        fetchAttendanceBoard({ orgUnitId: orgUnitId || undefined, from, to }),
        fetchAttendanceAlerts(today()),
        fetchAttendanceAdjusts(),
        fetchAttendanceLocks(),
        fetchEmployees(),
        fetchOrgUnits(),
      ]);
      setPolicy(p);
      setDevices(d);
      setFences(g);
      setMine(m);
      setBoard(b);
      setAlerts(a);
      setAdjusts(adj);
      setLocks(l);
      setEmployees(e);
      setOrgs(o);
      if (!orgUnitId && o[0]) setOrgUnitId(o[0].id);
      if (!adjEmp && e[0]) setAdjEmp(e[0].id);
      if (!syncCode && e[0]) setSyncCode(e[0].employeeCode);
    } catch {
      setError("Không tải được chấm công.");
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.employee.read</p>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Chấm công</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Cấu hình · check-in/out · bảng công · cảnh báo · điều chỉnh · khóa kỳ
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      <div className="flex flex-wrap gap-2">
        {(
          [
            ["punch", "Chấm & cá nhân"],
            ["board", "Bảng công"],
            ["adjust", "Điều chỉnh / khóa"],
            ["config", "Cấu hình"],
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

      {tab === "punch" && (
        <section className="grid gap-4 lg:grid-cols-2">
          <div className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Check-in / Check-out</h2>
            <p className="text-body text-muted-foreground">
              Cần hồ sơ NV gắn user đang đăng nhập. Phương thức: App.
            </p>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                className={btn.primary}
                onClick={async () => {
                  try {
                    await attendanceCheckIn({ method: "App" });
                    setOk("Đã check-in.");
                    await load();
                  } catch {
                    setError("Check-in thất bại (thiếu hồ sơ NV / đã chấm / kỳ khóa).");
                  }
                }}
              >
                Check-in
              </button>
              <button
                type="button"
                className={btn.secondary}
                onClick={async () => {
                  try {
                    await attendanceCheckOut({ method: "App" });
                    setOk("Đã check-out.");
                    await load();
                  } catch {
                    setError("Check-out thất bại.");
                  }
                }}
              >
                Check-out
              </button>
            </div>
            {alerts.length > 0 && (
              <div className="rounded-lg border border-destructive/40 bg-destructive/5 p-3">
                <h3 className="font-semibold text-destructive">Cảnh báo thiếu chấm ({alerts.length})</h3>
                <ul className="mt-1 max-h-40 overflow-y-auto text-body">
                  {alerts.slice(0, 20).map((a) => (
                    <li key={`${a.employeeId}-${a.alertType}`}>
                      {a.employeeCode} — {a.employeeName}: {a.alertType}
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
          <div className="rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Lịch sử cá nhân</h2>
            <RecordTable rows={mine} canManage={false} />
          </div>
        </section>
      )}

      {tab === "board" && (
        <section className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm">
          <div className="flex flex-wrap items-end gap-2">
            <label className="text-body">
              Đơn vị{" "}
              <select
                className="rounded-lg border border-border bg-background px-2 py-1"
                value={orgUnitId}
                onChange={(e) => setOrgUnitId(e.target.value)}
              >
                <option value="">— Tất cả (công ty) —</option>
                {orgs.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.code}
                  </option>
                ))}
              </select>
            </label>
            <label className="text-body">
              Từ{" "}
              <input
                type="date"
                className="rounded-lg border border-border bg-background px-2 py-1"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
              />
            </label>
            <label className="text-body">
              Đến{" "}
              <input
                type="date"
                className="rounded-lg border border-border bg-background px-2 py-1"
                value={to}
                onChange={(e) => setTo(e.target.value)}
              />
            </label>
            <button type="button" className={btn.secondary} onClick={() => void load()}>
              Lọc
            </button>
            {canManage && (
              <>
                <button
                  type="button"
                  className={btn.ghost}
                  onClick={async () => {
                    try {
                      const r = await markAttendanceMissing(today());
                      setOk(`Đã đánh dấu thiếu: ${r.marked}`);
                      await load();
                    } catch {
                      setError("Đánh dấu thiếu thất bại.");
                    }
                  }}
                >
                  Đánh dấu thiếu hôm nay
                </button>
                <button
                  type="button"
                  className={btn.ghost}
                  onClick={async () => {
                    try {
                      const r = await recalcAttendanceOt(from, to);
                      setOk(`Đã tính lại OT: ${r.recalculated}`);
                      await load();
                    } catch {
                      setError("Tính OT thất bại.");
                    }
                  }}
                >
                  Tính lại OT
                </button>
                <button
                  type="button"
                  className={btn.ghost}
                  onClick={async () => {
                    try {
                      const r = await syncAttendanceDevice([
                        {
                          employeeCode: syncCode,
                          punchedAt: new Date().toISOString(),
                          punchType: "in",
                          deviceCode: devices[0]?.code,
                        },
                      ]);
                      setOk(`Đồng bộ máy: ${r.synced}`);
                      await load();
                    } catch {
                      setError("Đồng bộ thất bại.");
                    }
                  }}
                >
                  Sync thử máy
                </button>
                <input
                  className="rounded-lg border border-border bg-background px-2 py-1 text-body"
                  value={syncCode}
                  onChange={(e) => setSyncCode(e.target.value)}
                  placeholder="Mã NV sync"
                />
              </>
            )}
          </div>
          <RecordTable
            rows={board}
            canManage={canManage}
            onConfirm={async (id) => {
              try {
                await confirmAttendanceRecord(id);
                setOk("Đã xác nhận công.");
                await load();
              } catch {
                setError("Xác nhận thất bại.");
              }
            }}
          />
        </section>
      )}

      {tab === "adjust" && (
        <section className="grid gap-4 lg:grid-cols-2">
          <form
            className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
            onSubmit={async (e: FormEvent) => {
              e.preventDefault();
              try {
                await createAttendanceAdjust({
                  employeeId: adjEmp,
                  workDate: adjDate,
                  reason: adjReason,
                  submit: true,
                });
                setOk("Đã gửi phiếu điều chỉnh.");
                setAdjReason("");
                await load();
              } catch {
                setError("Tạo phiếu điều chỉnh thất bại (quá hạn / kỳ khóa).");
              }
            }}
          >
            <h2 className="text-lead font-bold">Xin điều chỉnh công</h2>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Nhân viên</span>
              <select
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={adjEmp}
                onChange={(e) => setAdjEmp(e.target.value)}
              >
                {employees.map((e) => (
                  <option key={e.id} value={e.id}>
                    {e.employeeCode} — {e.fullName}
                  </option>
                ))}
              </select>
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Ngày công</span>
              <input
                type="date"
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={adjDate}
                onChange={(e) => setAdjDate(e.target.value)}
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Lý do / bằng chứng (mô tả)</span>
              <textarea
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                rows={2}
                value={adjReason}
                onChange={(e) => setAdjReason(e.target.value)}
                required
              />
            </label>
            <button type="submit" className={btn.primary}>
              Gửi duyệt
            </button>
            <ul className="space-y-2 text-body">
              {adjusts.slice(0, 10).map((a) => (
                <li key={a.id} className="flex flex-wrap items-center gap-2 border-b border-border/50 py-1">
                  <span>
                    {a.employeeCode} · {a.workDate.slice(0, 10)} · {a.status}
                  </span>
                  {canManage && a.status === "Submitted" && (
                    <>
                      <button
                        type="button"
                        className={btn.secondary}
                        onClick={async () => {
                          await approveAttendanceAdjust(a.id);
                          setOk("Đã duyệt điều chỉnh.");
                          await load();
                        }}
                      >
                        Duyệt
                      </button>
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={async () => {
                          await rejectAttendanceAdjust(a.id);
                          setOk("Đã từ chối.");
                          await load();
                        }}
                      >
                        Từ chối
                      </button>
                    </>
                  )}
                </li>
              ))}
            </ul>
          </form>

          <div className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Khóa / mở khóa kỳ</h2>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Kỳ yyyy-MM</span>
              <input
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={lockPeriod}
                onChange={(e) => setLockPeriod(e.target.value)}
              />
            </label>
            {canManage && (
              <div className="flex gap-2">
                <button
                  type="button"
                  className={btn.primary}
                  onClick={async () => {
                    try {
                      await lockAttendancePeriod(lockPeriod);
                      setOk("Đã khóa kỳ.");
                      await load();
                    } catch {
                      setError("Khóa thất bại.");
                    }
                  }}
                >
                  Khóa
                </button>
                <button
                  type="button"
                  className={btn.secondary}
                  onClick={async () => {
                    try {
                      await unlockAttendancePeriod(lockPeriod);
                      setOk("Đã mở khóa.");
                      await load();
                    } catch {
                      setError("Mở khóa thất bại.");
                    }
                  }}
                >
                  Mở khóa
                </button>
              </div>
            )}
            <ul className="text-body text-muted-foreground">
              {locks.map((l) => (
                <li key={l.id}>
                  {l.periodKey}: {l.isLocked ? "Locked" : "Unlocked"} · {l.lockerName}
                </li>
              ))}
            </ul>
          </div>
        </section>
      )}

      {tab === "config" && (
        <section className="grid gap-4 lg:grid-cols-2">
          <form
            className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
            onSubmit={async (e) => {
              e.preventDefault();
              if (!canManage) return;
              try {
                setPolicy(await upsertAttendancePolicy(policy));
                setOk("Đã lưu cấu hình chấm công.");
              } catch {
                setError("Lưu cấu hình thất bại.");
              }
            }}
          >
            <h2 className="text-lead font-bold">Quy tắc chấm công</h2>
            {(
              [
                ["enableFingerprint", "Vân tay / sinh trắc"],
                ["enableApp", "APP điện thoại"],
                ["enableQr", "QR / mã NV"],
                ["enableGeoFence", "Bắt buộc geo-fence"],
                ["enableOt", "Tính OT"],
                ["enableNightShiftRule", "Quy tắc ca đêm"],
                ["enableHolidayRule", "Quy tắc ngày lễ"],
              ] as const
            ).map(([key, label]) => (
              <label key={key} className="flex items-center gap-2 text-body">
                <input
                  type="checkbox"
                  checked={policy[key]}
                  disabled={!canManage}
                  onChange={(e) => setPolicy({ ...policy, [key]: e.target.checked })}
                />
                {label}
              </label>
            ))}
            <div className="grid grid-cols-2 gap-2">
              <Num
                label="Ân hạn trễ (phút)"
                value={policy.lateGraceMinutes}
                disabled={!canManage}
                onChange={(v) => setPolicy({ ...policy, lateGraceMinutes: v })}
              />
              <Num
                label="Mỗi bậc trễ (phút)"
                value={policy.lateDeductEveryMinutes}
                disabled={!canManage}
                onChange={(v) => setPolicy({ ...policy, lateDeductEveryMinutes: v })}
              />
              <Num
                label="Trừ công / bậc"
                value={policy.lateDeductWorkUnit}
                step={0.05}
                disabled={!canManage}
                onChange={(v) => setPolicy({ ...policy, lateDeductWorkUnit: v })}
              />
              <Num
                label="Quên checkout (giờ)"
                value={policy.forgotCheckoutHours}
                disabled={!canManage}
                onChange={(v) => setPolicy({ ...policy, forgotCheckoutHours: v })}
              />
              <Num
                label="Hạn điều chỉnh (ngày)"
                value={policy.adjustDeadlineDays}
                disabled={!canManage}
                onChange={(v) => setPolicy({ ...policy, adjustDeadlineDays: v })}
              />
              <Num
                label="OT sau (phút)"
                value={policy.otAfterMinutes}
                disabled={!canManage}
                onChange={(v) => setPolicy({ ...policy, otAfterMinutes: v })}
              />
            </div>
            {canManage && (
              <button type="submit" className={btn.primary}>
                Lưu cấu hình
              </button>
            )}
          </form>

          <div className="space-y-4">
            <form
              className="space-y-2 rounded-xl border border-border bg-surface p-4 shadow-sm"
              onSubmit={async (e) => {
                e.preventDefault();
                if (!canManage) return;
                try {
                  await upsertAttendanceDevice({
                    code: devCode,
                    name: devName,
                    deviceType: "Fingerprint",
                    orgUnitId: orgUnitId || null,
                    isActive: true,
                  });
                  setOk("Đã đăng ký thiết bị.");
                  await load();
                } catch {
                  setError("Đăng ký thiết bị thất bại.");
                }
              }}
            >
              <h2 className="text-lead font-bold">Thiết bị chấm</h2>
              <input
                className="w-full rounded-lg border border-border bg-background px-3 py-2 text-body"
                value={devCode}
                onChange={(e) => setDevCode(e.target.value)}
                disabled={!canManage}
                placeholder="Mã"
              />
              <input
                className="w-full rounded-lg border border-border bg-background px-3 py-2 text-body"
                value={devName}
                onChange={(e) => setDevName(e.target.value)}
                disabled={!canManage}
                placeholder="Tên"
              />
              {canManage && (
                <button type="submit" className={btn.secondary}>
                  Thêm thiết bị
                </button>
              )}
              <ul className="text-body text-muted-foreground">
                {devices.map((d) => (
                  <li key={d.id}>
                    {d.code} — {d.name} ({d.deviceType})
                  </li>
                ))}
              </ul>
            </form>

            <form
              className="space-y-2 rounded-xl border border-border bg-surface p-4 shadow-sm"
              onSubmit={async (e) => {
                e.preventDefault();
                if (!canManage) return;
                try {
                  await upsertAttendanceGeoFence({
                    name: fenceName,
                    orgUnitId: orgUnitId || null,
                    latitude: Number(fenceLat),
                    longitude: Number(fenceLng),
                    radiusMeters: 300,
                    isActive: true,
                  });
                  setOk("Đã lưu geo-fence.");
                  await load();
                } catch {
                  setError("Lưu geo-fence thất bại.");
                }
              }}
            >
              <h2 className="text-lead font-bold">Geo-fence</h2>
              <input
                className="w-full rounded-lg border border-border bg-background px-3 py-2 text-body"
                value={fenceName}
                onChange={(e) => setFenceName(e.target.value)}
                disabled={!canManage}
              />
              <div className="grid grid-cols-2 gap-2">
                <input
                  className="rounded-lg border border-border bg-background px-3 py-2 text-body"
                  value={fenceLat}
                  onChange={(e) => setFenceLat(e.target.value)}
                  disabled={!canManage}
                  placeholder="Lat"
                />
                <input
                  className="rounded-lg border border-border bg-background px-3 py-2 text-body"
                  value={fenceLng}
                  onChange={(e) => setFenceLng(e.target.value)}
                  disabled={!canManage}
                  placeholder="Lng"
                />
              </div>
              {canManage && (
                <button type="submit" className={btn.secondary}>
                  Thêm điểm
                </button>
              )}
              <ul className="text-body text-muted-foreground">
                {fences.map((f) => (
                  <li key={f.id}>
                    {f.name}: {f.latitude}, {f.longitude} ±{f.radiusMeters}m
                  </li>
                ))}
              </ul>
            </form>
          </div>
        </section>
      )}
    </div>
  );
}

function Num({
  label,
  value,
  onChange,
  disabled,
  step = 1,
}: {
  label: string;
  value: number;
  onChange: (v: number) => void;
  disabled: boolean;
  step?: number;
}) {
  return (
    <label className="block space-y-1 text-body">
      <span className="text-muted-foreground">{label}</span>
      <input
        type="number"
        step={step}
        className="w-full rounded-lg border border-border bg-background px-3 py-2"
        value={value}
        disabled={disabled}
        onChange={(e) => onChange(Number(e.target.value))}
      />
    </label>
  );
}

function RecordTable({
  rows,
  canManage,
  onConfirm,
}: {
  rows: AttendanceRecordDto[];
  canManage: boolean;
  onConfirm?: (id: string) => void;
}) {
  if (rows.length === 0) {
    return <p className="mt-2 text-body text-muted-foreground">Chưa có bản ghi.</p>;
  }
  return (
    <div className="mt-2 overflow-x-auto">
      <table className="w-full text-left text-body">
        <thead>
          <tr className="border-b border-border text-muted-foreground">
            <th className="py-2 pr-2">Ngày</th>
            <th className="py-2 pr-2">NV</th>
            <th className="py-2 pr-2">In/Out</th>
            <th className="py-2 pr-2">Trễ</th>
            <th className="py-2 pr-2">Công</th>
            <th className="py-2 pr-2">OT</th>
            <th className="py-2">TT</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.id} className="border-b border-border/60">
              <td className="py-2 pr-2">{r.workDate.slice(0, 10)}</td>
              <td className="py-2 pr-2">
                {r.employeeCode}
                {r.tag ? ` · ${r.tag}` : ""}
              </td>
              <td className="py-2 pr-2">
                {r.checkInAt ? new Date(r.checkInAt).toLocaleTimeString("vi-VN") : "—"} /{" "}
                {r.checkOutAt ? new Date(r.checkOutAt).toLocaleTimeString("vi-VN") : "—"}
              </td>
              <td className="py-2 pr-2">{r.lateMinutes}′ (−{r.deductedWorkUnit})</td>
              <td className="py-2 pr-2">{r.workUnit}</td>
              <td className="py-2 pr-2">{r.otMinutes}′</td>
              <td className="py-2">
                {r.status}
                {r.isConfirmed ? " ✓" : ""}
                {canManage && !r.isConfirmed && onConfirm && (
                  <button type="button" className={`${btn.ghost} ml-1`} onClick={() => onConfirm(r.id)}>
                    Xác nhận
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
