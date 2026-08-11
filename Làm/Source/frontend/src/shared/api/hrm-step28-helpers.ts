// hrm-step28-helpers.ts
// Frontend helpers cho Bước 28:
//   UC_HRM_097 — Báo cáo giờ / chi phí điều động (actualHours >= 0 & <= 1000)
//   UC_HRM_098 — Cấu hình chấm vân tay / sinh trắc (code 1-40 chars, name 1-100 chars)
//   UC_HRM_099 — Cấu hình chấm theo GPS / Wi-Fi (radius 10-50,000m, latitude/longitude bounds)
//   UC_HRM_100 — Cấu hình chấm bằng khuôn mặt & chính sách chấm công (confidence 0.50-0.99, graceMinutes 0-240)

export function validateActualHours(hours: number): { valid: boolean; error?: string } {
  if (isNaN(hours) || hours < 0 || hours > 1000)
    return { valid: false, error: 'Giờ thực tế phải từ 0 đến 1,000 giờ.' };
  return { valid: true };
}

export interface BiometricDeviceForm {
  code: string;
  name: string;
  deviceType?: string;
}

export function validateBiometricDevice(form: BiometricDeviceForm): { valid: boolean; error?: string } {
  const code = (form.code ?? '').trim();
  if (code.length < 1 || code.length > 40)
    return { valid: false, error: 'Mã thiết bị từ 1 đến 40 ký tự.' };

  const name = (form.name ?? '').trim();
  if (name.length < 1 || name.length > 100)
    return { valid: false, error: 'Tên thiết bị từ 1 đến 100 ký tự.' };

  const validTypes = new Set(['Fingerprint', 'Face', 'Biometric', 'Card', 'Qr']);
  const type = (form.deviceType ?? 'Fingerprint').trim();
  if (!validTypes.has(type))
    return { valid: false, error: 'Loại thiết bị chấm công không hợp lệ.' };

  return { valid: true };
}

export interface GeoFenceForm {
  name: string;
  latitude: number;
  longitude: number;
  radiusMeters: number;
}

export function validateGeoFence(form: GeoFenceForm): { valid: boolean; error?: string } {
  const name = (form.name ?? '').trim();
  if (name.length < 1 || name.length > 100)
    return { valid: false, error: 'Tên điểm chấm GPS từ 1 đến 100 ký tự.' };

  if (isNaN(form.latitude) || form.latitude < -90 || form.latitude > 90)
    return { valid: false, error: 'Vĩ độ (Latitude) phải từ -90 đến 90.' };

  if (isNaN(form.longitude) || form.longitude < -180 || form.longitude > 180)
    return { valid: false, error: 'Kinh độ (Longitude) phải từ -180 đến 180.' };

  if (isNaN(form.radiusMeters) || form.radiusMeters < 10 || form.radiusMeters > 50000)
    return { valid: false, error: 'Bán kính giới hạn từ 10 đến 50,000 mét.' };

  return { valid: true };
}

export interface AttendancePolicyForm {
  lateGraceMinutes: number;
  lateDeductEveryMinutes: number;
  lateDeductWorkUnit: number;
  minConfidenceScore?: number;
}

export function validateFaceRecognitionConfig(form: AttendancePolicyForm): { valid: boolean; error?: string } {
  if (isNaN(form.lateGraceMinutes) || form.lateGraceMinutes < 0 || form.lateGraceMinutes > 240)
    return { valid: false, error: 'Thời gian ân hạn trễ từ 0 đến 240 phút.' };

  if (isNaN(form.lateDeductEveryMinutes) || form.lateDeductEveryMinutes < 1 || form.lateDeductEveryMinutes > 480)
    return { valid: false, error: 'Bậc trừ công trễ từ 1 đến 480 phút.' };

  if (isNaN(form.lateDeductWorkUnit) || form.lateDeductWorkUnit < 0 || form.lateDeductWorkUnit > 1)
    return { valid: false, error: 'Mức trừ công trễ từ 0 đến 1.0 công.' };

  if (form.minConfidenceScore !== undefined) {
    if (isNaN(form.minConfidenceScore) || form.minConfidenceScore < 0.5 || form.minConfidenceScore > 0.99)
      return { valid: false, error: 'Ngưỡng tin cậy nhận diện khuôn mặt từ 0.50 đến 0.99.' };
  }

  return { valid: true };
}
