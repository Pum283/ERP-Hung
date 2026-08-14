'use client';

import React, { useState } from 'react';
import {
  formatDefectRate,
  formatUnitCost,
} from '@/shared/api/mfg-schedule-progress-rework-cost-helpers';

export default function MfgScheduleProgressReworkCostPage() {
  const [activeTab, setActiveTab] = useState<'sched' | 'progress' | 'rework' | 'cost'>('sched');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_MFG_016: Lịch SX theo xưởng/ca
  const [schedules] = useState([
    { id: 'sc-1', no: 'SCHED-20260814-001', workshop: 'Xưởng Cơ Khí 1', shift: 'Ca Sáng (06:00-14:00)', wo: 'WO-2026-088', target: 250, status: 'Running' },
    { id: 'sc-2', no: 'SCHED-20260814-002', workshop: 'Xưởng Lắp Ráp 2', shift: 'Ca Chiều (14:00-22:00)', wo: 'WO-2026-089', target: 180, status: 'Scheduled' },
  ]);

  // UC_MFG_021: Ghi nhận tiến độ công đoạn
  const [progressList, setProgressList] = useState([
    { id: 'p-1', wo: 'WO-2026-088', opCode: 'OP-10-CUT', opName: 'Cắt Phôi CNC', good: 245, bad: 5, operator: 'Trần Văn Kỹ' },
    { id: 'p-2', wo: 'WO-2026-088', opCode: 'OP-20-WELD', opName: 'Hàn Khung', good: 240, bad: 5, operator: 'Nguyễn Văn Hàn' },
  ]);

  const [progressForm, setProgressForm] = useState({
    wo: 'WO-2026-088',
    opCode: 'OP-40-ASSY',
    opName: 'Lắp Ráp Hoàn Thiện',
    good: 238,
    bad: 2,
    operator: 'Lê Văn Lắp',
  });

  const handleLogProgress = (e: React.FormEvent) => {
    e.preventDefault();
    const newP = {
      id: 'p-' + Date.now(),
      ...progressForm,
    };
    setProgressList([...progressList, newP]);
    showToast(`✓ Đã ghi nhận tiến độ công đoạn [${progressForm.opName}]: Đạt ${progressForm.good}, Lỗi ${progressForm.bad}!`, 'success');
  };

  // UC_MFG_026: Lệnh sản xuất lại
  const [reworkList, setReworkList] = useState([
    { id: 'rw-1', rwNo: 'WO-REWORK-2026-001', origWo: 'WO-2026-088', reason: 'Mối hàn khung bị xỉ hàn, cần chà nhám hàn đắp lại', qty: 5, ws: 'WC-REWORK-01', status: 'Approved' },
  ]);

  const [reworkForm, setReworkForm] = useState({
    origWo: 'WO-2026-088',
    reason: 'Sơn bề mặt bị bong tróc cần sơn lại lớp 2',
    qty: 3,
    ws: 'WC-PAINT-OUT',
  });

  const handleCreateRework = (e: React.FormEvent) => {
    e.preventDefault();
    const newRw = {
      id: 'rw-' + Date.now(),
      rwNo: 'WO-REWORK-' + Math.floor(1000 + Math.random() * 9000),
      origWo: reworkForm.origWo,
      reason: reworkForm.reason,
      qty: reworkForm.qty,
      ws: reworkForm.ws,
      status: 'Approved',
    };
    setReworkList([...reworkList, newRw]);
    showToast(`✓ Đã tạo lệnh sản xuất lại / tái chế [${newRw.rwNo}] cho ${reworkForm.qty} sản phẩm lỗi!`, 'success');
  };

  // UC_MFG_028: Phân bổ nhân công / chi phí chung
  const [costForm, setCostForm] = useState({
    wo: 'WO-2026-088',
    labor: 5000000,
    deprec: 2000000,
    overhead: 3000000,
    qty: 100,
  });

  const [costList, setCostList] = useState([
    { id: 'c-1', allocNo: 'MFG-COST-2026-001', wo: 'WO-2026-088', labor: 5000000, deprec: 2000000, overhead: 3000000, total: 10000000, qty: 100, unit: 100000 },
  ]);

  const handleAllocateCost = (e: React.FormEvent) => {
    e.preventDefault();
    const total = costForm.labor + costForm.deprec + costForm.overhead;
    const unit = total / (costForm.qty > 0 ? costForm.qty : 1);
    const newCost = {
      id: 'c-' + Date.now(),
      allocNo: 'MFG-COST-' + Math.floor(1000 + Math.random() * 9000),
      wo: costForm.wo,
      labor: costForm.labor,
      deprec: costForm.deprec,
      overhead: costForm.overhead,
      total,
      qty: costForm.qty,
      unit,
    };
    setCostList([newCost, ...costList]);
    showToast(`✓ Đã phân bổ chi phí sản xuất cho lệnh [${costForm.wo}]: Đơn giá ${unit.toLocaleString('vi-VN')} đ/SP!`, 'success');
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
              MFG - SHOP FLOOR SCHEDULING, WIP TRACKING, REWORK & OVERHEAD ALLOCATION
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Lịch Sản Xuất Xưởng/Ca, Tiến Độ Công Đoạn WIP, Lệnh Tái Chế & Phân Bổ Chi Phí</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Điều độ sản xuất từng phân xưởng, ghi nhận sản lượng đạt/lỗi theo công đoạn, phát lệnh tái chế và tính toán giá thành đơn vị
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
            onClick={() => setActiveTab('sched')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'sched' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📅 UC_MFG_016: Lịch SX Xưởng/Ca
          </button>
          <button
            onClick={() => setActiveTab('progress')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'progress' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚡ UC_MFG_021: Tiến Độ Công Đoạn WIP
          </button>
          <button
            onClick={() => setActiveTab('rework')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'rework' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔄 UC_MFG_026: Lệnh Sản Xuất Lại
          </button>
          <button
            onClick={() => setActiveTab('cost')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'cost' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💰 UC_MFG_028: Phân Bổ Chi Phí & Nhân Công
          </button>
        </div>
      </div>

      {activeTab === 'sched' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📅 Lịch Điều Độ Sản Xuất Chi Tiết Theo Xưởng & Ca (UC_MFG_016)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Lịch SX</th>
                  <th className="p-3">Phân Xưởng</th>
                  <th className="p-3">Ca Làm Việc</th>
                  <th className="p-3">Lệnh Sản Xuất (WO)</th>
                  <th className="p-3 text-center">Chỉ Tiêu Sản Lượng</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {schedules.map((s) => (
                  <tr key={s.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{s.no}</td>
                    <td className="p-3 font-semibold text-foreground">{s.workshop}</td>
                    <td className="p-3 text-slate-700">{s.shift}</td>
                    <td className="p-3 font-mono font-bold text-foreground">{s.wo}</td>
                    <td className="p-3 text-center font-extrabold text-foreground">{s.target} SP</td>
                    <td className="p-3 text-right">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${s.status === 'Running' ? 'bg-blue-100 text-blue-800 border-blue-300' : 'bg-amber-100 text-amber-800 border-amber-300'}`}>
                        ● {s.status === 'Running' ? 'Đang Chạy' : 'Đã Lên Lịch'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'progress' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">⚡ Ghi Nhận Tiến Độ WIP (UC_MFG_021)</h2>
            <form onSubmit={handleLogProgress} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Lệnh SX (WO):</label>
                <input type="text" value={progressForm.wo} onChange={(e) => setProgressForm({ ...progressForm, wo: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Mã & Tên Công Đoạn:</label>
                <div className="grid grid-cols-2 gap-2">
                  <input type="text" value={progressForm.opCode} onChange={(e) => setProgressForm({ ...progressForm, opCode: e.target.value })} className="border border-border rounded-lg p-2 bg-surface text-foreground font-mono" />
                  <input type="text" value={progressForm.opName} onChange={(e) => setProgressForm({ ...progressForm, opName: e.target.value })} className="border border-border rounded-lg p-2 bg-surface text-foreground" />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="block text-foreground font-medium mb-1">SL Đạt (Good):</label>
                  <input type="number" value={progressForm.good} onChange={(e) => setProgressForm({ ...progressForm, good: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold text-emerald-700" />
                </div>
                <div>
                  <label className="block text-foreground font-medium mb-1">SL Hỏng / Lỗi (Bad):</label>
                  <input type="number" value={progressForm.bad} onChange={(e) => setProgressForm({ ...progressForm, bad: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold text-rose-700" />
                </div>
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Công Nhân / Kỹ Thuật Viên:</label>
                <input type="text" value={progressForm.operator} onChange={(e) => setProgressForm({ ...progressForm, operator: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                ⚡ Cập Nhật Tiến Độ & Kiểm Lỗi
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Nhật Ký Báo Cáo Sản Lượng Theo Từng Công Đoạn</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Lệnh SX</th>
                    <th className="p-3">Công Đoạn</th>
                    <th className="p-3 text-center">SL Đạt</th>
                    <th className="p-3 text-center">SL Hỏng</th>
                    <th className="p-3 text-center">Tỷ Lệ Lỗi</th>
                    <th className="p-3">Người Vận Hành</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {progressList.map((p) => (
                    <tr key={p.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-foreground">{p.wo}</td>
                      <td className="p-3">
                        <div className="font-mono font-bold text-foreground">{p.opCode}</div>
                        <div className="text-xs text-muted-foreground">{p.opName}</div>
                      </td>
                      <td className="p-3 text-center font-extrabold text-emerald-700">{p.good}</td>
                      <td className="p-3 text-center font-extrabold text-rose-700">{p.bad}</td>
                      <td className="p-3 text-center font-bold text-slate-700">{formatDefectRate(p.good, p.bad)}</td>
                      <td className="p-3 text-slate-700">{p.operator}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'rework' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">🔄 Lập Lệnh Tái Chế / Sản Xuất Lại (UC_MFG_026)</h2>
            <form onSubmit={handleCreateRework} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Lệnh Gốc (Original WO):</label>
                <input type="text" value={reworkForm.origWo} onChange={(e) => setReworkForm({ ...reworkForm, origWo: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Lượng Cần Xử Lý Lại:</label>
                <input type="number" value={reworkForm.qty} onChange={(e) => setReworkForm({ ...reworkForm, qty: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Xưởng Giao Xử Lý Lỗi:</label>
                <input type="text" value={reworkForm.ws} onChange={(e) => setReworkForm({ ...reworkForm, ws: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Nguyên Nhân Lỗi Kỹ Thuật:</label>
                <textarea value={reworkForm.reason} onChange={(e) => setReworkForm({ ...reworkForm, reason: e.target.value })} rows={3} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                🔄 Phát Lệnh Tái Chế (WO-REWORK)
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Danh Sách Lệnh Sản Xuất Lại</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Mã Lệnh Tái Chế</th>
                    <th className="p-3">Lệnh Gốc</th>
                    <th className="p-3 text-center">SL Lỗi</th>
                    <th className="p-3">Xưởng Phụ Trách</th>
                    <th className="p-3">Lý Do / Khiếm Khuyết</th>
                    <th className="p-3 text-right">Trạng Thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {reworkList.map((r) => (
                    <tr key={r.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-brand">{r.rwNo}</td>
                      <td className="p-3 font-mono font-bold text-foreground">{r.origWo}</td>
                      <td className="p-3 text-center font-black text-rose-700">{r.qty} cái</td>
                      <td className="p-3 font-semibold text-slate-700">{r.ws}</td>
                      <td className="p-3 text-xs text-muted-foreground">{r.reason}</td>
                      <td className="p-3 text-right">
                        <span className="px-2.5 py-1 text-xs font-bold rounded-full border bg-emerald-100 text-emerald-800 border-emerald-300">
                          ● {r.status}
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

      {activeTab === 'cost' && (
        <div className="grid grid-cols-3 gap-6">
          <div className="col-span-1 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">💰 Phân Bổ Chi Phí Sản Xuất (UC_MFG_028)</h2>
            <form onSubmit={handleAllocateCost} className="space-y-3 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Lệnh SX (WO):</label>
                <input type="text" value={costForm.wo} onChange={(e) => setCostForm({ ...costForm, wo: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Nhân Công Trực Tiếp (VNĐ):</label>
                <input type="number" value={costForm.labor} onChange={(e) => setCostForm({ ...costForm, labor: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Khấu Hao Máy Móc / Thiết Bị (VNĐ):</label>
                <input type="number" value={costForm.deprec} onChange={(e) => setCostForm({ ...costForm, deprec: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Chi Phí Chung / Điện Nước Phân Xưởng (VNĐ):</label>
                <input type="number" value={costForm.overhead} onChange={(e) => setCostForm({ ...costForm, overhead: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Sản Lượng Hoàn Thành (SP):</label>
                <input type="number" value={costForm.qty} onChange={(e) => setCostForm({ ...costForm, qty: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>

              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm mt-2">
                🧮 Tính & Phân Bổ Giá Thành
              </button>
            </form>
          </div>

          <div className="col-span-2 bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
            <h2 className="text-lg font-bold text-foreground">📋 Lịch Sử Phân Bổ Giá Thành Sản Phẩm</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                    <th className="p-3">Mã Phân Bổ</th>
                    <th className="p-3">Lệnh SX</th>
                    <th className="p-3 text-right">Tổng Chi Phí (VNĐ)</th>
                    <th className="p-3 text-center">Sản Lượng</th>
                    <th className="p-3 text-right">Đơn Giá Thành (VNĐ/SP)</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {costList.map((c) => (
                    <tr key={c.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-brand">{c.allocNo}</td>
                      <td className="p-3 font-mono font-bold text-foreground">{c.wo}</td>
                      <td className="p-3 text-right font-extrabold text-foreground">{c.total.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-center font-bold text-slate-700">{c.qty} SP</td>
                      <td className="p-3 text-right font-black text-emerald-800">{formatUnitCost(c.unit)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
