'use client';

import React, { useState } from 'react';
import {
  evaluateCsatStars,
  formatOnlineOrderCode,
  validateLeadCaptureForm,
} from '@/shared/api/crm-chatbot-lead-csat-order-helpers';

export default function CrmChatbotLeadCsatOrderPage() {
  const [activeTab, setActiveTab] = useState<'lead' | 'handoff' | 'csat' | 'orders'>('lead');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: CHATBOT LEAD CAPTURE (UC_CRM_045)
  // ────────────────────────────────────────────────────────────────────────────
  const [botLeads, setBotLeads] = useState([
    { id: 'ld-1', customerName: 'Phạm Văn Hải', phone: '0909999888', email: 'hai.pham@gmail.com', note: 'Khách hỏi báo giá qua Zalo MiniApp', time: '10 phút trước' },
    { id: 'ld-2', customerName: 'Công ty TNHH Bách Khoa', phone: '0912345678', email: 'contact@bachkhoa.vn', note: 'Bot tự động ghi nhận nhu cầu ERP Cloud', time: '45 phút trước' },
  ]);

  const [leadForm, setLeadForm] = useState({ name: '', phone: '', email: '', note: '' });

  const handleCaptureLead = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateLeadCaptureForm(leadForm.name, leadForm.phone);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `ld-${Date.now()}`,
      customerName: leadForm.name,
      phone: leadForm.phone,
      email: leadForm.email || 'N/A',
      note: leadForm.note || 'Chatbot tự động ghi nhận',
      time: 'Vừa xong',
    };

    setBotLeads([created, ...botLeads]);
    setLeadForm({ name: '', phone: '', email: '', note: '' });
    showToast(`Đã ghi nhận Lead [${created.customerName}] từ Chatbot!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: BOT-TO-AGENT HANDOFF (UC_CRM_046)
  // ────────────────────────────────────────────────────────────────────────────
  const [botSessions, setBotSessions] = useState([
    { id: 'sess-1', customer: 'Lê Hoàng Nam (Zalo)', botState: 'Đang tương tác tự động', reason: 'Khách bấm chọn gặp Tư vấn viên' },
    { id: 'sess-2', customer: 'Nguyễn Thị Mai (Facebook)', botState: 'Đang thu thập nhu cầu', reason: 'Khách hỏi câu ngoài kịch bản 2 lần' },
  ]);

  const handleHandoff = (sessId: string, customer: string) => {
    setBotSessions((prev) => prev.filter((s) => s.id !== sessId));
    showToast(`Đã chuyển giao hội thoại của ${customer} cho Tư vấn viên thành công!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: CSAT EVALUATION (UC_CRM_048)
  // ────────────────────────────────────────────────────────────────────────────
  const [csatList] = useState([
    { id: 'csat-1', customer: 'Nguyễn Văn Nam', agent: 'Nguyễn Văn Sales 1', score: 5, text: 'Tư vấn nhiệt tình, giải đáp thỏa đáng.', time: 'Hôm nay' },
    { id: 'csat-2', customer: 'Vũ Thị Loan', agent: 'Trần Thị Agent 2', score: 4, text: 'Thái độ tốt, trả lời nhanh.', time: 'Hôm qua' },
    { id: 'csat-3', customer: 'Hoàng Anh Tuấn', agent: 'Phạm Văn CS 3', score: 2, text: 'Thời gian phản hồi ban đầu hơi lâu.', time: '2 ngày trước' },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: ONLINE ORDER INTAKE (UC_CRM_080)
  // ────────────────────────────────────────────────────────────────────────────
  const [onlineOrders] = useState([
    { id: 'ord-1', channel: 'Zalo MiniApp', code: '9912', customer: 'Nguyễn Thị Thu', phone: '0908123456', amount: 4500000, status: 'Received' },
    { id: 'ord-2', channel: 'Website Direct', code: '8841', customer: 'Công ty TNHH Hưng Thịnh', phone: '0912345678', amount: 18500000, status: 'Verified' },
  ]);

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
            <span className="bg-teal-500/30 text-teal-200 text-xs px-3 py-1 rounded-full font-semibold border border-teal-400/30">
              CRM - BOT LEAD CAPTURE, CSAT & ONLINE ORDERS
            </span>
            <h1 className="text-2xl font-bold mt-2">CRM Chatbot Lead Capture, CSAT Rating & Online Orders</h1>
            <p className="text-teal-100 text-sm mt-1">
              Chatbot thu thập Lead, Chuyển giao từ Bot sang Tư vấn viên, Đánh giá CSAT & Tiếp nhận Đơn hàng Online
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
            onClick={() => setActiveTab('lead')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'lead' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🤖 UC_CRM_045: Bot Thu Thập Lead
          </button>
          <button
            onClick={() => setActiveTab('handoff')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'handoff' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🔀 UC_CRM_046: Chuyển Bot sang Agent
          </button>
          <button
            onClick={() => setActiveTab('csat')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'csat' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            ⭐ UC_CRM_048: Đánh giá CSAT
          </button>
          <button
            onClick={() => setActiveTab('orders')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'orders' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🛒 UC_CRM_080: Đơn hàng Kênh Online
          </button>
        </div>
      </div>

      {/* TAB 1: BOT LEAD CAPTURE */}
      {activeTab === 'lead' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">🤖 Danh Sách Lead Thu Thập Bởi Chatbot (UC_CRM_045)</h2>
            <div className="space-y-3">
              {botLeads.map((l) => (
                <div key={l.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{l.customerName} ({l.phone})</h3>
                    <p className="text-xs text-slate-500 mt-1">Email: {l.email} • Thời gian: {l.time}</p>
                    <p className="text-xs text-teal-800 italic mt-1">"{l.note}"</p>
                  </div>
                  <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-teal-100 text-teal-800">
                    Lead Mới (Bot)
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Giả Lập Bot Thu Thập Lead</h2>
            <form onSubmit={handleCaptureLead} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên khách hàng:</label>
                <input
                  type="text"
                  value={leadForm.name}
                  onChange={(e) => setLeadForm({ ...leadForm, name: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Trần Thị B"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Số điện thoại:</label>
                <input
                  type="text"
                  value={leadForm.phone}
                  onChange={(e) => setLeadForm({ ...leadForm, phone: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="0908123456"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Email (nếu có):</label>
                <input
                  type="email"
                  value={leadForm.email}
                  onChange={(e) => setLeadForm({ ...leadForm, email: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-teal-600 text-white rounded-lg font-semibold hover:bg-teal-700">
                Lưu Lead Thu Thập
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 2: BOT HANDOFF */}
      {activeTab === 'handoff' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">🔀 Chuyển Giao Quyền Xử Lý Từ Bot Sang Agent Người Thật (UC_CRM_046)</h2>
          <div className="space-y-3">
            {botSessions.map((s) => (
              <div key={s.id} className="p-4 rounded-xl border border-amber-200 bg-amber-50/50 flex justify-between items-center">
                <div>
                  <h3 className="font-bold text-slate-900">{s.customer}</h3>
                  <p className="text-xs text-slate-500 mt-1">Trạng thái: {s.botState}</p>
                  <p className="text-xs text-amber-900 font-semibold mt-1">Lý do chuyển: {s.reason}</p>
                </div>
                <button
                  onClick={() => handleHandoff(s.id, s.customer)}
                  className="px-4 py-2 bg-emerald-600 text-white text-xs font-bold rounded-lg hover:bg-emerald-700 shadow-sm"
                >
                  👉 Tiếp Nhận Thay Bot
                </button>
              </div>
            ))}

            {botSessions.length === 0 && (
              <div className="p-8 text-center text-slate-400 text-sm">
                Không có cuộc chat nào từ Chatbot chờ tiếp nhận người thật.
              </div>
            )}
          </div>
        </div>
      )}

      {/* TAB 3: CSAT EVALUATION */}
      {activeTab === 'csat' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">⭐ Đánh Giá Chỉ Số Hài Lòng CSAT Khách Hàng (UC_CRM_048)</h2>
          <div className="space-y-3">
            {csatList.map((c) => {
              const starRes = evaluateCsatStars(c.score);
              return (
                <div key={c.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{c.customer}</h3>
                    <p className="text-xs text-slate-500 mt-1">
                      Tư vấn viên: <span className="font-semibold text-slate-700">{c.agent}</span> • Thời gian: {c.time}
                    </p>
                    <p className="text-xs text-slate-600 italic mt-1">"{c.text}"</p>
                  </div>
                  <div className="text-right">
                    <span className={`inline-block px-3 py-1 text-xs font-bold rounded-full border ${starRes.badgeClass}`}>
                      {starRes.starsDisplay}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* TAB 4: ONLINE ORDER INTAKE */}
      {activeTab === 'orders' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">🛒 Tiếp Nhận Đơn Hàng Tự Động Từ Kênh Online (UC_CRM_080)</h2>
          <div className="space-y-3">
            {onlineOrders.map((o) => (
              <div key={o.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="px-2 py-0.5 text-xs font-bold rounded bg-slate-200 text-slate-800">{o.channel}</span>
                    <h3 className="font-bold text-slate-900">{formatOnlineOrderCode(o.channel, o.code)}</h3>
                  </div>
                  <p className="text-xs text-slate-500 mt-1">Khách hàng: {o.customer} ({o.phone})</p>
                </div>
                <div className="text-right">
                  <span className="text-base font-extrabold text-slate-900 block">{o.amount.toLocaleString('vi-VN')} VNĐ</span>
                  <span className="inline-block mt-1 px-2.5 py-0.5 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">
                    Đã tiếp nhận
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
