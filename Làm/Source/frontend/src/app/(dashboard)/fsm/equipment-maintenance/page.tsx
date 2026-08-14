'use client';

import React, { useState } from 'react';
import {
  formatFrequencyLabel,
  formatCompletionRate,
} from '@/shared/api/fsm-equipment-maintenance-helpers';

export default function FsmEquipmentMaintenancePage() {
  const [activeTab, setActiveTab] = useState<'schedules' | 'tickets' | 'checklists' | 'reports'>('schedules');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FSM_033: Lịch bảo trì theo thiết bị
  const [schedules, setSchedules] = useState([
    { id: 's-1', sn: 'SN-RACK-42U-00129', model: 'Tủ Rack Server Cao Cấp 42U', cust: 'Công Ty Viễn Thông Viettel', freq: 'Quarterly', due: '2026-09-15', auto: true },
    { id: 's-2', sn: 'SN-CNC-MILL-508', model: 'Máy Phay CNC 5 Trục Model Pro', cust: 'Tập Đoàn Cơ Khí FPT', freq: 'Monthly', due: '2026-08-20', auto: true },
  ]);

  // UC_FSM_034: Tự tạo ticket bảo trì đến hạn
  const [autoTickets, setAutoTickets] = useState([
    { id: 't-1', tck: 'TCK-MAINT-20260814-01', sn: 'SN-CNC-MILL-508', cust: 'Tập Đoàn Cơ Khí FPT', type: 'Bảo Trì Định Kỳ Tháng 8', date: '2026-08-20', status: 'Dispatched' },
  ]);

  const handleGenerateTicket = (sn: string, cust: string) => {
    const newTck = {
      id: 't-' + Date.now(),
      tck: 'TCK-MAINT-' + Math.floor(1000 + Math.random() * 9000),
      sn,
      cust,
      type: 'Bảo Trì Định Kỳ Theo Lịch',
      date: '2026-08-25',
      status: 'Dispatched',
    };
    setAutoTickets([...autoTickets, newTck]);
    showToast(`✓ Đã tạo tự động ticket [${newTck.tck}] cho thiết bị [${sn}]!`, 'success');
  };

  // UC_FSM_035: Checklist bảo trì chuẩn
  const [checklists] = useState([
    { id: 'cl-1', cat: 'Chiller & HVAC', item: '1. Kiểm tra rò rỉ gas và áp suất nén', sop: 'Dùng đồng hồ đo áp suất chuyên dụng', order: 1, mandatory: true },
    { id: 'cl-2', cat: 'Chiller & HVAC', item: '2. Vệ sinh dàn ngưng và màng lọc bụi', sop: 'Xịt rửa áp lực thấp và hóa chất làm sạch', order: 2, mandatory: true },
    { id: 'cl-3', cat: 'Chiller & HVAC', item: '3. Đo dòng tải động cơ quạt và máy nén', sop: 'Đo ampe kìm và đối chiếu định mức catalog', order: 3, mandatory: true },
  ]);

  // UC_FSM_036: Báo cáo thực hiện bảo trì
  const [report] = useState({
    period: 'Tháng 08/2026',
    totalVisits: 48,
    completed: 46,
    delayed: 2,
    rate: 95.8,
    revenue: 240000000,
  });

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
              FSM - EQUIPMENT MAINTENANCE, AUTO DUE TICKETS & SOP CHECKLISTS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Lịch Bảo Trì Thiết Bị, Tự Động Tạo Ticket Đến Hạn & Checklist Bảo Trì Chuẩn SOP</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Lập kế hoạch bảo dưỡng định kỳ, tự động sinh phiếu điều phối KTV khi đến hạn, số hóa tiêu chuẩn SOP và phân tích tỷ lệ hoàn thành
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs FSM)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('schedules')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'schedules' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📅 UC_FSM_033: Lịch Bảo Trì Thiết Bị
          </button>
          <button
            onClick={() => setActiveTab('tickets')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'tickets' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚡ UC_FSM_034: Ticket Đến Hạn Tự Tạo
          </button>
          <button
            onClick={() => setActiveTab('checklists')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'checklists' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📑 UC_FSM_035: Checklist SOP Chuẩn
          </button>
          <button
            onClick={() => setActiveTab('reports')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'reports' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_FSM_036: Báo Cáo Bảo Trì
          </button>
        </div>
      </div>

      {activeTab === 'schedules' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📅 Kế Hoạch & Lịch Bảo Trì Định Kỳ Theo Thiết Bị (UC_FSM_033)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Serial & Model</th>
                  <th className="p-3">Khách Hàng Sử Dụng</th>
                  <th className="p-3 text-center">Tần Suất Bảo Dưỡng</th>
                  <th className="p-3">Hạn Bảo Trì Kế Tiếp</th>
                  <th className="p-3 text-center">Tự Động Sinh Ticket?</th>
                  <th className="p-3 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {schedules.map((s) => (
                  <tr key={s.id} className="hover:bg-surface-hover/50">
                    <td className="p-3">
                      <div className="font-mono font-bold text-brand">{s.sn}</div>
                      <div className="text-xs text-muted-foreground">{s.model}</div>
                    </td>
                    <td className="p-3 font-semibold text-slate-800">{s.cust}</td>
                    <td className="p-3 text-center font-bold text-foreground">{formatFrequencyLabel(s.freq)}</td>
                    <td className="p-3 font-mono font-bold text-rose-700">{s.due}</td>
                    <td className="p-3 text-center">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Auto Ticket
                      </span>
                    </td>
                    <td className="p-3 text-right">
                      <button onClick={() => handleGenerateTicket(s.sn, s.cust)} className="px-3 py-1 bg-brand text-brand-foreground text-xs font-bold rounded hover:opacity-90">
                        ⚡ Tạo Ticket Ngay
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'tickets' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⚡ Danh Sách Ticket Bảo Trì Tự Động Phát Sinh Đến Hạn (UC_FSM_034)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Ticket Bảo Trì</th>
                  <th className="p-3">Thiết Bị & Serial</th>
                  <th className="p-3">Khách Hàng</th>
                  <th className="p-3">Hạng Mục Bảo Trì</th>
                  <th className="p-3">Ngày Lên Lịch Thực Hiện</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {autoTickets.map((t) => (
                  <tr key={t.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{t.tck}</td>
                    <td className="p-3 font-mono font-bold text-slate-800">{t.sn}</td>
                    <td className="p-3 font-semibold text-foreground">{t.cust}</td>
                    <td className="p-3 text-slate-700">{t.type}</td>
                    <td className="p-3 text-slate-700">{t.date}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-blue-100 text-blue-800 border border-blue-300">
                        ● Đã Điều Phối KTV
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'checklists' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📑 Bộ Tiêu Chuẩn Checklist Bảo Trì Chuẩn SOP (UC_FSM_035)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3 text-center">Thứ Tự</th>
                  <th className="p-3">Nhóm Thiết Bị</th>
                  <th className="p-3">Nội Dung Kiểm Tra / Bảo Dưỡng</th>
                  <th className="p-3">Quy Trình Chuẩn (SOP)</th>
                  <th className="p-3 text-right">Bắt Buộc?</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {checklists.map((c) => (
                  <tr key={c.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 text-center font-bold text-slate-700">{c.order}</td>
                    <td className="p-3 font-semibold text-brand">{c.cat}</td>
                    <td className="p-3 font-bold text-foreground">{c.item}</td>
                    <td className="p-3 text-slate-700 text-xs">{c.sop}</td>
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

      {activeTab === 'reports' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <h2 className="text-lg font-bold text-foreground">📊 Báo Cáo Thực Hiện Kế Hoạch Bảo Trì Định Kỳ (UC_FSM_036)</h2>
          <div className="grid grid-cols-4 gap-4">
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Tổng Lượt Kế Hoạch</div>
              <div className="text-2xl font-black text-foreground mt-1">{report.totalVisits} lượt</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Đã Hoàn Thành</div>
              <div className="text-2xl font-black text-emerald-700 mt-1">{report.completed} lượt</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Tỷ Lệ Đúng Hạn</div>
              <div className="text-2xl font-black text-brand mt-1">{formatCompletionRate(report.rate)}</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Doanh Thu Bảo Trì</div>
              <div className="text-2xl font-black text-blue-700 mt-1">{report.revenue.toLocaleString('vi-VN')} đ</div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
