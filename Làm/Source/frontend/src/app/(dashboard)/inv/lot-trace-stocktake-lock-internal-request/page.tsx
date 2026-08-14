'use client';

import React, { useState } from 'react';
import {
  getLotTraceDirectionLabel,
  getStocktakeLockStatusPill,
} from '@/shared/api/inv-lot-trace-stocktake-lock-internal-request-helpers';

export default function InvLotTraceStocktakeLockInternalRequestPage() {
  const [activeTab, setActiveTab] = useState<'trace' | 'stocktake' | 'lock' | 'internalReq'>('trace');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_INV_047: Truy vết lô xuôi/ngược
  const [lotQuery, setLotQuery] = useState('LOT-MILK-2026');
  const [traceDirection, setTraceDirection] = useState<'Forward' | 'Backward'>('Forward');
  const [lotTraces] = useState([
    { id: 't-1', lot: 'LOT-MILK-2026', direction: 'Forward', supplier: 'PO-2026-001 (NCC Vinamilk)', batch: 'BATCH-VNM-001', so: 'SO-RETAIL-088', time: '2026-08-01' },
    { id: 't-2', lot: 'LOT-MILK-2026', direction: 'Backward', supplier: 'PO-2026-001 (NCC Vinamilk)', batch: 'BATCH-VNM-001', so: 'SO-RETAIL-001', time: '2026-08-10' },
  ]);

  // UC_INV_051: Kiểm kê theo vị trí / nhóm
  const [stkForm, setStkForm] = useState({
    scopeType: 'ByLocation',
    target: 'Khu Vực Kệ Hàng A1 - A5',
    planned: 200,
  });

  const handleCreateStk = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã tạo kế hoạch kiểm kê STK-20260814 cho [${stkForm.target}]!`, 'success');
  };

  // UC_INV_054: Khóa giao dịch khi đang kiểm kê
  const [isLocked, setIsLocked] = useState(true);

  const toggleLock = () => {
    setIsLocked(!isLocked);
    showToast(!isLocked ? '🔒 Đã bật chế độ KHÓA GIAO DỊCH kho phục vụ kiểm kê!' : '🔓 Đã MỞ KHÓA giao dịch kho hoạt động bình thường.', !isLocked ? 'error' : 'success');
  };

  // UC_INV_056: Đề nghị xuất nội bộ
  const [intForm, setIntForm] = useState({
    dept: 'Phòng Kỹ Thuật Bảo Trì',
    purpose: 'Xuất vật tư dầu nhờn & thiết bị thay thế phục vụ bảo dưỡng máy phát điện',
    cost: 1500000,
  });

  const handleSaveInternal = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã gửi đề nghị xuất kho nội bộ REQ-INT-20260814 từ [${intForm.dept}]!`, 'success');
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
              INV - LOT GENEALOGY, GROUP STOCKTAKE, TRANSACTION LOCK & INTERNAL REQUISITION
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Truy Vết Lô 2 Chiều, Kiểm Kê Theo Vị Trí, Khóa Kho & Đề Nghị Xuất Nội Bộ</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Truy vấn lịch sử phả hệ lô hàng xuôi/ngược, tạo kế hoạch kiểm đếm theo khu vực/nhóm, cơ chế chốt khóa kho và duyệt đề nghị xuất nội bộ
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs INV)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('trace')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'trace' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🧬 UC_INV_047: Truy Vết Lô 2 Chiều
          </button>
          <button
            onClick={() => setActiveTab('stocktake')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'stocktake' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📋 UC_INV_051: Kiểm Kê Theo Vị Trí / Nhóm
          </button>
          <button
            onClick={() => setActiveTab('lock')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'lock' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔒 UC_INV_054: Khóa Giao Dịch Kiểm Kê
          </button>
          <button
            onClick={() => setActiveTab('internalReq')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'internalReq' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📝 UC_INV_056: Đề Nghị Xuất Nội Bộ
          </button>
        </div>
      </div>

      {activeTab === 'trace' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-5">
          <div className="flex gap-3 items-end">
            <div className="flex-1">
              <label className="block text-foreground font-medium mb-1 text-sm">Số Lô Hàng Cần Phân Tích Phả Hệ (Lot Number):</label>
              <input
                type="text"
                value={lotQuery}
                onChange={(e) => setLotQuery(e.target.value)}
                className="w-full border border-border rounded-lg p-2.5 bg-surface text-foreground font-bold font-mono text-sm"
              />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1 text-sm">Hướng Truy Vết:</label>
              <select
                value={traceDirection}
                onChange={(e) => setTraceDirection(e.target.value as any)}
                className="border border-border rounded-lg p-2.5 bg-surface text-foreground font-bold text-sm"
              >
                <option value="Forward">Truy Vết Xuôi (NCC ➔ Khách Hàng)</option>
                <option value="Backward">Truy Vết Ngược (Khách Hàng ➔ NCC)</option>
              </select>
            </div>
            <button
              onClick={() => showToast(`✓ Đã truy xuất phả hệ lô [${lotQuery}] theo chiều ${traceDirection}!`, 'success')}
              className="px-5 py-2.5 bg-brand text-brand-foreground font-bold text-sm rounded-lg hover:opacity-90 shadow-sm"
            >
              🔍 Truy Vết Phả Hệ Lô
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Lô (Lot No)</th>
                  <th className="p-3">Loại Truy Vết</th>
                  <th className="p-3">Nguồn Gốc NCC / PO</th>
                  <th className="p-3">Lô Sản Xuất</th>
                  <th className="p-3">Đơn Bán Hàng (SO)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {lotTraces.map((t) => {
                  const dir = getLotTraceDirectionLabel(t.direction);
                  return (
                    <tr key={t.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-foreground">{t.lot}</td>
                      <td className="p-3">
                        <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${dir.colorClass}`}>
                          {dir.label}
                        </span>
                      </td>
                      <td className="p-3 text-slate-700 font-medium">{t.supplier}</td>
                      <td className="p-3 font-mono text-slate-700">{t.batch}</td>
                      <td className="p-3 font-mono font-bold text-emerald-800">{t.so}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'stocktake' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">📋 Lập Kế Hoạch Kiểm Kê Kho Theo Vị Trí / Nhóm (UC_INV_051)</h2>
          <form onSubmit={handleCreateStk} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Phương Thức Phân Loại:</label>
                <select
                  value={stkForm.scopeType}
                  onChange={(e) => setStkForm({ ...stkForm, scopeType: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold"
                >
                  <option value="ByLocation">Theo Vị Trí Ô Kệ (By Location)</option>
                  <option value="ByProductGroup">Theo Nhóm Ngành Hàng (By Product Group)</option>
                </select>
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Lượng Mặt Hàng Dự Kiến Đếm:</label>
                <input
                  type="number"
                  value={stkForm.planned}
                  onChange={(e) => setStkForm({ ...stkForm, planned: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold"
                />
              </div>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Phạm Vi Khu Vực / Tên Nhóm Mục Tiêu:</label>
              <input
                type="text"
                value={stkForm.target}
                onChange={(e) => setStkForm({ ...stkForm, target: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold"
              />
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              📋 Khởi Tạo Đợt Kiểm Kê Cục Bộ Mới
            </button>
          </form>
        </div>
      )}

      {activeTab === 'lock' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <div>
            <h2 className="text-lg font-bold text-foreground">🔒 Cơ Chế Khóa Giao Dịch Kho Khi Đang Kiểm Kê (UC_INV_054)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Tự động chặn các giao dịch Nhập/Xuất/Chuyển kho tại các khu vực đang có nhân viên đếm hàng</p>
          </div>

          <div className="p-4 rounded-xl border border-border bg-surface-hover flex items-center justify-between">
            <div>
              <div className="text-xs text-muted-foreground font-semibold">TRẠNG THÁI HIỆN TẠI:</div>
              <div className="mt-1">
                {(() => {
                  const p = getStocktakeLockStatusPill(isLocked);
                  return <span className={`px-3 py-1.5 text-xs font-black rounded-full border ${p.colorClass}`}>{p.label}</span>;
                })()}
              </div>
            </div>
            <button
              onClick={toggleLock}
              className={`px-4 py-2 font-bold text-xs rounded-lg text-white shadow-sm ${isLocked ? 'bg-emerald-600 hover:bg-emerald-700' : 'bg-rose-600 hover:bg-rose-700'}`}
            >
              {isLocked ? '🔓 Mở Khóa Giao Dịch' : '🔒 Khóa Giao Dịch Kho'}
            </button>
          </div>
        </div>
      )}

      {activeTab === 'internalReq' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">📝 Lập Phiếu Đề Nghị Xuất Nội Bộ (UC_INV_056)</h2>
          <form onSubmit={handleSaveInternal} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Phòng Ban Đề Nghị Xuất:</label>
              <input
                type="text"
                value={intForm.dept}
                onChange={(e) => setIntForm({ ...intForm, dept: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold"
              />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Mục Đích Xuất Nội Bộ:</label>
              <textarea
                value={intForm.purpose}
                onChange={(e) => setIntForm({ ...intForm, purpose: e.target.value })}
                rows={3}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Ước Tính Tổng Chi Phí (VNĐ):</label>
              <input
                type="number"
                value={intForm.cost}
                onChange={(e) => setIntForm({ ...intForm, cost: Number(e.target.value) })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold"
              />
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              💾 Gửi Đề Nghị Xuất Nội Bộ (REQ-INT)
            </button>
          </form>
        </div>
      )}
    </div>
  );
}
