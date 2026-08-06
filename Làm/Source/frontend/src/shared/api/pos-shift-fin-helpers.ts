/** Pure helpers — đóng ca → FIN (UC_POS_059). */

export type FinSyncParsed = {
  synced: number;
  already: number;
  paid: number;
  failed: number;
};

/** Parse tag dạng `FIN:2+1/3 fail=0` từ note ca. */
export function parseFinSyncFromNote(note: string | null | undefined): FinSyncParsed | null {
  if (!note) return null;
  const m = note.match(/FIN:(\d+)\+(\d+)\/(\d+)\s+fail=(\d+)/);
  if (!m) return null;
  return {
    synced: Number(m[1]),
    already: Number(m[2]),
    paid: Number(m[3]),
    failed: Number(m[4]),
  };
}

export function formatFinSyncFlash(r: {
  syncedCount: number;
  alreadyHadCount: number;
  paidSaleCount: number;
  failedCount: number;
}): string {
  return `FIN: ${r.syncedCount} mới · ${r.alreadyHadCount} đã có · ${r.failedCount} lỗi / ${r.paidSaleCount} Paid`;
}

export function isFinSyncComplete(r: {
  paidSaleCount: number;
  syncedCount: number;
  alreadyHadCount: number;
  failedCount: number;
}): boolean {
  return r.failedCount === 0 && r.syncedCount + r.alreadyHadCount === r.paidSaleCount;
}

export function finSyncCoveragePct(r: {
  paidSaleCount: number;
  syncedCount: number;
  alreadyHadCount: number;
}): number {
  if (r.paidSaleCount <= 0) return 100;
  return Math.round(((r.syncedCount + r.alreadyHadCount) / r.paidSaleCount) * 100);
}
