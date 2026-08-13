export interface CourseTagItem {
  id: string;
  courseId: string;
  tagName: string;
  tagType: 'Skill' | 'Position' | 'General' | string;
}

export interface QuestionBankItem {
  id: string;
  content: string;
}

/**
 * Tính toán phân bổ tỷ lệ xếp loại A/B/C/D
 */
export function calculateGradeDistribution(grades: string[]): {
  total: number;
  distributions: Record<'A' | 'B' | 'C' | 'D', { count: number; percentage: number }>;
} {
  const total = grades.length;
  const dist: Record<'A' | 'B' | 'C' | 'D', { count: number; percentage: number }> = {
    A: { count: 0, percentage: 0 },
    B: { count: 0, percentage: 0 },
    C: { count: 0, percentage: 0 },
    D: { count: 0, percentage: 0 },
  };

  if (total === 0) return { total: 0, distributions: dist };

  grades.forEach((g) => {
    const key = (g.toUpperCase() as 'A' | 'B' | 'C' | 'D') || 'D';
    if (dist[key]) {
      dist[key].count++;
    }
  });

  (['A', 'B', 'C', 'D'] as const).forEach((key) => {
    dist[key].percentage = Number(((dist[key].count / total) * 100).toFixed(1));
  });

  return { total, distributions: dist };
}

/**
 * Validate thông tin tag gán cho khóa học
 */
export function validateCourseTag(
  tagName: string,
  tagType: string
): { isValid: boolean; normalizedType: string; error?: string } {
  if (!tagName || !tagName.trim()) {
    return { isValid: false, normalizedType: 'Skill', error: 'Tên tag không được để trống.' };
  }
  const validTypes = ['Skill', 'Position', 'General'];
  const found = validTypes.find((t) => t.toLowerCase() === tagType.trim().toLowerCase());

  return {
    isValid: true,
    normalizedType: found || 'Skill',
  };
}

/**
 * Kiếm tra định dạng phiên bản khóa học (Semantic Versioning)
 */
export function parseSemanticVersion(versionStr: string): { isValid: boolean; normalized: string } {
  const cleaned = versionStr.trim();
  const isMatch = /^\d+\.\d+(\.\d+)?$/.test(cleaned);
  return {
    isValid: isMatch,
    normalized: isMatch ? cleaned : '1.0',
  };
}

/**
 * Sinh danh sách câu hỏi ngẫu nhiên cho đề thi
 */
export function generateRandomExamQuestions(
  pool: QuestionBankItem[],
  count: number
): { selected: QuestionBankItem[]; count: number } {
  if (!pool || pool.length === 0 || count <= 0) {
    return { selected: [], count: 0 };
  }
  const shuffled = [...pool].sort(() => 0.5 - Math.random());
  const selected = shuffled.slice(0, count);
  return { selected, count: selected.length };
}
