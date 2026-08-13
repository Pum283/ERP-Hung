'use client';

import React, { useState } from 'react';
import {
  evaluateLeadPriorityTier,
  generateClonedCampaignName,
  filterConversationsByChannel,
} from '@/shared/api/crm-lead-campaign-inbox-helpers';

export default function CrmLeadCampaignInboxPage() {
  const [activeTab, setActiveTab] = useState<'potential' | 'duplicate' | 'inbox' | 'assign'>('potential');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: LEAD POTENTIAL SCORING (UC_CRM_007)
  // ────────────────────────────────────────────────────────────────────────────
  const [potentialScores, setPotentialScores] = useState([
    { id: 'sc-1', customerName: 'Công ty TNHH Thực Phẩm An Phát', score: 92, evaluator: 'Nguyễn Văn Sales', notes: 'Khách hàng có nhu cầu mở rộng 5 nhà máy mới Q3', evaluatedAt: '13/08/2026' },
    { id: 'sc-2', customerName: 'Tập đoàn Bất động sản Nam Long', score: 75, evaluator: 'Trần Thị CRM', notes: 'Cần tư vấn gói giải pháp ERP Cloud', evaluatedAt: '11/08/2026' },
    { id: 'sc-3', customerName: 'DNTN Sản xuất Gỗ Mỹ Nghệ', score: 35, evaluator: 'Lê Văn Admin', notes: 'Chưa có ngân sách rõ ràng cho đợt này', evaluatedAt: '05/08/2026' },
  ]);

  const [scoreForm, setScoreForm] = useState({ customerName: '', score: 80, notes: '' });

  const handleEvaluatePotential = (e: React.FormEvent) => {
    e.preventDefault();
    if (!scoreForm.customerName) {
      showToast('Vui lòng nhập tên khách hàng.', 'error');
      return;
    }

    const created = {
      id: `sc-${Date.now()}`,
      customerName: scoreForm.customerName,
      score: scoreForm.score,
      evaluator: 'Sales Executive (Bạn)',
      notes: scoreForm.notes || 'Đã chấm điểm tiềm năng khách hàng',
      evaluatedAt: new Date().toLocaleDateString('vi-VN'),
    };

    setPotentialScores([created, ...potentialScores]);
    setScoreForm({ customerName: '', score: 80, notes: '' });
    showToast(`Đã lưu kết quả chấm điểm tiềm năng cho [${created.customerName}] thành công!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: CAMPAIGN DUPLICATION (UC_CRM_022)
  // ────────────────────────────────────────────────────────────────────────────
  const [campaigns] = useState([
    { id: 'cmp-1', name: 'Chiến dịch Khuyến mại Khách hàng Doanh nghiệp Q3', leadsCount: 145, channel: 'Omnichannel' },
    { id: 'cmp-2', name: 'Chương trình Tri ân Khách hàng Thân thiết 2026', leadsCount: 88, channel: 'Zalo OA' },
  ]);
  const [selectedCmp, setSelectedCmp] = useState(campaigns[0].name);

  const handleDuplicateCampaign = (e: React.FormEvent) => {
    e.preventDefault();
    const clonedName = generateClonedCampaignName(selectedCmp);
    showToast(`Đã nhân bản chiến dịch thành công thành [${clonedName}]!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: OMNICHANNEL INBOX (UC_CRM_039)
  // ────────────────────────────────────────────────────────────────────────────
  const [channelFilter, setChannelFilter] = useState('all');
  const [conversations, setConversations] = useState([
    { id: 'cv-1', channel: 'Zalo', externalId: 'ZALO-98214', customerName: 'Nguyễn Thị Mai', phone: '0908123456', agent: 'Sales Admin 1', status: 'Assigned', snippet: 'Dạ chào công ty, bên mình có gói ERP cho sản xuất gỗ không ạ?', time: '10 phút trước' },
    { id: 'cv-2', channel: 'Facebook', externalId: 'FB-88129', customerName: 'Trần Hoài Nam', phone: '0912345678', agent: 'Chưa phân công', status: 'New', snippet: 'Báo giá cho mình hệ thống CRM kết nối Zalo OA với ạ.', time: '15 phút trước' },
    { id: 'cv-3', channel: 'Email', externalId: 'EML-4412', customerName: 'Lê Văn Hùng', phone: '0987654321', agent: 'Sales Executive 2', status: 'Assigned', snippet: 'Gửi giúp tôi Hợp đồng dự thảo đợt 1 qua Email này nhé.', time: '1 giờ trước' },
  ]);

  const filteredConversations = filterConversationsByChannel(conversations, channelFilter);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: NEW CONVERSATION RECEPTION (UC_CRM_040)
  // ────────────────────────────────────────────────────────────────────────────
  const handleAssignToMe = (convId: string, custName: string) => {
    setConversations((prev) =>
      prev.map((c) => (c.id === convId ? { ...c, agent: 'Bạn (Sales Agent)', status: 'Assigned' } : c))
    );
    showToast(`Đã tiếp nhận hội thoại của khách hàng ${custName} thành công!`, 'success');
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
      <div className="bg-gradient-to-r from-orange-950 via-amber-950 to-slate-900 p-6 rounded-2xl text-white shadow-xl">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-amber-500/30 text-amber-200 text-xs px-3 py-1 rounded-full font-semibold border border-amber-400/30">
              CRM - LEAD POTENTIAL SCORING & OMNICHANNEL INBOX
            </span>
            <h1 className="text-2xl font-bold mt-2">Bước 168: CRM Sales Leads & Omnichannel Messaging</h1>
            <p className="text-amber-100 text-sm mt-1">
              Đánh giá tiềm năng Lead, Nhân bản Campaign Marketing, Hộp thư Đa kênh & Tiếp nhận hội thoại mới
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-amber-500/20 text-amber-300 border border-amber-500/30">
              ● Tiến độ 90% (4/4 UCs)
            </span>
          </div>
        </div>

        {/* Tab Selection */}
        <div className="flex space-x-2 mt-6 border-t border-white/10 pt-4">
          <button
            onClick={() => setActiveTab('potential')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'potential' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🔥 UC_CRM_007: Đánh giá tiềm năng
          </button>
          <button
            onClick={() => setActiveTab('duplicate')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'duplicate' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📋 UC_CRM_022: Nhân bản Campaign
          </button>
          <button
            onClick={() => setActiveTab('inbox')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'inbox' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            💬 UC_CRM_039: Hộp thư Đa kênh
          </button>
          <button
            onClick={() => setActiveTab('assign')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'assign' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            ⚡ UC_CRM_040: Tiếp nhận hội thoại
          </button>
        </div>
      </div>

      {/* TAB 1: LEAD POTENTIAL SCORING */}
      {activeTab === 'potential' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
              <h2 className="text-lg font-bold text-slate-800">🔥 Danh sách Đánh giá Tiềm năng Khách hàng (UC_CRM_007)</h2>

              <div className="space-y-3">
                {potentialScores.map((sc) => {
                  const tierRes = evaluateLeadPriorityTier(sc.score);
                  return (
                    <div key={sc.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                      <div>
                        <h3 className="font-bold text-slate-900">{sc.customerName}</h3>
                        <p className="text-xs text-slate-500 mt-1">
                          Đánh giá bởi: {sc.evaluator} • Ngày: {sc.evaluatedAt}
                        </p>
                        <p className="text-xs text-slate-600 italic mt-1">"{sc.notes}"</p>
                      </div>

                      <div className="text-right">
                        <span className="text-2xl font-extrabold text-slate-900">{sc.score} / 100</span>
                        <div>
                          <span className={`inline-block mt-1 px-3 py-0.5 text-xs font-bold rounded-full border ${tierRes.badgeColorClass}`}>
                            Phân loại: {tierRes.priorityTier}
                          </span>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Score Form */}
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
              <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Chấm Điểm Tiềm Năng Mới</h2>
              <form onSubmit={handleEvaluatePotential} className="space-y-4 text-sm">
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Tên khách hàng / Lead:</label>
                  <input
                    type="text"
                    value={scoreForm.customerName}
                    onChange={(e) => setScoreForm({ ...scoreForm, customerName: e.target.value })}
                    className="w-full border border-slate-300 rounded-lg p-2"
                    placeholder="VD: Công ty Cổ phần XNK Minh Tâm"
                  />
                </div>
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Điểm tiềm năng (0 - 100):</label>
                  <input
                    type="number"
                    value={scoreForm.score}
                    onChange={(e) => setScoreForm({ ...scoreForm, score: Number(e.target.value) })}
                    className="w-full border border-slate-300 rounded-lg p-2"
                    min={0}
                    max={100}
                  />
                </div>
                <div>
                  <label className="block text-slate-700 font-medium mb-1">Ghi chú đánh giá:</label>
                  <textarea
                    value={scoreForm.notes}
                    onChange={(e) => setScoreForm({ ...scoreForm, notes: e.target.value })}
                    className="w-full border border-slate-300 rounded-lg p-2"
                    rows={3}
                  />
                </div>

                <button type="submit" className="w-full py-2.5 bg-amber-600 text-white rounded-lg font-semibold hover:bg-amber-700">
                  Lưu Kết Quả Đánh Giá
                </button>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* TAB 2: CAMPAIGN DUPLICATION */}
      {activeTab === 'duplicate' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-6 max-w-2xl mx-auto">
          <h2 className="text-lg font-bold text-slate-800">📋 Nhân Bản Chiến Dịch Marketing CRM (UC_CRM_022)</h2>

          <form onSubmit={handleDuplicateCampaign} className="space-y-4 text-sm">
            <div>
              <label className="block text-slate-700 font-medium mb-1">Chọn chiến dịch cần nhân bản:</label>
              <select
                value={selectedCmp}
                onChange={(e) => setSelectedCmp(e.target.value)}
                className="w-full border border-slate-300 rounded-lg p-2.5 bg-white"
              >
                {campaigns.map((c) => (
                  <option key={c.id} value={c.name}>
                    {c.name} ({c.leadsCount} Leads)
                  </option>
                ))}
              </select>
            </div>

            <div className="p-4 bg-amber-50 rounded-xl border border-amber-200 text-xs text-amber-900 space-y-1">
              <p className="font-bold">Tên chiến dịch bản sao sẽ tạo:</p>
              <p className="font-semibold text-slate-800">{generateClonedCampaignName(selectedCmp)}</p>
              <p className="text-slate-600">Tất cả cấu hình rule, kịch bản gửi tin và tập khách hàng mục tiêu sẽ được sao chép nguyên bản.</p>
            </div>

            <button type="submit" className="w-full py-2.5 bg-amber-600 text-white rounded-lg font-semibold hover:bg-amber-700">
              ⚡ Kích Hoạt Nhân Bản Campaign
            </button>
          </form>
        </div>
      )}

      {/* TAB 3: OMNICHANNEL INBOX */}
      {activeTab === 'inbox' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-bold text-slate-800">💬 Hộp Thư Đa Kênh Tập Trung Zalo / Facebook / Email (UC_CRM_039)</h2>
            <div className="flex gap-2 text-xs">
              <button
                onClick={() => setChannelFilter('all')}
                className={`px-3 py-1.5 rounded-lg border font-medium ${channelFilter === 'all' ? 'bg-slate-900 text-white' : 'bg-white text-slate-700'}`}
              >
                Tất cả kênh
              </button>
              <button
                onClick={() => setChannelFilter('Zalo')}
                className={`px-3 py-1.5 rounded-lg border font-medium ${channelFilter === 'Zalo' ? 'bg-blue-600 text-white' : 'bg-white text-slate-700'}`}
              >
                Zalo OA
              </button>
              <button
                onClick={() => setChannelFilter('Facebook')}
                className={`px-3 py-1.5 rounded-lg border font-medium ${channelFilter === 'Facebook' ? 'bg-indigo-600 text-white' : 'bg-white text-slate-700'}`}
              >
                Facebook
              </button>
            </div>
          </div>

          <div className="space-y-3">
            {filteredConversations.map((cv) => (
              <div key={cv.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <span className="px-2 py-0.5 text-xs font-bold rounded bg-slate-200 text-slate-800">{cv.channel}</span>
                    <h3 className="font-bold text-slate-900 text-sm">{cv.customerName}</h3>
                    <span className="text-xs text-slate-500">({cv.phone})</span>
                  </div>
                  <p className="text-xs text-slate-700">"{cv.snippet}"</p>
                  <p className="text-xs text-slate-400">Thời gian: {cv.time}</p>
                </div>

                <div className="text-right">
                  <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${cv.agent === 'Chưa phân công' ? 'bg-rose-100 text-rose-800' : 'bg-emerald-100 text-emerald-800'}`}>
                    {cv.agent}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* TAB 4: NEW CONVERSATION RECEPTION */}
      {activeTab === 'assign' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">⚡ Tiếp Nhận & Phân Phối Hội thoại Mới (UC_CRM_040)</h2>

          <div className="space-y-3">
            {conversations
              .filter((c) => c.status === 'New')
              .map((c) => (
                <div key={c.id} className="p-4 rounded-xl border border-amber-200 bg-amber-50/50 flex justify-between items-center">
                  <div>
                    <span className="px-2 py-0.5 text-xs font-bold rounded bg-amber-200 text-amber-900">Kênh {c.channel}</span>
                    <h3 className="font-bold text-slate-900 mt-1">{c.customerName} ({c.phone})</h3>
                    <p className="text-xs text-slate-700 mt-1">"{c.snippet}"</p>
                  </div>

                  <button
                    onClick={() => handleAssignToMe(c.id, c.customerName)}
                    className="px-4 py-2 bg-emerald-600 text-white rounded-lg text-xs font-bold hover:bg-emerald-700 shadow-sm"
                  >
                    👉 Tiếp Nhận Hội Thoại Này
                  </button>
                </div>
              ))}

            {conversations.filter((c) => c.status === 'New').length === 0 && (
              <div className="p-8 text-center text-slate-400 text-sm">
                Hiện tại không có hội thoại mới nào chờ tiếp nhận.
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
