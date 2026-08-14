'use client';

import React, { useState } from 'react';
import {
  formatVehiclePayload,
  formatEstimatedTransitTime,
} from '@/shared/api/log-fleet-pricing-zone-helpers';

export default function LogFleetPricingZonePage() {
  const [activeTab, setActiveTab] = useState<'fleet' | 'pricing' | 'zones'>('fleet');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_LOG_002: Danh mục tài xế / xe
  const [fleetList, setFleetList] = useState([
    { id: 'f-1', driver: 'Trần Văn Tài', phone: '0908123456', license: 'B2-791122', plate: '51D-889.99', type: 'Truck-2.5T', payload: 2500, active: true },
    { id: 'f-2', driver: 'Nguyễn Hoàng Lái', phone: '0912334455', license: 'C-882211', plate: '50LD-123.45', type: 'Van', payload: 1250, active: true },
  ]);

  const [fleetForm, setFleetForm] = useState({
    driver: '',
    phone: '',
    license: '',
    plate: '',
    type: 'Truck-2.5T',
    payload: 2500,
  });

  const handleAddFleet = (e: React.FormEvent) => {
    e.preventDefault();
    if (!fleetForm.driver || !fleetForm.plate) {
      showToast('Vui lòng nhập tên tài xế và biển số xe!', 'error');
      return;
    }
    const newEntry = {
      id: 'f-' + Date.now(),
      ...fleetForm,
      active: true,
    };
    setFleetList([...fleetList, newEntry]);
    setFleetForm({ driver: '', phone: '', license: '', plate: '', type: 'Truck-2.5T', payload: 2500 });
    showToast(`✓ Đã thêm tài xế [${newEntry.driver}] và xe [${newEntry.plate}] vào đội xe!`, 'success');
  };

  // UC_LOG_003: Bảng giá cước vận chuyển
  const [pricingRates] = useState([
    { id: 'r-1', code: 'RATE-VAN-CITY', type: 'Van', base: 200000, kmFirst: 18000, kmAfter: 14000, loadFee: 50000 },
    { id: 'r-2', code: 'RATE-TRUCK-2T5', type: 'Truck-2.5T', base: 350000, kmFirst: 25000, kmAfter: 18000, loadFee: 100000 },
    { id: 'r-3', code: 'RATE-CONTAINER', type: 'Container', base: 1200000, kmFirst: 45000, kmAfter: 35000, loadFee: 300000 },
  ]);

  // UC_LOG_004: Cấu hình khu vực giao
  const [zones] = useState([
    { id: 'z-1', code: 'ZONE-HCM-NOITHANH', name: 'Nội Thành TP.HCM', city: 'TP. Hồ Chí Minh', districts: ['Quận 1', 'Quận 3', 'Bình Thạnh', 'Phú Nhuận'], hours: 3 },
    { id: 'z-2', code: 'ZONE-HCM-NGOAITHANH', name: 'Ngoại Thành TP.HCM', city: 'TP. Hồ Chí Minh', districts: ['Hóc Môn', 'Củ Chi', 'Bình Chánh', 'Cần Giờ'], hours: 6 },
    { id: 'z-3', code: 'ZONE-BD-DONGNAI', name: 'Bình Dương & Đồng Nai', city: 'Liên Tỉnh Đông Nam Bộ', districts: ['Thủ Dầu Một', 'Dĩ An', 'Biên Hòa', 'Long Thành'], hours: 8 },
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
              LOG - FLEET DIRECTORY, FREIGHT PRICING & DELIVERY ZONE CONFIGURATION
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Quản Lý Đội Xe / Tài Xế, Bảng Giá Cước Vận Chuyển & Khu Vực Giao Hàng</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Cấu hình thông tin tài xế và phương tiện, biểu giá cước theo cự ly và phân vùng khu vực tuyến giao hàng
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (3/3 UCs LOG)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('fleet')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'fleet' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🚚 UC_LOG_002: Danh Mục Tài Xế / Xe
          </button>
          <button
            onClick={() => setActiveTab('pricing')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'pricing' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💰 UC_LOG_003: Bảng Giá Cước Vận Chuyển
          </button>
          <button
            onClick={() => setActiveTab('zones')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'zones' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🗺️ UC_LOG_004: Cấu Hình Khu Vực Giao
          </button>
        </div>
      </div>

      {activeTab === 'fleet' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">➕ Thêm Phương Tiện & Tài Xế Mới</h2>
            <form onSubmit={handleAddFleet} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Họ Tên Tài Xế:</label>
                <input type="text" value={fleetForm.driver} onChange={(e) => setFleetForm({ ...fleetForm, driver: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Điện Thoại:</label>
                <input type="text" value={fleetForm.phone} onChange={(e) => setFleetForm({ ...fleetForm, phone: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Giấy Phép Lái Xe:</label>
                <input type="text" value={fleetForm.license} onChange={(e) => setFleetForm({ ...fleetForm, license: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Biển Số Xe:</label>
                <input type="text" value={fleetForm.plate} onChange={(e) => setFleetForm({ ...fleetForm, plate: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="block text-foreground font-medium mb-1">Loại Xe:</label>
                  <select value={fleetForm.type} onChange={(e) => setFleetForm({ ...fleetForm, type: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold">
                    <option value="Van">Xe Van</option>
                    <option value="Truck-1.25T">Tải 1.25T</option>
                    <option value="Truck-2.5T">Tải 2.5T</option>
                    <option value="Truck-5T">Tải 5T</option>
                    <option value="Container">Container</option>
                  </select>
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Tải Trọng (Kg):</label>
                  <input type="number" value={fleetForm.payload} onChange={(e) => setFleetForm({ ...fleetForm, payload: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
                </div>
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                💾 Lưu Vào Danh Mục Đội Xe
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">🚚 Danh Sách Đội Xe & Tài Xế Hoạt Động (UC_LOG_002)</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Tài Xế</th>
                    <th className="p-3">Biển Số Xe</th>
                    <th className="p-3">Loại Xe & Tải Trọng</th>
                    <th className="p-3 text-right">Trạng Thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {fleetList.map((f) => (
                    <tr key={f.id} className="hover:bg-surface-hover/50">
                      <td className="p-3">
                        <div className="font-bold text-foreground">{f.driver}</div>
                        <div className="text-xs text-muted-foreground">{f.phone} · GPLX: {f.license}</div>
                      </td>
                      <td className="p-3 font-mono font-bold text-foreground">{f.plate}</td>
                      <td className="p-3">
                        <span className="font-semibold text-slate-700">{f.type}</span>
                        <span className="ml-2 text-xs font-bold text-indigo-700">({formatVehiclePayload(f.payload)})</span>
                      </td>
                      <td className="p-3 text-right">
                        <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                          ● Sẵn Sàng Vận Hành
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'pricing' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">💰 Bảng Giá Cước Vận Chuyển Theo Loại Xe & Cự Ly (UC_LOG_003)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Biểu Cước</th>
                  <th className="p-3">Loại Xe Áp Dụng</th>
                  <th className="p-3 text-right">Giá Mở Cửa (VNĐ)</th>
                  <th className="p-3 text-right">Đơn Giá 10 Km Đầu</th>
                  <th className="p-3 text-right">Đơn Giá Từ Km 11+</th>
                  <th className="p-3 text-right">Phí Bốc Xếp</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {pricingRates.map((r) => (
                  <tr key={r.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-foreground">{r.code}</td>
                    <td className="p-3 font-bold text-slate-800">{r.type}</td>
                    <td className="p-3 text-right font-extrabold text-foreground">{r.base.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-bold text-emerald-700">{r.kmFirst.toLocaleString('vi-VN')} đ/km</td>
                    <td className="p-3 text-right font-bold text-blue-700">{r.kmAfter.toLocaleString('vi-VN')} đ/km</td>
                    <td className="p-3 text-right text-muted-foreground">{r.loadFee.toLocaleString('vi-VN')} đ</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'zones' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🗺️ Cấu Hình Khu Vực Giao Hàng & Thời Gian Ước Tính (UC_LOG_004)</h2>
          <div className="grid grid-cols-3 gap-4">
            {zones.map((z) => (
              <div key={z.id} className="p-5 rounded-xl border border-border bg-surface shadow-sm space-y-3">
                <div className="flex justify-between items-start">
                  <div>
                    <span className="text-xs font-mono font-bold text-brand">{z.code}</span>
                    <h3 className="text-base font-bold text-foreground mt-0.5">{z.name}</h3>
                  </div>
                  <span className="px-2 py-1 bg-amber-100 text-amber-800 text-xs font-black rounded-lg border border-amber-300">
                    ⏱️ {formatEstimatedTransitTime(z.hours)}
                  </span>
                </div>
                <div className="text-xs text-muted-foreground font-semibold">Tỉnh / Thành: {z.city}</div>
                <div>
                  <div className="text-xs text-muted-foreground font-semibold mb-1">Các quận / huyện áp dụng:</div>
                  <div className="flex flex-wrap gap-1.5">
                    {z.districts.map((d, i) => (
                      <span key={i} className="px-2 py-0.5 bg-surface-hover text-foreground text-xs rounded border border-border font-medium">
                        {d}
                      </span>
                    ))}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
