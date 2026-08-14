'use client';

import React, { useState } from 'react';
import {
  formatTotalHours,
  formatOverrunPercent,
} from '@/shared/api/pjm-timesheet-budget-checklist-helpers';

export default function PjmTimesheetBudgetChecklistPage() {
  const [activeTab, setActiveTab] = useState<'timesheet' | 'budget' | 'survey' | 'installation'>('timesheet');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PJM_020: Timesheet theo dự án
  const [timesheets] = useState([
    { id: 'ts-1', prj: 'PRJ-2026-088', emp: 'KS. Nguyễn Văn Hùng', task: 'Đấu nối tủ biến áp và chạy thử ATS', regular: 8, ot: 2, status: 'Approved', date: '2026-08-14' },
    { id: 'ts-2', prj: 'PRJ-2026-088', emp: 'KTV. Lê Hoàng Nam', task: 'Kéo cáp nguồn trục chính 3P+N', regular: 8, ot: 0, status: 'Approved', date: '2026-08-14' },
  ]);

  // UC_PJM_024: Cảnh báo vượt ngân sách
  const [warnings] = useState([
    { id: 'w-1', prj: 'PRJ-2026-088', name: 'Hệ thống trạm biến áp và tủ phân phối tổng', budget: 500000000, committed: 530000000, overrun: 30000000, pct: 6.0, severity: 'Warning' },
  ]);

  // UC_PJM_025: Checklist khảo sát
  const [surveys] = useState([
    { id: 's-1', prj: 'PRJ-2026-088', title: '1. Kiểm tra tải trọng sàn đặt trạm biến áp', standard: 'Tải trọng tối thiểu 1.500 kg/m2', pass: true, notes: 'Sàn bê tông cốt thép đạt yêu cầu thiết kế' },
    { id: 's-2', prj: 'PRJ-2026-088', title: '2. Đo đạc khoảng cách an toàn hành lang điện', standard: 'Khoảng cách thông thủy tối thiểu 1.2m', pass: true, notes: 'Đạt khoảng cách an toàn 1.5m' },
  ]);

  // UC_PJM_026: Checklist lắp đặt
  const [installs] = useState([
    { id: 'i-1', prj: 'PRJ-2026-088', step: '1. Siết bu lông chân máy biến áp theo lực siết 120 N.m', tag: 'TRANS-2000KVA', done: true, signer: 'KS. Trần Quốc Toản' },
    { id: 'i-2', prj: 'PRJ-2026-088', step: '2. Đo điện trở cách điện cuộn sơ cấp và thứ cấp', tag: 'TRANS-2000KVA', done: true, signer: 'KS. Nguyễn Văn Hùng' },
  ]);

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      {toast && (
        <div className={`fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white font-medium text-sm ${toast.type === 'success' ? 'bg-emerald-600' : 'bg-rose-600'}`}>
          {toast.message}
        </div>
      )}

      <div className="bg-surface border border-border p-6 rounded-2xl shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-brand-muted text-brand-strong text-xs px-3 py-1 rounded-full font-semibold border border-brand/30">
              PJM - TIMESHEETS, BUDGET OVERRUN & SITE TECHNICAL CHECKLISTS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Timesheet Dự Án, Cảnh Báo Vượt Ngân Sách, Checklist Khảo Sát & Lắp Đặt Hiện Trường</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Chấm công nhật ký thi công dự án, theo dõi vượt ngân sách committed cost, số hóa biên bản khảo sát mặt bằng và checklist tiêu chuẩn kỹ thuật lắp đặt
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs PJM)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('timesheet')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'timesheet' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⏱️ UC_PJM_020: Timesheet Dự Án
          </button>
          <button
            onClick={() => setActiveTab('budget')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'budget' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚠️ UC_PJM_024: Cảnh Báo Vượt Ngân Sách
          </button>
          <button
            onClick={() => setActiveTab('survey')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'survey' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📐 UC_PJM_025: Checklist Khảo Sát
          </button>
          <button
            onClick={() => setActiveTab('installation')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'installation' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔩 UC_PJM_026: Checklist Lắp Đặt
          </button>
        </div>
      </div>

      {activeTab === 'timesheet' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⏱️ Nhật Ký Giờ Công Lao Động Theo Dự Án (UC_PJM_020)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Dự Án</th>
                  <th className="p-3">Nhân Sự Thi Công</th>
                  <th className="p-3">Hạng Mục Công Việc</th>
                  <th className="p-3 text-right">Tổng Giờ (Giờ HC + Tăng Ca)</th>
                  <th className="p-3 text-center">Ngày Làm Việc</th>
                  <th className="p-3 text-right">Phê Duyệt</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {timesheets.map((ts) => (
                  <tr key={ts.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{ts.prj}</td>
                    <td className="p-3 font-bold text-foreground">{ts.emp}</td>
                    <td className="p-3 text-slate-800 font-medium">{ts.task}</td>
                    <td className="p-3 text-right font-black text-brand text-base">{formatTotalHours(ts.regular, ts.ot)}</td>
                    <td className="p-3 text-center text-slate-700">{ts.date}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Đã Duyệt (PM)
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'budget' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⚠️ Cảnh Báo Vượt Hạn Mức Ngân Sách Dự Án (UC_PJM_024)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Dự Án</th>
                  <th className="p-3 text-right">Ngân Sách Được Duyệt</th>
                  <th className="p-3 text-right">Chi Phí Đã Cam Kết</th>
                  <th className="p-3 text-right">Số Tiền Vượt</th>
                  <th className="p-3 text-center">Tỷ Lệ Vượt</th>
                  <th className="p-3 text-right">Mức Độ Cảnh Báo</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {warnings.map((w) => (
                  <tr key={w.id} className="hover:bg-surface-hover/50">
                    <td className="p-3">
                      <div className="font-bold text-foreground">{w.name}</div>
                      <div className="font-mono text-xs text-brand">{w.prj}</div>
                    </td>
                    <td className="p-3 text-right font-medium text-slate-700">{w.budget.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-bold text-rose-700">{w.committed.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-black text-rose-800">+{w.overrun.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-center font-black text-rose-700">{formatOverrunPercent(w.pct)}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-amber-100 text-amber-800 border border-amber-300">
                        ● Cần Điều Chỉnh Baseline
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'survey' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📐 Biên Bản Khảo Sát Hiện Trường Dự Án (UC_PJM_025)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Dự Án</th>
                  <th className="p-3">Tiêu Trí Khảo Sát Hiện Trường</th>
                  <th className="p-3">Yêu Cầu Tiêu Chuẩn Kỹ Thuật</th>
                  <th className="p-3">Ghi Chú Kỹ Sư Giám Sát</th>
                  <th className="p-3 text-right">Kết Quả Đánh Giá</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {surveys.map((s) => (
                  <tr key={s.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{s.prj}</td>
                    <td className="p-3 font-bold text-foreground">{s.title}</td>
                    <td className="p-3 text-slate-700 text-xs">{s.standard}</td>
                    <td className="p-3 text-slate-800 font-medium">{s.notes}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Đạt Yêu Cầu
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'installation' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🔩 Checklist Nghiệm Thu Công Đoạn Lắp Đặt (UC_PJM_026)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Dự Án</th>
                  <th className="p-3">Công Đoạn Kỹ Thuật Lắp Đặt</th>
                  <th className="p-3">Mã Thiết Bị (Tag)</th>
                  <th className="p-3">Kỹ Sư Trách Nhiệm Ký Tên</th>
                  <th className="p-3 text-right">Trạng Thái Thực Hiện</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {installs.map((i) => (
                  <tr key={i.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{i.prj}</td>
                    <td className="p-3 font-bold text-foreground">{i.step}</td>
                    <td className="p-3 font-mono font-bold text-slate-800">{i.tag}</td>
                    <td className="p-3 text-slate-800 font-medium">{i.signer}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Đã Hoàn Thành
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
