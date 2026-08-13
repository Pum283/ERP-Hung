export function checkPricelistActiveValidity(
  effectiveFrom: string | Date,
  effectiveTo: string | Date
): { isActive: boolean; isExpired: boolean; isFuture: boolean } {
  const now = new Date().getTime();
  const from = new Date(effectiveFrom).getTime();
  const to = new Date(effectiveTo).getTime();

  if (now < from) {
    return { isActive: false, isExpired: false, isFuture: true };
  }
  if (now > to) {
    return { isActive: false, isExpired: true, isFuture: false };
  }
  return { isActive: true, isExpired: false, isFuture: false };
}

export function validateBatchImportSupplierRows(
  rows: { supplierCode?: string; supplierName?: string }[]
): { totalCount: number; validCount: number; invalidCount: number } {
  if (!rows || rows.length === 0) {
    return { totalCount: 0, validCount: 0, invalidCount: 0 };
  }
  const valid = rows.filter((r) => r.supplierCode?.trim() && r.supplierName?.trim()).length;
  return {
    totalCount: rows.length,
    validCount: valid,
    invalidCount: rows.length - valid,
  };
}
