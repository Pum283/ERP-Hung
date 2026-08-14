'use client';

import React, { useState } from 'react';
import {
  formatProgressPercent,
  formatGanttStatusBadge,
} from '@/shared/api/pjm-checklist-gantt-plan-change-helpers';

export default function PjmChecklistGanttPlanChangePage() {
  const [activeTab, setActiveTab] = useState<'checklist' | 'gantt' | 'changes'>('checklist');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PJM_003: Mẫu checklist nghiệm thu
  const [templates] = useState([
    { id: 't-1', code: 'TMPL-ACCEPT-MECH', name: 'Nghiệm Thu Hệ Thống Cơ Điện (M&E)', cat: 'Thi Công Lắp Đặt', item: '1. Kiểm tra đấu nối dây tiếp địa và điện trở đất < 4 Ohm', order: 1, mandatory: true },
    { id: 't-2', code: 'TMPL-ACCEPT-MECH', name: 'Nghiệm Thu Hệ Thống Cơ Điện (M&E)', cat: 'Thi Công Lắp Đặt', item: '2. Chạy thử liên động không tải máy phát và ATS trong 60 phút', order: 2, mandatory: true },
    { id: 't-3', code: 'TMPL-ACCEPT-MECH', name: 'Nghiệm Thu Hệ Thống Cơ Điện (M&E)', cat: 'Thi Công Lắp Đặt', item: '3. Nghiệm thu tải giả 100% công suất liên tục 2 giờ', order: 3, mandatory: true },
  ]);

  // UC_PJM_016: Gantt / timeline tiến độ
  const [milestones] = useState([
    { id: 'm-1', code: 'MS-01', name: 'Khảo sát hiện trường & hoàn tất bản vẽ Shop-drawing', start: '2026-08-01', end: '2026-08-10', progress: 100, pred: 'None', status: 'Completed' },
    { id: 'm-2', code: 'MS-02', name: 'Thi công kéo rải tuyến cáp ngầm trung thế 22kV', start: '2026-08-11', end: '2026-08-25', progress: 65, pred: 'MS-01', status: 'InProgress' },
    { id: 'm-3', code: 'MS-03', name: 'Lắp đặt máy biến áp & tủ phân phối tổng MSB', start: '2026-08-26', end: '2026-09-05', progress: 15, pred: 'MS-02', status: 'InProgress' },
    { id: 'm-4', code: 'MS-04', name: 'Đóng điện chạy thử & ký biên bản nghiệm thu bàn giao', start: '2026-09-06', end: '2026-09-12', progress: 0, pred: 'MS-03', status: 'Planned' },
  ]);

  // UC_PJM_018: Nhật ký thay đổi kế hoạch
  const [changeLogs] = useState([
    { id: 'ch-1', prj: 'PRJ-2026-088', title: 'Gia hạn thêm 7 ngày do nhà máy cắt điện nguồn', reason: 'Khách hàng yêu cầu dừng thi công để nghiệm thu PCCC nội bộ', by: 'PM Nguyễn Văn Tuấn', status: 'Approved', date: '2026-08-12' },
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
              PJM - ACCEPTANCE CHECKLIST TEMPLATES, GANTT MILESTONES & PLAN AUDIT
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Mẫu Checklist Nghiệm Thu Dự Án, Gantt Tiến Độ & Nhật Ký Điều Chỉnh Kế Hoạch</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Chuẩn hóa tiêu chí bàn giao nghiệm thu, quản lý đường găng tiến độ Gantt milestones và lưu vết phê duyệt thay đổi baseline dự án
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (3/3 UCs PJM)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('checklist')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'checklist' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📋 UC_PJM_003: Mẫu Checklist Nghiệm Thu
          </button>
          <button
            onClick={() => setActiveTab('gantt')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'gantt' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_PJM_016: Gantt Timeline Tiến Độ
          </button>
          <button
            onClick={() => setActiveTab('changes')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'changes' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📝 UC_PJM_018: Nhật Ký Đổi Kế Hoạch
          </button>
        </div>
      </div>

      {activeTab === 'checklist' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📋 Bộ Mẫu Tiêu Chí Nghiệm Thu Bàn Giao Dự Án (UC_PJM_003)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3 text-center">STT</th>
                  <th className="p-3">Mã Mẫu Checklist</th>
                  <th className="p-3">Tên Mẫu & Nhóm Dự Án</th>
                  <th className="p-3">Nội Dung Tiêu Chí Nghiệm Thu</th>
                  <th className="p-3 text-right">Bắt Buộc?</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {templates.map((t) => (
                  <tr key={t.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 text-center font-bold text-slate-700">{t.order}</td>
                    <td className="p-3 font-mono font-bold text-brand">{t.code}</td>
                    <td className="p-3">
                      <div className="font-semibold text-foreground">{t.name}</div>
                      <div className="text-xs text-muted-foreground">{t.cat}</div>
                    </td>
                    <td className="p-3 text-slate-800 font-medium">{t.item}</td>
                    <td className="p-3 text-right">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-rose-100 text-rose-800">
                        * Bắt buộc
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'gantt' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📊 Đường Găng Tiến Độ & Mốc Gantt Milestones (UC_PJM_016)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Mốc</th>
                  <th className="p-3">Tên Giai Đoạn / Hạng Mục Dự Án</th>
                  <th className="p-3">Bắt Đầu</th>
                  <th className="p-3">Kết Thúc</th>
                  <th className="p-3 text-center">Mốc Phụ Thuộc</th>
                  <th className="p-3">Tiến Độ (%)</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {milestones.map((m) => (
                  <tr key={m.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{m.code}</td>
                    <td className="p-3 font-semibold text-foreground">{m.name}</td>
                    <td className="p-3 text-slate-700">{m.start}</td>
                    <td className="p-3 text-slate-700">{m.end}</td>
                    <td className="p-3 text-center font-mono text-xs font-bold text-slate-700">{m.pred}</td>
                    <td className="p-3">
                      <div className="flex items-center space-x-2">
                        <div className="w-24 bg-surface-hover rounded-full h-2.5 overflow-hidden border border-border">
                          <div className="bg-brand h-2.5 rounded-full" style={{ width: `${m.progress}%` }}></div>
                        </div>
                        <span className="font-bold text-xs text-foreground">{formatProgressPercent(m.progress)}</span>
                      </div>
                    </td>
                    <td className="p-3 text-right">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${formatGanttStatusBadge(m.status)}`}>
                        {m.status === 'Completed' ? '✓ Hoàn Thành' : m.status === 'InProgress' ? '▶ Đang Thi Công' : '○ Lên Kế Hoạch'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'changes' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📝 Nhật Ký Điều Chỉnh Kế Hoạch & Đường Cơ Sở Baseline Dự Án (UC_PJM_018)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Dự Án</th>
                  <th className="p-3">Nội Dung Điều Chỉnh</th>
                  <th className="p-3">Lý Do / Nguyên Nhân</th>
                  <th className="p-3">Người Đề Xuất</th>
                  <th className="p-3">Ngày Đề Xuất</th>
                  <th className="p-3 text-right">Phê Duyệt</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {changeLogs.map((ch) => (
                  <tr key={ch.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{ch.prj}</td>
                    <td className="p-3 font-semibold text-foreground">{ch.title}</td>
                    <td className="p-3 text-xs text-slate-700">{ch.reason}</td>
                    <td className="p-3 text-slate-800 font-medium">{ch.by}</td>
                    <td className="p-3 text-slate-700">{ch.date}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Đã Phê Duyệt
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
