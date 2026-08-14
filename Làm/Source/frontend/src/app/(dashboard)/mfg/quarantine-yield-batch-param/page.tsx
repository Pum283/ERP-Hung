'use client';

import React, { useState } from 'react';
import {
  formatYieldPercentage,
  formatBatchQuantity,
} from '@/shared/api/mfg-quarantine-yield-batch-param-helpers';

export default function MfgQuarantineYieldBatchParamPage() {
  const [activeTab, setActiveTab] = useState<'quarantine' | 'yield' | 'batch' | 'params'>('quarantine');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_MFG_035: Cách ly hàng lỗi
  const [quarantineList, setQuarantineList] = useState([
    { id: 'q-1', holdNo: 'Q-HOLD-2026-001', lot: 'LOT-2026-0814-B', item: 'FG-DESK-WOOD', qty: 2, loc: 'KHO-CACH-LY-01', defect: 'Nứt mối ghép mộng gỗ', status: 'UnderQuarantine' },
  ]);

  const [quarantineForm, setQuarantineForm] = useState({
    lot: 'LOT-2026-0814-C',
    item: 'FG-SERVER-42U',
    qty: 3,
    loc: 'KHO-CACH-LY-01',
    defect: 'Xước sơn tĩnh điện bề mặt cánh cửa',
  });

  const handleCreateQuarantine = (e: React.FormEvent) => {
    e.preventDefault();
    const newQ = {
      id: 'q-' + Date.now(),
      holdNo: 'Q-HOLD-' + Math.floor(1000 + Math.random() * 9000),
      ...quarantineForm,
      status: 'UnderQuarantine',
    };
    setQuarantineList([...quarantineList, newQ]);
    showToast(`✓ Đã niêm phong cách ly [${newQ.holdNo}] cho lô [${quarantineForm.lot}] (${quarantineForm.qty} SP)!`, 'success');
  };

  // UC_MFG_036: Báo cáo tỷ lệ đạt QC
  const [yieldSummary] = useState({
    totalLots: 45,
    inspectedQty: 12500,
    passedQty: 12200,
    rejectedQty: 300,
    passRate: 97.6,
    fpyRate: 95.2,
  });

  // UC_MFG_037: Lô/mẻ sản xuất
  const [batches, setBatches] = useState([
    { id: 'b-1', batchNo: 'BATCH-20260814-01', wo: 'WO-2026-088', sku: 'FG-SERVER-42U', planned: 300, actual: 280, mfgDate: '2026-08-14', expDate: '2028-08-14', status: 'InProduction' },
    { id: 'b-2', batchNo: 'BATCH-20260814-02', wo: 'WO-2026-089', sku: 'FG-DESK-WOOD', planned: 150, actual: 150, mfgDate: '2026-08-14', expDate: '2029-08-14', status: 'Completed' },
  ]);

  // UC_MFG_038: Ghi nhận thông số mẻ
  const [paramList, setParamList] = useState([
    { id: 'p-1', batch: 'BATCH-20260814-01', param: 'Nhiệt Độ Lò Nung Sơn', target: 180, actual: 181.5, uom: '°C', ok: true, by: 'Kỹ Sư Tuấn' },
    { id: 'p-2', batch: 'BATCH-20260814-01', param: 'Áp Suất Bắn Sơn Tĩnh Điện', target: 4.5, actual: 4.6, uom: 'Bar', ok: true, by: 'Kỹ Sư Tuấn' },
    { id: 'p-3', batch: 'BATCH-20260814-02', param: 'Độ Ẩm Gỗ Tự Nhiên', target: 12.0, actual: 14.8, uom: '%', ok: false, by: 'Kỹ Sư Hùng' },
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
              MFG - DEFECT QUARANTINE, YIELD REPORTING, BATCH PRODUCTION & PARAMETERS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Cách Ly Hàng Lỗi, Báo Cáo Tỷ Lệ Đạt QC, Lô/Mẻ Sản Xuất & Giám Sát Thông Số Kỹ Thuật</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Niêm phong cách ly hàng sai hỏng, thống kê chỉ số First-Pass Yield (FPY), quản lý lô/mẻ sản phẩm và nhật ký telemetry thông số chế tạo
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
            onClick={() => setActiveTab('quarantine')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'quarantine' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🛑 UC_MFG_035: Cách Ly Hàng Lỗi
          </button>
          <button
            onClick={() => setActiveTab('yield')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'yield' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📈 UC_MFG_036: Báo Cáo Tỷ Lệ Đạt QC
          </button>
          <button
            onClick={() => setActiveTab('batch')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'batch' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📦 UC_MFG_037: Quản Lý Lô/Mẻ SX
          </button>
          <button
            onClick={() => setActiveTab('params')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'params' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🎛️ UC_MFG_038: Thông Số Kỹ Thuật Mẻ
          </button>
        </div>
      </div>

      {activeTab === 'quarantine' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">🛑 Phát Lệnh Cách Ly Hàng Lỗi (UC_MFG_035)</h2>
            <form onSubmit={handleCreateQuarantine} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Lô Hàng (Lot No):</label>
                <input type="text" value={quarantineForm.lot} onChange={(e) => setQuarantineForm({ ...quarantineForm, lot: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Sản Phẩm / Vật Tư:</label>
                <input type="text" value={quarantineForm.item} onChange={(e) => setQuarantineForm({ ...quarantineForm, item: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Lượng Cần Niêm Phong Cách Ly:</label>
                <input type="number" value={quarantineForm.qty} onChange={(e) => setQuarantineForm({ ...quarantineForm, qty: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold text-rose-700" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Vị Trí Kho Cách Ly:</label>
                <input type="text" value={quarantineForm.loc} onChange={(e) => setQuarantineForm({ ...quarantineForm, loc: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Lý Do Khiếm Khuyết / Lỗi:</label>
                <textarea value={quarantineForm.defect} onChange={(e) => setQuarantineForm({ ...quarantineForm, defect: e.target.value })} rows={2} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-rose-600 text-white rounded-lg font-bold hover:bg-rose-700 shadow-sm mt-2">
                🛑 Khóa & Niêm Phong Hàng Cách Ly
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Danh Sách Hàng Hóa Đang Trong Khu Cách Ly</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Mã Lệnh Cách Ly</th>
                    <th className="p-3">Lô & Mặt Hàng</th>
                    <th className="p-3 text-center">SL Cách Ly</th>
                    <th className="p-3">Khu Vực Kho</th>
                    <th className="p-3">Dạng Lỗi</th>
                    <th className="p-3 text-right">Trạng Thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {quarantineList.map((q) => (
                    <tr key={q.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-rose-700">{q.holdNo}</td>
                      <td className="p-3">
                        <div className="font-mono font-bold text-foreground">{q.lot}</div>
                        <div className="text-xs text-muted-foreground">{q.item}</div>
                      </td>
                      <td className="p-3 text-center font-black text-rose-700">{q.qty} cái</td>
                      <td className="p-3 text-slate-700">{q.loc}</td>
                      <td className="p-3 text-xs text-muted-foreground">{q.defect}</td>
                      <td className="p-3 text-right">
                        <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-rose-100 text-rose-800 border border-rose-300">
                          ● Đang Cách Ly
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

      {activeTab === 'yield' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <h2 className="text-lg font-bold text-foreground">📈 Báo Cáo Tỷ Lệ Đạt Chuẩn & First-Pass Yield FPY (UC_MFG_036)</h2>
          <div className="grid grid-cols-4 gap-4">
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Tổng Lô Đã Kiểm Định</div>
              <div className="text-2xl font-black text-foreground mt-1">{yieldSummary.totalLots} Lô</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Tổng Sản Lượng Đạt (Pass)</div>
              <div className="text-2xl font-black text-emerald-700 mt-1">{yieldSummary.passedQty.toLocaleString('vi-VN')} SP</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">Tỷ Lệ Đạt Tổng Thể</div>
              <div className="text-2xl font-black text-brand mt-1">{formatYieldPercentage(yieldSummary.passRate)}</div>
            </div>
            <div className="p-4 rounded-xl border border-border bg-surface">
              <div className="text-xs text-muted-foreground font-semibold">First-Pass Yield (Đạt Lần 1)</div>
              <div className="text-2xl font-black text-blue-700 mt-1">{yieldSummary.fpyRate.toFixed(1)}% FPY</div>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'batch' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📦 Quản Lý Lô / Mẻ Sản Xuất & Hạn Sử Dụng (UC_MFG_037)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Số Mẻ (Batch No)</th>
                  <th className="p-3">Lệnh SX</th>
                  <th className="p-3">Sản Phẩm</th>
                  <th className="p-3 text-center">Sản Lượng (Thực Tế / KH)</th>
                  <th className="p-3">Ngày Sản Xuất</th>
                  <th className="p-3">Hạn Dùng / Bảo Hành</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {batches.map((b) => (
                  <tr key={b.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{b.batchNo}</td>
                    <td className="p-3 font-mono font-bold text-foreground">{b.wo}</td>
                    <td className="p-3 font-semibold text-slate-800">{b.sku}</td>
                    <td className="p-3 text-center font-extrabold text-foreground">{formatBatchQuantity(b.actual, b.planned)}</td>
                    <td className="p-3 text-slate-700">{b.mfgDate}</td>
                    <td className="p-3 text-slate-700">{b.expDate}</td>
                    <td className="p-3 text-right">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${b.status === 'Completed' ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-blue-100 text-blue-800 border-blue-300'}`}>
                        ● {b.status === 'Completed' ? 'Hoàn Thành' : 'Đang Sản Xuất'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'params' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🎛️ Nhật Ký Ghi Nhận Thông Số Kỹ Thuật Mẻ (UC_MFG_038)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Mẻ</th>
                  <th className="p-3">Tên Thông Số Kỹ Thuật</th>
                  <th className="p-3 text-center">Giá Trị Chuẩn</th>
                  <th className="p-3 text-center">Giá Trị Đo Thực Tế</th>
                  <th className="p-3 text-center">Đánh Giá Dung Sai</th>
                  <th className="p-3">Kỹ Sư Trực Ca</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {paramList.map((p) => (
                  <tr key={p.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{p.batch}</td>
                    <td className="p-3 font-semibold text-foreground">{p.param}</td>
                    <td className="p-3 text-center font-bold text-slate-700">{p.target} {p.uom}</td>
                    <td className="p-3 text-center font-extrabold text-foreground">{p.actual} {p.uom}</td>
                    <td className="p-3 text-center">
                      <span className={`px-2.5 py-1 text-xs font-black rounded-full border ${p.ok ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-rose-100 text-rose-800 border-rose-300'}`}>
                        {p.ok ? '✓ Chuẩn Dung Sai' : '⚠️ Vượt Dung Sai Cho Phép'}
                      </span>
                    </td>
                    <td className="p-3 text-slate-700">{p.by}</td>
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
