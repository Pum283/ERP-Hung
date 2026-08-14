'use client';

import React, { useState } from 'react';
import {
  formatOnTimeRate,
  formatWeightTons,
} from '@/shared/api/log-driver-productivity-cost-helpers';

export default function LogDriverProductivityCostPage() {
  const [activeTab, setActiveTab] = useState<'kpi' | 'cost'>('kpi');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_LOG_036: Năng suất tài xế / chuyến
  const [driverKpiList] = useState([
    { id: 'd-1', name: 'Trần Văn Tài', trips: 42, orders: 180, weight: 18500, onTime: 98.5 },
    { id: 'd-2', name: 'Nguyễn Hoàng Lái', trips: 38, orders: 155, weight: 12200, onTime: 96.8 },
  ]);

  // UC_LOG_037: Chi phí vận chuyển
  const [costForm, setCostForm] = useState({
    tripNo: 'TRIP-2026-0814',
    fuel: 450000,
    toll: 120000,
    allowance: 200000,
    orders: 5,
  });

  const [costList, setCostList] = useState([
    { id: 'c-1', allocNo: 'COST-ALLOC-001', tripNo: 'TRIP-2026-0814', fuel: 450000, toll: 120000, allowance: 200000, total: 770000, orders: 5, avg: 154000 },
  ]);

  const handleCalculateCost = (e: React.FormEvent) => {
    e.preventDefault();
    const total = costForm.fuel + costForm.toll + costForm.allowance;
    const avg = total / (costForm.orders > 0 ? costForm.orders : 1);
    const newEntry = {
      id: 'c-' + Date.now(),
      allocNo: 'COST-ALLOC-' + Math.floor(1000 + Math.random() * 9000),
      tripNo: costForm.tripNo,
      fuel: costForm.fuel,
      toll: costForm.toll,
      allowance: costForm.allowance,
      total,
      orders: costForm.orders,
      avg,
    };
    setCostList([newEntry, ...costList]);
    showToast(`✓ Đã tính và phân bổ chi phí chuyến [${costForm.tripNo}]: Bình quân ${avg.toLocaleString('vi-VN')} đ/đơn!`, 'success');
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
              LOG - DRIVER PRODUCTIVITY & TRIP COST ALLOCATION
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Năng Suất Tài Xế, Đánh Giá Chuyến Giao & Phân Bổ Chi Phí Vận Chuyển</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Báo cáo hiệu suất giao hàng đúng hạn của đội ngũ tài xế và mô hình tính toán chi phí nhiên liệu, phí cầu đường cho từng đơn
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (2/2 UCs LOG)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('kpi')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'kpi' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_LOG_036: Năng Suất Tài Xế
          </button>
          <button
            onClick={() => setActiveTab('cost')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'cost' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💰 UC_LOG_037: Phân Bổ Chi Phí Vận Chuyển
          </button>
        </div>
      </div>

      {activeTab === 'kpi' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📊 Bảng Xếp Hạng & Chỉ Số KPI Năng Suất Tài Xế (UC_LOG_036)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Họ Tên Tài Xế</th>
                  <th className="p-3 text-center">Số Chuyến Hoàn Tất</th>
                  <th className="p-3 text-center">Tổng Đơn Giao Đạt</th>
                  <th className="p-3 text-center">Khối Lượng Giao (Tấn)</th>
                  <th className="p-3 text-right">Tỷ Lệ Đúng Hạn (%)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {driverKpiList.map((d) => (
                  <tr key={d.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{d.name}</td>
                    <td className="p-3 text-center font-extrabold text-slate-800">{d.trips} chuyến</td>
                    <td className="p-3 text-center font-bold text-emerald-800">{d.orders} đơn</td>
                    <td className="p-3 text-center font-semibold text-slate-700">{formatWeightTons(d.weight)}</td>
                    <td className="p-3 text-right font-black text-brand">
                      {formatOnTimeRate(d.onTime)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'cost' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">💰 Tính Toán Chi Phí Chuyến (UC_LOG_037)</h2>
            <form onSubmit={handleCalculateCost} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Chuyến Xe (Trip No):</label>
                <input type="text" value={costForm.tripNo} onChange={(e) => setCostForm({ ...costForm, tripNo: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tiền Nhiên Liệu / Xăng Dầu (VNĐ):</label>
                <input type="number" value={costForm.fuel} onChange={(e) => setCostForm({ ...costForm, fuel: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Phí Cầu Đường / BOT (VNĐ):</label>
                <input type="number" value={costForm.toll} onChange={(e) => setCostForm({ ...costForm, toll: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Công Tác Phí Tài Xế (VNĐ):</label>
                <input type="number" value={costForm.allowance} onChange={(e) => setCostForm({ ...costForm, allowance: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Đơn Hàng Gộp Trong Chuyến:</label>
                <input type="number" value={costForm.orders} onChange={(e) => setCostForm({ ...costForm, orders: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                🧮 Phân Bổ Chi Phí Cho Từng Đơn
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Lịch Sử Phân Bổ Chi Phí Vận Chuyển</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Mã Phân Bổ</th>
                    <th className="p-3">Chuyến Xe</th>
                    <th className="p-3 text-right">Tổng Chi Phí (VNĐ)</th>
                    <th className="p-3 text-center">Số Đơn</th>
                    <th className="p-3 text-right">Chi Phí / Đơn</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {costList.map((c) => (
                    <tr key={c.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-brand">{c.allocNo}</td>
                      <td className="p-3 font-mono font-bold text-foreground">{c.tripNo}</td>
                      <td className="p-3 text-right font-extrabold text-foreground">{c.total.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-center font-bold text-slate-800">{c.orders} đơn</td>
                      <td className="p-3 text-right font-black text-emerald-800">{c.avg.toLocaleString('vi-VN')} đ</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
