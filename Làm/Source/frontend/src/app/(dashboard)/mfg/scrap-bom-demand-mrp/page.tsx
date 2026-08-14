'use client';

import React, { useState } from 'react';
import {
  formatScrapPercentage,
  formatGrossRequirement,
} from '@/shared/api/mfg-scrap-bom-demand-mrp-helpers';

export default function MfgScrapBomDemandMrpPage() {
  const [activeTab, setActiveTab] = useState<'scrap' | 'copyBom' | 'mps' | 'mrp'>('scrap');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_MFG_009: Định mức hao hụt
  const [scrapList, setScrapList] = useState([
    { id: 'sc-1', bom: 'BOM-SERVER-42U', mat: 'MAT-STEEL-SHEET', name: 'Tấm Thép Tĩnh Điện 2mm', net: 100, pct: 5.0, gross: 105, reason: 'Mạt phôi kim loại hao hụt khi cắt CNC' },
    { id: 'sc-2', bom: 'BOM-SERVER-42U', mat: 'MAT-CABLE-10M', name: 'Dây Điện Chống Nhiễu 10m', net: 200, pct: 3.0, gross: 206, reason: 'Cắt dư đầu mối nối' },
  ]);

  const [scrapForm, setScrapForm] = useState({
    bom: 'BOM-SERVER-42U',
    mat: 'MAT-SCREW-M6',
    name: 'Ốc Vít Lục Giác M6',
    net: 500,
    pct: 4.0,
    reason: 'Rơi rớt hao hụt trong quá trình bắn vít lắp ráp',
  });

  const handleAddScrap = (e: React.FormEvent) => {
    e.preventDefault();
    const gross = formatGrossRequirement(scrapForm.net, scrapForm.pct);
    const newEntry = {
      id: 'sc-' + Date.now(),
      bom: scrapForm.bom,
      mat: scrapForm.mat,
      name: scrapForm.name,
      net: scrapForm.net,
      pct: scrapForm.pct,
      gross,
      reason: scrapForm.reason,
    };
    setScrapList([...scrapList, newEntry]);
    showToast(`✓ Đã thiết lập tỷ lệ hao hụt [${scrapForm.pct}%] cho vật tư [${scrapForm.name}]!`, 'success');
  };

  // UC_MFG_011: Sao chép BOM
  const [copyForm, setCopyForm] = useState({
    srcBom: 'BOM-SERVER-42U',
    srcVer: 'v1.0',
    newVer: 'v1.1-EXPORT',
  });

  const handleCopyBom = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã sao chép thành công phiên bản mới [${copyForm.srcBom}-${copyForm.newVer}] từ [${copyForm.srcVer}]!`, 'success');
  };

  // UC_MFG_012: Kế hoạch SX theo nhu cầu (MPS)
  const [mpsForm, setMpsForm] = useState({
    planName: 'Kế Hoạch Sản Xuất Tháng 09/2026',
    sku: 'FG-SERVER-RACK-42U',
    name: 'Tủ Server Rack 42U Chuẩn Quốc Tế',
    forecast: 120,
    backlog: 45,
  });

  const handleCreateMps = (e: React.FormEvent) => {
    e.preventDefault();
    const total = mpsForm.forecast + mpsForm.backlog;
    showToast(`✓ Đã khởi tạo Kế hoạch MPS-20260814 với tổng sản lượng kế hoạch: ${total} chiếc!`, 'success');
  };

  // UC_MFG_014: Tính nhu cầu nguyên vật liệu (MRP)
  const [mrpList] = useState([
    { id: 'm-1', mat: 'MAT-STEEL-SHEET', name: 'Tấm Thép Tĩnh Điện 2mm', gross: 600, stock: 150, po: 50, net: 400, buy: 400, date: '2026-08-25' },
    { id: 'm-2', mat: 'MAT-SCREW-M6', name: 'Ốc Vít Lục Giác M6', gross: 5000, stock: 4200, po: 1000, net: 0, buy: 0, date: '2026-08-25' },
    { id: 'm-3', mat: 'MAT-FAN-12V', name: 'Quạt Tản Nhiệt Rack 12V', gross: 450, stock: 80, po: 0, net: 370, buy: 370, date: '2026-08-28' },
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
              MFG - BOM SCRAP ALLOWANCE, BOM DUPLICATION, MPS & MRP
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Hao Hụt BOM, Sao Chép Công Thức, Kế Hoạch MPS & Tính Toán Nhu Cầu MRP</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Cấu hình tỷ lệ hao hụt nguyên vật liệu, sao chép phiên bản BOM, lập kế hoạch sản xuất theo đơn hàng/dự báo và cân đối MRP
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs MFG)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('scrap')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'scrap' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📉 UC_MFG_009: Định Mức Hao Hụt
          </button>
          <button
            onClick={() => setActiveTab('copyBom')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'copyBom' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📑 UC_MFG_011: Sao Chép BOM
          </button>
          <button
            onClick={() => setActiveTab('mps')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'mps' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_MFG_012: Kế Hoạch Nhu Cầu MPS
          </button>
          <button
            onClick={() => setActiveTab('mrp')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'mrp' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🧮 UC_MFG_014: Cân Đối Nhu Cầu MRP
          </button>
        </div>
      </div>

      {activeTab === 'scrap' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📉 Thiết Lập Hao Hụt Cho BOM (UC_MFG_009)</h2>
            <form onSubmit={handleAddScrap} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã BOM Áp Dụng:</label>
                <input type="text" value={scrapForm.bom} onChange={(e) => setScrapForm({ ...scrapForm, bom: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Nguyên Vật Liệu:</label>
                <input type="text" value={scrapForm.mat} onChange={(e) => setScrapForm({ ...scrapForm, mat: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên Nguyên Vật Liệu:</label>
                <input type="text" value={scrapForm.name} onChange={(e) => setScrapForm({ ...scrapForm, name: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="block text-foreground font-medium mb-1">Định Mức Tinh (Net):</label>
                  <input type="number" value={scrapForm.net} onChange={(e) => setScrapForm({ ...scrapForm, net: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Tỷ Lệ Hao Hụt (%):</label>
                  <input type="number" step="0.5" value={scrapForm.pct} onChange={(e) => setScrapForm({ ...scrapForm, pct: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
                </div>
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Lý Do / Đặc Điểm Hao Hụt:</label>
                <textarea value={scrapForm.reason} onChange={(e) => setScrapForm({ ...scrapForm, reason: e.target.value })} rows={2} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                💾 Lưu Định Mức Có Hao Hụt
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Danh Sách Định Mức Tinh & Dự Phòng Hao Hụt</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Nguyên Vật Liệu</th>
                    <th className="p-3 text-center">Định Mức Tinh</th>
                    <th className="p-3 text-center">Tỷ Lệ Hao Hụt</th>
                    <th className="p-3 text-center">Tổng Cấp Phát (Gross)</th>
                    <th className="p-3">Ghi Chú Kỹ Thuật</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {scrapList.map((s) => (
                    <tr key={s.id} className="hover:bg-surface-hover/50">
                      <td className="p-3">
                        <div className="font-mono font-bold text-foreground">{s.mat}</div>
                        <div className="text-xs text-muted-foreground">{s.name}</div>
                      </td>
                      <td className="p-3 text-center font-bold text-slate-700">{s.net}</td>
                      <td className="p-3 text-center">
                        <span className="px-2 py-0.5 text-xs font-black rounded bg-amber-100 text-amber-800 border border-amber-300">
                          {formatScrapPercentage(s.pct)}
                        </span>
                      </td>
                      <td className="p-3 text-center font-extrabold text-brand">{s.gross}</td>
                      <td className="p-3 text-xs text-muted-foreground">{s.reason}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'copyBom' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">📑 Sao Chép BOM & Nhân Bản Phiên Bản Sản Xuất (UC_MFG_011)</h2>
          <form onSubmit={handleCopyBom} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã BOM Nguồn:</label>
                <input type="text" value={copyForm.srcBom} onChange={(e) => setCopyForm({ ...copyForm, srcBom: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Phiên Bản Nguồn:</label>
                <input type="text" value={copyForm.srcVer} onChange={(e) => setCopyForm({ ...copyForm, srcVer: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Mã Phiên Bản Mới Được Nhân Bản:</label>
              <input type="text" value={copyForm.newVer} onChange={(e) => setCopyForm({ ...copyForm, newVer: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold font-mono" />
            </div>

            <div className="p-4 rounded-xl border border-border bg-surface-hover text-xs text-muted-foreground">
              💡 Hệ thống sẽ tự động sao chép toàn bộ danh sách nguyên vật liệu, định mức tiêu hao, công đoạn routing và liên kết máy sang phiên bản BOM mới.
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              📑 Thực Hiện Sao Chép Toàn Bộ Cấu Trúc BOM
            </button>
          </form>
        </div>
      )}

      {activeTab === 'mps' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">📊 Kế Hoạch Sản Xuất Theo Nhu Cầu Thị Trường MPS (UC_MFG_012)</h2>
          <form onSubmit={handleCreateMps} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Tên Kế Hoạch Sản Xuất:</label>
              <input type="text" value={mpsForm.planName} onChange={(e) => setMpsForm({ ...mpsForm, planName: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Thành Phẩm (SKU):</label>
                <input type="text" value={mpsForm.sku} onChange={(e) => setMpsForm({ ...mpsForm, sku: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên Sản Phẩm:</label>
                <input type="text" value={mpsForm.name} onChange={(e) => setMpsForm({ ...mpsForm, name: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Dự Báo Bán Hàng (Forecast Qty):</label>
                <input type="number" value={mpsForm.forecast} onChange={(e) => setMpsForm({ ...mpsForm, forecast: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Đơn Bán Hàng Tồn Đọng (Backlog Qty):</label>
                <input type="number" value={mpsForm.backlog} onChange={(e) => setMpsForm({ ...mpsForm, backlog: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              📊 Phê Duyệt Kế Hoạch Sản Xuất MPS
            </button>
          </form>
        </div>
      )}

      {activeTab === 'mrp' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-bold text-foreground">🧮 Kết Quả Cân Đối Nhu Cầu Nguyên Vật Liệu MRP (UC_MFG_014)</h2>
              <p className="text-xs text-muted-foreground">Công thức: Nhu cầu thiếu hụt = Tổng nhu cầu - Tồn kho khả dụng - Đơn mua PO đang về</p>
            </div>
            <button onClick={() => showToast('✓ Đã chạy lại thuật toán cân đối MRP!', 'success')} className="px-3.5 py-1.5 bg-brand text-brand-foreground text-xs font-bold rounded-lg hover:opacity-90 shadow-sm">
              🔄 Chạy Lại Thuật Toán MRP
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Nguyên Vật Liệu (Material)</th>
                  <th className="p-3 text-center">Tổng Nhu Cầu (Gross)</th>
                  <th className="p-3 text-center">Tồn Kho Hiện Tại</th>
                  <th className="p-3 text-center">PO Đang Về</th>
                  <th className="p-3 text-center">Thiếu Hụt (Net Requirement)</th>
                  <th className="p-3 text-right">Đề Xuất Mua Thêm (PO Qty)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {mrpList.map((m) => (
                  <tr key={m.id} className="hover:bg-surface-hover/50">
                    <td className="p-3">
                      <div className="font-mono font-bold text-foreground">{m.mat}</div>
                      <div className="text-xs text-muted-foreground">{m.name}</div>
                    </td>
                    <td className="p-3 text-center font-bold text-slate-700">{m.gross}</td>
                    <td className="p-3 text-center font-semibold text-slate-600">{m.stock}</td>
                    <td className="p-3 text-center font-semibold text-blue-700">{m.po}</td>
                    <td className="p-3 text-center">
                      <span className={`px-2.5 py-1 text-xs font-black rounded-full border ${m.net > 0 ? 'bg-rose-100 text-rose-800 border-rose-300' : 'bg-emerald-100 text-emerald-800 border-emerald-300'}`}>
                        {m.net > 0 ? `Thiếu ${m.net}` : 'Đủ Hàng'}
                      </span>
                    </td>
                    <td className="p-3 text-right font-black text-brand">{m.buy > 0 ? `${m.buy} cái` : '-'}</td>
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
