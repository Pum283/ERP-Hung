"use client";

import { FormEvent, useEffect, useState } from "react";
import {
  fetchContracts,
  fetchEmployees,
  upsertContract,
  type ContractDto,
  type EmployeeDto,
} from "@/shared/api/hrm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { SideSheet } from "@/shared/ui/SideSheet";

export default function ContractsPage() {
  const { can } = usePermissions();
  const [rows, setRows] = useState<ContractDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [sheetOpen, setSheetOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const [employeeId, setEmployeeId] = useState("");
  const [contractNo, setContractNo] = useState("");
  const [contractType, setContractType] = useState("Indefinite");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [status, setStatus] = useState("Active");

  const canRead = can("hrm.contract.read");
  const canManage = can("hrm.contract.manage");

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [c, e] = await Promise.all([
        fetchContracts(),
        can("hrm.employee.read") ? fetchEmployees() : Promise.resolve([] as EmployeeDto[]),
      ]);
      setRows(c);
      setEmployees(e);
      if (!employeeId && e[0]) setEmployeeId(e[0].id);
    } catch {
      setError("Không tải được hợp đồng.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  function openCreate() {
    setFormError(null);
    setContractNo("");
    setContractType("Indefinite");
    setStartDate("");
    setEndDate("");
    setStatus("Active");
    if (employees[0]) setEmployeeId(employees[0].id);
    setSheetOpen(true);
  }

  async function onCreate(e: FormEvent) {
    e.preventDefault();
    setFormError(null);
    setSaving(true);
    try {
      await upsertContract({
        employeeId,
        contractNo,
        contractType,
        startDate,
        endDate: endDate || null,
        status,
      });
      setSheetOpen(false);
      await load();
    } catch {
      setFormError("Không lưu được hợp đồng (số HĐ trùng hoặc thiếu quyền).");
    } finally {
      setSaving(false);
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.contract.read</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Hợp đồng LĐ</h1>
          <p className="mt-1 text-body text-muted-foreground">Danh sách · thêm mở panel phải</p>
        </div>
        {canManage && (
          <button
            type="button"
            onClick={openCreate}
            className="inline-flex h-9 items-center rounded-md bg-brand px-3 text-body font-semibold text-brand-foreground hover:bg-brand-hover"
          >
            Thêm hợp đồng
          </button>
        )}
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}

      {loading ? (
        <p className="text-body text-muted-foreground">Đang tải…</p>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-semibold">Số HĐ</th>
                <th className="px-4 py-2.5 font-semibold">NV</th>
                <th className="px-4 py-2.5 font-semibold">Loại</th>
                <th className="px-4 py-2.5 font-semibold">Thời hạn</th>
                <th className="px-4 py-2.5 font-semibold">Status</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((c) => (
                <tr key={c.id} className="border-t border-border">
                  <td className="px-4 py-2.5 font-mono text-meta font-semibold text-brand-strong">
                    {c.contractNo}
                  </td>
                  <td className="px-4 py-2.5">{c.employeeName ?? c.employeeId}</td>
                  <td className="px-4 py-2.5">{c.contractType}</td>
                  <td className="px-4 py-2.5 text-meta">
                    {c.startDate}
                    {c.endDate ? ` → ${c.endDate}` : " → ∞"}
                  </td>
                  <td className="px-4 py-2.5">
                    <span className="inline-flex rounded-full bg-brand-muted px-2 py-0.5 text-meta font-semibold text-brand-strong">
                      {c.status}
                    </span>
                  </td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-6 text-center text-muted-foreground">
                    Chưa có hợp đồng.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      <SideSheet
        open={sheetOpen}
        onOpenChange={setSheetOpen}
        title="Thêm hợp đồng"
        description="Khung HĐLĐ Day-1"
        widthClassName="max-w-lg"
        footer={
          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={() => setSheetOpen(false)}
              className="h-9 rounded-md border border-border px-3 text-body font-medium hover:bg-muted"
            >
              Hủy
            </button>
            <button
              type="submit"
              form="contract-sheet-form"
              disabled={saving}
              className="h-9 rounded-md bg-brand px-4 text-body font-semibold text-brand-foreground hover:bg-brand-hover disabled:opacity-60"
            >
              {saving ? "Đang lưu…" : "Lưu"}
            </button>
          </div>
        }
      >
        <form id="contract-sheet-form" onSubmit={(e) => void onCreate(e)} className="grid gap-3">
          <label className="space-y-1 text-body">
            <span className="text-muted-foreground">Nhân viên</span>
            <select
              value={employeeId}
              onChange={(e) => setEmployeeId(e.target.value)}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
              required
            >
              {employees.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.employeeCode} · {emp.fullName}
                </option>
              ))}
            </select>
          </label>
          <label className="space-y-1 text-body">
            <span className="text-muted-foreground">Số HĐ</span>
            <input
              value={contractNo}
              onChange={(e) => setContractNo(e.target.value)}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
              required
            />
          </label>
          <label className="space-y-1 text-body">
            <span className="text-muted-foreground">Loại</span>
            <select
              value={contractType}
              onChange={(e) => setContractType(e.target.value)}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
            >
              <option value="Indefinite">Không xác định thời hạn</option>
              <option value="FixedTerm">Xác định thời hạn</option>
              <option value="Probation">Thử việc</option>
            </select>
          </label>
          <label className="space-y-1 text-body">
            <span className="text-muted-foreground">Bắt đầu</span>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
              required
            />
          </label>
          <label className="space-y-1 text-body">
            <span className="text-muted-foreground">Kết thúc</span>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
            />
          </label>
          <label className="space-y-1 text-body">
            <span className="text-muted-foreground">Status</span>
            <select
              value={status}
              onChange={(e) => setStatus(e.target.value)}
              className="h-9 w-full rounded-md border border-border bg-background px-2"
            >
              <option value="Active">Active</option>
              <option value="Expired">Expired</option>
              <option value="Terminated">Terminated</option>
            </select>
          </label>
          {formError && <p className="text-body text-destructive">{formError}</p>}
        </form>
      </SideSheet>
    </div>
  );
}
