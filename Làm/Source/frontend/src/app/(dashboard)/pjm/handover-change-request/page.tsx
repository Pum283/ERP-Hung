'use client';

import React, { useState } from 'react';
import {
  formatEcrImpactSummary,
  formatAttachmentSize,
} from '@/shared/api/pjm-handover-change-request-helpers';

export default function PjmHandoverChangeRequestPage() {
  const [activeTab, setActiveTab] = useState<'handover' | 'protocol' | 'ecr' | 'approval'>('handover');

  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // UC_PJM_027: Checklist bàn giao
  const [handovers] = useState([
    { id: 'h-1', prj: 'PRJ-2026-088', criteria: '1. Bàn giao đầy đủ hồ sơ hoàn công và sơ đồ nguyên lý đấu nối', pass: true, signer: 'Đại diện Chủ đầu tư FPT', date: '2026-08-14' },
    { id: 'h-2', prj: 'PRJ-2026-088', criteria: '2. Đào tạo chuyển giao công nghệ vận hành tủ điện phân phối MSB', pass: true, signer: 'Kỹ sư vận hành nhà máy FPT', date: '2026-08-14' },
  ]);

  // UC_PJM_028: Ghi nhận ảnh / biên bản
  const [attachments] = useState([
    { id: 'att-1', prj: 'PRJ-2026-088', title: 'Biên bản nghiệm thu đóng điện trạm biến áp có chữ ký CĐT', type: 'ProtocolPdf', url: '/uploads/pjm/protocols/prj-088-handover-signed.pdf', size: 2450000, date: '2026-08-14' },
    { id: 'att-2', prj: 'PRJ-2026-088', title: 'Ảnh chụp hiện trường đóng điện tủ MSB an toàn', type: 'PhotoJpg', url: '/uploads/pjm/photos/prj-088-commissioning.jpg', size: 3850000, date: '2026-08-14' },
  ]);

  // UC_PJM_029: Phát sinh change request
  const [ecrs, setEcrs] = useState([
    { id: 'ecr-1', no: 'ECR-20260814-01', prj: 'PRJ-2026-088', title: 'Bổ sung tủ tụ bù hạ thế 250kVAR', reason: 'Khách hàng mở rộng xưởng sản xuất và nâng hệ số cos phi', cost: 85000000, days: 5, status: 'Submitted' },
  ]);

  // UC_PJM_030: Duyệt change request
  const [approvals, setApprovals] = useState([
    { id: 'app-1', ecr: 'ECR-20260814-01', approved: true, cost: 85000000, days: 5, approver: 'Giám Đốc Ban Dự Án', comments: 'Đồng ý bổ sung phạm vi công việc và điều chỉnh phụ lục hợp đồng', date: '2026-08-14' },
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
              PJM - HANDOVER PROTOCOLS & ENGINEERING CHANGE REQUESTS (ECR)
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Checklist Bàn Giao Dự Án, Lưu Trữ Biên Bản & Quản Lý Yêu Cầu Thay Đổi Thiết Kế (ECR)</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Số hóa tiêu chí bàn giao nghiệm thu đưa vào sử dụng, lưu trữ hồ sơ hoàn công/ảnh hiện trường và quy trình phê duyệt thay đổi phạm vi & ngân sách
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (4/4 UCs PJM)
            </span>
          </div>
        </div>

        <div className="flex space-x-2 mt-6 border-t border-border pt-4">
          <button
            onClick={() => setActiveTab('handover')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'handover' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📋 UC_PJM_027: Checklist Bàn Giao
          </button>
          <button
            onClick={() => setActiveTab('protocol')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'protocol' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📸 UC_PJM_028: Biên Bản & Ảnh
          </button>
          <button
            onClick={() => setActiveTab('ecr')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'ecr' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            📝 UC_PJM_029: Phát Sinh ECR
          </button>
          <button
            onClick={() => setActiveTab('approval')}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              activeTab === 'approval' ? 'bg-brand text-brand-foreground shadow-sm' : 'text-muted-foreground hover:bg-surface-hover'
            }`}
          >
            ✓ UC_PJM_030: Duyệt ECR
          </button>
        </div>
      </div>

      {activeTab === 'handover' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📋 Biên Bản & Checklist Bàn Giao Đưa Vào Vận Hành (UC_PJM_027)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Dự Án</th>
                  <th className="p-3">Hạng Mục / Tiêu Chí Bàn Giao</th>
                  <th className="p-3">Đại Diện Khách Hàng / CĐT Ký Nhận</th>
                  <th className="p-3">Ngày Ký</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {handovers.map((h) => (
                  <tr key={h.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{h.prj}</td>
                    <td className="p-3 font-bold text-foreground">{h.criteria}</td>
                    <td className="p-3 text-slate-800 font-medium">{h.signer}</td>
                    <td className="p-3 text-slate-700">{h.date}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Đã Nghiệm Thu
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'protocol' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📸 Hồ Sơ Ảnh Hiện Trường & File Biên Bản Nghiệm Thu (UC_PJM_028)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã Dự Án</th>
                  <th className="p-3">Tiêu Đề Hồ Sơ / Ảnh Đính Kèm</th>
                  <th className="p-3 text-center">Định Dạng</th>
                  <th className="p-3 text-right">Dung Lượng</th>
                  <th className="p-3">Đường Dẫn Lưu Trữ</th>
                  <th className="p-3 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {attachments.map((a) => (
                  <tr key={a.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{a.prj}</td>
                    <td className="p-3 font-bold text-foreground">{a.title}</td>
                    <td className="p-3 text-center">
                      <span className={`px-2 py-0.5 text-xs font-bold rounded ${a.type === 'ProtocolPdf' ? 'bg-rose-100 text-rose-800' : 'bg-blue-100 text-blue-800'}`}>
                        {a.type === 'ProtocolPdf' ? 'PDF Doc' : 'JPG Photo'}
                      </span>
                    </td>
                    <td className="p-3 text-right text-slate-700 font-mono">{formatAttachmentSize(a.size)}</td>
                    <td className="p-3 font-mono text-xs text-muted-foreground">{a.url}</td>
                    <td className="p-3 text-right">
                      <button onClick={() => showToast(`Tải xuống [${a.title}]...`)} className="px-3 py-1 bg-brand text-brand-foreground text-xs font-bold rounded hover:opacity-90">
                        ⬇ Tải Về
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'ecr' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">📝 Danh Sách Yêu Cầu Thay Đổi Thiết Kế ECR (UC_PJM_029)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã ECR</th>
                  <th className="p-3">Dự Án</th>
                  <th className="p-3">Nội Dung Yêu Cầu Thay Đổi</th>
                  <th className="p-3">Lý Do Phát Sinh</th>
                  <th className="p-3 text-right">Tác Động Dự Kiến</th>
                  <th className="p-3 text-right">Trạng Thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {ecrs.map((e) => (
                  <tr key={e.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{e.no}</td>
                    <td className="p-3 font-mono font-bold text-slate-800">{e.prj}</td>
                    <td className="p-3 font-bold text-foreground">{e.title}</td>
                    <td className="p-3 text-xs text-slate-700">{e.reason}</td>
                    <td className="p-3 text-right font-black text-rose-700">{formatEcrImpactSummary(e.cost, e.days)}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-amber-100 text-amber-800 border border-amber-300">
                        ● Chờ Giám Đốc Duyệt
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === 'approval' && (
        <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
          <h2 className="text-lg font-bold text-foreground">✓ Quyết Định Phê Duyệt & Điều Chỉnh Hợp Đồng (UC_PJM_030)</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                  <th className="p-3">Mã ECR</th>
                  <th className="p-3">Chi Phí Bổ Sung</th>
                  <th className="p-3">Tiến Độ Gia Hạn</th>
                  <th className="p-3">Người Phê Duyệt</th>
                  <th className="p-3">Ý Kiến / Căn Cứ Pháp Lý</th>
                  <th className="p-3 text-right">Quyết Định</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {approvals.map((ap) => (
                  <tr key={ap.id} className="hover:bg-surface-hover/50">
                    <td className="p-3 font-mono font-bold text-brand">{ap.ecr}</td>
                    <td className="p-3 font-bold text-emerald-700">+{ap.cost.toLocaleString('vi-VN')} đ</td>
                    <td className="p-3 font-bold text-blue-700">+{ap.days} ngày</td>
                    <td className="p-3 font-medium text-slate-800">{ap.approver}</td>
                    <td className="p-3 text-xs text-muted-foreground">{ap.comments}</td>
                    <td className="p-3 text-right">
                      <span className="px-2.5 py-1 text-xs font-bold rounded-full bg-emerald-100 text-emerald-800 border border-emerald-300">
                        ✓ Đã Phê Duyệt
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
