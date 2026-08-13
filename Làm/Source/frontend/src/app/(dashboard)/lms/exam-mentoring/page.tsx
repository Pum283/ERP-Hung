'use client';

import React, { useState } from 'react';
import {
  checkAntiCheatViolation,
  calculateMentoringProgress,
  validateRatingScore,
  summarizeMentoringEffectiveness,
  ChecklistItem,
} from '@/shared/api/lms-exam-mentoring-helpers';

export default function LmsExamMentoringPage() {
  const [activeTab, setActiveTab] = useState<'anticheat' | 'checklist' | 'evaluation' | 'report'>('anticheat');

  // Toast notifications
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: ANTI-CHEAT EXAM SESSION (UC_LMS_015)
  // ────────────────────────────────────────────────────────────────────────────
  const [examState, setExamState] = useState({
    timeRemaining: 2700, // 45 phút
    focusLoss: 0,
    tabSwitch: 0,
    isSubmitted: false,
  });

  const handleSimulateViolation = (type: 'tab' | 'focus' | 'expire') => {
    if (examState.isSubmitted) return;

    let newTab = examState.tabSwitch;
    let newFocus = examState.focusLoss;
    let newTime = examState.timeRemaining;

    if (type === 'tab') newTab++;
    if (type === 'focus') newFocus++;
    if (type === 'expire') newTime = 0;

    const antiCheat = checkAntiCheatViolation(newFocus, newTab, newTime);

    setExamState({
      timeRemaining: newTime,
      focusLoss: newFocus,
      tabSwitch: newTab,
      isSubmitted: antiCheat.shouldForceSubmit,
    });

    if (antiCheat.shouldForceSubmit) {
      showToast(`HỆ THỐNG TỰ ĐỘNG NỘP BÀI: ${antiCheat.reason}`, 'error');
    } else if (antiCheat.isViolated) {
      showToast(antiCheat.reason || 'Cảnh báo vi phạm quy chế thi!', 'warning');
    }
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: MENTORING CHECKLIST (UC_LMS_024)
  // ────────────────────────────────────────────────────────────────────────────
  const [checklists, setChecklists] = useState<ChecklistItem[]>([
    { id: 't-1', taskName: 'Khởi tạo môi trường Dev & kết nối Git repo', isCompleted: true },
    { id: 't-2', taskName: 'Đọc & nắm vững quy chuẩn Clean Architecture C#', isCompleted: true },
    { id: 't-3', taskName: 'Thực hành viết 12 Unit Tests xUnit InMemory', isCompleted: false },
    { id: 't-4', taskName: 'Hoàn thành bài tập Review Code cùng Mentor', isCompleted: false },
  ]);
  const [newTaskName, setNewTaskName] = useState('');
  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);

  const progress = calculateMentoringProgress(checklists);

  const handleToggleTask = (id: string) => {
    setChecklists((prev) =>
      prev.map((t) => (t.id === id ? { ...t, isCompleted: !t.isCompleted } : t))
    );
    showToast('Cập nhật trạng thái công việc kèm cặp thành công!');
  };

  const handleAddTask = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTaskName.trim()) {
      showToast('Tên công việc không được để trống.', 'error');
      return;
    }
    setChecklists((prev) => [
      ...prev,
      { id: `t-${Date.now()}`, taskName: newTaskName.trim(), isCompleted: false },
    ]);
    setNewTaskName('');
    setIsTaskModalOpen(false);
    showToast('Thêm mới mục checklist thành công!');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: MENTORING EVALUATION (UC_LMS_026)
  // ────────────────────────────────────────────────────────────────────────────
  const [evaluations, setEvaluations] = useState([
    { id: 'e-1', type: 'MentorToMentee', rating: 5, feedback: 'Học viên chủ động, tiếp thu cực nhanh và hoàn thành đúng deadline.' },
    { id: 'e-2', type: 'MenteeToMentor', rating: 5, feedback: 'Mentor hướng dẫn vô cùng tận tình, chỉ rõ tư duy tối ưu code.' },
  ]);
  const [evalForm, setEvalForm] = useState({ type: 'MentorToMentee', rating: 5, feedback: '' });
  const [isEvalModalOpen, setIsEvalModalOpen] = useState(false);

  const handleSaveEvaluation = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateRatingScore(evalForm.rating);
    setEvaluations((prev) => [
      {
        id: `e-${Date.now()}`,
        type: evalForm.type,
        rating: val.normalizedRating,
        feedback: evalForm.feedback.trim() || 'Đã gửi đánh giá thành công.',
      },
      ...prev,
    ]);
    showToast('Lưu đánh giá kèm cặp thành công!');
    setIsEvalModalOpen(false);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: MENTORING EFFECTIVENESS REPORT (UC_LMS_027)
  // ────────────────────────────────────────────────────────────────────────────
  const mentorRatings = evaluations.filter((e) => e.type === 'MentorToMentee').map((e) => e.rating);
  const menteeRatings = evaluations.filter((e) => e.type === 'MenteeToMentor').map((e) => e.rating);
  const report = summarizeMentoringEffectiveness(5, progress.completed, progress.total, mentorRatings, menteeRatings);

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Thời gian làm bài & Chống gian lận, Checklist kèm cặp, Đánh giá Mentoring LMS
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Thời gian & chống gian lận (UC_LMS_015), Checklist kèm cặp (UC_LMS_024), Đánh giá mentor (UC_LMS_026) & Báo cáo hiệu quả (UC_LMS_027).
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
            toast.type === 'success'
              ? 'bg-emerald-500 text-white'
              : toast.type === 'warning'
              ? 'bg-amber-500 text-white'
              : 'bg-rose-500 text-white'
          }`}
        >
          {toast.message}
        </div>
      )}

      {/* Navigation Tabs */}
      <div className="flex border-b border-border gap-6">
        {[
          { key: 'anticheat', label: '1. Thời gian & Chống gian lận (UC_LMS_015)' },
          { key: 'checklist', label: '2. Checklist Kèm cặp Mentoring (UC_LMS_024)' },
          { key: 'evaluation', label: '3. Đánh giá Mentor / Học viên (UC_LMS_026)' },
          { key: 'report', label: '4. Báo cáo Hiệu quả Mentoring (UC_LMS_027)' },
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
      {/* TAB 1: ANTI-CHEAT EXAM SESSION */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'anticheat' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Màn hình Làm bài Thi & Giám sát Chống Gian lận Realtime</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* Status Card */}
            <div className="p-6 bg-slate-900 text-white rounded-xl shadow-md space-y-4">
              <h3 className="text-xs uppercase tracking-wider text-slate-400 font-bold">Trạng thái Phiên làm bài</h3>
              <div className="text-3xl font-mono font-bold text-amber-400">
                ⏱ {Math.floor(examState.timeRemaining / 60)}:
                {String(examState.timeRemaining % 60).padStart(2, '0')}
              </div>
              <div className="space-y-1 text-xs text-slate-300">
                <p>• Số lần Rời màn hình: <span className="font-bold text-rose-400">{examState.focusLoss}</span> / 5</p>
                <p>• Số lần Chuyển Tab: <span className="font-bold text-rose-400">{examState.tabSwitch}</span> / 3</p>
                <p>• Trạng thái nộp: {examState.isSubmitted ? <span className="text-emerald-400 font-bold">ĐÃ NỘP BÀI</span> : <span className="text-sky-400 font-bold">ĐANG LÀM BÀI</span>}</p>
              </div>
            </div>

            {/* Simulation Controls */}
            <div className="p-6 bg-surface border rounded-xl shadow-sm space-y-4 md:col-span-2">
              <h3 className="text-sm font-bold text-foreground dark:text-brand-foreground/80">Giả lập Hành vi Gian lận (Anti-cheat Trigger)</h3>
              <p className="text-xs text-muted-foreground">
                Hệ thống tự động ghi nhận khi học viên chuyển tab browser, mất focus hoặc hết thời gian và tự động nộp bài khi quá ngưỡng.
              </p>
              <div className="flex flex-wrap gap-3 pt-2">
                <button
                  onClick={() => handleSimulateViolation('tab')}
                  disabled={examState.isSubmitted}
                  className="px-4 py-2 bg-amber-600 hover:bg-amber-700 text-white text-xs font-semibold rounded-lg disabled:opacity-50 transition"
                >
                  ⚠️ Giả lập Chuyển Tab (+1)
                </button>
                <button
                  onClick={() => handleSimulateViolation('focus')}
                  disabled={examState.isSubmitted}
                  className="px-4 py-2 bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold rounded-lg disabled:opacity-50 transition"
                >
                  🚨 Giả lập Rời Màn Hình (+1)
                </button>
                <button
                  onClick={() => handleSimulateViolation('expire')}
                  disabled={examState.isSubmitted}
                  className="px-4 py-2 bg-slate-800 hover:bg-slate-900 text-white text-xs font-semibold rounded-lg disabled:opacity-50 transition"
                >
                  ⏳ Giả lập Hết Thời Gian
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 2: MENTORING CHECKLIST */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'checklist' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-semibold">Checklist Tiến độ Kèm cặp Mentoring</h2>
              <p className="text-xs text-muted-foreground mt-0.5">Tiến độ hoàn thành: {progress.completed}/{progress.total} mục ({progress.percentage}%)</p>
            </div>
            <button
              onClick={() => setIsTaskModalOpen(true)}
              className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium hover:bg-brand-hover transition"
            >
              + Thêm mục Checklist
            </button>
          </div>

          {/* Progress Bar */}
          <div className="w-full bg-slate-200 dark:bg-slate-800 rounded-full h-2.5 overflow-hidden">
            <div className="bg-emerald-500 h-2.5 transition-all duration-500" style={{ width: `${progress.percentage}%` }}></div>
          </div>

          <div className="bg-surface border rounded-xl divide-y divide-slate-200 dark:divide-slate-800 shadow-sm">
            {checklists.map((t) => (
              <div key={t.id} className="p-4 flex items-center justify-between hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                <div className="flex items-center gap-3">
                  <input
                    type="checkbox"
                    checked={t.isCompleted}
                    onChange={() => handleToggleTask(t.id)}
                    className="w-4 h-4 rounded text-brand focus:ring-brand cursor-pointer"
                  />
                  <span className={`text-sm ${t.isCompleted ? 'line-through text-slate-400' : 'font-medium text-foreground dark:text-brand-foreground/80'}`}>
                    {t.taskName}
                  </span>
                </div>
                <div>
                  <span className={`px-2.5 py-0.5 text-xs rounded font-semibold ${t.isCompleted ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'}`}>
                    {t.isCompleted ? 'Hoàn thành' : 'Đang làm'}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 3: MENTORING EVALUATION */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'evaluation' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Đánh giá 2 chiều giữa Mentor và Học viên</h2>
            <button
              onClick={() => setIsEvalModalOpen(true)}
              className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium hover:bg-brand-hover transition"
            >
              + Gửi Đánh giá mới
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {evaluations.map((e) => (
              <div key={e.id} className="p-5 bg-surface border rounded-xl shadow-sm space-y-2">
                <div className="flex justify-between items-center">
                  <span className="px-2.5 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">
                    {e.type === 'MentorToMentee' ? 'Mentor đánh giá Học viên' : 'Học viên đánh giá Mentor'}
                  </span>
                  <span className="text-amber-500 font-bold text-sm">{'⭐'.repeat(e.rating)} ({e.rating}/5)</span>
                </div>
                <p className="text-sm text-foreground dark:text-slate-300 italic">"{e.feedback}"</p>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 4: MENTORING EFFECTIVENESS REPORT */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'report' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Báo cáo Tổng hợp Hiệu quả Mentoring & Đồng hành</h2>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div className="p-4 bg-brand-muted border border-brand/30 rounded-xl shadow-sm">
              <p className="text-xs text-brand-strong font-bold">Tổng số cặp Mentoring</p>
              <p className="text-2xl font-bold mt-1 text-brand-strong">{report.completionRatePct > 0 ? 5 : 0} cặp</p>
            </div>
            <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-xl shadow-sm">
              <p className="text-xs text-emerald-700 font-bold">Tỷ lệ hoàn thành Checklist</p>
              <p className="text-2xl font-bold mt-1 text-emerald-900">{report.completionRatePct}%</p>
            </div>
            <div className="p-4 bg-amber-50 border border-amber-200 rounded-xl shadow-sm">
              <p className="text-xs text-amber-700 font-bold">Điểm Mentor đánh giá TB</p>
              <p className="text-2xl font-bold mt-1 text-amber-900">⭐ {report.avgMentorRating} / 5</p>
            </div>
            <div className="p-4 bg-brand-muted border border-brand/30 rounded-xl shadow-sm">
              <p className="text-xs text-brand-strong font-bold">Điểm Học viên đánh giá TB</p>
              <p className="text-2xl font-bold mt-1 text-brand-strong">⭐ {report.avgMenteeRating} / 5</p>
            </div>
          </div>
        </div>
      )}

      {/* CHECKLIST MODAL */}
      {isTaskModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleAddTask} className="bg-surface rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">Thêm mục Checklist Kèm cặp</h3>
            <div>
              <label className="text-xs font-semibold">Tên công việc cần hoàn thành</label>
              <input
                type="text"
                value={newTaskName}
                onChange={(e) => setNewTaskName(e.target.value)}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsTaskModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium">
                Thêm công việc
              </button>
            </div>
          </form>
        </div>
      )}

      {/* EVALUATION MODAL */}
      {isEvalModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveEvaluation} className="bg-surface rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">Gửi Đánh giá Mentoring</h3>
            <div>
              <label className="text-xs font-semibold">Chiều đánh giá</label>
              <select
                value={evalForm.type}
                onChange={(e) => setEvalForm({ ...evalForm, type: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              >
                <option value="MentorToMentee">Mentor đánh giá Học viên</option>
                <option value="MenteeToMentor">Học viên đánh giá Mentor</option>
              </select>
            </div>
            <div>
              <label className="text-xs font-semibold">Điểm đánh giá (1 - 5 sao)</label>
              <select
                value={evalForm.rating}
                onChange={(e) => setEvalForm({ ...evalForm, rating: Number(e.target.value) })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              >
                <option value={5}>⭐⭐⭐⭐⭐ (5 - Xuất sắc)</option>
                <option value={4}>⭐⭐⭐⭐ (4 - Tốt)</option>
                <option value={3}>⭐⭐⭐ (3 - Khá)</option>
                <option value={2}>⭐⭐ (2 - Trung bình)</option>
                <option value={1}>⭐ (1 - Cần cải thiện)</option>
              </select>
            </div>
            <div>
              <label className="text-xs font-semibold">Ý kiến nhận xét & phản hồi</label>
              <textarea
                rows={3}
                value={evalForm.feedback}
                onChange={(e) => setEvalForm({ ...evalForm, feedback: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsEvalModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium">
                Gửi đánh giá
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
