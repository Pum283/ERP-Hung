export interface VideoProtectionResult {
  canDownload: boolean;
  watermarkText: string;
  reason: string;
}

export function evaluateVideoDownloadPermission(
  isDownloadBlocked: boolean,
  userRole: string,
  userName: string = 'Learner'
): VideoProtectionResult {
  const isAdminOrInstructor = ['Admin', 'Instructor', 'Manager'].includes(userRole);

  if (!isDownloadBlocked || isAdminOrInstructor) {
    return {
      canDownload: true,
      watermarkText: '',
      reason: `Được phép tải video (Vai trò: ${userRole}).`,
    };
  }

  return {
    canDownload: false,
    watermarkText: `PROTECTED - USER: ${userName.toUpperCase()} - DO NOT DISTRIBUTE`,
    reason: 'Đã kích hoạt chế độ chặn tải video LMS đối với Học viên.',
  };
}

export interface SurveyScoreResult {
  scorePercentage: number;
  isPass: boolean;
  gradeBadge: string;
}

export function calculateSurveyScore(
  answers: Record<string, number>,
  totalQuestions: number,
  passingThreshold: number = 70
): SurveyScoreResult {
  if (totalQuestions <= 0) {
    return { scorePercentage: 0, isPass: false, gradeBadge: 'Không hợp lệ' };
  }

  const totalScore = Object.values(answers).reduce((sum, val) => sum + val, 0);
  const scorePercentage = Math.min(100, Math.max(0, Math.round((totalScore / (totalQuestions * 10)) * 100)));
  const isPass = scorePercentage >= passingThreshold;

  return {
    scorePercentage,
    isPass,
    gradeBadge: isPass ? 'Đạt tiêu chuẩn' : 'Chưa đạt',
  };
}

export interface ShiftGateResult {
  canEnterWorkShift: boolean;
  gateStatus: 'Passed' | 'Blocked';
  message: string;
}

export function evaluateShiftTrainingGate(
  isMandatoryCompleted: boolean,
  shiftStartTimeStr: string,
  currentTimeStr: string = new Date().toISOString()
): ShiftGateResult {
  if (isMandatoryCompleted) {
    return {
      canEnterWorkShift: true,
      gateStatus: 'Passed',
      message: 'Học viên đã hoàn thành khóa đào tạo bắt buộc trước ca. Cổng chấm công/vào ca MỞ.',
    };
  }

  const shiftStart = new Date(shiftStartTimeStr).getTime();
  const current = new Date(currentTimeStr).getTime();

  if (current >= shiftStart - 30 * 60 * 1000) {
    // Trong vòng 30 phút trước ca hoặc đã quá giờ ca mà chưa học xong
    return {
      canEnterWorkShift: false,
      gateStatus: 'Blocked',
      message: 'CHẶN VÀO CA: Học viên chưa hoàn thành bài đào tạo bắt buộc trước giờ ca làm việc!',
    };
  }

  return {
    canEnterWorkShift: false,
    gateStatus: 'Blocked',
    message: 'Cảnh báo: Ca làm việc sắp bắt đầu, học viên cần hoàn thành khóa học ngay!',
  };
}
