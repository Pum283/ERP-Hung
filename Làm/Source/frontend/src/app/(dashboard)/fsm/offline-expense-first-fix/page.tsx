'use client';

import React, { useState } from 'react';
import {
  formatFtfrPercentage,
  formatSettlementNet,
} from '@/shared/api/fsm-offline-expense-first-fix-helpers';

export default function FsmOfflineExpenseFirstFixPage() {
  const [activeTab, setActiveTab] = useState<'warnings' | 'offline' | 'settlements' | 'ftfr'>('warnings');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FSM_040: Cảnh báo thất thoát
  const [warnings] = useState([
    { id: 'w-1', tech: 'Trần Minh Hùng', code: 'PART-RELAY-12V', name: 'Rơ Le Nhiệt 12V', issued: 5, used: 3, returned: 1, loss: 1, severity: 'Warning', date: '2026-08-14' },
    { id: 'w-2', tech: 'Lê Anh Tuấn', code: 'PART-VALVE-SOLENOID', name: 'Van Điện Từ Đồng', issued: 4, used: 2, returned: 0, loss: 2, severity: 'Critical', date: '2026-08-14' },
  ]);

  // UC_FSM_043: Làm việc offline
  const [syncLogs, setSyncLogs] = useState([
    { id: 'l-1', tech: 'Nguyễn Văn Tuấn', device: 'SAMSUNG-TAB-ACTIVE-01', ops: 12, status: 'Success', start: '07:30', sync: '10:45' },
    { id: 'l-2', tech: 'Trần Minh Hùng', device: 'SM-A536B-ANDROID', ops: 8, status: 'Success', start: '08:00', sync: '10:50' },
  ]);

  // UC_FSM_044: Nộp quyết toán ngày
  const [settlements, setSettlements] = useState([
    { id: 'set-1', voucher: 'SETTLE-DAY-20260814-01', tech: 'Nguyễn Văn Tuấn', cash: 2500000, expense: 350000, net: 2150000, status: 'Submitted' },
    { id: 'set-2', voucher: 'SETTLE-DAY-20260814-02', tech: 'Trần Minh Hùng', cash: 1800000, expense: 200000, net: 1600000, status: 'Approved' },
  ]);

  // UC_FSM_048: Tỷ lệ sửa lần đầu
  const [ftfrReport] = useState({
    period: 'Tháng 08/2026',
    total: 120,
    firstFix: 108,
    recall: 12,
    rate: 90.0,
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
              FSM - PART LOSS ALERTS, OFFLINE SYNC, DAILY SETTLEMENT & FTFR METRICS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Cảnh Báo Thất Thoát Linh Kiện, Đồng Bộ Offline, Quyết Toán Ngày & Tỷ Lệ Sửa Lần Đầu (FTFR)</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Phát hiện chênh lệch xuất/dùng/trả linh kiện, quản lý delta sync khi mất sóng, duyệt quyết toán tiền mặt trong ca và đo lường First-Time Fix Rate
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
            onClick={() => setActiveTab('warnings')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'warnings' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚠️ UC_FSM_040: Cảnh Báo Thất Thoát
          </button>
          <button
            onClick={() => setActiveTab('offline')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'offline' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📶 UC_FSM_043: Đồng Bộ Offline
          </button>
          <button
            onClick={() => setActiveTab('settlements')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'settlements' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💵 UC_FSM_044: Quyết Toán Trong Ngày
          </button>
          <button
            onClick={() => setActiveTab('ftfr')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'ftfr' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🎯 UC_FSM_048: Tỷ Lệ Sửa Lần Đầu
          </button>
        </div>
      </div>

      {activeTab === 'warnings' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⚠️ Cảnh Báo Bất Thường Thất Thoát Linh Kiện Cấp Cho KTV (UC_FSM_040)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Kỹ Thuật Viên</th>
                  <th className="p-3">Linh Kiện</th>
                  <th className="p-3 text-center">Đã Xuất</th>
                  <th className="p-3 text-center">Đã Dùng</th>
                  <th className="p-3 text-center">Đã Hoàn</th>
                  <th className="p-3 text-center">Chênh Lệch Thiếu</th>
                  <th className="p-3 text-right">Mức Độ Cảnh Báo</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {warnings.map((w) => (
                  <tr key={w.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{w.tech}</td>
                    <td className="p-3">
                      <div className="font-semibold text-slate-800">{w.name}</div>
                      <div className="font-mono text-xs text-muted-foreground">{w.code}</div>
                    </td>
                    <td className="p-3 text-center font-bold text-slate-700">{w.issued}</td>
                    <td className="p-3 text-center font-bold text-slate-700">{w.used}</td>
                    <td className="p-3 text-center font-bold text-slate-700">{w.returned}</td>
                    <td className="p-3 text-center font-black text-rose-700">-{w.loss} cái</td>
                    <td className="p-3 text-right">
                      <span className={`px-2.5 py-1 text-xs font-black rounded-full border ${w.severity === 'Critical' ? 'bg-rose-100 text-rose-800 border-rose-300' : 'bg-amber-100 text-amber-800 border-amber-300'}`}>
                        ● {w.severity === 'Critical' ? 'Nghiêm Trọng' : 'Cần Giải Trình'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'offline' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📶 Nhật Ký Đồng Bộ Dữ Liệu Ngoại Tuyến (Offline Sync) (UC_FSM_043)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Kỹ Thuật Viên</th>
                  <th className="p-3">Thiết Bị Di Động</th>
                  <th className="p-3 text-center">Thao Tác Đồng Bộ</th>
                  <th className="p-3">Bắt Đầu Offline</th>
                  <th className="p-3">Thời Điểm Sync Thành Công</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {syncLogs.map((l) => (
                  <tr key={l.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{l.tech}</td>
                    <td className="p-3 font-mono text-slate-700">{l.device}</td>
                    <td className="p-3 text-center font-extrabold text-brand">{l.ops} giao dịch</td>
                    <td className="p-3 text-slate-700">{l.start}</td>
                    <td className="p-3 text-slate-700 font-medium">{l.sync}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Thành Công
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'settlements' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">💵 Nộp Quyết Toán Tiền Thu & Chi Phí Cuối Ngày (UC_FSM_044)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Chứng Từ Quyết Toán</th>
                  <th className="p-3">Kỹ Thuật Viên</th>
                  <th className="p-3 text-right">Tiền Thu Khách Hàng</th>
                  <th className="p-3 text-right">Chi Phí Tự Ứng</th>
                  <th className="p-3 text-right">Thực Nộp Về Quỹ</th>
                  <th className="p-3 text-right">Trạng Thái Duyệt</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {settlements.map((st) => (
                  <tr key={st.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{st.voucher}</td>
                    <td className="p-3 font-bold text-foreground">{st.tech}</td>
                    <td className="p-3 text-right font-medium text-slate-700">{st.cash.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-medium text-rose-700">-{st.expense.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-black text-emerald-700 text-base">{formatSettlementNet(st.net)}</td>
                    <td className="p-3 text-right">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${st.status === 'Approved' ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-amber-100 text-amber-800 border-amber-300'}`}>
                        ● {st.status === 'Approved' ? 'Đã Thủ Quỹ Duyệt' : 'Chờ Kế Toán Duyệt'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'ftfr' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <h2 className="text-lg font-bold text-foreground">🎯 Báo Cáo Tỷ Lệ Sửa Chữa Thành Công Lần Đầu (FTFR) (UC_FSM_048)</h2>
          <div className="grid grid-cols-4 gap-4">
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Tổng Ticket Hoàn Tất</div>
              <div className="text-2xl font-black text-foreground mt-1">{ftfrReport.total} tickets</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Sửa Xong Lần 1 (First-Time Fix)</div>
              <div className="text-2xl font-black text-emerald-700 mt-1">{ftfrReport.firstFix} tickets</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Tái Mở / Gọi Lại</div>
              <div className="text-2xl font-black text-rose-700 mt-1">{ftfrReport.recall} tickets</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Chỉ Số FTFR (%)</div>
              <div className="text-2xl font-black text-brand mt-1">{formatFtfrPercentage(ftfrReport.rate)}</div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
