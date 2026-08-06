/** Pure helpers — báo cáo POS Cap-2 (UC_POS_065/066/067). FE mirror BE. */

export function rankTopProducts<T extends { qty: number; revenue: number }>(
  rows: T[],
  by: "qty" | "revenue" = "qty",
  top = 10,
): (T & { rank: number })[] {
  const sorted = [...rows].sort((a, b) =>
    by === "qty" ? b.qty - a.qty || b.revenue - a.revenue : b.revenue - a.revenue || b.qty - a.qty,
  );
  return sorted.slice(0, Math.max(1, top)).map((r, i) => ({ ...r, rank: i + 1 }));
}

export function computeStoreShare(
  rows: { revenue: number }[],
): number[] {
  const total = rows.reduce((s, r) => s + r.revenue, 0);
  if (total === 0) return rows.map(() => 0);
  return rows.map((r) => Math.round((r.revenue * 10000) / total) / 100);
}

export function avgTicket(revenue: number, saleCount: number): number {
  if (saleCount <= 0) return 0;
  return Math.round((revenue / saleCount) * 100) / 100;
}

export function costVariance(theoreticalCost: number, actualCost: number): {
  variance: number;
  variancePercent: number;
} {
  const variance = Math.round((actualCost - theoreticalCost) * 100) / 100;
  const variancePercent =
    theoreticalCost === 0 ? 0 : Math.round((variance * 10000) / theoreticalCost) / 100;
  return { variance, variancePercent };
}

/** Tô màu variance: dương (vượt định mức) = danger, âm = success, ~0 = muted. */
export function varianceTone(variancePercent: number): "danger" | "success" | "muted" {
  if (variancePercent > 1) return "danger";
  if (variancePercent < -1) return "success";
  return "muted";
}

/** % đạt target tháng (UC_POS_072). Target 0 → 0. */
export function targetAttainment(monthRevenue: number, monthlyTarget: number): number {
  if (monthlyTarget <= 0) return 0;
  return Math.round((monthRevenue * 10000) / monthlyTarget) / 100;
}

/**
 * Nhịp đạt target so với % thời gian tháng đã trôi (UC_POS_069).
 * ahead = vượt nhịp, behind = chậm >5 điểm %, none = chưa đặt target.
 */
export function paceStatus(
  attainmentPercent: number,
  monthElapsedPercent: number,
  hasTarget: boolean,
): "ahead" | "on-track" | "behind" | "none" {
  if (!hasTarget) return "none";
  if (attainmentPercent >= monthElapsedPercent) return "ahead";
  if (monthElapsedPercent - attainmentPercent <= 5) return "on-track";
  return "behind";
}
