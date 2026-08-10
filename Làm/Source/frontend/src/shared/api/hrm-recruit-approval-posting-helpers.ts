// hrm-recruit-approval-posting-helpers.ts
// Frontend helpers cho Bước 17: UC_HRM_051 (Duyệt / từ chối đề xuất), UC_HRM_052 (Xem lịch sử duyệt đề xuất),
// UC_HRM_053 (Đóng / hủy phiếu đề xuất), UC_HRM_054 (Tạo tin tuyển từ phiếu đã duyệt)

export interface ApproveForm {
  action: 'Approve' | 'Reject';
  comment?: string;
}

export function validateApprovalForm(form: ApproveForm): { valid: boolean; error?: string } {
  if (form.action !== 'Approve' && form.action !== 'Reject')
    return { valid: false, error: 'Hành động duyệt không hợp lệ.' };

  if (form.action === 'Reject' && (!form.comment || form.comment.trim().length === 0))
    return { valid: false, error: 'Vui lòng nhập lý do khi từ chối phiếu đề xuất.' };

  return { valid: true };
}

export interface JobPostingForm {
  recruitmentRequestId: string;
  title: string;
  channel: 'Internal' | 'Website' | 'Facebook' | 'LinkedIn' | 'Other';
}

export function validateJobPostingForm(form: JobPostingForm): { valid: boolean; error?: string } {
  if (!form.recruitmentRequestId || form.recruitmentRequestId.trim().length === 0)
    return { valid: false, error: 'Phiếu đề xuất tuyển dụng không được để trống.' };

  if (!form.title || form.title.trim().length === 0)
    return { valid: false, error: 'Tiêu đề tin tuyển dụng không được để trống.' };

  if (form.title.trim().length < 5)
    return { valid: false, error: 'Tiêu đề tin tuyển dụng quá ngắn (tối thiểu 5 ký tự).' };

  return { valid: true };
}
