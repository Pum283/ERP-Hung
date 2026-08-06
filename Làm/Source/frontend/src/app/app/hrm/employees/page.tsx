"use client";

import { useEffect, useState } from "react";
import { Eye, Search } from "lucide-react";
import { fetchEmployees, type EmployeeDto } from "@/shared/api/hrm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { EmployeeSideSheet } from "./EmployeeSideSheet";

type SheetState =
  | { open: false }
  | { open: true; mode: "create" }
  | { open: true; mode: "view" | "edit"; employeeId: string };

export default function EmployeesPage() {
  const { can } = usePermissions();
  const [rows, setRows] = useState<EmployeeDto[]>([]);
  const [q, setQ] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [sheet, setSheet] = useState<SheetState>({ open: false });

  async function load(term?: string) {
    setLoading(true);
    setError(null);
    try {
      setRows(await fetchEmployees(term));
    } catch {
      setError("Không tải được danh sách nhân sự.");
    } finally {
      setLoading(false);
    }
  }

  const canRead = can("hrm.employee.read");
  const canManage = can("hrm.employee.manage");

  useEffect(() => {
    if (!canRead) return;
    void load();
  }, [canRead]);

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền hrm.employee.read</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Hồ sơ nhân sự</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Danh sách theo data scope · thêm/chi tiết mở panel phải
          </p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => {
              void (async () => {
                const { api } = await import("@/shared/api/client");
                const res = await api.get("/api/hrm/employees/export.csv", { responseType: "blob" });
                const url = URL.createObjectURL(res.data as Blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = "employees.csv";
                a.click();
                URL.revokeObjectURL(url);
              })();
            }}
            className="inline-flex h-9 items-center rounded-md border border-border bg-surface px-3 text-body font-semibold hover:bg-muted"
          >
            Xuất CSV
          </button>
        {canManage && (
          <button
            type="button"
            onClick={() => setSheet({ open: true, mode: "create" })}
            className="inline-flex h-9 items-center rounded-md bg-brand px-3 text-body font-semibold text-brand-foreground hover:bg-brand-hover"
          >
            Thêm nhân viên
          </button>
        )}
        </div>
      </div>

      <form
        className="flex max-w-md gap-2"
        onSubmit={(e) => {
          e.preventDefault();
          void load(q);
        }}
      >
        <div className="flex h-9 flex-1 items-center gap-2 rounded-md border border-border bg-surface px-2.5">
          <Search className="h-3.5 w-3.5 text-muted-foreground" />
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Tìm mã / tên / email…"
            className="w-full border-0 bg-transparent text-body outline-none"
          />
        </div>
        <button
          type="submit"
          className="h-9 rounded-md border border-border bg-surface px-3 text-body font-medium hover:bg-muted"
        >
          Tìm
        </button>
      </form>

      {error && <p className="text-body text-destructive">{error}</p>}
      {loading ? (
        <p className="text-body text-muted-foreground">Đang tải…</p>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-semibold">Mã</th>
                <th className="px-4 py-2.5 font-semibold">Họ tên</th>
                <th className="px-4 py-2.5 font-semibold">Phòng</th>
                <th className="px-4 py-2.5 font-semibold">Chức danh</th>
                <th className="px-4 py-2.5 font-semibold">Loại</th>
                <th className="px-4 py-2.5 font-semibold">Status</th>
                <th className="px-4 py-2.5 font-semibold" />
              </tr>
            </thead>
            <tbody>
              {rows.map((e) => (
                <tr key={e.id} className="border-t border-border hover:bg-muted/60">
                  <td className="px-4 py-2.5 font-mono text-meta font-semibold text-brand-strong">
                    {e.employeeCode}
                  </td>
                  <td className="px-4 py-2.5 font-medium text-foreground">{e.fullName}</td>
                  <td className="px-4 py-2.5">{e.departmentName ?? "—"}</td>
                  <td className="px-4 py-2.5">{e.jobTitleName ?? "—"}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{e.employeeTypeName ?? "—"}</td>
                  <td className="px-4 py-2.5">
                    <span className="inline-flex rounded-full bg-brand-muted px-2 py-0.5 text-meta font-semibold text-brand-strong">
                      {e.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-right">
                    <button
                      type="button"
                      onClick={() => setSheet({ open: true, mode: "view", employeeId: e.id })}
                      className="inline-flex h-8 items-center gap-1.5 rounded-md border border-brand/25 bg-brand-muted px-2.5 text-meta font-semibold text-brand-strong transition-colors hover:border-brand hover:bg-brand hover:text-brand-foreground"
                    >
                      <Eye className="h-3.5 w-3.5" />
                      Chi tiết
                    </button>
                  </td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-4 py-8 text-center text-muted-foreground">
                    Không có nhân viên trong phạm vi của bạn.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      <EmployeeSideSheet
        open={sheet.open}
        onOpenChange={(open) => {
          if (!open) setSheet({ open: false });
        }}
        mode={sheet.open ? sheet.mode : "view"}
        employeeId={sheet.open && sheet.mode !== "create" ? sheet.employeeId : null}
        onSaved={() => void load(q)}
      />
    </div>
  );
}
