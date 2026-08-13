'use client';

import React, { useState } from 'react';
import {
  validateSplitBillSelection,
  validateKitchenNoteLength,
} from '@/shared/api/pos-promo-report-bill-order-ops-helpers';

export default function PosPromoReportBillOrderOpsPage() {
  const [activeTab, setActiveTab] = useState<'report' | 'splitmerge' | 'transfer' | 'notes'>('report');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_POS_025: Báo cáo khuyến mại
  const [promoReport] = useState({
    totalApplied: 305,
    totalDiscountVnd: 32150000,
    details: [
      { id: 'pr-1', code: 'HAPPY-HOUR-20', name: 'Happy Hour Chiều Giảm 20%', count: 142, discount: 14200000, revenue: 71000000 },
      { id: 'pr-2', code: 'COMBO-BREAKFAST', name: 'Combo Bữa Sáng Bánh Mì + Cà Phê', count: 98, discount: 14700000, revenue: 44100000 },
      { id: 'pr-3', code: 'VOUCHER-SUMMER', name: 'Voucher Khai Trương Giảm 50K', count: 65, discount: 3250000, revenue: 32500000 },
    ],
  });

  // UC_POS_028: Tách bill / gộp bill
  const [orderItems, setOrderItems] = useState([
    { id: 'item-1', name: 'Cà Phê Sữa Đá (Size L)', price: 35000, selected: false },
    { id: 'item-2', name: 'Trà Đào Cam Sả', price: 45000, selected: false },
    { id: 'item-3', name: 'Bánh Mì Kẹp Thịt Nướng', price: 25000, selected: false },
  ]);

  const toggleSelectItem = (id: string) => {
    setOrderItems((prev) =>
      prev.map((it) => (it.id === id ? { ...it, selected: !it.selected } : it))
    );
  };

  const handleSplitBill = () => {
    const selectedIds = orderItems.filter((i) => i.selected).map((i) => i.id);
    const val = validateSplitBillSelection(selectedIds);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    setOrderItems((prev) => prev.filter((i) => !i.selected));
    showToast(`Đã tách ${selectedIds.length} món sang hóa đơn thanh toán mới!`, 'success');
  };

  // UC_POS_029: Chuyển đơn giữa quầy
  const [transferForm, setTransferForm] = useState({ fromCounter: 'POS01', toCounter: 'POS02', notes: 'Chuyển đơn thanh toán quầy 2' });

  const handleTransferCounter = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`Đã chuyển đơn hàng từ Quầy [${transferForm.fromCounter}] sang Quầy [${transferForm.toCounter}]!`, 'success');
  };

  // UC_POS_030: Ghi chú đơn hàng & Bếp
  const [notesForm, setNotesForm] = useState({ customerNotes: 'Giao trước 12h30', kitchenInstructions: 'Ít đường, không đá, không cay' });

  const handleSaveNotes = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateKitchenNoteLength(notesForm.kitchenInstructions);
    if (!val.isValid) {
      showToast(val.error || 'Ghi chú không hợp lệ.', 'error');
      return;
    }
    showToast('✓ Đã cập nhật ghi chú đơn hàng & hướng dẫn chế biến nhà bếp thành công!', 'success');
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
              POS - PROMO ANALYTICS, BILL OPERATIONS & KITCHEN NOTES
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Báo Cáo Khuyến Mại, Tách/Gộp Bill & Ghi Chú Đơn Bếp</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Phân tích hiệu quả CTKM tại quầy, thao tác tách/gộp bill linh hoạt, chuyển đơn giữa quầy và ghi chú bếp
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
            onClick={() => setActiveTab('report')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'report' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_POS_025: Báo Cáo Khuyến Mại
          </button>
          <button
            onClick={() => setActiveTab('splitmerge')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'splitmerge' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ✂️ UC_POS_028: Tách Bill / Gộp Bill
          </button>
          <button
            onClick={() => setActiveTab('transfer')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'transfer' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔄 UC_POS_029: Chuyển Đơn Giữa Quầy
          </button>
          <button
            onClick={() => setActiveTab('notes')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'notes' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📝 UC_POS_030: Ghi Chú Đơn Hàng & Bếp
          </button>
        </div>
      </div>

      {activeTab === 'report' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="bg-surface p-5 rounded-xl border border-border">
              <span className="text-xs font-semibold text-muted-foreground block">TỔNG LẦN ÁP DỤNG KHUYẾN MẠI</span>
              <span className="text-2xl font-bold text-foreground mt-1 block">{promoReport.totalApplied} lượt áp dụng</span>
            </div>
            <div className="bg-surface p-5 rounded-xl border border-border">
              <span className="text-xs font-semibold text-muted-foreground block">TỔNG TIỀN ƯU ĐÃI ĐÃ GIẢM</span>
              <span className="text-2xl font-bold text-brand-strong mt-1 block">{promoReport.totalDiscountVnd.toLocaleString('vi-VN')} VNĐ</span>
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">Thống Kê Chi Tiết Khuyến Mại Tại Quầy POS (UC_POS_025)</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Mã & Tên Chương Trình</th>
                    <th className="p-3">Số Lần Sử Dụng</th>
                    <th className="p-3">Tổng Tiền Giảm</th>
                    <th className="p-3 font-bold">Doanh Số Kéo Theo</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {promoReport.details.map((d) => (
                    <tr key={d.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-bold text-foreground">
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong mr-2">{d.code}</span>
                        {d.name}
                      </td>
                      <td className="p-3 text-slate-700">{d.count} lượt</td>
                      <td className="p-3 text-rose-700 font-medium">{d.discount.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 font-extrabold text-foreground">{d.revenue.toLocaleString('vi-VN')} đ</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'splitmerge' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div>
            <h2 className="text-lg font-bold text-foreground">✂️ Thao Tác Tách Bill / Gộp Hóa Đơn (UC_POS_028)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Chọn các món trong hóa đơn hiện tại để tách sang bill mới cho khách thanh toán riêng</p>
          </div>

          <div className="space-y-3">
            {orderItems.map((item) => (
              <div
                key={item.id}
                onClick={() => toggleSelectItem(item.id)}
                className={`p-4 rounded-xl border cursor-pointer flex justify-between items-center transition-all ${
                  item.selected ? 'bg-brand-muted border-brand/40 shadow-sm' : 'bg-surface-hover/50 border-border'
                }`}
              >
                <div className="flex items-center gap-3">
                  <input type="checkbox" checked={item.selected} onChange={() => {}} className="w-4 h-4 text-brand" />
                  <span className="font-bold text-foreground">{item.name}</span>
                </div>
                <span className="font-extrabold text-foreground">{item.price.toLocaleString('vi-VN')} VNĐ</span>
              </div>
            ))}
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <button
              onClick={handleSplitBill}
              className="px-5 py-2.5 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm"
            >
              ✂️ Thực Hiện Tách Bill Cho Món Đã Chọn
            </button>
          </div>
        </div>
      )}

      {activeTab === 'transfer' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 max-w-lg">
          <h2 className="text-lg font-bold text-foreground mb-4">🔄 Chuyển Đơn Hàng Sang Quầy Thu Ngân Khác (UC_POS_029)</h2>
          <form onSubmit={handleTransferCounter} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Từ quầy hiện tại:</label>
              <input type="text" value={transferForm.fromCounter} disabled className="w-full border border-border rounded-lg p-2 bg-slate-100 font-semibold" />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Đến quầy thu ngân đích:</label>
              <select
                value={transferForm.toCounter}
                onChange={(e) => setTransferForm({ ...transferForm, toCounter: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              >
                <option value="POS02">Quầy Thu Ngân 02 (Khu vực Tầng 1)</option>
                <option value="POS03">Quầy Thu Ngân 03 (Khu vực Tầng 2)</option>
                <option value="POS-TAKEAWAY">Quầy Mang Về (Takeaway)</option>
              </select>
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Ghi chú chuyển quầy:</label>
              <input
                type="text"
                value={transferForm.notes}
                onChange={(e) => setTransferForm({ ...transferForm, notes: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>
            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
              Xác Nhận Chuyển Đơn Quầy
            </button>
          </form>
        </div>
      )}

      {activeTab === 'notes' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 max-w-lg">
          <h2 className="text-lg font-bold text-foreground mb-4">📝 Ghi Chú Đơn Hàng & Hướng Dẫn Chế Biến Bếp (UC_POS_030)</h2>
          <form onSubmit={handleSaveNotes} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Ghi chú giao hàng / khách hàng:</label>
              <input
                type="text"
                value={notesForm.customerNotes}
                onChange={(e) => setNotesForm({ ...notesForm, customerNotes: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                placeholder="VD: Giao trước 12h30"
              />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Yêu cầu chế biến đặc biệt (In lệnh Bếp):</label>
              <textarea
                rows={3}
                value={notesForm.kitchenInstructions}
                onChange={(e) => setNotesForm({ ...notesForm, kitchenInstructions: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                placeholder="VD: Ít đường, không đá, nhiều hành..."
              />
            </div>
            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
              Lưu Ghi Chú Bếp
            </button>
          </form>
        </div>
      )}
    </div>
  );
}
