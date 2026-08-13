'use client';

import React, { useState } from 'react';
import {
  calculateGpsDistanceKm,
  formatVisitFrequencyLabel,
  validateGpsCoordinates,
} from '@/shared/api/crm-route-sales-visit-gps-helpers';

export default function CrmRouteSalesVisitGpsPage() {
  const [activeTab, setActiveTab] = useState<'territory' | 'frequency' | 'plan' | 'gps'>('territory');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: SALES TERRITORY MANAGEMENT (UC_CRM_089)
  // ────────────────────────────────────────────────────────────────────────────
  const [territories, setTerritories] = useState([
    { id: 't-1', code: 'T-HCM-Q1', name: 'Tuyến Quận 1 - TP.HCM', region: 'Miền Nam', frequency: 'Weekly', sales: 'Nguyễn Văn Sales', active: true },
    { id: 't-2', code: 'T-HN-CG', name: 'Tuyến Cầu Giấy - Hà Nội', region: 'Miền Bắc', frequency: 'BiWeekly', sales: 'Trần Thị CRM', active: true },
    { id: 't-3', code: 'T-DN-HC', name: 'Tuyến Hải Châu - Đà Nẵng', region: 'Miền Trung', frequency: 'Monthly', sales: 'Lê Văn Field', active: true },
  ]);

  const [territoryForm, setTerritoryForm] = useState({ code: '', name: '', region: 'Miền Nam', frequency: 'Weekly', sales: 'Nguyễn Văn Sales' });

  const handleCreateTerritory = (e: React.FormEvent) => {
    e.preventDefault();
    if (!territoryForm.code || !territoryForm.name) {
      showToast('Vui lòng nhập Mã tuyến và Tên tuyến bán hàng.', 'error');
      return;
    }

    const created = {
      id: `t-${Date.now()}`,
      code: territoryForm.code,
      name: territoryForm.name,
      region: territoryForm.region,
      frequency: territoryForm.frequency,
      sales: territoryForm.sales,
      active: true,
    };

    setTerritories([...territories, created]);
    setTerritoryForm({ code: '', name: '', region: 'Miền Nam', frequency: 'Weekly', sales: 'Nguyễn Văn Sales' });
    showToast(`Đã khởi tạo Tuyến bán hàng [${created.name}] thành công!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: VISIT FREQUENCY CLASSIFICATION (UC_CRM_090)
  // ────────────────────────────────────────────────────────────────────────────
  const handleUpdateFrequency = (tId: string, newFreq: string) => {
    setTerritories((prev) =>
      prev.map((t) => (t.id === tId ? { ...t, frequency: newFreq } : t))
    );
    showToast(`Đã cập nhật tần suất viếng thăm mới thành công!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: VISIT PLANNING & SCHEDULING (UC_CRM_091)
  // ────────────────────────────────────────────────────────────────────────────
  const [visitPlans, setVisitPlans] = useState([
    { id: 'vp-1', territory: 'Tuyến Quận 1 - TP.HCM', customer: 'Đại lý Thực phẩm An Phát', date: '13/08/2026', sales: 'Nguyễn Văn Sales', status: 'Planned', inGps: null, outGps: null, notes: 'Gặp chủ đại lý chốt đơn Q3' },
    { id: 'vp-2', territory: 'Tuyến Cầu Giấy - Hà Nội', customer: 'Chuỗi Cửa hàng Bách Hóa Việt', date: '14/08/2026', sales: 'Trần Thị CRM', status: 'Completed', inGps: '10.7769,106.7009', outGps: '10.7772,106.7012', notes: 'Đã ký biên bản viếng thăm' },
  ]);

  const [planForm, setPlanForm] = useState({ territory: territories[0].name, customer: '', date: '', notes: '' });

  const handleCreatePlan = (e: React.FormEvent) => {
    e.preventDefault();
    if (!planForm.customer || !planForm.date) {
      showToast('Vui lòng nhập tên Khách hàng và Ngày thăm kế hoạch.', 'error');
      return;
    }

    const created = {
      id: `vp-${Date.now()}`,
      territory: planForm.territory,
      customer: planForm.customer,
      date: planForm.date,
      sales: 'Field Sales Rep (Bạn)',
      status: 'Planned',
      inGps: null,
      outGps: null,
      notes: planForm.notes || 'Kế hoạch thăm định kỳ',
    };

    setVisitPlans([created, ...visitPlans]);
    setPlanForm({ territory: territories[0].name, customer: '', date: '', notes: '' });
    showToast(`Đã lập lịch thăm điểm bán [${created.customer}]!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: GPS CHECK-IN / CHECK-OUT (UC_CRM_092)
  // ────────────────────────────────────────────────────────────────────────────
  const handleCheckInGps = (vpId: string) => {
    const coords = '10.7769,106.7009'; // HCM City center mock
    setVisitPlans((prev) =>
      prev.map((p) => (p.id === vpId ? { ...p, status: 'InProgress', inGps: coords } : p))
    );
    showToast(`📍 Check-in GPS thành công tại tọa độ [${coords}]!`, 'success');
  };

  const handleCheckOutGps = (vpId: string) => {
    const coords = '10.7772,106.7012';
    setVisitPlans((prev) =>
      prev.map((p) => (p.id === vpId ? { ...p, status: 'Completed', outGps: coords } : p))
    );
    showToast(`🏁 Check-out GPS thành công tại tọa độ [${coords}]!`, 'success');
  };

  return (
    <div className="p-6 space-y-6">
      {/* Toast */}
      {toast && (
        <div
          className={`fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white font-medium text-sm transition-all ${
            toast.type === 'success' ? 'bg-emerald-600' : toast.type === 'error' ? 'bg-rose-600' : 'bg-amber-600'
          }`}
        >
          {toast.message}
        </div>
      )}

      {/* Header */}
      <div className="bg-brand p-5 rounded-xl text-brand-foreground shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-cyan-500/30 text-cyan-200 text-xs px-3 py-1 rounded-full font-semibold border border-cyan-400/30">
              CRM - FIELD SALES ROUTE & GPS VISIT
            </span>
            <h1 className="text-2xl font-bold mt-2">CRM Phân Vùng Bán Hàng & Định Vị GPS Viếng Thăm</h1>
            <p className="text-cyan-100 text-sm mt-1">
              Phân vùng tuyến bán hàng, Phân loại tần suất thăm, Lập kế hoạch visit & Check-in / Check-out GPS điểm bán
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-emerald-500/20 text-emerald-300 border border-emerald-500/30">
              ● Tiến độ 90% (4/4 UCs)
            </span>
          </div>
        </div>

        {/* Tab Selection */}
        <div className="flex space-x-2 mt-6 border-t border-white/10 pt-4">
          <button
            onClick={() => setActiveTab('territory')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'territory' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🗺️ UC_CRM_089: Tuyến Bán Hàng
          </button>
          <button
            onClick={() => setActiveTab('frequency')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'frequency' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📊 UC_CRM_090: Tần Suất Visit
          </button>
          <button
            onClick={() => setActiveTab('plan')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'plan' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📅 UC_CRM_091: Lập Kế Hoạch Visit
          </button>
          <button
            onClick={() => setActiveTab('gps')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'gps' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📍 UC_CRM_092: Check-in GPS
          </button>
        </div>
      </div>

      {/* TAB 1: SALES TERRITORY */}
      {activeTab === 'territory' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">🗺️ Danh Sách Phân Vùng & Tuyến Bán Hàng Field Sales (UC_CRM_089)</h2>
            <div className="space-y-3">
              {territories.map((t) => (
                <div key={t.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-cyan-100 text-cyan-900">{t.code}</span>
                      <h3 className="font-bold text-slate-900">{t.name}</h3>
                    </div>
                    <p className="text-xs text-slate-500 mt-1">
                      Khu vực: <span className="font-semibold text-slate-700">{t.region}</span> • Phụ trách: <span className="font-semibold text-slate-700">{t.sales}</span>
                    </p>
                  </div>
                  <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">
                    {formatVisitFrequencyLabel(t.frequency)}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Khởi Tạo Tuyến Bán Hàng Mới</h2>
            <form onSubmit={handleCreateTerritory} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Mã tuyến:</label>
                <input
                  type="text"
                  value={territoryForm.code}
                  onChange={(e) => setTerritoryForm({ ...territoryForm, code: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: T-HCM-Q3"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên tuyến bán hàng:</label>
                <input
                  type="text"
                  value={territoryForm.name}
                  onChange={(e) => setTerritoryForm({ ...territoryForm, name: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Tuyến Bàn Cờ - Quận 3"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Khu vực:</label>
                <select
                  value={territoryForm.region}
                  onChange={(e) => setTerritoryForm({ ...territoryForm, region: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2 bg-white"
                >
                  <option value="Miền Nam">Miền Nam</option>
                  <option value="Miền Bắc">Miền Bắc</option>
                  <option value="Miền Trung">Miền Trung</option>
                </select>
              </div>
              <button type="submit" className="w-full py-2.5 bg-teal-600 text-white rounded-lg font-semibold hover:bg-teal-700">
                Lưu Tuyến Bán Hàng
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 2: VISIT FREQUENCY */}
      {activeTab === 'frequency' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">📊 Phân Loại Tần Suất Viếng Thăm Theo Điểm Bán (UC_CRM_090)</h2>
          <div className="space-y-3">
            {territories.map((t) => (
              <div key={t.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                <div>
                  <h3 className="font-bold text-slate-900">{t.name} ({t.code})</h3>
                  <p className="text-xs text-slate-500 mt-1">Khu vực: {t.region} • Phụ trách: {t.sales}</p>
                </div>
                <div className="flex gap-2 items-center">
                  <select
                    value={t.frequency}
                    onChange={(e) => handleUpdateFrequency(t.id, e.target.value)}
                    className="border border-slate-300 text-xs font-semibold rounded-lg p-2 bg-white"
                  >
                    <option value="Weekly">Weekly (Hàng tuần)</option>
                    <option value="BiWeekly">BiWeekly (2 tuần/lần)</option>
                    <option value="Monthly">Monthly (Hàng tháng)</option>
                  </select>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* TAB 3: VISIT PLANNING */}
      {activeTab === 'plan' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">📅 Lập Kế Hoạch & Lịch Thăm Điểm Bán (UC_CRM_091)</h2>
            <div className="space-y-3">
              {visitPlans.map((vp) => (
                <div key={vp.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{vp.customer}</h3>
                    <p className="text-xs text-slate-500 mt-1">
                      Tuyến: {vp.territory} • Ngày thăm: <span className="font-semibold text-slate-800">{vp.date}</span>
                    </p>
                    <p className="text-xs text-slate-600 italic mt-1">"{vp.notes}"</p>
                  </div>
                  <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${vp.status === 'Completed' ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'}`}>
                    {vp.status === 'Completed' ? 'Đã hoàn thành' : 'Kế hoạch đã lập'}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Lập Lịch Viếng Thăm Mới</h2>
            <form onSubmit={handleCreatePlan} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên khách hàng / Điểm bán:</label>
                <input
                  type="text"
                  value={planForm.customer}
                  onChange={(e) => setPlanForm({ ...planForm, customer: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Chuỗi Cửa hàng Tiện Lợi An Khang"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Ngày thăm kế hoạch:</label>
                <input
                  type="date"
                  value={planForm.date}
                  onChange={(e) => setPlanForm({ ...planForm, date: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Mục đích viếng thăm:</label>
                <textarea
                  value={planForm.notes}
                  onChange={(e) => setPlanForm({ ...planForm, notes: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  rows={3}
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-teal-600 text-white rounded-lg font-semibold hover:bg-teal-700">
                Lưu Kế Hoạch Visit
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 4: GPS CHECK-IN / CHECK-OUT */}
      {activeTab === 'gps' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">📍 Định Vị GPS Check-in / Check-out Tại Điểm Thăm (UC_CRM_092)</h2>
          <div className="space-y-3">
            {visitPlans.map((vp) => (
              <div key={vp.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                <div className="space-y-1">
                  <h3 className="font-bold text-slate-900">{vp.customer}</h3>
                  <p className="text-xs text-slate-500">Tuyến: {vp.territory} • Sales: {vp.sales}</p>
                  {vp.inGps && <p className="text-xs text-emerald-700 font-semibold">Check-in GPS: {vp.inGps}</p>}
                  {vp.outGps && <p className="text-xs text-blue-700 font-semibold">Check-out GPS: {vp.outGps}</p>}
                </div>

                <div className="flex gap-2">
                  {!vp.inGps && (
                    <button
                      onClick={() => handleCheckInGps(vp.id)}
                      className="px-3.5 py-2 bg-emerald-600 text-white text-xs font-bold rounded-lg hover:bg-emerald-700 shadow-sm"
                    >
                      📍 Check-in GPS
                    </button>
                  )}
                  {vp.inGps && !vp.outGps && (
                    <button
                      onClick={() => handleCheckOutGps(vp.id)}
                      className="px-3.5 py-2 bg-blue-600 text-white text-xs font-bold rounded-lg hover:bg-blue-700 shadow-sm"
                    >
                      🏁 Check-out GPS
                    </button>
                  )}
                  {vp.outGps && (
                    <span className="px-3 py-1.5 bg-slate-200 text-slate-800 text-xs font-bold rounded-lg">
                      ✓ Đã hoàn tất visit
                    </span>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
