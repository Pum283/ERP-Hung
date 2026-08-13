export function calculateAiMatchScore(
  userSkills: string[],
  requiredSkills: string[]
): number {
  if (!requiredSkills || requiredSkills.length === 0) return 100;
  if (!userSkills || userSkills.length === 0) return 50;

  const userSkillsLower = userSkills.map((s) => s.toLowerCase());
  const matched = requiredSkills.filter((req) =>
    userSkillsLower.includes(req.toLowerCase())
  ).length;

  const score = Math.round((matched / requiredSkills.length) * 100);
  return Math.min(100, Math.max(60, score));
}

export function formatAiSummaryBullets(summaryText: string): string[] {
  if (!summaryText || !summaryText.trim()) return [];

  return summaryText
    .split(/\n|\./)
    .map((s) => s.trim())
    .filter((s) => s.length > 5);
}

export interface QuizValidationResult {
  isValid: boolean;
  errorMessage?: string;
}

export function validateAiQuizStructure(
  questions: Array<{ questionText: string; options: string[]; correctOptionIndex: number }>
): QuizValidationResult {
  if (!questions || questions.length === 0) {
    return { isValid: false, errorMessage: 'Danh sách câu hỏi không được để trống.' };
  }

  for (let i = 0; i < questions.length; i++) {
    const q = questions[i];
    if (!q.questionText || !q.questionText.trim()) {
      return { isValid: false, errorMessage: `Câu hỏi số ${i + 1} thiếu tiêu đề.` };
    }
    if (!q.options || q.options.length < 2) {
      return { isValid: false, errorMessage: `Câu hỏi số ${i + 1} phải có ít nhất 2 đáp án.` };
    }
    if (q.correctOptionIndex < 0 || q.correctOptionIndex >= q.options.length) {
      return { isValid: false, errorMessage: `Câu hỏi số ${i + 1} có đáp án đúng không hợp lệ.` };
    }
  }

  return { isValid: true };
}
