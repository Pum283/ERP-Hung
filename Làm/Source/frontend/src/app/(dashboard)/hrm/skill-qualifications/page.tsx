'use client';

import React, { useState } from 'react';
import {
  validateSkillProficiency,
  calculateMovementStats,
  renderContractTemplate,
  validateBulkCandidateRow,
  SkillItem,
  EmployeeMovementItem,
  CandidateImportRow,
} from '@/shared/api/hrm-skill-qualification-helpers';

export default function HrmSkillQualificationPage() {
  const [activeTab, setActiveTab] = useState<'skills' | 'movement' | 'contract' | 'import'>('skills');

  // Toast notifications
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: SKILLS (UC_HRM_024)
  // ────────────────────────────────────────────────────────────────────────────
  const [skills, setSkills] = useState<SkillItem[]>([
    { id: 'sk-1', employeeId: 'emp-101', skillName: 'C# .NET Core', proficiencyLevel: 'Expert', certificateRef: 'Cert-MICROSOFT-001' },
    { id: 'sk-2', employeeId: 'emp-101', skillName: 'React & Next.js', proficiencyLevel: 'Advanced', certificateRef: 'Cert-FE-2025' },
    { id: 'sk-3', employeeId: 'emp-102', skillName: 'Quản trị nhân sự', proficiencyLevel: 'Expert', certificateRef: 'Cert-HRM-CHRM' },
  ]);
  const [skillForm, setSkillForm] = useState<{ id?: string; employeeId: string; skillName: string; level: string; cert: string }>({
    employeeId: 'emp-101',
    skillName: '',
    level: 'Intermediate',
    cert: '',
  });
  const [isSkillModalOpen, setIsSkillModalOpen] = useState(false);

  const handleSaveSkill = (e: React.FormEvent) => {
    e.preventDefault();
    if (!skillForm.skillName.trim()) {
      showToast('Tên kỹ năng không được để trống', 'error');
      return;
    }
    const val = validateSkillProficiency(skillForm.level);

    if (skillForm.id) {
      setSkills((prev) =>
        prev.map((s) =>
          s.id === skillForm.id
            ? { ...s, skillName: skillForm.skillName, proficiencyLevel: val.normalized, certificateRef: skillForm.cert }
            : s
        )
      );
      showToast('Cập nhật kỹ năng thành công!');
    } else {
      setSkills((prev) => [
        ...prev,
        {
          id: `sk-${Date.now()}`,
          employeeId: skillForm.employeeId,
          skillName: skillForm.skillName,
          proficiencyLevel: val.normalized,
          certificateRef: skillForm.cert,
        },
      ]);
      showToast('Thêm kỹ năng thành công!');
    }
    setIsSkillModalOpen(false);
  };

  const handleDeleteSkill = (id: string) => {
    setSkills((prev) => prev.filter((s) => s.id !== id));
    showToast('Đã xóa kỹ năng.');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: MOVEMENT REPORT (UC_HRM_037)
  // ────────────────────────────────────────────────────────────────────────────
  const sampleEmployees: EmployeeMovementItem[] = [
    { id: '1', status: 'Active', hireDate: '2026-08-02' },
    { id: '2', status: 'Active', hireDate: '2026-08-05' },
    { id: '3', status: 'Terminated', hireDate: '2025-01-01', terminateDate: '2026-08-10' },
    { id: '4', status: 'OnLeave', hireDate: '2025-06-01' },
    { id: '5', status: 'Active', hireDate: '2024-03-01' },
  ];

  const movementStats = calculateMovementStats(sampleEmployees, '2026-08-01', '2026-08-31');

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: CONTRACT PRINT (UC_HRM_044)
  // ────────────────────────────────────────────────────────────────────────────
  const [selectedContract] = useState({
    contractNo: 'HD-2026-0089',
    employeeName: 'Nguyễn Văn Hùng',
    employeeCode: 'EMP-0089',
    contractType: 'Indefinite',
    startDate: '01/08/2026',
    endDate: null,
    baseSalary: 28000000,
  });

  const contractPreviewText = renderContractTemplate(selectedContract);

  const handleExportContractFile = () => {
    const blob = new Blob([contractPreviewText], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `contract_${selectedContract.contractNo}.txt`;
    a.click();
    showToast('Đã xuất file hợp đồng mẫu thành công!');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: BULK IMPORT (UC_HRM_058)
  // ────────────────────────────────────────────────────────────────────────────
  const [rawImportText, setRawImportText] = useState(
    `Nguyễn Văn Bình,binh.nv@example.com,0912345678,post-101\nPhạm Thị Hoa,hoa.pt@example.com,0987654321,post-101\nTrần Đức Anh,invalid-email,0900000000,post-102`
  );
  const [importRows, setImportRows] = useState<{ row: CandidateImportRow; error?: string }[]>([]);
  const [importResultSummary, setImportResultSummary] = useState<{ total: number; success: number; failed: number } | null>(null);

  const handleParseImport = () => {
    const lines = rawImportText.split('\n').filter((l) => l.trim().length > 0);
    const parsed = lines.map((line) => {
      const parts = line.split(',').map((p) => p.trim());
      const row: CandidateImportRow = {
        fullName: parts[0] || '',
        email: parts[1] || '',
        phone: parts[2] || '',
        jobPostingId: parts[3] || '',
      };
      const val = validateBulkCandidateRow(row);
      return { row, error: val.isValid ? undefined : val.error };
    });
    setImportRows(parsed);
    setImportResultSummary(null);
    showToast(`Đã phân tích ${parsed.length} dòng dữ liệu ứng viên.`);
  };

  const handleExecuteImport = () => {
    const valid = importRows.filter((r) => !r.error);
    const invalid = importRows.filter((r) => r.error);
    setImportResultSummary({
      total: importRows.length,
      success: valid.length,
      failed: invalid.length,
    });
    showToast(`Import hoàn tất! Thành công: ${valid.length}, Thất bại: ${invalid.length}`);
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Kỹ năng, Biến động nhân sự, Mẫu hợp đồng & Import ứng viên
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Quản lý trình độ (UC_HRM_024), Báo cáo biến động (UC_HRM_037), In hợp đồng (UC_HRM_044) & Import hàng loạt (UC_HRM_058).
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
          { key: 'skills', label: '1. Kỹ năng & Trình độ (UC_HRM_024)' },
          { key: 'movement', label: '2. Báo cáo Biến động (UC_HRM_037)' },
          { key: 'contract', label: '3. In & Mẫu Hợp đồng (UC_HRM_044)' },
          { key: 'import', label: '4. Import Ứng viên (UC_HRM_058)' },
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
      {/* TAB 1: SKILLS */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'skills' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Quản lý Kỹ năng & Trình độ Nhân sự</h2>
            <button
              onClick={() => {
                setSkillForm({ employeeId: 'emp-101', skillName: '', level: 'Intermediate', cert: '' });
                setIsSkillModalOpen(true);
              }}
              className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium hover:bg-brand-hover transition"
            >
              + Thêm kỹ năng mới
            </button>
          </div>

          <div className="bg-surface shadow rounded-lg overflow-hidden border border-border">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-muted-foreground dark:text-slate-300">
                <tr>
                  <th className="p-3">Tên kỹ năng</th>
                  <th className="p-3">Cấp độ thành thạo</th>
                  <th className="p-3">Mã chứng chỉ / Bằng cấp</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {skills.map((s) => (
                  <tr key={s.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-semibold">{s.skillName}</td>
                    <td className="p-3">
                      <span
                        className={`px-2.5 py-0.5 text-xs rounded font-bold ${
                          s.proficiencyLevel === 'Expert'
                            ? 'bg-brand-muted text-brand-strong'
                            : s.proficiencyLevel === 'Advanced'
                            ? 'bg-brand-muted text-brand-strong'
                            : 'bg-emerald-100 text-emerald-800'
                        }`}
                      >
                        {s.proficiencyLevel}
                      </span>
                    </td>
                    <td className="p-3 font-mono text-xs text-muted-foreground">{s.certificateRef || 'N/A'}</td>
                    <td className="p-3 text-right space-x-2">
                      <button
                        onClick={() => {
                          setSkillForm({
                            id: s.id,
                            employeeId: s.employeeId,
                            skillName: s.skillName,
                            level: s.proficiencyLevel,
                            cert: s.certificateRef || '',
                          });
                          setIsSkillModalOpen(true);
                        }}
                        className="text-xs text-brand hover:underline"
                      >
                        Sửa
                      </button>
                      <button onClick={() => handleDeleteSkill(s.id)} className="text-xs text-rose-600 hover:underline">
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
      {/* TAB 2: MOVEMENT REPORT */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'movement' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Báo cáo Biến động Nhân sự (Tháng 08/2026)</h2>
            <span className="text-xs text-muted-foreground">Từ 01/08/2026 đến 31/08/2026</span>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div className="p-4 bg-surface border rounded-xl shadow-sm">
              <p className="text-xs text-muted-foreground font-medium">Tổng số nhân sự</p>
              <p className="text-2xl font-bold mt-1 text-slate-900 dark:text-slate-100">{movementStats.total}</p>
            </div>
            <div className="p-4 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 rounded-xl shadow-sm">
              <p className="text-xs text-emerald-700 dark:text-emerald-300 font-medium">Tuyển mới (Joiners)</p>
              <p className="text-2xl font-bold mt-1 text-emerald-700 dark:text-emerald-300">+{movementStats.joiners}</p>
            </div>
            <div className="p-4 bg-rose-50 dark:bg-rose-950/40 border border-rose-200 rounded-xl shadow-sm">
              <p className="text-xs text-rose-700 dark:text-rose-300 font-medium">Nghỉ việc (Leavers)</p>
              <p className="text-2xl font-bold mt-1 text-rose-700 dark:text-rose-300">-{movementStats.leavers}</p>
            </div>
            <div className="p-4 bg-sky-50 dark:bg-sky-950/40 border border-sky-200 rounded-xl shadow-sm">
              <p className="text-xs text-sky-700 dark:text-sky-300 font-medium">Tỷ lệ biến động (Turnover)</p>
              <p className="text-2xl font-bold mt-1 text-sky-700 dark:text-sky-300">{movementStats.turnoverRate}%</p>
            </div>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 3: CONTRACT PRINT */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'contract' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Xem trước & In Mẫu Hợp đồng Lao động</h2>
            <button
              onClick={handleExportContractFile}
              className="px-4 py-2 bg-emerald-600 text-white rounded-lg text-sm font-medium hover:bg-emerald-700 transition"
            >
              📥 Xuất file Hợp đồng (.txt)
            </button>
          </div>

          <div className="p-6 bg-slate-950 text-slate-100 font-mono text-xs rounded-xl shadow border border-slate-800 whitespace-pre-wrap leading-relaxed">
            {contractPreviewText}
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 4: BULK IMPORT */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'import' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Import Ứng viên Hàng loạt (Bulk Candidate Import)</h2>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-foreground dark:text-slate-300">
              Nhập dữ liệu CSV (Họ tên, Email, Số điện thoại, Mã tin tuyển dụng):
            </label>
            <textarea
              rows={4}
              value={rawImportText}
              onChange={(e) => setRawImportText(e.target.value)}
              className="w-full p-3 font-mono text-xs border rounded-lg border-border dark:border-slate-700"
            />
            <button
              onClick={handleParseImport}
              className="px-4 py-2 bg-slate-800 text-white text-xs font-medium rounded-lg hover:bg-slate-900 transition"
            >
              🔍 Phân tích dữ liệu Import
            </button>
          </div>

          {importRows.length > 0 && (
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <h3 className="text-sm font-bold">Kết quả Phân tích ({importRows.length} dòng)</h3>
                <button
                  onClick={handleExecuteImport}
                  className="px-4 py-2 bg-brand text-white text-xs font-medium rounded-lg hover:bg-brand-hover transition"
                >
                  🚀 Tiến hành Import
                </button>
              </div>

              <div className="bg-surface shadow rounded-lg overflow-hidden border">
                <table className="w-full text-left text-xs">
                  <thead className="bg-slate-50 dark:bg-slate-800">
                    <tr>
                      <th className="p-2.5">Họ tên</th>
                      <th className="p-2.5">Email</th>
                      <th className="p-2.5">Số điện thoại</th>
                      <th className="p-2.5">Mã tin tuyển</th>
                      <th className="p-2.5">Trạng thái Validation</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y">
                    {importRows.map((item, idx) => (
                      <tr key={idx} className={item.error ? 'bg-rose-50/50 dark:bg-rose-950/20' : ''}>
                        <td className="p-2.5 font-semibold">{item.row.fullName}</td>
                        <td className="p-2.5">{item.row.email || '-'}</td>
                        <td className="p-2.5">{item.row.phone || '-'}</td>
                        <td className="p-2.5 font-mono">{item.row.jobPostingId}</td>
                        <td className="p-2.5">
                          {item.error ? (
                            <span className="text-rose-600 font-semibold">❌ {item.error}</span>
                          ) : (
                            <span className="text-emerald-600 font-semibold">✅ Hợp lệ</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {importResultSummary && (
            <div className="p-4 bg-brand-muted border border-brand/30 text-brand-strong text-xs rounded-xl space-y-1 font-medium">
              <p className="font-bold text-sm">🎉 Kết quả thực thi Import:</p>
              <p>• Tổng số ứng viên xử lý: {importResultSummary.total}</p>
              <p className="text-emerald-600 font-bold">• Thành công: {importResultSummary.success}</p>
              <p className="text-rose-600 font-bold">• Thất bại: {importResultSummary.failed}</p>
            </div>
          )}
        </div>
      )}

      {/* SKILL MODAL */}
      {isSkillModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSaveSkill} className="bg-surface rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">{skillForm.id ? 'Sửa kỹ năng' : 'Thêm kỹ năng nhân sự'}</h3>
            <div>
              <label className="text-xs font-semibold">Tên kỹ năng</label>
              <input
                type="text"
                value={skillForm.skillName}
                onChange={(e) => setSkillForm({ ...skillForm, skillName: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Cấp độ thành thạo</label>
              <select
                value={skillForm.level}
                onChange={(e) => setSkillForm({ ...skillForm, level: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              >
                <option value="Basic">Basic (Cơ bản)</option>
                <option value="Intermediate">Intermediate (Trung cấp)</option>
                <option value="Advanced">Advanced (Nâng cao)</option>
                <option value="Expert">Expert (Chuyên gia)</option>
              </select>
            </div>
            <div>
              <label className="text-xs font-semibold">Mã chứng chỉ / Bằng cấp</label>
              <input
                type="text"
                value={skillForm.cert}
                onChange={(e) => setSkillForm({ ...skillForm, cert: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsSkillModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-brand text-white rounded-lg text-sm font-medium">
                Lưu lại
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
