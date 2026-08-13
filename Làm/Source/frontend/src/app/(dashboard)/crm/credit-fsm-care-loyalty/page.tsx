'use client';

import React, { useState } from 'react';
import {
  evaluateCreditLimitStatus,
  formatLoyaltyPointsDisplay,
  validateFsmTicketHandoff,
} from '@/shared/api/crm-credit-fsm-care-loyalty-helpers';

export default function CrmCreditFsmCareLoyaltyPage() {
  const [activeTab, setActiveTab] = useState<'credit' | 'fsm' | 'care' | 'loyalty'>('credit');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: CREDIT LIMIT SALES BLOCKING (UC_CRM_111)
  // ────────────────────────────────────────────────────────────────────────────
  const [creditForm, setCreditForm] = useState({ customer: 'Đại lý Nông Sản Miền Tây', debt: 85000000, limit: 100000000, newOrderValue: 20000000 });
  const creditCheckResult = evaluateCreditLimitStatus(creditForm.debt, creditForm.limit, creditForm.newOrderValue);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: TICKET HANDOFF TO FSM (UC_CRM_114)
  // ────────────────────────────────────────────────────────────────────────────
  const [fsmTickets, setFsmTickets] = useState([
    { id: 'tck-1', code: 'TCK-FSM-991', customer: 'Đại lý Nông Sản Miền Tây', tech: 'Kỹ thuật viên Nguyễn Văn A', priority: 'High', status: 'TransferredToFsm', notes: 'Bảo trì máy phun phân bón tại trang trại Miền Tây' },
    { id: 'tck-2', code: 'TCK-FSM-882', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', tech: 'Kỹ thuật viên Phạm Văn B', priority: 'Normal', status: 'InProgress', notes: 'Sửa chữa hệ thống tủ đông' },
  ]);

  const [handoffForm, setHandoffForm] = useState({ ticketCode: '', techName: 'Kỹ thuật viên Nguyễn Văn A', notes: '' });

  const handleHandoffTicket = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateFsmTicketHandoff(handoffForm.ticketCode, handoffForm.techName);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `tck-${Date.now()}`,
      code: handoffForm.ticketCode,
      customer: 'Đại lý Nông Sản Miền Tây',
      tech: handoffForm.techName,
      priority: 'High',
      status: 'TransferredToFsm',
      notes: handoffForm.notes || 'Chuyển kiểm tra hiện trường',
    };

    setFsmTickets([created, ...fsmTickets]);
    setHandoffForm({ ticketCode: '', techName: 'Kỹ thuật viên Nguyễn Văn A', notes: '' });
    showToast(`Đã bàn giao Ticket [${created.code}] sang FSM!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: CUSTOMER CARE & REPURCHASE REMINDERS (UC_CRM_115)
  // ────────────────────────────────────────────────────────────────────────────
  const [careSchedules, setCareSchedules] = useState([
    { id: 'cs-1', customer: 'Đại lý Nông Sản Miền Tây', type: 'RepurchaseReminder', date: '2026-08-16', status: 'Pending', notes: 'Nhắc tái mua đợt hàng phân bón định kỳ 14 ngày' },
    { id: 'cs-2', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', type: 'PostServiceFollowUp', date: '2026-08-18', status: 'Pending', notes: 'Hỏi thăm mức độ hài lòng sau bảo trì' },
  ]);

  const [careForm, setCareForm] = useState({ customer: 'Đại lý Nông Sản Miền Tây', type: 'RepurchaseReminder', date: '2026-08-20', notes: '' });

  const handleScheduleCare = (e: React.FormEvent) => {
    e.preventDefault();
    const created = {
      id: `cs-${Date.now()}`,
      customer: careForm.customer,
      type: careForm.type,
      date: careForm.date,
      status: 'Pending',
      notes: careForm.notes || 'Lịch chăm sóc định kỳ',
    };

    setCareSchedules([created, ...careSchedules]);
    setCareForm({ customer: 'Đại lý Nông Sản Miền Tây', type: 'RepurchaseReminder', date: '2026-08-20', notes: '' });
    showToast(`Đã lên lịch chăm sóc cho [${created.customer}]!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: LOYALTY PROGRAMS (UC_CRM_116)
  // ────────────────────────────────────────────────────────────────────────────
  const [loyaltyPrograms] = useState([
    { id: 'loy-1', code: 'LOYALTY-GOLD-2026', name: 'Chương Trình Khách Hàng Thân Thiết Gold', rate: '1000 VNĐ = 1 điểm', minPoints: 100, active: true, enrolled: 142 },
    { id: 'loy-2', code: 'LOYALTY-AGRI-PRO', name: 'Chương Trình Đối Tác Nông Nghiệp Thâm Niên', rate: '1000 VNĐ = 2 điểm', minPoints: 200, active: true, enrolled: 88 },
  ]);

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
      <div className="bg-gradient-to-r from-rose-950 via-slate-900 to-indigo-950 p-6 rounded-2xl text-white shadow-xl">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-rose-500/30 text-rose-200 text-xs px-3 py-1 rounded-full font-semibold border border-rose-400/30">
              CRM - CREDIT GUARD, FSM HANDOFF, CARE & LOYALTY
            </span>
            <h1 className="text-2xl font-bold mt-2">Bước 175: CRM Kiểm Soát Công Nợ, Chuyển FSM, Chăm Sóc KH & Loyalty</h1>
            <p className="text-rose-100 text-sm mt-1">
              Chặn bán vượt công nợ, Bàn giao Ticket sang FSM, Lịch chăm sóc nhắc tái mua & Chương trình Loyalty
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
            onClick={() => setActiveTab('credit')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'credit' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🛑 UC_CRM_111: Chặn Bán Vượt Công Nợ
          </button>
          <button
            onClick={() => setActiveTab('fsm')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'fsm' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🛠️ UC_CRM_114: Chuyển Ticket Sang FSM
          </button>
          <button
            onClick={() => setActiveTab('care')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'care' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📅 UC_CRM_115: Chăm Sóc & Nhắc Tái Mua
          </button>
          <button
            onClick={() => setActiveTab('loyalty')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'loyalty' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🎁 UC_CRM_116: Chương Trình Loyalty
          </button>
        </div>
      </div>

      {/* TAB 1: CREDIT LIMIT SALES BLOCKING */}
      {activeTab === 'credit' && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">🛑 Kiểm Tra & Chặn Đơn Vượt Hạn Mức (UC_CRM_111)</h2>
            <div className="space-y-3 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Khách hàng:</label>
                <input type="text" value={creditForm.customer} disabled className="w-full bg-slate-100 border border-slate-300 rounded-lg p-2 font-semibold" />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Dư nợ hiện tại (VNĐ):</label>
                  <input
                    type="number"
                    value={creditForm.debt}
                    onChange={(e) => setCreditForm({ ...creditForm, debt: parseFloat(e.target.value) || 0 })}
                    className="w-full border border-slate-300 rounded-lg p-2"
                  />
                </div>
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Hạn mức công nợ (VNĐ):</label>
                  <input
                    type="number"
                    value={creditForm.limit}
                    onChange={(e) => setCreditForm({ ...creditForm, limit: parseFloat(e.target.value) || 0 })}
                    className="w-full border border-slate-300 rounded-lg p-2"
                  />
                </div>
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Giá trị đơn hàng mới (VNĐ):</label>
                <input
                  type="number"
                  value={creditForm.newOrderValue}
                  onChange={(e) => setCreditForm({ ...creditForm, newOrderValue: parseFloat(e.target.value) || 0 })}
                  className="w-full border border-slate-300 rounded-lg p-2 font-bold text-indigo-900"
                />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 flex flex-col justify-between">
            <div>
              <h2 className="text-lg font-bold text-slate-800 mb-4">⚙️ Kết Quả Thẩm Định Hệ Thống</h2>
              <div className={`p-4 rounded-xl border ${creditCheckResult.badgeClass} mb-4`}>
                <p className="font-bold text-base">{creditCheckResult.label}</p>
              </div>
              <div className="space-y-2 text-sm text-slate-700">
                <p>• Dư nợ hiện tại: <span className="font-bold">{creditForm.debt.toLocaleString('vi-VN')} VNĐ</span></p>
                <p>• Giá trị đơn mới: <span className="font-bold text-indigo-900">{creditForm.newOrderValue.toLocaleString('vi-VN')} VNĐ</span></p>
                <p>• Tổng nợ dự kiến: <span className="font-bold text-rose-900">{(creditForm.debt + creditForm.newOrderValue).toLocaleString('vi-VN')} VNĐ</span></p>
                <p>• Hạn mức tối đa: <span className="font-bold text-emerald-900">{creditForm.limit.toLocaleString('vi-VN')} VNĐ</span></p>
              </div>
            </div>
            <button
              onClick={() => showToast(creditCheckResult.isBlocked ? '🛑 Hệ thống đã tự động chặn tạo đơn vượt hạn mức!' : '✓ Đơn hàng đủ điều kiện tạo!', creditCheckResult.isBlocked ? 'error' : 'success')}
              className={`w-full py-3 rounded-lg font-bold text-white shadow-md transition-all ${
                creditCheckResult.isBlocked ? 'bg-rose-600 hover:bg-rose-700' : 'bg-emerald-600 hover:bg-emerald-700'
              }`}
            >
              {creditCheckResult.isBlocked ? 'Thực Hiện Chặn Đơn Hàng' : 'Cho Phép Khởi Tạo Đơn'}
            </button>
          </div>
        </div>
      )}

      {/* TAB 2: TICKET HANDOFF TO FSM */}
      {activeTab === 'fsm' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">🛠️ Danh Sách Ticket Chuyển Bàn Giao FSM (UC_CRM_114)</h2>
            <div className="space-y-3">
              {fsmTickets.map((t) => (
                <div key={t.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-rose-100 text-rose-800">{t.priority}</span>
                      <h3 className="font-bold text-slate-900">{t.code} - {t.customer}</h3>
                    </div>
                    <p className="text-xs text-slate-500 mt-1">Kỹ thuật viên FSM: {t.tech}</p>
                    <p className="text-xs text-slate-700 italic mt-0.5">"{t.notes}"</p>
                  </div>
                  <span className="px-3 py-1 text-xs font-semibold rounded-lg bg-indigo-100 text-indigo-800">
                    {t.status === 'TransferredToFsm' ? 'Đã bàn giao FSM' : 'Đang xử lý'}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Bàn Giao Ticket Sang FSM</h2>
            <form onSubmit={handleHandoffTicket} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Mã Ticket CRM:</label>
                <input
                  type="text"
                  value={handoffForm.ticketCode}
                  onChange={(e) => setHandoffForm({ ...handoffForm, ticketCode: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: TCK-FSM-999"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Kỹ thuật viên FSM tiếp nhận:</label>
                <input
                  type="text"
                  value={handoffForm.techName}
                  onChange={(e) => setHandoffForm({ ...handoffForm, techName: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Ghi chú yêu cầu kỹ thuật:</label>
                <textarea
                  value={handoffForm.notes}
                  onChange={(e) => setHandoffForm({ ...handoffForm, notes: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  rows={2}
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-indigo-600 text-white rounded-lg font-semibold hover:bg-indigo-700">
                Chuyển Ticket Sang FSM
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 3: CUSTOMER CARE & REPURCHASE REMINDERS */}
      {activeTab === 'care' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">📅 Lịch Chăm Sóc & Nhắc Tái Mua Khách Hàng (UC_CRM_115)</h2>
            <div className="space-y-3">
              {careSchedules.map((cs) => (
                <div key={cs.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{cs.customer}</h3>
                    <p className="text-xs text-slate-500 mt-1">Loại chăm sóc: {cs.type === 'RepurchaseReminder' ? 'Nhắc tái mua định kỳ' : 'Hỏi thăm sau dịch vụ'}</p>
                    <p className="text-xs text-slate-700 italic mt-0.5">"{cs.notes}"</p>
                  </div>
                  <div className="text-right">
                    <span className="text-xs font-bold text-indigo-900 block">Ngày hẹn: {cs.date}</span>
                    <span className="px-2.5 py-0.5 text-xs font-semibold rounded bg-amber-100 text-amber-800 inline-block mt-1">
                      {cs.status}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Lên Lịch Chăm Sóc</h2>
            <form onSubmit={handleScheduleCare} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên khách hàng:</label>
                <input
                  type="text"
                  value={careForm.customer}
                  onChange={(e) => setCareForm({ ...careForm, customer: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Ngày thực hiện:</label>
                <input
                  type="date"
                  value={careForm.date}
                  onChange={(e) => setCareForm({ ...careForm, date: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Ghi chú nội dung chăm sóc:</label>
                <textarea
                  value={careForm.notes}
                  onChange={(e) => setCareForm({ ...careForm, notes: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  rows={2}
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-teal-600 text-white rounded-lg font-semibold hover:bg-teal-700">
                Lưu Lịch Chăm Sóc
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 4: LOYALTY PROGRAMS */}
      {activeTab === 'loyalty' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">🎁 Quản Lý Chương Trình Loyalty & Tích Điểm (UC_CRM_116)</h2>
          <div className="space-y-3">
            {loyaltyPrograms.map((loy) => (
              <div key={loy.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="px-2 py-0.5 text-xs font-bold rounded bg-amber-100 text-amber-800">{loy.code}</span>
                    <h3 className="font-bold text-slate-900">{loy.name}</h3>
                  </div>
                  <p className="text-xs text-slate-500 mt-1">Tỷ lệ quy đổi: <span className="font-semibold text-teal-800">{loy.rate}</span></p>
                  <p className="text-xs text-slate-600 mt-0.5">Điểm tối thiểu đổi quà: {loy.minPoints} pts</p>
                </div>
                <div className="text-right">
                  <span className="text-xs text-slate-500 block">Số KH tham gia:</span>
                  <span className="text-base font-extrabold text-indigo-700">{formatLoyaltyPointsDisplay(loy.enrolled)}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
