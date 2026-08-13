'use client';

import React, { useState } from 'react';
import {
  calculateReceivingDiscrepancy,
  determineDiscrepancySeverity,
} from '@/shared/api/pur-reject-return-delivery-protocol-discrepancy-helpers';

export default function PurRejectReturnDeliveryProtocolDiscrepancyPage() {
  const [activeTab, setActiveTab] = useState<'reject' | 'rtv' | 'protocol' | 'settle'>('reject');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PUR_036: Từ chối lô hàng không đạt QC
  const [rejections] = useState([
    { id: 'rej-1', number: 'REJ-20260813-001', po: 'PO-202608-001', supplier: 'Vinamilk Co.', qty: 50, reason: 'Bao bì rách hỏng và nắp hở', status: 'Quarantined' },
    { id: 'rej-2', number: 'REJ-20260812-005', po: 'PO-202608-003', supplier: 'Mộc Châu Milk', qty: 20, reason: 'Sản phẩm hết hạn sử dụng (Expired)', status: 'Quarantined' },
  ]);

  // UC_PUR_038: Trả hàng nhà cung cấp (RTV)
  const handleIssueRtv = (rejNumber: string, supplier: string) => {
    showToast(`✓ Đã lập phiếu xuất trả hàng (RTV - Return to Vendor) thành công cho lô từ chối [${rejNumber}] (${supplier})!`, 'success');
  };

  // UC_PUR_039: Biên bản giao nhận & UC_PUR_042: Xử lý chênh lệch
  const [protocolForm, setProtocolForm] = useState({
    poNumber: 'PO-202608-001',
    supplier: 'Vinamilk Co.',
    driver: 'Trần Văn Bằng (29C-123.45)',
    orderedQty: 100,
    receivedQty: 95,
    unitPrice: 240000,
    action: 'AdjustInvoiceAmount',
  });

  const calc = calculateReceivingDiscrepancy(protocolForm.orderedQty, protocolForm.receivedQty, protocolForm.unitPrice);
  const severity = determineDiscrepancySeverity(calc.diffAmountVnd);

  const handleSignProtocol = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã lập Biên bản giao nhận & Xử lý chênh lệch thiếu ${calc.diffQty} sản phẩm (${calc.diffAmountVnd.toLocaleString('vi-VN')} đ) thành công!`, 'success');
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
              PUR - REJECTED SHIPMENT, RTV, DELIVERY PROTOCOL & VARIANCE SETTLEMENT
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Từ Chối Lô Hàng QC, Trả Hàng NCC (RTV), Biên Bản Giao Nhận & Xử Lý Chênh Lệch</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý cách ly hàng không đạt tiêu chuẩn QC, phát hành phiếu RTV xuất trả NCC, ký biên bản giao nhận và quyết toán chênh lệch hóa đơn
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
            onClick={() => setActiveTab('reject')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'reject' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🚫 UC_PUR_036: Từ Chối Lô Hàng Không Đạt
          </button>
          <button
            onClick={() => setActiveTab('rtv')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'rtv' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🚚 UC_PUR_038: Trả Hàng NCC (RTV)
          </button>
          <button
            onClick={() => setActiveTab('protocol')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'protocol' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📝 UC_PUR_039: Biên Bản Giao Nhận
          </button>
          <button
            onClick={() => setActiveTab('settle')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'settle' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚖️ UC_PUR_042: Xử Lý Chênh Lệch Hàng Hóa
          </button>
        </div>
      </div>

      {activeTab === 'reject' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🚫 Danh Sách Lô Hàng Bị QC Từ Chối Nhập Kho (UC_PUR_036)</h2>
          <div className="space-y-3">
            {rejections.map((r) => (
              <div key={r.id} className="p-4 rounded-xl border border-rose-200 bg-rose-50/40 flex justify-between items-center">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="px-2 py-0.5 text-xs font-bold rounded bg-rose-600 text-white">{r.number}</span>
                    <h3 className="font-bold text-foreground">{r.supplier} ({r.po})</h3>
                  </div>
                  <p className="text-xs text-rose-700 mt-1">
                    Số lượng từ chối: <b className="text-foreground">{r.qty} đơn vị</b> | Lý do: <i>"{r.reason}"</i>
                  </p>
                </div>
                <span className="px-3 py-1 bg-rose-100 text-rose-800 text-xs font-bold rounded-full border border-rose-300">
                  🔒 CÁCH LY KHO (QUARANTINED)
                </span>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'rtv' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🚚 Lập Phiếu Xuất Trả Hàng Cho Nhà Cung Cấp (RTV) (UC_PUR_038)</h2>
          <div className="space-y-3">
            {rejections.map((r) => (
              <div key={r.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{r.number}</span>
                    <h3 className="font-bold text-foreground">{r.supplier}</h3>
                  </div>
                  <p className="text-xs text-muted-foreground mt-1">
                    Xuất trả kho cách ly: <b>{r.qty} sản phẩm lỗi</b> | PO gốc: <b>{r.po}</b>
                  </p>
                </div>
                <button
                  onClick={() => handleIssueRtv(r.number, r.supplier)}
                  className="px-4 py-2 bg-brand text-brand-foreground rounded-lg font-bold text-xs hover:opacity-90 shadow-sm"
                >
                  🚚 Phát Hành Phiếu RTV Xuất Trả
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {(activeTab === 'protocol' || activeTab === 'settle') && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-5">
          <div>
            <h2 className="text-lg font-bold text-foreground">📝 Biên Bản Giao Nhận & Quyết Toán Chênh Lệch Hàng Hóa (UC_PUR_039 & UC_PUR_042)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Xác nhận thực tế số lượng hàng nhận với bên vận chuyển / NCC và thống nhất phương án xử lý chênh lệch</p>
          </div>

          <form onSubmit={handleSignProtocol} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã PO:</label>
                <input type="text" value={protocolForm.poNumber} readOnly className="w-full border border-border rounded-lg p-2 bg-surface-hover text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tài xế / Số xe giao:</label>
                <input
                  type="text"
                  value={protocolForm.driver}
                  onChange={(e) => setProtocolForm({ ...protocolForm, driver: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
            </div>

            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">SL Đặt (PO):</label>
                <input
                  type="number"
                  value={protocolForm.orderedQty}
                  onChange={(e) => setProtocolForm({ ...protocolForm, orderedQty: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">SL Thực Nhận:</label>
                <input
                  type="number"
                  value={protocolForm.receivedQty}
                  onChange={(e) => setProtocolForm({ ...protocolForm, receivedQty: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold text-brand-strong"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Đơn Giá (VNĐ):</label>
                <input
                  type="number"
                  value={protocolForm.unitPrice}
                  onChange={(e) => setProtocolForm({ ...protocolForm, unitPrice: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
            </div>

            <div className="p-4 rounded-xl border border-amber-300 bg-amber-50/50 space-y-2">
              <div className="flex justify-between items-center">
                <span className="text-xs font-bold text-amber-800">KẾT QUẢ KIỂM ĐẾM CHÊNH LỆCH GIAO HÀNG:</span>
                <span className={`px-2 py-0.5 text-xs font-black rounded ${severity === 'Minor' ? 'bg-slate-200 text-slate-800' : 'bg-amber-500 text-white'}`}>
                  Mức độ: {severity}
                </span>
              </div>
              <p className="text-sm font-bold text-foreground">
                Thực nhận thiếu: <span className="text-rose-600 font-extrabold">{calc.diffQty} sản phẩm</span> | Giá trị chênh lệch: <span className="text-rose-600 font-extrabold">{calc.diffAmountVnd.toLocaleString('vi-VN')} VNĐ</span>
              </p>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Phương án xử lý chênh lệch (UC_PUR_042):</label>
              <select
                value={protocolForm.action}
                onChange={(e) => setProtocolForm({ ...protocolForm, action: e.target.value })}
                className="w-full border border-border rounded-lg p-2.5 bg-surface text-foreground font-bold"
              >
                <option value="AdjustInvoiceAmount">1. Trừ tiền trực tiếp trên Hóa Đơn (Giảm công nợ)</option>
                <option value="DemandSupplierReplacement">2. Yêu cầu NCC giao bù đủ số thiếu</option>
                <option value="WaiveDiscrepancy">3. Bỏ qua chênh lệch (Dung sai chấp nhận được)</option>
              </select>
            </div>

            <button type="submit" className="w-full py-3 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm">
              ✍️ Ký Biên Bản Giao Nhận & Chốt Xử Lý Chênh Lệch
            </button>
          </form>
        </div>
      )}
    </div>
  );
}
