'use client';

import React, { useState } from 'react';
import {
  calculateGradeDistribution,
  validateCourseTag,
  parseSemanticVersion,
  generateRandomExamQuestions,
  CourseTagItem,
} from '@/shared/api/hrm-lms-eval-catalog-helpers';

export default function HrmLmsEvalCatalogPage() {
  const [activeTab, setActiveTab] = useState<'summary' | 'tags' | 'versions' | 'randomExam'>('summary');

  // Toast notifications
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: EVALUATION SUMMARY REPORT (UC_HRM_181)
  // ────────────────────────────────────────────────────────────────────────────
  const sampleGrades = ['A', 'A', 'A', 'B', 'B', 'B', 'B', 'C', 'C', 'D'];
  const gradeDist = calculateGradeDistribution(sampleGrades);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: COURSE SKILL TAGS (UC_LMS_007)
  // ────────────────────────────────────────────────────────────────────────────
  const [tags, setTags] = useState<CourseTagItem[]>([
    { id: 'tag-1', courseId: 'crs-101', tagName: 'C# .NET Core', tagType: 'Skill' },
    { id: 'tag-2', courseId: 'crs-101', tagName: 'Backend Developer', tagType: 'Position' },
    { id: 'tag-3', courseId: 'crs-102', tagName: 'Quản trị nhân sự', tagType: 'Skill' },
  ]);
  const [tagForm, setTagForm] = useState({ courseId: 'crs-101', tagName: '', tagType: 'Skill' });
  const [isTagModalOpen, setIsTagModalOpen] = useState(false);

  const handleSaveTag = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateCourseTag(tagForm.tagName, tagForm.tagType);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu tag không hợp lệ', 'error');
      return;
    }

    setTags((prev) => [
      ...prev,
      {
        id: `tag-${Date.now()}`,
        courseId: tagForm.courseId,
        tagName: tagForm.tagName.trim(),
        tagType: val.normalizedType,
      },
    ]);
    showToast(`Gắn tag [${tagForm.tagName}] cho khóa học thành công!`);
    setIsTagModalOpen(false);
  };

  const handleDeleteTag = (id: string) => {
    setTags((prev) => prev.filter((t) => t.id !== id));
    showToast('Đã xóa tag khóa học.');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: COURSE VERSIONS (UC_LMS_008)
  // ────────────────────────────────────────────────────────────────────────────
  const [versions, setVersions] = useState([
    { id: 'ver-1', courseName: 'Khóa học Lập trình C# .NET', version: '2.0', changelog: 'Cập nhật tài liệu .NET 8 & EF Core 8', date: '10/08/2026' },
    { id: 'ver-2', courseName: 'Khóa học Lập trình C# .NET', version: '1.0', changelog: 'Phiên bản khởi tạo ban đầu', date: '01/01/2026' },
  ]);
  const [verForm, setVerForm] = useState({ version: '2.1', changelog: '' });
  const [isVerModalOpen, setIsVerModalOpen] = useState(false);

  const handleSaveVersion = (e: React.FormEvent) => {
    e.preventDefault();
    const parsed = parseSemanticVersion(verForm.version);
    if (!parsed.isValid) {
      showToast('Định dạng phiên bản phải có dạng X.Y (VD: 1.0, 2.1)', 'error');
      return;
    }

    setVersions((prev) => [
      {
        id: `ver-${Date.now()}`,
        courseName: 'Khóa học Lập trình C# .NET',
        version: parsed.normalized,
        changelog: verForm.changelog || 'Cập nhật nội dung bài giảng mới',
        date: new Date().toLocaleDateString('vi-VN'),
      },
      ...prev,
    ]);
    showToast(`Phát hành phiên bản v${parsed.normalized} thành công!`);
    setIsVerModalOpen(false);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: RANDOM EXAM (UC_LMS_013)
  // ────────────────────────────────────────────────────────────────────────────
  const [examForm, setExamForm] = useState({
    title: 'Đề thi Kiểm tra Tổng hợp C# .NET',
    count: 5,
    passScore: 80,
    duration: 45,
  });

  const questionPool = [
    { id: 'q1', content: 'C# 12 giới thiệu tính năng Primary Constructors cho class như thế nào?' },
    { id: 'q2', content: 'Phân biệt IQueryable và IEnumerable trong EF Core?' },
    { id: 'q3', content: 'Dependency Injection Scoped vs Transient vs Singleton khác nhau như thế nào?' },
    { id: 'q4', content: 'Async / Await và Task.FromResult trong C#?' },
    { id: 'q5', content: 'Cấu trúc Middleware trong ASP.NET Core?' },
    { id: 'q6', content: 'Index trong SQL Server giúp tối ưu query như thế nào?' },
  ];

  const [generatedExamResult, setGeneratedExamResult] = useState<{ title: string; count: number; questions: any[] } | null>(null);

  const handleGenerateExam = (e: React.FormEvent) => {
    e.preventDefault();
    const res = generateRandomExamQuestions(questionPool, examForm.count);
    setGeneratedExamResult({
      title: examForm.title,
      count: res.count,
      questions: res.selected,
    });
    showToast(`Đã sinh thành công đề thi ngẫu nhiên gồm ${res.count} câu hỏi!`);
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Bước 160: Tổng hợp Đánh giá HRM, Tag Kỹ năng, Version & Đề thi Random LMS
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Tổng hợp kết quả đánh giá (UC_HRM_181), Tag kỹ năng/vị trí (UC_LMS_007), Phiên bản nội dung (UC_LMS_008) & Đề thi random (UC_LMS_013).
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
          { key: 'summary', label: '1. Báo cáo Tổng hợp Đánh giá (UC_HRM_181)' },
          { key: 'tags', label: '2. Tag Kỹ năng / Vị trí LMS (UC_LMS_007)' },
          { key: 'versions', label: '3. Phiên bản Khóa học (UC_LMS_008)' },
          { key: 'randomExam', label: '4. Sinh Đề thi Random (UC_LMS_013)' },
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
      {/* TAB 1: EVALUATION SUMMARY */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'summary' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Báo cáo Tổng hợp Kết quả Đánh giá Hiệu suất</h2>
            <button onClick={() => showToast('Đã xuất báo cáo tổng hợp đánh giá thành công!')} className="px-4 py-2 bg-emerald-600 text-white rounded-lg text-sm font-medium hover:bg-emerald-700 transition">
              📊 Xuất báo cáo Tổng hợp
            </button>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div className="p-4 bg-purple-50 border border-purple-200 rounded-xl shadow-sm">
              <p className="text-xs text-purple-700 font-bold">Xếp loại Hạng A (Xuất sắc)</p>
              <p className="text-2xl font-bold mt-1 text-purple-900">{gradeDist.distributions.A.count} người ({gradeDist.distributions.A.percentage}%)</p>
            </div>
            <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-xl shadow-sm">
              <p className="text-xs text-emerald-700 font-bold">Xếp loại Hạng B (Khá / Tốt)</p>
              <p className="text-2xl font-bold mt-1 text-emerald-900">{gradeDist.distributions.B.count} người ({gradeDist.distributions.B.percentage}%)</p>
            </div>
            <div className="p-4 bg-amber-50 border border-amber-200 rounded-xl shadow-sm">
              <p className="text-xs text-amber-700 font-bold">Xếp loại Hạng C (Trung bình)</p>
              <p className="text-2xl font-bold mt-1 text-amber-900">{gradeDist.distributions.C.count} người ({gradeDist.distributions.C.percentage}%)</p>
            </div>
            <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl shadow-sm">
              <p className="text-xs text-rose-700 font-bold">Xếp loại Hạng D (Yếu / Cần cải thiện)</p>
              <p className="text-2xl font-bold mt-1 text-rose-900">{gradeDist.distributions.D.count} người ({gradeDist.distributions.D.percentage}%)</p>
            </div>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 2: COURSE TAGS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'tags' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Gắn Tag Kỹ năng & Vị trí công việc cho Khóa học LMS</h2>
            <button
              onClick={() => {
                setTagForm({ courseId: 'crs-101', tagName: '', tagType: 'Skill' });
                setIsTagModalOpen(true);
              }}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition"
            >
              + Gắn Tag mới
            </button>
          </div>

          <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border border-slate-200 dark:border-slate-800">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-300">
                <tr>
                  <th className="p-3">Tên Tag gắn</th>
                  <th className="p-3">Loại Tag</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {tags.map((t) => (
                  <tr key={t.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-bold text-indigo-600">{t.tagName}</td>
                    <td className="p-3">
                      <span className="px-2.5 py-0.5 text-xs rounded font-bold bg-sky-100 text-sky-800">
                        {t.tagType}
                      </span>
                    </td>
                    <td className="p-3 text-right">
                      <button onClick={() => handleDeleteTag(t.id)} className="text-xs text-rose-600 hover:underline">
                        Xóa Tag
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
      {/* TAB 3: COURSE VERSIONS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'versions' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Quản lý Phiên bản Nội dung Khóa học LMS</h2>
            <button
              onClick={() => {
                setVerForm({ version: '2.1', changelog: '' });
                setIsVerModalOpen(true);
              }}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition"
            >
              + Phát hành phiên bản mới
            </button>
          </div>

          <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border border-slate-200 dark:border-slate-800">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-300">
                <tr>
                  <th className="p-3">Khóa học</th>
                  <th className="p-3">Số phiên bản</th>
                  <th className="p-3">Ghi chú thay đổi (Changelog)</th>
                  <th className="p-3">Ngày phát hành</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {versions.map((v) => (
                  <tr key={v.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-semibold">{v.courseName}</td>
                    <td className="p-3 font-mono font-bold text-emerald-600">v{v.version}</td>
                    <td className="p-3 text-xs">{v.changelog}</td>
                    <td className="p-3 text-xs text-slate-500">{v.date}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 4: RANDOM EXAM */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'randomExam' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Sinh Đề thi Ngẫu nhiên từ Ngân hàng Câu hỏi</h2>
          </div>

          <form onSubmit={handleGenerateExam} className="bg-white dark:bg-slate-900 border p-6 rounded-xl space-y-4 shadow-sm">
            <div>
              <label className="text-xs font-semibold">Tên đề thi</label>
              <input
                type="text"
                value={examForm.title}
                onChange={(e) => setExamForm({ ...examForm, title: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="text-xs font-semibold">Số câu hỏi random</label>
                <input
                  type="number"
                  min="1"
                  max="10"
                  value={examForm.count}
                  onChange={(e) => setExamForm({ ...examForm, count: Number(e.target.value) })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
              <div>
                <label className="text-xs font-semibold">Điểm đạt (%)</label>
                <input
                  type="number"
                  value={examForm.passScore}
                  onChange={(e) => setExamForm({ ...examForm, passScore: Number(e.target.value) })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
              <div>
                <label className="text-xs font-semibold">Thời gian (Phút)</label>
                <input
                  type="number"
                  value={examForm.duration}
                  onChange={(e) => setExamForm({ ...examForm, duration: Number(e.target.value) })}
                  className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                  required
                />
              </div>
            </div>
            <div className="pt-2 flex justify-end">
              <button type="submit" className="px-5 py-2.5 bg-indigo-600 text-white font-medium rounded-lg text-sm hover:bg-indigo-700 transition">
                🎲 Sinh Đề Thi Ngẫu Nhiên
              </button>
            </div>
          </form>

          {generatedExamResult && (
            <div className="p-6 bg-slate-900 text-slate-100 rounded-xl space-y-3 shadow font-mono text-xs">
              <h3 className="text-sm font-bold text-emerald-400">🎉 ĐÃ TẠO ĐỀ THI: {generatedExamResult.title}</h3>
              <p>• Số câu hỏi được trích xuất ngẫu nhiên: {generatedExamResult.count} câu</p>
              <div className="space-y-1.5 pt-2 border-t border-slate-800">
                {generatedExamResult.questions.map((q, idx) => (
                  <p key={q.id} className="text-slate-300">
                    Câu {idx + 1}: {q.content}
                  </p>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* TAG MODAL */}
      {isTagModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveTag} className="bg-white dark:bg-slate-900 rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">Gắn Tag Kỹ năng / Vị trí</h3>
            <div>
              <label className="text-xs font-semibold">Tên Tag</label>
              <input
                type="text"
                value={tagForm.tagName}
                onChange={(e) => setTagForm({ ...tagForm, tagName: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Loại Tag</label>
              <select
                value={tagForm.tagType}
                onChange={(e) => setTagForm({ ...tagForm, tagType: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              >
                <option value="Skill">Kỹ năng (Skill)</option>
                <option value="Position">Vị trí công việc (Position)</option>
                <option value="General">Chung (General)</option>
              </select>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsTagModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium">
                Gắn Tag
              </button>
            </div>
          </form>
        </div>
      )}

      {/* VERSION MODAL */}
      {isVerModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveVersion} className="bg-white dark:bg-slate-900 rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">Phát hành Phiên bản mới</h3>
            <div>
              <label className="text-xs font-semibold">Số phiên bản (VD: 2.1)</label>
              <input
                type="text"
                value={verForm.version}
                onChange={(e) => setVerForm({ ...verForm, version: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm font-mono dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Nội dung thay đổi (Changelog)</label>
              <textarea
                rows={3}
                value={verForm.changelog}
                onChange={(e) => setVerForm({ ...verForm, changelog: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsVerModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium">
                Phát hành
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
