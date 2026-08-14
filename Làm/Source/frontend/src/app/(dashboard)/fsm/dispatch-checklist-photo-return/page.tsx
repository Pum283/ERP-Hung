'use client';

import React, { useState } from 'react';
import {
  formatPhotoTypeLabel,
  formatReturnedPartQuantity,
} from '@/shared/api/fsm-dispatch-checklist-photo-return-helpers';

export default function FsmDispatchChecklistPhotoReturnPage() {
  const [activeTab, setActiveTab] = useState<'rules' | 'checklist' | 'photos' | 'returns'>('rules');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FSM_016: Phân công theo rule
  const [rules, setRules] = useState([
    { id: 'r-1', name: 'Phân công KTV HVAC TP.HCM', territory: 'REGION-SOUTH-01', skill: 'SKILL-HVAC', max: 5, auto: true },
    { id: 'r-2', name: 'Phân công KTV PLC Hà Nội', territory: 'REGION-NORTH-01', skill: 'SKILL-ELEC-PLC', max: 4, auto: true },
  ]);

  // UC_FSM_021: Checklist công việc
  const [checklist, setChecklist] = useState([
    { id: 'c-1', ticket: 'TCK-2026-0814-01', step: '1. Kiểm tra an toàn ngắt nguồn điện', mandatory: true, done: true, by: 'Trần Minh Hùng', at: '10:00' },
    { id: 'c-2', ticket: 'TCK-2026-0814-01', step: '2. Đo đạc thông số điện áp và tụ khởi động', mandatory: true, done: true, by: 'Trần Minh Hùng', at: '10:15' },
    { id: 'c-3', ticket: 'TCK-2026-0814-01', step: '3. Chạy thử máy không tải 15 phút và bàn giao', mandatory: true, done: false, by: '', at: '' },
  ]);

  const toggleChecklist = (id: string) => {
    setChecklist(checklist.map(c => c.id === id ? { ...c, done: !c.done, by: !c.done ? 'Kỹ Thuật Viên' : '', at: !c.done ? '10:30' : '' } : c));
    showToast('✓ Đã cập nhật trạng thái bước thực hiện checklist!', 'success');
  };

  // UC_FSM_023: Chụp ảnh trước/sau
  const [photos, setPhotos] = useState([
    { id: 'p-1', ticket: 'TCK-2026-0814-01', type: 'Before', url: '/uploads/photos/tck-before.jpg', caption: 'Hiện trạng quạt tản nhiệt bị kẹt bụi', time: '09:45' },
    { id: 'p-2', ticket: 'TCK-2026-0814-01', type: 'After', url: '/uploads/photos/tck-after.jpg', caption: 'Đã thay quạt mới và làm sạch cụm tản nhiệt', time: '10:25' },
  ]);

  // UC_FSM_025: Hoàn linh kiện thừa
  const [returns, setReturns] = useState([
    { id: 'ret-1', slip: 'RET-PART-20260814-01', ticket: 'TCK-2026-0814-01', part: 'PART-CAPACITOR-50UF', name: 'Tụ Điện Khởi Động 50uF', qty: 1, wh: 'KHO-LINH-KIEN-FSM', status: 'Received' },
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
              FSM - AUTO DISPATCH, JOB CHECKLIST, EVIDENCE PHOTOS & PARTS RETURN
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Phân Công Theo Quy Tắc, Checklist Hiện Trường, Chụp Ảnh Minh Chứng & Hoàn Trả Linh Kiện Thừa</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Tự động điều phối KTV theo chuyên môn & tải công việc, số hóa checklist từng bước, lưu trữ ảnh trước/sau sửa chữa và nhập kho linh kiện dư
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs FSM)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('rules')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'rules' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🤖 UC_FSM_016: Phân Công Theo Rule
          </button>
          <button
            onClick={() => setActiveTab('checklist')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'checklist' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📋 UC_FSM_021: Checklist Công Việc
          </button>
          <button
            onClick={() => setActiveTab('photos')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'photos' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📸 UC_FSM_023: Chụp Ảnh Trước / Sau
          </button>
          <button
            onClick={() => setActiveTab('returns')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'returns' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📦 UC_FSM_025: Hoàn Linh Kiện Thừa
          </button>
        </div>
      </div>

      {activeTab === 'rules' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🤖 Quy Tắc Tự Động Phân Công Kỹ Thuật Viên (UC_FSM_016)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Tên Quy Tắc Điều Phối</th>
                  <th className="p-3">Khu Vực Phụ Trách</th>
                  <th className="p-3">Kỹ Năng Yêu Cầu</th>
                  <th className="p-3 text-center">Tải Tối Đa (Ticket/KTV)</th>
                  <th className="p-3 text-center">Tự Động Phân Phối?</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {rules.map((r) => (
                  <tr key={r.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{r.name}</td>
                    <td className="p-3 font-mono font-bold text-brand">{r.territory}</td>
                    <td className="p-3 font-semibold text-slate-800">{r.skill}</td>
                    <td className="p-3 text-center font-extrabold text-slate-700">{r.max} tickets</td>
                    <td className="p-3 text-center font-bold text-emerald-700">✓ Bật Auto Dispatch</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Đang Áp Dụng
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'checklist' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-bold text-foreground">📋 Checklist Công Việc Hiện Trường (UC_FSM_021) — Ticket: TCK-2026-0814-01</h2>
            <button onClick={() => showToast('✓ Đã đồng bộ checklist với hệ thống trung tâm!', 'success')} className="px-3 py-1.5 bg-brand text-brand-foreground text-xs font-bold rounded-lg hover:opacity-90">
              🔄 Đồng Bộ Checklist
            </button>
          </div>
          <div className="space-y-3">
            {checklist.map((c) => (
              <div key={c.id} onClick={() => toggleChecklist(c.id)} className={`p-4 rounded-xl border cursor-pointer transition-all flex items-center justify-between ${c.done ? 'bg-emerald-50/50 border-emerald-300' : 'bg-surface border-border hover:border-brand/40'}`}>
                <div className="flex items-center space-x-3">
                  <input type="checkbox" checked={c.done} onChange={() => {}} className="w-5 h-5 accent-emerald-600 rounded" />
                  <div>
                    <div className={`font-semibold text-sm ${c.done ? 'line-through text-muted-foreground' : 'text-foreground'}`}>
                      {c.step} {c.mandatory && <span className="text-rose-600 text-xs font-bold ml-1">*Bắt buộc</span>}
                    </div>
                    {c.done && (
                      <div className="text-xs text-emerald-700 mt-0.5">
                        ✓ Thực hiện bởi {c.by} lúc {c.at}
                      </div>
                    )}
                  </div>
                </div>
                <div>
                  <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${c.done ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-amber-100 text-amber-800 border-amber-300'}`}>
                    {c.done ? 'Đã Xong' : 'Chưa Thực Hiện'}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'photos' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📸 Ảnh Chụp Hiện Trường Trước & Sau Sửa Chữa (UC_FSM_023)</h2>
          <div className="grid grid-cols-2 gap-6">
            {photos.map((p) => (
              <div key={p.id} className="p-4 rounded-xl border border-border bg-surface space-y-3">
                <div className="flex justify-between items-center">
                  <span className="font-bold text-sm text-foreground">{formatPhotoTypeLabel(p.type)}</span>
                  <span className="text-xs text-muted-foreground">Lúc {p.time}</span>
                </div>
                <div className="h-44 bg-surface-hover rounded-lg flex items-center justify-center border border-dashed border-border text-muted-foreground font-medium text-sm">
                  🖼️ [Ảnh MinIO / S3: {p.url}]
                </div>
                <div className="text-sm font-semibold text-slate-800">{p.caption}</div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'returns' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📦 Phiếu Hoàn Trả Linh Kiện Thừa Về Kho (UC_FSM_025)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Phiếu Hoàn Trả</th>
                  <th className="p-3">Ticket Gốc</th>
                  <th className="p-3">Linh Kiện Hoàn Trả</th>
                  <th className="p-3 text-center">Số Lượng</th>
                  <th className="p-3">Kho Tiếp Nhận</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {returns.map((r) => (
                  <tr key={r.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{r.slip}</td>
                    <td className="p-3 font-mono font-bold text-foreground">{r.ticket}</td>
                    <td className="p-3">
                      <div className="font-bold text-foreground">{r.name}</div>
                      <div className="font-mono text-xs text-muted-foreground">{r.part}</div>
                    </td>
                    <td className="p-3 text-center font-extrabold text-emerald-700">{formatReturnedPartQuantity(r.qty)}</td>
                    <td className="p-3 text-slate-700 font-mono font-semibold">{r.wh}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Đã Nhập Kho
                      </span>
                    </td>
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
