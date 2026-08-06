"use client";

import { FormEvent, useEffect, useState } from "react";
import { api } from "@/shared/api/client";
import {
  fetchPermissions,
  fetchRoles,
  type PermissionDto,
  type RoleDto,
} from "@/shared/api/sys-api";
import { CanAccess } from "@/shared/auth/CanAccess";
import { PermissionPicker } from "@/components/sys/PermissionPicker";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

export default function RolesPage() {
  const { can } = usePermissions();
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [perms, setPerms] = useState<PermissionDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<RoleDto | null>(null);
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [bypass, setBypass] = useState(false);
  const [isActive, setIsActive] = useState(true);

  const [permOpen, setPermOpen] = useState(false);
  const [permRole, setPermRole] = useState<RoleDto | null>(null);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [saving, setSaving] = useState(false);

  const canRead = can("sys.role.read") || can("sys.role.manage");
  const canUpdate = can("sys.role.update") || can("sys.role.manage");
  const canAssign = can("sys.role.assign") || can("sys.role.manage");

  async function load() {
    const [r, p] = await Promise.all([fetchRoles(), fetchPermissions()]);
    setRoles(r);
    setPerms(p);
  }

  useEffect(() => {
    if (!canRead) return;
    void load().catch(() => setError("Không tải được vai trò / quyền."));
  }, [canRead]);

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền sys.role.read</p>;
  }

  function openCreate() {
    setEditing(null);
    setCode("");
    setName("");
    setBypass(false);
    setIsActive(true);
    setFormOpen(true);
  }

  function openEdit(role: RoleDto) {
    setEditing(role);
    setCode(role.code);
    setName(role.name);
    setBypass(role.bypassDataScope);
    setIsActive(role.isActive);
    setFormOpen(true);
  }

  function openPerms(role: RoleDto) {
    setPermRole(role);
    setSelected(new Set(role.permissionIds));
    setPermOpen(true);
  }

  async function saveRole(e: FormEvent) {
    e.preventDefault();
    setSaving(true);
    setError(null);
    try {
      await api.post("/api/sys/roles", {
        id: editing?.id ?? null,
        code: code.trim().toUpperCase(),
        name: name.trim(),
        description: null,
        bypassDataScope: bypass,
        isActive,
      });
      setOk(editing ? "Đã cập nhật vai trò" : "Đã tạo vai trò");
      setFormOpen(false);
      await load();
    } catch {
      setError("Lưu vai trò thất bại");
    } finally {
      setSaving(false);
    }
  }

  async function savePerms() {
    if (!permRole) return;
    setSaving(true);
    setError(null);
    try {
      await api.put(`/api/sys/roles/${permRole.id}/permissions`, [...selected]);
      setOk(`Đã gán ${selected.size} quyền cho ${permRole.code}`);
      setPermOpen(false);
      await load();
    } catch {
      setError("Gán quyền thất bại");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Vai trò</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Digi RBAC · User → Role → Permission · BypassDataScope
          </p>
        </div>
        <CanAccess permission="sys.role.update">
          <button type="button" onClick={openCreate} className={btn.primary}>
            Thêm vai trò
          </button>
        </CanAccess>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      <div className="space-y-3">
        {roles.map((r) => (
          <div key={r.id} className="rounded-xl border border-border bg-surface p-4 shadow-sm">
            <div className="flex flex-wrap items-center gap-2">
              <span className="font-mono text-meta font-bold text-brand-strong">{r.code}</span>
              <span className="font-semibold">{r.name}</span>
              {r.bypassDataScope && (
                <span className="rounded bg-brand-muted px-2 py-0.5 text-meta text-brand-strong">
                  bypass
                </span>
              )}
              {r.isSystem && (
                <span className="rounded bg-muted px-2 py-0.5 text-meta text-muted-foreground">system</span>
              )}
              {!r.isActive && <span className="text-meta text-muted-foreground">inactive</span>}
              <span className="ml-auto text-meta text-muted-foreground">{r.permissionIds.length} quyền</span>
            </div>
            <div className="mt-3 flex flex-wrap gap-2">
              {canUpdate && (
                <button type="button" className={btn.soft} onClick={() => openEdit(r)}>
                  Sửa
                </button>
              )}
              {canAssign && (
                <button type="button" className={btn.soft} onClick={() => openPerms(r)}>
                  Gán quyền
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {formOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <form
            onSubmit={saveRole}
            className="w-full max-w-md space-y-3 rounded-xl border border-border bg-surface p-4 shadow-lg"
          >
            <h2 className="font-semibold">{editing ? "Sửa vai trò" : "Tạo vai trò"}</h2>
            <input
              className="w-full rounded border px-3 py-2 font-mono"
              placeholder="CODE"
              value={code}
              disabled={!!editing?.isSystem}
              onChange={(e) => setCode(e.target.value)}
              required
            />
            <input
              className="w-full rounded border px-3 py-2"
              placeholder="Tên hiển thị"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
            <label className="flex items-center gap-2 text-body">
              <input type="checkbox" checked={bypass} onChange={(e) => setBypass(e.target.checked)} />
              BypassDataScope (super — mọi quyền + mọi dữ liệu)
            </label>
            <label className="flex items-center gap-2 text-body">
              <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
              Active
            </label>
            <div className="flex justify-end gap-2">
              <button type="button" className={btn.secondary} onClick={() => setFormOpen(false)}>
                Hủy
              </button>
              <button type="submit" disabled={saving} className={btn.primary}>
                Lưu
              </button>
            </div>
          </form>
        </div>
      )}

      {permOpen && permRole && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-2xl space-y-3 rounded-xl border border-border bg-surface p-4 shadow-lg">
            <h2 className="font-semibold">
              Gán quyền · <span className="font-mono text-brand-strong">{permRole.code}</span>
            </h2>
            <PermissionPicker
              permissions={perms}
              selectedIds={selected}
              onChange={setSelected}
              resetKey={permRole.id}
            />
            <div className="flex justify-end gap-2">
              <button type="button" className={btn.secondary} onClick={() => setPermOpen(false)}>
                Hủy
              </button>
              <button type="button" disabled={saving} onClick={() => void savePerms()} className={btn.primary}>
                Lưu {selected.size} quyền
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
