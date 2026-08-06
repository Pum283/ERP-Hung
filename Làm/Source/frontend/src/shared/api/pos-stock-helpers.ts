/** Pure helpers cho POS stock alerts — FE unit test. */
export function rankPosStockAlert(alertType: string): number {
  switch (alertType) {
    case "OutOfStock": return 0;
    case "BelowMin": return 1;
    case "NearReorder": return 2;
    default: return 9;
  }
}

export function summarizePosStockAlerts(
  alerts: { alertType: string }[],
): { outOfStock: number; belowMin: number; nearReorder: number; total: number } {
  let outOfStock = 0;
  let belowMin = 0;
  let nearReorder = 0;
  for (const a of alerts) {
    if (a.alertType === "OutOfStock") outOfStock++;
    else if (a.alertType === "BelowMin") belowMin++;
    else if (a.alertType === "NearReorder") nearReorder++;
  }
  return { outOfStock, belowMin, nearReorder, total: alerts.length };
}

/** BOM explode: material qty * sale qty, gộp theo mã. */
export function explodeBomNeed(
  saleLines: { productId: string; quantity: number }[],
  bom: { productId: string; materialCode: string; qty: number }[],
): Record<string, number> {
  const need: Record<string, number> = {};
  for (const line of saleLines) {
    for (const b of bom.filter((x) => x.productId === line.productId)) {
      const code = b.materialCode.trim().toUpperCase();
      if (!code || b.qty <= 0 || line.quantity <= 0) continue;
      need[code] = Math.round(((need[code] ?? 0) + b.qty * line.quantity) * 1000) / 1000;
    }
  }
  return need;
}
