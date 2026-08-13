export interface ForumTopicPreview {
  title: string;
  preview: string;
  isShort: boolean;
}

/**
 * Validate thông tin nhắc học tiếp
 */
export function validateStudyReminder(
  frequency: string,
  message: string
): { isValid: boolean; normalizedFreq: string; error?: string } {
  const validFreqs = ['Daily', 'Weekly', 'Custom'];
  const found = validFreqs.find((f) => f.toLowerCase() === frequency.trim().toLowerCase());

  if (!message || !message.trim()) {
    return { isValid: false, normalizedFreq: 'Daily', error: 'Nội dung nhắc học không được để trống.' };
  }

  return {
    isValid: true,
    normalizedFreq: found || 'Daily',
  };
}

/**
 * Format xem trước bài đăng thảo luận trên diễn đàn
 */
export function formatForumTopicPreview(title: string, content: string, maxLength: number = 80): ForumTopicPreview {
  const cleanedTitle = (title || '').trim();
  const cleanedContent = (content || '').trim();

  if (cleanedContent.length <= maxLength) {
    return { title: cleanedTitle, preview: cleanedContent, isShort: true };
  }

  return {
    title: cleanedTitle,
    preview: cleanedContent.substring(0, maxLength) + '...',
    isShort: false,
  };
}

/**
 * Kiểm tra định dạng mã xác thực chứng chỉ (CERT-XXXX-XXXX)
 */
export function parseCertificateCode(codeStr: string): { isValid: boolean; normalized: string } {
  const cleaned = (codeStr || '').trim().toUpperCase();
  const isMatch = /^CERT-[A-Z0-9-]+$/.test(cleaned);
  return {
    isValid: isMatch,
    normalized: cleaned,
  };
}

/**
 * Đánh giá nhãn & kiểu hiển thị trạng thái chứng chỉ
 */
export function evaluateCertificateStatus(status: string): {
  label: string;
  badgeColor: 'success' | 'danger' | 'warning';
  isValid: boolean;
} {
  const s = (status || '').trim().toLowerCase();

  if (s === 'active') {
    return { label: 'Có hiệu lực', badgeColor: 'success', isValid: true };
  }
  if (s === 'revoked') {
    return { label: 'Đã thu hồi', badgeColor: 'danger', isValid: false };
  }

  return { label: 'Hết hạn / Không xác định', badgeColor: 'warning', isValid: false };
}
