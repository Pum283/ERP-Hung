"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  acknowledgeTransfer,
  activateTransfer,
  approveTransferRequest,
  cancelTransfer,
  completeTransfer,
  createTransferOrder,
  createTransferRequest,
  fetchEmployees,
  fetchMyTransfers,
  fetchTransferCostReport,
  fetchTransferTracking,
  fetchTransfers,
  issueTransferOrder,
  rejectTransferRequest,
  setTransferActualHours,
  setTransferAttendanceTag,
  submitTransferRequest,
  type EmployeeDto,
  type StaffTransferDto,
  type TransferCostReportRowDto,
} from "@/shared/api/hrm-api";
import { fetchOrgUnits, type OrgUnitDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

function today() {
  return new Date().toISOString().slice(0, 10);
}

export default function TransfersPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.employee.read");
  const canManage = can("hrm.employee.manage");

  const [all, setAll] = useState<StaffTransferDto[]>([]);
  const [mine, setMine] = useState<StaffTransferDto[]>([]);
  const [tracking, setTracking] = useState<StaffTransferDto[]>([]);
  const [report, setReport] = useState<TransferCostReportRowDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [orgs, setOrgs] = useState<OrgUnitDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState<"all" | "mine" | "track" | "report">("all");

  const [fromOrg, setFromOrg] = useState("");
  const [toOrg, setToOrg] = useState("");
  const [employeeId, setEmployeeId] = useState("");
  const [startDate, setStartDate] = useState(today);
  const [endDate, setEndDate] = useState("");
  const [reason, setReason] = useState("");
  const [headcount, setHeadcount] = useState("1");
  const [plannedHours, setPlannedHours] = useState("8");
  const [costRate, setCostRate] = useState("50000");
  const [attendanceTagged, setAttendanceTagged] = useState(true);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [a, m, t, r, e, o] = await Promise.all([
        fetchTransfers(),
        fetchMyTransfers(),
        fetchTransferTracking(),
        fetchTransferCostReport(),
        fetchEmployees(),
        fetchOrgUnits(),
      ]);
      setAll(a);
      setMine(m);
      setTracking(t);
      setReport(r);
      setEmployees(e);
      setOrgs(o);
      if (!fromOrg && o[0]) setFromOrg(o[0].id);
      if (!toOrg && o[1]) setToOrg(o[1].id);
      else if (!toOrg && o[0]) setToOrg(o[0].id);
      if (!employeeId && e[0]) setEmployeeId(e[0].id);
    } catch {
      setError("Không tải được điều động.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  async function onCreateRequest(e: FormEvent, submit: boolean) {
    e.preventDefault();
    if (!canManage) return;
    setError(null);
    setOk(null);
    try {
      await createTransferRequest({
        fromOrgUnitId: fromOrg,
        toOrgUnitId: toOrg,
        startDate,
        endDate: endDate || null,
        requestedHeadcount: Number(headcount),
        reason,
        submit,
      });
      setOk(submit ? "Đã gửi đề xuất điều động." : "Đã lưu đề xuất nháp.");
      setReason("");
      await load();
    } catch {
      setError("Tạo đề xuất thất bại.");
    }
  }

  async function onCreateOrder(e: FormEvent, issue: boolean) {
    e.preventDefault();
    if (!canManage) return;
    setError(null);
    setOk(null);
    try {
      await createTransferOrder({
        employeeId,
        fromOrgUnitId: fromOrg,
        toOrgUnitId: toOrg,
        startDate,
        endDate: endDate || null,
        reason,
        plannedHours: Number(plannedHours) || null,
        costRate: Number(costRate) || null,
        attendanceTagged,
        issue,
      });
      setOk(issue ? "Đã phát hành lệnh điều động." : "Đã lưu lệnh nháp.");
      setReason("");
      await load();
    } catch {
      setError("Tạo lệnh thất bại.");
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.employee.read</p>;
  }

  const list =
    tab === "mine" ? mine : tab === "track" ? tracking : tab === "all" ? all : [];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-title font-bold text-foreground">Điều động nhân sự</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Đề xuất · lệnh · nhận lệnh · theo dõi · nhãn chấm công · báo cáo giờ/chi phí
        </p>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      <section className="grid gap-4 lg:grid-cols-2">
        <form
          onSubmit={(e) => void onCreateRequest(e, false)}
          className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
        >
          <h2 className="text-lead font-bold">Đề xuất nhu cầu</h2>
          <OrgPair
            orgs={orgs}
            fromOrg={fromOrg}
            toOrg={toOrg}
            setFromOrg={setFromOrg}
            setToOrg={setToOrg}
            disabled={!canManage}
          />
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Số người</span>
            <input
              type="number"
              min={1}
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={headcount}
              onChange={(e) => setHeadcount(e.target.value)}
              disabled={!canManage}
            />
          </label>
          <Dates
            startDate={startDate}
            endDate={endDate}
            setStartDate={setStartDate}
            setEndDate={setEndDate}
            disabled={!canManage}
          />
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Lý do</span>
            <textarea
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              rows={2}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              disabled={!canManage}
              required
            />
          </label>
          {canManage && (
            <div className="flex flex-wrap gap-2">
              <button type="submit" className={btn.secondary}>
                Lưu nháp
              </button>
              <button
                type="button"
                className={btn.primary}
                onClick={(e) => void onCreateRequest(e as unknown as FormEvent, true)}
              >
                Gửi duyệt
              </button>
            </div>
          )}
        </form>

        <form
          onSubmit={(e) => void onCreateOrder(e, false)}
          className="space-y-3 rounded-xl border border-border bg-surface p-4 shadow-sm"
        >
          <h2 className="text-lead font-bold">Lệnh điều động</h2>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Nhân viên</span>
            <select
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              value={employeeId}
              onChange={(e) => setEmployeeId(e.target.value)}
              disabled={!canManage}
            >
              {employees.map((e) => (
                <option key={e.id} value={e.id}>
                  {e.employeeCode} — {e.fullName}
                </option>
              ))}
            </select>
          </label>
          <OrgPair
            orgs={orgs}
            fromOrg={fromOrg}
            toOrg={toOrg}
            setFromOrg={setFromOrg}
            setToOrg={setToOrg}
            disabled={!canManage}
          />
          <Dates
            startDate={startDate}
            endDate={endDate}
            setStartDate={setStartDate}
            setEndDate={setEndDate}
            disabled={!canManage}
          />
          <div className="grid grid-cols-2 gap-2">
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Giờ KH</span>
              <input
                type="number"
                min={0}
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={plannedHours}
                onChange={(e) => setPlannedHours(e.target.value)}
                disabled={!canManage}
              />
            </label>
            <label className="block space-y-1 text-body">
              <span className="text-muted-foreground">Đơn giá/giờ</span>
              <input
                type="number"
                min={0}
                className="w-full rounded-lg border border-border bg-background px-3 py-2"
                value={costRate}
                onChange={(e) => setCostRate(e.target.value)}
                disabled={!canManage}
              />
            </label>
          </div>
          <label className="flex items-center gap-2 text-body">
            <input
              type="checkbox"
              checked={attendanceTagged}
              onChange={(e) => setAttendanceTagged(e.target.checked)}
              disabled={!canManage}
            />
            Gắn nhãn công điều động (TRANSFER)
          </label>
          <label className="block space-y-1 text-body">
            <span className="text-muted-foreground">Lý do</span>
            <textarea
              className="w-full rounded-lg border border-border bg-background px-3 py-2"
              rows={2}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              disabled={!canManage}
              required
            />
          </label>
          {canManage && (
            <div className="flex flex-wrap gap-2">
              <button type="submit" className={btn.secondary}>
                Lưu nháp
              </button>
              <button
                type="button"
                className={btn.primary}
                onClick={(e) => void onCreateOrder(e as unknown as FormEvent, true)}
              >
                Phát hành
              </button>
            </div>
          )}
        </form>
      </section>

      <section className="rounded-xl border border-border bg-surface p-4 shadow-sm space-y-3">
        <div className="flex flex-wrap gap-2">
          {(
            [
              ["all", "Tất cả"],
              ["mine", "Lệnh của tôi"],
              ["track", "Đang điều động"],
              ["report", "Báo cáo giờ/CP"],
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

        {loading ? (
          <p className="text-body text-muted-foreground">Đang tải…</p>
        ) : tab === "report" ? (
          report.length === 0 ? (
            <p className="text-body text-muted-foreground">Chưa có dữ liệu báo cáo.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-body">
                <thead>
                  <tr className="border-b border-border text-muted-foreground">
                    <th className="py-2 pr-2">Đơn vị đích</th>
                    <th className="py-2 pr-2">Số lệnh</th>
                    <th className="py-2 pr-2">Giờ KH</th>
                    <th className="py-2 pr-2">Giờ TT</th>
                    <th className="py-2 pr-2">CP KH</th>
                    <th className="py-2">CP TT</th>
                  </tr>
                </thead>
                <tbody>
                  {report.map((r) => (
                    <tr key={r.orgUnitId} className="border-b border-border/60">
                      <td className="py-2 pr-2">{r.orgUnitName}</td>
                      <td className="py-2 pr-2">{r.orderCount}</td>
                      <td className="py-2 pr-2">{r.plannedHours}</td>
                      <td className="py-2 pr-2">{r.actualHours}</td>
                      <td className="py-2 pr-2">{r.estimatedCost.toLocaleString("vi-VN")}</td>
                      <td className="py-2">{r.actualCost.toLocaleString("vi-VN")}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )
        ) : list.length === 0 ? (
          <p className="text-body text-muted-foreground">Chưa có phiếu.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-body">
              <thead>
                <tr className="border-b border-border text-muted-foreground">
                  <th className="py-2 pr-2">Số</th>
                  <th className="py-2 pr-2">Loại</th>
                  <th className="py-2 pr-2">NV / nhu cầu</th>
                  <th className="py-2 pr-2">Từ → Đến</th>
                  <th className="py-2 pr-2">TT</th>
                  <th className="py-2">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {list.map((r) => (
                  <tr key={r.id} className="border-b border-border/60 align-top">
                    <td className="py-2 pr-2">
                      {r.docNo}
                      <div className="text-muted-foreground">{r.startDate.slice(0, 10)}</div>
                    </td>
                    <td className="py-2 pr-2">{r.kind}</td>
                    <td className="py-2 pr-2">
                      {r.kind === "Order"
                        ? `${r.employeeCode ?? ""} — ${r.employeeName ?? ""}`
                        : `${r.requestedHeadcount ?? 0} người`}
                      {r.attendanceTagged && (
                        <div className="text-muted-foreground">Tag: {r.attendanceTag || "TRANSFER"}</div>
                      )}
                    </td>
                    <td className="py-2 pr-2">
                      {r.fromOrgUnitName} → {r.toOrgUnitName}
                    </td>
                    <td className="py-2 pr-2">{r.status}</td>
                    <td className="py-2">
                      <div className="flex flex-wrap gap-1">
                        {canManage && r.kind === "Request" && r.status === "Draft" && (
                          <button
                            type="button"
                            className={btn.secondary}
                            onClick={() => void act(() => submitTransferRequest(r.id), "Đã gửi.")}
                          >
                            Gửi
                          </button>
                        )}
                        {canManage && r.kind === "Request" && r.status === "Submitted" && (
                          <>
                            <button
                              type="button"
                              className={btn.primary}
                              onClick={() => void act(() => approveTransferRequest(r.id), "Đã duyệt.")}
                            >
                              Duyệt
                            </button>
                            <button
                              type="button"
                              className={btn.ghost}
                              onClick={() => void act(() => rejectTransferRequest(r.id), "Đã từ chối.")}
                            >
                              Từ chối
                            </button>
                          </>
                        )}
                        {canManage && r.kind === "Order" && r.status === "Draft" && (
                          <button
                            type="button"
                            className={btn.primary}
                            onClick={() => void act(() => issueTransferOrder(r.id), "Đã phát hành.")}
                          >
                            Phát hành
                          </button>
                        )}
                        {tab === "mine" && r.status === "Issued" && (
                          <button
                            type="button"
                            className={btn.primary}
                            onClick={() => void act(() => acknowledgeTransfer(r.id), "Đã nhận lệnh.")}
                          >
                            Nhận lệnh
                          </button>
                        )}
                        {canManage &&
                          r.kind === "Order" &&
                          (r.status === "Issued" || r.status === "Acknowledged") && (
                            <button
                              type="button"
                              className={btn.secondary}
                              onClick={() => void act(() => activateTransfer(r.id), "Đã kích hoạt.")}
                            >
                              Active
                            </button>
                          )}
                        {canManage &&
                          r.kind === "Order" &&
                          (r.status === "Active" || r.status === "Acknowledged") && (
                            <button
                              type="button"
                              className={btn.secondary}
                              onClick={() => void act(() => completeTransfer(r.id), "Đã hoàn thành.")}
                            >
                              Hoàn thành
                            </button>
                          )}
                        {canManage && r.kind === "Order" && r.status !== "Completed" && r.status !== "Cancelled" && (
                          <>
                            <button
                              type="button"
                              className={btn.ghost}
                              onClick={() =>
                                void act(
                                  () => setTransferAttendanceTag(r.id, !r.attendanceTagged),
                                  r.attendanceTagged ? "Đã tắt nhãn." : "Đã gắn nhãn.",
                                )
                              }
                            >
                              {r.attendanceTagged ? "Tắt nhãn" : "Gắn nhãn"}
                            </button>
                            <button
                              type="button"
                              className={btn.ghost}
                              onClick={() => {
                                const v = window.prompt("Giờ thực tế", String(r.actualHours ?? r.plannedHours ?? 8));
                                if (v == null) return;
                                void act(() => setTransferActualHours(r.id, Number(v)), "Đã cập nhật giờ.");
                              }}
                            >
                              Giờ TT
                            </button>
                          </>
                        )}
                        {canManage &&
                          !["Completed", "Cancelled", "Converted"].includes(r.status) && (
                            <button
                              type="button"
                              className={btn.ghost}
                              onClick={() => void act(() => cancelTransfer(r.id), "Đã hủy.")}
                            >
                              Hủy
                            </button>
                          )}
                      </div>
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

  async function act(fn: () => Promise<unknown>, msg: string) {
    setError(null);
    setOk(null);
    try {
      await fn();
      setOk(msg);
      await load();
    } catch {
      setError("Thao tác thất bại.");
    }
  }
}

function OrgPair({
  orgs,
  fromOrg,
  toOrg,
  setFromOrg,
  setToOrg,
  disabled,
}: {
  orgs: OrgUnitDto[];
  fromOrg: string;
  toOrg: string;
  setFromOrg: (v: string) => void;
  setToOrg: (v: string) => void;
  disabled: boolean;
}) {
  return (
    <div className="grid grid-cols-2 gap-2">
      <label className="block space-y-1 text-body">
        <span className="text-muted-foreground">Từ ĐV</span>
        <select
          className="w-full rounded-lg border border-border bg-background px-3 py-2"
          value={fromOrg}
          onChange={(e) => setFromOrg(e.target.value)}
          disabled={disabled}
        >
          {orgs.map((o) => (
            <option key={o.id} value={o.id}>
              {o.code}
            </option>
          ))}
        </select>
      </label>
      <label className="block space-y-1 text-body">
        <span className="text-muted-foreground">Đến ĐV</span>
        <select
          className="w-full rounded-lg border border-border bg-background px-3 py-2"
          value={toOrg}
          onChange={(e) => setToOrg(e.target.value)}
          disabled={disabled}
        >
          {orgs.map((o) => (
            <option key={o.id} value={o.id}>
              {o.code}
            </option>
          ))}
        </select>
      </label>
    </div>
  );
}

function Dates({
  startDate,
  endDate,
  setStartDate,
  setEndDate,
  disabled,
}: {
  startDate: string;
  endDate: string;
  setStartDate: (v: string) => void;
  setEndDate: (v: string) => void;
  disabled: boolean;
}) {
  return (
    <div className="grid grid-cols-2 gap-2">
      <label className="block space-y-1 text-body">
        <span className="text-muted-foreground">Từ ngày</span>
        <input
          type="date"
          className="w-full rounded-lg border border-border bg-background px-3 py-2"
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
          disabled={disabled}
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
          disabled={disabled}
        />
      </label>
    </div>
  );
}
