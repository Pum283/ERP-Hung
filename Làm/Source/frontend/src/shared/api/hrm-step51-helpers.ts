// hrm-step51-helpers.ts
// Frontend helpers cho Bước 51:
//   UC_LMS_023 — Gán mentor cho học viên (validateLmsMentorAssignment & formatMentorAssignmentSummary)
//   UC_LMS_028 — Đăng ký tài khoản học viên (validateLearnerRegistration)
//   UC_LMS_029 — Đăng nhập / quên mật khẩu (validateLearnerLogin)
//   UC_LMS_030 — Danh sách & chi tiết khóa (formatCourseLessonProgressSummary)

export function validateLmsMentorAssignment(menteeId: string, mentorId: string): { valid: boolean; error?: string } {
  if (!menteeId?.trim())
    return { valid: false, error: 'Chưa chọn học viên (mentee).' };

  if (!mentorId?.trim())
    return { valid: false, error: 'Chưa chọn giảng viên đồng hành (mentor).' };

  if (menteeId.trim().toLowerCase() === mentorId.trim().toLowerCase())
    return { valid: false, error: 'Mentor và mentee không được là cùng một người.' };

  return { valid: true };
}

export function formatMentorAssignmentSummary(menteeName: string, mentorName: string): string {
  return `🤝 Mentor: ${mentorName} ➔ Kèm cặp học viên: ${menteeName}`;
}

export interface LearnerRegistrationInput {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export function validateLearnerRegistration(r: LearnerRegistrationInput): { valid: boolean; error?: string } {
  if (!r.username?.trim() || r.username.trim().length < 3 || r.username.trim().length > 50)
    return { valid: false, error: 'Tên đăng nhập phải từ 3 đến 50 ký tự.' };

  if (!r.email?.trim() || !r.email.includes('@'))
    return { valid: false, error: 'Địa chỉ email không hợp lệ.' };

  if (!r.password || r.password.length < 6)
    return { valid: false, error: 'Mật khẩu tối thiểu 6 ký tự.' };

  if (r.password !== r.confirmPassword)
    return { valid: false, error: 'Xác nhận mật khẩu không trùng khớp.' };

  return { valid: true };
}

export function validateLearnerLogin(username?: string, password?: string): { valid: boolean; error?: string } {
  if (!username?.trim())
    return { valid: false, error: 'Tên đăng nhập không được để trống.' };

  if (!password?.trim())
    return { valid: false, error: 'Mật khẩu không được để trống.' };

  return { valid: true };
}

export function formatCourseLessonProgressSummary(chapterCount: number, lessonCount: number): string {
  return `📚 Gồm ${chapterCount} chương | 📝 Total ${lessonCount} bài học`;
}
