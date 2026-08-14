'use client';

import React, { useState } from 'react';
import {
  calculateBlanketContractRemaining,
  checkContractExpirationRisk,
} from '@/shared/api/pur-advance-blanket-contract-expiration-helpers';

export default function PurAdvanceBlanketContractExpirationPage() {
  const [activeTab, setActiveTab] = useState<'advance' | 'blanket' | 'tracking' | 'alert'>('advance');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PUR_044: Tạm ứng nhà cung cấp
  const [advanceForm, setAdvanceForm] = useState({ poNumber: 'PO-202608-001', supplier: 'Vinamilk Co.', amount: 50000000, reason: 'Tạm ứng đặt cọc 30% nguyên liệu' });

  const handleCreateAdvance = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã gửi đề nghị tạm ứng ${advanceForm.amount.toLocaleString('vi-VN')} VNĐ cho ${advanceForm.supplier} thành công!`, 'success');
  };

  // UC_PUR_045, UC_PUR_046, UC_PUR_047: Hợp đồng khung, theo dõi sản lượng & Cảnh báo hết hạn
  const [contracts] = useState([
    {
      id: 'c-1',
      number: 'BPO-2026-VINAMILK',
      title: 'Hợp Đồng Khung Cung Cấp Sữa Tươi 2026',
      supplier: 'Vinamilk Co.',
      totalValue: 500000000,
      consumedValue: 320000000,
      totalQty: 20000,
      consumedQty: 12800,
      expDate: new Date(Date.now() + 18 * 24 * 60 * 60 * 1000).toISOString(),
    },
    {
      id: 'c-2',
      number: 'BPO-2026-TRUNGNGUYEN',
      title: 'Hợp Đồng Khung Mua Cà Phê Hạt Quý 3/2026',
      supplier: 'Trung Nguyên Corp',
      totalValue: 800000000,
      consumedValue: 150000000,
      totalQty: 4000,
      consumedQty: 750,
      expDate: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString(),
    },
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
              PUR - VENDOR DOWN PAYMENT, BLANKET CONTRACTS & EXPIRATION ALERTS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Tạm Ứng NCC, Hợp Đồng Mua Khung, Theo Dõi Sản Lượng & Cảnh Báo Hết Hạn</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý đề nghị tạm ứng trước tiền mua hàng, theo dõi hạn mức sản lượng/giá trị còn lại của hợp đồng khung và phát cảnh báo gia hạn hợp đồng
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
            onClick={() => setActiveTab('advance')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'advance' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💵 UC_PUR_044: Tạm Ứng Nhà Cung Cấp
          </button>
          <button
            onClick={() => setActiveTab('blanket')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'blanket' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📜 UC_PUR_045: Hợp Đồng Mua Khung
          </button>
          <button
            onClick={() => setActiveTab('tracking')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'tracking' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_PUR_046: Theo Dõi Sản Lượng & Giá Trị Còn Lại
          </button>
          <button
            onClick={() => setActiveTab('alert')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'alert' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⏰ UC_PUR_047: Cảnh Báo Hết Hạn Hợp Đồng
          </button>
        </div>
      </div>

      {activeTab === 'advance' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-xl space-y-4">
          <h2 className="text-lg font-bold text-foreground">💵 Đề Nghị Tạm Ứng Cho Nhà Cung Cấp (UC_PUR_044)</h2>
          <form onSubmit={handleCreateAdvance} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Mã PO Đặt Hàng:</label>
              <input type="text" value={advanceForm.poNumber} readOnly className="w-full border border-border rounded-lg p-2 bg-surface-hover text-foreground font-bold" />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Nhà Cung Cấp Thụ Hưởng:</label>
              <input type="text" value={advanceForm.supplier} readOnly className="w-full border border-border rounded-lg p-2 bg-surface-hover text-foreground font-bold" />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Số tiền đề nghị tạm ứng (VNĐ):</label>
              <input
                type="number"
                value={advanceForm.amount}
                onChange={(e) => setAdvanceForm({ ...advanceForm, amount: Number(e.target.value) })}
                className="w-full border border-border rounded-lg p-2.5 bg-surface text-foreground font-extrabold text-brand-strong"
              />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Lý do / Điều khoản tạm ứng:</label>
              <textarea
                value={advanceForm.reason}
                onChange={(e) => setAdvanceForm({ ...advanceForm, reason: e.target.value })}
                rows={2}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>
            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              💵 Gửi Đề Nghị Tạm Ứng Khoản Đặt Cọc
            </button>
          </form>
        </div>
      )}

      {activeTab === 'blanket' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📜 Danh Sách Hợp Đồng Mua Khung (Blanket Contracts) (UC_PUR_045)</h2>
          <div className="space-y-3">
            {contracts.map((c) => (
              <div key={c.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="px-2.5 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong border border-brand/30">{c.number}</span>
                    <h3 className="font-bold text-foreground">{c.title}</h3>
                  </div>
                  <p className="text-xs text-muted-foreground mt-1">
                    Nhà cung cấp: <b className="text-foreground">{c.supplier}</b> | Hạn mức giá trị: <b className="text-brand-strong">{c.totalValue.toLocaleString('vi-VN')} VNĐ</b>
                  </p>
                </div>
                <span className="px-3 py-1 bg-emerald-100 text-emerald-800 text-xs font-bold rounded-full border border-emerald-300">
                  ● HỢP ĐỒNG ĐANG HIỆU LỰC
                </span>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'tracking' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📊 Theo Dõi Sản Lượng & Giá Trị Hạn Mức Còn Lại (UC_PUR_046)</h2>
          <div className="space-y-4">
            {contracts.map((c) => {
              const rem = calculateBlanketContractRemaining(c.totalValue, c.consumedValue, c.totalQty, c.consumedQty);
              return (
                <div key={c.id} className="p-5 rounded-xl border border-border bg-surface space-y-3">
                  <div className="flex justify-between items-center">
                    <div>
                      <span className="text-xs font-bold text-brand-strong">{c.number}</span>
                      <h3 className="font-bold text-foreground text-base">{c.title} ({c.supplier})</h3>
                    </div>
                    <span className="text-sm font-black text-brand-strong">Đã tiêu thụ: {rem.consumedPercentage}%</span>
                  </div>

                  {/* Thanh Tiến Độ Tiêu Thụ */}
                  <div className="w-full bg-slate-200 h-3 rounded-full overflow-hidden">
                    <div className="bg-brand h-full transition-all" style={{ width: `${rem.consumedPercentage}%` }}></div>
                  </div>

                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 pt-2 text-xs">
                    <div className="p-2.5 rounded-lg border border-border bg-surface-hover/30">
                      <span className="text-muted-foreground block">Tổng Giá Trị Khung:</span>
                      <b className="text-foreground text-sm font-bold">{c.totalValue.toLocaleString('vi-VN')} đ</b>
                    </div>
                    <div className="p-2.5 rounded-lg border border-border bg-surface-hover/30">
                      <span className="text-muted-foreground block">Đã Sử Dụng (PO):</span>
                      <b className="text-slate-700 text-sm font-bold">{c.consumedValue.toLocaleString('vi-VN')} đ</b>
                    </div>
                    <div className="p-2.5 rounded-lg border border-brand/30 bg-brand-muted/20">
                      <span className="text-brand-strong block">Giá Trị Còn Lại:</span>
                      <b className="text-brand-strong text-sm font-extrabold">{rem.remainingValue.toLocaleString('vi-VN')} đ</b>
                    </div>
                    <div className="p-2.5 rounded-lg border border-brand/30 bg-brand-muted/20">
                      <span className="text-brand-strong block">Sản Lượng Còn Lại:</span>
                      <b className="text-brand-strong text-sm font-extrabold">{rem.remainingQty.toLocaleString('vi-VN')} đơn vị</b>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {activeTab === 'alert' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⏰ Cảnh Báo Hết Hạn Hợp Đồng Mua Hàng (&lt;= 30 ngày) (UC_PUR_047)</h2>
          <div className="space-y-3">
            {contracts.map((c) => {
              const risk = checkContractExpirationRisk(c.expDate, 30);
              if (!risk.isExpiringSoon && !risk.isExpired) return null;
              return (
                <div key={c.id} className="p-4 rounded-xl border border-amber-300 bg-amber-50/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-black rounded bg-amber-500 text-white">EXPIRING SOON</span>
                      <h3 className="font-bold text-foreground">{c.title} ({c.number})</h3>
                    </div>
                    <p className="text-xs text-amber-800 mt-1 font-medium">
                      Nhà cung cấp <b className="text-foreground">{c.supplier}</b> — Hợp đồng sẽ hết hạn trong <b className="text-amber-950 font-black">{risk.daysLeft} ngày tới</b>
                    </p>
                  </div>
                  <button
                    onClick={() => showToast(`✓ Đã phát hành yêu cầu đàm phán gia hạn hợp đồng [${c.number}]!`, 'success')}
                    className="px-3.5 py-2 bg-amber-600 text-white text-xs font-bold rounded-lg hover:bg-amber-700 shadow-sm"
                  >
                    📝 Đàm Phán Gia Hạn Hợp Đồng
                  </button>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
