'use client';

import React, { useState } from 'react';
import {
  formatBillableRepairAmount,
  formatStarRating,
} from '@/shared/api/fsm-repair-feedback-reopen-finance-helpers';

export default function FsmRepairFeedbackReopenFinancePage() {
  const [activeTab, setActiveTab] = useState<'billing' | 'feedback' | 'reopen' | 'finance'>('billing');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FSM_026: Ghi nhận phí sửa chữa
  const [repairCosts, setRepairCosts] = useState([
    { id: 'rc-1', ticket: 'TCK-2026-0814-01', labor: 350000, parts: 250000, travel: 150000, total: 750000, warranty: false },
    { id: 'rc-2', ticket: 'TCK-2026-0814-02', labor: 500000, parts: 450000, travel: 150000, total: 0, warranty: true },
  ]);

  // UC_FSM_029: Đánh giá dịch vụ
  const [feedbacks, setFeedbacks] = useState([
    { id: 'fb-1', ticket: 'TCK-2026-0814-01', stars: 5, comment: 'Kỹ thuật viên đến đúng giờ, khắc phục lỗi triệt để', customer: 'Anh Hoàng FPT', date: '2026-08-14' },
    { id: 'fb-2', ticket: 'TCK-2026-0814-02', stars: 4, comment: 'Dịch vụ tốt, thời gian xử lý nhanh', customer: 'Chị Mai Viettel', date: '2026-08-14' },
  ]);

  // UC_FSM_031: Tái mở ticket
  const [reopenLogs, setReopenLogs] = useState([
    { id: 'ro-1', ticket: 'TCK-2026-0812-09', reason: 'Nhiệt độ tủ server tiếp tục tăng sau 24h chạy thử', by: 'Khách Hàng', cause: 'Cần thay cụm quạt tản nhiệt thứ cấp', date: '2026-08-14' },
  ]);

  // UC_FSM_032: Chuyển chi phí sang FIN
  const [finTransfers, setFinTransfers] = useState([
    { id: 'ft-1', voucher: 'FIN-FSM-20260814-01', ticket: 'TCK-2026-0814-01', amount: 750000, debit: '627', credit: '154', status: 'Posted' },
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
              FSM - REPAIR BILLING, CSAT FEEDBACK, TICKET REOPEN & FIN GL INTEGRATION
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Bóc Tách Phí Sửa Chữa, Đánh Giá Khách Hàng CSAT, Tái Mở Ticket & Hạch Toán Kế Toán FIN</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Ghi nhận chi phí giờ công / linh kiện / di chuyển, lưu trữ khảo sát sao hài lòng, phân tích nguyên nhân tái mở sự cố và đẩy bút toán chi phí sang sổ cái Tài chính
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
            onClick={() => setActiveTab('billing')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'billing' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💰 UC_FSM_026: Phí Sửa Chữa
          </button>
          <button
            onClick={() => setActiveTab('feedback')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'feedback' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⭐ UC_FSM_029: Đánh Giá Dịch Vụ
          </button>
          <button
            onClick={() => setActiveTab('reopen')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'reopen' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔄 UC_FSM_031: Tái Mở Ticket
          </button>
          <button
            onClick={() => setActiveTab('finance')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'finance' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📑 UC_FSM_032: Chuyển Sang FIN
          </button>
        </div>
      </div>

      {activeTab === 'billing' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">💰 Bóc Tách Chi Phí Sửa Chữa & Quyết Toán Ticket (UC_FSM_026)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Ticket</th>
                  <th className="p-3 text-right">Phí Nhân Công</th>
                  <th className="p-3 text-right">Phí Linh Kiện</th>
                  <th className="p-3 text-right">Phí Đi Lại</th>
                  <th className="p-3 text-center">Bảo Hành?</th>
                  <th className="p-3 text-right">Tổng Thanh Toán</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {repairCosts.map((r) => (
                  <tr key={r.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{r.ticket}</td>
                    <td className="p-3 text-right font-medium text-slate-700">{r.labor.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-medium text-slate-700">{r.parts.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-medium text-slate-700">{r.travel.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-center">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${r.warranty ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-slate-100 text-slate-800 border-slate-300'}`}>
                        {r.warranty ? '✓ Bảo Hành Miễn Phí' : 'Tính Phí'}
                      </span>
                    </td>
                    <td className="p-3 text-right font-black text-rose-700 text-base">{formatBillableRepairAmount(r.total, r.warranty)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'feedback' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⭐ Đánh Giá Sự Hài Lòng Khách Hàng CSAT (UC_FSM_029)</h2>
          <div className="grid grid-cols-2 gap-6">
            {feedbacks.map((f) => (
              <div key={f.id} className="p-4 rounded-xl border border-border bg-surface space-y-2">
                <div className="flex justify-between items-center">
                  <span className="font-mono font-bold text-brand">{f.ticket}</span>
                  <span className="text-amber-500 font-bold text-lg tracking-widest">{formatStarRating(f.stars)}</span>
                </div>
                <p className="text-sm italic text-foreground">"{f.comment}"</p>
                <div className="text-xs text-muted-foreground border-t border-border pt-2 flex justify-between">
                  <span>Khách hàng: <strong className="text-slate-800">{f.customer}</strong></span>
                  <span>{f.date}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'reopen' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🔄 Nhật Ký Tái Mở Ticket Sau Nghiệm Thu (UC_FSM_031)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Ticket</th>
                  <th className="p-3">Lý Do Tái Mở</th>
                  <th className="p-3">Người Yêu Cầu</th>
                  <th className="p-3">Phân Loại Nguyên Nhân Gốc</th>
                  <th className="p-3 text-right">Ngày Tái Mở</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {reopenLogs.map((ro) => (
                  <tr key={ro.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-rose-700">{ro.ticket}</td>
                    <td className="p-3 font-semibold text-foreground">{ro.reason}</td>
                    <td className="p-3 text-slate-700">{ro.by}</td>
                    <td className="p-3 text-xs font-bold text-amber-800">{ro.cause}</td>
                    <td className="p-3 text-right text-slate-700">{ro.date}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'finance' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📑 Chuyển Chi Phí Dịch Vụ Sang Kế Toán FIN (UC_FSM_032)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Chứng Từ Bút Toán</th>
                  <th className="p-3">Ticket Nguồn</th>
                  <th className="p-3 text-right">Số Tiền Hạch Toán</th>
                  <th className="p-3 text-center">Nợ TK (Debit)</th>
                  <th className="p-3 text-center">Có TK (Credit)</th>
                  <th className="p-3 text-right">Trạng Thái Sổ Cái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {finTransfers.map((ft) => (
                  <tr key={ft.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{ft.voucher}</td>
                    <td className="p-3 font-mono font-bold text-foreground">{ft.ticket}</td>
                    <td className="p-3 text-right font-black text-emerald-700">{ft.amount.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-center font-mono font-bold text-slate-800">{ft.debit}</td>
                    <td className="p-3 text-center font-mono font-bold text-slate-800">{ft.credit}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Đã Ghi Sổ (Posted)
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
