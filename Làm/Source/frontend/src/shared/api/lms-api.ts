import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type LmsTrainingClassDto = {
  id: string;
  code: string;
  name: string;
  courseTitle: string;
  instructorId?: string | null;
  instructorName?: string | null;
  location?: string | null;
  startDate: string;
  endDate: string;
  status: string;
  summaryNote?: string | null;
  sessionCount: number;
  enrollmentCount: number;
};

export type LmsClassSessionDto = {
  id: string;
  classId: string;
  sessionDate: string;
  topic: string;
  startTime: string;
  endTime: string;
  sortOrder: number;
};

export type LmsClassEnrollmentDto = {
  id: string;
  classId: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  status: string;
  enrolledAt: string;
};

export type LmsSessionAttendanceDto = {
  id: string;
  sessionId: string;
  enrollmentId: string;
  present: boolean;
  note?: string | null;
};

export type LmsClassDetailDto = {
  class: LmsTrainingClassDto;
  sessions: LmsClassSessionDto[];
  enrollments: LmsClassEnrollmentDto[];
  attendance: LmsSessionAttendanceDto[];
};

export type LmsMentorAssignmentDto = {
  id: string;
  menteeEmployeeId: string;
  menteeCode: string;
  menteeName: string;
  mentorEmployeeId: string;
  mentorCode: string;
  mentorName: string;
  note?: string | null;
  isActive: boolean;
};

export async function fetchLmsClasses() {
  const { data } = await api.get<Envelope<LmsTrainingClassDto[]>>("/api/lms/classes");
  return data.data;
}

export async function upsertLmsClass(body: {
  id?: string;
  code: string;
  name: string;
  courseTitle: string;
  instructorId?: string | null;
  instructorName?: string;
  location?: string;
  startDate: string;
  endDate: string;
  status?: string;
}) {
  const { data } = await api.post<Envelope<LmsTrainingClassDto>>("/api/lms/classes", body);
  return data.data;
}

export async function fetchLmsClassDetail(id: string) {
  const { data } = await api.get<Envelope<LmsClassDetailDto>>(`/api/lms/classes/${id}`);
  return data.data;
}

export async function addLmsClassSession(
  classId: string,
  body: {
    sessionDate: string;
    topic: string;
    startTime: string;
    endTime: string;
    sortOrder?: number;
  },
) {
  const { data } = await api.post<Envelope<LmsClassSessionDto>>(
    `/api/lms/classes/${classId}/sessions`,
    body,
  );
  return data.data;
}

export async function enrollLmsClass(classId: string, employeeId: string) {
  const { data } = await api.post<Envelope<LmsClassEnrollmentDto>>(
    `/api/lms/classes/${classId}/enrollments`,
    { employeeId },
  );
  return data.data;
}

export async function closeLmsClass(classId: string, summaryNote?: string) {
  const { data } = await api.post<Envelope<LmsTrainingClassDto>>(
    `/api/lms/classes/${classId}/close`,
    { summaryNote },
  );
  return data.data;
}

export async function recordLmsAttendance(
  sessionId: string,
  body: { enrollmentId: string; present: boolean; note?: string },
) {
  const { data } = await api.post<Envelope<LmsSessionAttendanceDto>>(
    `/api/lms/sessions/${sessionId}/attendance`,
    body,
  );
  return data.data;
}

export async function fetchLmsMentors() {
  const { data } = await api.get<Envelope<LmsMentorAssignmentDto[]>>("/api/lms/mentors");
  return data.data;
}

export async function assignLmsMentor(body: {
  menteeEmployeeId: string;
  mentorEmployeeId: string;
  note?: string;
}) {
  const { data } = await api.post<Envelope<LmsMentorAssignmentDto>>("/api/lms/mentors", body);
  return data.data;
}

// ——— Catalog / learn ———

export type LmsProgramDto = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  status: string;
};

export type LmsCourseDto = {
  id: string;
  programId?: string | null;
  programName?: string | null;
  code: string;
  name: string;
  summary?: string | null;
  deliveryMode: string;
  status: string;
  price: number;
  currency: string;
  coverUrl?: string | null;
  chapterCount: number;
  lessonCount: number;
};

export type LmsChapterDto = {
  id: string;
  courseId: string;
  title: string;
  sortOrder: number;
  lessonCount: number;
};

export type LmsLessonDto = {
  id: string;
  chapterId: string;
  title: string;
  lessonType: string;
  contentUrl?: string | null;
  body?: string | null;
  sortOrder: number;
  durationSec?: number | null;
};

export type LmsCourseDetailDto = {
  course: LmsCourseDto;
  chapters: LmsChapterDto[];
  lessons: LmsLessonDto[];
};

export type LmsCatalogCourseDto = {
  id: string;
  code: string;
  name: string;
  summary?: string | null;
  deliveryMode: string;
  price: number;
  currency: string;
  coverUrl?: string | null;
  lessonCount: number;
  enrollmentStatus?: string | null;
  progressPct: number;
};

export type LmsOnlineEnrollmentDto = {
  id: string;
  courseId: string;
  userId: string;
  status: string;
  paidAmount: number;
  paidAt?: string | null;
  lastLessonId?: string | null;
  progressPct: number;
};

export type LmsLessonProgressDto = {
  lessonId: string;
  status: string;
  completedAt?: string | null;
  lastPositionSec?: number | null;
};

export type LmsLearnCourseDto = {
  course: LmsCourseDto;
  chapters: LmsChapterDto[];
  lessons: LmsLessonDto[];
  enrollment: LmsOnlineEnrollmentDto;
  progress: LmsLessonProgressDto[];
  resumeLessonId?: string | null;
};

export async function fetchLmsPrograms() {
  const { data } = await api.get<Envelope<LmsProgramDto[]>>("/api/lms/programs");
  return data.data;
}

export async function upsertLmsProgram(body: {
  id?: string;
  code: string;
  name: string;
  description?: string;
  status?: string;
}) {
  const { data } = await api.post<Envelope<LmsProgramDto>>("/api/lms/programs", body);
  return data.data;
}

export async function fetchLmsCourses() {
  const { data } = await api.get<Envelope<LmsCourseDto[]>>("/api/lms/courses");
  return data.data;
}

export async function upsertLmsCourse(body: {
  id?: string;
  programId?: string | null;
  code: string;
  name: string;
  summary?: string;
  deliveryMode: string;
  status?: string;
  price: number;
  currency?: string;
  coverUrl?: string;
}) {
  const { data } = await api.post<Envelope<LmsCourseDto>>("/api/lms/courses", body);
  return data.data;
}

export async function fetchLmsCourseDetail(id: string) {
  const { data } = await api.get<Envelope<LmsCourseDetailDto>>(`/api/lms/courses/${id}`);
  return data.data;
}

export async function publishLmsCourse(id: string, status: string) {
  const { data } = await api.post<Envelope<LmsCourseDto>>(`/api/lms/courses/${id}/publish`, { status });
  return data.data;
}

export async function upsertLmsChapter(
  courseId: string,
  body: { id?: string; title: string; sortOrder?: number },
) {
  const { data } = await api.post<Envelope<LmsChapterDto>>(
    `/api/lms/courses/${courseId}/chapters`,
    body,
  );
  return data.data;
}

export async function upsertLmsLesson(
  chapterId: string,
  body: {
    id?: string;
    title: string;
    lessonType: string;
    contentUrl?: string;
    body?: string;
    sortOrder?: number;
    durationSec?: number;
  },
) {
  const { data } = await api.post<Envelope<LmsLessonDto>>(
    `/api/lms/chapters/${chapterId}/lessons`,
    body,
  );
  return data.data;
}

export async function fetchLmsCatalog() {
  const { data } = await api.get<Envelope<LmsCatalogCourseDto[]>>("/api/lms/catalog");
  return data.data;
}

export async function enrollLmsCourse(courseId: string, voucherCode?: string) {
  const { data } = await api.post<Envelope<LmsOnlineEnrollmentDto>>(
    `/api/lms/catalog/${courseId}/enroll`,
    { voucherCode },
  );
  return data.data;
}

export async function fetchLmsLearn(courseId: string) {
  const { data } = await api.get<Envelope<LmsLearnCourseDto>>(`/api/lms/learn/${courseId}`);
  return data.data;
}

export async function completeLmsLesson(
  courseId: string,
  lessonId: string,
  lastPositionSec?: number,
) {
  const { data } = await api.post<Envelope<LmsLessonProgressDto>>(
    `/api/lms/learn/${courseId}/lessons/${lessonId}/complete`,
    { lastPositionSec },
  );
  return data.data;
}

// ——— Exam / certificate ———

export type LmsQuestionOptionDto = { key: string; text: string };

export type LmsQuestionDto = {
  id: string;
  code: string;
  stem: string;
  questionType: string;
  options: LmsQuestionOptionDto[];
  correctKeys: string[];
  points: number;
  tag?: string | null;
  isActive: boolean;
};

export type LmsExamDto = {
  id: string;
  code: string;
  name: string;
  examType: string;
  courseId?: string | null;
  courseName?: string | null;
  chapterId?: string | null;
  chapterTitle?: string | null;
  passScore: number;
  maxAttempts: number;
  timeLimitMin?: number | null;
  status: string;
  questionCount: number;
};

export type LmsExamQuestionItemDto = {
  id: string;
  questionId: string;
  questionCode: string;
  stem: string;
  questionType: string;
  sortOrder: number;
  points: number;
};

export type LmsExamDetailDto = {
  exam: LmsExamDto;
  questions: LmsExamQuestionItemDto[];
};

export type LmsLearnerExamDto = {
  id: string;
  code: string;
  name: string;
  examType: string;
  chapterId?: string | null;
  passScore: number;
  maxAttempts: number;
  attemptsUsed: number;
  canStart: boolean;
  lastPassed?: boolean | null;
  lastScore?: number | null;
};

export type LmsTakeQuestionDto = {
  questionId: string;
  stem: string;
  questionType: string;
  options: LmsQuestionOptionDto[];
  points: number;
  sortOrder: number;
};

export type LmsAttemptDto = {
  id: string;
  examId: string;
  attemptNo: number;
  status: string;
  startedAt: string;
  submittedAt?: string | null;
  score: number;
  maxScore: number;
  passed: boolean;
  questions?: LmsTakeQuestionDto[] | null;
};

export type LmsAnswerReviewDto = {
  questionId: string;
  stem: string;
  yourKey?: string | null;
  correctKeys: string[];
  isCorrect: boolean;
  pointsEarned: number;
  points: number;
};

export type LmsCertificateDto = {
  id: string;
  courseId: string;
  courseName: string;
  userId: string;
  code: string;
  issuedAt: string;
  status: string;
  scoreAtIssue?: number | null;
};

export type LmsAttemptResultDto = {
  id: string;
  examId: string;
  attemptNo: number;
  score: number;
  maxScore: number;
  passed: boolean;
  passScore: number;
  reviews: LmsAnswerReviewDto[];
  certificate?: LmsCertificateDto | null;
};

export async function fetchLmsQuestions() {
  const { data } = await api.get<Envelope<LmsQuestionDto[]>>("/api/lms/questions");
  return data.data;
}

export async function upsertLmsQuestion(body: {
  id?: string;
  code: string;
  stem: string;
  questionType: string;
  options: LmsQuestionOptionDto[];
  correctKeys: string[];
  points: number;
  tag?: string;
  isActive?: boolean;
}) {
  const { data } = await api.post<Envelope<LmsQuestionDto>>("/api/lms/questions", body);
  return data.data;
}

export async function fetchLmsExams() {
  const { data } = await api.get<Envelope<LmsExamDto[]>>("/api/lms/exams");
  return data.data;
}

export async function upsertLmsExam(body: {
  id?: string;
  code: string;
  name: string;
  examType: string;
  courseId?: string | null;
  chapterId?: string | null;
  passScore: number;
  maxAttempts: number;
  timeLimitMin?: number | null;
  status?: string;
}) {
  const { data } = await api.post<Envelope<LmsExamDto>>("/api/lms/exams", body);
  return data.data;
}

export async function fetchLmsExamDetail(id: string) {
  const { data } = await api.get<Envelope<LmsExamDetailDto>>(`/api/lms/exams/${id}`);
  return data.data;
}

export async function publishLmsExam(id: string, status: string) {
  const { data } = await api.post<Envelope<LmsExamDto>>(`/api/lms/exams/${id}/publish`, { status });
  return data.data;
}

export async function addQuestionToLmsExam(examId: string, questionId: string, pointsOverride?: number) {
  const { data } = await api.post<Envelope<LmsExamQuestionItemDto>>(
    `/api/lms/exams/${examId}/questions`,
    { questionId, pointsOverride },
  );
  return data.data;
}

export async function fetchLearnerExams(courseId: string) {
  const { data } = await api.get<Envelope<LmsLearnerExamDto[]>>(`/api/lms/learn/${courseId}/exams`);
  return data.data;
}

export async function startLmsExam(examId: string) {
  const { data } = await api.post<Envelope<LmsAttemptDto>>(`/api/lms/exams/${examId}/start`, {});
  return data.data;
}

export async function submitLmsAttempt(attemptId: string, answers: Record<string, string>) {
  const { data } = await api.post<Envelope<LmsAttemptResultDto>>(
    `/api/lms/attempts/${attemptId}/submit`,
    { answers },
  );
  return data.data;
}

export async function fetchMyLmsCertificates() {
  const { data } = await api.get<Envelope<LmsCertificateDto[]>>("/api/lms/certificates/mine");
  return data.data;
}
