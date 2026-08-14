'use client';

import React, { useState } from 'react';
import {
  formatExchangeRate,
  formatCashFlowTypeBadge,
} from '@/shared/api/fin-currency-cashflow-category-helpers';

export default function FinCurrencyCashFlowCategoryPage() {
  const [activeTab, setActiveTab] = useState<'currencies' | 'categories'>('currencies');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FIN_005: Đồng tiền hạch toán & tỷ giá
  const [currencies] = useState([
    { id: 'c-1', code: 'VND', name: 'Việt Nam Đồng', rate: 1, source: 'Ngân Hàng Nhà Nước', isBase: true, date: '2026-08-14' },
    { id: 'c-2', code: 'USD', name: 'Đô La Mỹ', rate: 25450, source: 'Vietcombank', isBase: false, date: '2026-08-14' },
    { id: 'c-3', code: 'EUR', name: 'Đồng Euro', rate: 27800, source: 'Vietcombank', isBase: false, date: '2026-08-14' },
    { id: 'c-4', code: 'JPY', name: 'Yên Nhật', rate: 168.5, source: 'Vietcombank', isBase: false, date: '2026-08-14' },
  ]);

  // UC_FIN_007: Khoản mục thu/chi
  const [categories] = useState([
    { id: 'cat-1', code: 'CASH-IN-PRJ', name: 'Thu tiền theo tiến độ hợp đồng dự án', type: 'Inflow', section: 'Operating', active: true },
    { id: 'cat-2', code: 'CASH-OUT-MAT', name: 'Chi tiền mua nguyên vật liệu & thiết bị', type: 'Outflow', section: 'Operating', active: true },
    { id: 'cat-3', code: 'CASH-OUT-LABOR', name: 'Chi trả tiền lương nhân công thi công', type: 'Outflow', section: 'Operating', active: true },
    { id: 'cat-4', code: 'CASH-OUT-TAX', name: 'Chi nộp thuế GTGT & thuế TNDN', type: 'Outflow', section: 'Operating', active: true },
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
              FIN - CURRENCIES, FX RATES & CASH FLOW CATEGORIES
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Đồng Tiền Hạch Toán & Tỷ Giá Ngoại Tệ, Khoản Mục Thu/Chi Dòng Tiền</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý đồng tiền cơ sở (VND) và bảng tỷ giá quy đổi ngoại tệ, chuẩn hóa danh mục khoản mục lưu chuyển tiền tệ (Cash Flow Statement)
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (2/2 UCs FIN)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('currencies')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'currencies' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💱 UC_FIN_005: Tiền Tệ & Tỷ Giá
          </button>
          <button
            onClick={() => setActiveTab('categories')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'categories' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💰 UC_FIN_007: Khoản Mục Thu/Chi
          </button>
        </div>
      </div>

      {activeTab === 'currencies' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">💱 Danh Mục Đồng Tiền Hạch Toán & Tỷ Giá Ngoại Tệ (UC_FIN_005)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Tiền Tệ</th>
                  <th className="p-3">Tên Đồng Tiền</th>
                  <th className="p-3 text-right">Tỷ Giá Quy Đổi VND</th>
                  <th className="p-3">Nguồn Cung Cấp Tỷ Giá</th>
                  <th className="p-3 text-center">Tiền Tệ Cơ Sở?</th>
                  <th className="p-3 text-right">Ngày Áp Dụng</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {currencies.map((c) => (
                  <tr key={c.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-black text-brand text-base">{c.code}</td>
                    <td className="p-3 font-semibold text-foreground">{c.name}</td>
                    <td className="p-3 text-right font-mono font-bold text-slate-800">{formatExchangeRate(c.rate, c.code)}</td>
                    <td className="p-3 text-slate-700">{c.source}</td>
                    <td className="p-3 text-center">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${c.isBase ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-slate-100 text-slate-800 border-slate-300'}`}>
                        {c.isBase ? '★ Đồng Tiền Sổ Cái' : 'Ngoại Tệ'}
                      </span>
                    </td>
                    <td className="p-3 text-right text-slate-700">{c.date}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'categories' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">💰 Danh Mục Khoản Mục Thu / Chi Dòng Tiền (UC_FIN_007)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Khoản Mục</th>
                  <th className="p-3">Tên Khoản Mục Thu / Chi</th>
                  <th className="p-3 text-center">Chiều Dòng Tiền</th>
                  <th className="p-3 text-center">Phân Nhóm Lưu Chuyển</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {categories.map((cat) => (
                  <tr key={cat.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{cat.code}</td>
                    <td className="p-3 font-semibold text-foreground">{cat.name}</td>
                    <td className="p-3 text-center">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${formatCashFlowTypeBadge(cat.type)}`}>
                        {cat.type === 'Inflow' ? '▲ Dòng Tiền Thu Vào' : '▼ Dòng Tiền Chi Ra'}
                      </span>
                    </td>
                    <td className="p-3 text-center font-mono text-xs font-bold text-slate-700">{cat.section}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Sử Dụng
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
