'use client';

import React, { useState } from 'react';
import {
  detectAbnormalPriceSpike,
  consolidateDemandsByProduct,
} from '@/shared/api/pur-price-history-alert-pr-consolidate-rfq-helpers';

export default function PurPriceHistoryAlertPrConsolidateRfqPage() {
  const [activeTab, setActiveTab] = useState<'history' | 'alert' | 'consolidate' | 'rfq'>('history');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PUR_012: Lịch sử giá mua & UC_PUR_013: Cảnh báo tăng giá
  const [priceHistory] = useState([
    { id: 'ph-1', code: 'SKU-MILK', name: 'Sữa Tươi NGUYÊN CHẤT 1L', supplier: 'Vinamilk Co.', prevPrice: 24000, currentPrice: 28000, date: '11/08/2026' },
    { id: 'ph-2', code: 'SKU-BEANS', name: 'Cà Phê Hạt Arabica 1KG', supplier: 'Trung Nguyên Corp', prevPrice: 215000, currentPrice: 220000, date: '03/08/2026' },
  ]);

  // UC_PUR_016: Gộp nhiều nhu cầu thành PR
  const [rawDemands] = useState([
    { productId: 'p1', productCode: 'SKU-PAPER', productName: 'Giấy In A4 70gsm', dept: 'Phòng Marketing', qty: 10 },
    { productId: 'p1', productCode: 'SKU-PAPER', productName: 'Giấy In A4 70gsm', dept: 'Phòng Kế Toán', qty: 15 },
    { productId: 'p2', productCode: 'SKU-PEN', productName: 'Bút Bi Xanh Tiêu Chuẩn', dept: 'Phòng Nhân Sự', qty: 50 },
  ]);

  const consolidatedList = consolidateDemandsByProduct(rawDemands);

  const handleConsolidatePR = () => {
    const totalQty = consolidatedList.reduce((acc, i) => acc + i.totalQty, 0);
    showToast(`✓ Đã gộp thành công ${rawDemands.length} yêu cầu từ các phòng ban thành 1 Phiếu PR Tổng Hợp (${totalQty} sản phẩm)!`, 'success');
  };

  // UC_PUR_021: Tạo RFQ gửi nhiều nhà cung cấp
  const [rfqTitle, setRfqTitle] = useState('Yêu Cầu Báo Giá Nông Sản & Cà Phê Hạt Quý 3/2026');
  const [selectedVendors, setSelectedVendors] = useState(['Vinamilk Co.', 'Trung Nguyên Corp', 'Nông Sản Sạch An Giang']);

  const handleCreateMultiRfq = () => {
    showToast(`✓ Đã khởi tạo và gửi phiếu RFQ [${rfqTitle}] thành công tới ${selectedVendors.length} nhà cung cấp cùng lúc!`, 'success');
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
              PUR - PRICE HISTORY, PRICE ALERT, PR CONSOLIDATION & MULTI-SUPPLIER RFQ
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Lịch Sử Giá Mua, Cảnh Báo Tăng Giá, Gộp Nhu Cầu PR & RFQ Đa NCC</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Theo dõi biến động giá mua, cảnh báo biến động giá bất thường (Spike), gộp nhu cầu các phòng ban thành PR và phát hành RFQ cho nhiều NCC
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
            📈 UC_PUR_012: Lịch Sử Biến Động Giá Mua
          </button>
          <button
            onClick={() => setActiveTab('alert')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'alert' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🚨 UC_PUR_013: Cảnh Báo Tăng Giá Bất Thường
          </button>
          <button
            onClick={() => setActiveTab('consolidate')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'consolidate' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📦 UC_PUR_016: Gộp Nhiều Nhu Cầu Thành PR
          </button>
          <button
            onClick={() => setActiveTab('rfq')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'rfq' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📨 UC_PUR_021: Tạo RFQ Gửi Nhiều NCC
          </button>
        </div>
      </div>

      {activeTab === 'history' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📈 Tra Cứu Lịch Sử Biến Động Giá Mua (UC_PUR_012)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã & Tên Sản Phẩm</th>
                  <th className="p-3">Nhà Cung Cấp</th>
                  <th className="p-3">Giá Mua Cũ</th>
                  <th className="p-3">Giá Mua Hiện Tại</th>
                  <th className="p-3 text-right">Biến Động (%)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {priceHistory.map((item) => {
                  const calc = detectAbnormalPriceSpike(item.prevPrice, item.currentPrice);
                  return (
                    <tr key={item.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-bold text-foreground">
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong mr-2">{item.code}</span>
                        {item.name}
                      </td>
                      <td className="p-3 text-slate-700">{item.supplier}</td>
                      <td className="p-3 text-slate-500 line-through">{item.prevPrice.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 font-extrabold text-foreground">{item.currentPrice.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-right">
                        <span className={`px-2 py-0.5 text-xs font-extrabold rounded ${calc.changePercent > 0 ? 'bg-amber-100 text-amber-800' : 'bg-emerald-100 text-emerald-800'}`}>
                          {calc.changePercent > 0 ? `+${calc.changePercent}%` : `${calc.changePercent}%`}
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

      {activeTab === 'alert' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🚨 Cảnh Báo Tăng Giá Bất Thường (&gt;= 10%) (UC_PUR_013)</h2>
          <div className="space-y-3">
            {priceHistory.map((item) => {
              const calc = detectAbnormalPriceSpike(item.prevPrice, item.currentPrice);
              if (!calc.isSpike) return null;
              return (
                <div key={item.id} className="p-4 rounded-xl border border-rose-300 bg-rose-50/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-extrabold rounded bg-rose-600 text-white">SPIKE DETECTED</span>
                      <h3 className="font-bold text-foreground">{item.name} ({item.code})</h3>
                    </div>
                    <p className="text-xs text-rose-700 mt-1 font-medium">
                      Nhà cung cấp <b className="text-foreground">{item.supplier}</b> đã tăng giá mua từ <span className="line-through">{item.prevPrice.toLocaleString('vi-VN')} đ</span> lên <b className="text-rose-800 font-extrabold">{item.currentPrice.toLocaleString('vi-VN')} đ</b> (+{calc.changePercent}%)
                    </p>
                  </div>
                  <button className="px-3 py-1.5 bg-rose-600 text-white text-xs font-bold rounded-lg hover:bg-rose-700 shadow-sm">
                    ⚠️ Yêu Cầu Thương Lượng Giá
                  </button>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {activeTab === 'consolidate' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-bold text-foreground">📦 Gộp Nhu Cầu Mua Hàng Từ Nhiều Phòng Ban Thành 1 PR (UC_PUR_016)</h2>
              <p className="text-xs text-muted-foreground mt-0.5">Tự động tổng hợp danh sách các phiếu yêu cầu nhỏ lẻ để đàm phán mua số lượng lớn</p>
            </div>
            <button
              onClick={handleConsolidatePR}
              className="px-4 py-2 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm"
            >
              🚀 Chốt Gộp Thành PR Tổng Hợp
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="p-4 rounded-xl border border-border bg-surface-hover/30 space-y-3">
              <h3 className="text-sm font-bold text-foreground">📋 Danh Sách Yêu Cầu Gốc (Các Phòng Ban)</h3>
              <div className="space-y-2">
                {rawDemands.map((d, idx) => (
                  <div key={idx} className="p-2.5 rounded-lg border border-border bg-surface flex justify-between items-center text-xs">
                    <div>
                      <span className="font-bold text-brand-strong">{d.dept}:</span> {d.productName}
                    </div>
                    <span className="font-extrabold text-foreground">{d.qty} đơn vị</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="p-4 rounded-xl border border-brand/30 bg-brand-muted/20 space-y-3">
              <h3 className="text-sm font-bold text-brand-strong">⚡ Kết Quả Sau Khi Gộp Nhu Cầu (PR Consolidated)</h3>
              <div className="space-y-2">
                {consolidatedList.map((c, idx) => (
                  <div key={idx} className="p-2.5 rounded-lg border border-brand/30 bg-surface flex justify-between items-center text-xs">
                    <div>
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong mr-2">{c.productCode}</span>
                      <span className="font-bold text-foreground">{c.productName}</span>
                    </div>
                    <span className="font-extrabold text-brand-strong text-sm">{c.totalQty} đơn vị</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'rfq' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-5">
          <div>
            <h2 className="text-lg font-bold text-foreground">📨 Tạo Phiếu Yêu Cầu Báo Giá (RFQ) Gửi Nhiều Nhà Cung Cấp (UC_PUR_021)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Phát hành yêu cầu chào giá cùng lúc tới danh sách NCC được lựa chọn để so sánh cạnh tranh</p>
          </div>

          <div className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Tiêu đề RFQ:</label>
              <input
                type="text"
                value={rfqTitle}
                onChange={(e) => setRfqTitle(e.target.value)}
                className="w-full border border-border rounded-lg p-2.5 bg-surface text-foreground font-bold"
              />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Danh sách nhà cung cấp nhận RFQ ({selectedVendors.length} NCC):</label>
              <div className="flex flex-wrap gap-2 pt-1">
                {selectedVendors.map((v, idx) => (
                  <span key={idx} className="px-3 py-1 bg-brand-muted text-brand-strong rounded-full text-xs font-bold border border-brand/30">
                    🏢 {v}
                  </span>
                ))}
              </div>
            </div>

            <button
              onClick={handleCreateMultiRfq}
              className="w-full py-3 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm"
            >
              📨 Khởi Tạo & Phát Hành RFQ Cho Nhiều NCC
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
