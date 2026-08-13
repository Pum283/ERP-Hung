'use client';

import React, { useState } from 'react';
import {
  calculateQuickAuditDiscrepancy,
  validateReplenishmentItemsCount,
} from '@/shared/api/pos-history-replenish-receive-audit-helpers';

export default function PosHistoryReplenishReceiveAuditPage() {
  const [activeTab, setActiveTab] = useState<'history' | 'replenish' | 'receive' | 'audit'>('history');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_POS_053: Tra cứu lịch sử mua
  const [historyList] = useState([
    { id: 'h-1', code: 'POS-ORD-20260810-01', date: '10/08/2026 14:30', total: 150000, itemsCount: 3, status: 'Hoàn thành' },
    { id: 'h-2', code: 'POS-ORD-20260805-04', date: '05/08/2026 10:15', total: 280000, itemsCount: 5, status: 'Hoàn thành' },
  ]);

  // UC_POS_056: Đề nghị nhập hàng
  const [replenishItems, setReplenishItems] = useState([
    { id: 'item-1', code: 'SKU-MILK', name: 'Sữa Tươi NGUYÊN CHẤT 1L', qty: 24 },
    { id: 'item-2', code: 'SKU-BEANS', name: 'Cà Phê Hạt Arabica 1KG', qty: 10 },
  ]);

  const handleCreateReplenish = () => {
    const val = validateReplenishmentItemsCount(replenishItems.map((i) => ({ quantityRequested: i.qty })));
    if (!val.isValid) {
      showToast('Số lượng đề nghị nhập phải lớn hơn 0.', 'error');
      return;
    }
    showToast(`✓ Đã gửi đề nghị nhập ${val.totalQty} sản phẩm lên Kho Trung Tâm thành công!`, 'success');
  };

  // UC_POS_057: Nhận hàng từ kho trung tâm
  const [transfers] = useState([
    { id: 'trf-1', code: 'TRF-20260812-001', from: 'Kho Tổng (Central Warehouse)', itemsCount: 34, status: 'Chờ nhận hàng' },
  ]);

  const handleConfirmReceive = (code: string) => {
    showToast(`✓ Đã xác nhận nhập kho thành công phiếu điều chuyển [${code}]! Tồn kho cửa hàng đã được cộng tự động.`, 'success');
  };

  // UC_POS_058: Kiểm kê nhanh
  const [auditLines, setAuditLines] = useState([
    { id: 'al-1', code: 'SKU-MILK', name: 'Sữa Tươi 1L', systemStock: 24, actualStock: 22 },
    { id: 'al-2', code: 'SKU-BEANS', name: 'Cà Phê Hạt 1KG', systemStock: 10, actualStock: 10 },
  ]);

  const updateActualStock = (id: string, qty: number) => {
    setAuditLines((prev) =>
      prev.map((l) => (l.id === id ? { ...l, actualStock: qty } : l))
    );
  };

  const handleSubmitAudit = () => {
    const discrepancies = auditLines.filter((l) => l.actualStock !== l.systemStock).length;
    showToast(`✓ Đã chốt phiếu kiểm kê nhanh thành công! Phát hiện ${discrepancies} sản phẩm chênh lệch tồn.`, 'success');
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
              POS - PURCHASE HISTORY, REPLENISHMENT & STORE AUDIT
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Tra Cứu Lịch Sử Mua, Đề Nghị Nhập Hàng & Kiểm Kê Nhanh POS</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Xem lịch sử đơn hàng của khách, lập đề nghị nhập bổ sung kho, xác nhận nhận hàng từ kho tổng và kiểm kê nhanh
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
            onClick={() => setActiveTab('history')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'history' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📜 UC_POS_053: Lịch Sử Mua Hàng Khách
          </button>
          <button
            onClick={() => setActiveTab('replenish')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'replenish' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📋 UC_POS_056: Đề Nghị Nhập Hàng Cửa Hàng
          </button>
          <button
            onClick={() => setActiveTab('receive')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'receive' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🚚 UC_POS_057: Nhận Hàng Kho Trung Tâm
          </button>
          <button
            onClick={() => setActiveTab('audit')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'audit' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔍 UC_POS_058: Kiểm Kê Nhanh Tại Cửa Hàng
          </button>
        </div>
      </div>

      {activeTab === 'history' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📜 Lịch Sử Mua Hàng Của Khách Hàng (UC_POS_053)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Đơn Hàng</th>
                  <th className="p-3">Thời Gian Mua</th>
                  <th className="p-3">Số Lượng Món</th>
                  <th className="p-3">Tổng Tiền Hóa Đơn</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {historyList.map((h) => (
                  <tr key={h.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{h.code}</td>
                    <td className="p-3 text-slate-700">{h.date}</td>
                    <td className="p-3 font-medium text-foreground">{h.itemsCount} món</td>
                    <td className="p-3 font-extrabold text-brand-strong">{h.total.toLocaleString('vi-VN')} VNĐ</td>
                    <td className="p-3 text-right">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-emerald-100 text-emerald-800">
                        {h.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'replenish' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-bold text-foreground">📋 Lập Phiếu Đề Nghị Nhập Hàng Bổ Sung (UC_POS_056)</h2>
              <p className="text-xs text-muted-foreground mt-0.5">Tạo đề nghị gửi Kho Trung Tâm chuyển thêm hàng hóa về cửa hàng POS</p>
            </div>
            <button
              onClick={handleCreateReplenish}
              className="px-4 py-2 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm"
            >
              🚀 Gửi Đề Nghị Nhập Hàng
            </button>
          </div>

          <div className="space-y-3">
            {replenishItems.map((item) => (
              <div key={item.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                <div>
                  <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong mr-2">{item.code}</span>
                  <span className="font-bold text-foreground">{item.name}</span>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-xs text-muted-foreground font-semibold">Số lượng đề nghị:</span>
                  <input
                    type="number"
                    value={item.qty}
                    onChange={(e) => {
                      const val = Number(e.target.value);
                      setReplenishItems((prev) => prev.map((i) => (i.id === item.id ? { ...i, qty: val } : i)));
                    }}
                    className="w-20 border border-border rounded p-1 text-center font-bold bg-surface text-foreground"
                  />
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'receive' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🚚 Nhận Hàng Điều Chuyển Từ Kho Trung Tâm (UC_POS_057)</h2>
          <div className="space-y-3">
            {transfers.map((t) => (
              <div key={t.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                <div>
                  <h3 className="font-extrabold text-foreground text-base">{t.code}</h3>
                  <p className="text-xs text-muted-foreground mt-1">Xuất phát từ: <span className="font-semibold text-foreground">{t.from}</span> | Quy mô: <span className="font-bold text-brand-strong">{t.itemsCount} sản phẩm</span></p>
                </div>
                <button
                  onClick={() => handleConfirmReceive(t.code)}
                  className="px-4 py-2 bg-emerald-600 text-white rounded-lg text-sm font-bold hover:bg-emerald-700 shadow-sm"
                >
                  ✓ Xác Nhận Đã Nhận Hàng
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'audit' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-bold text-foreground">🔍 Kiểm Kê Nhanh Tồn Kho Cửa Hàng POS (UC_POS_058)</h2>
              <p className="text-xs text-muted-foreground mt-0.5">Nhập số lượng thực tế tại kệ để đối soát chênh lệch với số lượng trên hệ thống</p>
            </div>
            <button
              onClick={handleSubmitAudit}
              className="px-4 py-2 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm"
            >
              ✓ Chốt Phiếu Kiểm Kê
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã & Tên Sản Phẩm</th>
                  <th className="p-3">Tồn Sách Sổ (Hệ Thống)</th>
                  <th className="p-3">Tồn Thực Tế Đếm Được</th>
                  <th className="p-3 text-right">Chênh Lệch Đối Soát</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {auditLines.map((line) => {
                  const check = calculateQuickAuditDiscrepancy(line.systemStock, line.actualStock);
                  return (
                    <tr key={line.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-bold text-foreground">
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong mr-2">{line.code}</span>
                        {line.name}
                      </td>
                      <td className="p-3 font-semibold text-slate-700">{line.systemStock}</td>
                      <td className="p-3">
                        <input
                          type="number"
                          value={line.actualStock}
                          onChange={(e) => updateActualStock(line.id, Number(e.target.value))}
                          className="w-20 border border-border rounded p-1 text-center font-bold bg-surface text-foreground"
                        />
                      </td>
                      <td className="p-3 text-right">
                        <span className={`font-extrabold ${check.isMatch ? 'text-emerald-700' : 'text-rose-700'}`}>
                          {check.diff > 0 ? `+${check.diff}` : check.diff}
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
