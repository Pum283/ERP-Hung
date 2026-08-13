export function calculateQuickAuditDiscrepancy(systemQty: number, actualQty: number): { diff: number; isMatch: boolean } {
  const diff = actualQty - systemQty;
  return {
    diff,
    isMatch: diff === 0,
  };
}

export function validateReplenishmentItemsCount(items: { quantityRequested: number }[]): { isValid: boolean; totalQty: number } {
  if (!items || items.length === 0) {
    return { isValid: false, totalQty: 0 };
  }
  const total = items.reduce((acc, i) => acc + (i.quantityRequested || 0), 0);
  return {
    isValid: total > 0,
    totalQty: total,
  };
}
