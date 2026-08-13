export function detectAbnormalPriceSpike(
  prevPrice: number,
  currentPrice: number,
  thresholdPercent: number = 10
): { changePercent: number; isSpike: boolean } {
  if (prevPrice <= 0) return { changePercent: 0, isSpike: false };
  const diff = currentPrice - prevPrice;
  const pct = Math.round((diff / prevPrice) * 10000) / 100;
  return {
    changePercent: pct,
    isSpike: pct >= thresholdPercent,
  };
}

export function consolidateDemandsByProduct(
  demands: { productId: string; productCode: string; productName: string; qty: number }[]
): { productId: string; productCode: string; productName: string; totalQty: number }[] {
  if (!demands || demands.length === 0) return [];

  const map = new Map<string, { productId: string; productCode: string; productName: string; totalQty: number }>();

  for (const d of demands) {
    const existing = map.get(d.productId);
    if (existing) {
      existing.totalQty += d.qty || 0;
    } else {
      map.set(d.productId, {
        productId: d.productId,
        productCode: d.productCode,
        productName: d.productName,
        totalQty: d.qty || 0,
      });
    }
  }

  return Array.from(map.values());
}
