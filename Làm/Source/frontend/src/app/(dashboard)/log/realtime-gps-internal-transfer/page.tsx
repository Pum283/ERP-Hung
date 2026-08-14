'use client';

import React, { useState } from 'react';
import {
  getInternalTransferStatusBadge,
  formatGpsCoordinates,
} from '@/shared/api/log-realtime-gps-internal-transfer-helpers';

export default function LogRealtimeGpsInternalTransferPage() {
  const [activeTab, setActiveTab] = useState<'gps' | 'internal' | 'reconcile'>('gps');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_LOG_019: Theo dõi realtime trên bản đồ
  const [fleetLocations] = useState([
    { id: 'p-1', plate: '51D-889.99 (Trần Văn Tài)', lat: 10.7769, lng: 106.7009, speed: 45.0, address: 'Vòng xoay An Lạc, Bình Tân, TP.HCM' },
    { id: 'p-2', plate: '50LD-123.45 (Nguyễn Hoàng Lái)', lat: 10.8231, lng: 106.6297, speed: 30.2, address: 'Đường Trường Chinh, Tân Bình, TP.HCM' },
  ]);

  // UC_LOG_031 & UC_LOG_032: Lệnh giao nội bộ & Xác nhận nhận hàng
  const [transferList, setTransferList] = useState([
    { id: 't-1', docNo: 'DEL-INT-2026-001', from: 'Kho Tổng Miền Nam', to: 'Kho CN 1 Bình Dương', driver: 'Trần Văn Tài', plate: '51D-889.99', sendQty: 500, recvQty: 0, status: 'InTransit' },
    { id: 't-2', docNo: 'DEL-INT-2026-002', from: 'Kho Tổng Miền Nam', to: 'Kho CN 2 Đồng Nai', driver: 'Nguyễn Hoàng Lái', plate: '50LD-123.45', sendQty: 200, recvQty: 190, status: 'DiscrepancyReported' },
  ]);

  const [createForm, setCreateForm] = useState({
    from: 'Kho Tổng Miền Nam',
    to: 'Kho Chi Nhánh Vũng Tàu',
    driver: 'Trần Văn Tài',
    plate: '51D-889.99',
    qty: 350,
  });

  const handleCreateTransfer = (e: React.FormEvent) => {
    e.preventDefault();
    const newDoc = {
      id: 't-' + Date.now(),
      docNo: 'DEL-INT-' + Math.floor(1000 + Math.random() * 9000),
      from: createForm.from,
      to: createForm.to,
      driver: createForm.driver,
      plate: createForm.plate,
      sendQty: createForm.qty,
      recvQty: 0,
      status: 'InTransit',
    };
    setTransferList([...transferList, newDoc]);
    showToast(`✓ Đã phát lệnh giao nội bộ [${newDoc.docNo}] từ [${createForm.from}] đi [${createForm.to}]!`, 'success');
  };

  const handleConfirmReceipt = (id: string, full: boolean) => {
    setTransferList(transferList.map(t => {
      if (t.id === id) {
        return {
          ...t,
          recvQty: full ? t.sendQty : t.sendQty - 10,
          status: full ? 'Received' : 'DiscrepancyReported',
        };
      }
      return t;
    }));
    showToast(full ? '✓ Đã xác nhận nhập kho nội bộ đủ 100%!' : '⚠️ Đã ghi nhận nhận thiếu hàng và phát cờ lệch đối soát!', full ? 'success' : 'error');
  };

  // UC_LOG_033: Đối soát giao nội bộ
  const [reconcileList] = useState([
    { id: 'r-1', code: 'REC-INT-2026-001', docNo: 'DEL-INT-2026-002', send: 200, recv: 190, diff: 10, cost: 2500000, cause: 'Hao hụt 10 đơn vị trong bốc xếp dỡ hàng' },
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
              LOG - REALTIME GPS TRACKING, INTERNAL TRANSFERS, RECEIPT & RECONCILIATION
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Theo Dõi GPS Trực Tuyến, Lệnh Giao Nội Bộ & Đối Soát Chênh Lệch Kho</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Giám sát vị trí phương tiện thời gian thực, điều phối luân chuyển liên kho và kiểm soát đối soát số lượng nhận thực tế
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs LOG)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('gps')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'gps' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🛰️ UC_LOG_019: Giám Sát GPS Bản Đồ
          </button>
          <button
            onClick={() => setActiveTab('internal')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'internal' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📦 UC_LOG_031 & 032: Giao & Nhận Nội Bộ
          </button>
          <button
            onClick={() => setActiveTab('reconcile')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'reconcile' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚖️ UC_LOG_033: Đối Soát Lệch Nội Bộ
          </button>
        </div>
      </div>

      {activeTab === 'gps' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div className="p-4 rounded-xl border border-blue-200 bg-blue-50/50 flex justify-between items-center">
            <div>
              <span className="text-xs font-bold text-blue-800">TRẠNG THÁI GPS TELEMETRY:</span>
              <div className="text-sm font-bold text-slate-800 mt-0.5">● Kết nối trực tuyến với 2 phương tiện đang di chuyển</div>
            </div>
            <button onClick={() => showToast('✓ Đã cập nhật tọa độ GPS mới nhất từ thiết bị!', 'success')} className="px-3 py-1.5 bg-blue-600 text-white text-xs font-bold rounded-lg hover:bg-blue-700">
              🔄 Làm Mới Tọa Độ
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Xe & Tài Xế</th>
                  <th className="p-3">Tọa Độ GPS</th>
                  <th className="p-3 text-center">Tốc Độ Hiện Tại</th>
                  <th className="p-3">Địa Chỉ Gần Nhất</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {fleetLocations.map((loc) => (
                  <tr key={loc.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{loc.plate}</td>
                    <td className="p-3 font-mono text-slate-700">{formatGpsCoordinates(loc.lat, loc.lng)}</td>
                    <td className="p-3 text-center font-extrabold text-brand">{loc.speed} km/h</td>
                    <td className="p-3 text-slate-700 font-medium">{loc.address}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'internal' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📦 Lập Lệnh Giao Nội Bộ Mới (UC_LOG_031)</h2>
            <form onSubmit={handleCreateTransfer} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Kho Xuất Hàng:</label>
                <input type="text" value={createForm.from} onChange={(e) => setCreateForm({ ...createForm, from: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Kho Đích Nhận:</label>
                <input type="text" value={createForm.to} onChange={(e) => setCreateForm({ ...createForm, to: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tài Xế & Biển Số:</label>
                <input type="text" value={createForm.driver} onChange={(e) => setCreateForm({ ...createForm, driver: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Lượng Kiện / Đơn Vị:</label>
                <input type="number" value={createForm.qty} onChange={(e) => setCreateForm({ ...createForm, qty: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                📦 Phát Lệnh Giao Chuyển Kho (DEL-INT)
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Lệnh Chuyển Kho & Xác Nhận Nhận Hàng (UC_LOG_032)</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Mã Lệnh Giao</th>
                    <th className="p-3">Tuyến Kho</th>
                    <th className="p-3 text-center">Xuất / Nhận</th>
                    <th className="p-3">Trạng Thái</th>
                    <th className="p-3 text-right">Thao Tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {transferList.map((t) => {
                    const badge = getInternalTransferStatusBadge(t.status);
                    return (
                      <tr key={t.id} className="hover:bg-surface-hover/50">
                        <td className="p-3 font-mono font-bold text-foreground">{t.docNo}</td>
                        <td className="p-3">
                          <div className="font-bold text-foreground">{t.from} ➔ {t.to}</div>
                          <div className="text-xs text-muted-foreground">Tài xế: {t.driver} ({t.plate})</div>
                        </td>
                        <td className="p-3 text-center font-bold text-slate-800">
                          {t.sendQty} / <span className={t.recvQty > 0 && t.recvQty < t.sendQty ? 'text-rose-700 font-extrabold' : ''}>{t.recvQty}</span>
                        </td>
                        <td className="p-3">
                          <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${badge.colorClass}`}>
                            ● {badge.label}
                          </span>
                        </td>
                        <td className="p-3 text-right space-x-1.5">
                          {t.status === 'InTransit' && (
                            <>
                              <button onClick={() => handleConfirmReceipt(t.id, true)} className="px-2.5 py-1 bg-emerald-600 text-white text-xs font-bold rounded hover:bg-emerald-700">
                                ✓ Nhận Đủ
                              </button>
                              <button onClick={() => handleConfirmReceipt(t.id, false)} className="px-2.5 py-1 bg-rose-600 text-white text-xs font-bold rounded hover:bg-rose-700">
                                ⚠️ Nhận Thiếu
                              </button>
                            </>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'reconcile' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⚖️ Biên Bản Đối Soát & Bồi Hoàn Chênh Lệch Giao Nội Bộ (UC_LOG_033)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Đối Soát</th>
                  <th className="p-3">Lệnh Giao Gốc</th>
                  <th className="p-3 text-center">SL Xuất / Nhận</th>
                  <th className="p-3 text-center">Lệch (SL)</th>
                  <th className="p-3 text-right">Chi Phí Bồi Hoàn (VNĐ)</th>
                  <th className="p-3">Nguyên Nhân & Kết Luận</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {reconcileList.map((r) => (
                  <tr key={r.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{r.code}</td>
                    <td className="p-3 font-mono font-bold text-foreground">{r.docNo}</td>
                    <td className="p-3 text-center font-bold text-slate-700">{r.send} / {r.recv}</td>
                    <td className="p-3 text-center font-black text-rose-700">-{r.diff} cái</td>
                    <td className="p-3 text-right font-extrabold text-rose-700">{r.cost.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-xs text-muted-foreground">{r.cause}</td>
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
