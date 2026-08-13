'use client';

import React, { useState } from 'react';
import {
  calculateWeightedQualityScore,
  validateOrderMoqCompliance,
} from '@/shared/api/pur-supplier-category-quality-pos-sync-helpers';

export default function PurSupplierCategoryQualityPosSyncPage() {
  const [activeTab, setActiveTab] = useState<'categories' | 'leadtimemoq' | 'quality' | 'possync'>('categories');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PUR_002: Phân loại nhóm NCC
  const [categories, setCategories] = useState([
    { id: 'cat-1', code: 'CAT-FOOD', name: 'Nhóm Nhà Cung Cấp Thực Phẩm Fresh', desc: 'Nông sản, thịt heo/bò tươi sống', active: true },
    { id: 'cat-2', code: 'CAT-PACKAGING', name: 'Nhóm Bao Bì & Vật Tư Tiêu Hao', desc: 'Bao bì hộp giấy, ly nhựa', active: true },
  ]);

  const [catForm, setCatForm] = useState({ code: '', name: '', desc: '' });

  const handleSaveCategory = (e: React.FormEvent) => {
    e.preventDefault();
    if (!catForm.code.trim() || !catForm.name.trim()) {
      showToast('Mã nhóm và tên nhóm NCC không được để trống.', 'error');
      return;
    }

    const created = {
      id: `cat-${Date.now()}`,
      code: catForm.code,
      name: catForm.name,
      desc: catForm.desc,
      active: true,
    };

    setCategories([...categories, created]);
    setCatForm({ code: '', name: '', desc: '' });
    showToast(`Đã thêm nhóm NCC [${created.name}] thành công!`, 'success');
  };

  // UC_PUR_004: Lead time & MOQ
  const [suppliers] = useState([
    { id: 'sup-1', code: 'SUP-001', name: 'Công Ty TNHH Thực Phẩm Sạch Vinamilk', leadTime: 3, moq: 100, mov: 10000000 },
    { id: 'sup-2', code: 'SUP-002', name: 'Tập Đoàn Nông Sản Trung Nguyên', leadTime: 5, moq: 50, mov: 5000000 },
  ]);

  const [testOrderQty, setTestOrderQty] = useState(80);
  const moqCheck = validateOrderMoqCompliance(testOrderQty, 100);

  // UC_PUR_005: Đánh giá chất lượng NCC
  const [evalForm, setEvalForm] = useState({ onTime: 95, quality: 90, price: 88, comments: 'Giao hàng đúng hẹn, tỷ lệ lỗi thấp' });
  const qualityScore = calculateWeightedQualityScore(evalForm.onTime, evalForm.quality, evalForm.price);

  const handleSaveEvaluation = (e: React.FormEvent) => {
    e.preventDefault();
    showToast(`✓ Đã lưu phiếu đánh giá chất lượng NCC! Xếp loại Hạng ${qualityScore.grade} (${qualityScore.overallScore} điểm)`, 'success');
  };

  // UC_POS_060: Đồng bộ đơn POS sang CRM
  const [syncHistory, setSyncHistory] = useState([
    { id: 'syn-1', orderCode: 'POS-ORD-20260813-01', customerName: 'Anh Hùng', status: 'Đã đồng bộ CRM', code: 'CRM-ACT-2026081301', time: '14:20' },
  ]);

  const handleSyncPosOrder = () => {
    const newSync = {
      id: `syn-${Date.now()}`,
      orderCode: `POS-ORD-${Math.floor(100000 + Math.random() * 900000)}`,
      customerName: 'Khách Hàng Thân Thiết POS',
      status: 'Đã đồng bộ CRM',
      code: `CRM-ACT-${Date.now()}`,
      time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
    };
    setSyncHistory([newSync, ...syncHistory]);
    showToast(`✓ Đã tự động đồng bộ đơn hàng POS sang hồ sơ CRM khách hàng [${newSync.code}]!`, 'success');
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
              PUR & POS - SUPPLIER MANAGEMENT & CRM SYNC
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Phân Loại Nhóm NCC, Đánh Giá Chất Lượng & Đồng Bộ Đơn POS-CRM</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Quản lý phân loại NCC, kiểm soát Lead Time / MOQ mua hàng, đánh giá KPI chất lượng NCC và đồng bộ đơn POS sang CRM
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
            onClick={() => setActiveTab('categories')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'categories' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🏷️ UC_PUR_002: Phân Loại Nhóm Nhà Cung Cấp
          </button>
          <button
            onClick={() => setActiveTab('leadtimemoq')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'leadtimemoq' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⏱️ UC_PUR_004: Quản Lý Lead Time & MOQ
          </button>
          <button
            onClick={() => setActiveTab('quality')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'quality' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ⭐ UC_PUR_005: Đánh Giá Chất Lượng NCC
          </button>
          <button
            onClick={() => setActiveTab('possync')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'possync' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            🔄 UC_POS_060: Đồng Bộ Đơn POS Sang CRM
          </button>
        </div>
      </div>

      {activeTab === 'categories' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 bg-surface rounded-xl shadow-sm border border-border p-5 space-y-4">
            <h2 className="text-lg font-bold text-foreground">🏷️ Phân Loại Nhóm Nhà Cung Cấp (UC_PUR_002)</h2>
            <div className="space-y-3">
              {categories.map((c) => (
                <div key={c.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 flex justify-between items-center">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{c.code}</span>
                      <h3 className="font-bold text-foreground">{c.name}</h3>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">{c.desc}</p>
                  </div>
                  <span className="px-2 py-0.5 text-xs font-bold rounded bg-emerald-100 text-emerald-800">
                    Hoạt động
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-surface rounded-xl shadow-sm border border-border p-5">
            <h2 className="text-lg font-bold text-foreground mb-4">➕ Thêm Nhóm NCC Mới</h2>
            <form onSubmit={handleSaveCategory} className="space-y-4 text-sm">
              <div>
                <label className="block text-foreground font-medium mb-1">Mã nhóm NCC:</label>
                <input
                  type="text"
                  value={catForm.code}
                  onChange={(e) => setCatForm({ ...catForm, code: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: CAT-MEAT"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Tên nhóm NCC:</label>
                <input
                  type="text"
                  value={catForm.name}
                  onChange={(e) => setCatForm({ ...catForm, name: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                  placeholder="VD: Nhóm Thịt Tươi Sống"
                />
              </div>
              <div>
                <label className="block text-foreground font-medium mb-1">Mô tả chi tiết:</label>
                <textarea
                  rows={2}
                  value={catForm.desc}
                  onChange={(e) => setCatForm({ ...catForm, desc: e.target.value })}
                  className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
                />
              </div>
              <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
                Lưu Nhóm NCC
              </button>
            </form>
          </div>
        </div>
      )}

      {activeTab === 'leadtimemoq' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div>
            <h2 className="text-lg font-bold text-foreground">⏱️ Quản Lý Thời Gian Giao Hàng (Lead Time) & Số Lượng Đặt Tối Thiểu (MOQ) (UC_PUR_004)</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Kiểm soát cam kết thời gian giao hàng và hạn mức MOQ/MOV khi lập PO mua hàng</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {suppliers.map((s) => (
              <div key={s.id} className="p-4 rounded-xl border border-border bg-surface-hover/50 space-y-3">
                <div className="flex justify-between items-center">
                  <span className="px-2 py-0.5 text-xs font-bold rounded bg-brand-muted text-brand-strong">{s.code}</span>
                  <span className="text-xs font-bold text-emerald-700 bg-emerald-100 px-2 py-0.5 rounded">Lead time: {s.leadTime} ngày</span>
                </div>
                <h3 className="font-bold text-foreground text-sm">{s.name}</h3>
                <div className="pt-2 border-t border-border flex justify-between text-xs text-muted-foreground font-medium">
                  <span>MOQ: <b className="text-foreground">{s.moq} đơn vị</b></span>
                  <span>MOV: <b className="text-foreground">{s.mov.toLocaleString('vi-VN')} đ</b></span>
                </div>
              </div>
            ))}
          </div>

          <div className="p-4 rounded-xl border border-border bg-surface max-w-md space-y-3">
            <h3 className="text-sm font-bold text-foreground">🧪 Kiểm Tra Hạn Mức Đặt Hàng Mua (MOQ Validation Test)</h3>
            <div className="flex items-center gap-2">
              <span className="text-xs text-muted-foreground">Nhập số lượng đặt mua:</span>
              <input
                type="number"
                value={testOrderQty}
                onChange={(e) => setTestOrderQty(Number(e.target.value))}
                className="w-24 border border-border rounded p-1.5 font-bold text-center bg-surface text-foreground text-sm"
              />
              <span className="text-xs font-semibold text-slate-700">(MOQ yêu cầu: 100)</span>
            </div>
            {!moqCheck.isCompliant && (
              <div className="p-2.5 rounded-lg bg-rose-100 border border-rose-300 text-rose-800 text-xs font-bold">
                ⚠️ Cảnh báo: Số lượng đặt chưa đủ MOQ! Còn thiếu {moqCheck.deficit} đơn vị nữa.
              </div>
            )}
            {moqCheck.isCompliant && (
              <div className="p-2.5 rounded-lg bg-emerald-100 border border-emerald-300 text-emerald-800 text-xs font-bold">
                ✓ Thỏa mãn điều kiện MOQ tối thiểu của nhà cung cấp.
              </div>
            )}
          </div>
        </div>
      )}

      {activeTab === 'quality' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 max-w-xl space-y-4">
          <h2 className="text-lg font-bold text-foreground">⭐ Đánh Giá Chất Lượng & Hiệu Suất Nhà Cung Cấp Định Kỳ (UC_PUR_005)</h2>
          <form onSubmit={handleSaveEvaluation} className="space-y-4 text-sm">
            <div>
              <label className="block text-foreground font-medium mb-1">Điểm đúng hạn giao hàng (On-Time Delivery 0-100):</label>
              <input
                type="number"
                max={100}
                value={evalForm.onTime}
                onChange={(e) => setEvalForm({ ...evalForm, onTime: Number(e.target.value) })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Điểm tuân thủ chất lượng (Quality Compliance 0-100):</label>
              <input
                type="number"
                max={100}
                value={evalForm.quality}
                onChange={(e) => setEvalForm({ ...evalForm, quality: Number(e.target.value) })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>
            <div>
              <label className="block text-foreground font-medium mb-1">Điểm cạnh tranh giá (Price Competitiveness 0-100):</label>
              <input
                type="number"
                max={100}
                value={evalForm.price}
                onChange={(e) => setEvalForm({ ...evalForm, price: Number(e.target.value) })}
                className="w-full border border-border rounded-lg p-2 bg-surface text-foreground"
              />
            </div>

            <div className="p-4 rounded-xl bg-brand-muted border border-brand/30 flex justify-between items-center">
              <div>
                <span className="text-xs text-muted-foreground block font-semibold">TỔNG ĐIỂM KPI & XẾP HẠNG NCC</span>
                <span className="text-xl font-extrabold text-brand-strong mt-0.5 block">{qualityScore.overallScore} / 100 điểm</span>
              </div>
              <span className="px-3 py-1 text-sm font-extrabold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                Xếp Hạng {qualityScore.grade}
              </span>
            </div>

            <button type="submit" className="w-full py-2.5 bg-brand text-brand-foreground rounded-lg font-semibold hover:opacity-90">
              Lưu Phiếu Đánh Giá Chất Lượng
            </button>
          </form>
        </div>
      )}

      {activeTab === 'possync' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-6">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-lg font-bold text-foreground">🔄 Tự Động Đồng Bộ Đơn Hàng POS Sang Hồ Sơ CRM (UC_POS_060)</h2>
              <p className="text-xs text-muted-foreground mt-0.5">Tự động đẩy giao dịch bán lẻ POS sang lịch sử tương tác 360 độ của CRM</p>
            </div>
            <button
              onClick={handleSyncPosOrder}
              className="px-4 py-2 bg-brand text-brand-foreground rounded-lg font-bold text-sm hover:opacity-90 shadow-sm"
            >
              🔄 Kích Hoạt Đồng Bộ Đơn POS Mới
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Đơn POS</th>
                  <th className="p-3">Khách Hàng</th>
                  <th className="p-3">Mã Ghi Nhận CRM</th>
                  <th className="p-3 text-right">Trạng Thái Đồng Bộ</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {syncHistory.map((s) => (
                  <tr key={s.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-bold text-foreground">{s.orderCode}</td>
                    <td className="p-3 text-slate-700">{s.customerName}</td>
                    <td className="p-3 font-mono text-xs text-brand-strong">{s.code}</td>
                    <td className="p-3 text-right">
                      <span className="px-2 py-0.5 text-xs font-bold rounded bg-emerald-100 text-emerald-800">
                        {s.status} ({s.time})
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
