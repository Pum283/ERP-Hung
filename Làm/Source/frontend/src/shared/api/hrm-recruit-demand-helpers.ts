// hrm-recruit-demand-helpers.ts
// Frontend helpers cho Bước 16: UC_HRM_047 (Tạo phiếu đề xuất tuyển dụng), UC_HRM_048 (Chọn vị trí & số lượng cần tuyển),
// UC_HRM_049 (Nhập lý do tuyển dụng), UC_HRM_050 (Gửi phiếu đề xuất đi duyệt)

export interface RecruitmentRequestForm {
  jobTitleId: string;
  orgUnitId: string;
  headcount: number;
  reason: string;
  submit?: boolean;
}

export function validateRecruitmentRequestForm(form: RecruitmentRequestForm): { valid: boolean; error?: string } {
  if (!form.jobTitleId || form.jobTitleId.trim().length === 0)
    return { valid: false, error: 'Chức danh / vị trí tuyển dụng không được để trống.' };

  if (!form.orgUnitId || form.orgUnitId.trim().length === 0)
    return { valid: false, error: 'Đơn vị tuyển dụng không được để trống.' };

  if (!Number.isInteger(form.headcount) || form.headcount < 1 || form.headcount > 999)
    return { valid: false, error: 'Số lượng tuyển phải từ 1 đến 999.' };

  const trimmedReason = (form.reason || '').trim();
  if (trimmedReason.length === 0)
    return { valid: false, error: 'Nhập lý do tuyển dụng.' };

  if (trimmedReason.length < 5)
    return { valid: false, error: 'Lý do tuyển dụng quá ngắn (tối thiểu 5 ký tự).' };

  if (trimmedReason.length > 1000)
    return { valid: false, error: 'Lý do tuyển dụng tối đa 1000 ký tự.' };

  return { valid: true };
}

export function getRecruitmentStatusBadge(status: string): { text: string; severity: 'draft' | 'pending' | 'approved' | 'rejected' | 'closed' } {
  switch (status) {
    case 'Draft': return { text: 'Nháp', severity: 'draft' };
    case 'Pending': return { text: 'Chờ duyệt', severity: 'pending' };
    case 'Approved': return { text: 'Đã duyệt', severity: 'approved' };
    case 'Rejected': return { text: 'Từ chối', severity: 'rejected' };
    case 'Closed': return { text: 'Đã đóng', severity: 'closed' };
    case 'Cancelled': return { text: 'Đã hủy', severity: 'closed' };
    default: return { text: status, severity: 'draft' };
  }
}
