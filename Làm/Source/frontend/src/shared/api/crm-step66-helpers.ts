// crm-step66-helpers.ts
// Frontend helpers cho Bước 66:
//   UC_CRM_047 — Lưu lịch sử chat (formatChatChannelBadge)
//   UC_CRM_049 — Tạo lead thủ công (validateManualLeadInput, formatLeadStatusBadge)
//   UC_CRM_050 — Tiếp nhận lead tự động (formatLeadStatusBadge)
//   UC_CRM_051 — Phân bổ lead cho sales (formatSalesOwnerSummary)

export function formatChatChannelBadge(channel?: string, direction?: string): { label: string; icon: string; isInbound: boolean } {
  const ch = (channel || '').trim().toLowerCase();
  const dir = (direction || '').trim().toLowerCase();
  const isInbound = dir === 'inbound';

  let label = 'Khách hàng';
  let icon = '💬';

  switch (ch) {
    case 'facebook':
      label = 'Facebook Messenger';
      icon = '🌐';
      break;
    case 'zalo':
      label = 'Zalo Official Account';
      icon = '📲';
      break;
    case 'webchat':
      label = 'Live Chat Website';
      icon = '💻';
      break;
    case 'whatsapp':
      label = 'WhatsApp Business';
      icon = '🟢';
      break;
  }

  const prefix = isInbound ? '📥 Đợi phản hồi' : '📤 Đã trả lời';
  return { label: `${prefix} · ${label}`, icon, isInbound };
}

export function validateManualLeadInput(input: { name: string; phone?: string; email?: string }): { isValid: boolean; error?: string } {
  const name = (input.name || '').trim();
  if (!name || name.length > 200) {
    return { isValid: false, error: 'Tên lead là bắt buộc và tối đa 200 ký tự.' };
  }

  const phone = (input.phone || '').trim();
  const email = (input.email || '').trim();

  if (!phone && !email) {
    return { isValid: false, error: 'Cần cung cấp ít nhất Số điện thoại hoặc Email để liên hệ.' };
  }

  if (phone && !/^[0-9+()\s.-]{8,20}$/.test(phone)) {
    return { isValid: false, error: 'Số điện thoại không đúng định dạng.' };
  }

  return { isValid: true };
}

export function formatLeadStatusBadge(status?: string): { label: string; styleClass: string } {
  switch ((status || '').trim()) {
    case 'New':
      return { label: '🌟 Mới tiếp nhận (New)', styleClass: 'badge-blue' };
    case 'Contacted':
      return { label: '📞 Đã liên hệ', styleClass: 'badge-yellow' };
    case 'Qualified':
      return { label: '🎯 Đủ điều kiện (Qualified)', styleClass: 'badge-green' };
    case 'Unqualified':
      return { label: '⛔ Không phù hợp', styleClass: 'badge-gray' };
    case 'Converted':
      return { label: '🏆 Đã chuyển đổi cơ hội', styleClass: 'badge-purple' };
    default:
      return { label: '📋 Đang xử lý', styleClass: 'badge-gray' };
  }
}

export function formatSalesOwnerSummary(ownerName?: string): string {
  if (!ownerName || !ownerName.trim()) {
    return '⚠️ Chưa phân bổ (Chờ gán sales)';
  }
  return `👤 Chăm sóc: ${ownerName.trim()}`;
}
