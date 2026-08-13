export function rankQuotationsByLowestPrice(
  quotations: { id: string; supplierName: string; totalAmountVnd: number; leadTimeDays: number }[]
): { id: string; supplierName: string; totalAmountVnd: number; leadTimeDays: number; rank: number; isBestValue: boolean }[] {
  if (!quotations || quotations.length === 0) return [];

  const sorted = [...quotations].sort((a, b) => a.totalAmountVnd - b.totalAmountVnd);
  return sorted.map((q, idx) => ({
    ...q,
    rank: idx + 1,
    isBestValue: idx === 0,
  }));
}

export function validatePoConfirmationStatus(status: string): { isConfirmed: boolean; requiresReview: boolean } {
  const norm = status?.trim()?.toLowerCase() || '';
  return {
    isConfirmed: norm === 'confirmed',
    requiresReview: norm === 'confirmedwithchanges' || norm === 'rejected',
  };
}
