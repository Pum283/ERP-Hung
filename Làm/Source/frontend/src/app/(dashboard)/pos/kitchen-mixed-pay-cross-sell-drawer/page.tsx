'use client';

import React, { useState } from 'react';
import {
  calculateMixedPaymentBalance,
  calculateShiftCashNetBalance,
} from '@/shared/api/pos-kitchen-mixed-pay-cross-sell-drawer-helpers';

export default function PosKitchenMixedPayCrossSellDrawerPage() {
  const [activeTab, setActiveTab] = useState<'kot' | 'mixedpay' | 'crosssell' | 'drawer'>('kot');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_POS_031: Gửi lệnh chế biến Bếp/Bar
  const [kotTickets, setKotTickets] = useState([
    { id: 'kot-1', number: 'KOT-140512', station: 'Bếp Nóng', items: ['2x Cà Phê Sữa Đá', '1x Bánh Mì Kẹp Thịt'], status: 'Sent', time: '14:05' },
    { id: 'kot-2', number: 'KOT-141022', station: 'Quầy Bar', items: ['1x Trà Đào Cam Sả', '1x Sinh Tố Bơ'], status: 'Preparing', time: '14:10' },
  ]);

  const handleDispatchKot = () => {
    const newKot = {
      id: `kot-${Date.now()}`,
      number: `KOT-${Math.floor(100000 + Math.random() * 900000)}`,
      station: 'Bếp Nóng',
      items: ['1x Cơm Tấm Sườn Bì Chả', '1x Canh Khổ Qua'],
      status: 'Sent',
      time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
    };
    setKotTickets([newKot, ...kotTickets]);
    showToast(`Đã in lệnh & gửi vé KOT [${newKot.number}] sang Bếp Nóng!`, 'success');
  };

  // UC_POS_036: Thanh toán hỗn hợp
  const [orderTotal] = useState(150000);
  const [payments, setPayments] = useState([
    { id: 'p-1', method: 'Tiền mặt (Cash)', amountVnd: 50000 },
    { id: 'p-2', method: 'Chuyển khoản (QR Code)', amountVnd: 100000 },
  ]);

  const mixedBalance = calculateMixedPaymentBalance(
    orderTotal,
    payments.map((p) => ({ amountVnd: p.amountVnd }))
  );

  // UC_POS_041: Gợi ý bán kèm (Cross-sell)
  const [recommendations] = useState([
    { id: 'rec-1', code: 'CAKE-CHOCO', name: 'Bánh Mì Ngọt Phô Mai Socola', price: 25000, reason: 'Combo tuyệt hảo khi dùng kèm Cà Phê' },
    { id: 'rec-2', code: 'TOPPING-PEARL', name: 'Trân Châu Trắng Giòn Thủy Tinh', price: 10000, reason: 'Topping bán chạy nhất kèm Trà Sữa' },
    { id: 'rec-3', code: 'DRINK-UPGRADE', name: 'Nâng Cấp Size L Đồ Uống', price: 8000, reason: 'Ưu đãi nâng size tiết kiệm 20%' },
  ]);

  // UC_POS_044: Nộp tiền / rút tiền ca
  const [cashInTotal, setCashInTotal] = useState(500000);
  const [cashOutTotal, setCashOutTotal] = useState(200000);
  const netDrawerBalance = calculateShiftCashNetBalance(cashInTotal, cashOutTotal, 1000000);

  const [drawerForm, setDrawerForm] = useState({ amount: 100000, reason: 'Nộp bổ sung tiền lẻ' });

  const handleCashIn = (e: React.FormEvent) => {
    e.preventDefault();
    setCashInTotal(cashInTotal + Number(drawerForm.amount));
    showToast(`Đã ghi nhận Nộp Tiền Ca (+${Number(drawerForm.amount).toLocaleString('vi-VN')} đ) thành công!`, 'success');
  };

  const handleCashOut = (e: React.FormEvent) => {
    e.preventDefault();
    setCashOutTotal(cashOutTotal + Number(drawerForm.amount));
    showToast(`Đã ghi nhận Rút Tiền Ca (-${Number(drawerForm.amount).toLocaleString('vi-VN')} đ) thành công!`, 'success');
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
              POS - KITCHEN DISPATCH, MIXED PAY & CASH DRAWER
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Lệnh Chế Biến Bếp, Thanh Toán Hỗn Hợp & Quản Lý Ngăn Kéo Ca</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Gửi lệnh KOT chế biến bếp/bar, tách nhiều phương thức thanh toán, gợi ý bán kèm sản phẩm và nộp/rút tiền ca
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
            onClick={() => setActiveTab('kot')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'kot' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            👨‍🍳 UC_POS_031: Gửi Lệnh Chế Biến (KOT Ticket)
          </button>
          <button
            onClick={() => setActiveTab('mixedpay')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'mixedpay' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💳 UC_POS_036: Thanh Toán Hỗn Hợp (Multi-Method)
          </button>
          <button
            onClick={() => setActiveTab('crosssell')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'crosssell' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💡 UC_POS_041: Gợi Ý Bán Kèm (Cross-Sell)
          </button>
          <button
            onClick={() => setActiveTab('drawer')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'drawer' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💵 UC_POS_044: Nộp Tiền / Rút Tiền Ca Bán
          </button>
        </div>
      </div>

      {activeTab === 'kot' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-bold text-foreground">👨‍🍳 Lệnh Chế Biến Gửi Bếp & Quầy Bar (UC_POS_031)</h2>
              <p className="text-xs text-muted-foreground mt-0.5">Tự động in lệnh KOT điều phối món ăn theo trạm chế biến</p>
            </div>
            <button
              onClick={handleDispatchKot}
              className="px-4 py-2 bg-brand text-brand-foreground text-sm font-bold rounded-lg hover:opacity-90 shadow-sm"
            >
              ➕ In Lệnh & Gửi KOT Mới Sang Bếp
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {kotTickets.map((kot) => (
              <div key={kot.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 space-y-2">
                <div className="flex justify-between items-center">
                  <span className="font-extrabold text-foreground">{kot.number}</span>
                  <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong border border-brand/30">
                    {kot.station} • {kot.time}
                  </span>
                </div>
                <ul className="text-xs space-y-1 text-slate-700 font-medium">
                  {kot.items.map((it, idx) => (
                    <li key={idx}>• {it}</li>
                  ))}
                </ul>
                <div className="pt-2 flex justify-between items-center text-xs">
                  <span className="text-muted-foreground">Trạng thái:</span>
                  <span className="font-bold text-emerald-700 bg-emerald-100 px-2 py-0.5 rounded">{kot.status}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'mixedpay' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div>
            <h2 className="text-lg font-bold text-foreground">💳 Màn Hình Thanh Toán Hỗn Hợp Nhanh (UC_POS_036)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Cho phép thanh toán một hóa đơn bằng kết hợp Tiền mặt, Thẻ ngân hàng, QR Code</p>
          </div>

          <div className="max-w-xl space-y-4">
            <div className="p-4 rounded-xl border border-border bg-surface-hover/50 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Tổng tiền hóa đơn:</span>
                <span className="font-extrabold text-foreground text-base">{mixedBalance.orderTotalVnd.toLocaleString('vi-VN')} VNĐ</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Đã thanh toán:</span>
                <span className="font-bold text-emerald-700 text-base">{mixedBalance.totalPaidVnd.toLocaleString('vi-VN')} VNĐ</span>
              </div>
              <div className="flex justify-between text-sm border-t border-border pt-2">
                <span className="font-bold text-foreground">Còn lại cần thu:</span>
                <span className={`font-extrabold text-base ${mixedBalance.balanceRemainingVnd === 0 ? 'text-emerald-700' : 'text-rose-700'}`}>
                  {mixedBalance.balanceRemainingVnd.toLocaleString('vi-VN')} VNĐ
                </span>
              </div>
            </div>

            <div className="space-y-2">
              <h3 className="text-sm font-bold text-foreground">Các phương thức đã thanh toán:</h3>
              {payments.map((p) => (
                <div key={p.id} className="p-3 rounded-lg border border-border bg-surface flex justify-between items-center text-sm">
                  <span className="font-semibold text-foreground">{p.method}</span>
                  <span className="font-bold text-foreground">{p.amountVnd.toLocaleString('vi-VN')} đ</span>
                </div>
              ))}
            </div>

            {mixedBalance.isFullyPaid && (
              <div className="p-3 rounded-lg bg-emerald-100 border border-emerald-300 text-emerald-800 text-sm font-bold text-center">
                ✓ Hóa đơn đã được thanh toán đủ 100%! Ready to checkout.
              </div>
            )}
          </div>
        </div>
      )}

      {activeTab === 'crosssell' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">💡 Gợi Ý Bán Kèm Sản Phẩm Cho Thu Ngân (UC_POS_041)</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {recommendations.map((rec) => (
              <div key={rec.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 space-y-2 flex flex-col justify-between">
                <div>
                  <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong mb-2 inline-block">{rec.code}</span>
                  <h3 className="font-bold text-foreground text-sm">{rec.name}</h3>
                  <p className="text-xs text-muted-foreground mt-1">{rec.reason}</p>
                </div>
                <div className="flex justify-between items-center pt-3 border-t border-border">
                  <span className="font-extrabold text-foreground text-sm">{rec.price.toLocaleString('vi-VN')} đ</span>
                  <button
                    onClick={() => showToast(`Đã thêm [${rec.name}] vào đơn hàng!`, 'success')}
                    className="px-3 py-1 bg-brand text-brand-foreground text-xs font-bold rounded-lg hover:opacity-90"
                  >
                    + Thêm Ngay
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'drawer' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">💵 Quản Lý Nộp Tiền / Rút Tiền Ca Bán Hàng (UC_POS_044)</h2>

            <div className="grid grid-cols-3 gap-4">
              <div className="p-4 rounded-xl border border-border bg-surface-hover/50">
                <span className="text-xs font-semibold text-muted-foreground block">Tiền Đầu Ca</span>
                <span className="text-lg font-bold text-foreground mt-1 block">1.000.000 đ</span>
              </div>
              <div className="p-4 rounded-xl border border-border bg-surface-hover/50">
                <span className="text-xs font-semibold text-muted-foreground block">Tổng Nộp Trong Ca</span>
                <span className="text-lg font-bold text-emerald-700 mt-1 block">+{cashInTotal.toLocaleString('vi-VN')} đ</span>
              </div>
              <div className="p-4 rounded-xl border border-border bg-surface-hover/50">
                <span className="text-xs font-semibold text-muted-foreground block">Tổng Rút Trong Ca</span>
                <span className="text-lg font-bold text-rose-700 mt-1 block">-{cashOutTotal.toLocaleString('vi-VN')} đ</span>
              </div>
            </div>

            <div className="p-4 rounded-xl bg-brand-muted border border-brand/30 flex justify-between items-center">
              <span className="font-bold text-foreground">Cân Bằng Tiền Mặt Hiện Tại Trong Ngăn Kéo:</span>
              <span className="text-xl font-extrabold text-brand-strong">{netDrawerBalance.toLocaleString('vi-VN')} VNĐ</span>
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">📝 Ghi Nhận Giao Dịch Ca</h2>
            <div className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Số tiền (VNĐ):</label>
                <input
                  type="number"
                  value={drawerForm.amount}
                  onChange={(e) => setDrawerForm({ ...drawerForm, amount: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Lý do nộp/rút tiền:</label>
                <input
                  type="text"
                  value={drawerForm.reason}
                  onChange={(e) => setDrawerForm({ ...drawerForm, reason: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
              <div className="flex gap-2">
                <button
                  onClick={handleCashIn}
                  className="flex-1 py-2.5 bg-emerald-600 text-white rounded-lg font-bold hover:bg-emerald-700 shadow-sm"
                >
                  + Nộp Tiền Ca
                </button>
                <button
                  onClick={handleCashOut}
                  className="flex-1 py-2.5 bg-rose-600 text-white rounded-lg font-bold hover:bg-rose-700 shadow-sm"
                >
                  - Rút Tiền Ca
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
