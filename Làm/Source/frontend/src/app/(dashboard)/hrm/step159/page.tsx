'use client';

import React, { useState } from 'react';
import {
  calculateFinalKpiGrade,
  validateTemplateWeights,
  validateSelfEvaluationScore,
  calculateCycleCompletionStats,
  ManagerEvaluationItem,
  KpiTemplateItem,
} from '@/shared/api/hrm-step159-helpers';

export default function HrmStep159Page() {
  const [activeTab, setActiveTab] = useState<'templates' | 'cycles' | 'evaluations' | 'selfEval'>('templates');

  // Toast notifications
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: KPI TEMPLATES (UC_HRM_177)
  // ────────────────────────────────────────────────────────────────────────────
  const [templates, setTemplates] = useState<KpiTemplateItem[]>([
    { id: 'tmpl-1', code: 'KPI_DEV_2026', title: 'Khung Đánh giá Kỹ sư Phần mềm', maxScore: 100, weightPercentage: 100 },
    { id: 'tmpl-2', code: 'KPI_SALES_2026', title: 'Khung Đánh giá Chuyên viên Kinh doanh', maxScore: 100, weightPercentage: 100 },
  ]);
  const [templateForm, setTemplateForm] = useState<{ id?: string; code: string; title: string; maxScore: number; weight: number }>({
    code: '',
    title: '',
    maxScore: 100,
    weight: 100,
  });
  const [isTemplateModalOpen, setIsTemplateModalOpen] = useState(false);

  const handleSaveTemplate = (e: React.FormEvent) => {
    e.preventDefault();
    if (!templateForm.code.trim() || !templateForm.title.trim()) {
      showToast('Mã và Tên mẫu KPI không được để trống', 'error');
      return;
    }
    const val = validateTemplateWeights(templateForm.maxScore, templateForm.weight);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu trọng số không hợp lệ', 'error');
      return;
    }

    if (templateForm.id) {
      setTemplates((prev) =>
        prev.map((t) =>
          t.id === templateForm.id
            ? { ...t, code: templateForm.code.toUpperCase(), title: templateForm.title, maxScore: templateForm.maxScore, weightPercentage: templateForm.weight }
            : t
        )
      );
      showToast('Cập nhật mẫu KPI thành công!');
    } else {
      setTemplates((prev) => [
        ...prev,
        {
          id: `tmpl-${Date.now()}`,
          code: templateForm.code.toUpperCase(),
          title: templateForm.title,
          maxScore: templateForm.maxScore,
          weightPercentage: templateForm.weight,
        },
      ]);
      showToast('Tạo mẫu KPI mới thành công!');
    }
    setIsTemplateModalOpen(false);
  };

  const handleDeleteTemplate = (id: string) => {
    setTemplates((prev) => prev.filter((t) => t.id !== id));
    showToast('Đã xóa mẫu KPI.');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: EVALUATION CYCLES (UC_HRM_178)
  // ────────────────────────────────────────────────────────────────────────────
  const [cycles, setCycles] = useState([
    { id: 'cyc-1', cycleName: 'Đánh giá Hiệu suất Quý 3/2026', periodKey: '2026-Q3', dates: '01/07/2026 - 30/09/2026', status: 'Active' },
    { id: 'cyc-2', cycleName: 'Đánh giá Năng lực Năm 2026', periodKey: '2026-FULL', dates: '01/01/2026 - 31/12/2026', status: 'Draft' },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: MANAGER EVALUATIONS (UC_HRM_179)
  // ────────────────────────────────────────────────────────────────────────────
  const [evaluations, setEvaluations] = useState<ManagerEvaluationItem[]>([
    { id: 'ev-1', employeeId: 'emp-101', kpiScore: 92, competencyScore: 88, finalGrade: 'A', status: 'Completed' },
    { id: 'ev-2', employeeId: 'emp-102', kpiScore: 78, competencyScore: 74, finalGrade: 'B', status: 'Completed' },
    { id: 'ev-3', employeeId: 'emp-103', kpiScore: 0, competencyScore: 0, finalGrade: 'D', status: 'Pending' },
  ]);

  const [evalForm, setEvalForm] = useState<{ id?: string; employeeId: string; kpiScore: number; competencyScore: number; comments: string }>({
    employeeId: 'emp-103',
    kpiScore: 80,
    competencyScore: 80,
    comments: '',
  });
  const [isEvalModalOpen, setIsEvalModalOpen] = useState(false);

  const handleSaveEvaluation = (e: React.FormEvent) => {
    e.preventDefault();
    const calculated = calculateFinalKpiGrade(evalForm.kpiScore, evalForm.competencyScore);

    if (evalForm.id) {
      setEvaluations((prev) =>
        prev.map((item) =>
          item.id === evalForm.id
            ? { ...item, kpiScore: evalForm.kpiScore, competencyScore: evalForm.competencyScore, finalGrade: calculated.grade, status: 'Completed' }
            : item
        )
      );
      showToast(`Cập nhật đánh giá thành công! Xếp loại: Xếp hạng ${calculated.grade} (${calculated.finalScore} điểm)`);
    } else {
      setEvaluations((prev) => [
        ...prev,
        {
          id: `ev-${Date.now()}`,
          employeeId: evalForm.employeeId,
          kpiScore: evalForm.kpiScore,
          competencyScore: evalForm.competencyScore,
          finalGrade: calculated.grade,
          status: 'Completed',
        },
      ]);
      showToast(`Tạo phiếu đánh giá mới thành công! Xếp loại: Xếp hạng ${calculated.grade}`);
    }
    setIsEvalModalOpen(false);
  };

  const cycleCompletionStats = calculateCycleCompletionStats(evaluations);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: SELF EVALUATION (UC_HRM_180)
  // ────────────────────────────────────────────────────────────────────────────
  const [selfForm, setSelfForm] = useState({
    period: '2026-Q3',
    achievements: 'Hoàn thành tốt 100% mục tiêu phát triển tính năng Module HRM đúng tiến độ Kế hoạch.',
    improvements: 'Cần nâng cao hơn nữa tốc độ thực thi unit test và tối ưu truy vấn EF Core.',
    rating: 5,
    status: 'Submitted',
  });

  const handleSaveSelfEval = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateSelfEvaluationScore(selfForm.rating);
    showToast(`Đã gửi tự đánh giá thành công! Đánh giá: ${val.clampedRating} sao ⭐`);
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Bước 159: Mẫu KPI, Kỳ đánh giá, Quản lý đánh giá & Tự đánh giá
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Mẫu KPI (UC_HRM_177), Kỳ đánh giá (UC_HRM_178), Quản lý đánh giá nhân sự (UC_HRM_179) & Nhân viên tự đánh giá (UC_HRM_180).
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
          { key: 'templates', label: '1. Mẫu KPI / Năng lực (UC_HRM_177)' },
          { key: 'cycles', label: '2. Kỳ Đánh giá (UC_HRM_178)' },
          { key: 'evaluations', label: '3. Đánh giá Nhân viên (UC_HRM_179)' },
          { key: 'selfEval', label: '4. Tự đánh giá (UC_HRM_180)' },
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
      {/* TAB 1: TEMPLATES */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'templates' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Quản lý Mẫu Đánh giá KPI & Năng lực</h2>
            <button
              onClick={() => {
                setTemplateForm({ code: '', title: '', maxScore: 100, weight: 100 });
                setIsTemplateModalOpen(true);
              }}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition"
            >
              + Tạo mẫu KPI mới
            </button>
          </div>

          <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border border-slate-200 dark:border-slate-800">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-300">
                <tr>
                  <th className="p-3">Mã mẫu</th>
                  <th className="p-3">Tên mẫu tiêu chuẩn</th>
                  <th className="p-3">Điểm tối đa</th>
                  <th className="p-3">Tỷ trọng (%)</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {templates.map((t) => (
                  <tr key={t.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-mono font-bold text-indigo-600">{t.code}</td>
                    <td className="p-3 font-semibold">{t.title}</td>
                    <td className="p-3 font-bold">{t.maxScore} điểm</td>
                    <td className="p-3 font-bold text-emerald-600">{t.weightPercentage}%</td>
                    <td className="p-3 text-right space-x-2">
                      <button
                        onClick={() => {
                          setTemplateForm({ id: t.id, code: t.code, title: t.title, maxScore: t.maxScore, weight: t.weightPercentage });
                          setIsTemplateModalOpen(true);
                        }}
                        className="text-xs text-indigo-600 hover:underline"
                      >
                        Sửa
                      </button>
                      <button onClick={() => handleDeleteTemplate(t.id)} className="text-xs text-rose-600 hover:underline">
                        Xóa
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
      {/* TAB 2: CYCLES */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'cycles' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Danh sách Kỳ Đánh giá Hiệu suất</h2>
          </div>

          <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border border-slate-200 dark:border-slate-800">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-300">
                <tr>
                  <th className="p-3">Tên kỳ đánh giá</th>
                  <th className="p-3">Mã kỳ</th>
                  <th className="p-3">Thời gian thực hiện</th>
                  <th className="p-3">Trạng thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {cycles.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-semibold">{c.cycleName}</td>
                    <td className="p-3 font-mono text-xs">{c.periodKey}</td>
                    <td className="p-3 text-xs text-slate-500">{c.dates}</td>
                    <td className="p-3">
                      <span
                        className={`px-2.5 py-0.5 text-xs rounded font-bold ${
                          c.status === 'Active' ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-100 text-slate-600'
                        }`}
                      >
                        {c.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 3: MANAGER EVALUATIONS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'evaluations' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Kết quả Đánh giá Quản lý & Xếp loại</h2>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div className="p-4 bg-white dark:bg-slate-900 border rounded-xl shadow-sm">
              <p className="text-xs text-slate-500 font-medium">Tổng đánh giá</p>
              <p className="text-2xl font-bold mt-1 text-slate-900 dark:text-slate-100">{cycleCompletionStats.total}</p>
            </div>
            <div className="p-4 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 rounded-xl shadow-sm">
              <p className="text-xs text-emerald-700 dark:text-emerald-300 font-medium">Đã hoàn thành</p>
              <p className="text-2xl font-bold mt-1 text-emerald-700 dark:text-emerald-300">{cycleCompletionStats.completed}</p>
            </div>
            <div className="p-4 bg-amber-50 dark:bg-amber-950/40 border border-amber-200 rounded-xl shadow-sm">
              <p className="text-xs text-amber-700 dark:text-amber-300 font-medium">Chờ đánh giá</p>
              <p className="text-2xl font-bold mt-1 text-amber-700 dark:text-amber-300">{cycleCompletionStats.pending}</p>
            </div>
            <div className="p-4 bg-sky-50 dark:bg-sky-950/40 border border-sky-200 rounded-xl shadow-sm">
              <p className="text-xs text-sky-700 dark:text-sky-300 font-medium">Tỷ lệ hoàn thành</p>
              <p className="text-2xl font-bold mt-1 text-sky-700 dark:text-sky-300">{cycleCompletionStats.completionRate}%</p>
            </div>
          </div>

          <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border border-slate-200 dark:border-slate-800">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-300">
                <tr>
                  <th className="p-3">Mã phiếu</th>
                  <th className="p-3">Điểm KPI</th>
                  <th className="p-3">Điểm Năng lực</th>
                  <th className="p-3">Xếp loại</th>
                  <th className="p-3">Trạng thái</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {evaluations.map((ev) => (
                  <tr key={ev.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-mono font-bold">{ev.id}</td>
                    <td className="p-3 font-bold">{ev.kpiScore}đ</td>
                    <td className="p-3 font-bold">{ev.competencyScore}đ</td>
                    <td className="p-3">
                      <span
                        className={`px-3 py-1 rounded-full text-xs font-black ${
                          ev.finalGrade === 'A'
                            ? 'bg-purple-100 text-purple-800'
                            : ev.finalGrade === 'B'
                            ? 'bg-emerald-100 text-emerald-800'
                            : 'bg-amber-100 text-amber-800'
                        }`}
                      >
                        Hạng {ev.finalGrade}
                      </span>
                    </td>
                    <td className="p-3 text-xs font-medium">{ev.status}</td>
                    <td className="p-3 text-right">
                      <button
                        onClick={() => {
                          setEvalForm({ id: ev.id, employeeId: ev.employeeId, kpiScore: ev.kpiScore, competencyScore: ev.competencyScore, comments: '' });
                          setIsEvalModalOpen(true);
                        }}
                        className="text-xs text-indigo-600 hover:underline"
                      >
                        {ev.status === 'Completed' ? 'Sửa điểm' : 'Chấm điểm'}
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
      {/* TAB 4: SELF EVALUATION */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'selfEval' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Phiếu Nhân viên Tự đánh giá Hiệu suất</h2>
          </div>

          <form onSubmit={handleSaveSelfEval} className="bg-white dark:bg-slate-900 border p-6 rounded-xl space-y-4 shadow-sm">
            <div>
              <label className="text-xs font-semibold">Kỳ đánh giá</label>
              <input
                type="text"
                value={selfForm.period}
                onChange={(e) => setSelfForm({ ...selfForm, period: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm font-mono dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Thành tựu & Kết quả đạt được trong kỳ</label>
              <textarea
                rows={3}
                value={selfForm.achievements}
                onChange={(e) => setSelfForm({ ...selfForm, achievements: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Điểm cần cải thiện & Định hướng sắp tới</label>
              <textarea
                rows={2}
                value={selfForm.improvements}
                onChange={(e) => setSelfForm({ ...selfForm, improvements: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Mức tự đánh giá (1 - 5 Sao)</label>
              <div className="flex gap-3 mt-2">
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    key={star}
                    type="button"
                    onClick={() => setSelfForm({ ...selfForm, rating: star })}
                    className={`px-4 py-2 rounded-lg text-sm font-bold border transition ${
                      selfForm.rating === star
                        ? 'bg-amber-500 text-white border-amber-600 shadow'
                        : 'border-slate-200 hover:bg-slate-50 dark:border-slate-800'
                    }`}
                  >
                    {star} ⭐
                  </button>
                ))}
              </div>
            </div>
            <div className="pt-2 flex justify-end">
              <button type="submit" className="px-5 py-2 bg-indigo-600 text-white font-medium rounded-lg text-sm hover:bg-indigo-700 transition">
                🚀 Gửi Phiếu Tự Đánh Giá
              </button>
            </div>
          </form>
        </div>
      )}

      {/* TEMPLATE MODAL */}
      {isTemplateModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveTemplate} className="bg-white dark:bg-slate-900 rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">{templateForm.id ? 'Sửa mẫu KPI' : 'Tạo mẫu KPI mới'}</h3>
            <div>
              <label className="text-xs font-semibold">Mã mẫu KPI</label>
              <input
                type="text"
                value={templateForm.code}
                onChange={(e) => setTemplateForm({ ...templateForm, code: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm font-mono dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Tên mẫu tiêu chuẩn</label>
              <input
                type="text"
                value={templateForm.title}
                onChange={(e) => setTemplateForm({ ...templateForm, title: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-xs font-semibold">Điểm tối đa</label>
                <input
                  type="number"
                  value={templateForm.maxScore}
                  onChange={(e) => setTemplateForm({ ...templateForm, maxScore: Number(e.target.value) })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
              <div>
                <label className="text-xs font-semibold">Tỷ trọng (%)</label>
                <input
                  type="number"
                  value={templateForm.weight}
                  onChange={(e) => setTemplateForm({ ...templateForm, weight: Number(e.target.value) })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsTemplateModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium">
                Lưu lại
              </button>
            </div>
          </form>
        </div>
      )}

      {/* EVALUATION MODAL */}
      {isEvalModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveEvaluation} className="bg-white dark:bg-slate-900 rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">Chấm điểm & Xếp loại Đánh giá</h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-xs font-semibold">Điểm KPI (0 - 100)</label>
                <input
                  type="number"
                  min="0"
                  max="100"
                  value={evalForm.kpiScore}
                  onChange={(e) => setEvalForm({ ...evalForm, kpiScore: Number(e.target.value) })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
              <div>
                <label className="text-xs font-semibold">Điểm Năng lực (0 - 100)</label>
                <input
                  type="number"
                  min="0"
                  max="100"
                  value={evalForm.competencyScore}
                  onChange={(e) => setEvalForm({ ...evalForm, competencyScore: Number(e.target.value) })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
            </div>
            <div className="p-3 bg-indigo-50 border border-indigo-200 text-indigo-900 text-xs rounded-lg font-semibold">
              Xếp loại dự kiến:{' '}
              <span className="text-sm font-bold text-indigo-700">
                Hạng {calculateFinalKpiGrade(evalForm.kpiScore, evalForm.competencyScore).grade} (
                {calculateFinalKpiGrade(evalForm.kpiScore, evalForm.competencyScore).finalScore} điểm)
              </span>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsEvalModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium">
                Lưu kết quả
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
