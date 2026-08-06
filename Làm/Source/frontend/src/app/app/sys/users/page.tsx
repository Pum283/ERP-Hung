"use client";

import { useEffect, useMemo, useState } from "react";
import { Eye, Search, Shield } from "lucide-react";
import { api } from "@/shared/api/client";
import { fetchRoles, type RoleDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { SideSheet } from "@/shared/ui/SideSheet";

type UserDepartmentDto = {
  departmentId: string;
  departmentName?: string | null;
  jobLevelId?: string | null;
  jobLevelName?: string | null;
  isPrimary: boolean;
};

type UserDto = {
  id: string;
  username: string;
  displayName?: string | null;
  email?: string | null;
  status: string;
  primaryOrgUnitId?: string | null;
  departmentId?: string | null;
  jobLevelId?: string | null;
  managerUserId?: string | null;
  roleIds: string[];
  departments?: UserDepartmentDto[];
};

type Opt = { id: string; name: string; code?: string };

type DeptRow = { departmentId: string; jobLevelId: string; isPrimary: boolean };

type SheetState =
  | { open: false }
  | { open: true; mode: "create" }
  | { open: true; mode: "view" | "edit"; user: UserDto };

const emptyForm = {
  username: "",
  displayName: "",
  email: "",
  phone: "",
  password: "",
  status: "Active",
  primaryOrgUnitId: "",
};

export default function UsersPage() {
  const { can } = usePermissions();
  const canRead = can("sys.user.read");
  const canManage = can("sys.user.manage");

  const [users, setUsers] = useState<UserDto[]>([]);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [q, setQ] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [sheet, setSheet] = useState<SheetState>({ open: false });
  const [orgs, setOrgs] = useState<Opt[]>([]);
  const [depts, setDepts] = useState<Opt[]>([]);
  const [levels, setLevels] = useState<Opt[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [deptRows, setDeptRows] = useState<DeptRow[]>([]);
  const [selectedRoleIds, setSelectedRoleIds] = useState<Set<string>>(new Set());
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [editMode, setEditMode] = useState(false);

  const roleById = useMemo(() => new Map(roles.map((r) => [r.id, r])), [roles]);

  function roleLabels(ids: string[] | undefined) {
    if (!ids?.length) return "—";
    return ids
      .map((id) => roleById.get(id)?.code ?? id.slice(0, 8))
      .join(", ");
  }

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [{ data }, roleList] = await Promise.all([
        api.get<{ data: UserDto[] }>("/api/sys/users"),
        fetchRoles().catch(() => [] as RoleDto[]),
      ]);
      setUsers(data.data);
      setRoles(roleList);
    } catch {
      setError("Không tải được danh sách user.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
  }, [canRead]);

  async function loadCatalogs() {
    const [o, d, j, r] = await Promise.all([
      api.get<{ data: Opt[] }>("/api/sys/org-units").then((res) => res.data.data),
      api.get<{ data: Opt[] }>("/api/sys/departments").then((res) => res.data.data),
      api.get<{ data: Opt[] }>("/api/sys/job-levels").then((res) => res.data.data),
      fetchRoles().catch(() => [] as RoleDto[]),
    ]);
    setOrgs(o);
    setDepts(d);
    setLevels(j);
    setRoles(r);
  }

  async function openCreate() {
    setFormError(null);
    setEditMode(true);
    setForm(emptyForm);
    setDeptRows([]);
    setSelectedRoleIds(new Set());
    await loadCatalogs().catch(() => undefined);
    setSheet({ open: true, mode: "create" });
  }

  async function openView(user: UserDto) {
    setFormError(null);
    setEditMode(false);
    setForm({
      username: user.username,
      displayName: user.displayName ?? "",
      email: user.email ?? "",
      phone: "",
      password: "",
      status: user.status,
      primaryOrgUnitId: user.primaryOrgUnitId ?? "",
    });
    const rows =
      user.departments?.length
        ? user.departments.map((d) => ({
            departmentId: d.departmentId,
            jobLevelId: d.jobLevelId ?? "",
            isPrimary: d.isPrimary,
          }))
        : user.departmentId
          ? [{ departmentId: user.departmentId, jobLevelId: user.jobLevelId ?? "", isPrimary: true }]
          : [];
    setDeptRows(rows);
    setSelectedRoleIds(new Set(user.roleIds ?? []));
    await loadCatalogs().catch(() => undefined);
    setSheet({ open: true, mode: "view", user });
  }

  function toggleRole(id: string) {
    setSelectedRoleIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  async function onSave(e: React.FormEvent) {
    e.preventDefault();
    if (!canManage) return;
    setSaving(true);
    setFormError(null);
    try {
      const id = sheet.open && sheet.mode !== "create" ? sheet.user.id : null;
      const { data } = await api.post<{ data: UserDto }>("/api/sys/users", {
        id,
        username: form.username,
        displayName: form.displayName || null,
        email: form.email || null,
        phone: form.phone || null,
        password: form.password || null,
        status: form.status,
        primaryOrgUnitId: form.primaryOrgUnitId || null,
        departmentId: null,
        jobLevelId: null,
        managerUserId: null,
        departments: deptRows
          .filter((r) => r.departmentId)
          .map((r) => ({
            departmentId: r.departmentId,
            jobLevelId: r.jobLevelId || null,
            isPrimary: r.isPrimary,
          })),
      });
      const userId = data.data.id;
      await api.put(`/api/sys/users/${userId}/roles`, [...selectedRoleIds]);
      setSheet({ open: false });
      await load();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        "Lưu thất bại.";
      setFormError(msg);
    } finally {
      setSaving(false);
    }
  }

  async function saveRolesOnly() {
    if (!canManage || !sheet.open || sheet.mode === "create") return;
    setSaving(true);
    setFormError(null);
    try {
      await api.put(`/api/sys/users/${sheet.user.id}/roles`, [...selectedRoleIds]);
      setSheet({ open: false });
      await load();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        "Gán vai trò thất bại.";
      setFormError(msg);
    } finally {
      setSaving(false);
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền sys.user.read</p>;
  }

  const filtered = q.trim()
    ? users.filter((u) => {
        const s = q.trim().toLowerCase();
        return (
          u.username.toLowerCase().includes(s) ||
          (u.displayName ?? "").toLowerCase().includes(s) ||
          (u.email ?? "").toLowerCase().includes(s)
        );
      })
    : users;

  const showForm = sheet.open && (sheet.mode === "create" || editMode);
  const title = !sheet.open
    ? ""
    : sheet.mode === "create"
      ? "Thêm người dùng"
      : editMode
        ? "Sửa người dùng"
        : "Chi tiết người dùng";

  const rolePicker = (
    <div className="space-y-2 sm:col-span-2">
      <div className="flex items-center gap-2 text-body text-muted-foreground">
        <Shield className="h-3.5 w-3.5" />
        Vai trò
      </div>
      {roles.length === 0 ? (
        <p className="text-meta text-muted-foreground">
          Không tải được danh sách role (cần sys.role.read hoặc đăng nhập lại).
        </p>
      ) : (
        <div className="max-h-48 space-y-1 overflow-y-auto rounded-md border border-border p-2">
          {roles.map((r) => (
            <label
              key={r.id}
              className="flex cursor-pointer items-center gap-2 rounded px-2 py-1.5 hover:bg-muted/50"
            >
              <input
                type="checkbox"
                disabled={!canManage}
                checked={selectedRoleIds.has(r.id)}
                onChange={() => toggleRole(r.id)}
              />
              <span className="font-mono text-meta font-semibold text-brand-strong">{r.code}</span>
              <span className="text-body">{r.name}</span>
              {r.bypassDataScope && (
                <span className="rounded bg-brand-muted px-1.5 text-meta text-brand-strong">bypass</span>
              )}
            </label>
          ))}
        </div>
      )}
    </div>
  );

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Người dùng</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Nhiều vai trò (hợp quyền) · nhiều phòng ban (1 chính) · mỗi PB một job level
          </p>
        </div>
        {canManage && (
          <button
            type="button"
            onClick={() => void openCreate()}
            className="inline-flex h-9 items-center rounded-md bg-brand px-3 text-body font-semibold text-brand-foreground hover:bg-brand-hover"
          >
            Thêm người dùng
          </button>
        )}
      </div>

      <div className="flex h-9 max-w-md items-center gap-2 rounded-md border border-border bg-surface px-2.5">
        <Search className="h-3.5 w-3.5 text-muted-foreground" />
        <input
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder="Lọc username / tên / email…"
          className="w-full border-0 bg-transparent text-body outline-none"
        />
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {loading ? (
        <p className="text-body text-muted-foreground">Đang tải…</p>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-semibold">Username</th>
                <th className="px-4 py-2.5 font-semibold">Tên</th>
                <th className="px-4 py-2.5 font-semibold">Vai trò</th>
                <th className="px-4 py-2.5 font-semibold">Status</th>
                <th className="px-4 py-2.5 font-semibold" />
              </tr>
            </thead>
            <tbody>
              {filtered.map((u) => (
                <tr key={u.id} className="border-t border-border hover:bg-muted/60">
                  <td className="px-4 py-2.5 font-medium text-foreground">{u.username}</td>
                  <td className="px-4 py-2.5">{u.displayName}</td>
                  <td className="px-4 py-2.5 font-mono text-meta">{roleLabels(u.roleIds)}</td>
                  <td className="px-4 py-2.5">
                    <span className="inline-flex rounded-full bg-brand-muted px-2 py-0.5 text-meta font-semibold text-brand-strong">
                      {u.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-right">
                    <button
                      type="button"
                      onClick={() => void openView(u)}
                      className="inline-flex h-8 items-center gap-1.5 rounded-md border border-brand/25 bg-brand-muted px-2.5 text-meta font-semibold text-brand-strong transition-colors hover:border-brand hover:bg-brand hover:text-brand-foreground"
                    >
                      <Eye className="h-3.5 w-3.5" />
                      Chi tiết
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <SideSheet
        open={sheet.open}
        onOpenChange={(open) => {
          if (!open) setSheet({ open: false });
        }}
        title={title}
        description={
          sheet.open && sheet.mode !== "create" ? sheet.user.username : "Tài khoản đăng nhập SYS"
        }
        widthClassName="max-w-lg"
        footer={
          showForm ? (
            <div className="flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setSheet({ open: false })}
                className="h-9 rounded-md border border-border px-3 text-body font-medium hover:bg-muted"
              >
                Hủy
              </button>
              <button
                type="submit"
                form="user-sheet-form"
                disabled={saving}
                className="h-9 rounded-md bg-brand px-4 text-body font-semibold text-brand-foreground hover:bg-brand-hover disabled:opacity-60"
              >
                {saving ? "Đang lưu…" : "Lưu (kèm vai trò)"}
              </button>
            </div>
          ) : (
            <div className="flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setSheet({ open: false })}
                className="h-9 rounded-md border border-border px-3 text-body font-medium hover:bg-muted"
              >
                Đóng
              </button>
              {canManage && (
                <>
                  <button
                    type="button"
                    disabled={saving}
                    onClick={() => void saveRolesOnly()}
                    className="h-9 rounded-md border border-brand/30 bg-brand-muted px-3 text-body font-semibold text-brand-strong hover:bg-brand hover:text-brand-foreground disabled:opacity-60"
                  >
                    Lưu vai trò
                  </button>
                  <button
                    type="button"
                    onClick={() => setEditMode(true)}
                    className="h-9 rounded-md bg-brand px-4 text-body font-semibold text-brand-foreground hover:bg-brand-hover"
                  >
                    Sửa
                  </button>
                </>
              )}
            </div>
          )
        }
      >
        {showForm ? (
          <form id="user-sheet-form" onSubmit={(e) => void onSave(e)} className="grid gap-3 sm:grid-cols-2">
            <label className="space-y-1 text-body">
              <span className="text-muted-foreground">Username</span>
              <input
                required
                disabled={sheet.open && sheet.mode !== "create"}
                className="h-9 w-full rounded-md border border-border bg-background px-2 disabled:opacity-60"
                value={form.username}
                onChange={(e) => setForm((f) => ({ ...f, username: e.target.value }))}
              />
            </label>
            <label className="space-y-1 text-body">
              <span className="text-muted-foreground">Họ tên</span>
              <input
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.displayName}
                onChange={(e) => setForm((f) => ({ ...f, displayName: e.target.value }))}
              />
            </label>
            <label className="space-y-1 text-body">
              <span className="text-muted-foreground">Email</span>
              <input
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.email}
                onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
              />
            </label>
            <label className="space-y-1 text-body">
              <span className="text-muted-foreground">
                Mật khẩu{sheet.open && sheet.mode !== "create" ? " (để trống = giữ)" : ""}
              </span>
              <input
                type="password"
                required={sheet.open && sheet.mode === "create"}
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.password}
                onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
              />
            </label>
            <label className="space-y-1 text-body">
              <span className="text-muted-foreground">Đơn vị</span>
              <select
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.primaryOrgUnitId}
                onChange={(e) => setForm((f) => ({ ...f, primaryOrgUnitId: e.target.value }))}
              >
                <option value="">—</option>
                {orgs.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </label>
            <div className="space-y-2 sm:col-span-2">
              <div className="flex items-center justify-between">
                <span className="text-body text-muted-foreground">
                  Phòng ban (1 chính · nhiều ngang hàng · mỗi PB một cấp bậc)
                </span>
                <button
                  type="button"
                  className="text-meta font-semibold text-brand-strong"
                  onClick={() =>
                    setDeptRows((rows) => [
                      ...rows,
                      {
                        departmentId: "",
                        jobLevelId: "",
                        isPrimary: rows.length === 0,
                      },
                    ])
                  }
                >
                  + Thêm phòng ban
                </button>
              </div>
              {deptRows.length === 0 && (
                <p className="text-meta text-muted-foreground">Chưa gán phòng ban.</p>
              )}
              {deptRows.map((row, idx) => (
                <div key={idx} className="flex flex-wrap items-center gap-2 rounded-md border border-border p-2">
                  <select
                    className="h-9 min-w-[140px] flex-1 rounded-md border border-border bg-background px-2"
                    value={row.departmentId}
                    onChange={(e) => {
                      const v = e.target.value;
                      setDeptRows((rows) => rows.map((r, i) => (i === idx ? { ...r, departmentId: v } : r)));
                    }}
                  >
                    <option value="">— phòng ban —</option>
                    {depts.map((o) => (
                      <option key={o.id} value={o.id}>
                        {o.name}
                      </option>
                    ))}
                  </select>
                  <select
                    className="h-9 min-w-[120px] flex-1 rounded-md border border-border bg-background px-2"
                    value={row.jobLevelId}
                    onChange={(e) => {
                      const v = e.target.value;
                      setDeptRows((rows) => rows.map((r, i) => (i === idx ? { ...r, jobLevelId: v } : r)));
                    }}
                  >
                    <option value="">— cấp bậc —</option>
                    {levels.map((o) => (
                      <option key={o.id} value={o.id}>
                        {o.name}
                      </option>
                    ))}
                  </select>
                  <label className="flex items-center gap-1 text-meta whitespace-nowrap">
                    <input
                      type="radio"
                      name="primary-dept"
                      checked={row.isPrimary}
                      onChange={() =>
                        setDeptRows((rows) => rows.map((r, i) => ({ ...r, isPrimary: i === idx })))
                      }
                    />
                    Chính
                  </label>
                  <button
                    type="button"
                    className="text-meta text-destructive"
                    onClick={() => {
                      setDeptRows((rows) => {
                        const next = rows.filter((_, i) => i !== idx);
                        if (next.length && !next.some((r) => r.isPrimary)) next[0].isPrimary = true;
                        return [...next];
                      });
                    }}
                  >
                    Xóa
                  </button>
                </div>
              ))}
            </div>
            <label className="space-y-1 text-body">
              <span className="text-muted-foreground">Status</span>
              <select
                className="h-9 w-full rounded-md border border-border bg-background px-2"
                value={form.status}
                onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))}
              >
                <option value="Active">Active</option>
                <option value="Locked">Locked</option>
                <option value="Disabled">Disabled</option>
              </select>
            </label>
            {rolePicker}
            {formError && <p className="text-body text-destructive sm:col-span-2">{formError}</p>}
          </form>
        ) : sheet.open && sheet.mode !== "create" ? (
          <div className="space-y-4">
            <dl className="grid gap-3 sm:grid-cols-2">
              {(
                [
                  ["Username", sheet.user.username],
                  ["Họ tên", sheet.user.displayName],
                  ["Email", sheet.user.email],
                  ["Status", sheet.user.status],
                ] as [string, string | null | undefined][]
              ).map(([label, value]) => (
                <div key={label}>
                  <dt className="text-meta text-muted-foreground">{label}</dt>
                  <dd className="text-body font-medium text-foreground">{value || "—"}</dd>
                </div>
              ))}
            </dl>
            <div>
              <dt className="text-meta text-muted-foreground">Phòng ban</dt>
              <ul className="mt-1 space-y-1 text-body">
                {(sheet.user.departments?.length
                  ? sheet.user.departments
                  : sheet.user.departmentId
                    ? [
                        {
                          departmentId: sheet.user.departmentId,
                          departmentName: depts.find((d) => d.id === sheet.user.departmentId)?.name,
                          jobLevelId: sheet.user.jobLevelId,
                          jobLevelName: levels.find((l) => l.id === sheet.user.jobLevelId)?.name,
                          isPrimary: true,
                        },
                      ]
                    : []
                ).map((d) => (
                  <li key={d.departmentId}>
                    {d.departmentName ?? d.departmentId.slice(0, 8)}
                    {d.isPrimary ? " · chính" : ""}
                    {d.jobLevelName ? ` · ${d.jobLevelName}` : ""}
                  </li>
                ))}
                {!sheet.user.departments?.length && !sheet.user.departmentId && <li>—</li>}
              </ul>
            </div>
            {rolePicker}
            {formError && <p className="text-body text-destructive">{formError}</p>}
          </div>
        ) : null}
      </SideSheet>
    </div>
  );
}
