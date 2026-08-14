'use client';

import React, { useState } from 'react';
import {
  formatHourlyRate,
  formatTravelFee,
} from '@/shared/api/fsm-service-pricing-helpers';

export default function FsmServicePricingPage() {
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FSM_004: Bảng giá dịch vụ
  const [priceList, setPriceList] = useState([
    { id: 'p-1', code: 'FSM-MAINT-STD', name: 'Bảo Trì & Vệ Sinh Máy Định Kỳ', cat: 'Bảo Trì', hourly: 250000, travel: 150000, surcharge: 30 },
    { id: 'p-2', code: 'FSM-REPAIR-ELEC', name: 'Sửa Chữa Hệ Thống Điện & Bo Mạch', cat: 'Sửa Chữa', hourly: 350000, travel: 200000, surcharge: 50 },
    { id: 'p-3', code: 'FSM-INSTALL-RACK', name: 'Lắp Đặt & Cấu Hình Tủ Server', cat: 'Lắp Đặt Mới', hourly: 300000, travel: 150000, surcharge: 20 },
  ]);

  const [form, setForm] = useState({
    code: '',
    name: '',
    cat: 'Sửa Chữa',
    hourly: 300000,
    travel: 150000,
    surcharge: 30,
  });

  const handleAddPrice = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.code || !form.name) {
      showToast('Vui lòng điền mã và tên dịch vụ!', 'error');
      return;
    }
    const newEntry = {
      id: 'p-' + Date.now(),
      ...form,
    };
    setPriceList([...priceList, newEntry]);
    setForm({ code: '', name: '', cat: 'Sửa Chữa', hourly: 300000, travel: 150000, surcharge: 30 });
    showToast(`✓ Đã thêm bảng giá dịch vụ [${newEntry.name}] vào hệ thống FSM!`, 'success');
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
              FSM - SERVICE PRICE LIST & LABOR HOURLY RATES
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Bảng Giá Dịch Vụ Kỹ Thuật Hiện Trường & Đơn Giá Giờ Công (UC_FSM_004)</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Cấu hình biểu giá giờ công kỹ thuật viên, phụ phí di chuyển theo vùng và tỷ lệ phụ thu dịch vụ khẩn cấp ngoài giờ
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (1/1 UC FSM)
            </span>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-6">
        <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">➕ Thêm Biểu Giá Dịch Vụ Mới</h2>
          <form onSubmit={handleAddPrice} className="space-y-3 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Mã Dịch Vụ (Service Code):</label>
              <input type="text" value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Tên Gói Dịch Vụ Kỹ Thuật:</label>
              <input type="text" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Hạng Mục Dịch Vụ:</label>
              <select value={form.cat} onChange={(e) => setForm({ ...form, cat: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold">
                <option value="Bảo Trì">Bảo Trì Định Kỳ</option>
                <option value="Sửa Chữa">Sửa Chữa Khắc Phục Sự Cố</option>
                <option value="Lắp Đặt Mới">Lắp Đặt & Triển Khai Mới</option>
                <option value="Khẩn Cấp">Cứu Hộ Khẩn Cấp 24/7</option>
              </select>
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Đơn Giá Giờ Công (VNĐ / Giờ):</label>
              <input type="number" value={form.hourly} onChange={(e) => setForm({ ...form, hourly: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Phí Đi Lại / Di Chuyển (VNĐ / Lượt):</label>
              <input type="number" value={form.travel} onChange={(e) => setForm({ ...form, travel: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Phụ Thu Khẩn Cấp / Ngoài Giờ (%):</label>
              <input type="number" value={form.surcharge} onChange={(e) => setForm({ ...form, surcharge: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
              💾 Lưu Bảng Giá Dịch Vụ
            </button>
          </form>
        </div>

        <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📋 Biểu Giá Dịch Vụ Kỹ Thuật Hiện Trường</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã & Gói Dịch Vụ</th>
                  <th className="p-3">Hạng Mục</th>
                  <th className="p-3 text-right">Giá Giờ Công</th>
                  <th className="p-3 text-right">Phí Di Chuyển</th>
                  <th className="p-3 text-center">Phụ Thu Khẩn Cấp</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {priceList.map((p) => (
                  <tr key={p.id} className="hover:bg-surface-hover/50">
                    <td className="p-3">
                      <div className="font-mono font-bold text-brand">{p.code}</div>
                      <div className="text-xs text-foreground font-semibold">{p.name}</div>
                    </td>
                    <td className="p-3 font-semibold text-slate-700">{p.cat}</td>
                    <td className="p-3 text-right font-extrabold text-foreground">{formatHourlyRate(p.hourly)}</td>
                    <td className="p-3 text-right font-semibold text-slate-700">{formatTravelFee(p.travel)}</td>
                    <td className="p-3 text-center font-bold text-rose-700">+{p.surcharge}%</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Đang Áp Dụng
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
