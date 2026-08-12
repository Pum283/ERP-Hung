export interface ManagerEvaluationItem {
  id: string;
  employeeId: string;
  kpiScore: number;
  competencyScore: number;
  finalGrade: 'A' | 'B' | 'C' | 'D' | string;
  status: 'Pending' | 'Completed' | string;
}

export interface KpiTemplateItem {
  id: string;
  code: string;
  title: string;
  maxScore: number;
  weightPercentage: number;
}

/**
 * Tính điểm trung bình & xếp loại KPI nhân sự
 */
export function calculateFinalKpiGrade(
  kpiScore: number,
  competencyScore: number
): { finalScore: number; grade: 'A' | 'B' | 'C' | 'D' } {
  const finalScore = Number(((kpiScore + competencyScore) / 2).toFixed(1));
  let grade: 'A' | 'B' | 'C' | 'D' = 'D';

  if (finalScore >= 85) grade = 'A';
  else if (finalScore >= 70) grade = 'B';
  else if (finalScore >= 50) grade = 'C';

  return { finalScore, grade };
}

/**
 * Validate trọng số & điểm tối đa mẫu KPI
 */
export function validateTemplateWeights(
  maxScore: number,
  weightPercentage: number
): { isValid: boolean; error?: string } {
  if (maxScore <= 0) {
    return { isValid: false, error: 'Điểm tối đa phải lớn hơn 0.' };
  }
  if (weightPercentage <= 0 || weightPercentage > 100) {
    return { isValid: false, error: 'Tỷ trọng % phải nằm trong khoảng (0, 100].' };
  }
  return { isValid: true };
}

/**
 * Validate mức tự đánh giá (1-5 sao)
 */
export function validateSelfEvaluationScore(rating: number): { isValid: boolean; clampedRating: number } {
  const clamped = Math.max(1, Math.min(5, Math.round(rating)));
  return {
    isValid: rating >= 1 && rating <= 5,
    clampedRating: clamped,
  };
}

/**
 * Thống kê tỷ lệ hoàn thành kỳ đánh giá
 */
export function calculateCycleCompletionStats(evaluations: ManagerEvaluationItem[]): {
  total: number;
  completed: number;
  pending: number;
  completionRate: number;
} {
  const total = evaluations.length;
  const completed = evaluations.filter((e) => e.status === 'Completed').length;
  const pending = total - completed;
  const completionRate = total > 0 ? Number(((completed / total) * 100).toFixed(1)) : 0;

  return { total, completed, pending, completionRate };
}
