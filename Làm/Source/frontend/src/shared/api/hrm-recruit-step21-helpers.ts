// hrm-recruit-step21-helpers.ts
// Frontend helpers cho Bước 21:
//   UC_HRM_068 — Tạo hồ sơ nhân viên mới từ ứng viên trúng tuyển
//   UC_HRM_069 — Gán người hướng dẫn (Onboarding Mentor)
//   UC_HRM_070 — Checklist tiếp nhận & tiến độ %
//   UC_HRM_071 — Upload chứng chỉ / giấy tờ onboarding

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_068 — Hire validation
// ────────────────────────────────────────────────────────────────────────────

export function validateHireRequest(body: { candidateId: string; orgUnitId?: string | null }): { valid: boolean; error?: string } {
  if (!body.candidateId?.trim())
    return { valid: false, error: 'Vui lòng chọn ứng viên trúng tuyển.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_069 — Mentor assignment validation
// ────────────────────────────────────────────────────────────────────────────

export function validateMentorAssignment(mentorId: string, employeeId: string): { valid: boolean; error?: string } {
  const m = (mentorId ?? '').trim();
  const e = (employeeId ?? '').trim();
  if (!m)
    return { valid: false, error: 'Vui lòng chọn người hướng dẫn (Mentor).' };

  if (m === e)
    return { valid: false, error: 'Không thể gán chính nhân viên mới làm người hướng dẫn.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_070 — Checklist progress calculation
// ────────────────────────────────────────────────────────────────────────────

export function calculateChecklistProgress(items: Array<{ done: boolean }>): number {
  if (!items || items.length === 0) return 0;
  const doneCount = items.filter((i) => i.done).length;
  const pct = (doneCount / items.length) * 100;
  return Math.round(pct);
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_071 — Document upload validation
// ────────────────────────────────────────────────────────────────────────────

export function validateOnboardingDocument(title: string, storageKey: string): { valid: boolean; error?: string } {
  const t = (title ?? '').trim();
  const k = (storageKey ?? '').trim();

  if (!t)
    return { valid: false, error: 'Tiêu đề chứng chỉ / giấy tờ không được để trống.' };

  if (t.length > 200)
    return { valid: false, error: 'Tiêu đề chứng chỉ / giấy tờ tối đa 200 ký tự.' };

  if (!k)
    return { valid: false, error: 'File chứng chỉ / giấy tờ chưa được tải lên.' };

  return { valid: true };
}
