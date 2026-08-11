// hrm-recruit-evaluation-decide-helpers.ts
// Frontend helpers cho Bước 19:
//   UC_HRM_060 — Chuyển đơn vị đánh giá
//   UC_HRM_061 — Form đánh giá ứng viên (EvalScore 0-100, EvalResult Pass|Fail|Hold)
//   UC_HRM_062 — Quyết định tuyển dụng (Accept / Reject kèm DecisionNote)
//   UC_HRM_063 — Strict pipeline state machine

export type EvalResultOption = 'Pass' | 'Fail' | 'Hold';
export const EVAL_RESULT_OPTIONS: EvalResultOption[] = ['Pass', 'Fail', 'Hold'];

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_060 — Chuyển đơn vị đánh giá
// ────────────────────────────────────────────────────────────────────────────

export interface AssignEvalOrgForm {
  evalOrgUnitId: string;
}

export function validateAssignEvalOrgForm(form: AssignEvalOrgForm): { valid: boolean; error?: string } {
  if (!form.evalOrgUnitId || form.evalOrgUnitId.trim().length === 0)
    return { valid: false, error: 'Vui lòng chọn đơn vị đánh giá.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_061 — Form đánh giá ứng viên chi tiết
// ────────────────────────────────────────────────────────────────────────────

export interface EvaluationForm {
  evalOrgUnitId?: string | null;
  evalScore: number;
  evalResult: string;
  evalComment?: string | null;
}

export function validateEvaluationForm(form: EvaluationForm): { valid: boolean; error?: string } {
  if (isNaN(form.evalScore) || form.evalScore < 0 || form.evalScore > 100)
    return { valid: false, error: 'Điểm đánh giá phải trong khoảng 0 đến 100.' };

  const result = (form.evalResult ?? '').trim();
  if (!result || !EVAL_RESULT_OPTIONS.includes(result as EvalResultOption))
    return { valid: false, error: 'Vui lòng chọn kết quả đánh giá (Pass, Fail hoặc Hold).' };

  const comment = (form.evalComment ?? '').trim();
  if (comment.length > 1000)
    return { valid: false, error: 'Nhận xét đánh giá tối đa 1000 ký tự.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_062 — Ra quyết định tuyển dụng (Accept / Reject)
// ────────────────────────────────────────────────────────────────────────────

export type CandidateDecisionAction = 'Accept' | 'Reject';

export interface DecisionForm {
  action: CandidateDecisionAction;
  decisionNote: string;
}

export function validateCandidateDecisionForm(form: DecisionForm): { valid: boolean; error?: string } {
  if (form.action !== 'Accept' && form.action !== 'Reject')
    return { valid: false, error: 'Hành động quyết định không hợp lệ (Accept hoặc Reject).' };

  const note = (form.decisionNote ?? '').trim();
  if (note.length === 0) {
    const msg = form.action === 'Accept'
      ? 'Vui lòng nhập ghi chú thư mời làm việc / nhận việc.'
      : 'Vui lòng nhập lý do từ chối tuyển dụng ứng viên.';
    return { valid: false, error: msg };
  }

  if (note.length > 1000)
    return { valid: false, error: 'Ghi chú quyết định tối đa 1000 ký tự.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_063 — Strict pipeline state machine
// ────────────────────────────────────────────────────────────────────────────

export function isValidPipelineTransition(currentStatus: string, nextStatus: string): boolean {
  if (currentStatus === nextStatus) return true;
  if (currentStatus === 'Accepted' || currentStatus === 'Rejected') return false;

  switch (currentStatus) {
    case 'New':
      return nextStatus === 'Screening' || nextStatus === 'Rejected';
    case 'Screening':
      return nextStatus === 'Evaluating' || nextStatus === 'Rejected';
    case 'Evaluating':
      return nextStatus === 'Accepted' || nextStatus === 'Rejected';
    default:
      return false;
  }
}
