'use client';

import React, { useState } from 'react';
import {
  formatCycleTime,
  formatEfficiencyPercentage,
} from '@/shared/api/mfg-routing-stage-shift-capacity-helpers';

export default function MfgRoutingStageShiftCapacityPage() {
  const [activeTab, setActiveTab] = useState<'stages' | 'capacity'>('stages');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_MFG_004: Danh mục công đoạn
  const [stages, setStages] = useState([
    { id: 's-1', code: 'OP-10-CUT', name: 'Cắt Phôi Kim Loại CNC', wc: 'WC-CNC-01', cycle: 12, setup: 20, outsource: false },
    { id: 's-2', code: 'OP-20-WELD', name: 'Hàn Khung Định Hình', wc: 'WC-WELD-02', cycle: 25, setup: 15, outsource: false },
    { id: 's-3', code: 'OP-30-PAINT', name: 'Sơn Tĩnh Điện Bề Mặt', wc: 'WC-PAINT-OUT', cycle: 45, setup: 60, outsource: true },
    { id: 's-4', code: 'OP-40-ASSEMBLE', name: 'Lắp Ráp Thành Phẩm', wc: 'WC-ASSY-03', cycle: 30, setup: 10, outsource: false },
  ]);

  const [stageForm, setStageForm] = useState({
    code: '',
    name: '',
    wc: 'WC-CNC-01',
    cycle: 15,
    setup: 20,
    outsource: false,
  });

  const handleAddStage = (e: React.FormEvent) => {
    e.preventDefault();
    if (!stageForm.code || !stageForm.name) {
      showToast('Vui lòng điền mã và tên công đoạn!', 'error');
      return;
    }
    const newStage = {
      id: 's-' + Date.now(),
      ...stageForm,
    };
    setStages([...stages, newStage]);
    setStageForm({ code: '', name: '', wc: 'WC-CNC-01', cycle: 15, setup: 20, outsource: false });
    showToast(`✓ Đã thêm công đoạn sản xuất [${newStage.name}] vào quy trình Routing!`, 'success');
  };

  // UC_MFG_005: Ca sản xuất / năng lực
  const [capacities] = useState([
    { id: 'c-1', shift: 'MFG-SHIFT-1', name: 'Ca Sáng (06:00 - 14:00)', wc: 'WC-CNC-01', hours: 8, efficiency: 90, max: 450 },
    { id: 'c-2', shift: 'MFG-SHIFT-2', name: 'Ca Chiều (14:00 - 22:00)', wc: 'WC-CNC-01', hours: 8, efficiency: 85, max: 420 },
    { id: 'c-3', shift: 'MFG-SHIFT-3', name: 'Ca Đêm (22:00 - 06:00)', wc: 'WC-ASSY-03', hours: 8, efficiency: 75, max: 350 },
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
              MFG - ROUTING STAGES & PRODUCTION SHIFT CAPACITY
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Danh Mục Công Đoạn Sản Xuất & Năng Lực Ca / Máy (UC_MFG_004 & 005)</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Định nghĩa quy trình công nghệ (Routing), thời gian chu kỳ tiêu chuẩn, cấu hình ca làm việc và công suất trần của từng xưởng sản xuất
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (2/2 UCs MFG)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('stages')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'stages' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚙️ UC_MFG_004: Danh Mục Công Đoạn
          </button>
          <button
            onClick={() => setActiveTab('capacity')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'capacity' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏭 UC_MFG_005: Ca SX & Năng Lực Máy
          </button>
        </div>
      </div>

      {activeTab === 'stages' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">➕ Thêm Công Đoạn Routing Mới</h2>
            <form onSubmit={handleAddStage} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Công Đoạn (Operation Code):</label>
                <input type="text" value={stageForm.code} onChange={(e) => setStageForm({ ...stageForm, code: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên Công Đoạn:</label>
                <input type="text" value={stageForm.name} onChange={(e) => setStageForm({ ...stageForm, name: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Trung Tâm Sản Xuất (Work Center):</label>
                <select value={stageForm.wc} onChange={(e) => setStageForm({ ...stageForm, wc: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold">
                  <option value="WC-CNC-01">WC-CNC-01 (Xưởng Gia Công Cơ Khí)</option>
                  <option value="WC-WELD-02">WC-WELD-02 (Xưởng Hàn & Khung)</option>
                  <option value="WC-PAINT-OUT">WC-PAINT-OUT (Gia Công Sơn Ngoài)</option>
                  <option value="WC-ASSY-03">WC-ASSY-03 (Chuyền Lắp Ráp)</option>
                </select>
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="block text-foreground font-medium mb-1">Cycle Time (Phút):</label>
                  <input type="number" value={stageForm.cycle} onChange={(e) => setStageForm({ ...stageForm, cycle: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">Setup Time (Phút):</label>
                  <input type="number" value={stageForm.setup} onChange={(e) => setStageForm({ ...stageForm, setup: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
                </div>
              </div>
              <div className="flex items-center gap-2 pt-1">
                <input type="checkbox" id="outsource" checked={stageForm.outsource} onChange={(e) => setStageForm({ ...stageForm, outsource: e.target.checked })} className="rounded text-brand" />
                <label htmlFor="outsource" className="text-foreground font-medium text-xs">Gia công ngoài (Outsourced)</label>
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                💾 Lưu Công Đoạn Sản Xuất
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">⚙️ Quy Trình Công Đoạn Tiêu Chuẩn (Routing Stages)</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Mã & Tên Công Đoạn</th>
                    <th className="p-3">Trung Tâm Sản Xuất</th>
                    <th className="p-3 text-center">Cycle Time</th>
                    <th className="p-3 text-center">Setup Time</th>
                    <th className="p-3 text-right">Hình Thức</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {stages.map((s) => (
                    <tr key={s.id} className="hover:bg-surface-hover/50">
                      <td className="p-3">
                        <div className="font-mono font-bold text-foreground">{s.code}</div>
                        <div className="text-xs text-muted-foreground font-semibold">{s.name}</div>
                      </td>
                      <td className="p-3 font-semibold text-slate-700">{s.wc}</td>
                      <td className="p-3 text-center font-bold text-brand">{formatCycleTime(s.cycle)}</td>
                      <td className="p-3 text-center font-bold text-slate-700">{s.setup} phút</td>
                      <td className="p-3 text-right">
                        <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${s.outsource ? 'bg-amber-100 text-amber-800 border-amber-300' : 'bg-emerald-100 text-emerald-800 border-emerald-300'}`}>
                          {s.outsource ? 'Gia Công Ngoài' : 'Nội Bộ (In-house)'}
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

      {activeTab === 'capacity' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🏭 Cấu Hình Ca Sản Xuất & Năng Lực Công Suất (UC_MFG_005)</h2>
          <div className="grid grid-cols-3 gap-4">
            {capacities.map((c) => (
              <div key={c.id} className="p-5 rounded-xl border border-border bg-surface shadow-sm space-y-3">
                <div className="flex justify-between items-start">
                  <div>
                    <span className="text-xs font-mono font-bold text-brand">{c.shift}</span>
                    <h3 className="text-base font-bold text-foreground mt-0.5">{c.name}</h3>
                  </div>
                  <span className="px-2.5 py-1 bg-emerald-100 text-emerald-800 text-xs font-bold rounded-full border border-emerald-300">
                    ● Hoạt Động
                  </span>
                </div>
                <div className="text-xs text-muted-foreground font-semibold">Trung tâm máy: {c.wc}</div>
                <div className="flex justify-between items-center text-sm pt-2 border-t border-border">
                  <span className="text-muted-foreground">Thời gian ca:</span>
                  <span className="font-bold text-foreground">{c.hours} Giờ / Ca</span>
                </div>
                <div className="flex justify-between items-center text-sm">
                  <span className="text-muted-foreground">Hiệu suất OEE:</span>
                  <span className="font-extrabold text-blue-700">{formatEfficiencyPercentage(c.efficiency)}</span>
                </div>
                <div className="flex justify-between items-center text-sm">
                  <span className="text-muted-foreground">Công suất trần:</span>
                  <span className="font-black text-brand">{c.max} SP / Ca</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
