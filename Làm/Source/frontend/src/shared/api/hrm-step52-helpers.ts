// hrm-step52-helpers.ts
// Frontend helpers cho Bước 52:
//   UC_LMS_031 — Mua khóa / thanh toán online (calculateCourseFinalPrice)
//   UC_LMS_032 — Kích hoạt bằng mã voucher (validateVoucherCode)
//   UC_LMS_033 — Tự mở khóa sau thanh toán (formatCourseEnrollmentStatus)
//   UC_LMS_034 — Xem video / tài liệu (calculateCourseProgressPercentage)

export function validateVoucherCode(voucherCode?: string): { valid: boolean; discountPct: number; error?: string } {
  if (!voucherCode?.trim())
    return { valid: false, discountPct: 0, error: 'Mã voucher không được để trống.' };

  const code = voucherCode.trim().toUpperCase();
  if (code === 'FREE' || code === 'DEMO100') {
    return { valid: true, discountPct: 100 };
  } else if (code === 'OFF20') {
    return { valid: true, discountPct: 20 };
  } else if (code === 'OFF50') {
    return { valid: true, discountPct: 50 };
  }

  return { valid: false, discountPct: 0, error: 'Mã voucher không hợp lệ hoặc đã hết hạn.' };
}

export function calculateCourseFinalPrice(originalPrice: number, voucherCode?: string): number {
  if (isNaN(originalPrice) || originalPrice <= 0) return 0;
  const voucherRes = validateVoucherCode(voucherCode);
  if (!voucherRes.valid) return originalPrice;
  const finalPrice = originalPrice * (1 - voucherRes.discountPct / 100);
  return Math.max(0, Math.round(finalPrice));
}

export function formatCourseEnrollmentStatus(status: string): string {
  switch (status) {
    case 'Pending':
      return '⏳ Chờ thanh toán / mở khóa';
    case 'Unlocked':
      return '🔓 Đã mở khóa — Sẵn sàng học';
    case 'Completed':
      return '🎓 Đã hoàn thành khóa học';
    case 'Expired':
      return '⌛ Đã hết hạn truy cập';
    default:
      return status || 'Khác';
  }
}

export function calculateCourseProgressPercentage(completedCount: number, totalCount: number): { progressPct: number; isCompleted: boolean } {
  if (isNaN(totalCount) || totalCount <= 0) return { progressPct: 0, isCompleted: false };
  const progressPct = Math.min(100, Math.round(((completedCount || 0) / totalCount) * 10000) / 100);
  return { progressPct, isCompleted: progressPct >= 100 };
}
