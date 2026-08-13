'use client';

import React, { useState } from 'react';
import {
  rankQuotationsByLowestPrice,
  validatePoConfirmationStatus,
} from '@/shared/api/pur-quotation-compare-award-po-confirm-helpers';

export default function PurQuotationCompareAwardPoConfirmPage() {
  const [activeTab, setActiveTab] = useState<'quotation' | 'compare' | 'award' | 'poconfirm'>('quotation');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PUR_022: Nhập báo giá từ NCC
  const [quoForm, setQuoForm] = useState({ supplierName: '', number: '', total: 25000000, leadTime: 3, terms: 'Net 30' });

  const handleSaveQuotation = (e: React.FormEvent) => {
    e.preventDefault();
    if (!quoForm.supplierName || !quoForm.number) {
      showToast('Vui lòng nhập Tên NCC và Số báo giá.', 'error');
      return;
    }
    showToast(`✓ Đã cập nhật phiếu báo giá [${quoForm.number}] từ ${quoForm.supplierName} thành công!`, 'success');
  };

  // UC_PUR_023 & UC_PUR_024: So sánh & Chọn NCC thắng
  const [quotationsList, setQuotationsList] = useState([
    { id: 'q-1', supplierName: 'Vinamilk Co.', totalAmountVnd: 24000000, leadTimeDays: 3, terms: 'Net 30', awarded: true },
    { id: 'q-2', supplierName: 'Mộc Châu Milk', totalAmountVnd: 25500000, leadTimeDays: 5, terms: 'Net 15', awarded: false },
    { id: 'q-3', supplierName: 'TH True Milk', totalAmountVnd: 26000000, leadTimeDays: 2, terms: 'Net 45', awarded: false },
  ]);

  const rankedQuotations = rankQuotationsByLowestPrice(quotationsList);

  const handleAwardWinner = (id: string, name: string) => {
    setQuotationsList((prev) =>
      prev.map((q) => ({ ...q, awarded: q.id === id }))
    );
    showToast(`🏆 Đã trao thầu / phê duyệt nhà cung cấp [${name}] thắng thầu gói mua sắm!`, 'success');
  };

  // UC_PUR_029: Xác nhận PO từ nhà cung cấp
  const [poConfirmations] = useState([
    { id: 'poc-1', poNumber: 'PO-202608-001', supplier: 'Vinamilk Co.', deliveryDate: '18/08/2026', status: 'Confirmed', comments: 'Xác nhận sẵn sàng giao đủ 1.000 thùng' },
    { id: 'poc-2', poNumber: 'PO-202608-002', supplier: 'Mộc Châu Milk', deliveryDate: '22/08/2026', status: 'ConfirmedWithChanges', comments: 'Đề nghị lùi 2 ngày do gián đoạn vận chuyển' },
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
              PUR - QUOTATIONS, MATRIX COMPARISON, AWARD WINNER & PO CONFIRMATION
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Nhập Báo Giá, So Sánh Ma Trận Cạnh Tranh, Phê Duyệt NCC & Xác Nhận PO</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Nhập chi tiết báo giá từ NCC, phân tích ma trận giá/điều kiện thanh toán, chốt NCC thắng thầu và theo dõi xác nhận PO từ NCC
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
            onClick={() => setActiveTab('quotation')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'quotation' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📄 UC_PUR_022: Nhập Báo Giá NCC
          </button>
          <button
            onClick={() => setActiveTab('compare')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'compare' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_PUR_023: So Sánh Ma Trận Báo Giá
          </button>
          <button
            onClick={() => setActiveTab('award')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'award' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏆 UC_PUR_024: Chọn NCC Thắng Thầu
          </button>
          <button
            onClick={() => setActiveTab('poconfirm')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'poconfirm' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ✅ UC_PUR_029: Xác Nhận PO Từ NCC
          </button>
        </div>
      </div>

      {activeTab === 'quotation' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-xl space-y-4">
          <h2 className="text-lg font-bold text-foreground">📄 Nhập Chi Tiết Báo Giá Từ Nhà Cung Cấp (UC_PUR_022)</h2>
          <form onSubmit={handleSaveQuotation} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Tên Nhà Cung Cấp:</label>
              <input
                type="text"
                value={quoForm.supplierName}
                onChange={(e) => setQuoForm({ ...quoForm, supplierName: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                placeholder="VD: Vinamilk Co."
              />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Số Báo Giá (Ref Number):</label>
              <input
                type="text"
                value={quoForm.number}
                onChange={(e) => setQuoForm({ ...quoForm, number: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                placeholder="VD: QUO-VIN-001"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Thời gian giao (ngày):</label>
                <input
                  type="number"
                  value={quoForm.leadTime}
                  onChange={(e) => setQuoForm({ ...quoForm, leadTime: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Điều khoản thanh toán:</label>
                <input
                  type="text"
                  value={quoForm.terms}
                  onChange={(e) => setQuoForm({ ...quoForm, terms: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
            </div>
            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
              Lưu Phiếu Báo Giá
            </button>
          </form>
        </div>
      )}

      {activeTab === 'compare' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📊 Bảng So Sánh Ma Trận Giá & Điều Kiện Giao Hàng (UC_PUR_023)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Thứ Hạng Giá</th>
                  <th className="p-3">Tên Nhà Cung Cấp</th>
                  <th className="p-3">Tổng Giá Trị Báo Giá</th>
                  <th className="p-3">Lead Time (Giao hàng)</th>
                  <th className="p-3">Điều Khoản Thanh Toán</th>
                  <th className="p-3 text-right">Đánh Giá Cạnh Tranh</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {rankedQuotations.map((q) => (
                  <tr key={q.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-extrabold text-foreground">
                      <span className={`w-6 h-6 inline-flex items-center justify-center rounded-full text-xs ${q.isBestValue ? 'bg-amber-400 text-amber-950 font-black' : 'bg-slate-200 text-slate-700'}`}>
                        #{q.rank}
                      </span>
                    </td>
                    <td className="p-3 font-bold text-foreground">{q.supplierName}</td>
                    <td className="p-3 font-extrabold text-brand-strong">{q.totalAmountVnd.toLocaleString('vi-VN')} VNĐ</td>
                    <td className="p-3 font-medium text-slate-700">{q.leadTimeDays} ngày</td>
                    <td className="p-3 text-slate-600">{q.terms}</td>
                    <td className="p-3 text-right">
                      {q.isBestValue ? (
                        <span className="px-2.5 py-1 text-xs font-black rounded-full bg-amber-100 text-amber-800 border border-amber-300">
                          ★ BEST VALUE (GIÁ TỐT NHẤT)
                        </span>
                      ) : (
                        <span className="px-2 py-0.5 text-xs font-semibold rounded bg-slate-100 text-slate-700">
                          Cạnh tranh vừa
                        </span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'award' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🏆 Phê Duyệt & Chọn Nhà Cung Cấp Thắng Thầu (UC_PUR_024)</h2>
          <div className="space-y-3">
            {quotationsList.map((q) => (
              <div key={q.id} className={`p-4 rounded-xl border flex justify-between items-center ${q.awarded ? 'border-amber-400 bg-amber-50/50' : 'border-border bg-surface-hover/30'}`}>
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="font-extrabold text-foreground text-base">{q.supplierName}</h3>
                    {q.awarded && <span className="px-2 py-0.5 text-xs font-bold rounded bg-amber-500 text-white">🏆 WINNER (ĐÃ CHỌN)</span>}
                  </div>
                  <p className="text-xs text-muted-foreground mt-1">
                    Giá chào: <b className="text-brand-strong">{q.totalAmountVnd.toLocaleString('vi-VN')} đ</b> | Lead time: <b className="text-foreground">{q.leadTimeDays} ngày</b>
                  </p>
                </div>
                {!q.awarded && (
                  <button
                    onClick={() => handleAwardWinner(q.id, q.supplierName)}
                    className="px-4 py-2 bg-brand text-brand-foreground rounded-lg text-sm font-bold hover:opacity-90 shadow-sm"
                  >
                    🏆 Phê Duyệt Thắng Thầu
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'poconfirm' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">✅ Xác Nhận Đơn Hàng Mua (PO) Từ Nhà Cung Cấp (UC_PUR_029)</h2>
          <div className="space-y-3">
            {poConfirmations.map((poc) => {
              const check = validatePoConfirmationStatus(poc.status);
              return (
                <div key={poc.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{poc.poNumber}</span>
                      <h3 className="font-bold text-foreground">{poc.supplier}</h3>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">
                      Ngày cam kết giao: <b className="text-foreground">{poc.deliveryDate}</b> | Phản hồi: <i>"{poc.comments}"</i>
                    </p>
                  </div>
                  <span className={`px-2.5 py-1 text-xs font-bold rounded ${check.isConfirmed ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'}`}>
                    {check.isConfirmed ? '✓ Đã Xác Nhận Đủ' : '⚠️ Cần Xem Xét Lại'}
                  </span>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
