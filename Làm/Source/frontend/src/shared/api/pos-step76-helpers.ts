// pos-step76-helpers.ts
// Frontend helpers cho Bước 76:
//   UC_POS_001 — Khai báo điểm bán POS (validateStoreRequest, formatStoreBadge)
//   UC_POS_002 — Khai báo quầy / máy POS (validateTerminalRequest, formatTerminalBadge)
//   UC_POS_003 — Cấu hình máy in hóa đơn (validatePrinterRequest, formatPrinterStatus)
//   UC_POS_007 — Phân quyền thu ngân trên POS (validateCashierAssignment, formatCashierRoleBadge)

export function validateStoreRequest(
  code: string,
  name: string,
  target?: number,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã điểm bán không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên điểm bán không được để trống.' };
  }
  if (target !== undefined && target < 0) {
    return { isValid: false, error: 'Target doanh thu phải lớn hơn hoặc bằng 0.' };
  }
  return { isValid: true };
}

export function formatStoreBadge(status: string): { label: string; style: string } {
  return status === 'Active'
    ? { label: '🟢 Hoạt động', style: 'active' }
    : { label: '🔴 Tạm ngưng', style: 'inactive' };
}

export function validateTerminalRequest(
  code: string,
  name: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã quầy POS không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên quầy POS không được để trống.' };
  }
  return { isValid: true };
}

export function formatTerminalBadge(code: string, name: string, status: string): string {
  const icon = status === 'Active' ? '💻' : '🔒';
  return `${icon} [${code}] ${name}`;
}

export function validatePrinterRequest(
  code: string,
  name: string,
  type: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã máy in không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên máy in không được để trống.' };
  }
  if (type !== 'Receipt' && type !== 'Kitchen') {
    return { isValid: false, error: 'Loại máy in phải là Receipt (Bill) hoặc Kitchen (Bếp).' };
  }
  return { isValid: true };
}

export function formatPrinterStatus(printerName: string, type: string, ip?: string): string {
  const typeLabel = type === 'Kitchen' ? '🍽️ Bếp' : '🧾 Hóa đơn';
  const ipText = ip ? ` (${ip})` : '';
  return `🖨️ ${printerName} - ${typeLabel}${ipText}`;
}

export function validateCashierAssignment(
  userId?: string,
  role?: string,
): { isValid: boolean; error?: string } {
  if (!userId || userId.trim().length === 0) {
    return { isValid: false, error: 'Vui lòng chọn thu ngân.' };
  }
  if (role !== 'Cashier' && role !== 'Supervisor') {
    return { isValid: false, error: 'Vai trò phải là Cashier hoặc Supervisor.' };
  }
  return { isValid: true };
}

export function formatCashierRoleBadge(role: string): { label: string; icon: string } {
  return role === 'Supervisor'
    ? { label: 'Quản lý quầy (Supervisor)', icon: '⭐' }
    : { label: 'Thu ngân (Cashier)', icon: '👤' };
}
