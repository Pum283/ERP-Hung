"use client";

import { useEffect, useMemo, useState } from "react";
import { api } from "@/shared/api/client";
import type { PermissionDto } from "@/shared/api/sys-api";
import { usePermissions } from "@/shared/hooks/use-permissions";

export default function PermissionsPage() {
  const { can } = usePermissions();
  const [rows, setRows] = useState<PermissionDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [q, setQ] = useState("");
  const [moduleFilter, setModuleFilter] = useState("");

  const canRead =
    can("sys.permission.read") || can("sys.role.read") || can("sys.role.manage") || can("sys.role.assign");

  async function load() {
    const { data } = await api.get<{ data: PermissionDto[] }>("/api/sys/permissions", {
      params: { includeInactive: false },
    });
    setRows(data.data);
  }

  useEffect(() => {
    if (!canRead) return;
    void load().catch(() => setError("Không tải được danh mục quyền."));
  }, [canRead]);

  const modules = useMemo(
    () => [...new Set(rows.map((r) => r.moduleCode))].sort((a, b) => a.localeCompare(b)),
    [rows],
  );

  const filtered = useMemo(() => {
    const qq = q.trim().toLowerCase();
    return rows.filter((p) => {
      if (moduleFilter && p.moduleCode !== moduleFilter) return false;
      if (!qq) return true;
      return (
        p.code.toLowerCase().includes(qq) ||
        p.name.toLowerCase().includes(qq) ||
        p.resource.toLowerCase().includes(qq)
      );
    });
  }, [rows, q, moduleFilter]);

  const byModule = filtered.reduce<Record<string, PermissionDto[]>>((acc, p) => {
    (acc[p.moduleCode] ??= []).push(p);
    return acc;
  }, {});

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền xem danh mục quyền.</p>;
  }

  return (
    <div className="space-y-4">
      <div>
        <h1 className="font-display text-title font-bold">Danh mục quyền</h1>
        <p className="mt-1 text-body text-muted-foreground">
          Chỉ xem — quyền được <strong>seed tự động</strong> khi làm chức năng. Gán quyền vào role tại trang Vai trò.
        </p>
      </div>
      {error && <p className="text-destructive">{error}</p>}

      <div className="flex flex-wrap gap-2">
        <input
          className="min-w-[200px] flex-1 rounded border border-border bg-surface px-3 py-2"
          placeholder="Tìm code / tên / resource…"
          value={q}
          onChange={(e) => setQ(e.target.value)}
        />
        <select
          className="rounded border border-border bg-surface px-3 py-2"
          value={moduleFilter}
          onChange={(e) => setModuleFilter(e.target.value)}
        >
          <option value="">Mọi module</option>
          {modules.map((m) => (
            <option key={m} value={m}>
              {m}
            </option>
          ))}
        </select>
      </div>

      <p className="text-meta text-muted-foreground">{filtered.length} / {rows.length} quyền</p>

      {Object.entries(byModule)
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([mod, items]) => (
          <section key={mod} className="rounded-xl border border-border bg-surface p-4">
            <h2 className="mb-2 font-mono text-meta font-bold uppercase text-brand-strong">{mod}</h2>
            <ul className="divide-y divide-border/60">
              {items
                .sort((a, b) => a.code.localeCompare(b.code))
                .map((p) => (
                  <li key={p.id} className="flex flex-wrap items-baseline gap-2 py-2">
                    <span className="font-mono text-meta font-semibold">{p.code}</span>
                    <span className="text-body">{p.name}</span>
                    <span className="text-meta text-muted-foreground">
                      {p.resource}.{p.action}
                    </span>
                  </li>
                ))}
            </ul>
          </section>
        ))}
    </div>
  );
}
