'use client';

import React, { useState } from 'react';
import {
  validatePrinterConfigForm,
  validateCashDrawerConfigForm,
} from '@/shared/api/pos-hardware-printer-drawer-helpers';

export default function PosHardwarePrinterDrawerPage() {
  const [activeTab, setActiveTab] = useState<'printers' | 'drawers'>('printers');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_POS_004: Cấu hình máy in bếp/khu vực
  const [printers, setPrinters] = useState([
    { id: 'p-1', name: 'Máy in Bếp Nóng', area: 'Kitchen', conn: 'LAN_IP', address: '192.168.1.201', width: 80, cut: true, active: true },
    { id: 'p-2', name: 'Máy in Quầy Bar / Đồ uống', area: 'Bar', conn: 'LAN_IP', address: '192.168.1.202', width: 80, cut: true, active: true },
  ]);

  const [printerForm, setPrinterForm] = useState({ name: '', area: 'Kitchen', conn: 'LAN_IP', address: '192.168.1.205', width: 80, cut: true });

  const handleSavePrinter = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validatePrinterConfigForm(printerForm.name, printerForm.address);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `p-${Date.now()}`,
      name: printerForm.name,
      area: printerForm.area,
      conn: printerForm.conn,
      address: printerForm.address,
      width: printerForm.width,
      cut: printerForm.cut,
      active: true,
    };

    setPrinters([...printers, created]);
    setPrinterForm({ name: '', area: 'Kitchen', conn: 'LAN_IP', address: '192.168.1.205', width: 80, cut: true });
    showToast(`Đã thêm cấu hình máy in bếp/khu vực [${created.name}] thành công!`, 'success');
  };

  // UC_POS_005: Cấu hình ngăn kéo tiền
  const [drawers, setDrawers] = useState([
    { id: 'd-1', name: 'Ngăn Kéo Quầy Thu Ngân 01', mode: 'PrinterKickout', hex: '1B700019FA', autoOpen: true, active: true },
  ]);

  const [drawerForm, setDrawerForm] = useState({ name: '', mode: 'PrinterKickout', hex: '1B700019FA', autoOpen: true });

  const handleSaveDrawer = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateCashDrawerConfigForm(drawerForm.name, drawerForm.hex);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `d-${Date.now()}`,
      name: drawerForm.name,
      mode: drawerForm.mode,
      hex: drawerForm.hex,
      autoOpen: drawerForm.autoOpen,
      active: true,
    };

    setDrawers([...drawers, created]);
    setDrawerForm({ name: '', mode: 'PrinterKickout', hex: '1B700019FA', autoOpen: true });
    showToast(`Đã lưu cấu hình ngăn kéo tiền [${created.name}] thành công!`, 'success');
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
              POS - HARDWARE PRINTER & CASH DRAWER CONFIG
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Cấu Hình Thiết Bị Phần Cứng POS</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Thiết lập kết nối máy in chế biến (bếp nóng/quầy bar) và cấu hình xung kích bật ngăn kéo tiền mặt (Cash Drawer)
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (2/2 UCs)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('printers')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'printers' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🖨️ UC_POS_004: Máy In Bếp & Khu Vực
          </button>
          <button
            onClick={() => setActiveTab('drawers')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'drawers' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💵 UC_POS_005: Ngăn Kéo Tiền Mặt
          </button>
        </div>
      </div>

      {activeTab === 'printers' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">🖨️ Danh Sách Máy In Chế Biến / Khu Vực (UC_POS_004)</h2>
            <div className="space-y-3">
              {printers.map((p) => (
                <div key={p.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{p.area}</span>
                      <h3 className="font-bold text-foreground">{p.name}</h3>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">Kết nối: <span className="font-semibold text-foreground">{p.conn} ({p.address})</span> | Khổ giấy: {p.width}mm</p>
                  </div>
                  <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                    ● Đang kết nối
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">➕ Thêm Cấu Hình Máy In</h2>
            <form onSubmit={handleSavePrinter} className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Tên máy in:</label>
                <input
                  type="text"
                  value={printerForm.name}
                  onChange={(e) => setPrinterForm({ ...printerForm, name: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: Máy in Bếp Nóng 02"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Khu vực chế biến:</label>
                <select
                  value={printerForm.area}
                  onChange={(e) => setPrinterForm({ ...printerForm, area: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                >
                  <option value="Kitchen">Nhà Bếp (Kitchen)</option>
                  <option value="Bar">Quầy Bar (Bar)</option>
                  <option value="Bakery">Bánh Bếp (Bakery)</option>
                  <option value="Cashier">Thu Ngân (Cashier)</option>
                </select>
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Địa chỉ IP / Cổng:</label>
                <input
                  type="text"
                  value={printerForm.address}
                  onChange={(e) => setPrinterForm({ ...printerForm, address: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: 192.168.1.205"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
                Lưu Cấu Hình Máy In
              </button>
            </form>
          </div>
        </div>
      )}

      {activeTab === 'drawers' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">💵 Cấu Hình Ngăn Kéo Tiền Mặt (UC_POS_005)</h2>
            <div className="space-y-3">
              {drawers.map((d) => (
                <div key={d.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-foreground">{d.name}</h3>
                    <p className="text-xs text-muted-foreground mt-1">Chế độ bật: <span className="font-semibold text-foreground">{d.mode}</span> | Command Hex: <code className="bg-surface border border-border px-1 py-0.5 rounded text-xs">{d.hex}</code></p>
                  </div>
                  <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-brand-muted text-brand-strong border border-brand/30">
                    Tự bật khi thanh toán tiền mặt
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">➕ Cấu Hình Ngăn Kéo Tiền Mới</h2>
            <form onSubmit={handleSaveDrawer} className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Tên thiết bị ngăn kéo:</label>
                <input
                  type="text"
                  value={drawerForm.name}
                  onChange={(e) => setDrawerForm({ ...drawerForm, name: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: Ngăn Kéo Quầy 02"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Lệnh kích mở Hex (Pulse Command):</label>
                <input
                  type="text"
                  value={drawerForm.hex}
                  onChange={(e) => setDrawerForm({ ...drawerForm, hex: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono"
                  placeholder="VD: 1B700019FA"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
                Lưu Ngăn Kéo Tiền
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
