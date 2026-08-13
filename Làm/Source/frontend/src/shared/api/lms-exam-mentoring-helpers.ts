export interface ChecklistItem {
  id: string;
  taskName: string;
  isCompleted: boolean;
}

/**
 * Kiểm tra vi phạm chống gian lận trong thời gian thi
 */
export function checkAntiCheatViolation(
  focusLossCount: number,
  tabSwitchCount: number,
  timeRemainingSeconds: number
): { isViolated: boolean; shouldForceSubmit: boolean; reason?: string } {
  if (timeRemainingSeconds <= 0) {
    return { isViolated: true, shouldForceSubmit: true, reason: 'Hết thời gian làm bài.' };
  }
  if (tabSwitchCount >= 3 || focusLossCount >= 5) {
    return { isViolated: true, shouldForceSubmit: true, reason: 'Vi phạm chuyển tab / mất focus vượt ngưỡng cho phép (>= 3 lần).' };
  }
  if (tabSwitchCount > 0 || focusLossCount > 0) {
    return { isViolated: true, shouldForceSubmit: false, reason: 'Cảnh báo: Bạn vừa rời khỏi màn hình thi!' };
  }
  return { isViolated: false, shouldForceSubmit: false };
}

/**
 * Tính toán tiến độ hoàn thành checklist kèm cặp
 */
export function calculateMentoringProgress(tasks: ChecklistItem[]): {
  total: number;
  completed: number;
  percentage: number;
} {
  const total = tasks.length;
  if (total === 0) return { total: 0, completed: 0, percentage: 0 };

  const completed = tasks.filter((t) => t.isCompleted).length;
  const percentage = Number(((completed / total) * 100).toFixed(1));

  return { total, completed, percentage };
}

/**
 * Validate thang điểm đánh giá 1-5 sao
 */
export function validateRatingScore(rating: number): { isValid: boolean; normalizedRating: number } {
  if (typeof rating !== 'number' || isNaN(rating)) {
    return { isValid: false, normalizedRating: 5 };
  }
  const rounded = Math.round(rating);
  const clamped = Math.max(1, Math.min(5, rounded));
  return {
    isValid: rating >= 1 && rating <= 5,
    normalizedRating: clamped,
  };
}

/**
 * Tổng hợp thông số báo cáo hiệu quả mentoring
 */
export function summarizeMentoringEffectiveness(
  totalAssignments: number,
  completedChecklists: number,
  totalChecklists: number,
  mentorRatings: number[],
  menteeRatings: number[]
): {
  completionRatePct: number;
  avgMentorRating: number;
  avgMenteeRating: number;
} {
  const completionRatePct = totalChecklists > 0 ? Number(((completedChecklists / totalChecklists) * 100).toFixed(1)) : 0;

  const avgMentorRating =
    mentorRatings.length > 0
      ? Number((mentorRatings.reduce((a, b) => a + b, 0) / mentorRatings.length).toFixed(2))
      : 0;

  const avgMenteeRating =
    menteeRatings.length > 0
      ? Number((menteeRatings.reduce((a, b) => a + b, 0) / menteeRatings.length).toFixed(2))
      : 0;

  return { completionRatePct, avgMentorRating, avgMenteeRating };
}
