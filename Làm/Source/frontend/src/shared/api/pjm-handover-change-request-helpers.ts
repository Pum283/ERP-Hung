export function formatEcrImpactSummary(cost: number, days: number): string {
  return `+${cost.toLocaleString('vi-VN')} đ (Gia hạn: +${days} ngày)`;
}

export function formatAttachmentSize(bytes: number): string {
  const mb = bytes / (1024 * 1024);
  return `${mb.toFixed(2)} MB`;
}
