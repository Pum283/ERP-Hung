'use client';

import React, { useState } from 'react';
import {
  validateShiftImportRow,
  normalizePenaltyType,
  calculatePayrollPenaltyTotal,
  generateFinJePreview,
  ShiftImportRow,
  PenaltyItem,
} from '@/shared/api/hrm-shift-import-helpers';

export default function HrmShiftImportPage() {
  const [activeTab, setActiveTab] = useState<'shiftImport' | 'penalties' | 'applyPayroll' | 'finSync'>('shiftImport');

  // Toast notifications
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: SHIFT IMPORT (UC_HRM_088)
  // ────────────────────────────────────────────────────────────────────────────
  const [rawShiftText, setRawShiftText] = useState(
    `EMP001,HC_01,2026-08-15,Ca hành chính\nEMP002,HC_01,2026-08-15,Ca hành chính\nEMP003,CA_DEM,15/08/2026,Sai định dạng ngày`
  );
  const [shiftRows, setShiftRows] = useState<{ row: ShiftImportRow; error?: string }[]>([]);
  const [shiftResultSummary, setShiftResultSummary] = useState<{ total: number; success: number; failed: number } | null>(null);

  const handleParseShifts = () => {
    const lines = rawShiftText.split('\n').filter((l) => l.trim().length > 0);
    const parsed = lines.map((line) => {
      const parts = line.split(',').map((p) => p.trim());
      const row: ShiftImportRow = {
        employeeCode: parts[0] || '',
        workShiftCode: parts[1] || '',
        workDate: parts[2] || '',
        note: parts[3] || null,
      };
      const val = validateShiftImportRow(row);
      return { row, error: val.isValid ? undefined : val.error };
    });
    setShiftRows(parsed);
    setShiftResultSummary(null);
    showToast(`Đã phân tích ${parsed.length} dòng phân ca.`);
  };

  const handleExecuteShiftImport = () => {
    const valid = shiftRows.filter((r) => !r.error);
    const invalid = shiftRows.filter((r) => r.error);
    setShiftResultSummary({
      total: shiftRows.length,
      success: valid.length,
      failed: invalid.length,
    });
    showToast(`Import lịch ca hoàn tất! Thành công: ${valid.length}, Thất bại: ${invalid.length}`);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: PAYROLL PENALTIES (UC_HRM_124)
  // ────────────────────────────────────────────────────────────────────────────
  const [penalties, setPenalties] = useState<PenaltyItem[]>([
    { id: 'p-1', employeeId: 'emp-101', reason: 'Đi trễ 45 phút ngày 05/08', penaltyType: 'LateArrival', amount: 100000, status: 'Pending' },
    { id: 'p-2', employeeId: 'emp-102', reason: 'Không đeo thẻ nhân viên', penaltyType: 'RegulationBreach', amount: 200000, status: 'Pending' },
    { id: 'p-3', employeeId: 'emp-101', reason: 'Về sớm không xin phép', penaltyType: 'EarlyLeave', amount: 150000, status: 'Applied' },
  ]);
  const [penaltyForm, setPenaltyForm] = useState<{ id?: string; employeeId: string; reason: string; penaltyType: string; amount: number }>({
    employeeId: 'emp-101',
    reason: '',
    penaltyType: 'LateArrival',
    amount: 100000,
  });
  const [isPenaltyModalOpen, setIsPenaltyModalOpen] = useState(false);

  const handleSavePenalty = (e: React.FormEvent) => {
    e.preventDefault();
    if (!penaltyForm.reason.trim()) {
      showToast('Lý do phạt không được để trống', 'error');
      return;
    }
    if (penaltyForm.amount < 0) {
      showToast('Số tiền phạt phải lớn hơn hoặc bằng 0', 'error');
      return;
    }

    const norm = normalizePenaltyType(penaltyForm.penaltyType);

    if (penaltyForm.id) {
      setPenalties((prev) =>
        prev.map((p) =>
          p.id === penaltyForm.id
            ? { ...p, reason: penaltyForm.reason, penaltyType: norm.normalized, amount: penaltyForm.amount }
            : p
        )
      );
      showToast('Cập nhật phiếu phạt thành công!');
    } else {
      setPenalties((prev) => [
        ...prev,
        {
          id: `p-${Date.now()}`,
          employeeId: penaltyForm.employeeId,
          reason: penaltyForm.reason,
          penaltyType: norm.normalized,
          amount: penaltyForm.amount,
          status: 'Pending',
        },
      ]);
      showToast('Tạo phiếu phạt mới thành công!');
    }
    setIsPenaltyModalOpen(false);
  };

  const handleDeletePenalty = (id: string) => {
    const item = penalties.find((p) => p.id === id);
    if (item?.status === 'Applied') {
      showToast('Không thể xóa phiếu phạt đã được áp dụng vào kỳ lương', 'error');
      return;
    }
    setPenalties((prev) => prev.filter((p) => p.id !== id));
    showToast('Đã xóa phiếu phạt.');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: APPLY PENALTIES TO PAYROLL (UC_HRM_125)
  // ────────────────────────────────────────────────────────────────────────────
  const [selectedPeriod, setSelectedPeriod] = useState('2026-08');
  const [selectedPenaltyIds, setSelectedPenaltyIds] = useState<string[]>([]);

  const pendingPenalties = penalties.filter((p) => p.status === 'Pending');

  const toggleSelectPenalty = (id: string) => {
    setSelectedPenaltyIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));
  };

  const handleApplyPenalties = () => {
    if (selectedPenaltyIds.length === 0) {
      showToast('Chưa chọn phiếu phạt nào để áp dụng', 'error');
      return;
    }

    setPenalties((prev) =>
      prev.map((p) => (selectedPenaltyIds.includes(p.id) ? { ...p, status: 'Applied' } : p))
    );
    showToast(`Đã áp dụng ${selectedPenaltyIds.length} phiếu phạt vào Kỳ lương ${selectedPeriod}!`);
    setSelectedPenaltyIds([]);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: SYNC PAYROLL FIN (UC_HRM_174)
  // ────────────────────────────────────────────────────────────────────────────
  const [grossSalary] = useState(125000000);
  const [netSalary] = useState(115000000);
  const appliedPenaltiesTotal = calculatePayrollPenaltyTotal(penalties.filter((p) => p.status === 'Applied'));
  const finJePreview = generateFinJePreview(grossSalary, netSalary, appliedPenaltiesTotal);

  const [syncedJeCode, setSyncedJeCode] = useState<string | null>(null);

  const handleExecuteFinSync = () => {
    const code = `JE-PY-${selectedPeriod.replace('-', '')}-${Date.now().toString().slice(-6)}`;
    setSyncedJeCode(code);
    showToast(`Đồng bộ bút toán lương thành công! Mã chứng từ: ${code}`);
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
            Bước 158: Import Lịch ca, Lập phiếu phạt, Khấu trừ lương & Bút toán FIN
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            Import phân ca (UC_HRM_088), Lập phiếu phạt (UC_HRM_124), Áp dụng vào kỳ lương (UC_HRM_125) & Đồng bộ Sổ cái Kế toán FIN (UC_HRM_174).
          </p>
        </div>
        <div>
          <span className="px-3 py-1 bg-indigo-100 text-indigo-800 dark:bg-indigo-950 dark:text-indigo-300 rounded-full text-xs font-semibold">
            Tiến độ Kế hoạch: 90% [XONG]
          </span>
        </div>
      </div>

      {/* Toast alert */}
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
          { key: 'shiftImport', label: '1. Import Lịch ca Excel (UC_HRM_088)' },
          { key: 'penalties', label: '2. Bảng Phạt Nhân sự (UC_HRM_124)' },
          { key: 'applyPayroll', label: '3. Khấu trừ Kỳ lương (UC_HRM_125)' },
          { key: 'finSync', label: '4. Bút toán Kế toán FIN (UC_HRM_174)' },
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
      {/* TAB 1: SHIFT IMPORT */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'shiftImport' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Import Phân Ca Làm Việc Hàng Loạt</h2>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-slate-700 dark:text-slate-300">
              Nhập dữ liệu CSV (Mã NV, Mã Ca, Ngày YYYY-MM-DD, Ghi chú):
            </label>
            <textarea
              rows={4}
              value={rawShiftText}
              onChange={(e) => setRawShiftText(e.target.value)}
              className="w-full p-3 font-mono text-xs border rounded-lg dark:bg-slate-900 border-slate-300 dark:border-slate-700"
            />
            <button
              onClick={handleParseShifts}
              className="px-4 py-2 bg-slate-800 text-white text-xs font-medium rounded-lg hover:bg-slate-900 transition"
            >
              🔍 Phân tích dữ liệu Phân ca
            </button>
          </div>

          {shiftRows.length > 0 && (
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <h3 className="text-sm font-bold">Kết quả Phân tích ({shiftRows.length} dòng)</h3>
                <button
                  onClick={handleExecuteShiftImport}
                  className="px-4 py-2 bg-indigo-600 text-white text-xs font-medium rounded-lg hover:bg-indigo-700 transition"
                >
                  🚀 Thực thi Import Lịch ca
                </button>
              </div>

              <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border">
                <table className="w-full text-left text-xs">
                  <thead className="bg-slate-50 dark:bg-slate-800">
                    <tr>
                      <th className="p-2.5">Mã NV</th>
                      <th className="p-2.5">Mã Ca</th>
                      <th className="p-2.5">Ngày làm việc</th>
                      <th className="p-2.5">Ghi chú</th>
                      <th className="p-2.5">Trạng thái Validation</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y">
                    {shiftRows.map((item, idx) => (
                      <tr key={idx} className={item.error ? 'bg-rose-50/50 dark:bg-rose-950/20' : ''}>
                        <td className="p-2.5 font-mono font-bold text-indigo-600">{item.row.employeeCode}</td>
                        <td className="p-2.5 font-mono">{item.row.workShiftCode}</td>
                        <td className="p-2.5">{item.row.workDate}</td>
                        <td className="p-2.5">{item.row.note || '-'}</td>
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

          {shiftResultSummary && (
            <div className="p-4 bg-indigo-50 border border-indigo-200 text-indigo-900 text-xs rounded-xl space-y-1 font-medium">
              <p className="font-bold text-sm">🎉 Kết quả phân ca hàng loạt:</p>
              <p>• Tổng phân ca xử lý: {shiftResultSummary.total}</p>
              <p className="text-emerald-600 font-bold">• Phân ca thành công: {shiftResultSummary.success}</p>
              <p className="text-rose-600 font-bold">• Thất bại: {shiftResultSummary.failed}</p>
            </div>
          )}
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 2: PENALTIES */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'penalties' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Danh sách Bảng phạt Nhân sự</h2>
            <button
              onClick={() => {
                setPenaltyForm({ employeeId: 'emp-101', reason: '', penaltyType: 'LateArrival', amount: 100000 });
                setIsPenaltyModalOpen(true);
              }}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition"
            >
              + Lập phiếu phạt mới
            </button>
          </div>

          <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border border-slate-200 dark:border-slate-800">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-300">
                <tr>
                  <th className="p-3">Lý do phạt</th>
                  <th className="p-3">Loại vi phạm</th>
                  <th className="p-3">Số tiền phạt (VNĐ)</th>
                  <th className="p-3">Trạng thái</th>
                  <th className="p-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                {penalties.map((p) => (
                  <tr key={p.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                    <td className="p-3 font-semibold">{p.reason}</td>
                    <td className="p-3 font-mono text-xs text-slate-500">{p.penaltyType}</td>
                    <td className="p-3 font-bold text-rose-600">-{p.amount.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3">
                      {p.status === 'Applied' ? (
                        <span className="px-2.5 py-0.5 text-xs rounded font-bold bg-sky-100 text-sky-800">Đã trừ lương</span>
                      ) : p.status === 'Pending' ? (
                        <span className="px-2.5 py-0.5 text-xs rounded font-bold bg-amber-100 text-amber-800">Chờ áp dụng</span>
                      ) : (
                        <span className="px-2.5 py-0.5 text-xs rounded font-bold bg-slate-100 text-slate-600">Đã hủy</span>
                      )}
                    </td>
                    <td className="p-3 text-right space-x-2">
                      {p.status !== 'Applied' && (
                        <>
                          <button
                            onClick={() => {
                              setPenaltyForm({ id: p.id, employeeId: p.employeeId, reason: p.reason, penaltyType: p.penaltyType, amount: p.amount });
                              setIsPenaltyModalOpen(true);
                            }}
                            className="text-xs text-indigo-600 hover:underline"
                          >
                            Sửa
                          </button>
                          <button onClick={() => handleDeletePenalty(p.id)} className="text-xs text-rose-600 hover:underline">
                            Xóa
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 3: APPLY TO PAYROLL */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'applyPayroll' && (
        <div className="space-y-4">
          <div className="flex justify-between items-center border-b pb-3">
            <div className="flex items-center gap-4">
              <h2 className="text-lg font-semibold">Khấu trừ Phiếu phạt vào Kỳ lương</h2>
              <select
                value={selectedPeriod}
                onChange={(e) => setSelectedPeriod(e.target.value)}
                className="p-2 border rounded-lg text-xs font-bold dark:bg-slate-800"
              >
                <option value="2026-08">Kỳ lương 08/2026</option>
                <option value="2026-09">Kỳ lương 09/2026</option>
              </select>
            </div>
            <button
              onClick={handleApplyPenalties}
              disabled={selectedPenaltyIds.length === 0}
              className="px-4 py-2 bg-rose-600 text-white rounded-lg text-sm font-medium hover:bg-rose-700 transition disabled:opacity-50"
            >
              ⚡ Áp dụng {selectedPenaltyIds.length} phạt vào kỳ lương
            </button>
          </div>

          {pendingPenalties.length === 0 ? (
            <div className="p-8 text-center bg-slate-50 dark:bg-slate-900 rounded-xl text-slate-500 text-sm border">
              🎉 Không có phiếu phạt nào đang chờ áp dụng.
            </div>
          ) : (
            <div className="bg-white dark:bg-slate-900 shadow rounded-lg overflow-hidden border border-slate-200 dark:border-slate-800">
              <table className="w-full text-left text-sm">
                <thead className="bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-300">
                  <tr>
                    <th className="p-3 w-10">Chọn</th>
                    <th className="p-3">Lý do phạt</th>
                    <th className="p-3">Loại vi phạm</th>
                    <th className="p-3">Số tiền phạt</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-200 dark:divide-slate-800">
                  {pendingPenalties.map((p) => (
                    <tr key={p.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/50">
                      <td className="p-3">
                        <input
                          type="checkbox"
                          checked={selectedPenaltyIds.includes(p.id)}
                          onChange={() => toggleSelectPenalty(p.id)}
                          className="rounded border-slate-300"
                        />
                      </td>
                      <td className="p-3 font-semibold">{p.reason}</td>
                      <td className="p-3 font-mono text-xs text-slate-500">{p.penaltyType}</td>
                      <td className="p-3 font-bold text-rose-600">-{p.amount.toLocaleString('vi-VN')} đ</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {/* TAB 4: FIN JOURNAL SYNC */}
      {/* ──────────────────────────────────────────────────────────────────────────── */}
      {activeTab === 'finSync' && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-semibold">Đồng bộ Bút toán Lương tự động sang Kế toán FIN (GL)</h2>
            <button
              onClick={handleExecuteFinSync}
              className="px-5 py-2.5 bg-emerald-600 text-white font-semibold rounded-lg text-sm hover:bg-emerald-700 transition"
            >
              🔄 Sinh & Đẩy Bút toán Sổ cái FIN
            </button>
          </div>

          <div className="p-6 bg-slate-900 text-slate-100 rounded-xl space-y-4 shadow border border-slate-800 font-mono text-xs">
            <h3 className="text-sm font-bold text-emerald-400">📊 PREVIEW BÚT TOÁN KẾ TOÁN LƯƠNG (BALANCED JOURNAL ENTRY)</h3>
            <div className="space-y-2 border-t border-slate-800 pt-3">
              <p className="text-indigo-300 font-semibold">[NỢ] {finJePreview.debitAccount}</p>
              <p className="text-emerald-300 font-semibold">[CÓ] {finJePreview.creditAccountSalary}</p>
              <p className="text-amber-300 font-semibold">[CÓ] {finJePreview.creditAccountPenalty}</p>
            </div>

            {syncedJeCode && (
              <div className="mt-4 p-3 bg-emerald-950/60 border border-emerald-500/50 rounded-lg text-emerald-300">
                ✅ Đã đồng bộ thành công sang FIN! Mã chứng từ FIN: <strong>{syncedJeCode}</strong>
              </div>
            )}
          </div>
        </div>
      )}

      {/* PENALTY MODAL */}
      {isPenaltyModalOpen && (
        <div className="fixed inset-0 bg-slate-900/50 flex items-center justify-center p-4 z-50">
          <form onSubmit={handleSavePenalty} className="bg-white dark:bg-slate-900 rounded-xl p-6 max-w-md w-full space-y-4 shadow-xl">
            <h3 className="text-lg font-bold">{penaltyForm.id ? 'Sửa phiếu phạt' : 'Lập phiếu phạt mới'}</h3>
            <div>
              <label className="text-xs font-semibold">Lý do phạt</label>
              <input
                type="text"
                value={penaltyForm.reason}
                onChange={(e) => setPenaltyForm({ ...penaltyForm, reason: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div>
              <label className="text-xs font-semibold">Loại vi phạm</label>
              <select
                value={penaltyForm.penaltyType}
                onChange={(e) => setPenaltyForm({ ...penaltyForm, penaltyType: e.target.value })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
              >
                <option value="LateArrival">Đi trễ (LateArrival)</option>
                <option value="EarlyLeave">Về sớm (EarlyLeave)</option>
                <option value="RegulationBreach">Vi phạm nội quy (RegulationBreach)</option>
                <option value="SafetyViolation">Vi phạm an toàn (SafetyViolation)</option>
                <option value="Other">Khác (Other)</option>
              </select>
            </div>
            <div>
              <label className="text-xs font-semibold">Số tiền phạt (VNĐ)</label>
              <input
                type="number"
                min="0"
                value={penaltyForm.amount}
                onChange={(e) => setPenaltyForm({ ...penaltyForm, amount: Number(e.target.value) })}
                className="w-full mt-1 p-2 border rounded-lg text-sm dark:bg-slate-800"
                required
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => setIsPenaltyModalOpen(false)} className="px-4 py-2 border rounded-lg text-sm">
                Hủy
              </button>
              <button type="submit" className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium">
                Lưu lại
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
