// hrm-step55-helpers.ts
// Frontend helpers cho Bước 55:
//   UC_LMS_045 — Cấp chứng chỉ điện tử (formatCertificateVerificationUrl)
//   UC_LMS_049 — Hồ sơ giảng viên (validateLmsInstructorInput & formatInstructorStatus)
//   UC_LMS_050 — Phân quyền giảng viên (formatInstructorRoleSummary)
//   UC_LMS_051 — Theo dõi danh sách học viên (calculateLearnerCompletionRate)

export interface LmsInstructorInput {
  code: string;
  displayName: string;
  email?: string;
  phone?: string;
}

export function validateLmsInstructorInput(input: LmsInstructorInput): { valid: boolean; error?: string } {
  if (!input.code?.trim() || input.code.trim().length < 1 || input.code.trim().length > 40)
    return { valid: false, error: 'Mã giảng viên phải từ 1 đến 40 ký tự.' };

  if (!input.displayName?.trim() || input.displayName.trim().length < 1 || input.displayName.trim().length > 200)
    return { valid: false, error: 'Tên giảng viên phải từ 1 đến 200 ký tự.' };

  if (input.email?.trim() && !input.email.includes('@'))
    return { valid: false, error: 'Địa chỉ email không hợp lệ.' };

  if (input.phone?.trim() && !/^[0-9+ \-]{8,20}$/.test(input.phone.trim()))
    return { valid: false, error: 'Số điện thoại không hợp lệ.' };

  return { valid: true };
}

export function formatInstructorStatus(status: string): string {
  switch (status) {
    case 'Active':
      return '🟢 Đang giảng dạy (Active)';
    case 'Inactive':
      return '⚪ Tạm ngưng (Inactive)';
    default:
      return status || 'Khác';
  }
}

export function formatInstructorRoleSummary(displayName: string, isGranted: boolean): string {
  return isGranted
    ? `👨‍🏫 ${displayName} — Đã được cấp quyền Giảng viên hệ thống`
    : `👤 ${displayName} — Chưa phân quyền Giảng viên hệ thống`;
}

export function formatCertificateVerificationUrl(certCode: string, baseUrl = 'https://lms.erp.vn'): string {
  if (!certCode?.trim()) return '';
  return `${baseUrl.replace(/\/$/, '')}/verify-cert/${encodeURIComponent(certCode.trim().toUpperCase())}`;
}

export function calculateLearnerCompletionRate(completedCount: number, totalCount: number): { completionRatePct: number; summaryText: string } {
  if (isNaN(totalCount) || totalCount <= 0)
    return { completionRatePct: 0, summaryText: 'Chưa có học viên ghi danh' };

  const pct = Math.min(100, Math.round(((completedCount || 0) / totalCount) * 10000) / 100);
  return {
    completionRatePct: pct,
    summaryText: `🎓 Tỷ lệ hoàn thành: ${completedCount}/${totalCount} học viên (${pct}%)`,
  };
}
