'use client';

import React, { useState } from 'react';
import {
  calculateCommissionAmount,
  evaluateRetentionHealth,
  validateRedemptionRequest,
} from '@/shared/api/crm-reward-survey-retention-commission-helpers';

export default function CrmRewardSurveyRetentionCommissionPage() {
  const [activeTab, setActiveTab] = useState<'redemption' | 'survey' | 'retention' | 'commission'>('redemption');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: REWARD REDEMPTION (UC_CRM_117)
  // ────────────────────────────────────────────────────────────────────────────
  const [customerPoints] = useState({ name: 'Đại lý Nông Sản Miền Tây', available: 1250 });
  const [redemptions, setRedemptions] = useState([
    { id: 'rd-1', customer: 'Đại lý Nông Sản Miền Tây', reward: 'Voucher Giảm 500,000 VNĐ Đơn Phân Bón', points: 500, status: 'Fulfilled', time: '10:15 - 13/08/2026' },
    { id: 'rd-2', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', reward: 'Bộ Bình Trà Gốm Sứ Cao Cấp Logo ERP', points: 800, status: 'Fulfilled', time: '09:30 - 11/08/2026' },
  ]);

  const [redeemForm, setRedeemForm] = useState({ rewardName: '', points: 500 });

  const handleRedeem = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateRedemptionRequest(customerPoints.available, redeemForm.points);
    if (!val.isValid) {
      showToast(val.error || 'Số điểm không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `rd-${Date.now()}`,
      customer: customerPoints.name,
      reward: redeemForm.rewardName || 'Voucher Khuyến Mại Thân Thiết',
      points: redeemForm.points,
      status: 'Fulfilled',
      time: 'Vừa xong',
    };

    setRedemptions([created, ...redemptions]);
    setRedeemForm({ rewardName: '', points: 500 });
    showToast(`🎁 Đã đổi quà [${created.reward}] thành công cho khách hàng!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: CUSTOMER SATISFACTION SURVEYS (UC_CRM_118)
  // ────────────────────────────────────────────────────────────────────────────
  const [surveys, setSurveys] = useState([
    { id: 'sv-1', customer: 'Đại lý Nông Sản Miền Tây', score: 5, comments: 'Nhân viên ghé thăm tư vấn rất tận tình, đơn giao nhanh', channel: 'StoreVisit' },
    { id: 'sv-2', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', score: 4, comments: 'Hàng hóa đóng gói cẩn thận, cần hỗ trợ thêm về hóa đơn điện tử', channel: 'OnlineOrder' },
  ]);

  const [surveyForm, setSurveyForm] = useState({ customer: 'Đại lý Nông Sản Miền Tây', score: 5, comments: '', channel: 'StoreVisit' });

  const handleSubmitSurvey = (e: React.FormEvent) => {
    e.preventDefault();
    const created = {
      id: `sv-${Date.now()}`,
      customer: surveyForm.customer,
      score: surveyForm.score,
      comments: surveyForm.comments || 'Đánh giá hài lòng tổng thể dịch vụ',
      channel: surveyForm.channel,
    };

    setSurveys([created, ...surveys]);
    setSurveyForm({ customer: 'Đại lý Nông Sản Miền Tây', score: 5, comments: '', channel: 'StoreVisit' });
    showToast('✓ Cảm ơn bạn! Đã ghi nhận phản hồi khảo sát hài lòng!', 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: CUSTOMER RETENTION & REPURCHASE REPORT (UC_CRM_119)
  // ────────────────────────────────────────────────────────────────────────────
  const retentionData = {
    totalCustomers: 180,
    repeatCustomers: 140,
    repeatRate: 77.8,
    churnRate: 22.2,
    avgLtv: 125000000,
  };
  const retentionHealth = evaluateRetentionHealth(retentionData.repeatRate);

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: COMMISSION RULES CONFIGURATION (UC_CRM_120)
  // ────────────────────────────────────────────────────────────────────────────
  const [commissionRules, setCommissionRules] = useState([
    { id: 'cr-1', code: 'COMM-FIELD-STD', name: 'Hoa Hồng Chuẩn Field Sales', role: 'FieldSales', minThreshold: 100000000, rate: 2.5, active: true },
    { id: 'cr-2', code: 'COMM-FIELD-PRO', name: 'Hoa Hồng Doanh Số Cao Field Sales (> 300Tr)', role: 'FieldSales', minThreshold: 300000000, rate: 4.0, active: true },
    { id: 'cr-3', code: 'COMM-AM-GOLD', name: 'Hoa Hồng Quản Lý Tài Khoản (Account Manager)', role: 'AccountManager', minThreshold: 500000000, rate: 5.0, active: true },
  ]);

  const [ruleForm, setRuleForm] = useState({ code: '', name: '', role: 'FieldSales', minThreshold: 100000000, rate: 2.5 });

  const handleCreateRule = (e: React.FormEvent) => {
    e.preventDefault();
    if (!ruleForm.code || !ruleForm.name) {
      showToast('Vui lòng nhập Mã và Tên quy tắc hoa hồng.', 'error');
      return;
    }

    const created = {
      id: `cr-${Date.now()}`,
      code: ruleForm.code,
      name: ruleForm.name,
      role: ruleForm.role,
      minThreshold: ruleForm.minThreshold,
      rate: ruleForm.rate,
      active: true,
    };

    setCommissionRules([...commissionRules, created]);
    setRuleForm({ code: '', name: '', role: 'FieldSales', minThreshold: 100000000, rate: 2.5 });
    showToast(`⚙️ Đã khởi tạo Quy tắc hoa hồng mới [${created.code}]!`, 'success');
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
            <span className="bg-amber-500/30 text-amber-200 text-xs px-3 py-1 rounded-full font-semibold border border-amber-400/30">
              CRM - REWARDS, CSAT SURVEYS, RETENTION & COMMISSIONS
            </span>
            <h1 className="text-2xl font-bold mt-2">CRM Đổi Quà Tích Điểm, Khảo Sát CSAT, Retention & Rule Hoa Hồng</h1>
            <p className="text-amber-100 text-sm mt-1">
              Đổi quà tích điểm, Khảo sát mức độ hài lòng, Báo cáo retention tỷ lệ tái mua & Cấu hình quy tắc hoa hồng
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
            onClick={() => setActiveTab('redemption')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'redemption' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🎁 UC_CRM_117: Đổi Quà Tích Điểm
          </button>
          <button
            onClick={() => setActiveTab('survey')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'survey' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            ⭐ UC_CRM_118: Khảo Sát Hài Lòng CSAT
          </button>
          <button
            onClick={() => setActiveTab('retention')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'retention' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📊 UC_CRM_119: Báo Cáo Retention Tái Mua
          </button>
          <button
            onClick={() => setActiveTab('commission')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'commission' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            ⚙️ UC_CRM_120: Cấu Hình Rule Hoa Hồng
          </button>
        </div>
      </div>

      {/* TAB 1: REWARD REDEMPTION */}
      {activeTab === 'redemption' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">🎁 Đổi Điểm Thưởng Lấy Quà Tặng / Voucher (UC_CRM_117)</h2>
            <div className="space-y-3">
              {redemptions.map((r) => (
                <div key={r.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{r.reward}</h3>
                    <p className="text-xs text-slate-500 mt-1">Khách hàng: {r.customer} • Điểm quy đổi: <span className="font-bold text-amber-700">{r.points} pts</span></p>
                  </div>
                  <span className="px-3 py-1 text-xs font-semibold rounded-lg bg-emerald-100 text-emerald-800">
                    {r.status === 'Fulfilled' ? 'Đã trao quà' : 'Đang xử lý'}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <div className="mb-4 p-3 rounded-lg bg-amber-50 border border-amber-200">
              <span className="text-xs font-semibold text-amber-800 block">Số điểm khả dụng của KH:</span>
              <span className="text-xl font-bold text-amber-900">{customerPoints.available} pts</span>
            </div>
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Thực Hiện Đổi Quà</h2>
            <form onSubmit={handleRedeem} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên quà tặng / Voucher:</label>
                <input
                  type="text"
                  value={redeemForm.rewardName}
                  onChange={(e) => setRedeemForm({ ...redeemForm, rewardName: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Voucher Giảm 500,000 VNĐ"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Số điểm quy đổi (pts):</label>
                <input
                  type="number"
                  value={redeemForm.points}
                  onChange={(e) => setRedeemForm({ ...redeemForm, points: parseInt(e.target.value) || 0 })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-amber-600 text-white rounded-lg font-semibold hover:bg-amber-700">
                Xác Nhận Đổi Quà
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 2: CUSTOMER SATISFACTION SURVEYS */}
      {activeTab === 'survey' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">⭐ Phản Hồi Khảo Sát Hài Lòng Khách Hàng CSAT (UC_CRM_118)</h2>
            <div className="space-y-3">
              {surveys.map((s) => (
                <div key={s.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-amber-100 text-amber-800">⭐ {s.score}/5 Điểm</span>
                      <h3 className="font-bold text-slate-900">{s.customer}</h3>
                    </div>
                    <p className="text-xs text-slate-600 italic mt-1">"{s.comments}"</p>
                  </div>
                  <span className="px-2.5 py-1 text-xs font-semibold rounded bg-slate-200 text-slate-800">
                    Kênh: {s.channel}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Khảo Sát Mới</h2>
            <form onSubmit={handleSubmitSurvey} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Khách hàng khảo sát:</label>
                <input
                  type="text"
                  value={surveyForm.customer}
                  onChange={(e) => setSurveyForm({ ...surveyForm, customer: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Điểm số CSAT (1 ➔ 5 ⭐):</label>
                <select
                  value={surveyForm.score}
                  onChange={(e) => setSurveyForm({ ...surveyForm, score: parseInt(e.target.value) || 5 })}
                  className="w-full border border-slate-300 rounded-lg p-2 bg-white"
                >
                  <option value={5}>5 ⭐ — Rất hài lòng</option>
                  <option value={4}>4 ⭐ — Hài lòng</option>
                  <option value={3}>3 ⭐ — Bình thường</option>
                  <option value={2}>2 ⭐ — Chưa hài lòng</option>
                  <option value={1}>1 ⭐ — Rất không hài lòng</option>
                </select>
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Ghi chú ý kiến đóng góp:</label>
                <textarea
                  value={surveyForm.comments}
                  onChange={(e) => setSurveyForm({ ...surveyForm, comments: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  rows={3}
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-emerald-600 text-white rounded-lg font-semibold hover:bg-emerald-700">
                Gửi Đánh Giá CSAT
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 3: CUSTOMER RETENTION & REPURCHASE REPORT */}
      {activeTab === 'retention' && (
        <div className="space-y-6">
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm flex justify-between items-center">
            <div>
              <span className="text-xs font-semibold text-slate-500">Đánh Giá Sức Khỏe Tái Mua (Retention Health)</span>
              <h2 className="text-xl font-bold text-slate-900 mt-0.5">{retentionHealth.statusLabel}</h2>
            </div>
            <span className={`px-3.5 py-1.5 rounded-full text-xs font-bold border ${retentionHealth.badgeClass}`}>
              Tỷ lệ tái mua {retentionData.repeatRate}%
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold text-slate-500">Tổng Khách Hàng Hoạt Động</span>
              <p className="text-2xl font-bold text-brand mt-1">{retentionData.totalCustomers} KH</p>
            </div>
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold text-slate-500">Khách Hàng Tái Mua Đa Kỳ</span>
              <p className="text-2xl font-bold text-emerald-600 mt-1">{retentionData.repeatCustomers} KH</p>
            </div>
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold text-slate-500">Tỷ Lệ Rời Bỏ (Churn Rate)</span>
              <p className="text-2xl font-bold text-rose-600 mt-1">{retentionData.churnRate}%</p>
            </div>
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold text-slate-500">Giá Trị Vòng Đời KH (LTV)</span>
              <p className="text-2xl font-bold text-blue-600 mt-1">{retentionData.avgLtv.toLocaleString('vi-VN')} VNĐ</p>
            </div>
          </div>
        </div>
      )}

      {/* TAB 4: COMMISSION RULES CONFIGURATION */}
      {activeTab === 'commission' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">⚙️ Cấu Hình Quy Tắc Hoa Hồng Kinh Doanh (UC_CRM_120)</h2>
            <div className="space-y-3">
              {commissionRules.map((cr) => {
                const sampleCommission = calculateCommissionAmount(cr.minThreshold, cr.rate);
                return (
                  <div key={cr.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="px-2 py-0.5 text-xs font-bold rounded bg-blue-100 text-blue-800">{cr.code}</span>
                        <h3 className="font-bold text-slate-900">{cr.name}</h3>
                      </div>
                      <p className="text-xs text-slate-500 mt-1">Vai trò: {cr.role} • Hạn mức tối thiểu: {cr.minThreshold.toLocaleString('vi-VN')} VNĐ</p>
                      <p className="text-xs text-slate-700 mt-0.5">
                        Ví dụ hoa hồng tại mốc tối thiểu: <span className="font-bold text-emerald-700">{sampleCommission.toLocaleString('vi-VN')} VNĐ</span>
                      </p>
                    </div>
                    <div className="text-right">
                      <span className="text-lg font-extrabold text-indigo-700 block">{cr.rate}%</span>
                      <span className="px-2.5 py-0.5 text-xs font-semibold rounded bg-emerald-100 text-emerald-800 inline-block mt-1">
                        Kích hoạt
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Thêm Rule Hoa Hồng</h2>
            <form onSubmit={handleCreateRule} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Mã rule:</label>
                <input
                  type="text"
                  value={ruleForm.code}
                  onChange={(e) => setRuleForm({ ...ruleForm, code: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: COMM-FIELD-2026"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tên quy tắc hoa hồng:</label>
                <input
                  type="text"
                  value={ruleForm.name}
                  onChange={(e) => setRuleForm({ ...ruleForm, name: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Hoa hồng Field Sales đợt 1"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Hạn mức doanh số tối thiểu (VNĐ):</label>
                <input
                  type="number"
                  value={ruleForm.minThreshold}
                  onChange={(e) => setRuleForm({ ...ruleForm, minThreshold: parseFloat(e.target.value) || 0 })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Tỷ lệ hoa hồng (%):</label>
                <input
                  type="number"
                  step="0.1"
                  value={ruleForm.rate}
                  onChange={(e) => setRuleForm({ ...ruleForm, rate: parseFloat(e.target.value) || 0 })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700">
                Lưu Rule Hoa Hồng
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
