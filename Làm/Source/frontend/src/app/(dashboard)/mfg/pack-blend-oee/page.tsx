'use client';

import React, { useState } from 'react';
import {
  formatOeePercentage,
  formatMixingRatio,
} from '@/shared/api/mfg-pack-blend-oee-helpers';

export default function MfgPackBlendOeePage() {
  const [activeTab, setActiveTab] = useState<'pack' | 'blend' | 'oee'>('pack');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_MFG_039: Đóng gói & gắn tem
  const [packList, setPackList] = useState([
    { id: 'p-1', sku: 'FG-SERVER-42U', type: 'Kiện Gỗ Pallet Xuất Khẩu', qty: 1, barcode: 'GS1-128 Serialized', tmpl: '/labels/rack-42u.prn' },
    { id: 'p-2', sku: 'FG-DESK-WOOD', type: 'Thùng Carton Chèn Xốp 5 Lớp', qty: 1, barcode: 'QR Code Truy Xuất', tmpl: '/labels/desk-wood.prn' },
  ]);

  // UC_MFG_040: Định mức phối trộn
  const [recipes, setRecipes] = useState([
    { id: 'r-1', code: 'RECIPE-PAINT-BLACK', name: 'Sơn Đen Mờ Tĩnh Điện', mat: 'MAT-EPOXY-RESIN', matName: 'Nhựa Epoxy Nền', ratio: 60, tol: 0.5, step: 'Bước 1: Nạp hạt nhựa' },
    { id: 'r-2', code: 'RECIPE-PAINT-BLACK', name: 'Sơn Đen Mờ Tĩnh Điện', mat: 'MAT-BLACK-PIGMENT', matName: 'Bột Màu Carbon Đen', ratio: 30, tol: 0.2, step: 'Bước 2: Phối màu phân tán' },
    { id: 'r-3', code: 'RECIPE-PAINT-BLACK', name: 'Sơn Đen Mờ Tĩnh Điện', mat: 'MAT-HARDENER-AG', matName: 'Chất Đóng Rắn Kháng Xước', ratio: 10, tol: 0.1, step: 'Bước 3: Gia nhiệt trộn đều' },
  ]);

  // UC_MFG_044: Hiệu suất / OEE
  const [oeeList, setOeeList] = useState([
    { id: 'o-1', wc: 'WC-CNC-01', name: 'Xưởng Gia Công Cơ Khí CNC', a: 92.5, p: 88.0, q: 98.0, oee: 79.8 },
    { id: 'o-2', wc: 'WC-ASSY-03', name: 'Chuyền Lắp Ráp & Đóng Gói', a: 95.0, p: 90.0, q: 99.0, oee: 84.6 },
  ]);

  const [oeeForm, setOeeForm] = useState({
    wc: 'WC-WELD-02',
    name: 'Xưởng Hàn Robot Tự Động',
    a: 90.0,
    p: 85.0,
    q: 96.0,
  });

  const handleCalculateOee = (e: React.FormEvent) => {
    e.preventDefault();
    const oee = (oeeForm.a / 100) * (oeeForm.p / 100) * (oeeForm.q / 100) * 100;
    const newOee = {
      id: 'o-' + Date.now(),
      wc: oeeForm.wc,
      name: oeeForm.name,
      a: oeeForm.a,
      p: oeeForm.p,
      q: oeeForm.q,
      oee: Number(oee.toFixed(1)),
    };
    setOeeList([...oeeList, newOee]);
    showToast(`✓ Đã tính toán chỉ số OEE cho [${oeeForm.name}]: ${formatOeePercentage(oee)}!`, 'success');
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
              MFG - PACKAGING & LABELS, BLENDING RECIPES & EQUIPMENT OEE
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Quy Cách Đóng Gói, Định Mức Phối Trộn & Hiệu Suất Thiết Bị Tổng Thể OEE</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý quy cách tem nhãn mã vạch đóng gói, công thức tỷ lệ phối trộn hóa chất và chỉ số OEE (Availability, Performance, Quality)
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (3/3 UCs MFG)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('pack')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'pack' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📦 UC_MFG_039: Đóng Gói & Tem Nhãn
          </button>
          <button
            onClick={() => setActiveTab('blend')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'blend' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🧪 UC_MFG_040: Định Mức Phối Trộn
          </button>
          <button
            onClick={() => setActiveTab('oee')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'oee' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚡ UC_MFG_044: Hiệu Suất OEE
          </button>
        </div>
      </div>

      {activeTab === 'pack' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📦 Quy Cách Đóng Gói & In Tem Mã Vạch (UC_MFG_039)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Thành Phẩm</th>
                  <th className="p-3">Loại Bao Bì Đóng Gói</th>
                  <th className="p-3 text-center">Quy Cách (SP/Thùng)</th>
                  <th className="p-3">Chuẩn Mã Vạch</th>
                  <th className="p-3">Đường Dẫn Mẫu In</th>
                  <th className="p-3 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {packList.map((p) => (
                  <tr key={p.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{p.sku}</td>
                    <td className="p-3 font-semibold text-foreground">{p.type}</td>
                    <td className="p-3 text-center font-extrabold text-slate-800">{p.qty} SP</td>
                    <td className="p-3 text-slate-700">{p.barcode}</td>
                    <td className="p-3 font-mono text-xs text-muted-foreground">{p.tmpl}</td>
                    <td className="p-3 text-right">
                      <button onClick={() => showToast(`✓ Đã gửi lệnh in tem mã vạch cho sản phẩm [${p.sku}]!`, 'success')} className="px-3 py-1 bg-brand text-brand-foreground text-xs font-bold rounded hover:opacity-90">
                        🖨️ In Tem Mẫu
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'blend' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🧪 Tỷ Lệ & Thứ Tự Phối Trộn Công Thức Hóa Chất / Sơn (UC_MFG_040)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Công Thức</th>
                  <th className="p-3">Thành Phần Phối Trộn</th>
                  <th className="p-3 text-center">Tỷ Lệ Định Mức (%)</th>
                  <th className="p-3">Quy Trình & Thứ Tự Nạp Liệu</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {recipes.map((r) => (
                  <tr key={r.id} className="hover:bg-surface-hover/50">
                    <td className="p-3">
                      <div className="font-mono font-bold text-brand">{r.code}</div>
                      <div className="text-xs text-muted-foreground">{r.name}</div>
                    </td>
                    <td className="p-3">
                      <div className="font-mono font-bold text-foreground">{r.mat}</div>
                      <div className="text-xs text-slate-700 font-semibold">{r.matName}</div>
                    </td>
                    <td className="p-3 text-center font-extrabold text-foreground">{formatMixingRatio(r.ratio, r.tol)}</td>
                    <td className="p-3 text-slate-700 font-medium">{r.step}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'oee' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">⚡ Tính Toán OEE (UC_MFG_044)</h2>
            <form onSubmit={handleCalculateOee} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Phân Xưởng / Máy:</label>
                <input type="text" value={oeeForm.wc} onChange={(e) => setOeeForm({ ...oeeForm, wc: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên Trung Tâm Sản Xuất:</label>
                <input type="text" value={oeeForm.name} onChange={(e) => setOeeForm({ ...oeeForm, name: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tỷ Lệ Sẵn Sàng (Availability %):</label>
                <input type="number" step="0.1" value={oeeForm.a} onChange={(e) => setOeeForm({ ...oeeForm, a: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Hiệu Suất Vận Hành (Performance %):</label>
                <input type="number" step="0.1" value={oeeForm.p} onChange={(e) => setOeeForm({ ...oeeForm, p: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tỷ Lệ Chất Lượng (Quality %):</label>
                <input type="number" step="0.1" value={oeeForm.q} onChange={(e) => setOeeForm({ ...oeeForm, q: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                ⚡ Tính Chỉ Số OEE Tổng Thể
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Bảng Thống Kê Hiệu Suất Thiết Bị OEE</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Trung Tâm Máy</th>
                    <th className="p-3 text-center">Availability (A)</th>
                    <th className="p-3 text-center">Performance (P)</th>
                    <th className="p-3 text-center">Quality (Q)</th>
                    <th className="p-3 text-right">Chỉ Số OEE</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {oeeList.map((o) => (
                    <tr key={o.id} className="hover:bg-surface-hover/50">
                      <td className="p-3">
                        <div className="font-mono font-bold text-brand">{o.wc}</div>
                        <div className="text-xs text-muted-foreground">{o.name}</div>
                      </td>
                      <td className="p-3 text-center font-bold text-slate-700">{o.a}%</td>
                      <td className="p-3 text-center font-bold text-slate-700">{o.p}%</td>
                      <td className="p-3 text-center font-bold text-slate-700">{o.q}%</td>
                      <td className="p-3 text-right font-black text-blue-700 text-base">{formatOeePercentage(o.oee)}</td>
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
