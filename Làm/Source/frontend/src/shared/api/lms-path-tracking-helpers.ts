export interface ComplianceRateResult {
  complianceRatePct: number;
  statusBadge: 'Good' | 'Warning' | 'Critical';
}

export function calculateComplianceRatePct(
  acknowledgedCount: number,
  totalEmployees: number
): ComplianceRateResult {
  if (totalEmployees <= 0) {
    return { complianceRatePct: 0, statusBadge: 'Critical' };
  }

  const rate = Math.min(100, Math.max(0, Math.round((acknowledgedCount / totalEmployees) * 1000) / 10));

  let statusBadge: 'Good' | 'Warning' | 'Critical' = 'Good';
  if (rate < 70) {
    statusBadge = 'Critical';
  } else if (rate < 85) {
    statusBadge = 'Warning';
  }

  return {
    complianceRatePct: rate,
    statusBadge,
  };
}

export interface PathProgressEvaluation {
  progressPct: number;
  isCompleted: boolean;
  isOverdue: boolean;
  statusText: string;
}

export function evaluatePathProgress(
  completedCoursesCount: number,
  totalCoursesCount: number,
  dueDateStr: string,
  currentTimeStr: string = new Date().toISOString()
): PathProgressEvaluation {
  if (totalCoursesCount <= 0) {
    return { progressPct: 100, isCompleted: true, isOverdue: false, statusText: 'Đã hoàn thành' };
  }

  const progressPct = Math.min(100, Math.max(0, Math.round((completedCoursesCount / totalCoursesCount) * 100)));
  const isCompleted = completedCoursesCount >= totalCoursesCount;

  const dueTime = new Date(dueDateStr).getTime();
  const currentTime = new Date(currentTimeStr).getTime();
  const isOverdue = !isCompleted && currentTime > dueTime;

  let statusText = 'Đang học';
  if (isCompleted) {
    statusText = 'Đã hoàn thành';
  } else if (isOverdue) {
    statusText = 'Quá hạn đào tạo';
  }

  return {
    progressPct,
    isCompleted,
    isOverdue,
    statusText,
  };
}

export function filterLearningPathsByRole<T extends { jobTitle: string; title: string }>(
  paths: T[],
  searchRole: string
): T[] {
  if (!searchRole || !searchRole.trim()) return paths;

  const term = searchRole.trim().toLowerCase();
  return paths.filter(
    (p) => p.jobTitle.toLowerCase().includes(term) || p.title.toLowerCase().includes(term)
  );
}
