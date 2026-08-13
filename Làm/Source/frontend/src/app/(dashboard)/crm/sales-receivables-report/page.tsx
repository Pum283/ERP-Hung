'use client';

import React, { useState } from 'react';
import {
  evaluateReceivableDebtRiskLevel,
  validateReportExportForm,
} from '@/shared/api/crm-sales-receivables-report-helpers';

export default function CrmSalesReceivablesReportPage() {
  const [activeTab, setActiveTab] = useState<'aging' | 'export'>('aging');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_CRM_130: Báo cáo công nợ bán
  const [agingSummary] = useState({
    totalReceivables: 195000000,
    totalOverdue: 58000000,
    overdueRate: 29.7,
    customers: [
      { id: 'c-1', name: 'Đại lý Nông Sản Miền Tây', current: 50000000, d30: 20000000, d60: 15000000, d90: 0, over90: 0, total: 85000000 },
      { id: 'c-2', name: 'Chuỗi Cửa hàng Tiện Lợi An Khang', current: 30000000, d30: 12000000, d60: 0, d90: 0, over90: 0, total: 42000000 },
      { id: 'c-3', name: 'Công ty TNHH Bách Hóa Việt', current: 20000000, d30: 25000000, d60: 13000000, d90: 10000000, over90: 0, total: 68000000 },
    ],
  });

  // UC_CRM_131: Xuất báo cáo định kỳ
  const [scheduledExports, setScheduledExports] = useState([
    { id: 'exp-1', name: 'Báo Cáo Phân Tích Công Nợ Quá Hạn', type: 'ReceivablesAging', format: 'PDF', frequency: 'Monthly', emails: 'giamdoc@erphung.vn, ketoan@erphung.vn' },
    { id: 'exp-2', name: 'Báo Cáo Hoa Hồng Kế Toán Chi Trả', type: 'CommissionSummary', format: 'Excel', frequency: 'Weekly', emails: 'ketoanluong@erphung.vn' },
  ]);

  const [exportForm, setExportForm] = useState({ reportName: '', reportType: 'ReceivablesAging', format: 'PDF', frequency: 'Monthly', recipientEmails: '' });

  const handleCreateSchedule = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateReportExportForm(exportForm.reportName, exportForm.recipientEmails);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `exp-${Date.now()}`,
      name: exportForm.reportName,
      type: exportForm.reportType,
      format: exportForm.format,
      frequency: exportForm.frequency,
      emails: exportForm.recipientEmails,
    };

    setScheduledExports([created, ...scheduledExports]);
    setExportForm({ reportName: '', reportType: 'ReceivablesAging', format: 'PDF', frequency: 'Monthly', recipientEmails: '' });
    showToast(`Đã thiết lập lịch xuất báo cáo [${created.name}] tự động thành công!`, 'success');
  };

  const riskBadge = evaluateReceivableDebtRiskLevel(agingSummary.overdueRate);

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
              CRM - SALES RECEIVABLES & EXPORTS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Báo Cáo Công Nợ Bán Hàng & Xuất Báo Cáo Định Kỳ</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Phân tích công nợ phải thu, phân loại tuổi nợ quá hạn và cấu hình lịch xuất báo cáo tự động sang PDF/Excel
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (2/2 UCs)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('aging')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'aging' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_CRM_130: Báo Cáo Phân Tích Công Nợ Bán
          </button>
          <button
            onClick={() => setActiveTab('export')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'export' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📁 UC_CRM_131: Cấu Hình Xuất Báo Cáo Định Kỳ
          </button>
        </div>
      </div>

      {activeTab === 'aging' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-surface p-5 rounded-xl border border-border">
              <span className="text-xs font-semibold text-muted-foreground block">TỔNG CÔNG NỢ PHẢI THU</span>
              <span className="text-2xl font-bold text-foreground mt-1 block">{agingSummary.totalReceivables.toLocaleString('vi-VN')} VNĐ</span>
            </div>
            <div className="bg-surface p-5 rounded-xl border border-border">
              <span className="text-xs font-semibold text-muted-foreground block">TỔNG NỢ QUÁ HẠN (&gt;30 NGÀY)</span>
              <span className="text-2xl font-bold text-rose-600 mt-1 block">{agingSummary.totalOverdue.toLocaleString('vi-VN')} VNĐ</span>
            </div>
            <div className="bg-surface p-5 rounded-xl border border-border">
              <span className="text-xs font-semibold text-muted-foreground block">MỨC ĐỘ RỦI RO CÔNG NỢ</span>
              <div className="mt-2">
                <span className={`px-3 py-1 text-xs rounded-full border ${riskBadge.badgeClass}`}>
                  {riskBadge.label}
                </span>
              </div>
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">Chi Tiết Tuổi Nợ Theo Khách Hàng (Aging Analysis)</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Khách Hàng</th>
                    <th className="p-3">Trong Hạn</th>
                    <th className="p-3">1-30 Ngày</th>
                    <th className="p-3">31-60 Ngày</th>
                    <th className="p-3">61-90 Ngày</th>
                    <th className="p-3 font-bold">Tổng Công Nợ</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {agingSummary.customers.map((c) => (
                    <tr key={c.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-bold text-foreground">{c.name}</td>
                      <td className="p-3 text-emerald-700">{c.current.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-slate-700">{c.d30.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-amber-700 font-medium">{c.d60.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-rose-700 font-bold">{c.d90.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 font-extrabold text-foreground">{c.total.toLocaleString('vi-VN')} đ</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'export' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📁 Danh Sách Lịch Xuất Báo Cáo Định Kỳ (UC_CRM_131)</h2>
            <div className="space-y-3">
              {scheduledExports.map((exp) => (
                <div key={exp.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{exp.format}</span>
                      <h3 className="font-bold text-foreground">{exp.name}</h3>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">Tần suất gửi: <span className="font-semibold text-foreground">{exp.frequency}</span> | Email: {exp.emails}</p>
                  </div>
                  <button className="px-3 py-1.5 bg-brand text-brand-foreground text-xs font-semibold rounded-lg hover:opacity-90">
                    Xuất Ngay PDF/Excel
                  </button>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">➕ Thiết Lập Lịch Báo Cáo Mới</h2>
            <form onSubmit={handleCreateSchedule} className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Tên báo cáo:</label>
                <input
                  type="text"
                  value={exportForm.reportName}
                  onChange={(e) => setExportForm({ ...exportForm, reportName: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: Báo Cáo Doanh Số & Công Nợ Tuần"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Định dạng file:</label>
                <select
                  value={exportForm.format}
                  onChange={(e) => setExportForm({ ...exportForm, format: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                >
                  <option value="PDF">Tệp PDF (.pdf)</option>
                  <option value="Excel">Bảng tính Excel (.xlsx)</option>
                  <option value="CSV">Dữ liệu CSV (.csv)</option>
                </select>
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tần suất gửi:</label>
                <select
                  value={exportForm.frequency}
                  onChange={(e) => setExportForm({ ...exportForm, frequency: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                >
                  <option value="Daily">Hàng ngày (Daily)</option>
                  <option value="Weekly">Hàng tuần (Weekly)</option>
                  <option value="Monthly">Hàng tháng (Monthly)</option>
                </select>
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Email nhận báo cáo:</label>
                <input
                  type="text"
                  value={exportForm.recipientEmails}
                  onChange={(e) => setExportForm({ ...exportForm, recipientEmails: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: sếp@erphung.vn, ketoan@erphung.vn"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
                Lưu Lịch Xuất Báo Cáo
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
