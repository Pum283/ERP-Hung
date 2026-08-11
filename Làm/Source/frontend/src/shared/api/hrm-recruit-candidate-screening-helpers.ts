// hrm-recruit-candidate-screening-helpers.ts
// Frontend helpers cho Bước 18:
//   UC_HRM_055 — Kênh đăng tuyển
//   UC_HRM_056 — Nhập hồ sơ ứng viên (validation chặt)
//   UC_HRM_057 — Upload file CV (validate type/size)
//   UC_HRM_059 — Sơ loại ứng viên (Screen / ScreenReject)

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_055 — Kênh đăng tuyển
// ────────────────────────────────────────────────────────────────────────────

export type RecruitChannel = 'Internal' | 'Website' | 'Facebook' | 'LinkedIn' | 'Other';
export const RECRUIT_CHANNELS: RecruitChannel[] = ['Internal', 'Website', 'Facebook', 'LinkedIn', 'Other'];

export function isValidChannel(channel: string): channel is RecruitChannel {
  return (RECRUIT_CHANNELS as string[]).includes(channel);
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_056 — Nhập hồ sơ ứng viên
// ────────────────────────────────────────────────────────────────────────────

export interface CandidateForm {
  jobPostingId: string;
  fullName: string;
  email?: string;
  phone?: string;
  cvStorageKey?: string | null;
}

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/i;
/** Phone: chỉ số, +, -, (, ), space; dài 9–15 ký tự. */
const PHONE_REGEX = /^[\d+\-()\s]{9,15}$/;

export function validateCandidateForm(form: CandidateForm): { valid: boolean; error?: string } {
  if (!form.jobPostingId || form.jobPostingId.trim().length === 0)
    return { valid: false, error: 'Vui lòng chọn tin tuyển dụng.' };

  const name = (form.fullName ?? '').trim();
  if (name.length === 0)
    return { valid: false, error: 'Họ tên ứng viên không được để trống.' };
  if (name.length > 200)
    return { valid: false, error: 'Họ tên ứng viên tối đa 200 ký tự.' };

  if (form.email && form.email.trim().length > 0) {
    if (!EMAIL_REGEX.test(form.email.trim()))
      return { valid: false, error: `Địa chỉ email không hợp lệ: ${form.email.trim()}.` };
  }

  if (form.phone && form.phone.trim().length > 0) {
    if (!PHONE_REGEX.test(form.phone.trim()))
      return { valid: false, error: 'Số điện thoại không hợp lệ (9–15 ký tự số).' };
  }

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_057 — Upload file CV (validate trên FE trước khi gửi lên server)
// ────────────────────────────────────────────────────────────────────────────

export const CV_ALLOWED_TYPES = ['application/pdf', 'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document'];
export const CV_ALLOWED_EXTENSIONS = ['.pdf', '.doc', '.docx'];
export const CV_MAX_SIZE_BYTES = 10 * 1024 * 1024; // 10MB

export interface CvValidationResult {
  valid: boolean;
  error?: string;
}

export function validateCvFile(file: File): CvValidationResult {
  const ext = file.name.slice(file.name.lastIndexOf('.')).toLowerCase();
  if (!CV_ALLOWED_EXTENSIONS.includes(ext))
    return {
      valid: false,
      error: `Chỉ chấp nhận file ${CV_ALLOWED_EXTENSIONS.join(', ')}. File "${file.name}" không hợp lệ.`,
    };

  if (file.size > CV_MAX_SIZE_BYTES)
    return {
      valid: false,
      error: `File CV quá lớn (${(file.size / 1024 / 1024).toFixed(1)}MB). Tối đa 10MB.`,
    };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_059 — Sơ loại ứng viên
// ────────────────────────────────────────────────────────────────────────────

export type ScreenAction = 'Screen' | 'ScreenReject';

export interface ScreenForm {
  action: ScreenAction;
  screeningNote: string;
}

export function validateScreenForm(form: ScreenForm): { valid: boolean; error?: string } {
  if (form.action !== 'Screen' && form.action !== 'ScreenReject')
    return { valid: false, error: 'Hành động sơ loại không hợp lệ. Chấp nhận: Screen | ScreenReject.' };

  const note = (form.screeningNote ?? '').trim();
  if (note.length === 0) {
    const msg = form.action === 'Screen'
      ? 'Vui lòng nhập ghi chú sơ loại (lý do tiếp tục vòng tiếp theo).'
      : 'Vui lòng nhập lý do từ chối sơ loại.';
    return { valid: false, error: msg };
  }

  if (note.length > 500)
    return { valid: false, error: 'Ghi chú sơ loại tối đa 500 ký tự.' };

  return { valid: true };
}

/** Kiểm tra trạng thái ứng viên có cho phép sơ loại không. */
export function canScreen(pipelineStatus: string): boolean {
  return pipelineStatus === 'New' || pipelineStatus === 'Screening';
}
