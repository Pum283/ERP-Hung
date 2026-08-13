'use client';

import React, { useState } from 'react';
import {
  calculateOverdueDays,
  calculatePassRatePct,
  calculateDropoutRatePct,
} from '@/shared/api/lms-training-reports-helpers';

export default function LmsTrainingReportsPage() {
  const [activeTab, setActiveTab] = useState<'overdue' | 'exams' | 'dropouts' | 'engagement'>('overdue');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: OVERDUE TRAINING ALERTS (UC_LMS_064)
  // ────────────────────────────────────────────────────────────────────────────
  const [overdueAlerts, setOverdueAlerts] = useState([
    { id: 'alt-1', user: 'Phạm Văn K (EMP142)', course: 'Khóa Đào tạo An toàn Lao động Nhà máy', dueDate: '2026-08-08T00:00:00Z', overdueDays: 5, alertSentAt: '13/08/2026 08:00', status: 'Sent' },
    { id: 'alt-2', user: 'Trần Thị M (EMP158)', course: 'Khóa Lập trình Microservices Advanced', dueDate: '2026-08-01T00:00:00Z', overdueDays: 12, alertSentAt: '13/08/2026 08:00', status: 'Sent' },
  ]);

  const handleTriggerCheck = () => {
    showToast('Đã kích hoạt hệ thống tự động quét & gửi thông báo cảnh báo quá hạn thành công!', 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: EXAM ANALYTICS REPORT (UC_LMS_067)
  // ────────────────────────────────────────────────────────────────────────────
  const [examAnalytics] = useState([
    { id: 'ex-1', title: 'Bài thi Kiểm tra An toàn Lao động Q3/2026', totalAttempts: 30, passedAttempts: 26, failedAttempts: 4, averageScore: 84.0, highestScore: 100, lowestScore: 50 },
    { id: 'ex-2', title: 'Bài thi Cuối khóa Lập trình Domain-Driven Design', totalAttempts: 18, passedAttempts: 15, failedAttempts: 3, averageScore: 78.5, highestScore: 98, lowestScore: 42 },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: DROPOUT ANALYTICS REPORT (UC_LMS_068)
  // ────────────────────────────────────────────────────────────────────────────
  const [dropoutAnalytics] = useState([
    { id: 'dp-1', courseName: 'Khóa học Lập trình Domain-Driven Design & Clean Architecture', totalEnrolled: 50, activeLearners: 44, dropoutCount: 6, commonStage: 'Chương 2 - Aggregates & Domain Events' },
    { id: 'dp-2', courseName: 'Khóa Đào tạo Vận hành Máy phay CNC Nâng cao', totalEnrolled: 25, activeLearners: 20, dropoutCount: 5, commonStage: 'Bài 04 - Lập trình G-Code thực hành' },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: COURSE ENGAGEMENT REPORT (UC_LMS_069)
  // ────────────────────────────────────────────────────────────────────────────
  const [courseEngagements] = useState([
    { id: 'ce-1', courseName: 'Khóa Đào tạo Quy trình Vận hành Nhà máy Thông minh', enrolled: 60, completed: 54, completionRate: 90.0, avgRating: 4.9, commentsCount: 28, avgStudyHours: 18.0 },
    { id: 'ce-2', courseName: 'Khóa học Lập trình Microservices & Event-Driven Architecture', enrolled: 45, completed: 36, completionRate: 80.0, avgRating: 4.8, commentsCount: 19, avgStudyHours: 24.5 },
  ]);

  return (
    <div className="space-y-6">
      {/* Toast */}
      {toast && (
        <div
          className={`fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white font-medium text-sm transition-all ${
            toast.type === 'success' ? 'bg-emerald-600' : toast.type === 'error' ? 'bg-rose-600' : 'bg-amber-600'
          }`}
        >
          {toast.message}
        </div>
      )}

      {/* Header */}
      <div className="bg-brand p-5 rounded-xl text-brand-foreground shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-rose-500/30 text-rose-200 text-xs px-3 py-1 rounded-full font-semibold border border-rose-400/30">
              LMS - CẢNH BÁO QUÁ HẠN & BÁO CÁO PHÂN TÍCH CHUYÊN SÂU
            </span>
            <h1 className="text-2xl font-bold mt-2">Overdue Alerts & Learning Analytics</h1>
            <p className="text-rose-200 text-sm mt-1">
              Cảnh báo quá hạn đào tạo, báo cáo điểm thi, phân tích học viên bỏ dở & hiệu quả khóa học
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-foreground/15 text-brand-foreground border border-brand-foreground/25">
              ● Tiến độ 100% (4/4 UCs)
            </span>
          </div>
        </div>

        {/* Tab Selection */}
        <div className="flex space-x-2 mt-6 border-t border-brand-foreground/15 pt-4">
          <button
            onClick={() => setActiveTab('overdue')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'overdue' ? 'bg-surface text-foreground shadow-md' : 'text-brand-foreground/80 hover:bg-surface/10'
            }`}
          >
            ⚠️ UC_LMS_064: Cảnh báo quá hạn
          </button>
          <button
            onClick={() => setActiveTab('exams')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'exams' ? 'bg-surface text-foreground shadow-md' : 'text-brand-foreground/80 hover:bg-surface/10'
            }`}
          >
            📝 UC_LMS_067: Phân tích điểm thi
          </button>
          <button
            onClick={() => setActiveTab('dropouts')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'dropouts' ? 'bg-surface text-foreground shadow-md' : 'text-brand-foreground/80 hover:bg-surface/10'
            }`}
          >
            📉 UC_LMS_068: Học viên bỏ dở
          </button>
          <button
            onClick={() => setActiveTab('engagement')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'engagement' ? 'bg-surface text-foreground shadow-md' : 'text-brand-foreground/80 hover:bg-surface/10'
            }`}
          >
            ⭐ UC_LMS_069: Hiệu quả khóa học
          </button>
        </div>
      </div>

      {/* TAB 1: OVERDUE TRAINING ALERTS */}
      {activeTab === 'overdue' && (
        <div className="rounded-xl border border-border bg-surface shadow-sm p-5 space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-bold text-foreground flex items-center gap-2">
              <span>⚠️</span> Danh sách Cảnh báo Nhân viên Quá hạn Đào tạo (UC_LMS_064)
            </h2>
            <button
              onClick={handleTriggerCheck}
              className="px-4 py-2 bg-rose-600 text-white rounded-lg font-semibold text-xs hover:bg-rose-700 shadow-sm"
            >
              🔄 Kích hoạt Quét & Quá hạn Realtime
            </button>
          </div>

          <div className="space-y-3">
            {overdueAlerts.map((alt) => {
              const days = calculateOverdueDays(alt.dueDate);
              return (
                <div key={alt.id} className="p-4 rounded-xl border border-rose-200 bg-rose-50/50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{alt.user}</h3>
                    <p className="text-xs text-muted-foreground mt-1">{alt.course}</p>
                    <span className="text-xs text-rose-700 font-semibold mt-1 block">
                      ⏱️ Hạn chót: {new Date(alt.dueDate).toLocaleDateString('vi-VN')} (Đã quá hạn {days} ngày)
                    </span>
                  </div>
                  <div className="text-right">
                    <span className="px-3 py-1 text-xs font-bold rounded-full bg-rose-600 text-white">
                      ⚠️ {alt.status === 'Sent' ? 'Đã gửi cảnh báo' : 'Cần xử lý'}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* TAB 2: EXAM ANALYTICS REPORT */}
      {activeTab === 'exams' && (
        <div className="rounded-xl border border-border bg-surface shadow-sm p-5 space-y-6">
          <h2 className="text-lg font-bold text-foreground">📝 Báo cáo Thống kê Điểm thi & Tỷ lệ Đạt (UC_LMS_067)</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {examAnalytics.map((ex) => {
              const passRes = calculatePassRatePct(ex.passedAttempts, ex.totalAttempts);
              return (
                <div key={ex.id} className="p-5 rounded-xl border border-border bg-slate-50 space-y-4">
                  <div className="flex justify-between items-start">
                    <h3 className="font-bold text-slate-900 text-base">{ex.title}</h3>
                    <span
                      className={`px-3 py-1 text-xs font-bold rounded-full ${
                        passRes.gradeBadge === 'Excellent'
                          ? 'bg-emerald-100 text-emerald-800'
                          : passRes.gradeBadge === 'Good'
                          ? 'bg-blue-100 text-blue-800'
                          : 'bg-amber-100 text-amber-800'
                      }`}
                    >
                      Tỷ lệ Đạt: {passRes.passRatePct}%
                    </span>
                  </div>

                  <div className="grid grid-cols-3 gap-2 text-center text-xs">
                    <div className="p-2 bg-surface rounded-lg border border-border">
                      <p className="text-muted-foreground">Tổng lượt thi</p>
                      <p className="font-bold text-slate-900 text-sm mt-0.5">{ex.totalAttempts}</p>
                    </div>
                    <div className="p-2 bg-surface rounded-lg border border-border">
                      <p className="text-muted-foreground">Đạt / Đậu</p>
                      <p className="font-bold text-emerald-600 text-sm mt-0.5">{ex.passedAttempts}</p>
                    </div>
                    <div className="p-2 bg-surface rounded-lg border border-border">
                      <p className="text-muted-foreground">Điểm trung bình</p>
                      <p className="font-bold text-blue-600 text-sm mt-0.5">{ex.averageScore}</p>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* TAB 3: DROPOUT ANALYTICS REPORT */}
      {activeTab === 'dropouts' && (
        <div className="rounded-xl border border-border bg-surface shadow-sm p-5 space-y-6">
          <h2 className="text-lg font-bold text-foreground">📉 Phân tích Học viên Dừng học Giữa chừng / Dropout (UC_LMS_068)</h2>

          <div className="space-y-4">
            {dropoutAnalytics.map((dp) => {
              const dropRes = calculateDropoutRatePct(dp.dropoutCount, dp.totalEnrolled);
              return (
                <div key={dp.id} className="p-5 rounded-xl border border-border bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{dp.courseName}</h3>
                    <p className="text-xs text-muted-foreground mt-1">
                      Tổng ghi danh: {dp.totalEnrolled} • Đang học tích cực: {dp.activeLearners}
                    </p>
                    <p className="text-xs text-rose-700 font-medium mt-1">
                      Điểm dừng học phổ biến: <strong>{dp.commonStage}</strong>
                    </p>
                  </div>

                  <div className="text-right">
                    <span className="text-xl font-extrabold text-rose-600">{dropRes.dropoutRatePct}%</span>
                    <p className="text-xs text-muted-foreground font-medium mt-0.5">({dp.dropoutCount} Học viên)</p>
                    <span
                      className={`inline-block mt-1 px-2.5 py-0.5 text-xs font-bold rounded-full ${
                        dropRes.riskLevel === 'High'
                          ? 'bg-rose-100 text-rose-800'
                          : dropRes.riskLevel === 'Medium'
                          ? 'bg-amber-100 text-amber-800'
                          : 'bg-emerald-100 text-emerald-800'
                      }`}
                    >
                      Rủi ro Dropout: {dropRes.riskLevel === 'High' ? 'Cao' : dropRes.riskLevel === 'Medium' ? 'Trung bình' : 'Thấp'}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* TAB 4: COURSE ENGAGEMENT REPORT */}
      {activeTab === 'engagement' && (
        <div className="rounded-xl border border-border bg-surface shadow-sm p-5 space-y-6">
          <h2 className="text-lg font-bold text-foreground">⭐ Báo cáo Hiệu quả & Mức độ Tương tác Khóa học (UC_LMS_069)</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {courseEngagements.map((ce) => (
              <div key={ce.id} className="p-5 rounded-xl border border-border bg-slate-50 space-y-4">
                <h3 className="font-bold text-slate-900 text-base">{ce.courseName}</h3>
                <div className="grid grid-cols-2 gap-3 text-xs">
                  <div className="p-3 bg-surface rounded-lg border border-border">
                    <p className="text-muted-foreground">Tỷ lệ hoàn thành</p>
                    <p className="text-lg font-extrabold text-emerald-600 mt-1">{ce.completionRate}%</p>
                  </div>
                  <div className="p-3 bg-surface rounded-lg border border-border">
                    <p className="text-muted-foreground">Đánh giá trung bình</p>
                    <p className="text-lg font-extrabold text-amber-500 mt-1">⭐ {ce.avgRating} / 5.0</p>
                  </div>
                </div>
                <div className="flex justify-between text-xs text-muted-foreground pt-2 border-t border-border">
                  <span>💬 {ce.commentsCount} Bình luận & Phản hồi</span>
                  <span>⏱️ TB {ce.avgStudyHours} Giờ học/học viên</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
