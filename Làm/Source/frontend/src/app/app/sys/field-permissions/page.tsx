"use client";

import React, { useEffect, useState } from "react";
import {
  fetchRoles,
  fetchSensitiveFields,
  fetchRoleFieldPermissions,
  upsertSensitiveField,
  upsertRoleFieldPermission,
  type RoleDto,
  type SysSensitiveFieldDto,
  type SysRoleFieldPermissionDto,
} from "@/shared/api/sys-api";
import {
  applyFieldMaskUi,
  isAllowedFieldAccess,
} from "@/shared/api/sys-sso-field-config-push-helpers";
import { RefreshCw, Shield } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function FieldPermissionsPage() {
  const [fields, setFields] = useState<SysSensitiveFieldDto[]>([]);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [roleId, setRoleId] = useState("");
  const [rolePerms, setRolePerms] = useState<SysRoleFieldPermissionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);

  const [moduleCode, setModuleCode] = useState("HRM");
  const [entityName, setEntityName] = useState("Employee");
  const [fieldKey, setFieldKey] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [defaultMask, setDefaultMask] = useState("Mask");
  const [permFieldId, setPermFieldId] = useState("");
  const [access, setAccess] = useState("Masked");
  const [preview, setPreview] = useState("0912345678");

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const [f, r] = await Promise.all([fetchSensitiveFields(), fetchRoles()]);
      setFields(f);
      setRoles(r);
      if (!roleId && r[0]) setRoleId(r[0].id);
      if (!permFieldId && f[0]) setPermFieldId(f[0].id);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }

  async function loadRolePerms(id: string) {
    if (!id) {
      setRolePerms([]);
      return;
    }
    try {
      setRolePerms(await fetchRoleFieldPermissions(id));
    } catch (err) {
      setError((err as Error).message);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  useEffect(() => {
    void loadRolePerms(roleId);
  }, [roleId]);

  async function onAddField(e: React.FormEvent) {
    e.preventDefault();
    if (!fieldKey.trim() || !displayName.trim()) {
      setError("FieldKey và DisplayName bắt buộc.");
      return;
    }
    try {
      setError(null);
      await upsertSensitiveField({
        moduleCode,
        entityName,
        fieldKey,
        displayName,
        defaultMask,
        isActive: true,
      });
      setMsg("Đã thêm trường nhạy cảm.");
      setFieldKey("");
      setDisplayName("");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onAssign(e: React.FormEvent) {
    e.preventDefault();
    if (!roleId || !permFieldId) {
      setError("Chọn role và trường.");
      return;
    }
    if (!isAllowedFieldAccess(access)) {
      setError("Access không hợp lệ.");
      return;
    }
    try {
      setError(null);
      await upsertRoleFieldPermission({ roleId, sensitiveFieldId: permFieldId, access });
      setMsg("Đã gán quyền trường.");
      await loadRolePerms(roleId);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="max-w-5xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground flex items-center gap-2">
            <Shield className="w-6 h-6 text-brand" /> Quyền trường nhạy cảm (UC_SYS_031)
          </h1>
          <p className="text-body text-muted-foreground mt-1">
            Danh mục trường + gán None/Masked/Read/Write theo vai trò. Hiệu lực lấy quyền rộng nhất.
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      <div className={`${panel} space-y-2`}>
        <div className="text-sm font-semibold">Xem trước mask</div>
        <div className="flex flex-wrap gap-3 items-end">
          <label className="block space-y-1">
            <span className="text-xs text-muted-foreground">Giá trị thô</span>
            <input className={field} value={preview} onChange={(e) => setPreview(e.target.value)} />
          </label>
          <div className="text-sm text-foreground">
            Masked: <span className="font-mono">{applyFieldMaskUi(preview, "Masked")}</span>
            {" · "}
            None: <span className="font-mono">{applyFieldMaskUi(preview, "None")}</span>
          </div>
        </div>
      </div>

      <form onSubmit={(e) => void onAddField(e)} className={`${panel} space-y-3`}>
        <div className="text-sm font-semibold">Thêm trường nhạy cảm</div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <input className={field} placeholder="Module" value={moduleCode} onChange={(e) => setModuleCode(e.target.value)} />
          <input className={field} placeholder="Entity" value={entityName} onChange={(e) => setEntityName(e.target.value)} />
          <input className={field} placeholder="FieldKey" value={fieldKey} onChange={(e) => setFieldKey(e.target.value)} />
          <input className={field} placeholder="DisplayName" value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
          <select className={field} value={defaultMask} onChange={(e) => setDefaultMask(e.target.value)}>
            <option value="Hide">Hide</option>
            <option value="Mask">Mask</option>
            <option value="ReadOnly">ReadOnly</option>
          </select>
          <button type="submit" className={btn.primary}>Thêm trường</button>
        </div>
      </form>

      <div className="bg-surface shadow rounded-xl border border-border overflow-hidden">
        {loading ? (
          <div className="p-4 text-sm text-muted-foreground">Đang tải…</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-slate-50 dark:bg-slate-800/50 text-left">
              <tr>
                <th className="px-4 py-2">Module</th>
                <th className="px-4 py-2">Entity.Field</th>
                <th className="px-4 py-2">Tên</th>
                <th className="px-4 py-2">Default</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
              {fields.map((f) => (
                <tr key={f.id}>
                  <td className="px-4 py-2">{f.moduleCode}</td>
                  <td className="px-4 py-2 font-mono text-xs">{f.entityName}.{f.fieldKey}</td>
                  <td className="px-4 py-2">{f.displayName}</td>
                  <td className="px-4 py-2">{f.defaultMask}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <form onSubmit={(e) => void onAssign(e)} className={`${panel} space-y-3`}>
        <div className="text-sm font-semibold">Gán quyền theo vai trò</div>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
          <select className={field} value={roleId} onChange={(e) => setRoleId(e.target.value)}>
            {roles.map((r) => (
              <option key={r.id} value={r.id}>{r.code} — {r.name}</option>
            ))}
          </select>
          <select className={field} value={permFieldId} onChange={(e) => setPermFieldId(e.target.value)}>
            {fields.map((f) => (
              <option key={f.id} value={f.id}>{f.fieldKey}</option>
            ))}
          </select>
          <select className={field} value={access} onChange={(e) => setAccess(e.target.value)}>
            <option value="None">None</option>
            <option value="Masked">Masked</option>
            <option value="Read">Read</option>
            <option value="Write">Write</option>
          </select>
          <button type="submit" className={btn.primary}>Gán quyền</button>
        </div>
        <ul className="text-sm text-muted-foreground space-y-1">
          {rolePerms.map((p) => (
            <li key={p.id} className="font-mono text-xs">{p.fieldKey} → {p.access}</li>
          ))}
          {rolePerms.length === 0 && <li className="text-slate-400">Chưa gán quyền cho role này.</li>}
        </ul>
      </form>
    </div>
  );
}
