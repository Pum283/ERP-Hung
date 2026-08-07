export function getMfgShiftLabel(hour: number): string {
  if (hour >= 6 && hour < 14) return "Ca 1 (06:00-14:00)";
  if (hour >= 14 && hour < 22) return "Ca 2 (14:00-22:00)";
  return "Ca 3 (22:00-06:00)";
}

export interface RawMfgReceipt {
  receivedAt: string;
  qty: number;
  workshopId?: string;
  workshopCode?: string;
  workshopName?: string;
  workOrderId: string;
}

export interface MfgShiftOutputSummary {
  day: string;
  shiftLabel: string;
  workshopId?: string;
  workshopCode?: string;
  workshopName?: string;
  qtyFg: number;
  receiptCount: number;
  workOrderCount: number;
}

export function groupReceiptsByShift(receipts: RawMfgReceipt[]): MfgShiftOutputSummary[] {
  const map = new Map<string, MfgShiftOutputSummary>();

  for (const r of receipts) {
    const dt = new Date(r.receivedAt);
    const day = dt.toISOString().slice(0, 10);
    const shiftLabel = getMfgShiftLabel(dt.getHours());
    const key = `${day}_${shiftLabel}_${r.workshopId || "none"}`;

    const existing = map.get(key);
    if (existing) {
      existing.qtyFg += r.qty;
      existing.receiptCount += 1;
    } else {
      map.set(key, {
        day,
        shiftLabel,
        workshopId: r.workshopId,
        workshopCode: r.workshopCode,
        workshopName: r.workshopName,
        qtyFg: r.qty,
        receiptCount: 1,
        workOrderCount: 1,
      });
    }
  }

  return Array.from(map.values()).sort((a, b) => b.day.localeCompare(a.day) || a.shiftLabel.localeCompare(b.shiftLabel));
}
