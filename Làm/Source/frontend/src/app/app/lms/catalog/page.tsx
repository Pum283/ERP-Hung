"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  enrollLmsCourse,
  fetchLmsCatalog,
  type LmsCatalogCourseDto,
} from "@/shared/api/lms-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill } from "@/shared/ui/field";

function money(n: number, currency: string) {
  if (n <= 0) return "Miễn phí";
  return `${n.toLocaleString("vi-VN")} ${currency}`;
}

export default function LmsCatalogPage() {
  const { can } = usePermissions();
  const canRead = can("lms.learn.read");
  const canEnroll = can("lms.learn.enroll");

  const [items, setItems] = useState<LmsCatalogCourseDto[]>([]);
  const [voucher, setVoucher] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    setItems(await fetchLmsCatalog());
  }, []);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    load()
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  async function enroll(courseId: string) {
    try {
      await enrollLmsCourse(courseId, voucher || undefined);
      setOk("Đã mở khóa khóa học (thanh toán mock / voucher).");
      setError(null);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem catalog học online.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Học online</h1>
          <p className="mt-1 text-sm text-[var(--muted)]">
            Catalog khóa đã xuất bản · mua / voucher · vào học (UC_LMS_030–033)
          </p>
        </div>
        {canEnroll && (
          <div className="flex items-center gap-2">
            <input
              className={`${field} w-40`}
              placeholder="Voucher (FREE)"
              value={voucher}
              onChange={(e) => setVoucher(e.target.value)}
            />
          </div>
        )}
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
        {items.map((c) => {
          const unlocked = c.enrollmentStatus === "Unlocked" || c.enrollmentStatus === "Completed";
          return (
            <article key={c.id} className={panel}>
              <div className="mb-2 flex items-start justify-between gap-2">
                <div>
                  <h2 className="text-sm font-semibold">{c.name}</h2>
                  <p className="text-xs text-[var(--muted)]">{c.code} · {c.deliveryMode}</p>
                </div>
                {c.enrollmentStatus && (
                  <span className={statusPill(unlocked ? "success" : "warning")}>{c.enrollmentStatus}</span>
                )}
              </div>
              <p className="mb-3 line-clamp-3 text-sm text-[var(--muted)]">
                {c.summary || "Không có mô tả."}
              </p>
              <div className="mb-3 flex items-center justify-between text-xs text-[var(--muted)]">
                <span>{c.lessonCount} bài</span>
                <span className="font-medium text-[var(--fg)]">{money(c.price, c.currency)}</span>
              </div>
              {unlocked && (
                <div className="mb-3 h-1.5 overflow-hidden rounded-full bg-[var(--surface-2)]">
                  <div
                    className="h-full bg-[var(--brand)]"
                    style={{ width: `${Math.min(100, c.progressPct)}%` }}
                  />
                </div>
              )}
              <div className="flex flex-wrap gap-2">
                {unlocked ? (
                  <Link href={`/app/lms/learn/${c.id}`} className={btn.primary}>
                    {c.progressPct > 0 ? `Tiếp tục (${c.progressPct}%)` : "Vào học"}
                  </Link>
                ) : canEnroll ? (
                  <button type="button" className={btn.primary} onClick={() => enroll(c.id)}>
                    {c.price > 0 ? "Mua & mở khóa" : "Ghi danh miễn phí"}
                  </button>
                ) : (
                  <span className="text-xs text-[var(--muted)]">Cần quyền ghi danh</span>
                )}
              </div>
            </article>
          );
        })}
      </div>

      {!loading && items.length === 0 && (
        <p className="text-sm text-[var(--muted)]">
          Chưa có khóa Published. Vào <Link className="underline" href="/app/lms/courses">Khóa học (catalog)</Link> để tạo & xuất bản.
        </p>
      )}
    </div>
  );
}
