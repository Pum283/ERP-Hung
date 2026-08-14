'use client';

import React, { useState } from 'react';
import {
  getTransferApprovalStatusBadge,
  formatSerialLifecycleSummary,
} from '@/shared/api/inv-project-transfer-serial-tracking-helpers';

export default function InvProjectTransferSerialTrackingPage() {
  const [activeTab, setActiveTab] = useState<'project' | 'approval' | 'onestep' | 'serial'>('project');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_INV_028: Xuất cho dự án
  const [projectForm, setProjectForm] = useState({
    projectName: 'Dự Án Xây Dựng Trạm Viễn Thông Bến Cát',
    phase: 'Phase 1 - Đổ móng và dựng cột anten',
    allocatedValue: 45000000,
  });

  const handleSaveProjectDispatch = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã tạo phiếu xuất kho cấp phát vật tư cho dự án [${projectForm.projectName}]!`, 'success');
  };

  // UC_INV_032: Duyệt chuyển kho
  const [approvals, setApprovals] = useState([
    { id: 'app-1', code: 'TRF-REQ-2026-001', from: 'Kho Tổng TP.HCM', to: 'Kho Chi Nhánh Hà Nội', status: 'PendingApproval' },
    { id: 'app-2', code: 'TRF-REQ-2026-002', from: 'Kho Bình Dương', to: 'Kho Đà Nẵng', status: 'Approved' },
  ]);

  const handleApprove = (id: string, isApprove: boolean) => {
    setApprovals(approvals.map(a => a.id === id ? { ...a, status: isApprove ? 'Approved' : 'Rejected' } : a));
    showToast(isApprove ? '✓ Đã phê duyệt yêu cầu điều chuyển kho!' : '✓ Đã từ chối yêu cầu điều chuyển kho.', isApprove ? 'success' : 'error');
  };

  // UC_INV_034: Chuyển kho một bước
  const [oneStepForm, setOneStepForm] = useState({
    fromWh: 'Kho Tổng TP.HCM',
    toWh: 'Kho Trung Chuyển Tân Bình',
    sku: 'SKU-ROUTER-ENTERPRISE',
    qty: 25,
    reason: 'Cân bằng lượng hàng phân phối tức thời',
  });

  const handleExecuteOneStep = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã hoàn tất điều chuyển 1 bước 25 sản phẩm sang [${oneStepForm.toWh}]!`, 'success');
  };

  // UC_INV_046: Theo dõi serial
  const [serialQuery, setSerialQuery] = useState('SN-CISCO-889911');
  const [serialEvents] = useState([
    { id: 'ev-1', event: 'Nhập Kho Ban Đầu', location: 'Kho Tổng TP.HCM', ref: 'GRN-2026-001', time: '2026-08-01 09:30' },
    { id: 'ev-2', event: 'Điều Chuyển Nội Bộ', location: 'Kho Chi Nhánh Hà Nội', ref: 'TRF-DIRECT-088', time: '2026-08-08 14:15' },
    { id: 'ev-3', event: 'Xuất Cấp Phát Dự Án', location: 'Trạm Viễn Thông Bến Cát', ref: 'PRJ-OUT-20260814', time: '2026-08-14 08:00' },
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
              INV - PROJECT DISPATCH, TRANSFER APPROVAL, ONE-STEP TRANSFER & SERIAL TRACKING
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Xuất Kho Dự Án, Duyệt Điều Chuyển, Chuyển Kho 1 Bước & Truy Vết Serial</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý xuất kho vật tư theo giai đoạn dự án, quy trình phê duyệt luân chuyển kho, điều chuyển trực tiếp 1 bước và tra cứu hành trình serial
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
            onClick={() => setActiveTab('project')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'project' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏗️ UC_INV_028: Xuất Cho Dự Án
          </button>
          <button
            onClick={() => setActiveTab('approval')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'approval' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ✅ UC_INV_032: Duyệt Chuyển Kho
          </button>
          <button
            onClick={() => setActiveTab('onestep')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'onestep' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚡ UC_INV_034: Chuyển Kho Một Bước
          </button>
          <button
            onClick={() => setActiveTab('serial')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'serial' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔍 UC_INV_046: Theo Dõi Hành Trình Serial
          </button>
        </div>
      </div>

      {activeTab === 'project' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">🏗️ Lập Phiếu Xuất Kho Cấp Phát Vật Tư Theo Dự Án (UC_INV_028)</h2>
          <form onSubmit={handleSaveProjectDispatch} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Tên Dự Án Mục Tiêu:</label>
              <input
                type="text"
                value={projectForm.projectName}
                onChange={(e) => setProjectForm({ ...projectForm, projectName: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold"
              />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Giai Đoạn Thi Công (Project Phase):</label>
              <input
                type="text"
                value={projectForm.phase}
                onChange={(e) => setProjectForm({ ...projectForm, phase: e.target.value })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Giá Trị Vật Tư Xuất Cấp Phát (VNĐ):</label>
              <input
                type="number"
                value={projectForm.allocatedValue}
                onChange={(e) => setProjectForm({ ...projectForm, allocatedValue: Number(e.target.value) })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold"
              />
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              💾 Lưu Phiếu Xuất Kho Dự Án (PRJ-OUT)
            </button>
          </form>
        </div>
      )}

      {activeTab === 'approval' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">✅ Danh Sách Phê Duyệt Yêu Cầu Điều Chuyển Kho (UC_INV_032)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Yêu Cầu Chuyển Kho</th>
                  <th className="p-3">Kho Xuất Hàng</th>
                  <th className="p-3">Kho Đích Nhận Hàng</th>
                  <th className="p-3">Trạng Thái Duyệt</th>
                  <th className="p-3 text-right">Thao Tác Quyết Định</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {approvals.map((a) => {
                  const badge = getTransferApprovalStatusBadge(a.status);
                  return (
                    <tr key={a.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-foreground">{a.code}</td>
                      <td className="p-3 text-slate-700 font-medium">{a.from}</td>
                      <td className="p-3 text-slate-700 font-medium">{a.to}</td>
                      <td className="p-3">
                        <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${badge.colorClass}`}>
                          ● {badge.label}
                        </span>
                      </td>
                      <td className="p-3 text-right space-x-2">
                        {a.status === 'PendingApproval' && (
                          <>
                            <button
                              onClick={() => handleApprove(a.id, true)}
                              className="px-3 py-1 bg-emerald-600 text-white text-xs font-bold rounded hover:bg-emerald-700 shadow-sm"
                            >
                              ✓ Duyệt
                            </button>
                            <button
                              onClick={() => handleApprove(a.id, false)}
                              className="px-3 py-1 bg-rose-600 text-white text-xs font-bold rounded hover:bg-rose-700 shadow-sm"
                            >
                              ✕ Từ Chối
                            </button>
                          </>
                        )}
                        {a.status !== 'PendingApproval' && (
                          <span className="text-xs text-muted-foreground italic">Đã xử lý xong</span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'onestep' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <div>
            <h2 className="text-lg font-bold text-foreground">⚡ Chuyển Kho Một Bước Trực Tiếp (UC_INV_034)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Xuất kho và nhập kho tức thời trong cùng một giao dịch (không qua trạng thái hàng đi đường)</p>
          </div>

          <form onSubmit={handleExecuteOneStep} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Từ Kho Xuất (Source WH):</label>
                <input type="text" value={oneStepForm.fromWh} onChange={(e) => setOneStepForm({ ...oneStepForm, fromWh: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Đến Kho Nhập (Dest WH):</label>
                <input type="text" value={oneStepForm.toWh} onChange={(e) => setOneStepForm({ ...oneStepForm, toWh: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Sản Phẩm (SKU):</label>
                <input type="text" value={oneStepForm.sku} onChange={(e) => setOneStepForm({ ...oneStepForm, sku: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Lượng Chuyển Kho:</label>
                <input
                  type="number"
                  value={oneStepForm.qty}
                  onChange={(e) => setOneStepForm({ ...oneStepForm, qty: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold"
                />
              </div>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Lý Do Chuyển Kho Tức Thời:</label>
              <input type="text" value={oneStepForm.reason} onChange={(e) => setOneStepForm({ ...oneStepForm, reason: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              ⚡ Thực Thi Điều Chuyển Kho 1 Bước Tức Thời
            </button>
          </form>
        </div>
      )}

      {activeTab === 'serial' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-3xl space-y-6">
          <div>
            <h2 className="text-lg font-bold text-foreground">🔍 Tra Cứu & Truy Vết Hành Trình Vòng Đời Serial (UC_INV_046)</h2>
            <div className="flex gap-2 mt-3">
              <input
                type="text"
                value={serialQuery}
                onChange={(e) => setSerialQuery(e.target.value)}
                className="w-full border border-border rounded-lg p-2.5 bg-surface text-foreground font-mono font-bold text-sm"
              />
              <button
                type="button"
                onClick={() => showToast(`✓ Đã tra cứu dữ liệu lịch sử của Serial [${serialQuery}]`, 'success')}
                className="px-4 py-2.5 bg-brand text-brand-foreground font-bold text-sm rounded-lg hover:opacity-90 whitespace-nowrap"
              >
                Tra Cứu
              </button>
            </div>
          </div>

          <div className="p-3.5 rounded-xl border border-brand/30 bg-brand-muted/15 text-xs font-bold text-brand-strong">
            {formatSerialLifecycleSummary(serialEvents.length, 'Trạm Viễn Thông Bến Cát')}
          </div>

          {/* Timeline */}
          <div className="space-y-4 relative border-l-2 border-brand/40 ml-4 pl-6 pt-2">
            {serialEvents.map((ev) => (
              <div key={ev.id} className="relative space-y-1">
                <span className="absolute -left-[31px] top-1 w-3.5 h-3.5 rounded-full bg-brand border-2 border-white shadow" />
                <div className="flex items-center justify-between text-xs">
                  <span className="font-extrabold text-foreground text-sm">{ev.event}</span>
                  <span className="text-muted-foreground">{ev.time}</span>
                </div>
                <div className="text-xs text-slate-700 font-medium">Vị trí hiện diện: <b className="text-foreground">{ev.location}</b></div>
                <div className="text-[11px] text-muted-foreground font-mono">Chứng từ tham chiếu: {ev.ref}</div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
