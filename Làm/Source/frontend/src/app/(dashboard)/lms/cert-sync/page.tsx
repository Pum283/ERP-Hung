'use client';

import React, { useState } from 'react';
import {
  validateHrmSyncEligibility,
  evaluateAssignmentScore,
  calculateCourseRevenue,
  checkAccountSharingViolation,
  AccountDevice,
} from '@/shared/api/lms-cert-sync-helpers';

export default function LmsCertSyncOpsPage() {
  const [activeTab, setActiveTab] = useState<'sync' | 'assignment' | 'revenue' | 'guard'>('sync');

  // Toast notifications
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: HRM CERTIFICATE SYNC (UC_LMS_048)
  // ────────────────────────────────────────────────────────────────────────────
  const [certificates, setCertificates] = useState([
    { id: 'cert-1', code: 'CERT-DDD-2026', learner: 'Vũ Thị I (EMP160)', course: 'Khóa học Lập trình Domain-Driven Design', status: 'Active', issuedAt: '12/08/2026', isSynced: false },
    { id: 'cert-2', code: 'CERT-REVOKED-01', learner: 'Trần Văn K (EMP161)', course: 'Khóa học Lập trình Microservices', status: 'Revoked', issuedAt: '01/05/2026', isSynced: false },
  ]);

  const handleSyncToHrm = (certId: string) => {
    const cert = certificates.find((c) => c.id === certId);
    if (!cert) return;

    const val = validateHrmSyncEligibility(cert.status, cert.issuedAt);
    if (!val.isEligible) {
      showToast(val.reason || 'Không đủ điều kiện đồng bộ sang HRM.', 'error');
      return;
    }

    setCertificates((prev) =>
      prev.map((c) => (c.id === certId ? { ...c, isSynced: true } : c))
    );
    showToast(`Đã đồng bộ thành công chứng chỉ [${cert.code}] vào Hồ sơ Kỹ năng HRM của ${cert.learner}!`);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: ASSIGNMENT FEEDBACK (UC_LMS_052)
  // ────────────────────────────────────────────────────────────────────────────
  const [feedbacks, setFeedbacks] = useState([
    { id: 'fb-1', student: 'Nguyễn Văn A', lesson: 'Bài tập Aggregate Root & Value Object', submissionUrl: 'https://github.com/nguyenvana/ddd-lab', score: 95, comment: 'Thiết kế Bounded Context và Aggregate Root rất mượt mà!', status: 'Graded' },
  ]);
  const [gradeForm, setGradeForm] = useState({ student: 'Trần Thị B', lesson: 'Bài tập Event Sourcing & Kafka', submissionUrl: 'https://github.com/tranthib/kafka-lab', score: 85, comment: '' });

  const handleGradeAssignment = (e: React.FormEvent) => {
    e.preventDefault();
    const evalRes = evaluateAssignmentScore(gradeForm.score);
    if (gradeForm.score < 0 || gradeForm.score > 100) {
      showToast('Điểm số phải từ 0 đến 100.', 'error');
      return;
    }

    setFeedbacks((prev) => [
      {
        id: `fb-${Date.now()}`,
        student: gradeForm.student,
        lesson: gradeForm.lesson,
        submissionUrl: gradeForm.submissionUrl,
        score: gradeForm.score,
        comment: gradeForm.comment || 'Đã hoàn thành chấm điểm bài tập.',
        status: evalRes.isPass ? 'Graded' : 'RevisionRequired',
      },
      ...prev,
    ]);
    showToast(`Đã lưu kết quả chấm điểm bài tập cho học viên ${gradeForm.student} (${evalRes.grade})!`);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: COURSE REVENUE STATS (UC_LMS_053)
  // ────────────────────────────────────────────────────────────────────────────
  const [revenueStats] = useState([
    { courseId: 'c1', courseName: 'Khóa học Lập trình Domain-Driven Design', price: 2000000, totalEnrollments: 25, paidEnrollments: 20 },
    { courseId: 'c2', courseName: 'Khóa học Lập trình Microservices & Event-Driven', price: 3500000, totalEnrollments: 18, paidEnrollments: 15 },
    { courseId: 'c3', courseName: 'Khóa học DevOps & Kubernetes Enterprise', price: 4000000, totalEnrollments: 10, paidEnrollments: 8 },
  ]);

  const totalGrossRevenue = revenueStats.reduce((sum, item) => sum + calculateCourseRevenue(item.price, item.paidEnrollments).grossRevenue, 0);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: ACCOUNT SHARING GUARD (UC_LMS_054)
  // ────────────────────────────────────────────────────────────────────────────
  const [activeSessions, setActiveSessions] = useState<AccountDevice[]>([
    { deviceId: 'DEV-CHROME-WIN11', ipAddress: '14.225.1.1' },
  ]);
  const [testSession, setTestSession] = useState({ deviceId: 'DEV-MOBILE-IOS', ipAddress: '192.168.99.50' });

  const handleValidateSession = (e: React.FormEvent) => {
    e.preventDefault();
    const violation = checkAccountSharingViolation(testSession.deviceId, testSession.ipAddress, activeSessions);

    if (violation.isViolation) {
      showToast(violation.warningMsg || 'Cảnh báo vi phạm chia sẻ tài khoản!', 'error');
      setActiveSessions([{ deviceId: testSession.deviceId, ipAddress: testSession.ipAddress }]);
    } else {
      showToast('Phiên đăng nhập hợp lệ!');
      setActiveSessions((prev) => [...prev, { deviceId: testSession.deviceId, ipAddress: testSession.ipAddress }]);
    }
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Đồng bộ chứng chỉ HRM, Phản hồi bài tập, Thống kê doanh thu & Chống chia sẻ tài khoản LMS
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Đồng bộ chứng chỉ sang HRM (UC_LMS_048), Phản hồi bài tập (UC_LMS_052), Doanh thu (UC_LMS_053) & Chống chia sẻ tài khoản (UC_LMS_054).
          </p>
        </div>
        <div>
          <span className="px-3 py-1 bg-brand-muted text-brand-strong  rounded-full text-xs font-semibold">
            Tiến độ Kế hoạch: 90% [XONG]
          </span>
        </div>
      </div>

      {/* Toast Alert */}
      {toast && (
        <div
          className={`p-4 rounded-lg shadow-md text-sm font-medium transition-all ${
            toast.type === 'success' ? 'bg-emerald-500 text-white' : 'bg-rose-500 text-white'
          }`}
        >
          {toast.message}
        </div>
      )}

      {/* Navigation Tabs */}
      <div className="flex border-b border-border gap-6">
        {[
          { key: 'sync', label: '1. Đồng bộ Chứng chỉ HRM (UC_LMS_048)' },
          { key: 'assignment', label: '2. Phản hồi Bài tập (UC_LMS_052)' },
          { key: 'revenue', label: '3. Doanh thu Khóa học (UC_LMS_053)' },
          { key: 'guard', label: '4. Chống Chia sẻ Tài khoản (UC_LMS_054)' },
        ].map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key as any)}
            className={`pb-3 text-sm font-semibold border-b-2 transition-colors ${
              activeTab === tab.key
                ? 'border-brand text-brand '
                : 'border-transparent text-muted-foreground hover:text-foreground dark:hover:text-slate-300'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 1: HRM CERTIFICATE SYNC */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'sync' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Danh sách Chứng chỉ LMS & Đồng bộ sang Hồ sơ Kỹ năng HRM</h2>
          </div>

          <div className="bg-surface shadow rounded-lg overflow-hidden border border-border">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-muted-foreground dark:text-slate-300">
                <tr>
                  <th className="p-3">Mã chứng chỉ</th>
                  <th className="p-3">Học viên / Nhân sự</th>
                  <th className="p-3">Khóa học</th>
                  <th className="p-3">Trạng thái LMS</th>
                  <th className="p-3">Trạng thái HRM</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {certificates.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-mono font-bold">{c.code}</td>
                    <td className="p-3 font-semibold">{c.learner}</td>
                    <td className="p-3 text-xs text-muted-foreground dark:text-slate-400">{c.course}</td>
                    <td className="p-3">
                      <span className={`px-2 py-0.5 rounded text-xs font-bold ${c.status === 'Active' ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800'}`}>
                        {c.status}
                      </span>
                    </td>
                    <td className="p-3">
                      {c.isSynced ? (
                        <span className="px-2 py-0.5 rounded text-xs font-bold bg-brand-muted text-brand-strong">
                          ✓ Đã đồng bộ HRM
                        </span>
                      ) : (
                        <span className="px-2 py-0.5 rounded text-xs font-medium text-slate-400">
                          Chưa đồng bộ
                        </span>
                      )}
                    </td>
                    <td className="p-3 text-right">
                      <button
                        onClick={() => handleSyncToHrm(c.id)}
                        disabled={c.isSynced}
                        className={`px-3 py-1 text-xs font-medium rounded-lg transition ${
                          c.isSynced
                            ? 'bg-slate-100 text-slate-400 cursor-not-allowed'
                            : 'bg-brand text-white hover:bg-brand-hover'
                        }`}
                      >
                        {c.isSynced ? 'Đã đồng bộ' : '🔄 Đồng bộ HRM'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 2: ASSIGNMENT FEEDBACK */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'assignment' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Chấm điểm & Phản hồi Bài tập Học viên</h2>
          </div>

          <form onSubmit={handleGradeAssignment} className="bg-surface border p-6 rounded-xl space-y-4 shadow-sm">
            <h3 className="text-sm font-bold border-b pb-2">Chấm bài mới</h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-xs font-semibold">Học viên</label>
                <input
                  type="text"
                  value={gradeForm.student}
                  onChange={(e) => setGradeForm({ ...gradeForm, student: e.target.value })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
              <div>
                <label className="text-xs font-semibold">Bài tập / Bài học</label>
                <input
                  type="text"
                  value={gradeForm.lesson}
                  onChange={(e) => setGradeForm({ ...gradeForm, lesson: e.target.value })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
              <div>
                <label className="text-xs font-semibold">Link bài nộp (GitHub / Drive)</label>
                <input
                  type="url"
                  value={gradeForm.submissionUrl}
                  onChange={(e) => setGradeForm({ ...gradeForm, submissionUrl: e.target.value })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm font-mono dark:bg-slate-800"
                  required
                />
              </div>
              <div>
                <label className="text-xs font-semibold">Điểm số (0 - 100)</label>
                <input
                  type="number"
                  min={0}
                  max={100}
                  value={gradeForm.score}
                  onChange={(e) => setGradeForm({ ...gradeForm, score: Number(e.target.value) })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
            </div>
            <div>
              <label className="text-xs font-semibold">Nhận xét của Giảng viên / Mentor</label>
              <textarea
                rows={3}
                placeholder="Nhập nhận xét chi tiết..."
                value={gradeForm.comment}
                onChange={(e) => setGradeForm({ ...gradeForm, comment: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              />
            </div>
            <div className="flex justify-end">
              <button type="submit" className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium hover:bg-brand-hover transition">
                + Lưu phản hồi & Chấm điểm
              </button>
            </div>
          </form>

          <div className="space-y-3">
            <h3 className="text-sm font-bold">Lịch sử Chấm bài & Phản hồi</h3>
            {feedbacks.map((f) => {
              const evalRes = evaluateAssignmentScore(f.score);
              return (
                <div key={f.id} className="p-4 bg-surface border rounded-xl shadow-sm space-y-2">
                  <div className="flex justify-between items-center">
                    <div>
                      <h4 className="font-bold text-sm">{f.student} — <span className="text-muted-foreground font-normal">{f.lesson}</span></h4>
                      <a href={f.submissionUrl} target="_blank" rel="noreferrer" className="text-xs text-brand underline font-mono">
                        {f.submissionUrl}
                      </a>
                    </div>
                    <div className="text-right">
                      <span className={`px-2.5 py-1 rounded text-xs font-bold ${evalRes.badgeColor === 'success' ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'}`}>
                        {f.score}/100 ({evalRes.grade})
                      </span>
                    </div>
                  </div>
                  <p className="text-xs text-muted-foreground dark:text-slate-400 bg-slate-50 dark:bg-slate-800 p-2.5 rounded-lg border">
                    💬 "{f.comment}"
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 3: COURSE REVENUE STATS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'revenue' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Báo cáo Doanh thu & Lượt Đăng ký Khóa học LMS</h2>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div className="p-5 bg-brand text-white rounded-xl shadow-md space-y-1">
              <span className="text-xs text-brand-foreground/80 font-medium">Tổng Doanh thu Gộp</span>
              <p className="text-2xl font-extrabold">{totalGrossRevenue.toLocaleString('vi-VN')} VNĐ</p>
            </div>
            <div className="p-5 bg-surface border rounded-xl shadow-sm space-y-1">
              <span className="text-xs text-muted-foreground font-medium">Tổng Lượt Đăng ký Đã thanh toán</span>
              <p className="text-2xl font-bold text-slate-900 dark:text-slate-100">
                {revenueStats.reduce((sum, item) => sum + item.paidEnrollments, 0)} Học viên
              </p>
            </div>
            <div className="p-5 bg-surface border rounded-xl shadow-sm space-y-1">
              <span className="text-xs text-muted-foreground font-medium">Doanh thu Trung bình / Khóa</span>
              <p className="text-2xl font-bold text-slate-900 dark:text-slate-100">
                {Math.round(totalGrossRevenue / revenueStats.length).toLocaleString('vi-VN')} VNĐ
              </p>
            </div>
          </div>

          <div className="bg-surface shadow rounded-lg overflow-hidden border border-border">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-muted-foreground dark:text-slate-300">
                <tr>
                  <th className="p-3">Tên Khóa học</th>
                  <th className="p-3">Đơn giá khóa</th>
                  <th className="p-3">Tổng lượt ĐK</th>
                  <th className="p-3">ĐK Đã thanh toán</th>
                  <th className="p-3 text-right">Doanh thu Gộp</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {revenueStats.map((item) => {
                  const rev = calculateCourseRevenue(item.price, item.paidEnrollments);
                  return (
                    <tr key={item.courseId} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                      <td className="p-3 font-semibold">{item.courseName}</td>
                      <td className="p-3 text-xs font-mono">{item.price.toLocaleString('vi-VN')} VNĐ</td>
                      <td className="p-3 text-xs">{item.totalEnrollments}</td>
                      <td className="p-3 text-xs font-bold text-emerald-600">{item.paidEnrollments}</td>
                      <td className="p-3 text-right font-bold font-mono text-brand">{rev.formattedVnd}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 4: ACCOUNT SHARING GUARD */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'guard' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Giám sát & Chống chia sẻ Tài khoản Học viên</h2>
          </div>

          <form onSubmit={handleValidateSession} className="bg-surface border p-6 rounded-xl space-y-4 shadow-sm">
            <h3 className="text-sm font-bold">Mô phỏng Đăng nhập mới từ Thiết bị / IP</h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-xs font-semibold">Device ID</label>
                <input
                  type="text"
                  value={testSession.deviceId}
                  onChange={(e) => setTestSession({ ...testSession, deviceId: e.target.value })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm font-mono dark:bg-slate-800"
                  required
                />
              </div>
              <div>
                <label className="text-xs font-semibold">IP Address (Thử nhập IP 192.168.99.x để test cảnh báo)</label>
                <input
                  type="text"
                  value={testSession.ipAddress}
                  onChange={(e) => setTestSession({ ...testSession, ipAddress: e.target.value })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm font-mono dark:bg-slate-800"
                  required
                />
              </div>
            </div>
            <div className="flex justify-end">
              <button type="submit" className="px-4 py-2 bg-rose-600 text-white rounded-lg text-sm font-medium hover:bg-rose-700 transition">
                ⚡ Kiểm tra Phiên Đăng nhập
              </button>
            </div>
          </form>

          <div className="space-y-3">
            <h3 className="text-sm font-bold">Phiên làm việc Đang hoạt động</h3>
            {activeSessions.map((s, idx) => (
              <div key={idx} className="p-4 bg-emerald-50/50 border border-emerald-200 rounded-xl flex justify-between items-center">
                <div>
                  <h4 className="font-bold text-sm font-mono">{s.deviceId}</h4>
                  <p className="text-xs text-muted-foreground mt-0.5">IP: <span className="font-mono">{s.ipAddress}</span></p>
                </div>
                <span className="px-2.5 py-1 bg-emerald-100 text-emerald-800 rounded text-xs font-bold">
                  🟢 Đang hoạt động
                </span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
