'use client';

import React, { useState } from 'react';
import {
  validateEan13BarcodeFormat,
  parseQrPayload,
} from '@/shared/api/inv-product-image-barcode-qr-helpers';

export default function InvProductImageBarcodeQrPage() {
  const [activeTab, setActiveTab] = useState<'media' | 'barcode' | 'scan'>('media');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_INV_006: Ảnh & mô tả sản phẩm
  const [mediaForm, setMediaForm] = useState({
    code: 'SKU-MILK-1L',
    name: 'Sữa Tươi Nguyên Chất 1L',
    url: 'https://images.unsplash.com/photo-1550583724-b2692b85b150?w=600',
    description: 'Sữa tươi nguyên chất tiệt trùng 100% nguyên liệu tự nhiên không đường, nhập khẩu tiệt trùng công nghệ Châu Âu.',
    material: '99.9% Sữa bò tươi nguyên chất, Vitamin D3 & C',
  });

  const handleSaveMedia = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã cập nhật hình ảnh đại diện và mô tả kỹ thuật cho sản phẩm [${mediaForm.code}]!`, 'success');
  };

  // UC_INV_009: Barcode / QR theo sản phẩm
  const [barcodeInput, setBarcodeInput] = useState('8935000123456');
  const [qrScanInput, setQrScanInput] = useState('ERP-PROD|p-100|SKU-MILK-1L|BC:8935000123456');

  const isBarcodeValid = validateEan13BarcodeFormat(barcodeInput);
  const scannedQrResult = parseQrPayload(qrScanInput);

  const handleGenerateBarcode = () => {
    if (!isBarcodeValid) {
      showToast('Mã Barcode EAN-13 phải bao gồm chính xác 13 chữ số.', 'error');
      return;
    }
    showToast(`✓ Đã tạo và đóng gói tem in Barcode [${barcodeInput}] & QR Code thành công!`, 'success');
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
              INV - PRODUCT MEDIA, RICH SPECIFICATION & BARCODE / QR CODE GENERATOR
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Ảnh Mô Tả Kỹ Thuật Sản Phẩm & Quản Lý Mã Vạch Barcode / QR Code</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Cập nhật thư viện ảnh sản phẩm, thông số quy chuẩn kỹ thuật và khởi tạo mã vạch Barcode EAN-13 / QR Code để in tem kiểm kê kho
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (2/2 UCs INV)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('media')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'media' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🖼️ UC_INV_006: Ảnh & Mô Tả Kỹ Thuật
          </button>
          <button
            onClick={() => setActiveTab('barcode')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'barcode' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏷️ UC_INV_009: Tạo Tem Barcode & QR Code
          </button>
          <button
            onClick={() => setActiveTab('scan')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'scan' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔍 Giả Lập Quét Mã QR Kho
          </button>
        </div>
      </div>

      {activeTab === 'media' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-3xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">🖼️ Cập Nhật Hình Ảnh & Mô Tả Sản Phẩm Kỹ Thuật (UC_INV_006)</h2>
          <form onSubmit={handleSaveMedia} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Sản Phẩm (SKU):</label>
                <input type="text" value={mediaForm.code} readOnly className="w-full border border-border rounded-lg p-2 bg-surface-hover text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên Sản Phẩm:</label>
                <input type="text" value={mediaForm.name} readOnly className="w-full border border-border rounded-lg p-2 bg-surface-hover text-foreground font-bold" />
              </div>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">URL Ảnh Đại Diện Sản Phẩm:</label>
              <input
                type="text"
                value={mediaForm.url}
                onChange={(e) => setMediaForm({ ...mediaForm, url: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Mô Tả Kỹ Thuật Chi Tiết (Rich Description):</label>
              <textarea
                value={mediaForm.description}
                onChange={(e) => setMediaForm({ ...mediaForm, description: e.target.value })}
                rows={3}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Quy Chuẩn Vật Liệu / Thành Phần (Material Spec):</label>
              <input
                type="text"
                value={mediaForm.material}
                onChange={(e) => setMediaForm({ ...mediaForm, material: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              💾 Lưu Thông Tin Kỹ Thuật & Media Sản Phẩm
            </button>
          </form>
        </div>
      )}

      {activeTab === 'barcode' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <div>
            <h2 className="text-lg font-bold text-foreground">🏷️ Khởi Tạo & In Tem Mã Vạch Barcode EAN-13 / QR Code (UC_INV_009)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Mã hóa thông tin mã sản phẩm thành chuẩn tem nhãn tiêu chuẩn 50x30mm để dán lên thùng/lô hàng</p>
          </div>

          <div className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Mã Vạch EAN-13 (13 chữ số):</label>
              <div className="flex gap-2">
                <input
                  type="text"
                  value={barcodeInput}
                  onChange={(e) => setBarcodeInput(e.target.value)}
                  className={`w-full border rounded-lg p-2 bg-surface font-mono font-bold ${isBarcodeValid ? 'border-border text-foreground' : 'border-rose-500 text-rose-600'}`}
                />
                <button
                  type="button"
                  onClick={() => setBarcodeInput('8935000' + Math.floor(100000 + Math.random() * 900000))}
                  className="px-3 py-2 bg-surface-hover text-foreground text-xs font-bold rounded-lg border border-border whitespace-nowrap"
                >
                  🎲 Tự Sinh Mã
                </button>
              </div>
              {!isBarcodeValid && <p className="text-xs text-rose-600 mt-1 font-semibold">⚠️ Vui lòng nhập đúng 13 chữ số chuẩn EAN-13</p>}
            </div>

            {/* Khung Xem Trước Tem In */}
            <div className="p-4 rounded-xl border border-dashed border-brand/40 bg-brand-muted/10 space-y-3 text-center">
              <span className="text-xs font-bold text-brand-strong">PREVIEW TEM IN SẢN PHẨM (50x30mm)</span>
              <div className="bg-white p-4 rounded-lg shadow-inner inline-block space-y-2 border border-slate-300">
                <div className="text-xs font-black text-black">SKU-MILK-1L</div>
                <div className="font-mono text-lg tracking-widest font-black text-slate-900 bg-slate-100 px-3 py-1 rounded">
                  ||||| ||| |||| ||||
                </div>
                <div className="text-xs font-mono font-bold text-slate-700">{barcodeInput}</div>
              </div>
            </div>

            <button
              onClick={handleGenerateBarcode}
              className="w-full py-3 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm"
            >
              🖨️ Xuất File In Tem Nhãn Barcode & QR Code
            </button>
          </div>
        </div>
      )}

      {activeTab === 'scan' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-xl space-y-4">
          <h2 className="text-lg font-bold text-foreground">🔍 Giả Lập Đầu Quét Mã QR / Barcode Kiểm Kê Kho</h2>
          <div>
            <label className="block text-foreground font-medium mb-1">Chuỗi Payload Quét Được:</label>
            <input
              type="text"
              value={qrScanInput}
              onChange={(e) => setQrScanInput(e.target.value)}
              className="w-full border border-border rounded-lg p-2.5 bg-surface text-foreground font-mono text-xs"
            />
          </div>

          <div className={`p-4 rounded-xl border ${scannedQrResult.isProductQr ? 'border-emerald-300 bg-emerald-50/50' : 'border-rose-300 bg-rose-50/50'}`}>
            {scannedQrResult.isProductQr ? (
              <div className="space-y-1 text-xs text-emerald-900 font-medium">
                <div className="font-extrabold text-sm text-emerald-950">✓ NHẬN DẠNG MÃ QR SẢN PHẨM THÀNH CÔNG</div>
                <div>Mã Sản Phẩm: <b className="text-foreground">{scannedQrResult.productCode}</b></div>
                <div>Mã Vạch: <b className="text-foreground">{scannedQrResult.barcode}</b></div>
              </div>
            ) : (
              <div className="text-xs text-rose-800 font-bold">⚠️ Mã QR không thuộc định dạng sản phẩm hệ thống ERP Hùng</div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
