// pos-step86-helpers.ts
// Frontend helpers cho Bước 86:
//   UC_POS_068 — Xuất báo cáo POS CSV (validatePosCsvExport)
//   UC_POS_069 — Giám sát doanh thu chuỗi realtime (calculateChainTargetAttainment)
//   UC_POS_072 — Cấu hình target doanh thu (validateMonthlyTarget)
//   UC_PUR_001 — Tạo / cập nhật nhà cung cấp (validateVendorUpsert)

export function validatePosCsvExport(
  reportType: string,
  rowCount: number,
): { canExport: boolean; reason?: string } {
  if (!reportType || reportType.trim().length === 0) {
    return { canExport: false, reason: 'Chưa chọn loại báo cáo POS để xuất CSV.' };
  }
  if (rowCount <= 0) {
    return { canExport: false, reason: 'Báo cáo không có dữ liệu — không thể xuất CSV.' };
  }
  return { canExport: true };
}

export function calculateChainTargetAttainment(
  monthRevenue: number,
  monthlyTarget: number,
  elapsedPct: number = 50,
): { attainmentPct: number; isAheadOfSchedule: boolean } {
  if (monthlyTarget <= 0) {
    return { attainmentPct: 0, isAheadOfSchedule: false };
  }
  const attainmentPct = Math.round((monthRevenue / monthlyTarget) * 10000) / 100;
  const isAheadOfSchedule = attainmentPct >= elapsedPct;
  return { attainmentPct, isAheadOfSchedule };
}

export function validateMonthlyTarget(target: number): { isValid: boolean; error?: string } {
  if (isNaN(target) || target < 0) {
    return { isValid: false, error: 'Target doanh thu tháng phải là số >= 0.' };
  }
  return { isValid: true };
}

export function validateVendorUpsert(
  code: string,
  name: string,
  taxCode?: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã nhà cung cấp không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên nhà cung cấp không được để trống.' };
  }
  if (taxCode && taxCode.trim().length > 0 && !/^[0-9\-]{10,14}$/.test(taxCode.trim())) {
    return { isValid: false, error: 'Mã số thuế không đúng định dạng (10-14 chữ số).' };
  }
  return { isValid: true };
}
