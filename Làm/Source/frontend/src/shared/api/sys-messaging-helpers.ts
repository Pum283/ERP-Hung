export interface SystemSettingItem {
  key: string;
  valueJson: string;
}

export interface AppNotificationItem {
  id: string;
  title: string;
  body: string;
  link?: string | null;
  eventType: string;
  isRead: boolean;
  createdAt: string;
}

export interface MessageTemplateItem {
  id: string;
  code: string;
  channel: 'Email' | 'Sms';
  subject: string;
  body: string;
  isActive: boolean;
}

export function validateMessageTemplateForm(data: { code: string; channel: string; subject?: string; body: string }): { valid: boolean; error?: string } {
  if (!data.code || !data.code.trim()) {
    return { valid: false, error: 'Mã template không được để trống.' };
  }
  if (!data.body || !data.body.trim()) {
    return { valid: false, error: 'Nội dung template không được để trống.' };
  }
  if (data.channel === 'Email' && (!data.subject || !data.subject.trim())) {
    return { valid: false, error: 'Tiêu đề email không được để trống.' };
  }
  return { valid: true };
}

export function validateChannelSendForm(data: { channel: string; target: string; templateCode: string }): { valid: boolean; error?: string } {
  if (!data.channel || (data.channel !== 'Email' && data.channel !== 'Sms')) {
    return { valid: false, error: "Kênh gửi không hợp lệ. Hệ thống chỉ hỗ trợ 'Email' hoặc 'Sms'." };
  }
  if (!data.target || !data.target.trim()) {
    return { valid: false, error: 'Người nhận (Target) không được để trống.' };
  }
  if (data.channel === 'Email') {
    const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    if (!emailRegex.test(data.target.trim())) {
      return { valid: false, error: 'Địa chỉ Email người nhận không hợp lệ.' };
    }
  } else if (data.channel === 'Sms') {
    const cleaned = data.target.replace(/[^\d+]/g, '');
    if (cleaned.length < 8 || cleaned.length > 15) {
      return { valid: false, error: 'Số điện thoại người nhận không hợp lệ.' };
    }
  }
  if (!data.templateCode || !data.templateCode.trim()) {
    return { valid: false, error: 'Mã template không được để trống.' };
  }
  return { valid: true };
}

export function renderTemplateText(templateText: string, vars?: Record<string, string>): string {
  if (!templateText) return '';
  if (!vars) return templateText;
  let result = templateText;
  for (const [key, value] of Object.entries(vars)) {
    result = result.replace(new RegExp(`\\{${key}\\}`, 'gi'), value);
  }
  return result;
}
