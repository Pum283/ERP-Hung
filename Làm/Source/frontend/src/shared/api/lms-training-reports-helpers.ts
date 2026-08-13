export function calculateOverdueDays(
  dueDateStr: string,
  currentDateStr: string = new Date().toISOString()
): number {
  const dueTime = new Date(dueDateStr).getTime();
  const currentTime = new Date(currentDateStr).getTime();

  if (currentTime <= dueTime) return 0;

  const diffMs = currentTime - dueTime;
  return Math.max(1, Math.floor(diffMs / (1000 * 60 * 60 * 24)));
}

export interface PassRateResult {
  passRatePct: number;
  gradeBadge: 'Excellent' | 'Good' | 'NeedsImprovement';
}

export function calculatePassRatePct(
  passedCount: number,
  totalAttempts: number
): PassRateResult {
  if (totalAttempts <= 0) {
    return { passRatePct: 0, gradeBadge: 'NeedsImprovement' };
  }

  const rate = Math.min(100, Math.max(0, Math.round((passedCount / totalAttempts) * 1000) / 10));

  let gradeBadge: 'Excellent' | 'Good' | 'NeedsImprovement' = 'Good';
  if (rate >= 85) {
    gradeBadge = 'Excellent';
  } else if (rate < 70) {
    gradeBadge = 'NeedsImprovement';
  }

  return {
    passRatePct: rate,
    gradeBadge,
  };
}

export interface DropoutRateResult {
  dropoutRatePct: number;
  riskLevel: 'Low' | 'Medium' | 'High';
}

export function calculateDropoutRatePct(
  dropoutCount: number,
  totalEnrolled: number
): DropoutRateResult {
  if (totalEnrolled <= 0) {
    return { dropoutRatePct: 0, riskLevel: 'Low' };
  }

  const rate = Math.min(100, Math.max(0, Math.round((dropoutCount / totalEnrolled) * 1000) / 10));

  let riskLevel: 'Low' | 'Medium' | 'High' = 'Low';
  if (rate >= 20) {
    riskLevel = 'High';
  } else if (rate >= 10) {
    riskLevel = 'Medium';
  }

  return {
    dropoutRatePct: rate,
    riskLevel,
  };
}
