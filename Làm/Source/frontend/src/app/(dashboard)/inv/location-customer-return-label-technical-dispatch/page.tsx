'use client';

import React, { useState } from 'react';
import {
  formatBinLocationCode,
  getInspectionConditionLabel,
} from '@/shared/api/inv-location-customer-return-label-technical-dispatch-helpers';

export default function InvLocationCustomerReturnLabelTechnicalDispatchPage() {
  const [activeTab, setActiveTab] = useState<'location' | 'return' | 'label' | 'dispatch'>('location');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_INV_013: Vị trí / kệ / bin
  const [binForm, setBinForm] = useState({ zone: 'Zone A', aisle: 'Aisle 01', rack: 'Rack 02', bin: 'Bin 05' });
  const [binList, setBinList] = useState([
    { id: 'b-1', code: 'Zone A-Aisle 01-Rack 01-Bin 01', zone: 'Zone A', aisle: 'Aisle 01', active: true },
    { id: 'b-2', code: 'Zone A-Aisle 01-Rack 01-Bin 02', zone: 'Zone A', aisle: 'Aisle 01', active: true },
  ]);

  const handleAddBinLocation = (e: React.FormEvent) => {
    e.preventDefault();
    const code = formatBinLocationCode(binForm.zone, binForm.aisle, binForm.rack, binForm.bin);
    setBinList([...binList, { id: 'b-' + (binList.length + 1), code, zone: binForm.zone, aisle: binForm.aisle, active: true }]);
    showToast(`✓ Đã bổ sung vị trí ô kệ mới [${code}] vào danh mục sơ đồ kho!`, 'success');
  };

  // UC_INV_021: Nhập trả từ khách
  const [returnForm, setReturnForm] = useState({
    customer: 'Công Ty TNHH Thương Mại Minh Phát',
    so: 'SO-2026-088',
    reason: 'Hàng móp vỏ hộp vận chuyển',
    condition: 'GoodRestockable',
    amount: 1250000,
  });

  const handleSaveReturn = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã tạo phiếu nhập hàng khách trả RET-20260814 (${returnForm.amount.toLocaleString('vi-VN')} đ)!`, 'success');
  };

  // UC_INV_023: In tem lô / serial
  const [labelForm, setLabelForm] = useState({
    sku: 'SKU-RAM-16GB',
    lot: 'LOT-20260814',
    serial: 'SN-RAM-998811',
    mfg: '2026-08-01',
    exp: '2029-08-01',
  });

  // UC_INV_027: Xuất cho dịch vụ kỹ thuật
  const [dispatchForm, setDispatchForm] = useState({
    ticket: 'TK-SERVICE-044',
    technician: 'Nguyễn Văn Kỹ Thuật',
    partsValue: 2400000,
    comments: 'Xuất thanh ram và nguồn dự phòng sửa máy chủ trạm bảo hành',
  });

  const handleSaveDispatch = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã lập phiếu xuất kho vật tư linh kiện sửa chữa kỹ thuật TSD-20260814!`, 'success');
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
              INV - BIN LOCATIONS, CUSTOMER RETURNS, LOT/SERIAL LABELS & TECHNICAL DISPATCH
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Sơ Đồ Kệ Kho, Hàng Khách Trả, In Tem Lô/Serial & Xuất Kho Kỹ Thuật</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý chi tiết vị trí dãy/kệ/bin, xử lý kiểm kê hàng khách nhập trả, đóng gói tem nhãn Lô/Serial và xuất linh kiện vật tư sửa chữa
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
            onClick={() => setActiveTab('location')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'location' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📍 UC_INV_013: Vị Trí / Kệ / Bin
          </button>
          <button
            onClick={() => setActiveTab('return')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'return' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📦 UC_INV_021: Nhập Trả Từ Khách
          </button>
          <button
            onClick={() => setActiveTab('label')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'label' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏷️ UC_INV_023: In Tem Lô / Serial
          </button>
          <button
            onClick={() => setActiveTab('dispatch')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'dispatch' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🛠️ UC_INV_027: Xuất Kho Dịch Vụ Kỹ Thuật
          </button>
        </div>
      </div>

      {activeTab === 'location' && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-base font-bold text-foreground">Thêm Vị Trí Ô Kệ Bin Mới (UC_INV_013)</h2>
            <form onSubmit={handleAddBinLocation} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Khu Vực (Zone):</label>
                <input type="text" value={binForm.zone} onChange={(e) => setBinForm({ ...binForm, zone: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Dãy (Aisle):</label>
                <input type="text" value={binForm.aisle} onChange={(e) => setBinForm({ ...binForm, aisle: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface" />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="block text-foreground font-medium mb-1">Kệ (Rack):</label>
                  <input type="text" value={binForm.rack} onChange={(e) => setBinForm({ ...binForm, rack: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface" />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Hộc/Bin:</label>
                  <input type="text" value={binForm.bin} onChange={(e) => setBinForm({ ...binForm, bin: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface" />
                </div>
              </div>
              <div className="p-3 rounded-lg border border-brand/30 bg-brand-muted/10 text-xs font-bold text-brand-strong">
                MÃ KHU VỰC DỰ KIẾN: {formatBinLocationCode(binForm.zone, binForm.aisle, binForm.rack, binForm.bin)}
              </div>
              <button type="submit" className="w-full py-2 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90">
                + Thêm Vị Trí Ô Kệ Kho
              </button>
            </form>
          </div>

          <div className="md:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-base font-bold text-foreground">Sơ Đồ Vị Trí Kệ Bin Đã Đăng Ký</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Mã Vị Trí Vận Hành</th>
                    <th className="p-3">Khu Vực</th>
                    <th className="p-3">Dãy Kệ</th>
                    <th className="p-3 text-right">Trạng Thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {binList.map((b) => (
                    <tr key={b.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-foreground">{b.code}</td>
                      <td className="p-3 text-slate-600">{b.zone}</td>
                      <td className="p-3 text-slate-600">{b.aisle}</td>
                      <td className="p-3 text-right">
                        <span className="px-2 py-0.5 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800">● ĐANG HOẠT ĐỘNG</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'return' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">📦 Lập Phiếu Nhập Trả Từ Khách Hàng (UC_INV_021)</h2>
          <form onSubmit={handleSaveReturn} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Tên Khách Hàng:</label>
              <input type="text" value={returnForm.customer} onChange={(e) => setReturnForm({ ...returnForm, customer: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Đơn Bán Hàng (SO):</label>
                <input type="text" value={returnForm.so} onChange={(e) => setReturnForm({ ...returnForm, so: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tổng Số Tiền Hoàn Trả (VNĐ):</label>
                <input
                  type="number"
                  value={returnForm.amount}
                  onChange={(e) => setReturnForm({ ...returnForm, amount: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold"
                />
              </div>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Lý Do Khách Trả Hàng:</label>
              <input type="text" value={returnForm.reason} onChange={(e) => setReturnForm({ ...returnForm, reason: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Kết Quả Kiểm Định Chất Lượng (Quality Inspection):</label>
              <select
                value={returnForm.condition}
                onChange={(e) => setReturnForm({ ...returnForm, condition: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold"
              >
                <option value="GoodRestockable">GoodRestockable — Hàng Đạt Chuẩn, Nhập Lại Kho</option>
                <option value="NeedsRefurbish">NeedsRefurbish — Cần Đóng Gói / Sửa Chữa Lại</option>
                <option value="DamagedScrap">DamagedScrap — Hàng Hư Hỏng, Thanh Lý / Phế Liệu</option>
              </select>
            </div>

            {/* Preview Status Pill */}
            <div className="p-3 rounded-lg border border-border bg-surface-hover flex items-center justify-between">
              <span className="text-xs font-semibold text-muted-foreground">ĐÁNH GIÁ PHÂN LOẠI KHO:</span>
              {(() => {
                const cond = getInspectionConditionLabel(returnForm.condition);
                return <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${cond.colorClass}`}>{cond.label}</span>;
              })()}
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              💾 Hoàn Tất Phiếu Nhập Trả Khách Hàng
            </button>
          </form>
        </div>
      )}

      {activeTab === 'label' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">🏷️ Thiết Kế & In Tem Nhãn Lô Batch / Serial Number (UC_INV_023)</h2>
          <div className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Sản Phẩm (SKU):</label>
                <input type="text" value={labelForm.sku} onChange={(e) => setLabelForm({ ...labelForm, sku: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Lô (Lot Number):</label>
                <input type="text" value={labelForm.lot} onChange={(e) => setLabelForm({ ...labelForm, lot: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
            </div>

            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Số Serial (Serial No):</label>
                <input type="text" value={labelForm.serial} onChange={(e) => setLabelForm({ ...labelForm, serial: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono text-xs" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Ngày Sản Xuất:</label>
                <input type="date" value={labelForm.mfg} onChange={(e) => setLabelForm({ ...labelForm, mfg: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Hạn Sử Dụng (EXP):</label>
                <input type="date" value={labelForm.exp} onChange={(e) => setLabelForm({ ...labelForm, exp: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
            </div>

            {/* Template preview */}
            <div className="p-4 rounded-xl border border-dashed border-brand/40 bg-brand-muted/10 space-y-2 text-center">
              <span className="text-xs font-bold text-brand-strong">PREVIEW TEM LÔ SERIAL (60x40mm)</span>
              <div className="bg-white p-4 rounded-lg shadow-inner inline-block space-y-1 text-left border border-slate-300 w-full max-w-xs">
                <div className="text-xs font-extrabold text-black">{labelForm.sku}</div>
                <div className="text-[11px] font-mono font-bold text-slate-800">LOT: {labelForm.lot}</div>
                <div className="text-[11px] font-mono font-bold text-slate-800">S/N: {labelForm.serial}</div>
                <div className="text-[10px] text-slate-600">MFG: {labelForm.mfg} | EXP: {labelForm.exp}</div>
              </div>
            </div>

            <button
              onClick={() => showToast(`✓ Đã chuyển lệnh in tem Lô [${labelForm.lot}] / Serial [${labelForm.serial}] sang máy in Barcode!`, 'success')}
              className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm"
            >
              🖨️ Xuất Lệnh In Tem Nhãn Lô / Serial
            </button>
          </div>
        </div>
      )}

      {activeTab === 'dispatch' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">🛠️ Lập Phiếu Xuất Kho Cho Dịch Vụ Kỹ Thuật (UC_INV_027)</h2>
          <form onSubmit={handleSaveDispatch} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Phiếu Bảo Hành / Sửa Chữa (Ticket):</label>
                <input
                  type="text"
                  value={dispatchForm.ticket}
                  onChange={(e) => setDispatchForm({ ...dispatchForm, ticket: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên Kỹ Thuật Viên Phụ Trách:</label>
                <input
                  type="text"
                  value={dispatchForm.technician}
                  onChange={(e) => setDispatchForm({ ...dispatchForm, technician: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold"
                />
              </div>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Giá Trị Linh Kiện Xuất Kho (VNĐ):</label>
              <input
                type="number"
                value={dispatchForm.partsValue}
                onChange={(e) => setDispatchForm({ ...dispatchForm, partsValue: Number(e.target.value) })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold"
              />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Ghi Chú Mục Đích Xuất Dịch Vụ Kỹ Thuật:</label>
              <textarea
                value={dispatchForm.comments}
                onChange={(e) => setDispatchForm({ ...dispatchForm, comments: e.target.value })}
                rows={3}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              💾 Xác Nhận Phiếu Xuất Kho Linh Kiện Kỹ Thuật
            </button>
          </form>
        </div>
      )}
    </div>
  );
}
