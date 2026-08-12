export interface AccountDevice {
  deviceId: string;
  ipAddress: string;
}

/**
 * Kiểm tra điều kiện chứng chỉ để đồng bộ sang HRM
 */
export function validateHrmSyncEligibility(
  status: string,
  issuedAt?: string
): { isEligible: boolean; reason?: string } {
  if ((status || '').trim().toLowerCase() === 'revoked') {
    return { isEligible: false, reason: 'Chứng chỉ đã bị thu hồi, không thể đồng bộ.' };
  }
  if (!issuedAt) {
    return { isEligible: false, reason: 'Chứng chỉ chưa có ngày cấp chính thức.' };
  }
  return { isEligible: true };
}

/**
 * Đánh giá kết quả điểm số bài tập (0-100)
 */
export function evaluateAssignmentScore(score: number): {
  isPass: boolean;
  grade: 'Xuất sắc' | 'Đạt' | 'Cần sửa';
  badgeColor: 'success' | 'warning' | 'danger';
} {
  if (typeof score !== 'number' || isNaN(score) || score < 50) {
    return { isPass: false, grade: 'Cần sửa', badgeColor: 'danger' };
  }
  if (score >= 90) {
    return { isPass: true, grade: 'Xuất sắc', badgeColor: 'success' };
  }
  return { isPass: true, grade: 'Đạt', badgeColor: 'warning' };
}

/**
 * Tính toán doanh thu khóa học
 */
export function calculateCourseRevenue(
  price: number,
  paidEnrollments: number
): { grossRevenue: number; formattedVnd: string } {
  const p = Math.max(0, price || 0);
  const count = Math.max(0, paidEnrollments || 0);
  const grossRevenue = p * count;

  return {
    grossRevenue,
    formattedVnd: grossRevenue.toLocaleString('vi-VN') + ' VNĐ',
  };
}

/**
 * Kiểm tra cảnh báo chia sẻ tài khoản / truy cập bất thường
 */
export function checkAccountSharingViolation(
  deviceId: string,
  ipAddress: string,
  activeDevices: AccountDevice[]
): { isViolation: boolean; shouldForceLogout: boolean; warningMsg?: string } {
  if (!deviceId || !ipAddress) {
    return { isViolation: true, shouldForceLogout: true, warningMsg: 'Thông tin thiết bị không hợp lệ.' };
  }

  const isKnownDevice = activeDevices.some((d) => d.deviceId === deviceId);
  const isSuspiciousIp = ipAddress.startsWith('10.99.') || ipAddress.startsWith('192.168.99.');

  if (isSuspiciousIp || (!isKnownDevice && activeDevices.length >= 2)) {
    return {
      isViolation: true,
      shouldForceLogout: true,
      warningMsg: 'Cảnh báo: Phát hiện đăng nhập từ IP / thiết bị khác đồng thời! Đã đăng xuất phiên cũ.',
    };
  }

  return { isViolation: false, shouldForceLogout: false };
}
