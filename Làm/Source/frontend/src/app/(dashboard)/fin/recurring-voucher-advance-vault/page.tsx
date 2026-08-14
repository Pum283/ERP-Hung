'use client';

import React, { useState } from 'react';
import {
  formatAdvanceRefundSummary,
  formatFrequencyLabel,
} from '@/shared/api/fin-recurring-voucher-advance-vault-helpers';

export default function FinRecurringVoucherAdvanceVaultPage() {
  const [activeTab, setActiveTab] = useState<'recurring' | 'attachment' | 'advance' | 'vault'>('recurring');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FIN_011: Bút toán định kỳ / mẫu
  const [templates] = useState([
    { id: 't-1', code: 'TMPL-DEPR-OFFICE', name: 'Trích khấu hao tài sản cố định văn phòng định kỳ hàng tháng', freq: 'Monthly', amount: 35000000, debit: '6424', credit: '2141', active: true },
    { id: 't-2', code: 'TMPL-RENT-QUARTER', name: 'Phân bổ chi phí thuê văn phòng trụ sở định kỳ theo quý', freq: 'Quarterly', amount: 120000000, debit: '6427', credit: '242', active: true },
  ]);

  // UC_FIN_017: Đính kèm chứng từ gốc
  const [attachments] = useState([
    { id: 'att-1', vno: 'PKT-2026-0814', name: 'Hóa đơn giá trị gia tăng số 0001288 (PDF gốc)', url: '/uploads/fin/vouchers/inv-0001288-signed.pdf', mime: 'application/pdf', size: 850000, date: '2026-08-14' },
    { id: 'att-2', vno: 'PKT-2026-0814', name: 'Hợp đồng mua sắm vật tư ký kết 3 bên có dấu mộc đỏ', url: '/uploads/fin/vouchers/contract-mat-088.pdf', mime: 'application/pdf', size: 1450000, date: '2026-08-14' },
  ]);

  // UC_FIN_021: Đề nghị tạm ứng / hoàn ứng
  const [advances] = useState([
    { id: 'adv-1', reqNo: 'TU-2026-0814', name: 'Kỹ Sư Trưởng Nguyễn Văn An', purpose: 'Tạm ứng tiền vé máy bay và lưu trú công tác hiện trường dự án Solar FPT', adv: 15000000, set: 14200000, ref: 800000, status: 'Settled', date: '2026-08-14' },
  ]);

  // UC_FIN_022: Kiểm kê quỹ
  const [vaultAudits] = useState([
    { id: 'va-1', code: 'QUY-MAT-VND', name: 'Quỹ Tiền Mặt Trụ Sở Chính (VND)', book: 85200000, physical: 85200000, diff: 0, auditor: 'Kế Toán Trưởng & Thủ Quỹ', conclusion: 'Khớp đúng 100% giữa sổ quỹ tiền mặt và tiền mặt thực tế tại két sắt', date: '2026-08-14' },
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
              FIN - RECURRING JOURNALS, ATTACHMENTS, ADVANCES & CASH VAULT AUDIT
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Bút Toán Định Kỳ, Đính Kèm Hóa Đơn Gốc, Quản Lý Tạm Ứng & Kiểm Kê Quỹ</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Tự động sinh bút toán phân bổ/khấu hao định kỳ, lưu trữ hóa đơn VAT điện tử, kiểm soát quy trình quyết toán hoàn ứng và kiểm kê quỹ tiền mặt
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs FIN)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('recurring')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'recurring' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔄 UC_FIN_011: Bút Toán Định Kỳ
          </button>
          <button
            onClick={() => setActiveTab('attachment')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'attachment' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📎 UC_FIN_017: Chứng Từ Gốc
          </button>
          <button
            onClick={() => setActiveTab('advance')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'advance' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            💳 UC_FIN_021: Tạm Ứng / Hoàn Ứng
          </button>
          <button
            onClick={() => setActiveTab('vault')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'vault' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏦 UC_FIN_022: Kiểm Kê Quỹ
          </button>
        </div>
      </div>

      {activeTab === 'recurring' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🔄 Danh Mục Mẫu Bút Toán Định Kỳ Tự Động Hạch Toán (UC_FIN_011)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Mẫu</th>
                  <th className="p-3">Tên Mẫu Bút Toán Định Kỳ</th>
                  <th className="p-3 text-center">Tần Suất</th>
                  <th className="p-3 text-right">Số Tiền Mặc Định</th>
                  <th className="p-3 text-center">TK Nợ / TK Có</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {templates.map((t) => (
                  <tr key={t.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{t.code}</td>
                    <td className="p-3 font-semibold text-foreground">{t.name}</td>
                    <td className="p-3 text-center">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-blue-100 text-blue-800">
                        {formatFrequencyLabel(t.freq)}
                      </span>
                    </td>
                    <td className="p-3 text-right font-bold text-slate-800 font-mono">{t.amount.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-center font-mono font-bold text-slate-700">Nợ {t.debit} / Có {t.credit}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ● Tự Động Kích Hoạt
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'attachment' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📎 Hồ Sơ Đính Kèm Chứng Từ Gốc & Hóa Đơn VAT Điện Tử (UC_FIN_017)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Chứng Từ Sổ Cái</th>
                  <th className="p-3">Tên File Chứng Từ Gốc</th>
                  <th className="p-3 text-center">Định Dạng</th>
                  <th className="p-3 text-right">Dung Lượng</th>
                  <th className="p-3">Đường Dẫn File</th>
                  <th className="p-3 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {attachments.map((att) => (
                  <tr key={att.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{att.vno}</td>
                    <td className="p-3 font-bold text-foreground">{att.name}</td>
                    <td className="p-3 text-center font-mono text-xs text-rose-800 font-bold bg-rose-50 rounded">PDF</td>
                    <td className="p-3 text-right font-mono text-slate-700">{(att.size / 1024).toFixed(0)} KB</td>
                    <td className="p-3 font-mono text-xs text-muted-foreground">{att.url}</td>
                    <td className="p-3 text-right">
                      <button onClick={() => showToast(`Mở tài liệu [${att.name}]...`)} className="px-3 py-1 bg-brand text-brand-foreground text-xs font-bold rounded hover:opacity-90">
                        👁 Xem File
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'advance' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">💳 Hồ Sơ Đề Nghị Tạm Ứng & Bảng Kê Hoàn Ứng Quyết Toán (UC_FIN_021)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Phiếu Tạm Ứng</th>
                  <th className="p-3">Nhân Viên Đề Nghị</th>
                  <th className="p-3">Mục Đích Tạm Ứng</th>
                  <th className="p-3 text-right">Tóm Tắt Quyết Toán Hoàn Ứng</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {advances.map((adv) => (
                  <tr key={adv.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{adv.reqNo}</td>
                    <td className="p-3 font-bold text-foreground">{adv.name}</td>
                    <td className="p-3 text-xs text-slate-700">{adv.purpose}</td>
                    <td className="p-3 text-right font-mono text-xs font-bold text-emerald-700">
                      {formatAdvanceRefundSummary(adv.adv, adv.set, adv.ref)}
                    </td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Đã Quyết Toán Xong
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'vault' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🏦 Biên Bản Kiểm Kê Quỹ Tiền Mặt & Đối Chiếu Số Dư Sổ Cái (UC_FIN_022)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã & Tên Quỹ</th>
                  <th className="p-3 text-right">Số Dư Sổ Cái Kế Toán</th>
                  <th className="p-3 text-right">Tiền Mặt Thực Tế Két Sắt</th>
                  <th className="p-3 text-center">Chênh Lệch</th>
                  <th className="p-3">Hội Đồng Kiểm Kê</th>
                  <th className="p-3">Kết Luận</th>
                  <th className="p-3 text-right">Kết Quả</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {vaultAudits.map((va) => (
                  <tr key={va.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">
                      <span className="font-mono text-brand font-bold mr-2">{va.code}</span> {va.name}
                    </td>
                    <td className="p-3 text-right font-mono font-bold text-slate-800">{va.book.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-mono font-bold text-emerald-700">{va.physical.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-center font-mono font-bold text-slate-700">{va.diff} đ</td>
                    <td className="p-3 text-slate-800 font-medium">{va.auditor}</td>
                    <td className="p-3 text-xs text-muted-foreground">{va.conclusion}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Khớp Đúng 100%
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
