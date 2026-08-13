'use client';

import React, { useState } from 'react';
import {
  checkPricelistActiveValidity,
  validateBatchImportSupplierRows,
} from '@/shared/api/pur-blacklist-import-legal-pricelist-helpers';

export default function PurBlacklistImportLegalPricelistPage() {
  const [activeTab, setActiveTab] = useState<'blacklist' | 'import' | 'legal' | 'pricelist'>('blacklist');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PUR_006: Blacklist / Ngưng dùng
  const [blacklistedSuppliers, setBlacklistedSuppliers] = useState([
    { id: 'b-1', code: 'SUP-099', name: 'Công Ty Nông Sản Kém Chất Lượng', reason: 'Tỷ lệ hàng giao lỗi quá cao > 8%, vi phạm cam kết hợp đồng', date: '01/08/2026', status: 'Blacklisted' },
  ]);

  const [blForm, setBlForm] = useState({ code: '', name: '', reason: '' });

  const handleAddBlacklist = (e: React.FormEvent) => {
    e.preventDefault();
    if (!blForm.code || !blForm.reason) {
      showToast('Vui lòng nhập đầy đủ Mã NCC và Lý do ngưng dùng.', 'error');
      return;
    }

    const created = {
      id: `b-${Date.now()}`,
      code: blForm.code,
      name: blForm.name || 'Nhà Cung Cấp Hạn Chế',
      reason: blForm.reason,
      date: new Date().toLocaleDateString('vi-VN'),
      status: 'Blacklisted',
    };

    setBlacklistedSuppliers([...blacklistedSuppliers, created]);
    setBlForm({ code: '', name: '', reason: '' });
    showToast(`✓ Đã đưa nhà cung cấp [${created.code}] vào danh sách Blacklist ngưng giao dịch!`, 'success');
  };

  // UC_PUR_007: Import danh sách NCC
  const [importRows] = useState([
    { supplierCode: 'SUP-010', supplierName: 'Công Ty Nông Sản Sạch An Giang', taxCode: '0312999888', phone: '0909123456' },
    { supplierCode: 'SUP-011', supplierName: 'Công Ty Bao Bì Giấy Hùng Phát', taxCode: '0312777666', phone: '0908765432' },
  ]);

  const importVal = validateBatchImportSupplierRows(importRows);

  const handleExecuteImport = () => {
    showToast(`✓ Đã import thành công ${importVal.validCount}/${importVal.totalCount} bản ghi nhà cung cấp vào hệ thống!`, 'success');
  };

  // UC_PUR_008: Hồ sơ pháp lý NCC
  const [legalDocs] = useState([
    { id: 'doc-1', type: 'Giấy ĐKKD', number: 'GPKD-0312345678', issued: '15/01/2022', expires: '15/01/2030', status: 'Valid', file: 'gpkd_vinamilk.pdf' },
    { id: 'doc-2', type: 'Chứng Nhận ATVSTP', number: 'ATTP-2024-9988', issued: '10/05/2024', expires: '10/05/2027', status: 'Valid', file: 'attp_cert.pdf' },
  ]);

  // UC_PUR_011: Hiệu lực bảng giá mua
  const [pricelists] = useState([
    { id: 'pl-1', code: 'PL-PUR-2026-Q3', name: 'Bảng giá mua Quý 3/2026', from: '2026-07-01', to: '2026-09-30', active: true },
    { id: 'pl-2', code: 'PL-PUR-2026-H1', name: 'Bảng giá mua 6 tháng đầu năm', from: '2026-01-01', to: '2026-06-30', active: false },
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
              PUR - BLACKLIST, BATCH IMPORT, LEGAL DOSSIERS & PRICELISTS
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Blacklist NCC, Import Hàng Loạt, Hồ Sơ Pháp Lý & Bảng Giá Mua</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý danh sách đen ngưng giao dịch, import hàng loạt dữ liệu NCC, theo dõi hạn hồ sơ pháp lý và thời gian hiệu lực bảng giá mua
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('blacklist')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'blacklist' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🚫 UC_PUR_006: Blacklist & Ngưng Dùng NCC
          </button>
          <button
            onClick={() => setActiveTab('import')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'import' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📥 UC_PUR_007: Import Hàng Loạt Nhà Cung Cấp
          </button>
          <button
            onClick={() => setActiveTab('legal')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'legal' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📑 UC_PUR_008: Hồ Sơ Pháp Lý Nhà Cung Cấp
          </button>
          <button
            onClick={() => setActiveTab('pricelist')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'pricelist' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📅 UC_PUR_011: Hiệu Lực Bảng Giá Mua
          </button>
        </div>
      </div>

      {activeTab === 'blacklist' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">🚫 Danh Sách Nhà Cung Cấp Bị Ngưng Dùng / Blacklist (UC_PUR_006)</h2>
            <div className="space-y-3">
              {blacklistedSuppliers.map((b) => (
                <div key={b.id} className="p-4 rounded-xl border border-rose-200 bg-rose-50/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-rose-200 text-rose-800">{b.code}</span>
                      <h3 className="font-bold text-foreground">{b.name}</h3>
                    </div>
                    <p className="text-xs text-rose-700 mt-1 font-medium">Lý do: {b.reason}</p>
                  </div>
                  <span className="px-2 py-0.5 text-xs font-bold rounded bg-rose-600 text-white">
                    {b.status} ({b.date})
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">➕ Khóa / Đưa NCC Vào Blacklist</h2>
            <form onSubmit={handleAddBlacklist} className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã NCC khóa:</label>
                <input
                  type="text"
                  value={blForm.code}
                  onChange={(e) => setBlForm({ ...blForm, code: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: SUP-099"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên nhà cung cấp:</label>
                <input
                  type="text"
                  value={blForm.name}
                  onChange={(e) => setBlForm({ ...blForm, name: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="Tên NCC"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Lý do ngưng hợp tác:</label>
                <textarea
                  rows={3}
                  value={blForm.reason}
                  onChange={(e) => setBlForm({ ...blForm, reason: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="Ghi rõ vi phạm hợp đồng hoặc chất lượng hàng hóa..."
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-rose-600 text-white rounded-lg font-semibold hover:bg-rose-700">
                🚫 Khóa & Đưa Vào Blacklist
              </button>
            </form>
          </div>
        </div>
      )}

      {activeTab === 'import' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-bold text-foreground">📥 Import Danh Sách Nhà Cung Cấp Từ File Excel/CSV (UC_PUR_007)</h2>
              <p className="text-xs text-muted-foreground mt-0.5">Hỗ trợ kiểm tra và đẩy hàng loạt danh sách NCC vào hệ thống ERP</p>
            </div>
            <button
              onClick={handleExecuteImport}
              className="px-4 py-2 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm"
            >
              📥 Thực Hiện Import ({importVal.validCount} Dòng Hợp Lệ)
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã NCC</th>
                  <th className="p-3">Tên Nhà Cung Cấp</th>
                  <th className="p-3">Mã Số Thuế</th>
                  <th className="p-3">Số Điện Thoại</th>
                  <th className="p-3 text-right">Trạng Thái Kiểm Trả</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {importRows.map((r, idx) => (
                  <tr key={idx} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{r.supplierCode}</td>
                    <td className="p-3 font-semibold text-slate-700">{r.supplierName}</td>
                    <td className="p-3 font-mono text-xs">{r.taxCode}</td>
                    <td className="p-3 text-slate-600">{r.phone}</td>
                    <td className="p-3 text-right">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-emerald-100 text-emerald-800">
                        ✓ Hợp lệ
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'legal' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📑 Quản Lý Hồ Sơ Pháp Lý & Giấy Phép Nhà Cung Cấp (UC_PUR_008)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Loại Giấy Tờ</th>
                  <th className="p-3">Số Đăng Ký / Văn Bản</th>
                  <th className="p-3">Ngày Cấp</th>
                  <th className="p-3">Ngày Hết Hạn</th>
                  <th className="p-3">Tệp Đính Kèm</th>
                  <th className="p-3 text-right">Trạng Thái Hiệu Lực</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {legalDocs.map((d) => (
                  <tr key={d.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{d.type}</td>
                    <td className="p-3 font-mono text-xs text-brand-strong">{d.number}</td>
                    <td className="p-3 text-slate-600">{d.issued}</td>
                    <td className="p-3 text-slate-600">{d.expires}</td>
                    <td className="p-3">
                      <a href="#" className="text-brand hover:underline font-medium text-xs">📎 {d.file}</a>
                    </td>
                    <td className="p-3 text-right">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-emerald-100 text-emerald-800">
                        {d.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'pricelist' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📅 Quản Lý Thời Gian Hiệu Lực Bảng Giá Mua (UC_PUR_011)</h2>
          <div className="space-y-3">
            {pricelists.map((pl) => {
              const check = checkPricelistActiveValidity(pl.from, pl.to);
              return (
                <div key={pl.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{pl.code}</span>
                      <h3 className="font-bold text-foreground">{pl.name}</h3>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">
                      Thời hạn: Từ <b className="text-foreground">{pl.from}</b> đến <b className="text-foreground">{pl.to}</b>
                    </p>
                  </div>
                  <span className={`px-2 py-0.5 text-xs font-bold rounded ${check.isActive ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'}`}>
                    {check.isActive ? '● Đang Hiệu Lực' : check.isExpired ? 'Hết Hạn' : 'Chưa Đến Hạn'}
                  </span>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
