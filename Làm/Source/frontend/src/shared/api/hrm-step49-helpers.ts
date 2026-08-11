// hrm-step49-helpers.ts
// Frontend helpers cho Bước 49:
//   UC_LMS_006 — Upload tài liệu PDF / slide (validateLmsDocumentLessonInput)
//   UC_LMS_009 — Ẩn / xuất bản khóa học (formatLmsCoursePublishStatus)
//   UC_LMS_014 — Cấu hình điểm đạt / số lần thi (validateLmsExamPassConfig)
//   UC_LMS_016 — Mở lớp đào tạo offline (validateLmsOfflineClassInput)

export function validateLmsDocumentLessonInput(title: string, contentUrl?: string): { valid: boolean; error?: string } {
  if (!title?.trim() || title.trim().length > 200)
    return { valid: false, error: 'Tiêu đề bài học tài liệu phải từ 1 đến 200 ký tự.' };

  if (!contentUrl?.trim())
    return { valid: false, error: 'Bài học tài liệu bắt buộc phải điền URL tài liệu PDF/Slide.' };

  return { valid: true };
}

export function formatLmsCoursePublishStatus(status: string): string {
  switch (status) {
    case 'Draft':
      return '📝 Bản nháp (Draft)';
    case 'Published':
      return '🚀 Đã xuất bản (Published)';
    case 'Hidden':
      return '🔒 Đã ẩn (Hidden)';
    default:
      return status || 'Khác';
  }
}

export function validateLmsExamPassConfig(passScore: number, maxAttempts: number): { valid: boolean; error?: string } {
  if (isNaN(passScore) || passScore < 0 || passScore > 100)
    return { valid: false, error: 'Điểm đạt thi phải nằm trong khoảng từ 0 đến 100.' };

  if (isNaN(maxAttempts) || maxAttempts < 1)
    return { valid: false, error: 'Số lần thi tối đa phải lớn hơn hoặc bằng 1.' };

  return { valid: true };
}

export interface OfflineClassInput {
  code: string;
  name: string;
  courseTitle: string;
  startDate: string;
  endDate: string;
  capacity?: number;
}

export function validateLmsOfflineClassInput(c: OfflineClassInput): { valid: boolean; error?: string } {
  if (!c.code?.trim() || c.code.trim().length > 40)
    return { valid: false, error: 'Mã lớp đào tạo phải từ 1 đến 40 ký tự.' };

  if (!c.name?.trim() || c.name.trim().length > 200)
    return { valid: false, error: 'Tên lớp đào tạo phải từ 1 đến 200 ký tự.' };

  if (!c.courseTitle?.trim() || c.courseTitle.trim().length > 200)
    return { valid: false, error: 'Tên khóa học liên kết phải từ 1 đến 200 ký tự.' };

  if (c.startDate && c.endDate && new Date(c.endDate) < new Date(c.startDate))
    return { valid: false, error: 'Ngày kết thúc lớp học không được trước ngày bắt đầu.' };

  if (c.capacity !== undefined && (isNaN(c.capacity) || c.capacity <= 0))
    return { valid: false, error: 'Sức chứa học viên của lớp phải lớn hơn 0.' };

  return { valid: true };
}
