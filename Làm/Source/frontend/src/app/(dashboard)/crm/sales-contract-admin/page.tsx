'use client';

import React, { useState } from 'react';
import {
  evaluateContractValidityStatus,
  formatContractFileSize,
  validateContractForm,
} from '@/shared/api/crm-sales-contract-admin-helpers';

export default function CrmSalesContractAdminPage() {
  const [activeTab, setActiveTab] = useState<'productivity' | 'contract' | 'attachment' | 'renewal'>('productivity');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: SALES ADMIN PRODUCTIVITY REPORT (UC_CRM_105)
  // ────────────────────────────────────────────────────────────────────────────
  const [productivityList] = useState([
    { id: 'adm-1', name: 'Nguyễn Thị SalesAdmin 1', orders: 145, contracts: 28, avgTime: '1.2 giờ', accuracy: 98.5 },
    { id: 'adm-2', name: 'Phạm Văn Admin 2', orders: 112, contracts: 22, avgTime: '1.5 giờ', accuracy: 96.8 },
    { id: 'adm-3', name: 'Lê Hoàng SalesAdmin 3', orders: 98, contracts: 19, avgTime: '1.8 giờ', accuracy: 97.2 },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: SALES CONTRACT MANAGEMENT (UC_CRM_106)
  // ────────────────────────────────────────────────────────────────────────────
  const [contracts, setContracts] = useState([
    { id: 'ct-1', code: 'HD-2026-991', title: 'Hợp đồng Cung ứng Nông sản Q3', customer: 'Đại lý Nông Sản Miền Tây', value: 350000000, startDate: '2026-02-01', endDate: '2026-12-31', status: 'Active', attachments: 2 },
    { id: 'ct-2', code: 'HD-2026-882', title: 'Hợp đồng Phân phối Chuỗi Tiện Lợi', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', value: 180000000, startDate: '2025-09-01', endDate: '2026-08-30', status: 'ExpiringSoon', attachments: 1 },
  ]);

  const [contractForm, setContractForm] = useState({ code: '', title: '', customer: '', value: 100000000, startDate: '', endDate: '' });

  const handleCreateContract = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateContractForm(contractForm.code, contractForm.value, contractForm.customer);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `ct-${Date.now()}`,
      code: contractForm.code,
      title: contractForm.title || `Hợp đồng ${contractForm.code}`,
      customer: contractForm.customer,
      value: contractForm.value,
      startDate: contractForm.startDate || '2026-08-13',
      endDate: contractForm.endDate || '2027-08-13',
      status: 'Active',
      attachments: 0,
    };

    setContracts([created, ...contracts]);
    setContractForm({ code: '', title: '', customer: '', value: 100000000, startDate: '', endDate: '' });
    showToast(`Đã tạo Hợp đồng bán hàng mới [${created.code}] thành công!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: CONTRACT FILE ATTACHMENTS (UC_CRM_107)
  // ────────────────────────────────────────────────────────────────────────────
  const [attachments, setAttachments] = useState([
    { id: 'att-1', contractCode: 'HD-2026-991', fileName: 'HopDong_KyKet_Scan.pdf', size: 2450000, type: 'PDF', time: '10:00 - 13/08/2026' },
    { id: 'att-2', contractCode: 'HD-2026-991', fileName: 'PhuLuc_GiaSp_DinhKem.docx', size: 1024000, type: 'DOCX', time: '10:05 - 13/08/2026' },
  ]);

  const [selectedFile, setSelectedFile] = useState('');

  const handleAttachFile = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedFile) {
      showToast('Vui lòng chọn hoặc nhập tên file đính kèm.', 'error');
      return;
    }

    const created = {
      id: `att-${Date.now()}`,
      contractCode: contracts[0].code,
      fileName: selectedFile,
      size: 1540000,
      type: 'PDF',
      time: 'Vừa xong',
    };

    setAttachments([created, ...attachments]);
    setSelectedFile('');
    showToast(`📎 Đã đính kèm file [${created.fileName}] vào hợp đồng!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: CONTRACT VALIDITY & RENEWAL TRACKING (UC_CRM_108)
  // ────────────────────────────────────────────────────────────────────────────
  const handleRenewContract = (ctId: string, code: string) => {
    setContracts((prev) =>
      prev.map((c) => (c.id === ctId ? { ...c, status: 'Renewed', endDate: '2027-12-31' } : c))
    );
    showToast(`🔄 Đã tái tục Hợp đồng [${code}] thêm 12 tháng thành công!`, 'success');
  };

  return (
    <div className="p-6 space-y-6">
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
      <div className="bg-brand p-5 rounded-xl text-brand-foreground shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-blue-500/30 text-blue-200 text-xs px-3 py-1 rounded-full font-semibold border border-blue-400/30">
              CRM - SALES ADMIN & CONTRACT MANAGEMENT
            </span>
            <h1 className="text-2xl font-bold mt-2">CRM Báo Cáo Sales Admin & Quản Lý Hợp Đồng Bán Hàng</h1>
            <p className="text-blue-100 text-sm mt-1">
              Báo cáo năng suất Sales Admin, Quản lý hợp đồng bán, Đính kèm tài liệu & Theo dõi hiệu lực tái tục
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
            onClick={() => setActiveTab('productivity')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'productivity' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📊 UC_CRM_105: Năng Suất Sales Admin
          </button>
          <button
            onClick={() => setActiveTab('contract')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'contract' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📜 UC_CRM_106: Quản Lý Hợp Đồng
          </button>
          <button
            onClick={() => setActiveTab('attachment')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'attachment' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📎 UC_CRM_107: File Đính Kèm Hợp Đồng
          </button>
          <button
            onClick={() => setActiveTab('renewal')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'renewal' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🔄 UC_CRM_108: Hiệu Lực & Tái Tục
          </button>
        </div>
      </div>

      {/* TAB 1: SALES ADMIN PRODUCTIVITY REPORT */}
      {activeTab === 'productivity' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">📊 Báo Cáo Hiệu Suất Năng Suất Sales Admin (UC_CRM_105)</h2>
          <div className="space-y-3">
            {productivityList.map((p) => (
              <div key={p.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                <div>
                  <h3 className="font-bold text-slate-900">{p.name}</h3>
                  <p className="text-xs text-slate-500 mt-1">
                    Đơn hàng xử lý: <span className="font-bold text-teal-800">{p.orders} đơn</span> • Hợp đồng quản lý: <span className="font-bold text-blue-800">{p.contracts} HĐ</span>
                  </p>
                  <p className="text-xs text-slate-600 mt-0.5">Thời gian xử lý TB/đơn: {p.avgTime}</p>
                </div>
                <div className="text-right">
                  <span className="text-xs text-slate-500 block">Tỷ lệ chính xác:</span>
                  <span className="text-base font-extrabold text-emerald-600">{p.accuracy}%</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* TAB 2: SALES CONTRACT MANAGEMENT */}
      {activeTab === 'contract' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">📜 Danh Sách Hợp Đồng Bán Hàng (UC_CRM_106)</h2>
            <div className="space-y-3">
              {contracts.map((c) => {
                const valStatus = evaluateContractValidityStatus(c.endDate);
                return (
                  <div key={c.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-blue-100 text-blue-800">{c.code}</span>
                        <h3 className="font-bold text-slate-900">{c.title}</h3>
                      </div>
                      <p className="text-xs text-slate-500 mt-1">Khách hàng: {c.customer}</p>
                      <p className="text-xs text-slate-600 mt-0.5">
                        Thời hạn: {c.startDate} ➔ {c.endDate} (File đính kèm: {c.attachments})
                      </p>
                    </div>
                    <div className="text-right space-y-1">
                      <span className="text-sm font-extrabold text-slate-900 block">{c.value.toLocaleString('vi-VN')} VNĐ</span>
                      <span className={`inline-block px-2.5 py-0.5 text-xs font-bold rounded-full border ${valStatus.badgeClass}`}>
                        {valStatus.label}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Tạo Hợp Đồng Mới</h2>
            <form onSubmit={handleCreateContract} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Mã hợp đồng:</label>
                <input
                  type="text"
                  value={contractForm.code}
                  onChange={(e) => setContractForm({ ...contractForm, code: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: HD-2026-999"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên hợp đồng:</label>
                <input
                  type="text"
                  value={contractForm.title}
                  onChange={(e) => setContractForm({ ...contractForm, title: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Hợp đồng phân phối tổng hợp"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Khách hàng đứng tên:</label>
                <input
                  type="text"
                  value={contractForm.customer}
                  onChange={(e) => setContractForm({ ...contractForm, customer: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Đại lý Nông Sản Miền Tây"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Giá trị hợp đồng (VNĐ):</label>
                <input
                  type="number"
                  value={contractForm.value}
                  onChange={(e) => setContractForm({ ...contractForm, value: parseFloat(e.target.value) || 0 })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-teal-600 text-white rounded-lg font-semibold hover:bg-teal-700">
                Lưu Hợp Đồng
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 3: FILE ATTACHMENTS */}
      {activeTab === 'attachment' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">📎 Danh Sách File Đính Kèm Hợp Đồng (UC_CRM_107)</h2>
            <div className="space-y-3">
              {attachments.map((a) => (
                <div key={a.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-slate-200 text-slate-800">{a.type}</span>
                      <h3 className="font-bold text-slate-900">{a.fileName}</h3>
                    </div>
                    <p className="text-xs text-slate-500 mt-1">Thuộc HĐ: {a.contractCode} • Dung lượng: {formatContractFileSize(a.size)}</p>
                  </div>
                  <span className="px-3 py-1 text-xs font-semibold rounded-lg bg-teal-100 text-teal-800">
                    Đã tải lên
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Đính Kèm File Mới</h2>
            <form onSubmit={handleAttachFile} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên file đính kèm (Scan PDF/DOCX):</label>
                <input
                  type="text"
                  value={selectedFile}
                  onChange={(e) => setSelectedFile(e.target.value)}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: HopDong_PhuLuc_Signed.pdf"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700">
                Tải Lên File Hợp Đồng
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 4: CONTRACT VALIDITY & RENEWAL */}
      {activeTab === 'renewal' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">🔄 Theo Dõi Hiệu Lực & Tái Tục Hợp Đồng Bán Hàng (UC_CRM_108)</h2>
          <div className="space-y-3">
            {contracts.map((c) => {
              const statusBadge = evaluateContractValidityStatus(c.endDate);
              return (
                <div key={c.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{c.title} ({c.code})</h3>
                    <p className="text-xs text-slate-500 mt-1">Khách hàng: {c.customer} • Ngày hết hạn: {c.endDate}</p>
                  </div>
                  <div className="flex gap-3 items-center">
                    <span className={`px-3 py-1 text-xs font-bold rounded-full border ${statusBadge.badgeClass}`}>
                      {statusBadge.label}
                    </span>
                    {c.status !== 'Renewed' ? (
                      <button
                        onClick={() => handleRenewContract(c.id, c.code)}
                        className="px-3.5 py-1.5 bg-emerald-600 text-white text-xs font-bold rounded-lg hover:bg-emerald-700 shadow-sm"
                      >
                        🔄 Tái Tục Hợp Đồng
                      </button>
                    ) : (
                      <span className="px-3 py-1.5 bg-emerald-100 text-emerald-800 text-xs font-bold rounded-lg">
                        ✓ Đã tái tục năm 2027
                      </span>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
