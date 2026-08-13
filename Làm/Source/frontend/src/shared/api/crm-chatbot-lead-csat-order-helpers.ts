export interface CsatStarResult {
  starsDisplay: string;
  badgeClass: string;
}

export function evaluateCsatStars(score: number): CsatStarResult {
  const safeScore = Math.max(1, Math.min(5, Math.round(score)));
  const stars = '⭐'.repeat(safeScore);

  if (safeScore >= 4) {
    return { starsDisplay: `${stars} (${safeScore}/5)`, badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
  }
  if (safeScore === 3) {
    return { starsDisplay: `${stars} (${safeScore}/5)`, badgeClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
  return { starsDisplay: `${stars} (${safeScore}/5)`, badgeClass: 'bg-rose-100 text-rose-800 border-rose-300' };
}

export function formatOnlineOrderCode(channel: string, code: string): string {
  if (!code || !code.trim()) return 'ORD-UNKNOWN';
  const prefix = channel ? channel.toUpperCase().replace(/\s+/g, '') : 'ONLINE';
  const cleanCode = code.trim();
  return cleanCode.startsWith('ORD-') ? cleanCode : `ORD-${prefix}-${cleanCode}`;
}

export function validateLeadCaptureForm(name: string, phone: string): { isValid: boolean; error?: string } {
  if (!name || !name.trim()) {
    return { isValid: false, error: 'Họ tên khách hàng không được để trống.' };
  }
  if (!phone || !phone.trim() || phone.trim().length < 8) {
    return { isValid: false, error: 'Số điện thoại thu thập không hợp lệ.' };
  }
  return { isValid: true };
}
