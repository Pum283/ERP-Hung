// crm-step62-helpers.ts
// Frontend helpers cho Bước 62:
//   UC_CRM_023 — Đóng campaign (formatCampaignStatusBadge)
//   UC_CRM_024 — Danh mục nguồn lead (validateLeadSourceInput, formatLeadSourceChannelType)
//   UC_CRM_025 — Đồng bộ lead mạng xã hội (formatLandingPageUrl)
//   UC_CRM_026 — Đồng bộ lead website / landing (formatLandingPageUrl)

export function formatCampaignStatusBadge(status?: string): { label: string; styleClass: string; isClosed: boolean } {
  switch ((status || '').trim()) {
    case 'Draft':
      return { label: '📝 Bản nháp (Draft)', styleClass: 'badge-gray', isClosed: false };
    case 'Active':
      return { label: '🟢 Đang triển khai', styleClass: 'badge-green', isClosed: false };
    case 'Paused':
      return { label: '🟡 Tạm dừng', styleClass: 'badge-yellow', isClosed: false };
    case 'Closed':
      return { label: '🔒 Đã hoàn tất / Đóng', styleClass: 'badge-purple', isClosed: true };
    default:
      return { label: '❓ Chưa xác định', styleClass: 'badge-gray', isClosed: false };
  }
}

export function validateLeadSourceInput(input: { code: string; name: string; channelType: string }): { isValid: boolean; errors: string[] } {
  const errors: string[] = [];

  const code = (input.code || '').trim();
  if (!code || code.length > 40) {
    errors.push('Mã nguồn lead là bắt buộc và tối đa 40 ký tự.');
  }

  const name = (input.name || '').trim();
  if (!name || name.length > 200) {
    errors.push('Tên nguồn lead là bắt buộc và tối đa 200 ký tự.');
  }

  const validChannels = ['Manual', 'Website', 'Social', 'Other'];
  if (!validChannels.includes((input.channelType || '').trim())) {
    errors.push('Loại kênh nguồn phải thuộc: Manual, Website, Social, Other.');
  }

  return {
    isValid: errors.length === 0,
    errors,
  };
}

export function formatLeadSourceChannelType(channelType?: string): string {
  switch ((channelType || '').trim()) {
    case 'Manual':
      return '✍️ Nhập thủ công';
    case 'Website':
      return '🌐 Website / Landing Page';
    case 'Social':
      return '📲 Mạng xã hội';
    default:
      return '📡 Kênh khác';
  }
}

export function formatLandingPageUrl(landingUrl?: string): string {
  if (!landingUrl || !landingUrl.trim()) {
    return '🔗 Không có URL trang nguồn';
  }
  const cleanUrl = landingUrl.trim();
  return cleanUrl.length > 60 ? `${cleanUrl.slice(0, 57)}...` : cleanUrl;
}
