'use client';

import React, { useState } from 'react';
import {
  evaluateComplaintSeverityBadge,
  calculateReconciliationMatchRate,
  validateComplaintForm,
} from '@/shared/api/crm-field-sales-ops-dispute-helpers';

export default function CrmFieldSalesOpsDisputePage() {
  const [activeTab, setActiveTab] = useState<'ai' | 'dashboard' | 'recon' | 'complaint'>('ai');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: AI PRIORITY RECOMMENDATIONS (UC_CRM_097)
  // ────────────────────────────────────────────────────────────────────────────
  const [aiActions] = useState([
    { id: 'ai-1', customer: 'Đại lý Nông Sản Miền Tây', level: 'High', title: 'Thăm điểm bán & Chốt đơn hàng phân bón đợt 2', reason: 'Khách hàng sắp hết tồn kho theo chu kỳ 14 ngày & đã xem bảng giá mới', potential: 25000000 },
    { id: 'ai-2', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', level: 'High', title: 'Giải quyết phản hồi mẫu thử sản phẩm mới', reason: 'Khách hàng đã trải nghiệm mẫu thử 3 ngày và có ý định đặt 50 thùng', potential: 15000000 },
    { id: 'ai-3', customer: 'Công ty TNHH Bách Hóa Việt', level: 'Medium', title: 'Nhắc tái đặt hàng nước giải khát', reason: 'Chu kỳ mua trung bình 30 ngày, còn 2 ngày đến hạn', potential: 8000000 },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: FIELD SALES REVENUE DASHBOARD (UC_CRM_098)
  // ────────────────────────────────────────────────────────────────────────────
  const metrics = {
    totalRevenue: 185000000,
    visitsPlanned: 40,
    visitsCompleted: 36,
    ordersCreated: 14,
    averageOrderValue: 13214000,
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: DOCUMENT RECONCILIATION (UC_CRM_102)
  // ────────────────────────────────────────────────────────────────────────────
  const [reconciliations, setReconciliations] = useState([
    { id: 'rc-1', orderCode: 'ORD-ONSITE-260813-A912', docCode: 'VAT-2026-991', docType: 'VATInvoice', status: 'Matched', notes: 'Khớp số tiền VAT và chữ ký nhận hàng' },
    { id: 'rc-2', orderCode: 'ORD-ONSITE-260812-B441', docCode: 'DN-2026-882', docType: 'DeliveryNote', status: 'Discrepancy', notes: 'Biên bản thiếu 2 thùng hàng do vỡ vận chuyển' },
  ]);

  const [reconForm, setReconForm] = useState({ orderCode: 'ORD-ONSITE-260813-A912', docCode: '', docType: 'VATInvoice', status: 'Matched', notes: '' });

  const handleReconcile = (e: React.FormEvent) => {
    e.preventDefault();
    if (!reconForm.docCode) {
      showToast('Vui lòng nhập Mã chứng từ đối soát.', 'error');
      return;
    }

    const created = {
      id: `rc-${Date.now()}`,
      orderCode: reconForm.orderCode,
      docCode: reconForm.docCode,
      docType: reconForm.docType,
      status: reconForm.status,
      notes: reconForm.notes || 'Đã đối soát với chứng từ gốc',
    };

    setReconciliations([created, ...reconciliations]);
    setReconForm({ orderCode: 'ORD-ONSITE-260813-A912', docCode: '', docType: 'VATInvoice', status: 'Matched', notes: '' });
    showToast(`Đã ghi nhận đối soát chứng từ [${created.docCode}]!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: ORDER COMPLAINT RESOLUTION (UC_CRM_103)
  // ────────────────────────────────────────────────────────────────────────────
  const [complaints, setComplaints] = useState([
    { id: 'cp-1', customer: 'Đại lý Nông Sản Miền Tây', orderCode: 'ORD-ONSITE-260813-A912', reason: 'Giao hàng trễ 2 tiếng so với cam kết', severity: 'Medium', status: 'Open', resolution: '' },
    { id: 'cp-2', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', orderCode: 'ORD-ONSITE-260812-B441', reason: 'Thiếu 2 thùng sản phẩm khi kiểm đếm', severity: 'High', status: 'Resolved', resolution: 'Đã xuất kho đợt 2 bù đủ cho khách' },
  ]);

  const [complaintForm, setComplaintForm] = useState({ orderCode: '', customer: 'Đại lý Nông Sản Miền Tây', reason: '', severity: 'Medium' });

  const handleCreateComplaint = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateComplaintForm(complaintForm.orderCode, complaintForm.reason);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `cp-${Date.now()}`,
      customer: complaintForm.customer,
      orderCode: complaintForm.orderCode,
      reason: complaintForm.reason,
      severity: complaintForm.severity,
      status: 'Open',
      resolution: '',
    };

    setComplaints([created, ...complaints]);
    setComplaintForm({ orderCode: '', customer: 'Đại lý Nông Sản Miền Tây', reason: '', severity: 'Medium' });
    showToast(`Đã tạo khiếu nại đơn hàng [${created.orderCode}]!`, 'success');
  };

  const handleResolveComplaint = (cpId: string) => {
    setComplaints((prev) =>
      prev.map((c) => (c.id === cpId ? { ...c, status: 'Resolved', resolution: 'Đã xử lý đền bù / cấp lại chứng từ mới' } : c))
    );
    showToast('✓ Đã xử lý giải quyết khiếu nại thành công!', 'success');
  };

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      {/* Toast */}
      {toast && (
        <div
          className={`fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white font-medium text-sm transition-all ${
            toast.type === 'success' ? 'bg-emerald-600' : toast.type === 'error' ? 'bg-rose-600' : 'bg-amber-600'
          }`}
        >
          {toast.message}
        </div>
      )}

      {/* Header */}
      <div className="bg-gradient-to-r from-emerald-950 via-slate-900 to-indigo-950 p-6 rounded-2xl text-white shadow-xl">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-emerald-500/30 text-emerald-200 text-xs px-3 py-1 rounded-full font-semibold border border-emerald-400/30">
              CRM - AI RECOMMENDATIONS, FIELD REVENUE & DISPUTES
            </span>
            <h1 className="text-2xl font-bold mt-2">Bước 173: CRM AI Gợi Ý Việc Ưu Tiên, Doanh Số Field & Khiếu Nại Đơn Hàng</h1>
            <p className="text-emerald-100 text-sm mt-1">
              AI gợi ý công việc ưu tiên, Dashboard doanh số field, Đối soát chứng từ đơn & Xử lý khiếu nại đơn hàng
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-emerald-500/20 text-emerald-300 border border-emerald-500/30">
              ● Tiến độ 90% (4/4 UCs)
            </span>
          </div>
        </div>

        {/* Tab Selection */}
        <div className="flex space-x-2 mt-6 border-t border-white/10 pt-4">
          <button
            onClick={() => setActiveTab('ai')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'ai' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🤖 UC_CRM_097: AI Gợi Ý Ưu Tiên
          </button>
          <button
            onClick={() => setActiveTab('dashboard')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'dashboard' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📈 UC_CRM_098: Dashboard Field Sales
          </button>
          <button
            onClick={() => setActiveTab('recon')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'recon' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📑 UC_CRM_102: Đối Soát Chứng Từ
          </button>
          <button
            onClick={() => setActiveTab('complaint')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'complaint' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            ⚠️ UC_CRM_103: Khiếu Nại Đơn Hàng
          </button>
        </div>
      </div>

      {/* TAB 1: AI PRIORITY ACTIONS */}
      {activeTab === 'ai' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">🤖 AI Gợi Ý Công Việc Ưu Tiên Cho Field Sales (UC_CRM_097)</h2>
          <div className="space-y-3">
            {aiActions.map((act) => (
              <div key={act.id} className="p-4 rounded-xl border border-indigo-100 bg-indigo-50/40 flex justify-between items-center">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="px-2 py-0.5 text-xs font-bold rounded bg-rose-100 text-rose-800">Ưu tiên {act.level}</span>
                    <h3 className="font-bold text-slate-900">{act.customer}</h3>
                  </div>
                  <p className="text-sm font-semibold text-indigo-950 mt-1">{act.title}</p>
                  <p className="text-xs text-slate-600 mt-1 italic">💡 Lý do AI gợi ý: "{act.reason}"</p>
                </div>
                <div className="text-right">
                  <span className="text-xs text-slate-500 block">Doanh số tiềm năng:</span>
                  <span className="text-base font-extrabold text-emerald-700">{act.potential.toLocaleString('vi-VN')} VNĐ</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* TAB 2: FIELD SALES REVENUE DASHBOARD */}
      {activeTab === 'dashboard' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold text-slate-500">Tổng Doanh Số Field</span>
              <p className="text-2xl font-bold text-emerald-600 mt-1">{metrics.totalRevenue.toLocaleString('vi-VN')} VNĐ</p>
            </div>
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold text-slate-500">Tỷ Lệ Thăm Hoàn Thành</span>
              <p className="text-2xl font-bold text-teal-600 mt-1">
                {metrics.visitsCompleted}/{metrics.visitsPlanned} ({calculateReconciliationMatchRate(metrics.visitsCompleted, metrics.visitsPlanned)}%)
              </p>
            </div>
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold text-slate-500">Số Đơn Hàng On-site</span>
              <p className="text-2xl font-bold text-indigo-600 mt-1">{metrics.ordersCreated} đơn</p>
            </div>
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold text-slate-500">Giá Trị Đơn Trung Bình</span>
              <p className="text-2xl font-bold text-blue-600 mt-1">{metrics.averageOrderValue.toLocaleString('vi-VN')} VNĐ</p>
            </div>
          </div>
        </div>
      )}

      {/* TAB 3: DOCUMENT RECONCILIATION */}
      {activeTab === 'recon' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">📑 Đối Soát Chứng Từ Đơn Hàng (UC_CRM_102)</h2>
            <div className="space-y-3">
              {reconciliations.map((r) => (
                <div key={r.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{r.docCode} (Loại: {r.docType})</h3>
                    <p className="text-xs text-slate-500 mt-1">Đơn hàng: {r.orderCode}</p>
                    <p className="text-xs text-slate-700 italic mt-1">"{r.notes}"</p>
                  </div>
                  <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${r.status === 'Matched' ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800'}`}>
                    {r.status === 'Matched' ? 'Khớp chứng từ' : 'Sai lệch'}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Thực Hiện Đối Soát</h2>
            <form onSubmit={handleReconcile} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Mã chứng từ:</label>
                <input
                  type="text"
                  value={reconForm.docCode}
                  onChange={(e) => setReconForm({ ...reconForm, docCode: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: VAT-2026-999"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Loại chứng từ:</label>
                <select
                  value={reconForm.docType}
                  onChange={(e) => setReconForm({ ...reconForm, docType: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2 bg-white"
                >
                  <option value="VATInvoice">Hóa đơn VAT (VATInvoice)</option>
                  <option value="DeliveryNote">Phiếu giao hàng (DeliveryNote)</option>
                  <option value="PaymentReceipt">Biên nhận thanh toán (PaymentReceipt)</option>
                </select>
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Ghi chú đối soát:</label>
                <textarea
                  value={reconForm.notes}
                  onChange={(e) => setReconForm({ ...reconForm, notes: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  rows={2}
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-teal-600 text-white rounded-lg font-semibold hover:bg-teal-700">
                Lưu Kết Quả Đối Soát
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 4: ORDER COMPLAINT RESOLUTION */}
      {activeTab === 'complaint' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">⚠️ Tiếp Nhận & Xử Lý Khiếu Nại Đơn Hàng (UC_CRM_103)</h2>
            <div className="space-y-3">
              {complaints.map((c) => {
                const sevBadge = evaluateComplaintSeverityBadge(c.severity);
                return (
                  <div key={c.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className={`px-2 py-0.5 text-xs font-bold rounded border ${sevBadge.badgeClass}`}>{sevBadge.label}</span>
                        <h3 className="font-bold text-slate-900">{c.customer}</h3>
                      </div>
                      <p className="text-xs text-slate-500 mt-1">Mã đơn: {c.orderCode}</p>
                      <p className="text-xs text-rose-900 font-semibold mt-1">Lý do: "{c.reason}"</p>
                      {c.resolution && <p className="text-xs text-emerald-800 italic mt-0.5">Xử lý: {c.resolution}</p>}
                    </div>

                    <div className="text-right">
                      {c.status === 'Open' ? (
                        <button
                          onClick={() => handleResolveComplaint(c.id)}
                          className="px-3.5 py-2 bg-emerald-600 text-white text-xs font-bold rounded-lg hover:bg-emerald-700 shadow-sm"
                        >
                          ✓ Xử Lý Giải Quyết
                        </button>
                      ) : (
                        <span className="px-3 py-1.5 bg-emerald-100 text-emerald-800 text-xs font-bold rounded-lg">
                          ✓ Đã giải quyết
                        </span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Tiếp Nhận Khiếu Nại Mới</h2>
            <form onSubmit={handleCreateComplaint} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Mã đơn hàng:</label>
                <input
                  type="text"
                  value={complaintForm.orderCode}
                  onChange={(e) => setComplaintForm({ ...complaintForm, orderCode: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: ORD-ONSITE-260813-A912"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Nội dung khiếu nại:</label>
                <textarea
                  value={complaintForm.reason}
                  onChange={(e) => setComplaintForm({ ...complaintForm, reason: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  rows={3}
                  placeholder="VD: Hàng giao bị rách vỏ hộp..."
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-rose-600 text-white rounded-lg font-semibold hover:bg-rose-700">
                Gửi Khiếu Nại
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
