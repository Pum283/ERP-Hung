// hrm-recruit-step20-helpers.ts
// Frontend helpers cho Bước 20:
//   UC_HRM_064 — Lịch sử chăm sóc ứng viên
//   UC_HRM_065 — Báo cáo hiệu quả kênh tuyển
//   UC_HRM_066 — Cấu hình thời hạn onboarding (1-365 ngày)
//   UC_HRM_067 — Cấu hình thời hạn thử việc (1-365 ngày)

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_064 — Care note validation
// ────────────────────────────────────────────────────────────────────────────

export function validateCareNote(note: string): { valid: boolean; error?: string } {
  const trimmed = (note ?? '').trim();
  if (trimmed.length === 0)
    return { valid: false, error: 'Ghi chú chăm sóc không được để trống.' };

  if (trimmed.length > 1000)
    return { valid: false, error: 'Ghi chú chăm sóc tối đa 1000 ký tự.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_065 — Channel conversion rate calculation
// ────────────────────────────────────────────────────────────────────────────

export function calculateConversionRate(acceptedCount: number, candidateCount: number): number {
  if (candidateCount <= 0 || acceptedCount <= 0) return 0;
  const pct = (acceptedCount / candidateCount) * 100;
  return Math.round(pct * 100) / 100;
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_066 + UC_HRM_067 — Onboarding & Trial Duration Settings
// ────────────────────────────────────────────────────────────────────────────

export interface OnboardingSettingsForm {
  onboardingDays: number;
  trialDays: number;
}

export function validateOnboardingSettingsForm(form: OnboardingSettingsForm): { valid: boolean; error?: string } {
  if (isNaN(form.onboardingDays) || form.onboardingDays < 1 || form.onboardingDays > 365)
    return { valid: false, error: 'Thời hạn onboarding phải từ 1 đến 365 ngày.' };

  if (isNaN(form.trialDays) || form.trialDays < 1 || form.trialDays > 365)
    return { valid: false, error: 'Thời hạn thử việc phải từ 1 đến 365 ngày.' };

  return { valid: true };
}
