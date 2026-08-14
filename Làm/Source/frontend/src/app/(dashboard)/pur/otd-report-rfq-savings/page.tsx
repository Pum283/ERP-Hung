'use client';

import React, { useState } from 'react';
import {
  calculateOtdRating,
  calculateRfqNegotiationSavings,
} from '@/shared/api/pur-otd-report-rfq-savings-helpers';

export default function PurOtdReportRfqSavingsPage() {
  const [activeTab, setActiveTab] = useState<'otd' | 'savings'>('otd');

  // UC_PUR_049: Báo cáo đúng hạn giao hàng (OTD)
  const [otdData] = useState([
    { id: 'v-1', supplier: 'Vinamilk Co.', totalOrders: 40, onTime: 38, late: 2, rate: 95.0 },
    { id: 'v-2', supplier: 'Mộc Châu Milk', totalOrders: 20, onTime: 16, late: 4, rate: 80.0 },
    { id: 'v-3', supplier: 'Trung Nguyên Corp', totalOrders: 30, onTime: 27, late: 3, rate: 90.0 },
  ]);

  // UC_PUR_050: Báo cáo tiết kiệm chi phí từ RFQ
  const [rfqSavingsList] = useState([
    { id: 'rfq-1', number: 'RFQ-2026-001', title: 'Gói Thầu Cung Cấp Sữa Tươi Quý 3', budget: 300000000, awarded: 240000000 },
    { id: 'rfq-2', number: 'RFQ-2026-002', title: 'Gói Thầu Bao Bì Hộp Giấy', budget: 150000000, awarded: 125000000 },
  ]);

  const totalBudget = rfqSavingsList.reduce((acc, i) => acc + i.budget, 0);
  const totalAwarded = rfqSavingsList.reduce((acc, i) => acc + i.awarded, 0);
  const totalSavingsSummary = calculateRfqNegotiationSavings(totalBudget, totalAwarded);

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      <div className="bg-surface border border-border p-6 rounded-2xl shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-brand-muted text-brand-strong text-xs px-3 py-1 rounded-full font-semibold border border-brand/30">
              PUR - OTD PERFORMANCE REPORT & RFQ COST SAVINGS REPORT
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Báo Cáo Đúng Hạn Giao Hàng (OTD) & Báo Cáo Tiết Kiệm Chi Phí RFQ</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Phân tích chỉ số giao hàng đúng hẹn (OTD %) của nhà cung cấp và tổng hợp ngân sách tiết kiệm qua chào giá đàm phán RFQ
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (2/2 UCs PUR)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('otd')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'otd' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🚚 UC_PUR_049: Báo Cáo Đúng Hạn Giao Hàng (OTD %)
          </button>
          <button
            onClick={() => setActiveTab('savings')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'savings' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💰 UC_PUR_050: Báo Cáo Tiết Kiệm Chi Phí RFQ
          </button>
        </div>
      </div>

      {activeTab === 'otd' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🚚 Báo Cáo Tỷ Lệ Giao Hàng Đúng Hạn (OTD %) Theo Nhà Cung Cấp (UC_PUR_049)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Tên Nhà Cung Cấp</th>
                  <th className="p-3 text-center">Tổng Đơn Hàng (PO)</th>
                  <th className="p-3 text-center">Giao Đúng Hạn</th>
                  <th className="p-3 text-center">Giao Trễ Hạn</th>
                  <th className="p-3 text-right">Tỷ Lệ OTD (%)</th>
                  <th className="p-3 text-right">Đánh Giá Hiệu Suất</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {otdData.map((v) => {
                  const rating = calculateOtdRating(v.rate);
                  return (
                    <tr key={v.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-bold text-foreground">{v.supplier}</td>
                      <td className="p-3 text-center font-semibold text-slate-700">{v.totalOrders} đơn</td>
                      <td className="p-3 text-center font-bold text-emerald-700">{v.onTime} đơn</td>
                      <td className="p-3 text-center font-bold text-rose-600">{v.late} đơn</td>
                      <td className="p-3 text-right font-extrabold text-foreground">{v.rate}%</td>
                      <td className="p-3 text-right">
                        <span
                          className={`px-2.5 py-1 text-xs font-bold rounded-full border ${
                            rating === 'Excellent'
                              ? 'bg-emerald-100 text-emerald-800 border-emerald-300'
                              : rating === 'Good'
                              ? 'bg-blue-100 text-blue-800 border-blue-300'
                              : 'bg-rose-100 text-rose-800 border-rose-300'
                          }`}
                        >
                          ● {rating.toUpperCase()}
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

      {activeTab === 'savings' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div className="p-5 rounded-xl border border-brand/30 bg-brand-muted/20 flex justify-between items-center">
            <div>
              <span className="text-xs font-bold text-brand-strong">TỔNG HỢP TIẾT KIỆM CHI PHÍ QUA ĐÀM PHÁN RFQ:</span>
              <h2 className="text-2xl font-black text-brand-strong mt-1">{totalSavingsSummary.savingsAmount.toLocaleString('vi-VN')} VNĐ</h2>
            </div>
            <div className="text-right">
              <span className="text-xs text-muted-foreground">Tỷ Lệ Tiết Kiệm Trung Bình:</span>
              <div className="text-xl font-extrabold text-emerald-700">+{totalSavingsSummary.savingsPercentage}%</div>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã & Tiêu Đề RFQ</th>
                  <th className="p-3">Ngân Sách Ban Đầu</th>
                  <th className="p-3">Giá Trị Trúng Thầu</th>
                  <th className="p-3 text-right">Số Tiền Tiết Kiệm</th>
                  <th className="p-3 text-right">Tỷ Lệ Tiết Kiệm (%)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {rfqSavingsList.map((item) => {
                  const s = calculateRfqNegotiationSavings(item.budget, item.awarded);
                  return (
                    <tr key={item.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-bold text-foreground">
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong mr-2">{item.number}</span>
                        {item.title}
                      </td>
                      <td className="p-3 text-slate-600">{item.budget.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 font-bold text-foreground">{item.awarded.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-right font-extrabold text-emerald-700">+{s.savingsAmount.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-right">
                        <span className="px-2 py-0.5 text-xs font-black rounded bg-emerald-100 text-emerald-800">
                          +{s.savingsPercentage}%
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
