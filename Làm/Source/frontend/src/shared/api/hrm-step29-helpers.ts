// hrm-step29-helpers.ts
// Frontend helpers cho Bước 29:
//   UC_HRM_101 — Đăng ký & đồng bộ thiết bị chấm (items required, max 500)
//   UC_HRM_102 — Cấu hình geo-fence điểm chấm (name 1-100 chars, radius 10-50,000m, lat/lng bounds)
//   UC_HRM_103 — Cấu hình quy tắc đi trễ (lateGraceMinutes 0-240m)
//   UC_HRM_104 — Cấu hình mức trừ công khi trễ (lateDeductEveryMinutes 1-480m, lateDeductWorkUnit 0-1.0)

export interface DeviceSyncItemInput {
  employeeCode: string;
  punchedAt: string;
  punchType: string;
  deviceCode?: string;
}

export function validateDeviceSyncRequest(items: DeviceSyncItemInput[]): { valid: boolean; error?: string } {
  if (!items || items.length === 0)
    return { valid: false, error: 'Danh sách bản ghi đồng bộ chấm công không được rỗng.' };

  if (items.length > 500)
    return { valid: false, error: 'Mỗi lần đồng bộ tối đa 500 bản ghi chấm công.' };

  for (let i = 0; i < items.length; i++) {
    const item = items[i];
    if (!item.employeeCode?.trim())
      return { valid: false, error: `Bản ghi số ${i + 1} thiếu mã nhân viên.` };

    if (!item.punchedAt?.trim())
      return { valid: false, error: `Bản ghi số ${i + 1} thiếu thời gian chấm.` };

    const type = (item.punchType ?? '').trim().toLowerCase();
    if (type !== 'in' && type !== 'checkin' && type !== 'out' && type !== 'checkout')
      return { valid: false, error: `Bản ghi số ${i + 1} loại chấm công không hợp lệ (${item.punchType}).` };
  }

  return { valid: true };
}

export interface GeoFenceLocationInput {
  name: string;
  latitude: number;
  longitude: number;
  radiusMeters: number;
}

export function validateGeoFenceLocation(input: GeoFenceLocationInput): { valid: boolean; error?: string } {
  const name = (input.name ?? '').trim();
  if (name.length < 1 || name.length > 100)
    return { valid: false, error: 'Tên điểm chấm công GPS phải từ 1 đến 100 ký tự.' };

  if (isNaN(input.latitude) || input.latitude < -90 || input.latitude > 90)
    return { valid: false, error: 'Vĩ độ (Latitude) phải từ -90 đến 90.' };

  if (isNaN(input.longitude) || input.longitude < -180 || input.longitude > 180)
    return { valid: false, error: 'Kinh độ (Longitude) phải từ -180 đến 180.' };

  if (isNaN(input.radiusMeters) || input.radiusMeters < 10 || input.radiusMeters > 50000)
    return { valid: false, error: 'Bán kính giới hạn phải từ 10 đến 50,000 mét.' };

  return { valid: true };
}

export function validateLateGraceRules(lateGraceMinutes: number): { valid: boolean; error?: string } {
  if (isNaN(lateGraceMinutes) || lateGraceMinutes < 0 || lateGraceMinutes > 240)
    return { valid: false, error: 'Thời gian ân hạn đi trễ phải từ 0 đến 240 phút.' };
  return { valid: true };
}

export interface LateDeductionScaleInput {
  lateDeductEveryMinutes: number;
  lateDeductWorkUnit: number;
}

export function validateLateDeductionScale(input: LateDeductionScaleInput): { valid: boolean; error?: string } {
  if (isNaN(input.lateDeductEveryMinutes) || input.lateDeductEveryMinutes < 1 || input.lateDeductEveryMinutes > 480)
    return { valid: false, error: 'Bậc trừ công trễ phải từ 1 đến 480 phút.' };

  if (isNaN(input.lateDeductWorkUnit) || input.lateDeductWorkUnit < 0 || input.lateDeductWorkUnit > 1.0)
    return { valid: false, error: 'Mức trừ công mỗi bậc phải từ 0 đến 1.0 công.' };

  return { valid: true };
}
