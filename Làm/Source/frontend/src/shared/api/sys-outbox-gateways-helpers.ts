// sys-outbox-gateways-helpers.ts
// Frontend helpers cho Bước 10: UC_SYS_087 (Outbox Queue), UC_SYS_088 (Email Gateway),
// UC_SYS_089 (SMS Gateway), UC_SYS_101 (Chat Message Attachments)

export interface OutboxMessageItem {
  id: string;
  eventType: string;
  sourceModule: string;
  correlationId: string | null;
  payloadJson: string;
  status: 'Pending' | 'Published' | 'Failed' | 'Dead';
  attemptCount: number;
  nextAttemptAt: string | null;
  publishedAt: string | null;
  lastError: string | null;
  createdAt: string;
}

export interface EmailGatewayConfig {
  providerType: 'Smtp' | 'SendGrid' | 'AmazonSES';
  smtpHost: string;
  smtpPort: number;
  useSsl: boolean;
  senderEmail: string;
  senderName: string;
  apiKey?: string;
  username?: string;
}

export interface SmsGatewayConfig {
  providerType: 'Twilio' | 'VietGuys' | 'eSMS' | 'SpeedSMS';
  senderId: string;
  accountSidOrUser: string;
  apiKeyOrSecret: string;
  apiUrl?: string;
}

export interface SendChatMessageForm {
  conversationId: string;
  body: string;
  attachmentFileId?: string;
}

// ─── UC_SYS_087: Outbox Helpers ───

export function validateEnqueueOutbox(data: { eventType: string; sourceModule: string; payloadJson: string }): { valid: boolean; error?: string } {
  if (!data.eventType || data.eventType.trim().length === 0)
    return { valid: false, error: 'EventType không được để trống.' };
  if (!data.sourceModule || data.sourceModule.trim().length === 0)
    return { valid: false, error: 'SourceModule không được để trống.' };
  if (!data.payloadJson || data.payloadJson.trim().length === 0)
    return { valid: false, error: 'PayloadJson không được để trống.' };
  try {
    JSON.parse(data.payloadJson);
  } catch {
    return { valid: false, error: 'PayloadJson không phải là chuỗi JSON hợp lệ.' };
  }
  return { valid: true };
}

export function getOutboxStatusBadgeColor(status: string): string {
  const map: Record<string, string> = {
    Pending: '#f59e0b',
    Published: '#10b981',
    Failed: '#ef4444',
    Dead: '#6b7280',
  };
  return map[status] ?? '#6b7280';
}

// ─── UC_SYS_088: Email Gateway Helpers ───

export function validateEmailGatewayForm(code: string, name: string, cfg: EmailGatewayConfig): { valid: boolean; error?: string } {
  if (!code || code.trim().length === 0)
    return { valid: false, error: 'Mã kết nối (Code) không được để trống.' };
  if (!name || name.trim().length === 0)
    return { valid: false, error: 'Tên kết nối (Name) không được để trống.' };
  if (!['Smtp', 'SendGrid', 'AmazonSES'].includes(cfg.providerType))
    return { valid: false, error: 'Loại nhà cung cấp Email chỉ hỗ trợ Smtp, SendGrid, AmazonSES.' };
  if (cfg.providerType === 'Smtp') {
    if (!cfg.smtpHost || cfg.smtpHost.trim().length === 0)
      return { valid: false, error: 'SmtpHost không được để trống.' };
    if (!cfg.smtpPort || cfg.smtpPort <= 0 || cfg.smtpPort > 65535)
      return { valid: false, error: 'SmtpPort không hợp lệ.' };
  }
  if (!cfg.senderEmail || !cfg.senderEmail.includes('@'))
    return { valid: false, error: 'Email người gửi (SenderEmail) không hợp lệ.' };
  return { valid: true };
}

// ─── UC_SYS_089: SMS Gateway Helpers ───

export function validateSmsGatewayForm(code: string, name: string, cfg: SmsGatewayConfig): { valid: boolean; error?: string } {
  if (!code || code.trim().length === 0)
    return { valid: false, error: 'Mã kết nối (Code) không được để trống.' };
  if (!name || name.trim().length === 0)
    return { valid: false, error: 'Tên kết nối (Name) không được để trống.' };
  if (!['Twilio', 'VietGuys', 'eSMS', 'SpeedSMS'].includes(cfg.providerType))
    return { valid: false, error: 'Loại nhà cung cấp SMS chỉ hỗ trợ Twilio, VietGuys, eSMS, SpeedSMS.' };
  if (!cfg.senderId || cfg.senderId.trim().length === 0)
    return { valid: false, error: 'SenderId (Brandname) không được để trống.' };
  if (!cfg.apiKeyOrSecret || cfg.apiKeyOrSecret.trim().length === 0)
    return { valid: false, error: 'ApiKeyOrSecret không được để trống.' };
  return { valid: true };
}

// ─── UC_SYS_101: Chat Attachment Security & Size Helpers ───

const FORBIDDEN_EXTENSIONS = ['.exe', '.bat', '.cmd', '.sh', '.vbs', '.msi'];

export function validateChatAttachment(fileName: string, sizeBytes: number): { valid: boolean; error?: string } {
  const ext = fileName.substring(fileName.lastIndexOf('.')).toLowerCase();
  if (FORBIDDEN_EXTENSIONS.includes(ext)) {
    return { valid: false, error: `Định dạng file '${ext}' bị cấm đính kèm vì lý do bảo mật.` };
  }
  const maxBytes = 25 * 1024 * 1024; // 25MB
  if (sizeBytes > maxBytes) {
    return { valid: false, error: 'Dung lượng file đính kèm cho tin nhắn vượt quá 25MB.' };
  }
  return { valid: true };
}

export function validateSendChatMessage(form: SendChatMessageForm): { valid: boolean; error?: string } {
  if (!form.conversationId || form.conversationId.trim().length === 0) {
    return { valid: false, error: 'ConversationId không được để trống.' };
  }
  if ((!form.body || form.body.trim().length === 0) && !form.attachmentFileId) {
    return { valid: false, error: 'Tin nhắn phải có nội dung văn bản hoặc file đính kèm.' };
  }
  return { valid: true };
}
