export interface DepartmentItem {
  id: string;
  code: string;
  name: string;
  parentId: string | null;
  orgUnitId: string;
  managerUserId: string | null;
  isActive: boolean;
}

export interface JobLevelItem {
  id: string;
  code: string;
  name: string;
  levelOrder: number;
  defaultScopeType: number;
  isActive: boolean;
}

export function validateDepartmentForm(data: { code: string; name: string; orgUnitId?: string; parentId?: string | null; id?: string | null }): { valid: boolean; error?: string } {
  if (!data.code || !data.code.trim()) {
    return { valid: false, error: 'Mã phòng ban không được để trống.' };
  }
  if (!data.name || !data.name.trim()) {
    return { valid: false, error: 'Tên phòng ban không được để trống.' };
  }
  if (!data.orgUnitId) {
    return { valid: false, error: 'Chi nhánh gán vào phòng ban không được để trống.' };
  }
  if (data.id && data.parentId && data.id === data.parentId) {
    return { valid: false, error: 'Phòng ban không thể chọn chính nó làm đơn vị cấp trên.' };
  }
  return { valid: true };
}

export function validateJobLevelForm(data: { code: string; name: string; levelOrder: number }): { valid: boolean; error?: string } {
  if (!data.code || !data.code.trim()) {
    return { valid: false, error: 'Mã chức danh không được để trống.' };
  }
  if (!data.name || !data.name.trim()) {
    return { valid: false, error: 'Tên chức danh không được để trống.' };
  }
  if (data.levelOrder < 0) {
    return { valid: false, error: 'Thứ tự cấp bậc phải lớn hơn hoặc bằng 0.' };
  }
  return { valid: true };
}

export function formatLocaleDate(dateStr: string, dateFormat: string = 'dd/MM/yyyy'): string {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const year = d.getFullYear();

  if (dateFormat === 'yyyy-MM-dd') return `${year}-${month}-${day}`;
  if (dateFormat === 'MM/dd/yyyy') return `${month}/${day}/${year}`;
  return `${day}/${month}/${year}`;
}
