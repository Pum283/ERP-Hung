'use client';

import React, { useState } from 'react';
import {
  getRequisitionStatusBadge,
  getSlowMovingRiskLevelBadge,
} from '@/shared/api/inv-material-requisition-approval-slow-moving-helpers';

export default function InvMaterialRequisitionApprovalSlowMovingPage() {
  const [activeTab, setActiveTab] = useState<'createReq' | 'manageReq' | 'slowMoving'>('createReq');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_INV_057: Đề nghị cấp hàng
  const [reqForm, setReqForm] = useState({
    requester: 'Lê Văn Kỹ Sư',
    dept: 'Xưởng Cơ Khí Lắp Ráp',
    sku: 'SKU-BOLT-M8',
    qty: 500,
  });

  const handleCreateReq = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã lập phiếu đề nghị cấp hàng REQ-MAT-20260814 cho [${reqForm.dept}]!`, 'success');
  };

  // UC_INV_058 & UC_INV_059: Duyệt đề nghị & Chuyển thành phiếu xuất
  const [requisitionList, setRequisitionList] = useState([
    { id: 'r-1', code: 'REQ-MAT-2026-001', requester: 'Lê Văn Kỹ Sư', dept: 'Xưởng Cơ Khí', sku: 'SKU-BOLT-M8', qty: 500, status: 'Submitted', issueNo: '' },
    { id: 'r-2', code: 'REQ-MAT-2026-002', requester: 'Nguyễn Kỹ Thuật', dept: 'Phòng Bảo Trì', sku: 'SKU-OIL-10W40', qty: 20, status: 'Approved', issueNo: '' },
  ]);

  const handleApproveReq = (id: string, isApprove: boolean) => {
    setRequisitionList(requisitionList.map(r => r.id === id ? { ...r, status: isApprove ? 'Approved' : 'Rejected' } : r));
    showToast(isApprove ? '✓ Đã phê duyệt đề nghị cấp vật tư!' : '✓ Đã từ chối đề nghị cấp vật tư.', isApprove ? 'success' : 'error');
  };

  const handleConvertToIssue = (id: string) => {
    const issueNum = 'ISSUE-MAT-' + Math.floor(1000 + Math.random() * 9000);
    setRequisitionList(requisitionList.map(r => r.id === id ? { ...r, status: 'ConvertedToIssue', issueNo: issueNum } : r));
    showToast(`✓ Đã tạo phiếu xuất kho cấp phát [${issueNum}] thành công!`, 'success');
  };

  // UC_INV_066: Hàng chậm luân chuyển
  const [slowMovingList] = useState([
    { id: 'sm-1', sku: 'SKU-OLD-BOARD', name: 'Bo Mạch Chủ Server Gen 8', stock: 40, days: 210, capital: 80000000, risk: 'HighRisk' },
    { id: 'sm-2', sku: 'SKU-FAN-COOLER', name: 'Quạt Tản Nhiệt Rack Công Suất Lớn', stock: 120, days: 110, capital: 36000000, risk: 'MediumRisk' },
    { id: 'sm-3', sku: 'SKU-CABLE-CAT6', name: 'Cuộn Dây Cáp Mạng Cat6 305m', stock: 15, days: 60, capital: 15000000, risk: 'LowRisk' },
  ]);

  const totalCapital = slowMovingList.reduce((acc, i) => acc + i.capital, 0);

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
              INV - MATERIAL REQUISITIONS, APPROVAL, CONVERT ISSUE & SLOW MOVING INVENTORY
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Đề Nghị Cấp Hàng, Duyệt Đề Nghị, Xuất Cấp Phát & Cảnh Báo Hàng Chậm Luân Chuyển</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quy trình quản lý phiếu yêu cầu vật tư, phê duyệt xuất kho liên phòng ban và báo cáo phân tích vốn đọng hàng lưu kho lâu ngày
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
            onClick={() => setActiveTab('createReq')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'createReq' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📝 UC_INV_057: Tạo Đề Nghị Cấp Hàng
          </button>
          <button
            onClick={() => setActiveTab('manageReq')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'manageReq' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ✅ UC_INV_058 & 059: Duyệt & Xuất Cấp Hàng
          </button>
          <button
            onClick={() => setActiveTab('slowMoving')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'slowMoving' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⏳ UC_INV_066: Hàng Chậm Luân Chuyển
          </button>
        </div>
      </div>

      {activeTab === 'createReq' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">📝 Lập Phiếu Đề Nghị Cấp Hàng Vật Tư (UC_INV_057)</h2>
          <form onSubmit={handleCreateReq} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Người Đề Nghị:</label>
                <input type="text" value={reqForm.requester} onChange={(e) => setReqForm({ ...reqForm, requester: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Phòng Ban / Xưởng Yêu Cầu:</label>
                <input type="text" value={reqForm.dept} onChange={(e) => setReqForm({ ...reqForm, dept: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Sản Phẩm / Vật Tư (SKU):</label>
                <input type="text" value={reqForm.sku} onChange={(e) => setReqForm({ ...reqForm, sku: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Lượng Cần Cấp Phát:</label>
                <input
                  type="number"
                  value={reqForm.qty}
                  onChange={(e) => setReqForm({ ...reqForm, qty: Number(e.target.value) })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold"
                />
              </div>
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              💾 Gửi Đề Nghị Cấp Hàng (REQ-MAT)
            </button>
          </form>
        </div>
      )}

      {activeTab === 'manageReq' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">✅ Phê Duyệt & Chuyển Thành Phiếu Xuất Kho Cấp Hàng (UC_INV_058 & UC_INV_059)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Đề Nghị</th>
                  <th className="p-3">Người & Phòng Ban</th>
                  <th className="p-3">Sản Phẩm & Số Lượng</th>
                  <th className="p-3">Trạng Thái</th>
                  <th className="p-3 text-right">Thao Tác Xử Lý</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {requisitionList.map((r) => {
                  const badge = getRequisitionStatusBadge(r.status);
                  return (
                    <tr key={r.id} className="hover:bg-surface-hover/50">
                      <td className="p-3 font-mono font-bold text-foreground">{r.code}</td>
                      <td className="p-3">
                        <div className="font-bold text-foreground">{r.requester}</div>
                        <div className="text-xs text-muted-foreground">{r.dept}</div>
                      </td>
                      <td className="p-3 font-medium text-slate-700">
                        {r.sku} ({r.qty} cái)
                      </td>
                      <td className="p-3">
                        <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${badge.colorClass}`}>
                          ● {badge.label}
                        </span>
                        {r.issueNo && <div className="text-xs font-mono font-bold text-blue-700 mt-1">Phiếu: {r.issueNo}</div>}
                      </td>
                      <td className="p-3 text-right space-x-2">
                        {r.status === 'Submitted' && (
                          <>
                            <button
                              onClick={() => handleApproveReq(r.id, true)}
                              className="px-3 py-1 bg-emerald-600 text-white text-xs font-bold rounded hover:bg-emerald-700"
                            >
                              ✓ Duyệt
                            </button>
                            <button
                              onClick={() => handleApproveReq(r.id, false)}
                              className="px-3 py-1 bg-rose-600 text-white text-xs font-bold rounded hover:bg-rose-700"
                            >
                              ✕ Từ Chối
                            </button>
                          </>
                        )}
                        {r.status === 'Approved' && (
                          <button
                            onClick={() => handleConvertToIssue(r.id)}
                            className="px-3 py-1 bg-brand text-brand-foreground text-xs font-bold rounded hover:opacity-90 shadow-sm"
                          >
                            📦 Chuyển Thành Phiếu Xuất
                          </button>
                        )}
                        {r.status === 'ConvertedToIssue' && (
                          <span className="text-xs text-emerald-800 font-bold">✓ Đã Hoàn Tất Xuất Kho</span>
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

      {activeTab === 'slowMoving' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div className="p-5 rounded-xl border border-brand/30 bg-brand-muted/20 flex justify-between items-center">
            <div>
              <span className="text-xs font-bold text-brand-strong">TỔNG VỐN ĐỌNG TRONG HÀNG CHẬM LUÂN CHUYỂN:</span>
              <h2 className="text-2xl font-black text-brand-strong mt-1">{totalCapital.toLocaleString('vi-VN')} VNĐ</h2>
            </div>
            <div className="text-right">
              <span className="text-xs text-muted-foreground">Tổng SKU Chậm Luân Chuyển:</span>
              <div className="text-xl font-extrabold text-rose-700">{slowMovingList.length} SKU</div>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã SKU & Tên Sản Phẩm</th>
                  <th className="p-3 text-center">Tồn Kho Hiện Tại</th>
                  <th className="p-3 text-center">Số Ngày Không Có Xuất</th>
                  <th className="p-3 text-right">Vốn Đọng (VNĐ)</th>
                  <th className="p-3 text-right">Mức Độ Cảnh Báo</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {slowMovingList.map((item) => {
                  const risk = getSlowMovingRiskLevelBadge(item.risk);
                  return (
                    <tr key={item.id} className="hover:bg-surface-hover/50">
                      <td className="p-3">
                        <div className="font-mono font-bold text-foreground">{item.sku}</div>
                        <div className="text-xs text-muted-foreground">{item.name}</div>
                      </td>
                      <td className="p-3 text-center font-bold text-slate-800">{item.stock} cái</td>
                      <td className="p-3 text-center font-extrabold text-rose-700">{item.days} ngày</td>
                      <td className="p-3 text-right font-extrabold text-foreground">{item.capital.toLocaleString('vi-VN')} đ</td>
                      <td className="p-3 text-right">
                        <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${risk.colorClass}`}>
                          ● {risk.label}
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
