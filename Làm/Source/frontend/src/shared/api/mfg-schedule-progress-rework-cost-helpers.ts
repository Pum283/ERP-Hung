export function formatDefectRate(completed: number, defective: number): string {
  const total = completed + defective;
  if (total === 0) return '0.0%';
  const rate = (defective / total) * 100;
  return `${rate.toFixed(1)}% Lỗi`;
}

export function formatUnitCost(unitCost: number): string {
  return `${unitCost.toLocaleString('vi-VN')} đ/SP`;
}
