'use client';

import React, { useState } from 'react';
import {
  validateStudyReminder,
  formatForumTopicPreview,
  parseCertificateCode,
  evaluateCertificateStatus,
} from '@/shared/api/lms-engage-cert-helpers';

export default function LmsEngageCertPage() {
  const [activeTab, setActiveTab] = useState<'reminder' | 'forum' | 'verify' | 'revoke'>('reminder');

  // Toast notifications
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: STUDY REMINDER (UC_LMS_038)
  // ────────────────────────────────────────────────────────────────────────────
  const [reminders, setReminders] = useState([
    { id: 'rem-1', courseName: 'Khóa học Lập trình Microservices & Event-Driven', frequency: 'Daily', message: 'Bạn còn 3 bài học chưa hoàn thành, hãy dành 15 phút mỗi ngày nhé!', date: 'Hôm nay' },
  ]);
  const [reminderForm, setReminderForm] = useState({ frequency: 'Daily', message: '' });

  const handleSaveReminder = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateStudyReminder(reminderForm.frequency, reminderForm.message || 'Bạn còn bài học chưa hoàn thành, hãy học tiếp!');
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ', 'error');
      return;
    }

    setReminders((prev) => [
      {
        id: `rem-${Date.now()}`,
        courseName: 'Khóa học Lập trình Microservices & Event-Driven',
        frequency: val.normalizedFreq,
        message: reminderForm.message || 'Bạn còn bài học chưa hoàn thành, hãy học tiếp!',
        date: new Date().toLocaleDateString('vi-VN'),
      },
      ...prev,
    ]);
    showToast('Tạo lịch nhắc học tiếp thành công!');
    setReminderForm({ frequency: 'Daily', message: '' });
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: FORUM TOPICS (UC_LMS_039)
  // ────────────────────────────────────────────────────────────────────────────
  const [topics, setTopics] = useState([
    { id: 'top-1', title: 'Hỏi đáp về Saga Pattern trong Microservices', author: 'Nguyễn Văn A', content: 'Có nên dùng Orchestration Saga cho hệ thống thanh toán thương mại điện tử không mọi người?', replies: 5, isPinned: true },
    { id: 'top-2', title: 'Thảo luận về Event Sourcing & Kafka', author: 'Trần Thị B', content: 'Làm sao để đảm bảo Idempotency khi consume message từ Kafka Topic?', replies: 3, isPinned: false },
  ]);
  const [topicForm, setTopicForm] = useState({ title: '', content: '', isPinned: false });
  const [isTopicModalOpen, setIsTopicModalOpen] = useState(false);

  const handleCreateTopic = (e: React.FormEvent) => {
    e.preventDefault();
    if (!topicForm.title.trim() || !topicForm.content.trim()) {
      showToast('Tiêu đề và nội dung không được để trống.', 'error');
      return;
    }
    setTopics((prev) => [
      {
        id: `top-${Date.now()}`,
        title: topicForm.title.trim(),
        author: 'Học viên ERP (Bạn)',
        content: topicForm.content.trim(),
        replies: 0,
        isPinned: topicForm.isPinned,
      },
      ...prev,
    ]);
    showToast('Đăng chủ đề thảo luận thành công!');
    setIsTopicModalOpen(false);
    setTopicForm({ title: '', content: '', isPinned: false });
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: CERTIFICATE VERIFICATION (UC_LMS_046)
  // ────────────────────────────────────────────────────────────────────────────
  const [searchCode, setSearchCode] = useState('CERT-2026-X999');
  const [verifyResult, setVerifyResult] = useState<{
    code: string;
    status: string;
    learner: string;
    course: string;
    date: string;
    score: number;
  } | null>({
    code: 'CERT-2026-X999',
    status: 'Active',
    learner: 'Vũ Thị I (EMP160)',
    course: 'Khóa học Lập trình Microservices & Event-Driven',
    date: '12/08/2026',
    score: 95,
  });

  const handleVerify = (e: React.FormEvent) => {
    e.preventDefault();
    const parsed = parseCertificateCode(searchCode);
    if (!parsed.isValid) {
      showToast('Mã chứng chỉ phải có dạng CERT-XXXX-XXXX (VD: CERT-2026-X999)', 'error');
      return;
    }

    if (parsed.normalized === 'CERT-2026-X999') {
      setVerifyResult({
        code: 'CERT-2026-X999',
        status: 'Active',
        learner: 'Vũ Thị I (EMP160)',
        course: 'Khóa học Lập trình Microservices & Event-Driven',
        date: '12/08/2026',
        score: 95,
      });
      showToast('Xác thực chứng chỉ thành công: CHỨNG CHỈ HỢP LỆ!');
    } else {
      setVerifyResult(null);
      showToast('Mã chứng chỉ không tồn tại trong hệ thống!', 'error');
    }
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: CERTIFICATE REVOCATION (UC_LMS_047)
  // ────────────────────────────────────────────────────────────────────────────
  const [revocations, setRevocations] = useState([
    { id: 'rev-1', certCode: 'CERT-2025-OLD1', reason: 'Phát hiện gian lận thi cử qua kiểm tra camera', date: '01/05/2026', by: 'Ban Quản trị Đào tạo' },
  ]);
  const [revokeForm, setRevokeForm] = useState({ certCode: '', reason: '' });
  const [isRevokeModalOpen, setIsRevokeModalOpen] = useState(false);

  const handleRevoke = (e: React.FormEvent) => {
    e.preventDefault();
    if (!revokeForm.certCode.trim() || !revokeForm.reason.trim()) {
      showToast('Mã chứng chỉ và lý do thu hồi không được để trống.', 'error');
      return;
    }

    setRevocations((prev) => [
      {
        id: `rev-${Date.now()}`,
        certCode: revokeForm.certCode.trim().toUpperCase(),
        reason: revokeForm.reason.trim(),
        date: new Date().toLocaleDateString('vi-VN'),
        by: 'Admin ERP',
      },
      ...prev,
    ]);

    if (verifyResult && verifyResult.code === revokeForm.certCode.trim().toUpperCase()) {
      setVerifyResult({ ...verifyResult, status: 'Revoked' });
    }

    showToast(`Đã thu hồi thành công chứng chỉ [${revokeForm.certCode.toUpperCase()}]!`);
    setIsRevokeModalOpen(false);
    setRevokeForm({ certCode: '', reason: '' });
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Bước 162: Nhắc học tiếp, Diễn đàn thảo luận, Xác thực & Thu hồi Chứng chỉ LMS
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Nhắc học tiếp (UC_LMS_038), Diễn đàn (UC_LMS_039), Xác thực chứng chỉ (UC_LMS_046) & Thu hồi chứng chỉ (UC_LMS_047).
          </p>
        </div>
        <div>
          <span className="px-3 py-1 bg-indigo-100 text-indigo-800 dark:bg-indigo-950 dark:text-indigo-300 rounded-full text-xs font-semibold">
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
      <div className="flex border-b border-slate-200 dark:border-slate-800 gap-6">
        {[
          { key: 'reminder', label: '1. Nhắc học tiếp (UC_LMS_038)' },
          { key: 'forum', label: '2. Diễn đàn thảo luận (UC_LMS_039)' },
          { key: 'verify', label: '3. Xác thực Chứng chỉ (UC_LMS_046)' },
          { key: 'revoke', label: '4. Thu hồi Chứng chỉ (UC_LMS_047)' },
        ].map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key as any)}
            className={`pb-3 text-sm font-semibold border-b-2 transition-colors ${
              activeTab === tab.key
                ? 'border-indigo-600 text-indigo-600 dark:text-indigo-400'
                : 'border-transparent text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 1: STUDY REMINDER */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'reminder' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Cấu hình Lịch Nhắc học tiếp cho Học viên</h2>
          </div>

          <form onSubmit={handleSaveReminder} className="bg-white dark:bg-slate-900 border p-6 rounded-xl space-y-4 shadow-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-xs font-semibold">Tần suất nhắc nhở</label>
                <select
                  value={reminderForm.frequency}
                  onChange={(e) => setReminderForm({ ...reminderForm, frequency: e.target.value })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                >
                  <option value="Daily">Hàng ngày (Daily)</option>
                  <option value="Weekly">Hàng tuần (Weekly)</option>
                  <option value="Custom">Tùy chỉnh (Custom)</option>
                </select>
              </div>
              <div>
                <label className="text-xs font-semibold">Nội dung tin nhắn nhắc nhở</label>
                <input
                  type="text"
                  placeholder="Bạn còn bài học chưa hoàn thành, hãy học tiếp!"
                  value={reminderForm.message}
                  onChange={(e) => setReminderForm({ ...reminderForm, message: e.target.value })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
            </div>
            <div className="flex justify-end">
              <button type="submit" className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition">
                + Thiết lập Nhắc học
              </button>
            </div>
          </form>

          <div className="space-y-3">
            <h3 className="text-sm font-bold">Danh sách Lịch Nhắc học đang kích hoạt</h3>
            {reminders.map((r) => (
              <div key={r.id} className="p-4 bg-indigo-50/50 dark:bg-slate-900 border border-indigo-100 dark:border-slate-800 rounded-xl flex justify-between items-center">
                <div>
                  <h4 className="font-semibold text-sm text-indigo-950 dark:text-indigo-200">{r.courseName}</h4>
                  <p className="text-xs text-slate-600 dark:text-slate-400 mt-0.5">"{r.message}"</p>
                </div>
                <div className="text-right">
                  <span className="px-2.5 py-0.5 text-xs rounded font-bold bg-indigo-100 text-indigo-800">
                    {r.frequency}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 2: FORUM TOPICS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'forum' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Diễn đàn Thảo luận & Hỏi đáp Khóa học LMS</h2>
            <button
              onClick={() => setIsTopicModalOpen(true)}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition"
            >
              + Tạo chủ đề mới
            </button>
          </div>

          <div className="space-y-4">
            {topics.map((t) => {
              const preview = formatForumTopicPreview(t.title, t.content);
              return (
                <div key={t.id} className="p-5 bg-white dark:bg-slate-900 border rounded-xl shadow-sm space-y-2">
                  <div className="flex justify-between items-center">
                    <div className="flex items-center gap-2">
                      {t.isPinned && <span className="px-2 py-0.5 bg-amber-100 text-amber-800 rounded text-xs font-bold">📌 Ghim</span>}
                      <h3 className="font-bold text-sm text-slate-900 dark:text-slate-100">{t.title}</h3>
                    </div>
                    <span className="text-xs text-slate-500 font-medium">💬 {t.replies} câu trả lời</span>
                  </div>
                  <p className="text-xs text-slate-600 dark:text-slate-400">{preview.preview}</p>
                  <p className="text-[11px] text-slate-400">Đăng bởi: <span className="font-semibold text-slate-600 dark:text-slate-300">{t.author}</span></p>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 3: CERTIFICATE VERIFICATION */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'verify' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Cổng Tra cứu & Xác thực Chứng chỉ Điện tử</h2>
          </div>

          <form onSubmit={handleVerify} className="flex gap-3 bg-white dark:bg-slate-900 border p-4 rounded-xl shadow-sm">
            <input
              type="text"
              placeholder="Nhập mã chứng chỉ (VD: CERT-2026-X999)"
              value={searchCode}
              onChange={(e) => setSearchCode(e.target.value)}
              className="flex-1 p-2.5 border rounded-lg text-sm font-mono dark:bg-slate-800"
              required
            />
            <button type="submit" className="px-5 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition">
              🔍 Xác thực ngay
            </button>
          </form>

          {verifyResult && (
            <div className="p-6 bg-slate-900 text-white rounded-xl shadow-md space-y-4 font-mono text-xs">
              <div className="flex justify-between items-center border-b border-slate-800 pb-3">
                <h3 className="text-sm font-bold text-emerald-400">📜 THÔNG TIN CHỨNG CHỈ ĐIỆN TỬ</h3>
                {(() => {
                  const evalRes = evaluateCertificateStatus(verifyResult.status);
                  return (
                    <span className={`px-3 py-1 rounded text-xs font-bold ${evalRes.isValid ? 'bg-emerald-500 text-white' : 'bg-rose-500 text-white'}`}>
                      {evalRes.label}
                    </span>
                  );
                })()}
              </div>
              <div className="grid grid-cols-2 gap-4 text-slate-300">
                <p>Mã xác thực: <span className="font-bold text-white">{verifyResult.code}</span></p>
                <p>Học viên nhận: <span className="font-bold text-white">{verifyResult.learner}</span></p>
                <p>Khóa học: <span className="font-bold text-white">{verifyResult.course}</span></p>
                <p>Điểm cấp chứng chỉ: <span className="font-bold text-amber-400">{verifyResult.score}/100</span></p>
                <p>Ngày cấp: <span className="font-bold text-white">{verifyResult.date}</span></p>
              </div>
            </div>
          )}
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 4: CERTIFICATE REVOCATION */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'revoke' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Thu hồi & Nhật ký Xử lý Vi phạm Chứng chỉ</h2>
            <button
              onClick={() => setIsRevokeModalOpen(true)}
              className="px-4 py-2 bg-rose-600 text-white rounded-lg text-sm font-medium hover:bg-rose-700 transition"
            >
              🚨 Thu hồi Chứng chỉ
            </button>
          </div>

          <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border border-slate-200 dark:border-slate-800">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-300">
                <tr>
                  <th className="p-3">Mã Chứng chỉ</th>
                  <th className="p-3">Lý do thu hồi</th>
                  <th className="p-3">Ngày thu hồi</th>
                  <th className="p-3">Người thực hiện</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {revocations.map((r) => (
                  <tr key={r.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-mono font-bold text-rose-600">{r.certCode}</td>
                    <td className="p-3 text-xs font-medium text-slate-700 dark:text-slate-300">{r.reason}</td>
                    <td className="p-3 text-xs text-slate-500">{r.date}</td>
                    <td className="p-3 text-xs font-semibold">{r.by}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* TOPIC MODAL */}
      {isTopicModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleCreateTopic} className="bg-white dark:bg-slate-900 rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">Tạo Chủ đề Thảo luận mới</h3>
            <div>
              <label className="text-xs font-semibold">Tiêu đề chủ đề</label>
              <input
                type="text"
                value={topicForm.title}
                onChange={(e) => setTopicForm({ ...topicForm, title: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Nội dung câu hỏi / trao đổi</label>
              <textarea
                rows={4}
                value={topicForm.content}
                onChange={(e) => setTopicForm({ ...topicForm, content: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="pinCheck"
                checked={topicForm.isPinned}
                onChange={(e) => setTopicForm({ ...topicForm, isPinned: e.target.checked })}
                className="w-4 h-4 text-indigo-600 rounded"
              />
              <label htmlFor="pinCheck" className="text-xs font-semibold">Ghim chủ đề lên đầu trang</label>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsTopicModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium">
                Đăng bài
              </button>
            </div>
          </form>
        </div>
      )}

      {/* REVOKE MODAL */}
      {isRevokeModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleRevoke} className="bg-white dark:bg-slate-900 rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold text-rose-600">Quyết định Thu hồi Chứng chỉ</h3>
            <div>
              <label className="text-xs font-semibold">Mã chứng chỉ bị thu hồi</label>
              <input
                type="text"
                placeholder="VD: CERT-2026-X999"
                value={revokeForm.certCode}
                onChange={(e) => setRevokeForm({ ...revokeForm, certCode: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm font-mono dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Lý do thu hồi chứng chỉ</label>
              <textarea
                rows={3}
                placeholder="Nhập lý do chi tiết..."
                value={revokeForm.reason}
                onChange={(e) => setRevokeForm({ ...revokeForm, reason: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsRevokeModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-rose-600 text-white rounded-lg text-sm font-medium">
                Xác nhận Thu hồi
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
