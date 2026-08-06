"use client";

import { useEffect, useState } from "react";
import { fetchMyLmsCertificates, type LmsCertificateDto } from "@/shared/api/lms-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function LmsCertificatesPage() {
  const { can } = usePermissions();
  const canRead = can("lms.learn.read");
  const [items, setItems] = useState<LmsCertificateDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    fetchMyLmsCertificates()
      .then(setItems)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead]);

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem chứng chỉ.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Chứng chỉ của tôi</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Cấp khi hoàn thành bài học + đậu thi Final (UC_LMS_044–045)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <section className={panel}>
        <div className={tableWrap}>
          <table className="w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Mã xác thực</th>
                <th className={th}>Khóa học</th>
                <th className={th}>Điểm</th>
                <th className={th}>Ngày cấp</th>
                <th className={th}>TT</th>
              </tr>
            </thead>
            <tbody>
              {items.map((c) => (
                <tr key={c.id}>
                  <td className={`${td} font-mono`}>{c.code}</td>
                  <td className={td}>{c.courseName}</td>
                  <td className={td}>{c.scoreAtIssue != null ? `${c.scoreAtIssue}%` : "—"}</td>
                  <td className={td}>{new Date(c.issuedAt).toLocaleString("vi-VN")}</td>
                  <td className={td}>
                    <span className={statusPill(c.status === "Active" ? "success" : "danger")}>{c.status}</span>
                  </td>
                </tr>
              ))}
              {!loading && items.length === 0 && (
                <tr>
                  <td className={td} colSpan={5}>
                    Chưa có chứng chỉ. Hoàn thành bài học và đậu thi Final để được cấp.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
