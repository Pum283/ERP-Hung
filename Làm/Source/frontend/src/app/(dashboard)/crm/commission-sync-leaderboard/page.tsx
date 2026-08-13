'use client';

import React, { useState } from 'react';
import {
  evaluateCommissionPeriodStatusBadge,
  formatLeaderboardRankBadge,
  validateCommissionPeriodForm,
} from '@/shared/api/crm-commission-sync-leaderboard-helpers';

export default function CrmCommissionSyncLeaderboardPage() {
  const [activeTab, setActiveTab] = useState<'calculate' | 'approve' | 'sync' | 'leaderboard'>('calculate');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: PERIODIC COMMISSION CALCULATION (UC_CRM_121)
  // ────────────────────────────────────────────────────────────────────────────
  const [periods, setPeriods] = useState([
    { id: 'p-1', code: 'COMM-2026-M07', name: 'Bảng Hoa Hồng Tháng 07/2026', startDate: '2026-07-01', endDate: '2026-07-31', totalAmount: 48500000, status: 'SyncedToHrmFin' },
    { id: 'p-2', code: 'COMM-2026-M08', name: 'Bảng Hoa Hồng Tháng 08/2026', startDate: '2026-08-01', endDate: '2026-08-31', totalAmount: 52300000, status: 'Calculated' },
  ]);

  const [calcForm, setCalcForm] = useState({ periodCode: '', periodName: '', startDate: '2026-09-01', endDate: '2026-09-30' });

  const handleCalculatePeriod = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateCommissionPeriodForm(calcForm.periodCode, calcForm.startDate, calcForm.endDate);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `p-${Date.now()}`,
      code: calcForm.periodCode,
      name: calcForm.periodName || `Bảng Hoa Hồng ${calcForm.periodCode}`,
      startDate: calcForm.startDate,
      endDate: calcForm.endDate,
      totalAmount: 42100000,
      status: 'Calculated',
    };

    setPeriods([created, ...periods]);
    setCalcForm({ periodCode: '', periodName: '', startDate: '2026-09-01', endDate: '2026-09-30' });
    showToast(`Đã tính toán hoa hồng cho kỳ [${created.code}] thành công!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: COMMISSION APPROVAL (UC_CRM_122)
  // ────────────────────────────────────────────────────────────────────────────
  const handleApprovePeriod = (pId: string, code: string) => {
    setPeriods((prev) =>
      prev.map((p) => (p.id === pId ? { ...p, status: 'Approved' } : p))
    );
    showToast(`✓ Đã phê duyệt chi trả hoa hồng cho kỳ [${code}]!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: SYNC COMMISSION TO HRM/FIN (UC_CRM_123)
  // ────────────────────────────────────────────────────────────────────────────
  const handleSyncHrmFin = (pId: string, code: string) => {
    setPeriods((prev) =>
      prev.map((p) => (p.id === pId ? { ...p, status: 'SyncedToHrmFin' } : p))
    );
    showToast(`🔗 Đã đồng bộ hoa hồng kỳ [${code}] sang HRM (Bảng lương) & FIN (Kế toán chi phí)!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: SALES LEADERBOARD (UC_CRM_125)
  // ────────────────────────────────────────────────────────────────────────────
  const [leaderboard] = useState([
    { id: 'lb-1', rank: 1, name: 'Nguyễn Văn FieldSales 1', revenue: 450000000, newCust: 12, commission: 18000000 },
    { id: 'lb-2', rank: 2, name: 'Trần Thị SalesRep 2', revenue: 380000000, newCust: 9, commission: 14500000 },
    { id: 'lb-3', rank: 3, name: 'Phạm Hoàng SalesRep 3', revenue: 310000000, newCust: 7, commission: 12000000 },
    { id: 'lb-4', rank: 4, name: 'Lê Thu SalesRep 4', revenue: 260000000, newCust: 5, commission: 9800000 },
  ]);

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      {/* Toast Notification */}
      {toast && (
        <div
          className={`fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white font-medium text-sm transition-all ${
            toast.type === 'success' ? 'bg-emerald-600' : toast.type === 'error' ? 'bg-rose-600' : 'bg-amber-600'
          }`}
        >
          {toast.message}
        </div>
      )}

      {/* Header Container - Tuân thủ Branding Rule 11 */}
      <div className="bg-surface border border-border p-6 rounded-2xl shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-brand-muted text-brand-strong text-xs px-3 py-1 rounded-full font-semibold border border-brand/30">
              CRM - COMMISSION & SALES LEADERBOARD
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Tính Hoa Hồng Theo Kỳ & Bảng Xếp Hạng Sales</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Tính hoa hồng kinh doanh theo kỳ, phê duyệt chi trả, đồng bộ tự động sang HRM/FIN và bảng xếp hạng Sales
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs)
            </span>
          </div>
        </div>

        {/* Tab Navigation - Branding CSS Variables */}
        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('calculate')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'calculate' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🧮 UC_CRM_121: Tính Hoa Hồng Theo Kỳ
          </button>
          <button
            onClick={() => setActiveTab('approve')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'approve' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ✓ UC_CRM_122: Duyệt Bảng Hoa Hồng
          </button>
          <button
            onClick={() => setActiveTab('sync')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'sync' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔗 UC_CRM_123: Đồng Bộ HRM / FIN
          </button>
          <button
            onClick={() => setActiveTab('leaderboard')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'leaderboard' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏆 UC_CRM_125: Bảng Xếp Hạng Sales
          </button>
        </div>
      </div>

      {/* TAB 1: PERIODIC COMMISSION CALCULATION */}
      {activeTab === 'calculate' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">🧮 Danh Sách Kỳ Tính Hoa Hồng Kinh Doanh (UC_CRM_121)</h2>
            <div className="space-y-3">
              {periods.map((p) => {
                const statusBadge = evaluateCommissionPeriodStatusBadge(p.status);
                return (
                  <div key={p.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{p.code}</span>
                        <h3 className="font-bold text-foreground">{p.name}</h3>
                      </div>
                      <p className="text-xs text-muted-foreground mt-1">Thời gian: {p.startDate} ➔ {p.endDate}</p>
                    </div>
                    <div className="text-right space-y-1">
                      <span className="text-base font-extrabold text-foreground block">{p.totalAmount.toLocaleString('vi-VN')} VNĐ</span>
                      <span className={`inline-block px-2.5 py-0.5 text-xs rounded-full border ${statusBadge.badgeClass}`}>
                        {statusBadge.label}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">➕ Khởi Tạo Kỳ Hoa Hồng Mới</h2>
            <form onSubmit={handleCalculatePeriod} className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã kỳ tính hoa hồng:</label>
                <input
                  type="text"
                  value={calcForm.periodCode}
                  onChange={(e) => setCalcForm({ ...calcForm, periodCode: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: COMM-2026-M09"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên kỳ:</label>
                <input
                  type="text"
                  value={calcForm.periodName}
                  onChange={(e) => setCalcForm({ ...calcForm, periodName: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: Bảng Hoa Hồng Tháng 09/2026"
                />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="block text-foreground font-medium mb-1">Từ ngày:</label>
                  <input
                    type="date"
                    value={calcForm.startDate}
                    onChange={(e) => setCalcForm({ ...calcForm, startDate: e.target.value })}
                    className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Đến ngày:</label>
                  <input
                    type="date"
                    value={calcForm.endDate}
                    onChange={(e) => setCalcForm({ ...calcForm, endDate: e.target.value })}
                    className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  />
                </div>
              </div>
              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90 transition-opacity">
                Tính Tự Động Hoa Hồng
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 2: COMMISSION APPROVAL */}
      {activeTab === 'approve' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">✓ Phê Duyệt Bảng Hoa Hồng Kinh Doanh (UC_CRM_122)</h2>
          <div className="space-y-3">
            {periods.map((p) => {
              const statusBadge = evaluateCommissionPeriodStatusBadge(p.status);
              return (
                <div key={p.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-foreground">{p.name} ({p.code})</h3>
                    <p className="text-xs text-muted-foreground mt-1">
                      Tổng tiền hoa hồng: <span className="font-bold text-foreground">{p.totalAmount.toLocaleString('vi-VN')} VNĐ</span>
                    </p>
                  </div>
                  <div className="flex gap-3 items-center">
                    <span className={`px-3 py-1 text-xs rounded-full border ${statusBadge.badgeClass}`}>
                      {statusBadge.label}
                    </span>
                    {p.status === 'Calculated' ? (
                      <button
                        onClick={() => handleApprovePeriod(p.id, p.code)}
                        className="px-3.5 py-1.5 bg-emerald-600 text-white text-xs font-bold rounded-lg hover:bg-emerald-700 shadow-sm"
                      >
                        ✓ Duyệt Bảng Hoa Hồng
                      </button>
                    ) : (
                      <span className="px-3 py-1 text-xs font-bold rounded-lg bg-emerald-100 text-emerald-800">
                        ✓ Đã phê duyệt
                      </span>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* TAB 3: SYNC COMMISSION TO HRM/FIN */}
      {activeTab === 'sync' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🔗 Đồng Bộ Hoa Hồng Sang Phân Hệ HRM & FIN (UC_CRM_123)</h2>
          <div className="space-y-3">
            {periods.map((p) => (
              <div key={p.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                <div>
                  <h3 className="font-bold text-foreground">{p.name} ({p.code})</h3>
                  <p className="text-xs text-muted-foreground mt-1">Giá trị chi trả: {p.totalAmount.toLocaleString('vi-VN')} VNĐ</p>
                </div>
                <div>
                  {p.status === 'SyncedToHrmFin' ? (
                    <span className="px-3.5 py-1.5 text-xs font-bold rounded-lg bg-brand-muted text-brand-strong border border-brand/30">
                      🔗 Đã đồng bộ Lương HRM & Chi phí FIN
                    </span>
                  ) : p.status === 'Approved' ? (
                    <button
                      onClick={() => handleSyncHrmFin(p.id, p.code)}
                      className="px-3.5 py-1.5 bg-brand text-brand-foreground text-xs font-bold rounded-lg hover:opacity-90 shadow-sm"
                    >
                      🔗 Thực Hiện Đồng Bộ Sang HRM/FIN
                    </button>
                  ) : (
                    <span className="px-3 py-1 text-xs font-semibold text-muted-foreground">Cần duyệt trước khi đồng bộ</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* TAB 4: SALES LEADERBOARD */}
      {activeTab === 'leaderboard' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-bold text-foreground">🏆 Bảng Xếp Hạng Sales Leaderboard (UC_CRM_125)</h2>
            <span className="text-xs font-semibold text-muted-foreground">Kỳ xếp hạng: Tháng 08/2026</span>
          </div>
          <div className="space-y-3">
            {leaderboard.map((lb) => {
              const rankBadge = formatLeaderboardRankBadge(lb.rank);
              return (
                <div key={lb.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div className="flex items-center gap-3">
                    <span className={`px-3 py-1 text-xs rounded-full border ${rankBadge.badgeClass}`}>
                      {rankBadge.label}
                    </span>
                    <div>
                      <h3 className="font-bold text-foreground">{lb.name}</h3>
                      <p className="text-xs text-muted-foreground mt-0.5">Khách hàng mới: {lb.newCust} KH</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <span className="text-sm font-extrabold text-foreground block">Doanh số: {lb.revenue.toLocaleString('vi-VN')} VNĐ</span>
                    <span className="text-xs text-brand-strong font-bold">Hoa hồng đạt: {lb.commission.toLocaleString('vi-VN')} VNĐ</span>
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
