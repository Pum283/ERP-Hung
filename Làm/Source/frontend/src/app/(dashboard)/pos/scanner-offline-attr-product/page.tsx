'use client';

import React, { useState } from 'react';
import {
  evaluateOfflineSyncBadgeStatus,
  validateScannerConfigForm,
} from '@/shared/api/pos-scanner-offline-attr-product-helpers';

export default function PosScannerOfflineAttrProductPage() {
  const [activeTab, setActiveTab] = useState<'scanner' | 'offline' | 'attributes'>('scanner');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_POS_006: Cấu hình thiết bị quét mã
  const [scanners, setScanners] = useState([
    { id: 's-1', name: 'Đầu Quét Mã Honeywell Xenon 1950g', conn: 'USB_HID', suffix: 'ENTER', timeout: 300, active: true },
    { id: 's-2', name: 'Đầu Quét QR Code Zebra DS2208', conn: 'USB_COM', suffix: 'TAB', timeout: 250, active: true },
  ]);

  const [scannerForm, setScannerForm] = useState({ name: '', conn: 'USB_HID', suffix: 'ENTER', timeout: 300 });

  const handleSaveScanner = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateScannerConfigForm(scannerForm.name);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `s-${Date.now()}`,
      name: scannerForm.name,
      conn: scannerForm.conn,
      suffix: scannerForm.suffix,
      timeout: scannerForm.timeout,
      active: true,
    };

    setScanners([...scanners, created]);
    setScannerForm({ name: '', conn: 'USB_HID', suffix: 'ENTER', timeout: 300 });
    showToast(`Đã thêm cấu hình đầu quét mã [${created.name}] thành công!`, 'success');
  };

  // UC_POS_008: Chế độ offline tạm & Đệm đồng bộ
  const [offlineBuffer, setOfflineBuffer] = useState({
    terminalCode: 'POS-POS01',
    ordersCount: 14,
    revenueTotal: 1280000,
    status: 'Pending',
  });

  const handleSyncOfflineOrders = () => {
    setOfflineBuffer({ ...offlineBuffer, ordersCount: 0, revenueTotal: 0, status: 'Synced' });
    showToast('🚀 Đã đồng bộ thành công 14 đơn hàng offline lên hệ thống trung tâm!', 'success');
  };

  // UC_POS_011 & UC_POS_013: Thuộc tính sản phẩm & Thứ tự hiển thị
  const [attributes] = useState([
    { id: 'a-1', attrName: 'Kích Thước (Size)', optionVal: 'Size L (Lớn)', extraPrice: 10000, img: '/images/size-l.png', order: 1, isDefault: false },
    { id: 'a-2', attrName: 'Topping Đi Kèm', optionVal: 'Thạch Trái Cây', extraPrice: 5000, img: '/images/topping.png', order: 2, isDefault: false },
    { id: 'a-3', attrName: 'Mức Đường', optionVal: '50% Đường', extraPrice: 0, img: '', order: 3, isDefault: true },
  ]);

  const offlineBadge = evaluateOfflineSyncBadgeStatus(offlineBuffer.status);

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
              POS - SCANNER, OFFLINE BUFFER & PRODUCT MODIFIERS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Cấu Hình Đầu Quét Mã, Chế Độ Offline & Thuộc Tính Sản Phẩm</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Thiết lập thiết bị quét mã vạch/QR, quản lý bộ đệm đơn hàng POS offline và tùy chỉnh thuộc tính/ảnh hiển thị món ăn
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('scanner')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'scanner' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📷 UC_POS_006: Đầu Quét Mã Vạch & QR
          </button>
          <button
            onClick={() => setActiveTab('offline')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'offline' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📡 UC_POS_008: Chế Độ POS Offline & Đệm Đồng Bộ
          </button>
          <button
            onClick={() => setActiveTab('attributes')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'attributes' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏷️ UC_POS_011 & 013: Thuộc Tính & Thứ Tự Ảnh Hiển Thị
          </button>
        </div>
      </div>

      {activeTab === 'scanner' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📷 Cấu Hình Thiết Bị Quét Mã Vạch / QR Code (UC_POS_006)</h2>
            <div className="space-y-3">
              {scanners.map((s) => (
                <div key={s.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-foreground">{s.name}</h3>
                    <p className="text-xs text-muted-foreground mt-1">Kết nối: <span className="font-semibold text-foreground">{s.conn}</span> | Phím kết thúc: <code className="bg-surface border px-1 py-0.5 rounded text-xs">{s.suffix}</code> | Timeout: {s.timeout}ms</p>
                  </div>
                  <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                    ● Sẵn sàng quét
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">➕ Thêm Đầu Quét Mã Mới</h2>
            <form onSubmit={handleSaveScanner} className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Tên thiết bị quét:</label>
                <input
                  type="text"
                  value={scannerForm.name}
                  onChange={(e) => setScannerForm({ ...scannerForm, name: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: Quét mã quầy tính tiền 02"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Chuẩn kết nối:</label>
                <select
                  value={scannerForm.conn}
                  onChange={(e) => setScannerForm({ ...scannerForm, conn: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                >
                  <option value="USB_HID">USB Keyboard Emulation (USB_HID)</option>
                  <option value="USB_COM">Virtual COM Port (USB_COM)</option>
                  <option value="Bluetooth">Bluetooth 无线</option>
                  <option value="SerialRS232">Cổng nối tiếp Serial RS232</option>
                </select>
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Phím Suffix gửi kèm:</label>
                <input
                  type="text"
                  value={scannerForm.suffix}
                  onChange={(e) => setScannerForm({ ...scannerForm, suffix: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono"
                  placeholder="VD: ENTER"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
                Lưu Cấu Hình Đầu Quét
              </button>
            </form>
          </div>
        </div>
      )}

      {activeTab === 'offline' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div className="flex justify-between items-center border-b border-border pb-4">
            <div>
              <h2 className="text-lg font-bold text-foreground">📡 Bộ Đệm Đơn Hàng POS Chế Độ Offline (UC_POS_008)</h2>
              <p className="text-xs text-muted-foreground mt-0.5">Tự động lưu đơn hàng khi mất mạng internet và đồng bộ khi khôi phục kết nối</p>
            </div>
            <span className={`px-3 py-1 text-xs rounded-full border ${offlineBadge.badgeClass}`}>
              {offlineBadge.label}
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-surface p-4 rounded-xl border border-border">
              <span className="text-xs font-semibold text-muted-foreground block">MÃ TRẠM THU NGÂN</span>
              <span className="text-xl font-bold text-foreground mt-1 block">{offlineBuffer.terminalCode}</span>
            </div>
            <div className="bg-surface p-4 rounded-xl border border-border">
              <span className="text-xs font-semibold text-muted-foreground block">ĐƠN HÀNG ĐANG LƯU ĐỆM</span>
              <span className="text-xl font-bold text-amber-600 mt-1 block">{offlineBuffer.ordersCount} đơn</span>
            </div>
            <div className="bg-surface p-4 rounded-xl border border-border">
              <span className="text-xs font-semibold text-muted-foreground block">TỔNG DOANH SỐ ĐỆM OFFLINE</span>
              <span className="text-xl font-bold text-foreground mt-1 block">{offlineBuffer.revenueTotal.toLocaleString('vi-VN')} VNĐ</span>
            </div>
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <button
              onClick={handleSyncOfflineOrders}
              disabled={offlineBuffer.ordersCount === 0}
              className={`px-5 py-2.5 rounded-lg text-sm font-bold shadow-sm transition-all ${
                offlineBuffer.ordersCount > 0 ? 'bg-brand text-brand-foreground hover:opacity-90' : 'bg-slate-200 text-slate-500 cursor-not-allowed'
              }`}
            >
              🚀 Kích Hoạt Đồng Bộ Đơn Offline Lên Server
            </button>
          </div>
        </div>
      )}

      {activeTab === 'attributes' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🏷️ Thuộc Tính Sản Phẩm & Thứ Tự Ảnh Hiển Thị (UC_POS_011 & UC_POS_013)</h2>
          <div className="space-y-3">
            {attributes.map((a) => (
              <div key={a.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                <div className="flex items-center gap-3">
                  <span className="w-8 h-8 rounded-full bg-brand-muted text-brand-strong flex items-center justify-center font-bold text-xs">
                    #{a.order}
                  </span>
                  <div>
                    <h3 className="font-bold text-foreground">{a.attrName}: <span className="text-brand-strong">{a.optionVal}</span></h3>
                    <p className="text-xs text-muted-foreground mt-0.5">Giá phụ thu: +{a.extraPrice.toLocaleString('vi-VN')} VNĐ {a.isDefault && '• (Mac định)'}</p>
                  </div>
                </div>
                <span className="text-xs font-semibold text-muted-foreground">Thứ tự hiển thị POS: {a.order}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
