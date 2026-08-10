export interface FileObjectItem {
  id: string;
  storageKey: string;
  fileName: string;
  contentType: string | null;
  sizeBytes: number;
  folderId: string | null;
  isDeleted: boolean;
}

export interface NotificationRuleItem {
  id: string;
  eventType: string;
  titleTemplate: string;
  bodyTemplate: string;
  isEnabled: boolean;
}

export function validateFileUploadForm(data: { fileName: string; sizeBytes: number; maxSizeBytes?: number }): { valid: boolean; error?: string } {
  if (!data.fileName || !data.fileName.trim()) {
    return { valid: false, error: 'Tên file không được để trống.' };
  }
  if (data.sizeBytes <= 0) {
    return { valid: false, error: 'Kích thước file phải lớn hơn 0 byte.' };
  }
  const maxLimit = data.maxSizeBytes || 50 * 1024 * 1024; // 50MB
  if (data.sizeBytes > maxLimit) {
    return { valid: false, error: `Kích thước file không được vượt quá ${Math.round(maxLimit / (1024 * 1024))}MB.` };
  }
  return { valid: true };
}

export function validateNotificationRuleForm(data: { eventType: string; titleTemplate: string; bodyTemplate: string }): { valid: boolean; error?: string } {
  if (!data.eventType || !data.eventType.trim()) {
    return { valid: false, error: 'Mã sự kiện không được để trống.' };
  }
  if (!data.titleTemplate || !data.titleTemplate.trim()) {
    return { valid: false, error: 'Tiêu đề thông báo mẫu không được để trống.' };
  }
  if (!data.bodyTemplate || !data.bodyTemplate.trim()) {
    return { valid: false, error: 'Nội dung thông báo mẫu không được để trống.' };
  }
  return { valid: true };
}

export function formatFileSize(bytes: number): string {
  if (bytes <= 0) return '0 B';
  const units = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(1024));
  const formatted = (bytes / Math.pow(1024, i)).toFixed(2);
  return `${formatted} ${units[i]}`;
}
