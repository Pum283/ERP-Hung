'use client';

import React, { useState } from 'react';
import {
  formatVariancePercentage,
  getQcResultBadge,
} from '@/shared/api/mfg-cost-variance-qc-inspection-helpers';

export default function MfgCostVarianceQcInspectionPage() {
  const [activeTab, setActiveTab] = useState<'variance' | 'iqc' | 'fqc' | 'disposition'>('variance');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_MFG_030: Đối chiếu lý thuyết vs thực tế
  const [varianceList, setVarianceList] = useState([
    { id: 'v-1', no: 'VAR-COST-001', wo: 'WO-2026-088', std: 10000000, act: 11500000, diff: 1500000, pct: 15.0, cause: 'Hao hụt thép tấm và chi phí chạy ngoài giờ ca 3' },
  ]);

  const [varianceForm, setVarianceForm] = useState({
    wo: 'WO-2026-089',
    std: 8500000,
    act: 8900000,
    cause: 'Giá sơn tĩnh điện gia công ngoài tăng 5%',
  });

  const handleAnalyzeVariance = (e: React.FormEvent) => {
    e.preventDefault();
    const diff = varianceForm.act - varianceForm.std;
    const pct = (diff / varianceForm.std) * 100;
    const newEntry = {
      id: 'v-' + Date.now(),
      no: 'VAR-COST-' + Math.floor(1000 + Math.random() * 9000),
      wo: varianceForm.wo,
      std: varianceForm.std,
      act: varianceForm.act,
      diff,
      pct,
      cause: varianceForm.cause,
    };
    setVarianceList([...varianceList, newEntry]);
    showToast(`✓ Đã phân tích độ lệch giá thành lệnh [${varianceForm.wo}]: Chênh lệch ${diff.toLocaleString('vi-VN')} đ (${formatVariancePercentage(pct)})!`, 'success');
  };

  // UC_MFG_032: Tiêu chí QC đầu vào (IQC)
  const [criteria, setCriteria] = useState([
    { id: 'c-1', code: 'QC-STEEL-THICK', name: 'Độ Dày Thép Tấm 2mm', group: 'Kim Loại Tấm', spec: '2.0mm ± 0.05mm', method: 'Thước Panme điện tử', mandatory: true },
    { id: 'c-2', code: 'QC-PAINT-ADHESION', name: 'Độ Bám Dính Bề Mặt Sơn', group: 'Sơn Tĩnh Điện', spec: 'TCVN 2097:1993', method: 'Dao cắt ô cờ', mandatory: true },
  ]);

  // UC_MFG_033: QC thành phẩm (FQC)
  const [fqcList, setFqcList] = useState([
    { id: 'f-1', no: 'FQC-20260814-001', wo: 'WO-2026-088', sku: 'FG-SERVER-42U', sample: 20, defect: 0, result: 'Pass', inspector: 'Kỹ Sư An' },
    { id: 'f-2', no: 'FQC-20260814-002', wo: 'WO-2026-089', sku: 'FG-DESK-WOOD', sample: 15, defect: 2, result: 'Fail', inspector: 'Kỹ Sư Bình' },
  ]);

  // UC_MFG_034: Ghi nhận lô đạt / không đạt
  const [dispositionList, setDispositionList] = useState([
    { id: 'd-1', lot: 'LOT-2026-0814-A', item: 'FG-SERVER-42U', total: 100, ok: 100, ng: 0, decision: 'ReleaseToStock', note: 'Lô đạt 100% tiêu chuẩn xuất xưởng' },
    { id: 'd-2', lot: 'LOT-2026-0814-B', item: 'FG-DESK-WOOD', total: 50, ok: 48, ng: 2, decision: 'Quarantine', note: '2 sản phẩm chuyển sang lệnh sản xuất lại sửa bề mặt' },
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
              MFG - COST VARIANCE ANALYSIS & QUALITY CONTROL (IQC / FQC)
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Đối Chiếu Giá Thành Lý Thuyết vs Thực Tế & Kiểm Soát Chất Lượng QC</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Phân tích phương sai chi phí sản xuất, thiết lập tiêu chí IQC đầu vào, kiểm tra FQC thành phẩm và ra quyết định xử lý lô kiểm định
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
            onClick={() => setActiveTab('variance')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'variance' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📊 UC_MFG_030: Đối Chiếu Giá Thành
          </button>
          <button
            onClick={() => setActiveTab('iqc')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'iqc' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📥 UC_MFG_032: Tiêu Chí QC Đầu Vào
          </button>
          <button
            onClick={() => setActiveTab('fqc')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'fqc' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ✅ UC_MFG_033: QC Thành Phẩm (FQC)
          </button>
          <button
            onClick={() => setActiveTab('disposition')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'disposition' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏷️ UC_MFG_034: Xử Lý Lô Đạt / Không Đạt
          </button>
        </div>
      </div>

      {activeTab === 'variance' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📊 Phân Tích Độ Lệch Chi Phí (UC_MFG_030)</h2>
            <form onSubmit={handleAnalyzeVariance} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Lệnh SX (WO):</label>
                <input type="text" value={varianceForm.wo} onChange={(e) => setVarianceForm({ ...varianceForm, wo: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Chi Phí Định Mức Lý Thuyết (VNĐ):</label>
                <input type="number" value={varianceForm.std} onChange={(e) => setVarianceForm({ ...varianceForm, std: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Chi Phí Thực Tế Phát Sinh (VNĐ):</label>
                <input type="number" value={varianceForm.act} onChange={(e) => setVarianceForm({ ...varianceForm, act: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Nguyên Nhân Chênh Lệch:</label>
                <textarea value={varianceForm.cause} onChange={(e) => setVarianceForm({ ...varianceForm, cause: e.target.value })} rows={3} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                🧮 Đối Chiếu & Báo Cáo Phương Sai
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Bảng Phân Tích Phương Sai Chi Phí Sản Xuất</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Lệnh SX</th>
                    <th className="p-3 text-right">Định Mức (VNĐ)</th>
                    <th className="p-3 text-right">Thực Tế (VNĐ)</th>
                    <th className="p-3 text-right">Chênh Lệch</th>
                    <th className="p-3 text-center">Tỷ Lệ (%)</th>
                    <th className="p-3">Nguyên Nhân Gốc</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {varianceList.map((v) => (
                    <tr key={v.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-brand">{v.wo}</td>
                      <td className="p-3 text-right text-slate-700">{v.std.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-right font-extrabold text-foreground">{v.act.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-right font-black text-rose-700">{v.diff > 0 ? `+${v.diff.toLocaleString('vi-VN')}` : v.diff.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-center">
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-rose-100 text-rose-800 border border-rose-300">
                          {formatVariancePercentage(v.pct)}
                        </span>
                      </td>
                      <td className="p-3 text-xs text-muted-foreground">{v.cause}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'iqc' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📥 Danh Mục Tiêu Chí Kiểm Soát Chất Lượng Đầu Vào IQC (UC_MFG_032)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã & Tên Tiêu Chí</th>
                  <th className="p-3">Nhóm Vật Tư</th>
                  <th className="p-3">Quy Chuẩn Kỹ Thuật</th>
                  <th className="p-3">Phương Pháp Kiểm Tra</th>
                  <th className="p-3 text-right">Bắt Buộc?</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {criteria.map((c) => (
                  <tr key={c.id} className="hover:bg-surface-hover/50">
                    <td className="p-3">
                      <div className="font-mono font-bold text-brand">{c.code}</div>
                      <div className="text-xs text-foreground font-semibold">{c.name}</div>
                    </td>
                    <td className="p-3 font-semibold text-slate-700">{c.group}</td>
                    <td className="p-3 font-mono text-slate-800">{c.spec}</td>
                    <td className="p-3 text-slate-600">{c.method}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Bắt Buộc 100%
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'fqc' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">✅ Phiếu Kiểm Nghiệm Chất Lượng Thành Phẩm FQC (UC_MFG_033)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Phiếu FQC</th>
                  <th className="p-3">Lệnh SX</th>
                  <th className="p-3">Mã Thành Phẩm</th>
                  <th className="p-3 text-center">Cỡ Mẫu (Sample)</th>
                  <th className="p-3 text-center">Lỗi Phát Hiện</th>
                  <th className="p-3 text-center">Kết Luận</th>
                  <th className="p-3">Kỹ Sư Kiểm Định</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {fqcList.map((f) => {
                  const badge = getQcResultBadge(f.result);
                  return (
                    <tr key={f.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-brand">{f.no}</td>
                      <td className="p-3 font-mono font-bold text-foreground">{f.wo}</td>
                      <td className="p-3 font-semibold text-slate-800">{f.sku}</td>
                      <td className="p-3 text-center font-extrabold text-foreground">{f.sample} cái</td>
                      <td className="p-3 text-center font-extrabold text-rose-700">{f.defect} cái</td>
                      <td className="p-3 text-center">
                        <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${badge.colorClass}`}>
                          ● {badge.label}
                        </span>
                      </td>
                      <td className="p-3 text-slate-700">{f.inspector}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'disposition' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🏷️ Quyết Định Xử Lý & Giải Phóng Lô Hàng (UC_MFG_034)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Số Lô</th>
                  <th className="p-3">Mặt Hàng</th>
                  <th className="p-3 text-center">Tổng SL Lô</th>
                  <th className="p-3 text-center">SL Đạt Nhập Kho</th>
                  <th className="p-3 text-center">SL Từ Chối / Lỗi</th>
                  <th className="p-3">Quyết Định Xử Lý</th>
                  <th className="p-3">Ghi Chú Trưởng Bộ Phận QC</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {dispositionList.map((d) => (
                  <tr key={d.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{d.lot}</td>
                    <td className="p-3 font-semibold text-foreground">{d.item}</td>
                    <td className="p-3 text-center font-bold text-slate-800">{d.total}</td>
                    <td className="p-3 text-center font-extrabold text-emerald-700">{d.ok}</td>
                    <td className="p-3 text-center font-extrabold text-rose-700">{d.ng}</td>
                    <td className="p-3 font-bold text-slate-800">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${d.decision === 'ReleaseToStock' ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-amber-100 text-amber-800 border-amber-300'}`}>
                        {d.decision === 'ReleaseToStock' ? '✓ Nhập Kho Thành Phẩm' : '⚠️ Cách Ly Chờ Xử Lý'}
                      </span>
                    </td>
                    <td className="p-3 text-xs text-muted-foreground">{d.note}</td>
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
