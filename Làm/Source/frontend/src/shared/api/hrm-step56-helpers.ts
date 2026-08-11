// hrm-step56-helpers.ts
// Frontend helpers cho Bước 56:
//   UC_LMS_058 — Xác nhận đã đọc nội quy (formatPolicyAckSummary)
//   UC_LMS_065 — Dashboard tiến độ đào tạo (calculateOverallPassRate)
//   UC_LMS_066 — Báo cáo hoàn thành theo đơn vị (formatOrgCompletionRow)
//   UC_LMS_070 — Xuất báo cáo đào tạo (generateTrainingReportFileName)

export function formatPolicyAckSummary(policyTitle: string, acknowledgedAtIso?: string): { isAcknowledged: boolean; statusText: string } {
  if (!acknowledgedAtIso)
    return { isAcknowledged: false, statusText: `⚠️ Chưa xác nhận đọc nội quy: ${policyTitle}` };

  const formattedDate = new Date(acknowledgedAtIso).toLocaleDateString('vi-VN');
  return {
    isAcknowledged: true,
    statusText: `✅ Đã xác nhận đọc: ${policyTitle} (vào ngày ${formattedDate})`,
  };
}

export function calculateOverallPassRate(passedCount: number, totalSubmittedCount: number): { passRatePct: number; label: string } {
  if (isNaN(totalSubmittedCount) || totalSubmittedCount <= 0)
    return { passRatePct: 0, label: 'Chưa có lượt thi nộp bài' };

  const passRatePct = Math.min(100, Math.round(((passedCount || 0) / totalSubmittedCount) * 10000) / 100);
  return {
    passRatePct,
    label: `📊 Tỷ lệ đậu: ${passRatePct}% (${passedCount}/${totalSubmittedCount} lượt đạt)`,
  };
}

export function formatOrgCompletionRow(orgName: string, onlineCompleted: number, onlineTotal: number, offlineCompleted: number, offlineTotal: number): string {
  const totalCompleted = (onlineCompleted || 0) + (offlineCompleted || 0);
  const totalEnrollments = (onlineTotal || 0) + (offlineTotal || 0);
  const pct = totalEnrollments > 0 ? Math.round((totalCompleted / totalEnrollments) * 100) : 0;

  return `🏢 ${orgName}: Hoàn thành ${totalCompleted}/${totalEnrollments} lượt học (${pct}%) — Online: ${onlineCompleted}/${onlineTotal} | Offline: ${offlineCompleted}/${offlineTotal}`;
}

export function generateTrainingReportFileName(reportType: string, tenantCode = 'ERP'): string {
  const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '');
  const typeMap: Record<string, string> = {
    OnlineEnrollments: 'BaoCao_HocVien_Online',
    Classes: 'BaoCao_LopHoc_Offline',
    Courses: 'BaoCao_DanhMuc_KhoaHoc',
    Certificates: 'BaoCao_ChungChi_Cap',
  };

  const filePrefix = typeMap[reportType] || 'BaoCao_DaoTao';
  return `${tenantCode}_${filePrefix}_${dateStr}.csv`;
}
