// hrm-step53-helpers.ts
// Frontend helpers cho Bước 53:
//   UC_LMS_035 — Đánh dấu hoàn thành bài học (formatLessonCompletionStatus)
//   UC_LMS_036 — Tiếp tục học dở (formatResumeLessonText)
//   UC_LMS_037 — Theo dõi % tiến độ khóa (calculateCourseCompletionPercentage)
//   UC_LMS_040 — Làm quiz cuối chương (validateChapterQuizSubmission & formatQuizResultSummary)

export function formatLessonCompletionStatus(isCompleted: boolean): string {
  return isCompleted ? '✅ Đã hoàn thành' : '⏺️ Chưa hoàn thành';
}

export function formatResumeLessonText(lessonTitle?: string, chapterTitle?: string): string {
  if (!lessonTitle?.trim()) return '📖 Bắt đầu học bài đầu tiên';
  if (chapterTitle?.trim()) return `▶️ Tiếp tục học: ${chapterTitle} — ${lessonTitle}`;
  return `▶️ Tiếp tục học: ${lessonTitle}`;
}

export function calculateCourseCompletionPercentage(completedLessonsCount: number, totalLessonsCount: number): { completionPct: number; isFullyCompleted: boolean } {
  if (isNaN(totalLessonsCount) || totalLessonsCount <= 0) return { completionPct: 0, isFullyCompleted: false };
  const completionPct = Math.min(100, Math.round(((completedLessonsCount || 0) / totalLessonsCount) * 10000) / 100);
  return { completionPct, isFullyCompleted: completionPct >= 100 };
}

export interface QuizSubmissionItem {
  questionId: string;
  selectedAnswers: string[];
}

export function validateChapterQuizSubmission(answers: QuizSubmissionItem[], totalQuestions: number): { valid: boolean; answeredCount: number; unAnsweredCount: number; warningMsg?: string } {
  const answeredCount = answers.filter(a => a.selectedAnswers && a.selectedAnswers.length > 0).length;
  const unAnsweredCount = Math.max(0, totalQuestions - answeredCount);

  if (answeredCount === 0)
    return { valid: false, answeredCount, unAnsweredCount, warningMsg: 'Bạn chưa trả lời câu hỏi nào.' };

  if (unAnsweredCount > 0)
    return { valid: true, answeredCount, unAnsweredCount, warningMsg: `Vẫn còn ${unAnsweredCount} câu chưa trả lời. Bạn có chắc muốn nộp bài?` };

  return { valid: true, answeredCount, unAnsweredCount };
}

export function formatQuizResultSummary(score: number, maxScore: number, passScorePct: number, isPassed: boolean): string {
  const scorePct = maxScore > 0 ? Math.round((score / maxScore) * 100) : 0;
  const statusText = isPassed ? '🎉 ĐẠT' : '❌ KHÔNG ĐẠT';
  return `${statusText} | Kết quả: ${score}/${maxScore} điểm (${scorePct}%) | Điểm yêu cầu: ${passScorePct}%`;
}
