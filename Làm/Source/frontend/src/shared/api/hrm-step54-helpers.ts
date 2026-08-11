// hrm-step54-helpers.ts
// Frontend helpers cho Bước 54:
//   UC_LMS_041 — Thi cuối khóa (formatExamTimeRemaining)
//   UC_LMS_042 — Chấm điểm tự động (calculateExamScorePercentage)
//   UC_LMS_043 — Xem kết quả & đáp án (evaluateExamPassStatus)
//   UC_LMS_044 — Điều kiện cấp chứng chỉ (formatCertificateEligibilityMessage)

export function formatExamTimeRemaining(durationMin?: number, startedAtIso?: string): string {
  if (!durationMin || durationMin <= 0 || !startedAtIso) return '⏱️ Không giới hạn thời gian';

  const startTime = new Date(startedAtIso).getTime();
  const endTime = startTime + durationMin * 60 * 1000;
  const now = Date.now();
  const diffSec = Math.max(0, Math.floor((endTime - now) / 1000));

  const minutes = Math.floor(diffSec / 60);
  const seconds = diffSec % 60;
  const padSec = seconds < 10 ? `0${seconds}` : `${seconds}`;

  return diffSec > 0 ? `⏳ Còn lại: ${minutes}:${padSec}` : '⌛ Đã hết thời gian thi';
}

export function calculateExamScorePercentage(score: number, maxScore: number): number {
  if (isNaN(maxScore) || maxScore <= 0) return 0;
  return Math.min(100, Math.round(((score || 0) / maxScore) * 10000) / 100);
}

export function evaluateExamPassStatus(scorePct: number, passScorePct: number): { isPassed: boolean; label: string; badgeColor: string } {
  const isPassed = scorePct >= passScorePct;
  return {
    isPassed,
    label: isPassed ? '🎉 ĐẠT KẾT QUẢ' : '❌ KHÔNG ĐẠT',
    badgeColor: isPassed ? 'success' : 'danger',
  };
}

export function formatCertificateEligibilityMessage(isPassed: boolean, courseCompletionPct: number): { eligible: boolean; message: string } {
  if (!isPassed)
    return { eligible: false, message: 'Bạn chưa đạt bài thi cuối khóa. Vui lòng thi lại.' };

  if (courseCompletionPct < 100)
    return { eligible: false, message: `Bạn đã đạt bài thi nhưng chưa hoàn thành 100% bài học (${courseCompletionPct}%).` };

  return { eligible: true, message: '🏆 Bạn đủ điều kiện nhận chứng chỉ hoàn thành khóa học!' };
}
