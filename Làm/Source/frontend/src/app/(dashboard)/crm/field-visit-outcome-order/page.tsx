'use client';

import React, { useState } from 'react';
import {
  evaluateOutcomeStatusBadge,
  calculateOnSiteOrderTotal,
  validateDemandEntry,
} from '@/shared/api/crm-field-visit-outcome-order-helpers';

export default function CrmFieldVisitOutcomeOrderPage() {
  const [activeTab, setActiveTab] = useState<'outcome' | 'demand' | 'order' | 'history'>('outcome');

  // Toast notification
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' | 'warning' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 1: VISIT OUTCOME (UC_CRM_093)
  // ────────────────────────────────────────────────────────────────────────────
  const [outcomes, setOutcomes] = useState([
    { id: 'oc-1', customer: 'Đại lý Nông Sản Miền Tây', purpose: 'Gặp chủ đại lý chốt đơn Q3', status: 'Successful', notes: 'Đồng ý nhập thêm 100 sản phẩm mới', actionItems: 'Gửi hợp đồng ký kết trong tuần', time: 'Hôm nay' },
    { id: 'oc-2', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', purpose: 'Kiểm tra trưng bày hàng tồn', status: 'FollowUpRequired', notes: 'Hàng trưng bày đẹp nhưng thiếu mẫu mới', actionItems: 'Gửi bảng giá mẫu mới', time: 'Hôm qua' },
  ]);

  const [outcomeForm, setOutcomeForm] = useState({ customer: 'Đại lý Nông Sản Miền Tây', purpose: '', status: 'Successful', notes: '', actionItems: '' });

  const handleRecordOutcome = (e: React.FormEvent) => {
    e.preventDefault();
    if (!outcomeForm.purpose || !outcomeForm.notes) {
      showToast('Vui lòng nhập Mục đích và Ghi chú kết quả viếng thăm.', 'error');
      return;
    }

    const created = {
      id: `oc-${Date.now()}`,
      customer: outcomeForm.customer,
      purpose: outcomeForm.purpose,
      status: outcomeForm.status,
      notes: outcomeForm.notes,
      actionItems: outcomeForm.actionItems || 'Không có',
      time: 'Vừa xong',
    };

    setOutcomes([created, ...outcomes]);
    setOutcomeForm({ customer: 'Đại lý Nông Sản Miền Tây', purpose: '', status: 'Successful', notes: '', actionItems: '' });
    showToast(`Đã ghi nhận kết quả buổi viếng thăm [${created.customer}]!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 2: CUSTOMER DEMAND LOGGING (UC_CRM_094)
  // ────────────────────────────────────────────────────────────────────────────
  const [demands, setDemands] = useState([
    { id: 'dm-1', customer: 'Đại lý Nông Sản Miền Tây', category: 'Phân bón sinh học hữu cơ', qty: 250, urgency: 'High', competitor: 'Đối thủ A giảm 5%', feedback: 'Khách yêu cầu giao gấp trong 3 ngày' },
    { id: 'dm-2', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', category: 'Nước giải khát vị trái cây', qty: 100, urgency: 'Medium', competitor: 'Đối thủ B tặng quà KM', feedback: 'Cần chương trình quà tặng kèm' },
  ]);

  const [demandForm, setDemandForm] = useState({ customer: 'Đại lý Nông Sản Miền Tây', category: '', qty: 10, urgency: 'Medium', competitor: '', feedback: '' });

  const handleRecordDemand = (e: React.FormEvent) => {
    e.preventDefault();
    const val = validateDemandEntry(demandForm.category, demandForm.qty);
    if (!val.isValid) {
      showToast(val.error || 'Dữ liệu không hợp lệ.', 'error');
      return;
    }

    const created = {
      id: `dm-${Date.now()}`,
      customer: demandForm.customer,
      category: demandForm.category,
      qty: demandForm.qty,
      urgency: demandForm.urgency,
      competitor: demandForm.competitor || 'Không có thông tin',
      feedback: demandForm.feedback || 'Nhu cầu bình thường',
    };

    setDemands([created, ...demands]);
    setDemandForm({ customer: 'Đại lý Nông Sản Miền Tây', category: '', qty: 10, urgency: 'Medium', competitor: '', feedback: '' });
    showToast(`Đã ghi nhận nhu cầu khách hàng [${created.category}]!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 3: ON-SITE STORE ORDERING (UC_CRM_095)
  // ────────────────────────────────────────────────────────────────────────────
  const [onSiteOrders, setOnSiteOrders] = useState([
    { id: 'ord-1', code: 'ORD-ONSITE-260813-A912', customer: 'Đại lý Nông Sản Miền Tây', totalAmount: 11000000, status: 'OnSiteSubmitted', time: '10:15 - 13/08/2026' },
  ]);

  const [orderItems, setOrderItems] = useState([
    { name: 'Phân bón sinh học cao cấp (Bao 50kg)', qty: 10, price: 500000 },
    { name: 'Chế phẩm sinh học BVTV 1L', qty: 5, price: 1200000 },
  ]);

  const handleCreateOnSiteOrder = () => {
    const total = calculateOnSiteOrderTotal(orderItems);
    const created = {
      id: `ord-${Date.now()}`,
      code: `ORD-ONSITE-${Math.floor(Math.random() * 9000 + 1000)}`,
      customer: 'Đại lý Nông Sản Miền Tây',
      totalAmount: total,
      status: 'OnSiteSubmitted',
      time: 'Vừa xong',
    };

    setOnSiteOrders([created, ...onSiteOrders]);
    showToast(`🛒 Đã lên đơn trực tiếp tại điểm thăm [${created.code}] với tổng tiền ${total.toLocaleString('vi-VN')} VNĐ!`, 'success');
  };

  // ────────────────────────────────────────────────────────────────────────────
  // TAB 4: VISIT AUDIT HISTORY LOG (UC_CRM_096)
  // ────────────────────────────────────────────────────────────────────────────
  const [historyLogs] = useState([
    { id: 'log-1', date: '13/08/2026', customer: 'Đại lý Nông Sản Miền Tây', sales: 'Nguyễn Văn Sales', inGps: '10.7769,106.7009 (10:00)', outGps: '10.7772,106.7012 (10:45)', status: 'Thành công', outcome: 'Đã lên đơn 11 triệu & hẹn giao hàng' },
    { id: 'log-2', date: '12/08/2026', customer: 'Chuỗi Cửa hàng Tiện Lợi An Khang', sales: 'Trần Thị CRM', inGps: '10.7800,106.6900 (14:15)', outGps: '10.7805,106.6910 (15:00)', status: 'Cần theo dõi', outcome: 'Khách hỏi chính sách chiết khấu số lượng lớn' },
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
      <div className="bg-gradient-to-r from-emerald-950 via-teal-950 to-indigo-950 p-6 rounded-2xl text-white shadow-xl">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-indigo-500/30 text-indigo-200 text-xs px-3 py-1 rounded-full font-semibold border border-indigo-400/30">
              CRM - FIELD VISIT OUTCOME & ON-SITE ORDERING
            </span>
            <h1 className="text-2xl font-bold mt-2">Bước 172: CRM Kết Quả Viếng Thăm, Nhu Cầu & Đặt Hàng Tại Điểm Thăm</h1>
            <p className="text-indigo-100 text-sm mt-1">
              Ghi nhận mục đích & kết quả visit, Thu thập nhu cầu khách hàng, Đặt hàng trực tiếp & Nhật ký viếng thăm
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
            onClick={() => setActiveTab('outcome')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'outcome' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📝 UC_CRM_093: Kết Quả Visit
          </button>
          <button
            onClick={() => setActiveTab('demand')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'demand' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            💡 UC_CRM_094: Nhu Cầu Khách Hàng
          </button>
          <button
            onClick={() => setActiveTab('order')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'order' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            🛒 UC_CRM_095: Đặt Hàng Tại Điểm Thăm
          </button>
          <button
            onClick={() => setActiveTab('history')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'history' ? 'bg-white text-slate-950 shadow-md' : 'text-slate-200 hover:bg-white/10'
            }`}
          >
            📜 UC_CRM_096: Lịch Sử Viếng Thăm
          </button>
        </div>
      </div>

      {/* TAB 1: VISIT OUTCOME */}
      {activeTab === 'outcome' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">📝 Mục Đích & Kết Quả Viếng Thăm (UC_CRM_093)</h2>
            <div className="space-y-3">
              {outcomes.map((o) => {
                const badge = evaluateOutcomeStatusBadge(o.status);
                return (
                  <div key={o.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                    <div>
                      <h3 className="font-bold text-slate-900">{o.customer}</h3>
                      <p className="text-xs text-slate-500 mt-1">Mục đích: {o.purpose}</p>
                      <p className="text-xs text-slate-700 font-semibold mt-1">Ghi chú: "{o.notes}"</p>
                      <p className="text-xs text-indigo-700 italic mt-0.5">Việc cần làm tiếp: {o.actionItems}</p>
                    </div>
                    <span className={`px-3 py-1 text-xs font-bold rounded-full border ${badge.badgeClass}`}>
                      {badge.label}
                    </span>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Ghi Nhận Kết Quả Visit Mới</h2>
            <form onSubmit={handleRecordOutcome} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Mục đích viếng thăm:</label>
                <input
                  type="text"
                  value={outcomeForm.purpose}
                  onChange={(e) => setOutcomeForm({ ...outcomeForm, purpose: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Giới thiệu dòng SP hữu cơ Q3"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Kết quả chung:</label>
                <select
                  value={outcomeForm.status}
                  onChange={(e) => setOutcomeForm({ ...outcomeForm, status: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2 bg-white"
                >
                  <option value="Successful">Thành công (Successful)</option>
                  <option value="Partial">Đạt một phần (Partial)</option>
                  <option value="FollowUpRequired">Cần theo dõi thêm (FollowUpRequired)</option>
                  <option value="Unsuccessful">Không thành công (Unsuccessful)</option>
                </select>
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Ghi chú kết quả chi tiết:</label>
                <textarea
                  value={outcomeForm.notes}
                  onChange={(e) => setOutcomeForm({ ...outcomeForm, notes: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  rows={2}
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Hành động tiếp theo (Action Items):</label>
                <input
                  type="text"
                  value={outcomeForm.actionItems}
                  onChange={(e) => setOutcomeForm({ ...outcomeForm, actionItems: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-teal-600 text-white rounded-lg font-semibold hover:bg-teal-700">
                Lưu Kết Quả Visit
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 2: CUSTOMER DEMAND */}
      {activeTab === 'demand' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
            <h2 className="text-lg font-bold text-slate-800">💡 Ghi Nhận Nhu Cầu Khách Hàng Tại Điểm Bán (UC_CRM_094)</h2>
            <div className="space-y-3">
              {demands.map((d) => (
                <div key={d.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                  <div>
                    <h3 className="font-bold text-slate-900">{d.customer}</h3>
                    <p className="text-xs text-slate-600 mt-1">
                      Nhóm SP quan tâm: <span className="font-semibold text-slate-900">{d.category}</span> (Dự kiến: {d.qty})
                    </p>
                    <p className="text-xs text-slate-500 mt-0.5">Đối thủ cạnh tranh: {d.competitor}</p>
                    <p className="text-xs text-amber-900 italic mt-0.5">Phản hồi: "{d.feedback}"</p>
                  </div>
                  <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${d.urgency === 'High' ? 'bg-rose-100 text-rose-800' : 'bg-amber-100 text-amber-800'}`}>
                    Mức độ gấp: {d.urgency}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5">
            <h2 className="text-lg font-bold text-slate-800 mb-4">➕ Thu Thập Nhu Cầu Mới</h2>
            <form onSubmit={handleRecordDemand} className="space-y-4 text-sm">
              <div>
                <label className="block text-slate-700 font-medium mb-1">Nhóm sản phẩm quan tâm:</label>
                <input
                  type="text"
                  value={demandForm.category}
                  onChange={(e) => setDemandForm({ ...demandForm, category: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                  placeholder="VD: Phân bón sinh học"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Số lượng dự kiến:</label>
                <input
                  type="number"
                  value={demandForm.qty}
                  onChange={(e) => setDemandForm({ ...demandForm, qty: parseInt(e.target.value) || 1 })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <div>
                <label className="block text-slate-700 font-medium mb-1">Thông tin đối thủ cạnh tranh:</label>
                <input
                  type="text"
                  value={demandForm.competitor}
                  onChange={(e) => setDemandForm({ ...demandForm, competitor: e.target.value })}
                  className="w-full border border-slate-300 rounded-lg p-2"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-teal-600 text-white rounded-lg font-semibold hover:bg-teal-700">
                Lưu Nhu Cầu
              </button>
            </form>
          </div>
        </div>
      )}

      {/* TAB 3: ON-SITE ORDERING */}
      {activeTab === 'order' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <div className="flex justify-between items-center">
            <h2 className="text-lg font-bold text-slate-800">🛒 Tạo Đơn Hàng Trực Tiếp Tại Điểm Thăm (UC_CRM_095)</h2>
            <button
              onClick={handleCreateOnSiteOrder}
              className="px-4 py-2 bg-emerald-600 text-white text-xs font-bold rounded-lg hover:bg-emerald-700 shadow-sm"
            >
              ➕ Chốt Đơn Trực Tiếp Tại Điểm Bán
            </button>
          </div>

          <div className="p-4 rounded-xl border border-slate-200 bg-slate-50 space-y-3">
            <h3 className="font-bold text-slate-800 text-sm">Danh Sách Sản Phẩm Đặt (Tuyến Field Sales):</h3>
            {orderItems.map((item, idx) => (
              <div key={idx} className="flex justify-between text-xs border-b border-slate-200 pb-2">
                <span>{item.name} (x{item.qty})</span>
                <span className="font-semibold text-slate-900">{(item.qty * item.price).toLocaleString('vi-VN')} VNĐ</span>
              </div>
            ))}
            <div className="text-right text-sm font-extrabold text-teal-900 pt-1">
              Tổng tiền đơn hàng: {calculateOnSiteOrderTotal(orderItems).toLocaleString('vi-VN')} VNĐ
            </div>
          </div>

          <div className="space-y-3 pt-2">
            <h3 className="font-bold text-slate-800 text-sm">Đơn Hàng On-site Đã Tiếp Nhận:</h3>
            {onSiteOrders.map((o) => (
              <div key={o.id} className="p-4 rounded-xl border border-slate-200 bg-white flex justify-between items-center">
                <div>
                  <h4 className="font-bold text-slate-900">{o.code}</h4>
                  <p className="text-xs text-slate-500 mt-1">Khách hàng: {o.customer} • Thời gian: {o.time}</p>
                </div>
                <div className="text-right">
                  <span className="text-sm font-extrabold text-slate-900 block">{o.totalAmount.toLocaleString('vi-VN')} VNĐ</span>
                  <span className="inline-block mt-1 px-2.5 py-0.5 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">
                    Đã tạo tại điểm thăm
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* TAB 4: VISIT AUDIT HISTORY LOG */}
      {activeTab === 'history' && (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-5 space-y-4">
          <h2 className="text-lg font-bold text-slate-800">📜 Nhật Ký & Lịch Sử Viếng Thăm Điểm Bán (UC_CRM_096)</h2>
          <div className="space-y-3">
            {historyLogs.map((log) => (
              <div key={log.id} className="p-4 rounded-xl border border-slate-200 bg-slate-50 flex justify-between items-center">
                <div>
                  <h3 className="font-bold text-slate-900">{log.customer}</h3>
                  <p className="text-xs text-slate-500 mt-1">
                    Ngày: <span className="font-semibold text-slate-700">{log.date}</span> • NVKD: <span className="font-semibold text-slate-700">{log.sales}</span>
                  </p>
                  <p className="text-xs text-emerald-800 font-semibold mt-1">Check-in: {log.inGps}</p>
                  <p className="text-xs text-blue-800 font-semibold mt-0.5">Check-out: {log.outGps}</p>
                  <p className="text-xs text-slate-600 italic mt-1">Kết quả: "{log.outcome}"</p>
                </div>
                <div className="text-right">
                  <span className="px-3 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800">
                    {log.status}
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
