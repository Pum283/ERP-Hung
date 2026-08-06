"use client";

import { useEffect, useState } from "react";
import {
  addEmployeeDocument,
  deleteEmployeeDocument,
  fetchEmployee,
  fetchEmployeeDocuments,
  fetchEmployeeTypes,
  fetchJobTitles,
  uploadHrmFile,
  upsertEmployee,
  type EmployeeDocumentDto,
  type EmployeeDto,
} from "@/shared/api/hrm-api";
import { api } from "@/shared/api/client";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { SideSheet } from "@/shared/ui/SideSheet";
import { btn } from "@/shared/ui/btn";

type Opt = { id: string; name: string };

type Mode = "create" | "edit" | "view";

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mode: Mode;
  employeeId?: string | null;
  onSaved?: () => void;
};

const emptyForm = {
  employeeCode: "",
  fullName: "",
  email: "",
  phone: "",
  gender: "Male",
  orgUnitId: "",
  departmentId: "",
  jobLevelId: "",
  jobTitleId: "",
  employeeTypeId: "",
  status: "Active",
  hireDate: "",
};

export function EmployeeSideSheet({
  open,
  onOpenChange,
  mode,
  employeeId,
  onSaved,
}: Props) {
  const { can } = usePermissions();
  const canManage = can("hrm.employee.manage");
  const readOnly = mode === "view" || !canManage;

  const [orgUnits, setOrgUnits] = useState<Opt[]>([]);
  const [departments, setDepartments] = useState<Opt[]>([]);
  const [jobLevels, setJobLevels] = useState<Opt[]>([]);
  const [titles, setTitles] = useState<Opt[]>([]);
  const [types, setTypes] = useState<Opt[]>([]);
  const [view, setView] = useState<EmployeeDto | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [editMode, setEditMode] = useState(mode === "edit");
  const [docs, setDocs] = useState<EmployeeDocumentDto[]>([]);
  const [docType, setDocType] = useState("IdCard");
  const [docTitle, setDocTitle] = useState("");
  const apiBase = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

  async function reloadDocs(id: string) {
    try {
      setDocs(await fetchEmployeeDocuments(id));
    } catch {
      setDocs([]);
    }
  }

  useEffect(() => {
    if (!open) return;
    setError(null);
    setEditMode(mode === "edit" || mode === "create");
    setView(null);
    setForm(emptyForm);
    setDocs([]);
    setDocTitle("");
    setDocType("IdCard");

    let cancelled = false;
    void (async () => {
      setLoading(true);
      try {
        if (mode === "create" || mode === "edit" || (mode === "view" && canManage)) {
          const [orgs, depts, jls, jts, ets] = await Promise.all([
            api.get<{ data: Opt[] }>("/api/sys/org-units").then((r) => r.data.data),
            api.get<{ data: Opt[] }>("/api/sys/departments").then((r) => r.data.data),
            api.get<{ data: Opt[] }>("/api/sys/job-levels").then((r) => r.data.data),
            fetchJobTitles(),
            fetchEmployeeTypes(),
          ]);
          if (cancelled) return;
          setOrgUnits(orgs);
          setDepartments(depts);
          setJobLevels(jls);
          setTitles(jts);
          setTypes(ets);
          if (mode === "create" && orgs[0]) {
            setForm((f) => ({ ...f, orgUnitId: orgs[0].id }));
          }
        }

        if (employeeId && mode !== "create") {
          const e = await fetchEmployee(employeeId);
          if (cancelled) return;
          setView(e);
          setForm({
            employeeCode: e.employeeCode,
            fullName: e.fullName,
            email: e.email ?? "",
            phone: e.phone ?? "",
            gender: e.gender ?? "Male",
            orgUnitId: e.orgUnitId,
            departmentId: e.departmentId ?? "",
            jobLevelId: e.jobLevelId ?? "",
            jobTitleId: e.jobTitleId ?? "",
            employeeTypeId: e.employeeTypeId ?? "",
            status: e.status,
            hireDate: e.hireDate ?? "",
          });
          await reloadDocs(employeeId);
        }
      } catch {
        if (!cancelled) setError("Không tải được dữ liệu.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [open, mode, employeeId, canManage]);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (readOnly && !editMode) return;
    setSaving(true);
    setError(null);
    try {
      await upsertEmployee({
        id: mode === "create" ? null : employeeId,
        employeeCode: form.employeeCode,
        fullName: form.fullName,
        email: form.email || null,
        phone: form.phone || null,
        gender: form.gender || null,
        orgUnitId: form.orgUnitId,
        departmentId: form.departmentId || null,
        jobLevelId: form.jobLevelId || null,
        jobTitleId: form.jobTitleId || null,
        employeeTypeId: form.employeeTypeId || null,
        status: form.status,
        hireDate: form.hireDate || null,
      });
      onOpenChange(false);
      onSaved?.();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        "Lưu thất bại.";
      setError(msg);
    } finally {
      setSaving(false);
    }
  }

  function set<K extends keyof typeof form>(key: K, value: (typeof form)[K]) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  const title =
    mode === "create" ? "Thêm nhân viên" : editMode ? "Sửa hồ sơ" : "Chi tiết nhân viên";
  const description =
    mode === "create"
      ? "Tạo hồ sơ mới"
      : view
        ? `${view.employeeCode} · ${view.fullName}`
        : undefined;

  const showForm = mode === "create" || editMode;

  return (
    <SideSheet
      open={open}
      onOpenChange={onOpenChange}
      title={title}
      description={description}
      widthClassName="max-w-xl"
      footer={
        showForm ? (
          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={() => onOpenChange(false)}
              className="h-9 rounded-md border border-border px-3 text-body font-medium hover:bg-muted"
            >
              Hủy
            </button>
            <button
              type="submit"
              form="employee-sheet-form"
              disabled={saving || loading}
              className="h-9 rounded-md bg-brand px-4 text-body font-semibold text-brand-foreground hover:bg-brand-hover disabled:opacity-60"
            >
              {saving ? "Đang lưu…" : "Lưu"}
            </button>
          </div>
        ) : (
          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={() => onOpenChange(false)}
              className="h-9 rounded-md border border-border px-3 text-body font-medium hover:bg-muted"
            >
              Đóng
            </button>
            {canManage && (
              <button
                type="button"
                onClick={() => setEditMode(true)}
                className="h-9 rounded-md bg-brand px-4 text-body font-semibold text-brand-foreground hover:bg-brand-hover"
              >
                Sửa
              </button>
            )}
          </div>
        )
      }
    >
      {loading ? (
        <p className="text-body text-muted-foreground">Đang tải…</p>
      ) : error && !showForm && !view ? (
        <p className="text-body text-destructive">{error}</p>
      ) : showForm ? (
        <form id="employee-sheet-form" onSubmit={onSubmit} className="space-y-3">
          <div className="grid gap-3 sm:grid-cols-2">
            <Field label="Mã NV">
              <input
                required
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.employeeCode}
                onChange={(e) => set("employeeCode", e.target.value)}
              />
            </Field>
            <Field label="Họ tên">
              <input
                required
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.fullName}
                onChange={(e) => set("fullName", e.target.value)}
              />
            </Field>
            <Field label="Email">
              <input
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.email}
                onChange={(e) => set("email", e.target.value)}
              />
            </Field>
            <Field label="Điện thoại">
              <input
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.phone}
                onChange={(e) => set("phone", e.target.value)}
              />
            </Field>
            <Field label="Đơn vị">
              <select
                required
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.orgUnitId}
                onChange={(e) => set("orgUnitId", e.target.value)}
              >
                {orgUnits.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Phòng ban">
              <select
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.departmentId}
                onChange={(e) => set("departmentId", e.target.value)}
              >
                <option value="">—</option>
                {departments.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Cấp bậc">
              <select
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.jobLevelId}
                onChange={(e) => set("jobLevelId", e.target.value)}
              >
                <option value="">—</option>
                {jobLevels.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Chức danh">
              <select
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.jobTitleId}
                onChange={(e) => set("jobTitleId", e.target.value)}
              >
                <option value="">—</option>
                {titles.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Loại NS">
              <select
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.employeeTypeId}
                onChange={(e) => set("employeeTypeId", e.target.value)}
              >
                <option value="">—</option>
                {types.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Trạng thái">
              <select
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.status}
                onChange={(e) => set("status", e.target.value)}
              >
                <option value="Active">Active</option>
                <option value="Probation">Probation</option>
                <option value="Terminated">Terminated</option>
              </select>
            </Field>
            <Field label="Ngày vào">
              <input
                type="date"
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.hireDate}
                onChange={(e) => set("hireDate", e.target.value)}
              />
            </Field>
          </div>
          {error && <p className="text-body text-destructive">{error}</p>}
        </form>
      ) : view ? (
        <div className="space-y-5">
          <dl className="grid gap-3 sm:grid-cols-2">
            {(
              [
                ["Mã NV", view.employeeCode],
                ["Họ tên", view.fullName],
                ["Email", view.email],
                ["Điện thoại", view.phone],
                ["Giới tính", view.gender],
                ["Đơn vị", view.orgUnitName],
                ["Phòng ban", view.departmentName],
                ["Cấp bậc", view.jobLevelName],
                ["Chức danh", view.jobTitleName],
                ["Loại NS", view.employeeTypeName],
                ["Quản lý", view.managerName],
                ["Trạng thái", view.status],
                ["Ngày vào", view.hireDate],
                ["Ngày nghỉ", view.terminateDate],
              ] as [string, string | null | undefined][]
            ).map(([label, value]) => (
              <div key={label}>
                <dt className="text-meta text-muted-foreground">{label}</dt>
                <dd className="text-body font-medium text-foreground">{value || "—"}</dd>
              </div>
            ))}
          </dl>

          <section className="rounded-xl border border-border bg-muted/30 p-3">
            <h3 className="mb-2 font-display text-lead font-bold">Giấy tờ tùy thân</h3>
            {canManage && (
              <div className="mb-3 flex flex-wrap items-end gap-2">
                <label className="block space-y-1 text-body">
                  <span className="text-meta text-muted-foreground">Loại</span>
                  <select
                    className="h-9 rounded-md border border-border bg-background px-2"
                    value={docType}
                    onChange={(e) => setDocType(e.target.value)}
                  >
                    <option value="IdCard">CMND/CCCD</option>
                    <option value="Passport">Hộ chiếu</option>
                    <option value="Household">Hộ khẩu</option>
                    <option value="Degree">Bằng cấp</option>
                    <option value="Other">Khác</option>
                  </select>
                </label>
                <label className="block min-w-[160px] flex-1 space-y-1 text-body">
                  <span className="text-meta text-muted-foreground">Tiêu đề</span>
                  <input
                    className="h-9 w-full rounded-md border border-border bg-background px-2"
                    value={docTitle}
                    onChange={(e) => setDocTitle(e.target.value)}
                    placeholder="VD: CCCD mặt trước"
                  />
                </label>
                <label className={btn.soft + " cursor-pointer"}>
                  Upload
                  <input
                    type="file"
                    hidden
                    onChange={async (ev) => {
                      const f = ev.target.files?.[0];
                      if (!f || !employeeId) return;
                      try {
                        const up = await uploadHrmFile(f);
                        await addEmployeeDocument(employeeId, {
                          docType,
                          title: docTitle.trim() || f.name,
                          storageKey: up.storageKey,
                        });
                        setDocTitle("");
                        await reloadDocs(employeeId);
                      } catch {
                        setError("Upload giấy tờ thất bại.");
                      }
                      ev.target.value = "";
                    }}
                  />
                </label>
              </div>
            )}
            {docs.length === 0 ? (
              <p className="text-meta text-muted-foreground">Chưa có giấy tờ.</p>
            ) : (
              <ul className="space-y-1.5">
                {docs.map((d) => (
                  <li
                    key={d.id}
                    className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border bg-surface px-2.5 py-1.5 text-body"
                  >
                    <div>
                      <span className="font-mono text-meta text-brand-strong">{d.docType}</span>
                      <span className="mx-1.5 text-muted-foreground">·</span>
                      <a
                        className="font-medium text-foreground underline-offset-2 hover:underline"
                        href={`${apiBase}/api/sys/files/${encodeURIComponent(d.storageKey)}`}
                        target="_blank"
                        rel="noreferrer"
                      >
                        {d.title}
                      </a>
                    </div>
                    {canManage && (
                      <button
                        type="button"
                        className={btn.danger}
                        onClick={async () => {
                          if (!employeeId) return;
                          try {
                            await deleteEmployeeDocument(employeeId, d.id);
                            await reloadDocs(employeeId);
                          } catch {
                            setError("Xóa giấy tờ thất bại.");
                          }
                        }}
                      >
                        Xóa
                      </button>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </section>
        </div>
      ) : null}
    </SideSheet>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block space-y-1 text-body">
      <span className="text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}
