// crm-step58-helpers.ts
// Frontend helpers cho Bước 58:
//   UC_CRM_005 — Gộp khách hàng trùng (validateMergeInput)
//   UC_CRM_006 — Phân loại tệp khách hàng (formatSegmentBadge)
//   UC_CRM_008 — Gán người phụ trách (formatOwnerStatus)
//   UC_CRM_009 — Bàn giao khách hàng (formatHandoverNote)

export function formatSegmentBadge(segment?: string): { label: string; styleClass: string } {
  switch ((segment || '').trim()) {
    case 'Lead':
      return { label: '🎯 Lead tiềm năng', styleClass: 'badge-blue' };
    case 'Prospect':
      return { label: '🔍 Prospect triển vọng', styleClass: 'badge-yellow' };
    case 'Customer':
      return { label: '💎 Khách hàng', styleClass: 'badge-green' };
    case 'Partner':
      return { label: '🤝 Đối tác', styleClass: 'badge-purple' };
    default:
      return { label: '❓ Chưa phân loại', styleClass: 'badge-gray' };
  }
}

export function validateMergeInput(sourceCustomerId: string, targetCustomerId: string): { isValid: boolean; error?: string } {
  const src = (sourceCustomerId || '').trim();
  const tgt = (targetCustomerId || '').trim();

  if (!src || !tgt) {
    return { isValid: false, error: 'Vui lòng chọn cả khách hàng nguồn và khách hàng đích để gộp.' };
  }

  if (src.toLowerCase() === tgt.toLowerCase()) {
    return { isValid: false, error: 'Khách hàng nguồn và khách hàng đích không được trùng nhau.' };
  }

  return { isValid: true };
}

export function formatOwnerStatus(ownerName?: string): string {
  if (!ownerName || !ownerName.trim()) {
    return '⚠️ Chưa gán người phụ trách';
  }
  return `👤 Phụ trách: ${ownerName.trim()}`;
}

export function formatHandoverNote(fromName: string, toName: string, note?: string): string {
  const reason = note && note.trim() ? ` — Lý do: "${note.trim()}"` : '';
  return `🔄 Bàn giao từ [${fromName || 'Hệ thống'}] sang [${toName}]${reason}`;
}
