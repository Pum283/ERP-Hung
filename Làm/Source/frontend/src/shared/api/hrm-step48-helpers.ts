// hrm-step48-helpers.ts
// Frontend helpers cho Bước 48:
//   UC_LMS_002 — Danh mục khóa học (validateLmsCourseInput)
//   UC_LMS_003 — Phân loại khóa online/offline/blended (formatLmsDeliveryMode)
//   UC_LMS_004 — Quản lý chương / bài học (validateLmsChapterInput)
//   UC_LMS_005 — Upload video bài giảng (validateLmsLessonVideoInput)

export interface LmsCourseInput {
  code: string;
  name: string;
  deliveryMode: string;
  price: number;
}

export function validateLmsCourseInput(c: LmsCourseInput): { valid: boolean; error?: string } {
  if (!c.code?.trim() || c.code.trim().length > 40)
    return { valid: false, error: 'Mã khóa học phải từ 1 đến 40 ký tự.' };

  if (!c.name?.trim() || c.name.trim().length > 200)
    return { valid: false, error: 'Tên khóa học phải từ 1 đến 200 ký tự.' };

  const validModes = ['Online', 'Offline', 'Blended'];
  if (!validModes.includes(c.deliveryMode))
    return { valid: false, error: 'Hình thức đào tạo phải là Online, Offline hoặc Blended.' };

  if (isNaN(c.price) || c.price < 0)
    return { valid: false, error: 'Giá khóa học không được là số âm.' };

  return { valid: true };
}

export function formatLmsDeliveryMode(mode: string): string {
  switch (mode) {
    case 'Online':
      return '💻 Trực tuyến (Online E-Learning)';
    case 'Offline':
      return '🏫 Tập trung (Offline Class)';
    case 'Blended':
      return '🔀 Kết hợp (Blended Learning)';
    default:
      return mode || 'Khác';
  }
}

export function validateLmsChapterInput(title: string, sortOrder: number): { valid: boolean; error?: string } {
  if (!title?.trim() || title.trim().length > 200)
    return { valid: false, error: 'Tiêu đề chương học phải từ 1 đến 200 ký tự.' };

  if (isNaN(sortOrder) || sortOrder < 1)
    return { valid: false, error: 'Thứ tự hiển thị chương phải lớn hơn hoặc bằng 1.' };

  return { valid: true };
}

export interface LmsLessonInput {
  title: string;
  lessonType: string;
  videoUrl?: string;
  durationMinutes?: number;
}

export function validateLmsLessonVideoInput(l: LmsLessonInput): { valid: boolean; error?: string } {
  if (!l.title?.trim() || l.title.trim().length > 200)
    return { valid: false, error: 'Tiêu đề bài học phải từ 1 đến 200 ký tự.' };

  const validTypes = ['Video', 'Document', 'Text'];
  if (!validTypes.includes(l.lessonType))
    return { valid: false, error: 'Loại bài học phải là Video, Document hoặc Text.' };

  if (l.lessonType === 'Video') {
    if (!l.videoUrl?.trim())
      return { valid: false, error: 'Bài học loại Video bắt buộc phải điền đường dẫn URL video.' };

    if (l.durationMinutes !== undefined && (isNaN(l.durationMinutes) || l.durationMinutes <= 0))
      return { valid: false, error: 'Thời lượng video phải lớn hơn 0 phút.' };
  }

  return { valid: true };
}
