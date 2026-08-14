export function formatWarrantyApprovalRate(rate: number): string {
  return `${rate.toFixed(1)}% Duyệt Bảo Hành`;
}

export function formatClaimAmount(amt: number): string {
  return `${amt.toLocaleString('vi-VN')} đ Chi Phí`;
}
