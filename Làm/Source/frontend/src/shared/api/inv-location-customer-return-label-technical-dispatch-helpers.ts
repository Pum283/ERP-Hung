export function formatBinLocationCode(
  zone: string,
  aisle: string,
  rack: string,
  bin: string
): string {
  const z = zone?.trim() || 'ZONE-A';
  const a = aisle?.trim() || 'A1';
  const r = rack?.trim() || 'R1';
  const b = bin?.trim() || 'B1';
  return `${z}-${a}-${r}-${b}`;
}

export function getInspectionConditionLabel(condition: string): { label: string; colorClass: string } {
  switch (condition) {
    case 'GoodRestockable':
      return { label: 'Hàng Đạt Chuẩn - Nhập Lại Kho', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
    case 'NeedsRefurbish':
      return { label: 'Cần Đóng Gói / Sửa Chữa Lại', colorClass: 'bg-amber-100 text-amber-800 border-amber-300' };
    case 'DamagedScrap':
      return { label: 'Hàng Hư Hỏng - Thanh Lý / Phế Liệu', colorClass: 'bg-rose-100 text-rose-800 border-rose-300' };
    default:
      return { label: 'Chưa Kiểm Định Quality Check', colorClass: 'bg-slate-100 text-slate-800 border-slate-300' };
  }
}
