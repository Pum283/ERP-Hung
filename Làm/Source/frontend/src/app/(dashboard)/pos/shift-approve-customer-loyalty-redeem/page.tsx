'use client';

import React, { useState } from 'react';
import {
  calculatePosEarnedLoyaltyPoints,
  calculatePosRedeemPointsDiscount,
} from '@/shared/api/pos-shift-approve-customer-loyalty-redeem-helpers';

export default function PosShiftApproveCustomerLoyaltyRedeemPage() {
  const [activeTab, setActiveTab] = useState<'shiftapproval' | 'customer' | 'loyalty'>('shiftapproval');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_POS_049: Duyệt xác nhận ca
  const [pendingShifts, setPendingShifts] = useState([
    { id: 'sh-1', cashier: 'Trần Thị Thu Ngân (POS01)', float: 1000000, revenue: 5450000, actualCash: 6450000, status: 'Pending' },
    { id: 'sh-2', cashier: 'Lê Văn Ca Sáng (POS02)', float: 1000000, revenue: 3200000, actualCash: 4180000, status: 'Pending' },
  ]);

  const handleApproveShift = (id: string, approve: boolean) => {
    setPendingShifts((prev) =>
      prev.map((s) => (s.id === id ? { ...s, status: approve ? 'Approved' : 'Rejected' } : s))
    );
    showToast(
      approve ? `✓ Quản lý đã duyệt xác nhận đóng ca thành công!` : `⚠️ Đã từ chối duyệt ca! Yêu cầu giải trình lệch két.`,
      approve ? 'success' : 'error'
    );
  };

  // UC_POS_050: Gắn khách hàng vào đơn
  const [customerSearch, setCustomerSearch] = useState('0909123456');
  const [assignedCustomer, setAssignedCustomer] = useState<{ name: string; phone: string; points: number; tier: string } | null>({
    name: 'Anh Nguyễn Văn Hùng',
    phone: '0909123456',
    points: 350,
    tier: 'Vàng (Gold Member)',
  });

  const handleSearchCustomer = (e: React.FormEvent) => {
    e.preventDefault();
    setAssignedCustomer({
      name: 'Anh Nguyễn Văn Hùng',
      phone: customerSearch || '0909123456',
      points: 350,
      tier: 'Vàng (Gold Member)',
    });
    showToast(`Đã gắn khách hàng [${customerSearch}] vào hóa đơn thành công!`, 'success');
  };

  // UC_POS_051 & UC_POS_052: Tích điểm & Đổi điểm
  const [orderAmount] = useState(150000);
  const pointsEarned = calculatePosEarnedLoyaltyPoints(orderAmount, 10000);

  const [pointsToRedeem, setPointsToRedeem] = useState(50);
  const pointsDiscountVnd = calculatePosRedeemPointsDiscount(pointsToRedeem, 1000);

  const handleRedeemPoints = () => {
    if (!assignedCustomer || assignedCustomer.points < pointsToRedeem) {
      showToast('Điểm tích lũy không đủ để quy đổi.', 'error');
      return;
    }
    setAssignedCustomer({
      ...assignedCustomer,
      points: assignedCustomer.points - pointsToRedeem,
    });
    showToast(`Đã quy đổi ${pointsToRedeem} điểm thành voucher giảm ${pointsDiscountVnd.toLocaleString('vi-VN')} VNĐ!`, 'success');
  };

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
              POS - SHIFT MANAGER APPROVAL & LOYALTY PROGRAM
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Duyệt Xác Nhận Ca, Gắn Khách Hàng & Tích/Đổi Điểm Loyalty POS</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Phê duyệt kết ca thu ngân cho Quản lý, tra cứu gắn thẻ thành viên và tích lũy/tiêu điểm trực tiếp tại quầy thanh toán
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('shiftapproval')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'shiftapproval' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🛡️ UC_POS_049: Quản Lý Duyệt Xác Nhận Ca
          </button>
          <button
            onClick={() => setActiveTab('customer')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'customer' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            👤 UC_POS_050: Gắn Khách Hàng Vào Đơn POS
          </button>
          <button
            onClick={() => setActiveTab('loyalty')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'loyalty' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⭐ UC_POS_051 & 052: Tích Điểm & Đổi Điểm Loyalty
          </button>
        </div>
      </div>

      {activeTab === 'shiftapproval' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🛡️ Danh Sách Ca Thu Ngân Chờ Duyệt Kết Ca (UC_POS_049)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Thu Ngân / Quầy</th>
                  <th className="p-3">Tiền Đầu Ca</th>
                  <th className="p-3">Doanh Thu Thu Được</th>
                  <th className="p-3">Tiền Thực Đếm</th>
                  <th className="p-3">Trạng Thái</th>
                  <th className="p-3 text-right">Thao Tác Duyệt</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {pendingShifts.map((s) => (
                  <tr key={s.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{s.cashier}</td>
                    <td className="p-3 text-slate-700">{s.float.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 font-medium text-emerald-700">{s.revenue.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 font-extrabold text-foreground">{s.actualCash.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3">
                      <span className={`px-2 py-0.5 text-xs font-bold rounded ${s.status === 'Approved' ? 'bg-emerald-100 text-emerald-800' : s.status === 'Rejected' ? 'bg-rose-100 text-rose-800' : 'bg-amber-100 text-amber-800'}`}>
                        {s.status}
                      </span>
                    </td>
                    <td className="p-3 text-right space-x-2">
                      {s.status === 'Pending' ? (
                        <>
                          <button
                            onClick={() => handleApproveShift(s.id, true)}
                            className="px-3 py-1 bg-emerald-600 text-white text-xs font-bold rounded-lg hover:bg-emerald-700"
                          >
                            ✓ Duyệt Ca
                          </button>
                          <button
                            onClick={() => handleApproveShift(s.id, false)}
                            className="px-3 py-1 bg-rose-600 text-white text-xs font-bold rounded-lg hover:bg-rose-700"
                          >
                            ✕ Từ Chối
                          </button>
                        </>
                      ) : (
                        <span className="text-xs text-muted-foreground font-semibold">Đã xử lý</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'customer' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-lg space-y-4">
          <h2 className="text-lg font-bold text-foreground">👤 Tra Cứu & Gắn Khách Hàng Vào Đơn Bán (UC_POS_050)</h2>
          <form onSubmit={handleSearchCustomer} className="flex gap-2">
            <input
              type="text"
              value={customerSearch}
              onChange={(e) => setCustomerSearch(e.target.value)}
              className="flex-1 border border-border rounded-lg p-2.5 bg-surface text-foreground font-semibold text-sm"
              placeholder="Nhập SĐT hoặc Mã Thẻ Thành Viên..."
            />
            <button type="submit" className="px-4 py-2.5 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90">
              🔍 Tra Cứu
            </button>
          </form>

          {assignedCustomer && (
            <div className="p-4 rounded-xl border border-brand/30 bg-brand-muted space-y-2">
              <div className="flex justify-between items-center">
                <span className="font-extrabold text-foreground text-base">{assignedCustomer.name}</span>
                <span className="px-2 py-0.5 text-xs font-bold rounded bg-amber-100 text-amber-800 border border-amber-300">
                  {assignedCustomer.tier}
                </span>
              </div>
              <p className="text-xs text-muted-foreground">SĐT: <span className="font-bold text-foreground">{assignedCustomer.phone}</span></p>
              <div className="pt-2 border-t border-brand/20 flex justify-between items-center text-sm">
                <span className="font-medium text-foreground">Điểm tích lũy hiện có:</span>
                <span className="font-extrabold text-brand-strong text-base">{assignedCustomer.points} ⭐</span>
              </div>
            </div>
          )}
        </div>
      )}

      {activeTab === 'loyalty' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">⭐ Tích Điểm Đơn Hàng Hiện Tại (UC_POS_051)</h2>
            <div className="p-4 rounded-xl border border-border bg-surface-hover/50 space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Giá trị đơn hàng:</span>
                <span className="font-bold text-foreground">{orderAmount.toLocaleString('vi-VN')} VNĐ</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Tỷ lệ quy đổi điểm:</span>
                <span className="font-semibold text-foreground">10.000 VNĐ = 1 điểm</span>
              </div>
              <div className="flex justify-between border-t border-border pt-2 text-base">
                <span className="font-bold text-foreground">Số điểm tích lũy thêm:</span>
                <span className="font-extrabold text-emerald-700">+{pointsEarned} Điểm Thưởng ⭐</span>
              </div>
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">🎁 Đổi Điểm Thưởng Trừ Tiền Hóa Đơn (UC_POS_052)</h2>
            <div className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Số điểm muốn quy đổi (1 điểm = 1.000đ):</label>
                <input
                  type="number"
                  value={pointsToRedeem}
                  onChange={(e) => setPointsToRedeem(Number(e.target.value))}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold"
                />
              </div>

              <div className="p-3 rounded-lg border border-border bg-surface-hover/50 flex justify-between items-center">
                <span className="text-muted-foreground">Số tiền giảm tương ứng:</span>
                <span className="font-extrabold text-rose-700 text-base">-{pointsDiscountVnd.toLocaleString('vi-VN')} VNĐ</span>
              </div>

              <button
                onClick={handleRedeemPoints}
                className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm"
              >
                🎁 Đồng Ý Đổi Điểm Trừ Tiền
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
