export interface LinkedFileItem {
  fileId: string;
  fileName: string;
  contentType: string | null;
  sizeBytes: number;
  entityType: string;
  entityId: string;
  linkedAt: string;
}

export interface ImportExportJobItem {
  id: string;
  jobType: string;      // Import | Export
  entityType: string;
  format: string | null;
  status: string;        // Pending | Running | Completed | Failed
  rowCount: number;
  errorCount: number;
  errorDetails: string | null;
  startedAt: string;
  completedAt: string | null;
  actorId: string | null;
}

export function validateLinkFileForm(data: { fileId: string; entityType: string; entityId: string }): { valid: boolean; error?: string } {
  if (!data.fileId || data.fileId.trim().length === 0) {
    return { valid: false, error: 'FileId không được để trống.' };
  }
  if (!data.entityType || data.entityType.trim().length === 0) {
    return { valid: false, error: 'Loại đối tượng (EntityType) không được để trống.' };
  }
  if (!data.entityId || data.entityId.trim().length === 0) {
    return { valid: false, error: 'ID đối tượng (EntityId) không được để trống.' };
  }
  return { valid: true };
}

export function validateExportRequest(data: { entityType: string; format: string }): { valid: boolean; error?: string } {
  if (!data.entityType || data.entityType.trim().length === 0) {
    return { valid: false, error: 'Loại đối tượng xuất (EntityType) không được để trống.' };
  }
  const validFormats = ['Csv', 'Pdf'];
  if (!validFormats.includes(data.format)) {
    return { valid: false, error: "Định dạng xuất chỉ hỗ trợ 'Csv' hoặc 'Pdf'." };
  }
  return { valid: true };
}

export function getJobStatusLabel(status: string): string {
  const map: Record<string, string> = {
    Pending: '⏳ Đang chờ',
    Running: '🔄 Đang xử lý',
    Completed: '✅ Hoàn thành',
    Failed: '❌ Thất bại',
  };
  return map[status] ?? status;
}

export function getJobStatusColor(status: string): string {
  const map: Record<string, string> = {
    Pending: '#f59e0b',
    Running: '#3b82f6',
    Completed: '#10b981',
    Failed: '#ef4444',
  };
  return map[status] ?? '#6b7280';
}
