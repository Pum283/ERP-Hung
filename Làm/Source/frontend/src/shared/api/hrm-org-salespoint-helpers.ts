// hrm-org-salespoint-helpers.ts
// Frontend helpers cho Bước 11: UC_HRM_001 (Sơ đồ tổ chức), UC_HRM_002 (Khối vận hành),
// UC_HRM_003 (Khối sản xuất), UC_HRM_004 (Danh mục điểm bán)

export interface OrgUnitForm {
  code: string;
  name: string;
  parentId?: string | null;
  unitType: 'Company' | 'Branch' | 'OperationsBlock' | 'ProductionBlock' | 'Department' | 'Team';
  managerUserId?: string | null;
  sortOrder?: number;
  isActive?: boolean;
}

export interface SalesPointForm {
  code: string;
  name: string;
  orgUnitId?: string | null;
  address?: string | null;
  isActive?: boolean;
}

// ─── UC_HRM_001 / 002 / 003: OrgUnit Validation ───

export function validateOrgUnitForm(form: OrgUnitForm): { valid: boolean; error?: string } {
  if (!form.code || form.code.trim().length === 0)
    return { valid: false, error: 'Mã đơn vị tổ chức (Code) không được để trống.' };
  if (!form.name || form.name.trim().length === 0)
    return { valid: false, error: 'Tên đơn vị tổ chức (Name) không được để trống.' };
  if (!form.unitType || form.unitType.trim().length === 0)
    return { valid: false, error: 'Loại đơn vị tổ chức (UnitType) không được để trống.' };

  const validTypes = ['Company', 'Branch', 'OperationsBlock', 'ProductionBlock', 'Department', 'Team'];
  if (!validTypes.includes(form.unitType))
    return { valid: false, error: 'Loại đơn vị tổ chức không hợp lệ.' };

  return { valid: true };
}

export function getUnitTypeLabel(unitType: string): string {
  const map: Record<string, string> = {
    Company: '🏢 Tập đoàn / Công ty',
    Branch: '🌿 Chi nhánh',
    OperationsBlock: '⚙️ Khối Vận Hành',
    ProductionBlock: '🏗️ Khối Sản Xuất',
    Department: '📂 Phòng ban',
    Team: '👥 Tổ / Nhóm',
  };
  return map[unitType] ?? unitType;
}

export function getUnitTypeBadgeColor(unitType: string): string {
  const map: Record<string, string> = {
    Company: '#4f46e5',
    Branch: '#10b981',
    OperationsBlock: '#f59e0b',
    ProductionBlock: '#ec4899',
    Department: '#3b82f6',
    Team: '#8b5cf6',
  };
  return map[unitType] ?? '#6b7280';
}

// ─── UC_HRM_004: SalesPoint Validation ───

export function validateSalesPointForm(form: SalesPointForm): { valid: boolean; error?: string } {
  if (!form.code || form.code.trim().length === 0)
    return { valid: false, error: 'Mã điểm bán (Code) không được để trống.' };
  if (!form.name || form.name.trim().length === 0)
    return { valid: false, error: 'Tên điểm bán (Name) không được để trống.' };
  return { valid: true };
}
