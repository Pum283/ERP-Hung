export function formatPhotoTypeLabel(type: string): string {
  if (type === 'Before') return '📷 Ảnh Trước Khi Sửa';
  if (type === 'After') return '📸 Ảnh Sau Khi Nghiệm Thu';
  return '📦 Ảnh Linh Kiện Thay Thế';
}

export function formatReturnedPartQuantity(qty: number): string {
  return `${qty} Linh Kiện Hoàn Kho`;
}
