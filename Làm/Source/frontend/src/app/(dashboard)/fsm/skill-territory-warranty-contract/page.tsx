'use client';

import React, { useState } from 'react';
import {
  formatWarrantyDaysRemaining,
  formatContractValue,
} from '@/shared/api/fsm-skill-territory-warranty-contract-helpers';

export default function FsmSkillTerritoryWarrantyContractPage() {
  const [activeTab, setActiveTab] = useState<'skills' | 'territories' | 'alerts' | 'contracts'>('skills');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FSM_006: Kỹ năng & chứng chỉ KTV
  const [skills, setSkills] = useState([
    { id: 's-1', name: 'Nguyễn Văn Tuấn', code: 'SKILL-HVAC', skill: 'Hệ Thống Lạnh Chiller & HVAC', level: 'Chuyên Gia', cert: 'CERT-HVAC-998', issued: '2024-05-10', active: true },
    { id: 's-2', name: 'Trần Minh Hùng', code: 'SKILL-ELEC-PLC', skill: 'Điện Công Nghiệp & Lập Trình PLC', level: 'Bậc 5/7', cert: 'CERT-PLC-412', issued: '2025-01-15', active: true },
  ]);

  // UC_FSM_007: Vùng phụ trách
  const [territories, setTerritories] = useState([
    { id: 't-1', code: 'REGION-SOUTH-01', name: 'Khu Vực TP.HCM & Bình Dương', city: 'TP. Hồ Chí Minh', hub: 'HUB-HCM-01', lead: 'Trần Minh Hùng' },
    { id: 't-2', code: 'REGION-NORTH-01', name: 'Khu Vực Hà Nội & Bắc Ninh', city: 'Hà Nội', hub: 'HUB-HN-01', lead: 'Nguyễn Văn Tuấn' },
  ]);

  // UC_FSM_011: Cảnh báo hết hạn bảo hành
  const [alerts] = useState([
    { id: 'a-1', sn: 'SN-RACK-42U-00129', model: 'Tủ Rack Server Cao Cấp 42U', cust: 'Công Ty Viễn Thông Viettel', end: '2026-08-29', days: 15, notified: true },
    { id: 'a-2', sn: 'SN-CNC-MILL-508', model: 'Máy Phay CNC 5 Trục Model Pro', cust: 'Tập Đoàn Cơ Khí FPT', end: '2026-08-21', days: 7, notified: false },
  ]);

  // UC_FSM_012: Hợp đồng bảo trì định kỳ
  const [contracts, setContracts] = useState([
    { id: 'c-1', no: 'CTR-MAINT-2026-01', cust: 'Tập Đoàn Bưu Chính Viễn Thông VNPT', sla: 'Diamond SLA 1h', visits: 12, val: 120000000, start: '2026-01-01', end: '2026-12-31', status: 'Active' },
    { id: 'c-2', no: 'CTR-MAINT-2026-02', cust: 'Công Ty CP Thép Hòa Phát', sla: 'Gold SLA 2h', visits: 4, val: 48000000, start: '2026-06-01', end: '2027-05-31', status: 'Active' },
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
              FSM - TECHNICIAN SKILLS, TERRITORIES, WARRANTY ALERTS & CONTRACTS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Kỹ Năng Kỹ Thuật Viên, Vùng Phụ Trách, Cảnh Báo Bảo Hành & Hợp Đồng Bảo Trì</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý chứng chỉ chuyên môn, phân bổ địa bàn kỹ thuật, theo dõi hạn bảo hành thiết bị và quản trị hợp đồng SLA định kỳ
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
            onClick={() => setActiveTab('skills')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'skills' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🎓 UC_FSM_006: Kỹ Năng & Chứng Chỉ
          </button>
          <button
            onClick={() => setActiveTab('territories')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'territories' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🗺️ UC_FSM_007: Vùng Phụ Trách
          </button>
          <button
            onClick={() => setActiveTab('alerts')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'alerts' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚠️ UC_FSM_011: Cảnh Báo Hết Hạn BH
          </button>
          <button
            onClick={() => setActiveTab('contracts')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'contracts' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📜 UC_FSM_012: Hợp Đồng Bảo Trì SLA
          </button>
        </div>
      </div>

      {activeTab === 'skills' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🎓 Hồ Sơ Kỹ Năng & Chứng Chỉ Kỹ Thuật Viên (UC_FSM_006)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Họ Tên Kỹ Thuật Viên</th>
                  <th className="p-3">Chuyên Môn & Kỹ Năng</th>
                  <th className="p-3">Cấp Bậc Chuyên Môn</th>
                  <th className="p-3">Số Hiệu Chứng Chỉ</th>
                  <th className="p-3">Ngày Cấp</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {skills.map((s) => (
                  <tr key={s.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{s.name}</td>
                    <td className="p-3">
                      <div className="font-semibold text-brand">{s.skill}</div>
                      <div className="font-mono text-xs text-muted-foreground">{s.code}</div>
                    </td>
                    <td className="p-3 font-semibold text-slate-800">{s.level}</td>
                    <td className="p-3 font-mono font-bold text-slate-700">{s.cert}</td>
                    <td className="p-3 text-slate-700">{s.issued}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Có Hiệu Lực
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'territories' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🗺️ Phân Bổ Địa Bàn & Vùng Phụ Trách Kỹ Thuật (UC_FSM_007)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Vùng</th>
                  <th className="p-3">Tên Địa Bàn Phụ Trách</th>
                  <th className="p-3">Tỉnh / Thành Phố</th>
                  <th className="p-3">Kho Hub Điều Phối</th>
                  <th className="p-3">Trưởng Vùng Kỹ Thuật</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {territories.map((t) => (
                  <tr key={t.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{t.code}</td>
                    <td className="p-3 font-bold text-foreground">{t.name}</td>
                    <td className="p-3 text-slate-700">{t.city}</td>
                    <td className="p-3 font-mono font-bold text-slate-700">{t.hub}</td>
                    <td className="p-3 font-semibold text-slate-800">{t.lead}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-blue-100 text-blue-800 border border-blue-300">
                        ● Đang Hoạt Động
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'alerts' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⚠️ Cảnh Báo Thiết Bị Sắp Hết Hạn Bảo Hành (UC_FSM_011)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Serial & Thiết Bị</th>
                  <th className="p-3">Khách Hàng Sử Dụng</th>
                  <th className="p-3">Ngày Hết Hạn BH</th>
                  <th className="p-3 text-center">Thời Gian Còn Lại</th>
                  <th className="p-3 text-center">Đã Gửi Thông Báo?</th>
                  <th className="p-3 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {alerts.map((a) => (
                  <tr key={a.id} className="hover:bg-surface-hover/50">
                    <td className="p-3">
                      <div className="font-mono font-bold text-foreground">{a.sn}</div>
                      <div className="text-xs text-muted-foreground">{a.model}</div>
                    </td>
                    <td className="p-3 font-semibold text-slate-800">{a.cust}</td>
                    <td className="p-3 font-medium text-rose-700">{a.end}</td>
                    <td className="p-3 text-center font-bold text-amber-700">{formatWarrantyDaysRemaining(a.days)}</td>
                    <td className="p-3 text-center">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${a.notified ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-slate-100 text-slate-800 border-slate-300'}`}>
                        {a.notified ? '✓ Đã Gửi Email/SMS' : 'Chưa Gửi'}
                      </span>
                    </td>
                    <td className="p-3 text-right">
                      <button onClick={() => showToast(`✓ Đã gửi đề xuất gia hạn bảo trì cho [${a.cust}]!`, 'success')} className="px-3 py-1 bg-brand text-brand-foreground text-xs font-bold rounded hover:opacity-90">
                        ✉️ Chào Gia Hạn
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'contracts' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📜 Quản Lý Hợp Đồng Bảo Trì Định Kỳ SLA (UC_FSM_012)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Hợp Đồng</th>
                  <th className="p-3">Khách Hàng</th>
                  <th className="p-3">Gói Dịch Vụ SLA</th>
                  <th className="p-3 text-center">Số Lượt/Năm</th>
                  <th className="p-3 text-right">Giá Trị Hợp Đồng</th>
                  <th className="p-3">Thời Hạn Hiệu Lực</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {contracts.map((c) => (
                  <tr key={c.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{c.no}</td>
                    <td className="p-3 font-bold text-foreground">{c.cust}</td>
                    <td className="p-3 font-semibold text-blue-700">{c.sla}</td>
                    <td className="p-3 text-center font-extrabold text-slate-800">{c.visits} lượt</td>
                    <td className="p-3 text-right font-black text-foreground">{formatContractValue(c.val)}</td>
                    <td className="p-3 text-xs text-slate-700">{c.start} ➔ {c.end}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Đang Hiệu Lực
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
