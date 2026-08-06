"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/shared/api/client";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { statusPill, tableWrap, td, th } from "@/shared/ui/field";
import { cn } from "@/shared/lib/cn";

type Row = {
  id: string;
  username: string;
  success: boolean;
  ipAddress?: string | null;
  failureReason?: string | null;
  attemptedAt: string;
};

export default function LoginAuditsPage() {
  const { can } = usePermissions();
  const [rows, setRows] = useState<Row[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<"all" | "ok" | "fail">("all");

  const load = useCallback(() => {
    if (!can("sys.license.manage")) return;
    setLoading(true);
    setError(null);
    void api
      .get<{ data: Row[] }>("/api/sys/login-audits?take=100")
      .then((r) => setRows(r.data.data))
      .catch(() => setError("Không tải được nhật ký đăng nhập."))
      .finally(() => setLoading(false));
  }, [can]);

  useEffect(() => {
    load();
  }, [load]);

  if (!can("sys.license.manage")) {
    return <p className="text-body text-destructive">Không có quyền sys.license.manage</p>;
  }

  const filtered = rows.filter((r) => {
    if (filter === "ok") return r.success;
    if (filter === "fail") return !r.success;
    return true;
  });

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Nhật ký đăng nhập</h1>
          <p className="mt-1 text-body text-muted-foreground">100 lần thử gần nhất · theo dõi thành công / thất bại</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {(
            [
              ["all", "Tất cả"],
              ["ok", "Thành công"],
              ["fail", "Thất bại"],
            ] as const
          ).map(([id, label]) => (
            <button
              key={id}
              type="button"
              className={filter === id ? btn.soft : btn.ghost}
              onClick={() => setFilter(id)}
            >
              {label}
            </button>
          ))}
          <button type="button" className={btn.ghost} onClick={load} disabled={loading}>
            {loading ? "Đang tải…" : "Làm mới"}
          </button>
        </div>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}

      <div className={tableWrap}>
        <table className="w-full text-body">
          <thead className="border-b border-border bg-muted">
            <tr>
              <th className={th}>Thời điểm</th>
              <th className={th}>User</th>
              <th className={th}>Kết quả</th>
              <th className={th}>IP</th>
              <th className={th}>Chi tiết</th>
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-3 py-8 text-center text-muted-foreground">
                  {loading ? "Đang tải…" : "Không có bản ghi."}
                </td>
              </tr>
            ) : (
              filtered.map((r) => (
                <tr key={r.id} className="border-t border-border">
                  <td className={cn(td, "tabular-nums text-muted-foreground")}>
                    {new Date(r.attemptedAt).toLocaleString("vi-VN")}
                  </td>
                  <td className={cn(td, "font-mono text-brand-strong")}>{r.username}</td>
                  <td className={td}>
                    <span className={statusPill(r.success ? "success" : "danger")}>
                      {r.success ? "Thành công" : "Thất bại"}
                    </span>
                  </td>
                  <td className={cn(td, "tabular-nums")}>{r.ipAddress ?? "—"}</td>
                  <td className={cn(td, "text-muted-foreground")}>
                    {r.success ? "—" : r.failureReason ?? "FAIL"}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
