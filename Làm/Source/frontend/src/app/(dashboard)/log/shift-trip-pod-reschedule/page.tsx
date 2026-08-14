'use client';

import React, { useState } from 'react';
import {
  getTripStatusBadge,
  formatShiftTiming,
} from '@/shared/api/log-shift-trip-pod-reschedule-helpers';

export default function LogShiftTripPodReschedulePage() {
  const [activeTab, setActiveTab] = useState<'shifts' | 'trips' | 'pod' | 'redelivery'>('shifts');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_LOG_005: Cấu hình ca giao hàng
  const [shifts] = useState([
    { id: 's-1', code: 'SHIFT-MORNING', name: 'Ca Sáng (08:00 - 12:00)', start: '08:00', end: '12:00', cap: 35 },
    { id: 's-2', code: 'SHIFT-AFTERNOON', name: 'Ca Chiều (13:30 - 17:30)', start: '13:30', end: '17:30', cap: 30 },
    { id: 's-3', code: 'SHIFT-EVENING', name: 'Ca Tối (18:00 - 21:00)', start: '18:00', end: '21:00', cap: 15 },
  ]);

  // UC_LOG_007: Gộp nhiều đơn thành chuyến
  const [tripForm, setTripForm] = useState({
    driver: 'Trần Văn Tài',
    plate: '51D-889.99',
    orders: 6,
    weight: 1450,
  });

  const handleCreateTrip = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã tạo chuyến xe TRIP-20260814 gộp [${tripForm.orders}] đơn cho tài xế [${tripForm.driver}]!`, 'success');
  };

  // UC_LOG_016: Chứng từ ký nhận (POD)
  const [podForm, setPodForm] = useState({
    orderNo: 'DEL-2026-088',
    recipient: 'Nguyễn Văn Nhận',
    phone: '0909887766',
    notes: 'Khách kiểm tra hàng đầy đủ, ký nhận thành công',
  });

  const handleSubmitPod = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã xác nhận chứng từ ký nhận POD cho lệnh giao [${podForm.orderNo}]!`, 'success');
  };

  // UC_LOG_018: Hẹn giao lại
  const [redelivForm, setRedelivForm] = useState({
    orderNo: 'DEL-2026-092',
    reason: 'Khách hàng đi công tác, hẹn giao lại vào thứ 2',
    shift: 'Ca Sáng',
  });

  const handleCreateRedelivery = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã lên lịch hẹn giao lại cho đơn [${redelivForm.orderNo}]!`, 'success');
  };

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
              LOG - DELIVERY SHIFTS, TRIP CONSOLIDATION, POD & REDELIVERY
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Cấu Hình Ca Giao, Gộp Chuyến, Chứng Từ POD & Hẹn Giao Lại</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Điều phối chuyến xe đa đơn hàng, quản lý năng lực ca giao, chứng từ ký nhận điện tử và xử lý hẹn giao lại
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs LOG)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('shifts')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'shifts' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⏰ UC_LOG_005: Cấu Hình Ca Giao
          </button>
          <button
            onClick={() => setActiveTab('trips')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'trips' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🚚 UC_LOG_007: Gộp Đơn Thành Chuyến
          </button>
          <button
            onClick={() => setActiveTab('pod')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'pod' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ✍️ UC_LOG_016: Chứng Từ Ký Nhận POD
          </button>
          <button
            onClick={() => setActiveTab('redelivery')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'redelivery' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔄 UC_LOG_018: Hẹn Giao Lại
          </button>
        </div>
      </div>

      {activeTab === 'shifts' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">⏰ Danh Mục Ca Giao Hàng & Năng Lực Tiếp Nhận (UC_LOG_005)</h2>
          <div className="grid grid-cols-3 gap-4">
            {shifts.map((s) => (
              <div key={s.id} className="p-5 rounded-xl border border-border bg-surface shadow-sm space-y-3">
                <div className="flex justify-between items-start">
                  <div>
                    <span className="text-xs font-mono font-bold text-brand">{s.code}</span>
                    <h3 className="text-base font-bold text-foreground mt-0.5">{s.name}</h3>
                  </div>
                  <span className="px-2.5 py-1 bg-emerald-100 text-emerald-800 text-xs font-bold rounded-full border border-emerald-300">
                    ● Hoạt Động
                  </span>
                </div>
                <div className="flex justify-between items-center text-sm pt-2 border-t border-border">
                  <span className="text-muted-foreground">Khung giờ:</span>
                  <span className="font-bold text-foreground">{formatShiftTiming(s.start, s.end)}</span>
                </div>
                <div className="flex justify-between items-center text-sm">
                  <span className="text-muted-foreground">Công suất tối đa:</span>
                  <span className="font-extrabold text-brand">{s.cap} Đơn / Ca</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'trips' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">🚚 Điều Phối Gộp Nhiều Đơn Vào Một Chuyến Xe (UC_LOG_007)</h2>
          <form onSubmit={handleCreateTrip} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Tài Xế Phụ Trách:</label>
                <input type="text" value={tripForm.driver} onChange={(e) => setTripForm({ ...tripForm, driver: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Biển Số Xe:</label>
                <input type="text" value={tripForm.plate} onChange={(e) => setTripForm({ ...tripForm, plate: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Số Lượng Đơn Hàng Gộp:</label>
                <input type="number" value={tripForm.orders} onChange={(e) => setTripForm({ ...tripForm, orders: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tổng Khối Lượng Hàng (Kg):</label>
                <input type="number" value={tripForm.weight} onChange={(e) => setTripForm({ ...tripForm, weight: Number(e.target.value) })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-extrabold" />
              </div>
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              🚚 Khởi Tạo & Phát Lệnh Chuyến Giao Hàng Mới
            </button>
          </form>
        </div>
      )}

      {activeTab === 'pod' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">✍️ Xác Nhận Chứng Từ Ký Nhận Điện Tử (POD - UC_LOG_016)</h2>
          <form onSubmit={handleSubmitPod} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Lệnh Giao Hàng:</label>
                <input type="text" value={podForm.orderNo} onChange={(e) => setPodForm({ ...podForm, orderNo: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Số Điện Thoại Người Nhận:</label>
                <input type="text" value={podForm.phone} onChange={(e) => setPodForm({ ...podForm, phone: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
              </div>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Họ Tên Người Nhận Hàng Thực Tế:</label>
              <input type="text" value={podForm.recipient} onChange={(e) => setPodForm({ ...podForm, recipient: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold" />
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Ghi Chú & Tình Trạng Kiện Hàng:</label>
              <textarea value={podForm.notes} onChange={(e) => setPodForm({ ...podForm, notes: e.target.value })} rows={2} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
            </div>

            <div className="p-4 rounded-xl border border-dashed border-border bg-surface-hover text-center space-y-1">
              <div className="text-2xl">🖋️</div>
              <div className="text-xs font-bold text-foreground">Chữ Ký Điện Tử & Ảnh Chụp Bàn Giao Đã Sẵn Sàng</div>
              <div className="text-xs text-muted-foreground">Tự động đồng bộ hóa lên hệ thống ERP Cloud Storage</div>
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              💾 Lưu & Xác Nhận Giao Hàng Thành Công (POD)
            </button>
          </form>
        </div>
      )}

      {activeTab === 'redelivery' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-2xl space-y-6">
          <h2 className="text-lg font-bold text-foreground">🔄 Xử Lý Yêu Cầu Hẹn Giao Lại Đơn Hàng (UC_LOG_018)</h2>
          <form onSubmit={handleCreateRedelivery} className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã Lệnh Giao Cần Hẹn Lại:</label>
                <input type="text" value={redelivForm.orderNo} onChange={(e) => setRedelivForm({ ...redelivForm, orderNo: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-mono font-bold" />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Ca Giao Mong Muốn:</label>
                <select value={redelivForm.shift} onChange={(e) => setRedelivForm({ ...redelivForm, shift: e.target.value })} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground font-bold">
                  <option value="Ca Sáng">Ca Sáng (08:00 - 12:00)</option>
                  <option value="Ca Chiều">Ca Chiều (13:30 - 17:30)</option>
                  <option value="Ca Tối">Ca Tối (18:00 - 21:00)</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-foreground font-medium mb-1">Lý Do Giao Hàng Không Thành Công:</label>
              <textarea value={redelivForm.reason} onChange={(e) => setRedelivForm({ ...redelivForm, reason: e.target.value })} rows={2} className="w-full border border-border rounded-lg p-2 bg-surface text-foreground" />
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-bold hover:opacity-90 shadow-sm">
              🔄 Lưu Lịch Hẹn & Chuyển Trạng Thái Chờ Điều Phối Lại
            </button>
          </form>
        </div>
      )}
    </div>
  );
}
