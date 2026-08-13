'use client';

import React, { useState } from 'react';
import {
  evaluateSlaStatus,
  validateRoutingRule,
  parseBotFlowSteps,
} from '@/shared/api/crm-omnichannel-routing-sla-helpers';

export default function CrmOmnichannelRoutingSlaPage() {
  const [activeTab, setActiveTab] = useState<'rules' | 'transfer' | 'sla' | 'bot'>('rules');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: ROUTING RULES (UC_CRM_041)
  // ────────────────────────────────────────────────────────────────────────────
  const [routingRules, setRoutingRules] = useState([
    { id: 'r-1', ruleName: 'Phân phối Xoay vòng Zalo Sales', strategy: 'RoundRobin', group: 'Sales_Zalo', priority: 1, isActive: true },
    { id: 'r-2', ruleName: 'Cân bằng tải Khách Facebook', strategy: 'LoadBalance', group: 'Sales_FB', priority: 2, isActive: true },
    { id: 'r-3', ruleName: 'Phân phối theo kỹ năng Chuyên môn ERP', strategy: 'SkillBased', group: 'ERP_Consultants', priority: 3, isActive: true },
  ]);

  const [ruleForm, setRuleForm] = useState({ ruleName: '', strategy: 'RoundRobin', group: 'Sales_Zalo', priority: 1 });

  const handleCreateRule = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateRoutingRule(ruleForm.ruleName, ruleForm.strategy);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `r-${Date.now()}`,
      ruleName: ruleForm.ruleName,
      strategy: ruleForm.strategy,
      group: ruleForm.group,
      priority: ruleForm.priority,
      isActive: true,
    };

    setRoutingRules([...routingRules, created]);
    setRuleForm({ ruleName: '', strategy: 'RoundRobin', group: 'Sales_Zalo', priority: 1 });
    showToast(`Đã khởi tạo Quy tắc phân phối [${created.ruleName}] thành công!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: AGENT CONVERSATION TRANSFER (UC_CRM_042)
  // ────────────────────────────────────────────────────────────────────────────
  const [activeSessions] = useState([
    { id: 'cv-101', customerName: 'Trần Thanh Tâm (Zalo)', currentAgent: 'Nguyễn Văn Sales 1', channel: 'Zalo' },
    { id: 'cv-102', customerName: 'Lê Minh Tuấn (Facebook)', currentAgent: 'Phạm Thị Agent 2', channel: 'Facebook' },
  ]);

  const [transferForm, setTransferForm] = useState({ convId: 'cv-101', targetAgent: 'Trần Văn Specialist', note: '' });

  const handleTransfer = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`Đã chuyển giao hội thoại sang [${transferForm.targetAgent}] kèm ghi chú!`, 'success');
    setTransferForm({ ...transferForm, note: '' });
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: SLA RESPONSE & ALERTS (UC_CRM_043)
  // ────────────────────────────────────────────────────────────────────────────
  const [slaLogs] = useState([
    { id: 'sla-1', customer: 'Nguyễn Văn Nam (0908123456)', maxMins: 5, actualMins: 2, agent: 'Sales 1' },
    { id: 'sla-2', customer: 'Vũ Thị Loan (0912345678)', maxMins: 5, actualMins: 4, agent: 'Sales 2' },
    { id: 'sla-3', customer: 'Hoàng Anh Tuấn (0987654321)', maxMins: 5, actualMins: 14, agent: 'Sales 3' },
  ]);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: SCRIPTED CHATBOT (UC_CRM_044)
  // ────────────────────────────────────────────────────────────────────────────
  const [botFlows] = useState([
    {
      id: 'flow-1',
      name: 'Kịch bản Chào mừng Khách Zalo OA',
      keyword: '#chao',
      json: '[{"step":1,"action":"send_msg","text":"Chào bạn! Cảm ơn bạn đã nhắn tin cho ERP Hùng."},{"step":2,"action":"ask_option","text":"Bạn cần hỗ trợ dịch vụ nào?"}]',
    },
    {
      id: 'flow-2',
      name: 'Kịch bản Tự động Nhận báo giá ERP Cloud',
      keyword: '#baogia',
      json: '[{"step":1,"action":"send_msg","text":"Dạ vui lòng để lại SĐT để nhận bảng giá chi tiết."}]',
    },
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
      <div className="bg-gradient-to-r from-slate-900 via-indigo-950 to-blue-950 p-6 rounded-2xl text-white shadow-xl">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-blue-500/30 text-blue-200 text-xs px-3 py-1 rounded-full font-semibold border border-blue-400/30">
              CRM - CHAT ROUTING, SLA & SCRIPTED BOT
            </span>
            <h1 className="text-2xl font-bold mt-2">Bước 169: CRM Omnichannel Routing, SLA & Scripted Chatbot</h1>
            <p className="text-blue-100 text-sm mt-1">
              Phân phối hội thoại theo Rule, Chuyển giao cuộc chat, Cảnh báo vi phạm SLA & Kịch bản Bot tự động
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
            onClick={() => setActiveTab('rules')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'rules' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🔀 UC_CRM_041: Phân phối theo Rule
          </button>
          <button
            onClick={() => setActiveTab('transfer')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'transfer' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🔄 UC_CRM_042: Chuyển hội thoại Agent
          </button>
          <button
            onClick={() => setActiveTab('sla')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'sla' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            ⏱️ UC_CRM_043: SLA & Cảnh báo trễ
          </button>
          <button
            onClick={() => setActiveTab('bot')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'bot' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🤖 UC_CRM_044: Chatbot kịch bản
          </button>
        </div>
      </div>

      {/* TAB 1: ROUTING RULES */}
      {activeTab === 'rules' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">🔀 Quy Tắc Phân Phối Hội Thoại Tự Động (UC_CRM_041)</h2>
            <div className="space-y-3">
              {routingRules.map((r) => (
                <div key={r.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-blue-100 text-blue-800">Ưu tiên #{r.priority}</span>
                      <h3 className="font-bold text-slate-900">{r.ruleName}</h3>
                    </div>
                    <p className="text-xs text-slate-500 mt-1">
                      Chiến lược: <span className="font-semibold text-slate-800">{r.strategy}</span> • Nhóm nhận: <span className="font-semibold text-slate-800">{r.group}</span>
                    </p>
                  </div>
                  <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">
                    Đang kích hoạt
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Thêm Quy Tắc Phân Phối</h2>
            <form onSubmit={handleCreateRule} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên quy tắc:</label>
                <input
                  type="text"
                  value={ruleForm.ruleName}
                  onChange={(e) => setRuleForm({ ...ruleForm, ruleName: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Phân phối Zalo Sales Miền Nam"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Chiến lược phân phối:</label>
                <select
                  value={ruleForm.strategy}
                  onChange={(e) => setRuleForm({ ...ruleForm, strategy: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2 bg-white"
                >
                  <option value="RoundRobin">RoundRobin (Xoay vòng đều)</option>
                  <option value="LoadBalance">LoadBalance (Cân bằng số chat)</option>
                  <option value="SkillBased">SkillBased (Theo kỹ năng tư vấn)</option>
                </select>
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Nhóm tư vấn viên tiếp nhận:</label>
                <input
                  type="text"
                  value={ruleForm.group}
                  onChange={(e) => setRuleForm({ ...ruleForm, group: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700">
                Lưu Quy Tắc Phân Phối
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 2: CONVERSATION TRANSFER */}
      {activeTab === 'transfer' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-6 max-w-2xl mx-auto">
          <h2 className="text-lg font-bold text-slate-800">🔄 Chuyển Giao Hội Thoại Giữa Các Tư Vấn Viên (UC_CRM_042)</h2>

          <form onSubmit={handleTransfer} className="space-y-4 text-sm">
            <div>
              <label className="block text-slate-700 font-medium mb-1">Chọn phiên hội thoại cần chuyển giao:</label>
              <select
                value={transferForm.convId}
                onChange={(e) => setTransferForm({ ...transferForm, convId: e.target.value })}
                className="w-full border border-slate-300 rounded-lg p-2.5 bg-white"
              >
                {activeSessions.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.customerName} (Đang xử lý bởi: {s.currentAgent})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-slate-700 font-medium mb-1">Chuyển sang Agent tiếp nhận:</label>
              <input
                type="text"
                value={transferForm.targetAgent}
                onChange={(e) => setTransferForm({ ...transferForm, targetAgent: e.target.value })}
                className="w-full border border-slate-300 rounded-lg p-2"
              />
            </div>

            <div>
              <label className="block text-slate-700 font-medium mb-1">Ghi chú lý do chuyển giao:</label>
              <textarea
                value={transferForm.note}
                onChange={(e) => setTransferForm({ ...transferForm, note: e.target.value })}
                className="w-full border border-slate-300 rounded-lg p-2"
                rows={3}
                placeholder="VD: Khách hàng cần hỗ trợ kỹ thuật chi tiết về phân hệ sản xuất"
              />
            </div>

            <button type="submit" className="w-full py-2.5 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700">
              🚀 Thực Hiện Chuyển Giao Cuộc Chat
            </button>
          </form>
        </div>
      )}

      {/* TAB 3: SLA ALERTS */}
      {activeTab === 'sla' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">⏱️ Giám Sát Thời Gian Phản Hồi & Cảnh Báo SLA (UC_CRM_043)</h2>

          <div className="space-y-3">
            {slaLogs.map((s) => {
              const slaRes = evaluateSlaStatus(s.maxMins, s.actualMins);
              return (
                <div key={s.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{s.customer}</h3>
                    <p className="text-xs text-slate-500 mt-1">
                      Tư vấn viên: <span className="font-semibold text-slate-700">{s.agent}</span> • SLA cam kết: {s.maxMins} phút
                    </p>
                  </div>

                  <div className="text-right">
                    <span className="text-sm font-extrabold text-slate-900 block">Thời gian chờ: {s.actualMins} phút</span>
                    <span className={`inline-block mt-1 px-3 py-0.5 text-xs font-bold rounded-full border ${slaRes.badgeClass}`}>
                      {slaRes.statusText}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* TAB 4: SCRIPTED CHATBOT */}
      {activeTab === 'bot' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">🤖 Cấu Hình Kịch Bản Chatbot Tự Động (UC_CRM_044)</h2>

          <div className="space-y-4">
            {botFlows.map((f) => {
              const steps = parseBotFlowSteps(f.json);
              return (
                <div key={f.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 space-y-3">
                  <div className="flex justify-between items-center">
                    <div>
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-indigo-100 text-indigo-800">Từ khóa: {f.keyword}</span>
                      <h3 className="font-bold text-slate-900 text-base mt-1">{f.name}</h3>
                    </div>
                    <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">Active</span>
                  </div>

                  <div className="bg-white p-3 rounded-lg border border-slate-200 text-xs space-y-2">
                    <p className="font-bold text-slate-700">Các bước kịch bản:</p>
                    {steps.map((st, idx) => (
                      <div key={idx} className="flex gap-2 items-start text-slate-600">
                        <span className="font-semibold text-indigo-600">Bước {st.step}:</span>
                        <span>{st.text}</span>
                      </div>
                    ))}
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
