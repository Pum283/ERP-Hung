// hrm-work-level-code-doc-helpers.ts
// Frontend helpers cho Bước 12: UC_HRM_006 (Giờ làm việc theo đơn vị), UC_HRM_010 (Cấp bậc / Level),
// UC_HRM_012 (Sinh mã nhân sự tự động), UC_HRM_017 (Upload giấy tờ tùy thân)

export interface WorkCalendarForm {
  code: string;
  name: string;
  weekMask: string; // 7 chars, e.g. "1111100"
  holidaysJson?: string | null;
  isActive?: boolean;
}

export interface JobLevelForm {
  code: string;
  name: string;
  levelOrder: number;
  defaultScopeType: 'Own' | 'Department' | 'Branch' | 'All';
  description?: string | null;
  isActive?: boolean;
}

export interface EmployeeDocumentForm {
  docType: 'IdCard' | 'Passport' | 'Household' | 'Degree' | 'Other';
  title: string;
  storageKey: string;
  issuedOn?: string | null;
  expiresOn?: string | null;
}

// ─── UC_HRM_006: WorkCalendar Validation ───

export function validateWorkCalendarForm(form: WorkCalendarForm): { valid: boolean; error?: string } {
  if (!form.code || form.code.trim().length === 0)
    return { valid: false, error: 'Mã lịch làm việc (Code) không được để trống.' };
  if (!form.name || form.name.trim().length === 0)
    return { valid: false, error: 'Tên lịch làm việc (Name) không được để trống.' };
  if (!form.weekMask || form.weekMask.length !== 7 || !/^[01]{7}$/.test(form.weekMask))
    return { valid: false, error: 'WeekMask phải đúng 7 ký tự 0 và 1 (Ví dụ: 1111100 cho Thứ 2 - Thứ 6).' };
  if (form.holidaysJson && form.holidaysJson.trim().length > 0) {
    try { JSON.parse(form.holidaysJson); }
    catch { return { valid: false, error: 'HolidaysJson không phải là JSON hợp lệ.' }; }
  }
  return { valid: true };
}

export function formatWeekMaskLabel(mask: string): string {
  if (mask.length !== 7) return mask;
  const days = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
  const activeDays = days.filter((_, idx) => mask[idx] === '1');
  return activeDays.length > 0 ? activeDays.join(', ') : 'Không có ngày làm việc';
}

// ─── UC_HRM_010: JobLevel Validation ───

export function validateJobLevelForm(form: JobLevelForm): { valid: boolean; error?: string } {
  if (!form.code || form.code.trim().length === 0)
    return { valid: false, error: 'Mã cấp bậc (Code) không được để trống.' };
  if (!form.name || form.name.trim().length === 0)
    return { valid: false, error: 'Tên cấp bậc (Name) không được để trống.' };
  if (form.levelOrder === undefined || form.levelOrder < 0)
    return { valid: false, error: 'Thứ tự cấp bậc (LevelOrder) không được nhỏ hơn 0.' };
  if (!['Own', 'Department', 'Branch', 'All'].includes(form.defaultScopeType))
    return { valid: false, error: 'DefaultScopeType không hợp lệ.' };
  return { valid: true };
}

// ─── UC_HRM_012: Employee Code Generator Helper ───

export function previewGeneratedEmployeeCode(pattern: string, seqVal: number): string {
  const currentYear = new Date().getFullYear().toString();
  const shortYear = currentYear.slice(-2);
  let res = pattern.replace('{YYYY}', currentYear).replace('{YY}', shortYear);

  const seqMatch = res.match(/\{SEQ:(\d+)\}/);
  if (seqMatch) {
    const digits = parseInt(seqMatch[1], 10);
    res = res.replace(seqMatch[0], String(seqVal).padStart(digits, '0'));
  } else {
    res = res.replace('{SEQ}', String(seqVal));
  }
  return res;
}

// ─── UC_HRM_017: Employee Document Validation ───

export function validateEmployeeDocumentForm(form: EmployeeDocumentForm): { valid: boolean; error?: string } {
  if (!form.docType || !['IdCard', 'Passport', 'Household', 'Degree', 'Other'].includes(form.docType))
    return { valid: false, error: 'Loại giấy tờ (DocType) không hợp lệ.' };
  if (!form.title || form.title.trim().length === 0)
    return { valid: false, error: 'Tên giấy tờ (Title) không được để trống.' };
  if (!form.storageKey || form.storageKey.trim().length === 0)
    return { valid: false, error: 'File đính kèm (StorageKey) chưa được upload.' };
  if (form.issuedOn && form.expiresOn && new Date(form.expiresOn) <= new Date(form.issuedOn))
    return { valid: false, error: 'Ngày hết hạn (ExpiresOn) phải sau ngày cấp (IssuedOn).' };
  return { valid: true };
}
