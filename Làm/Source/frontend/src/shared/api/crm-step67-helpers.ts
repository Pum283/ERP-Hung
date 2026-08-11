// crm-step67-helpers.ts
// Frontend helpers cho Bước 67:
//   UC_CRM_052 — Lead scoring (formatLeadScoreBadge)
//   UC_CRM_053 — Cập nhật trạng thái pipeline (formatPipelineStageStep)
//   UC_CRM_054 — Task follow-up lead (validateLeadTaskInput)
//   UC_CRM_055 — Nhắc việc follow-up (formatTaskReminderNotice)

export function formatLeadScoreBadge(score: number): { label: string; color: string; tier: string } {
  const clamped = Math.clamp ? Math.clamp(score, 0, 100) : Math.max(0, Math.min(100, score));

  if (clamped >= 80) {
    return { label: `🔥 ${clamped} điểm (Hot Lead)`, color: 'green', tier: 'Hot' };
  } else if (clamped >= 50) {
    return { label: `⚡ ${clamped} điểm (Warm Lead)`, color: 'orange', tier: 'Warm' };
  } else {
    return { label: `❄️ ${clamped} điểm (Cold Lead)`, color: 'blue', tier: 'Cold' };
  }
}

export function formatPipelineStageStep(stage?: string): { name: string; stepNumber: number; totalSteps: number } {
  const st = (stage || '').trim();
  switch (st) {
    case 'New':
      return { name: '🌟 1. Tiếp nhận ban đầu', stepNumber: 1, totalSteps: 5 };
    case 'Contacted':
      return { name: '📞 2. Đã liên hệ tư vấn', stepNumber: 2, totalSteps: 5 };
    case 'Qualified':
      return { name: '🎯 3. Đánh giá khả thi', stepNumber: 3, totalSteps: 5 };
    case 'Converted':
      return { name: '🏆 4. Chuyển đổi cơ hội', stepNumber: 4, totalSteps: 5 };
    case 'Lost':
      return { name: '❌ 5. Thất bại (Lost)', stepNumber: 5, totalSteps: 5 };
    default:
      return { name: '📋 1. Tiếp nhận ban đầu', stepNumber: 1, totalSteps: 5 };
  }
}

export function validateLeadTaskInput(input: { title: string; dueAt?: string | Date }): { isValid: boolean; error?: string } {
  const title = (input.title || '').trim();
  if (!title || title.length > 200) {
    return { isValid: false, error: 'Tiêu đề công việc là bắt buộc và tối đa 200 ký tự.' };
  }

  if (!input.dueAt) {
    return { isValid: false, error: 'Hạn hoàn thành công việc là bắt buộc.' };
  }

  const dueDate = new Date(input.dueAt);
  if (isNaN(dueDate.getTime())) {
    return { isValid: false, error: 'Thời hạn hoàn thành không đúng định dạng ngày tháng.' };
  }

  return { isValid: true };
}

export function formatTaskReminderNotice(isReminder: boolean, dueAt: string | Date, status: string): string {
  if (status === 'Completed') {
    return '✅ Công việc đã hoàn thành';
  }

  const dueDate = new Date(dueAt);
  const now = new Date();
  const diffHours = (dueDate.getTime() - now.getTime()) / (1000 * 60 * 60);

  if (diffHours < 0) {
    return '🚨 CẢNH BÁO: Công việc đã QUÁ HẠN follow-up!';
  }

  if (isReminder) {
    return `⏰ Bật nhắc nhở (Còn ${Math.ceil(diffHours)} giờ đến hạn)`;
  }

  return `📅 Hạn hoàn thành: ${dueDate.toLocaleDateString('vi-VN')}`;
}
