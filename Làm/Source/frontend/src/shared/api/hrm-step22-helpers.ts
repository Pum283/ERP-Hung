// hrm-step22-helpers.ts
// Frontend helpers cho Bước 22:
//   UC_HRM_072 — Đánh giá kết thúc thử việc (Score 0-100, Comment <= 1000)
//   UC_HRM_073 — Chuyển thử việc thành chính thức (Yêu cầu trialScore != null)
//   UC_HRM_074 — Cảnh báo hết hạn thử việc
//   UC_HRM_075 — Khai báo định biên theo đơn vị (PlannedHeadcount >= 0, ScopeType hợp lệ)

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_072 — Trial evaluation validation
// ────────────────────────────────────────────────────────────────────────────

export function validateTrialEvaluation(score: number, comment?: string): { valid: boolean; error?: string } {
  if (isNaN(score) || score < 0 || score > 100)
    return { valid: false, error: 'Điểm đánh giá thử việc phải từ 0 đến 100.' };

  const c = (comment ?? '').trim();
  if (c.length > 1000)
    return { valid: false, error: 'Nhận xét đánh giá thử việc tối đa 1000 ký tự.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_073 — Convert to official validation
// ────────────────────────────────────────────────────────────────────────────

export function validateConvertOfficial(trialScore?: number | null): { valid: boolean; error?: string } {
  if (trialScore == null)
    return { valid: false, error: 'Cần đánh giá thử việc trước khi chuyển chính thức.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_075 — Org Headcount plan validation
// ────────────────────────────────────────────────────────────────────────────

export interface HeadcountPlanForm {
  scopeType: string;
  orgUnitId: string;
  plannedHeadcount: number;
}

export function validateHeadcountPlan(form: HeadcountPlanForm): { valid: boolean; error?: string } {
  const scope = (form.scopeType ?? '').trim();
  if (!['OrgUnit', 'Department', 'Shift'].includes(scope))
    return { valid: false, error: 'Loại định biên phải là OrgUnit, Department hoặc Shift.' };

  if (!form.orgUnitId?.trim())
    return { valid: false, error: 'Vui lòng chọn Đơn vị / Phòng ban.' };

  if (isNaN(form.plannedHeadcount) || form.plannedHeadcount < 0 || form.plannedHeadcount > 100000)
    return { valid: false, error: 'Chỉ số định biên phải là số không âm (≥ 0).' };

  return { valid: true };
}
