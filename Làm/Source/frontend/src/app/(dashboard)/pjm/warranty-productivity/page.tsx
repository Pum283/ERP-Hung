'use client';

import React, { useState } from 'react';
import {
  formatWarrantyPeriod,
  formatUtilizationPercent,
} from '@/shared/api/pjm-warranty-productivity-helpers';

export default function PjmWarrantyProductivityPage() {
  const [activeTab, setActiveTab] = useState<'warranty' | 'productivity'>('warranty');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PJM_037: Bảo hành sau dự án
  const [warranties] = useState([
    { id: 'w-1', prj: 'PRJ-2026-088', cust: 'Công Ty Viễn Thông Viettel', start: '2026-08-14', end: '2028-08-14', months: 24, hotline: '1900-8888', active: true },
    { id: 'w-2', prj: 'PRJ-2026-065', cust: 'Tập Đoàn Cơ Khí FPT', start: '2026-02-10', end: '2027-02-10', months: 12, hotline: '1900-8888', active: true },
  ]);

  // UC_PJM_041: Năng suất nguồn lực
  const [report] = useState({
    period: 'Tháng 08/2026',
    engineers: 18,
    allocated: 2880,
    timesheet: 2650,
    utilization: 92.0,
    outputPerEng: 125000000,
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
              PJM - POST-PROJECT WARRANTY & RESOURCE PRODUCTIVITY
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Bảo Hành Sau Dự Án & Báo Cáo Hiệu Suất Năng Suất Nguồn Lực Kỹ Sư</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý thời hạn cam kết bảo hành hậu dự án, hotline hỗ trợ kỹ thuật và phân tích tỷ lệ tận dụng nguồn lực kỹ sư (Resource Utilization Rate)
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (2/2 UCs PJM)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('warranty')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'warranty' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🛡️ UC_PJM_037: Bảo Hành Sau Dự Án
          </button>
          <button
            onClick={() => setActiveTab('productivity')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'productivity' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_PJM_041: Năng Suất Nguồn Lực
          </button>
        </div>
      </div>

      {activeTab === 'warranty' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🛡️ Cam Kết & Thời Hạn Bảo Hành Sau Bàn Giao Dự Án (UC_PJM_037)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Dự Án</th>
                  <th className="p-3">Khách Hàng Hưởng Bảo Hành</th>
                  <th className="p-3">Thời Hạn Cam Kết</th>
                  <th className="p-3">Ngày Bắt Đầu</th>
                  <th className="p-3">Ngày Kết Thúc</th>
                  <th className="p-3">Hotline Hỗ Trợ</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {warranties.map((w) => (
                  <tr key={w.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{w.prj}</td>
                    <td className="p-3 font-semibold text-foreground">{w.cust}</td>
                    <td className="p-3 font-bold text-slate-800">{formatWarrantyPeriod(w.months)}</td>
                    <td className="p-3 text-slate-700">{w.start}</td>
                    <td className="p-3 font-mono font-bold text-emerald-700">{w.end}</td>
                    <td className="p-3 font-mono font-bold text-blue-700">{w.hotline}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Đang Hiệu Lực
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'productivity' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <h2 className="text-lg font-bold text-foreground">📊 Báo Cáo Đo Lường Năng Suất & Tỷ Lệ Tận Dụng Nguồn Lực (UC_PJM_041)</h2>
          <div className="grid grid-cols-4 gap-4">
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Tổng Số Kỹ Sư</div>
              <div className="text-2xl font-black text-foreground mt-1">{report.engineers} nhân sự</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Giờ Công Đã Ghi Nhận</div>
              <div className="text-2xl font-black text-slate-800 mt-1">{report.timesheet}h / {report.allocated}h</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Hiệu Suất Sử Dụng</div>
              <div className="text-2xl font-black text-brand mt-1">{formatUtilizationPercent(report.utilization)}</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Doanh Số Bình Quân / Kỹ Sư</div>
              <div className="text-2xl font-black text-emerald-700 mt-1">{report.outputPerEng.toLocaleString('vi-VN')} đ</div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
