"use client";

import { useEffect, useMemo, useState } from "react";
import type { PermissionDto } from "@/shared/api/sys-api";
import { btn } from "@/shared/ui/btn";

type Props = {
  permissions: PermissionDto[];
  selectedIds: Set<string>;
  onChange: (ids: Set<string>) => void;
  resetKey?: string;
};

export function PermissionPicker({ permissions, selectedIds, onChange, resetKey = "" }: Props) {
  const [search, setSearch] = useState("");
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set());

  const active = useMemo(() => permissions.filter((p) => p.isActive !== false), [permissions]);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return active;
    return active.filter((p) =>
      [p.code, p.name, p.resource, p.moduleCode, p.description ?? ""].join(" ").toLowerCase().includes(q)
    );
  }, [active, search]);

  const groups = useMemo(() => {
    const map = new Map<string, PermissionDto[]>();
    for (const p of filtered) {
      const list = map.get(p.moduleCode) ?? [];
      list.push(p);
      map.set(p.moduleCode, list);
    }
    return [...map.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([moduleCode, items]) => ({
        moduleCode,
        items: items.sort((a, b) => a.code.localeCompare(b.code)),
      }));
  }, [filtered]);

  useEffect(() => {
    const withSel = new Set(
      active.filter((p) => selectedIds.has(p.id)).map((p) => p.moduleCode)
    );
    const next = new Set<string>();
    for (const p of active) {
      if (!withSel.has(p.moduleCode)) next.add(p.moduleCode);
    }
    setCollapsed(next);
    setSearch("");
    // Chỉ reset khi mở lại panel (resetKey)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [resetKey]);

  function toggle(id: string) {
    const next = new Set(selectedIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onChange(next);
  }

  function toggleModule(moduleCode: string, items: PermissionDto[]) {
    const next = new Set(selectedIds);
    const allOn = items.every((p) => next.has(p.id));
    for (const p of items) {
      if (allOn) next.delete(p.id);
      else next.add(p.id);
    }
    onChange(next);
  }

  return (
    <div className="space-y-3">
      <input
        className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-body"
        placeholder="Tìm mã / tên / module…"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />
      <div className="max-h-[420px] space-y-2 overflow-y-auto rounded-lg border border-border p-2">
        {groups.map((g) => {
          const isCollapsed = collapsed.has(g.moduleCode);
          const selectedCount = g.items.filter((p) => selectedIds.has(p.id)).length;
          return (
            <div key={g.moduleCode} className="rounded-md border border-border/70">
              <div className="flex items-center gap-2 bg-muted/50 px-2 py-1.5">
                <button
                  type="button"
                  className="text-meta font-bold uppercase text-brand-strong"
                  onClick={() => {
                    const next = new Set(collapsed);
                    if (next.has(g.moduleCode)) next.delete(g.moduleCode);
                    else next.add(g.moduleCode);
                    setCollapsed(next);
                  }}
                >
                  {isCollapsed ? "▸" : "▾"} {g.moduleCode}
                </button>
                <span className="text-meta text-muted-foreground">
                  {selectedCount}/{g.items.length}
                </span>
                <button
                  type="button"
                  className={`ml-auto ${btn.ghost}`}
                  onClick={() => toggleModule(g.moduleCode, g.items)}
                >
                  {g.items.every((p) => selectedIds.has(p.id)) ? "Bỏ chọn" : "Chọn hết"}
                </button>
              </div>
              {!isCollapsed && (
                <ul className="divide-y divide-border/60">
                  {g.items.map((p) => (
                    <li key={p.id}>
                      <label className="flex cursor-pointer items-start gap-2 px-2 py-1.5 hover:bg-muted/40">
                        <input
                          type="checkbox"
                          className="mt-1"
                          checked={selectedIds.has(p.id)}
                          onChange={() => toggle(p.id)}
                        />
                        <span>
                          <span className="font-mono text-meta font-semibold">{p.code}</span>
                          <span className="ml-2 text-body">{p.name}</span>
                          <span className="mt-0.5 block text-meta text-muted-foreground">
                            {p.resource} · {p.action}
                          </span>
                        </span>
                      </label>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          );
        })}
        {groups.length === 0 && (
          <p className="p-3 text-body text-muted-foreground">Không có quyền khớp tìm kiếm.</p>
        )}
      </div>
    </div>
  );
}
