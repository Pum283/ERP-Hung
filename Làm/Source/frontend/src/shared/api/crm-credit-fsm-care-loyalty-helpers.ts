export interface CreditCheckBadgeResult {
  label: string;
  badgeClass: string;
  isBlocked: boolean;
}

export function evaluateCreditLimitStatus(currentDebt: number, limit: number, orderValue: number): CreditCheckBadgeResult {
  const projected = (currentDebt || 0) + (orderValue || 0);
  if (projected > limit) {
    return {
      label: `CHẶN (Nợ dự kiến ${projected.toLocaleString('vi-VN')} VNĐ > Hạn mức ${limit.toLocaleString('vi-VN')} VNĐ)`,
      badgeClass: 'bg-rose-100 text-rose-800 border-rose-300',
      isBlocked: true,
    };
  }
  return {
    label: `DUYỆT (Nợ dự kiến ${projected.toLocaleString('vi-VN')} VNĐ nằm trong hạn mức)`,
    badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300',
    isBlocked: false,
  };
}

export function formatLoyaltyPointsDisplay(points: number): string {
  if (!points || points <= 0) return '0 điểm';
  return `${points.toLocaleString('vi-VN')} pts`;
}

export function validateFsmTicketHandoff(ticketId: string, fsmTechId: string): { isValid: boolean; error?: string } {
  if (!ticketId || !ticketId.trim()) {
    return { isValid: false, error: 'Mã Ticket yêu cầu không được để trống.' };
  }
  if (!fsmTechId || !fsmTechId.trim()) {
    return { isValid: false, error: 'Vui lòng chọn Kỹ thuật viên FSM tiếp nhận ticket.' };
  }
  return { isValid: true };
}
