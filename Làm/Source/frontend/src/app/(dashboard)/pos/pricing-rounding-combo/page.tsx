'use client';

import React, { useState } from 'react';
import {
  calculatePosCashRounding,
  formatComboDiscountSavings,
} from '@/shared/api/pos-pricing-rounding-combo-helpers';

export default function PosPricingRoundingComboPage() {
  const [activeTab, setActiveTab] = useState<'timeslot' | 'rounding' | 'combo'>('timeslot');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_POS_017 & UC_POS_018: Giá theo khung giờ & ngày trong tuần
  const [timeRules, setTimeRules] = useState([
    { id: 'tr-1', name: 'Happy Hour Cà Phê Chiều (14h - 17h)', time: '14:00 - 17:00', days: 'Thứ 2 - Thứ 6', price: 25000, discount: 20, active: true },
    { id: 'tr-2', name: 'Ưu Đãi Trà Sữa Cuối Tuần', time: '08:00 - 22:00', days: 'Thứ 7, Chủ Nhật', price: 35000, discount: 15, active: true },
  ]);

  const [ruleForm, setRuleForm] = useState({ name: '', time: '14:00 - 17:00', days: 'Thứ 2 - Thứ 6', price: 25000, discount: 20 });

  const handleSaveTimeRule = (e: React.FormEvent) => {
    e.preventDefault();
    if (!ruleForm.name.trim()) {
      showToast('Tên quy tắc giá không được để trống.', 'error');
      return;
    }

    const created = {
      id: `tr-${Date.now()}`,
      name: ruleForm.name,
      time: ruleForm.time,
      days: ruleForm.days,
      price: ruleForm.price,
      discount: ruleForm.discount,
      active: true,
    };

    setTimeRules([...timeRules, created]);
    setRuleForm({ name: '', time: '14:00 - 17:00', days: 'Thứ 2 - Thứ 6', price: 25000, discount: 20 });
    showToast(`Đã thêm quy tắc giá [${created.name}] thành công!`, 'success');
  };

  // UC_POS_020: Làm tròn tiền thanh toán
  const [testAmount, setTestAmount] = useState(123400);
  const roundingCalc = calculatePosCashRounding(testAmount, 500);

  // UC_POS_023: Khuyến mại theo combo
  const [combos] = useState([
    { id: 'cb-1', code: 'COMBO-BREAKFAST', name: 'Combo Bữa Sáng: Bánh Mì + Cà Phê Sữa', original: 60000, fixedPrice: 45000, active: true },
    { id: 'cb-2', code: 'COMBO-LUNCH', name: 'Combo Bữa Trưa: Cơm Tấm + Trà Đá', original: 70000, fixedPrice: 55000, active: true },
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
              POS - PRICING, ROUNDING & COMBOS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Bảng Giá Theo Khung Giờ, Làm Tròn Tiền & Gói Combo POS</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Cấu hình bảng giá Happy Hour theo khung giờ/ngày trong tuần, quy tắc làm tròn tiền mặt và gói khuyến mại Combo
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
            onClick={() => setActiveTab('timeslot')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'timeslot' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⏰ UC_POS_017 & 018: Bảng Giá Theo Khung Giờ & Ngày
          </button>
          <button
            onClick={() => setActiveTab('rounding')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'rounding' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🪙 UC_POS_020: Quy Tắc Làm Tròn Tiền Thanh Toán
          </button>
          <button
            onClick={() => setActiveTab('combo')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'combo' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🍱 UC_POS_023: Khuyến Mại Theo Gói Combo
          </button>
        </div>
      </div>

      {activeTab === 'timeslot' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">⏰ Quy Tắc Giá Khung Giờ & Ngày Trong Tuần (UC_POS_017 & 018)</h2>
            <div className="space-y-3">
              {timeRules.map((tr) => (
                <div key={tr.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-foreground">{tr.name}</h3>
                    <p className="text-xs text-muted-foreground mt-1">Khung giờ: <span className="font-semibold text-foreground">{tr.time}</span> | Ngày áp dụng: {tr.days}</p>
                  </div>
                  <div className="text-right">
                    <span className="text-sm font-extrabold text-foreground block">{tr.price.toLocaleString('vi-VN')} VNĐ</span>
                    <span className="text-xs font-bold text-emerald-700 bg-emerald-100 px-2 py-0.5 rounded border border-emerald-300">
                      Giảm {tr.discount}%
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">➕ Thêm Quy Tắc Giá Mới</h2>
            <form onSubmit={handleSaveTimeRule} className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Tên quy tắc giá:</label>
                <input
                  type="text"
                  value={ruleForm.name}
                  onChange={(e) => setRuleForm({ ...ruleForm, name: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: Happy Hour Trưa T6"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Khung giờ áp dụng:</label>
                <input
                  type="text"
                  value={ruleForm.time}
                  onChange={(e) => setRuleForm({ ...ruleForm, time: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="14:00 - 17:00"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Giá khuyến mại (VNĐ):</label>
                <input
                  type="number"
                  value={ruleForm.price}
                  onChange={(e) => setRuleForm({ ...ruleForm, price: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
                Lưu Quy Tắc Giá
              </button>
            </form>
          </div>
        </div>
      )}

      {activeTab === 'rounding' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div>
            <h2 className="text-lg font-bold text-foreground">🪙 Mô Phỏng Làm Tròn Tiền Thanh Toán Tiền Mặt POS (UC_POS_020)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Tự động làm tròn tiền hóa đơn thanh toán tiền mặt theo bội số 500đ hoặc 1.000đ gần nhất</p>
          </div>

          <div className="max-w-md space-y-4">
            <div>
              <label className="block text-sm font-semibold text-foreground mb-1">Nhập tổng tiền đơn hàng gốc (VNĐ):</label>
              <input
                type="number"
                value={testAmount}
                onChange={(e) => setTestAmount(Number(e.target.value))}
                className="w-full border border-border rounded-lg p-2.5 text-lg font-bold bg-surface text-foreground"
              />
            </div>

            <div className="p-4 rounded-xl border border-border bg-surface-hover/50 space-y-3 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Tổng tiền thực tế đơn:</span>
                <span className="font-bold text-foreground">{roundingCalc.originalTotalVnd.toLocaleString('vi-VN')} VNĐ</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Quy tắc làm tròn:</span>
                <span className="font-semibold text-brand-strong">Làm tròn 500 VNĐ gần nhất</span>
              </div>
              <div className="flex justify-between text-base border-t border-border pt-2">
                <span className="font-bold text-foreground">Số tiền thanh toán sau làm tròn:</span>
                <span className="font-extrabold text-brand-strong">{roundingCalc.roundedTotalVnd.toLocaleString('vi-VN')} VNĐ</span>
              </div>
              <div className="flex justify-between text-xs">
                <span className="text-muted-foreground">Chênh lệch làm tròn:</span>
                <span className={`font-bold ${roundingCalc.roundingDifferenceVnd >= 0 ? 'text-emerald-700' : 'text-rose-700'}`}>
                  {roundingCalc.roundingDifferenceVnd >= 0 ? `+${roundingCalc.roundingDifferenceVnd}` : roundingCalc.roundingDifferenceVnd} VNĐ
                </span>
              </div>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'combo' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🍱 Gói Khuyến Mại Combo Sản Phẩm (UC_POS_023)</h2>
          <div className="space-y-3">
            {combos.map((cb) => {
              const savings = formatComboDiscountSavings(cb.fixedComboPrice, cb.original);
              return (
                <div key={cb.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{cb.code}</span>
                      <h3 className="font-bold text-foreground">{cb.name}</h3>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">
                      Giá lẻ gốc: <span className="line-through">{cb.original.toLocaleString('vi-VN')} đ</span> | Tiết kiệm: <span className="font-bold text-emerald-700">{savings.savingsVnd.toLocaleString('vi-VN')} đ ({savings.savingsPercent}%)</span>
                    </p>
                  </div>
                  <div className="text-right">
                    <span className="text-base font-extrabold text-foreground block">{cb.fixedComboPrice.toLocaleString('vi-VN')} VNĐ</span>
                    <span className="px-2.5 py-0.5 text-xs font-semibold rounded-full bg-brand-muted text-brand-strong border border-brand/30">
                      Gói Combo
                    </span>
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
