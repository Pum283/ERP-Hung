'use client';

import React, { useState } from 'react';
import {
  formatPercentageBreakdown,
  getPurposeColorIndicator,
} from '@/shared/api/inv-dispatch-purpose-report-helpers';

export default function InvDispatchPurposeReportPage() {
  const [categories] = useState([
    { id: 'c-1', name: 'Xuất Bán Hàng (SO Delivery)', count: 240, value: 850000000, pct: 56.7 },
    { id: 'c-2', name: 'Xuất Cho Dự Án (Project Issue)', count: 45, value: 320000000, pct: 21.3 },
    { id: 'c-3', name: 'Xuất Sản Xuất Lắp Ráp (MFG BOM)', count: 90, value: 240000000, pct: 16.0 },
    { id: 'c-4', name: 'Xuất Kỹ Thuật Bảo Trì (Technical Service)', count: 35, value: 60000000, pct: 4.0 },
    { id: 'c-5', name: 'Xuất Tiêu Hao Nội Bộ (Internal Use)', count: 20, value: 30000000, pct: 2.0 },
  ]);

  const totalValue = categories.reduce((acc, c) => acc + c.value, 0);
  const totalCount = categories.reduce((acc, c) => acc + c.count, 0);

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      <div className="bg-surface border border-border p-6 rounded-2xl shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-brand-muted text-brand-strong text-xs px-3 py-1 rounded-full font-semibold border border-brand/30">
              INV - STOCK DISPATCH BREAKDOWN BY PURPOSE REPORT
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Báo Cáo Xuất Kho Phân Bổ Theo Mục Đích (UC_INV_068)</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Phân tích cơ cấu và tỷ trọng giá trị hàng hóa xuất kho phục vụ bán hàng, dự án, sản xuất, bảo trì kỹ thuật và nội bộ
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (Phân hệ INV)
            </span>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-6">
        <div className="bg-surface rounded-xl border border-border p-6 shadow-sm flex items-center justify-between">
          <div>
            <div className="text-xs font-bold text-muted-foreground">TỔNG GIÁ TRỊ XUẤT KHO TRONG KỲ:</div>
            <div className="text-2xl font-black text-brand mt-1">{totalValue.toLocaleString('vi-VN')} VNĐ</div>
          </div>
          <div className="text-3xl">📊</div>
        </div>
        <div className="bg-surface rounded-xl border border-border p-6 shadow-sm flex items-center justify-between">
          <div>
            <div className="text-xs font-bold text-muted-foreground">TỔNG SỐ LƯỢT PHIẾU XUẤT:</div>
            <div className="text-2xl font-black text-slate-800 mt-1">{totalCount} Lượt Xuất</div>
          </div>
          <div className="text-3xl">📦</div>
        </div>
      </div>

      <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
        <h2 className="text-lg font-bold text-foreground">📋 Bảng Thống Kê Chi Tiết Theo Mục Đích Xuất Kho</h2>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm border-collapse">
            <thead>
              <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                <th className="p-3">Mục Đích Xuất Kho</th>
                <th className="p-3 text-center">Số Phiếu Xuất</th>
                <th className="p-3 text-right">Tổng Giá Trị (VNĐ)</th>
                <th className="p-3 text-right">Tỷ Trọng (%)</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {categories.map((c) => {
                const colorClass = getPurposeColorIndicator(c.name);
                return (
                  <tr key={c.id} className="hover:bg-surface-hover/50">
                    <td className="p-3">
                      <span className={`px-3 py-1 text-xs font-bold rounded-lg border ${colorClass}`}>
                        {c.name}
                      </span>
                    </td>
                    <td className="p-3 text-center font-bold text-slate-700">{c.count} phiếu</td>
                    <td className="p-3 text-right font-extrabold text-foreground">{c.value.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-extrabold text-brand">
                      {formatPercentageBreakdown(c.pct)}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
