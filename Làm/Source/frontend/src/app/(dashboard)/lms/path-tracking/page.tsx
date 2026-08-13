'use client';

import React, { useState } from 'react';
import {
  calculateComplianceRatePct,
  evaluatePathProgress,
  filterLearningPathsByRole,
} from '@/shared/api/lms-path-tracking-helpers';

export default function LmsPathTrackingPage() {
  const [activeTab, setActiveTab] = useState<'report' | 'paths' | 'autoassign' | 'progress'>('report');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: COMPLIANCE RATE REPORT (UC_LMS_060)
  // ────────────────────────────────────────────────────────────────────────────
  const [acknowledgementReports] = useState([
    { department: 'Xưởng Sản xuất 1', totalEmployees: 40, acknowledgedCount: 38, pendingCount: 2 },
    { department: 'Khối Văn phòng', totalEmployees: 25, acknowledgedCount: 22, pendingCount: 3 },
    { department: 'Kho & Vận chuyển', totalEmployees: 30, acknowledgedCount: 20, pendingCount: 10 },
    { department: 'Bộ phận QC & An toàn', totalEmployees: 15, acknowledgedCount: 15, pendingCount: 0 },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: LEARNING PATH BY ROLE (UC_LMS_061)
  // ────────────────────────────────────────────────────────────────────────────
  const [searchRoleFilter, setSearchRoleFilter] = useState('');
  const [learningPaths, setLearningPaths] = useState([
    {
      id: 'path-1',
      title: 'Lộ trình Đào tạo Backend Developer Junior',
      jobTitle: 'Backend Developer',
      description: 'Lộ trình chuẩn 30 ngày cho Lập trình viên Backend mới nhận việc',
      targetDaysToComplete: 30,
      isActive: true,
      courses: ['Khóa Đào tạo Nội quy & Văn hóa', 'Khóa Lập trình DDD & C#', 'Khóa Microservices & Message Queue'],
    },
    {
      id: 'path-2',
      title: 'Lộ trình Onboarding Nhân viên Vận hành Kho',
      jobTitle: 'Warehouse Operator',
      description: 'Lộ trình 14 ngày chuẩn hóa quy trình xuất/nhập/kiểm kê kho',
      targetDaysToComplete: 14,
      isActive: true,
      courses: ['Khóa An toàn Lao động Kho', 'Khóa Hướng dẫn sử dụng phần mềm ERP INV'],
    },
  ]);

  const [newPathForm, setNewPathForm] = useState({ title: '', jobTitle: '', description: '', targetDays: 30 });

  const handleCreatePath = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newPathForm.title || !newPathForm.jobTitle) {
      showToast('Vui lòng nhập tiêu đề và chức danh lộ trình.', 'error');
      return;
    }

    const created = {
      id: `path-${Date.now()}`,
      title: newPathForm.title,
      jobTitle: newPathForm.jobTitle,
      description: newPathForm.description || 'Lộ trình đào tạo theo chức danh',
      targetDaysToComplete: newPathForm.targetDays,
      isActive: true,
      courses: ['Khóa Học Định hướng & Nội quy'],
    };

    setLearningPaths([created, ...learningPaths]);
    setNewPathForm({ title: '', jobTitle: '', description: '', targetDays: 30 });
    showToast(`Đã tạo thành công Lộ trình đào tạo [${created.title}] cho chức danh ${created.jobTitle}!`, 'success');
  };

  const filteredPaths = filterLearningPathsByRole(learningPaths, searchRoleFilter);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: AUTO ASSIGN ON HIRE (UC_LMS_062)
  // ────────────────────────────────────────────────────────────────────────────
  const [onboardForm, setOnboardForm] = useState({
    employeeName: 'Nguyễn Văn M (EMP165)',
    jobTitle: 'Backend Developer',
  });
  const [autoAssignResult, setAutoAssignResult] = useState<any>(null);

  const handleAutoAssignOnHire = (e: React.FormEvent) => {
    e.preventDefault();
    const matchedPath = learningPaths.find((p) => p.jobTitle.toLowerCase() === onboardForm.jobTitle.toLowerCase()) || learningPaths[0];

    const result = {
      employeeName: onboardForm.employeeName,
      jobTitle: onboardForm.jobTitle,
      assignedPathTitle: matchedPath.title,
      coursesAssigned: matchedPath.courses,
      targetDays: matchedPath.targetDaysToComplete,
      assignedAt: new Date().toLocaleDateString('vi-VN'),
    };

    setAutoAssignResult(result);
    showToast(`Tự động gán thành công ${matchedPath.courses.length} khóa học bắt buộc cho ${onboardForm.employeeName}!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: PATH COMPLETION TRACKING (UC_LMS_063)
  // ────────────────────────────────────────────────────────────────────────────
  const [userPathProgresses] = useState([
    { id: 'up-1', employee: 'Trần Thị H (EMP120)', jobTitle: 'Backend Developer', pathTitle: 'Lộ trình Backend Junior', completedCount: 3, totalCount: 3, dueDate: '2026-08-30T00:00:00Z' },
    { id: 'up-2', employee: 'Lê Văn K (EMP145)', jobTitle: 'Warehouse Operator', pathTitle: 'Lộ trình Vận hành Kho', completedCount: 1, totalCount: 2, dueDate: '2026-08-01T00:00:00Z' },
    { id: 'up-3', employee: 'Vũ Thị N (EMP160)', jobTitle: 'Backend Developer', pathTitle: 'Lộ trình Backend Junior', completedCount: 1, totalCount: 3, dueDate: '2026-09-15T00:00:00Z' },
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
            <span className="bg-brand-foreground/20 text-brand-foreground/80 text-xs px-3 py-1 rounded-full font-semibold border border-brand-foreground/30">
              LMS - LỘ TRÌNH ĐÀO TẠO & BÁO CÁO TUÂN THỦ
            </span>
            <h1 className="text-2xl font-bold mt-2">Acknowledgement Report & Job Role Learning Paths</h1>
            <p className="text-brand-foreground/80 text-sm mt-1">
              Báo cáo tỷ lệ xác nhận, gán lộ trình theo chức danh, tự gán khóa khi nhận việc & theo dõi tiến độ
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
            onClick={() => setActiveTab('report')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'report' ? 'bg-surface text-foreground shadow-md' : 'text-brand-foreground/80 hover:bg-surface/10'
            }`}
          >
            📊 UC_LMS_060: Báo cáo tỷ lệ xác nhận
          </button>
          <button
            onClick={() => setActiveTab('paths')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'paths' ? 'bg-surface text-foreground shadow-md' : 'text-brand-foreground/80 hover:bg-surface/10'
            }`}
          >
            🎯 UC_LMS_061: Lộ trình theo chức danh
          </button>
          <button
            onClick={() => setActiveTab('autoassign')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'autoassign' ? 'bg-surface text-foreground shadow-md' : 'text-brand-foreground/80 hover:bg-surface/10'
            }`}
          >
            ⚡ UC_LMS_062: Tự gán khi nhận việc
          </button>
          <button
            onClick={() => setActiveTab('progress')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'progress' ? 'bg-surface text-foreground shadow-md' : 'text-brand-foreground/80 hover:bg-surface/10'
            }`}
          >
            📈 UC_LMS_063: Theo dõi hoàn thành
          </button>
        </div>
      </div>

      {/* TAB 1: ACKNOWLEDGEMENT REPORT */}
      {activeTab === 'report' && (
        <div className="rounded-xl border border-border bg-surface shadow-sm p-5 space-y-6">
          <h2 className="text-lg font-bold text-foreground flex items-center gap-2">
            <span>📊</span> Báo cáo Tỷ lệ Xác nhận Đọc Quy định & Nội quy (UC_LMS_060)
          </h2>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {acknowledgementReports.map((item) => {
              const evalRes = calculateComplianceRatePct(item.acknowledgedCount, item.totalEmployees);
              return (
                <div key={item.department} className="p-4 rounded-xl border border-border bg-slate-50 flex flex-col justify-between">
                  <div>
                    <h3 className="font-bold text-slate-900 text-sm">{item.department}</h3>
                    <div className="flex justify-between text-xs text-muted-foreground mt-2">
                      <span>Tổng nhân sự: {item.totalEmployees}</span>
                      <span>Đã xác nhận: {item.acknowledgedCount}</span>
                    </div>
                  </div>

                  <div className="mt-4 pt-3 border-t border-border flex justify-between items-center">
                    <span className="text-2xl font-extrabold text-slate-900">{evalRes.complianceRatePct}%</span>
                    <span
                      className={`px-2.5 py-0.5 text-xs font-bold rounded-full ${
                        evalRes.statusBadge === 'Good'
                          ? 'bg-emerald-100 text-emerald-800'
                          : evalRes.statusBadge === 'Warning'
                          ? 'bg-amber-100 text-amber-800'
                          : 'bg-rose-100 text-rose-800'
                      }`}
                    >
                      {evalRes.statusBadge === 'Good' ? 'Tốt' : evalRes.statusBadge === 'Warning' ? 'Cần nhắc nhở' : 'Chưa đạt'}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* TAB 2: LEARNING PATHS */}
      {activeTab === 'paths' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 rounded-xl border border-border bg-surface shadow-sm p-5 space-y-4">
              <div className="flex justify-between items-center">
                <h2 className="text-lg font-bold text-foreground">🎯 Quản lý Lộ trình Đào tạo Theo Chức danh (UC_LMS_061)</h2>
                <input
                  type="text"
                  placeholder="Lọc theo chức danh..."
                  value={searchRoleFilter}
                  onChange={(e) => setSearchRoleFilter(e.target.value)}
                  className="border border-border rounded-lg text-xs px-3 py-1.5 w-48"
                />
              </div>

              <div className="space-y-4">
                {filteredPaths.map((p) => (
                  <div key={p.id} className="p-4 rounded-xl border border-border bg-slate-50 space-y-3">
                    <div className="flex justify-between items-start">
                      <div>
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{p.jobTitle}</span>
                        <h3 className="font-bold text-slate-900 mt-1 text-base">{p.title}</h3>
                        <p className="text-xs text-muted-foreground">{p.description}</p>
                      </div>
                      <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800">
                        ⏱️ Hạn hoàn thành: {p.targetDaysToComplete} ngày
                      </span>
                    </div>

                    <div className="bg-surface p-3 rounded-lg border border-border text-xs space-y-1">
                      <p className="font-semibold text-foreground">Các khóa học bắt buộc trong lộ trình:</p>
                      <ul className="list-disc list-inside text-muted-foreground">
                        {p.courses.map((c, idx) => (
                          <li key={idx}>{c}</li>
                        ))}
                      </ul>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Create form */}
            <div className="rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4">➕ Tạo Lộ trình Đào tạo Mới</h2>
              <form onSubmit={handleCreatePath} className="space-y-4 text-sm">
                <div>
                  <label className="block text-foreground font-medium mb-1">Tên lộ trình:</label>
                  <input
                    type="text"
                    value={newPathForm.title}
                    onChange={(e) => setNewPathForm({ ...newPathForm, title: e.target.value })}
                    className="w-full border border-border rounded-lg p-2"
                    placeholder="VD: Lộ trình Frontend React Senior"
                  />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Chức danh áp dụng:</label>
                  <input
                    type="text"
                    value={newPathForm.jobTitle}
                    onChange={(e) => setNewPathForm({ ...newPathForm, jobTitle: e.target.value })}
                    className="w-full border border-border rounded-lg p-2"
                    placeholder="VD: Frontend Developer"
                  />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Mô tả:</label>
                  <textarea
                    value={newPathForm.description}
                    onChange={(e) => setNewPathForm({ ...newPathForm, description: e.target.value })}
                    className="w-full border border-border rounded-lg p-2"
                    rows={2}
                  />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Thời hạn hoàn thành (ngày):</label>
                  <input
                    type="number"
                    value={newPathForm.targetDays}
                    onChange={(e) => setNewPathForm({ ...newPathForm, targetDays: Number(e.target.value) })}
                    className="w-full border border-border rounded-lg p-2"
                  />
                </div>

                <button type="submit" className="w-full py-2 bg-brand text-white rounded-lg font-semibold hover:bg-brand-hover">
                  Lưu Lộ trình Đào tạo
                </button>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* TAB 3: AUTO ASSIGN ON HIRE */}
      {activeTab === 'autoassign' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4">⚡ Tự Động Gán Khóa Bắt Buộc Khi Nhận Việc (UC_LMS_062)</h2>
              <form onSubmit={handleAutoAssignOnHire} className="space-y-4 text-sm">
                <div>
                  <label className="block text-foreground font-medium mb-1">Tên nhân viên mới nhận việc:</label>
                  <input
                    type="text"
                    value={onboardForm.employeeName}
                    onChange={(e) => setOnboardForm({ ...onboardForm, employeeName: e.target.value })}
                    className="w-full border border-border rounded-lg p-2"
                  />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Chức danh vị trí tuyển dụng:</label>
                  <select
                    value={onboardForm.jobTitle}
                    onChange={(e) => setOnboardForm({ ...onboardForm, jobTitle: e.target.value })}
                    className="w-full border border-border rounded-lg p-2 bg-surface"
                  >
                    <option value="Backend Developer">Backend Developer</option>
                    <option value="Warehouse Operator">Warehouse Operator</option>
                  </select>
                </div>

                <button type="submit" className="w-full py-2 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700">
                  Kích hoạt Tự động Gán Lộ trình Onboarding
                </button>
              </form>
            </div>

            <div className="lg:col-span-2 rounded-xl border border-border bg-surface shadow-sm p-5">
              <h2 className="text-lg font-bold text-foreground mb-4">📋 Kết quả Gán Khóa Đào tạo Tự động</h2>
              {autoAssignResult ? (
                <div className="p-4 rounded-xl border border-emerald-200 bg-emerald-50/50 space-y-3">
                  <div className="flex justify-between items-center">
                    <h3 className="font-bold text-emerald-950 text-base">{autoAssignResult.employeeName}</h3>
                    <span className="px-3 py-1 text-xs font-bold rounded-full bg-emerald-600 text-white">✓ Đã gán tự động</span>
                  </div>
                  <p className="text-xs text-foreground">
                    Chức danh: <strong>{autoAssignResult.jobTitle}</strong> • Lộ trình: <strong>{autoAssignResult.assignedPathTitle}</strong>
                  </p>
                  <div className="bg-surface p-3 rounded-lg border border-border text-xs space-y-1">
                    <p className="font-semibold text-foreground">Danh sách khóa học được tự động kích hoạt:</p>
                    <ul className="list-disc list-inside text-muted-foreground">
                      {autoAssignResult.coursesAssigned.map((c: string, idx: number) => (
                        <li key={idx}>{c}</li>
                      ))}
                    </ul>
                  </div>
                  <p className="text-xs text-muted-foreground italic">Hạn hoàn thành bài học: {autoAssignResult.targetDays} ngày kể từ ngày nhận việc.</p>
                </div>
              ) : (
                <div className="p-8 text-center text-slate-400 text-sm">
                  Chưa thực hiện gán khóa onboarding. Hãy bấm nút kích hoạt ở bên trái.
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* TAB 4: PATH COMPLETION TRACKING */}
      {activeTab === 'progress' && (
        <div className="rounded-xl border border-border bg-surface shadow-sm p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📈 Theo Dõi Hoàn Thành Lộ Trình Đào Tạo Nhân Viên (UC_LMS_063)</h2>

          <div className="space-y-3">
            {userPathProgresses.map((item) => {
              const evalProgress = evaluatePathProgress(item.completedCount, item.totalCount, item.dueDate);
              return (
                <div key={item.id} className="p-4 rounded-xl border border-border bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{item.employee}</h3>
                    <p className="text-xs text-muted-foreground mt-1">
                      Chức danh: {item.jobTitle} • {item.pathTitle}
                    </p>
                    <div className="w-64 bg-slate-200 rounded-full h-2 mt-2">
                      <div
                        className={`h-2 rounded-full ${
                          evalProgress.isCompleted ? 'bg-emerald-500' : evalProgress.isOverdue ? 'bg-rose-500' : 'bg-blue-500'
                        }`}
                        style={{ width: `${evalProgress.progressPct}%` }}
                      ></div>
                    </div>
                  </div>

                  <div className="text-right">
                    <span className="text-lg font-extrabold text-slate-900">{evalProgress.progressPct}%</span>
                    <p className="text-xs text-muted-foreground font-medium mt-0.5">
                      ({item.completedCount}/{item.totalCount} Khóa học)
                    </p>
                    <span
                      className={`inline-block mt-1 px-2.5 py-0.5 text-xs font-bold rounded-full ${
                        evalProgress.isCompleted
                          ? 'bg-emerald-100 text-emerald-800'
                          : evalProgress.isOverdue
                          ? 'bg-rose-100 text-rose-800'
                          : 'bg-blue-100 text-blue-800'
                      }`}
                    >
                      {evalProgress.statusText}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
