// hrm-step50-helpers.ts
// Frontend helpers cho Bước 50:
//   UC_LMS_017 — Gán giảng viên / địa điểm / lịch (validateLmsClassSessionInput)
//   UC_LMS_018 — Tuyển sinh / ghi danh học viên (calculateClassAttendanceRate)
//   UC_LMS_019 — Điểm danh buổi học (formatLmsAttendanceStatus)
//   UC_LMS_022 — Đóng lớp & tổng kết (formatLmsClassStatus)

export interface ClassSessionInput {
  topic: string;
  sessionDate: string;
  startTime: string;
  endTime: string;
}

export function validateLmsClassSessionInput(s: ClassSessionInput): { valid: boolean; error?: string } {
  if (!s.topic?.trim() || s.topic.trim().length > 300)
    return { valid: false, error: 'Chủ đề buổi học phải từ 1 đến 300 ký tự.' };

  if (!s.sessionDate?.trim())
    return { valid: false, error: 'Ngày học bắt buộc phải điền.' };

  if (!s.startTime?.trim() || !s.endTime?.trim())
    return { valid: false, error: 'Thời gian bắt đầu và kết thúc buổi học không được để trống.' };

  if (s.startTime >= s.endTime)
    return { valid: false, error: 'Giờ kết thúc phải sau giờ bắt đầu buổi học.' };

  return { valid: true };
}

export function formatLmsAttendanceStatus(status: string): string {
  switch (status) {
    case 'Present':
      return '✅ Có mặt';
    case 'Absent':
      return '❌ Vắng mặt';
    case 'Late':
      return '⏰ Đi trễ';
    case 'Excused':
      return '📝 Vắng có lý do';
    default:
      return status || 'Khác';
  }
}

export function calculateClassAttendanceRate(totalLearners: number, presentCount: number): { attendanceRatePct: number; isGood: boolean } {
  if (isNaN(totalLearners) || totalLearners <= 0) return { attendanceRatePct: 0, isGood: false };
  const attendanceRatePct = Math.round(((presentCount || 0) / totalLearners) * 10000) / 100;
  return { attendanceRatePct, isGood: attendanceRatePct >= 80 };
}

export function formatLmsClassStatus(status: string): string {
  switch (status) {
    case 'Draft':
      return '📝 Bản nháp';
    case 'Open':
      return '🟢 Đang mở ghi danh';
    case 'InProgress':
      return '📘 Đang diễn ra';
    case 'Closed':
      return '🏁 Đã kết thúc / Đóng lớp';
    default:
      return status || 'Khác';
  }
}
