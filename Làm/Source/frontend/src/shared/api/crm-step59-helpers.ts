// crm-step59-helpers.ts
// Frontend helpers cho Bước 59:
//   UC_CRM_010 — Hồ sơ khách 360° (formatCustomerStatusBadge)
//   UC_CRM_011 — Danh sách người liên hệ (validateContactInput, formatPrimaryContactBadge)
//   UC_CRM_012 — Lịch sử thay đổi dữ liệu (formatAuditTrailSummary)
//   UC_CRM_013 — Ngưng sử dụng / blacklist (formatCustomerStatusBadge)

export function formatCustomerStatusBadge(status?: string): { label: string; styleClass: string; isBlacklisted: boolean } {
  switch ((status || '').trim()) {
    case 'Active':
      return { label: '🟢 Hoạt động', styleClass: 'badge-green', isBlacklisted: false };
    case 'Inactive':
      return { label: '⚪ Tạm ngưng', styleClass: 'badge-gray', isBlacklisted: false };
    case 'Merged':
      return { label: '🔀 Đã gộp', styleClass: 'badge-purple', isBlacklisted: false };
    case 'Blacklisted':
      return { label: '🚫 Danh sách đen (Blacklist)', styleClass: 'badge-red', isBlacklisted: true };
    default:
      return { label: '❓ Không rõ', styleClass: 'badge-gray', isBlacklisted: false };
  }
}

export function validateContactInput(input: { fullName: string; phone?: string; email?: string }): { isValid: boolean; errors: string[] } {
  const errors: string[] = [];

  const name = (input.fullName || '').trim();
  if (!name || name.length > 200) {
    errors.push('Họ tên người liên hệ là bắt buộc và tối đa 200 ký tự.');
  }

  if (input.email && input.email.trim()) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(input.email.trim())) {
      errors.push('Định dạng email liên hệ không hợp lệ.');
    }
  }

  if (input.phone && input.phone.trim()) {
    const normPhone = input.phone.trim().replace(/[\s\-\.\(\)]/g, '');
    if (!/^\+?\d{9,15}$/.test(normPhone)) {
      errors.push('Số điện thoại liên hệ không hợp lệ (9-15 chữ số).');
    }
  }

  return {
    isValid: errors.length === 0,
    errors,
  };
}

export function formatPrimaryContactBadge(isPrimary: boolean): string {
  return isPrimary ? '⭐ Liên hệ chính' : '👤 Người liên hệ';
}

export function formatAuditTrailSummary(handoversCount: number, contactsCount: number, status: string): string {
  return `📊 Hồ sơ 360° — Trạng thái: ${status} | Người liên hệ: ${contactsCount} | Lịch sử bàn giao & thay đổi: ${handoversCount} sự kiện`;
}
