'use client';

import React, { useState } from 'react';
import {
  formatOffsetBalanceSummary,
  formatDunningLevelBadge,
} from '@/shared/api/fin-statement-offset-dunning-bad-debt-helpers';

export default function FinStatementOffsetDunningBadDebtPage() {
  const [activeTab, setActiveTab] = useState<'statement' | 'offset' | 'dunning' | 'baddebt'>('statement');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_FIN_028: Import sao kê
  const [statementImports] = useState([
    { id: 'st-1', acc: '190388889999', bank: 'Techcombank', file: 'VCB_Statement_202608.xlsx', txCount: 48, credit: 450000000, debit: 280000000, status: 'Success', date: '2026-08-14' },
  ]);

  // UC_FIN_033: Bù trừ công nợ
  const [offsets] = useState([
    { id: 'off-1', no: 'BT-2026-0814', partner: 'Công Ty TNHH Thiết Bị Điện Miền Nam (Vừa là NCC vừa là Khách hàng)', ar: 65000000, ap: 65000000, net: 0, voucher: 'PKT-BT-0012', status: 'Approved', date: '2026-08-14' },
  ]);

  // UC_FIN_034: Nhắc nợ tự động
  const [dunnings] = useState([
    { id: 'dun-1', inv: 'INV-2026-0814', cust: 'Công Ty CP Xây Lắp Điện Hải Phòng', amount: 42500000, days: 15, level: 'Level1_Reminder', channel: 'Email', contact: 'ketoan@haiphong-power.vn', date: '2026-08-14' },
    { id: 'dun-2', inv: 'INV-2026-0720', cust: 'Công Ty CP Năng Lượng Xanh', amount: 88000000, days: 45, level: 'Level2_Warning', channel: 'SMS + Email', contact: 'giamdoc@greenenergy.vn', date: '2026-08-14' },
  ]);

  // UC_FIN_037: Xử lý nợ khó đòi
  const [badDebts] = useState([
    { id: 'bd-1', no: 'NX-2026-0814', cust: 'Công Ty Cơ Khí Hoàng Gia (Đã giải thể)', origin: 30000000, prov: 30000000, rate: 100.0, action: 'WriteOff', doc: 'Nghị quyết HĐQT số 18/2026/NQ-HDQT duyệt xóa nợ xấu', date: '2026-08-14' },
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
              FIN - BANK STATEMENT IMPORT, AR/AP OFFSET, DUNNING & BAD DEBT WRITE-OFF
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Import Sao Kê Ngân Hàng, Bù Trừ Công Nợ Hai Chiều, Nhắc Nợ Tự Động & Xử Lý Nợ Xấu</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Tự động hóa đối soát sao kê Excel/CSV, lập biên bản cấn trừ AR-AP cùng đối tác, gửi email/SMS nhắc nợ đa cấp và trích lập dự phòng/xóa sổ nợ khó đòi
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
            onClick={() => setActiveTab('statement')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'statement' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📂 UC_FIN_028: Import Sao Kê
          </button>
          <button
            onClick={() => setActiveTab('offset')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'offset' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚖️ UC_FIN_033: Bù Trừ AR/AP
          </button>
          <button
            onClick={() => setActiveTab('dunning')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'dunning' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔔 UC_FIN_034: Nhắc Nợ Tự Động
          </button>
          <button
            onClick={() => setActiveTab('baddebt')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'baddebt' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⚠️ UC_FIN_037: Xử Lý Nợ Xấu
          </button>
        </div>
      </div>

      {activeTab === 'statement' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📂 Lịch Sử Import File Sao Kê Ngân Hàng Điện Tử (UC_FIN_028)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Tài Khoản</th>
                  <th className="p-3">Ngân Hàng</th>
                  <th className="p-3">Tên File Import</th>
                  <th className="p-3 text-center">Số Giao Dịch</th>
                  <th className="p-3 text-right">Tổng Tiền Thu (Credit)</th>
                  <th className="p-3 text-right">Tổng Tiền Chi (Debit)</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {statementImports.map((st) => (
                  <tr key={st.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{st.acc}</td>
                    <td className="p-3 font-semibold text-foreground">{st.bank}</td>
                    <td className="p-3 font-mono text-xs text-slate-800">{st.file}</td>
                    <td className="p-3 text-center font-bold">{st.txCount} GD</td>
                    <td className="p-3 text-right font-mono font-bold text-emerald-700">+{st.credit.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-mono font-bold text-rose-700">-{st.debit.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Thành Công
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'offset' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⚖️ Biên Bản & Bút Toán Bù Trừ Công Nợ Phải Thu - Phải Trả (UC_FIN_033)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Biên Bản</th>
                  <th className="p-3">Đối Tác Bù Trừ (Khách Hàng / NCC)</th>
                  <th className="p-3 text-right">Tổng Hợp Đối Trừ Công Nợ</th>
                  <th className="p-3 font-mono">Chứng Từ Sổ Cái</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {offsets.map((off) => (
                  <tr key={off.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{off.no}</td>
                    <td className="p-3 font-bold text-foreground">{off.partner}</td>
                    <td className="p-3 text-right font-mono text-xs font-bold text-slate-800">
                      {formatOffsetBalanceSummary(off.ar, off.ap, off.net)}
                    </td>
                    <td className="p-3 font-mono text-xs text-blue-700 font-bold">{off.voucher}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Đã Cấn Trừ
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'dunning' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">🔔 Nhật Ký Gửi Thông Báo Nhắc Nợ Tự Động Đa Kênh (UC_FIN_034)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Số Hóa Đơn</th>
                  <th className="p-3">Khách Hàng Nợ</th>
                  <th className="p-3 text-right">Số Tiền Quá Hạn</th>
                  <th className="p-3 text-center">Quá Hạn</th>
                  <th className="p-3 text-center">Cấp Độ Nhắc</th>
                  <th className="p-3">Kênh & Địa Chỉ Gửi</th>
                  <th className="p-3 text-right">Thời Gian Gửi</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {dunnings.map((d) => (
                  <tr key={d.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{d.inv}</td>
                    <td className="p-3 font-bold text-foreground">{d.cust}</td>
                    <td className="p-3 text-right font-mono font-bold text-rose-700">{d.amount.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-center font-bold text-rose-600">{d.days} ngày</td>
                    <td className="p-3 text-center">
                      <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${formatDunningLevelBadge(d.level)}`}>
                        {d.level === 'Level1_Reminder' ? 'Cấp 1: Nhắc Nhở' : 'Cấp 2: Cảnh Báo'}
                      </span>
                    </td>
                    <td className="p-3 text-xs text-slate-700">
                      <span className="font-bold text-slate-800 mr-1">[{d.channel}]</span> {d.contact}
                    </td>
                    <td className="p-3 text-right text-xs text-muted-foreground">{d.date}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'baddebt' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⚠️ Xử Lý Trích Lập Dự Phòng & Xóa Sổ Nợ Khó Đòi (UC_FIN_037)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Hồ Sơ Xử Lý</th>
                  <th className="p-3">Khách Hàng Nợ Xấu</th>
                  <th className="p-3 text-right">Khoản Nợ Gốc</th>
                  <th className="p-3 text-right">Số Tiền Trích Lập / Xóa</th>
                  <th className="p-3 text-center">Tỷ Lệ</th>
                  <th className="p-3">Căn Cứ Pháp Lý & Quyết Định</th>
                  <th className="p-3 text-right">Biện Pháp</th>
                </tr>
              </thead>
              <tbody className="divide-y border-border">
                {badDebts.map((bd) => (
                  <tr key={bd.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{bd.no}</td>
                    <td className="p-3 font-bold text-foreground">{bd.cust}</td>
                    <td className="p-3 text-right font-mono font-bold text-slate-800">{bd.origin.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-right font-mono font-black text-rose-700">{bd.prov.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 text-center font-bold text-rose-800">{bd.rate}%</td>
                    <td className="p-3 text-xs text-muted-foreground">{bd.doc}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-rose-100 text-rose-800 border border-rose-300">
                        ✗ Xóa Sổ Nợ Xấu
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
