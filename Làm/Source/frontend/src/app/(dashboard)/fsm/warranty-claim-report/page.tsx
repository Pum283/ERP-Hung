'use client';

import React, { useState } from 'react';
import {
  formatWarrantyApprovalRate,
  formatClaimAmount,
} from '@/shared/api/fsm-warranty-claim-report-helpers';

export default function FsmWarrantyClaimReportPage() {
  const [report] = useState({
    period: 'Tháng 08/2026',
    totalClaims: 35,
    approved: 32,
    rejected: 3,
    rate: 91.4,
    coveredAmount: 155000000,
  });

  const [claims] = useState([
    { id: 'c-1', claimNo: 'CLM-2026-0810-01', sn: 'SN-RACK-42U-00129', item: 'Bộ nguồn Redundant Power Supply 800W', cost: 12000000, status: 'Approved', note: 'Chập cầu chì trong thời hạn 24 tháng bảo hành tiêu chuẩn' },
    { id: 'c-2', claimNo: 'CLM-2026-0812-04', sn: 'SN-CNC-MILL-508', item: 'Bo mạch điều khiển Servo Amplifier', cost: 45000000, status: 'Approved', note: 'Lỗi linh kiện từ nhà sản xuất, miễn phí 100%' },
    { id: 'c-3', claimNo: 'CLM-2026-0814-08', sn: 'SN-TRANS-2000KVA', item: 'Cảm biến nhiệt độ dầu máy biến áp', cost: 8500000, status: 'Rejected', note: 'Hư hỏng do sét đánh lan truyền ngoài phạm vi bảo hành' },
  ]);

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      <div className="bg-surface border border-border p-6 rounded-2xl shadow-sm">
        <div className="flex justify-between items-center">
          <div>
            <span className="bg-brand-muted text-brand-strong text-xs px-3 py-1 rounded-full font-semibold border border-brand/30">
              FSM - WARRANTY CLAIMS & COVERAGE REPORT
            </span>
            <h1 className="text-2xl font-bold font-display text-foreground mt-2">Báo Cáo Yêu Cầu & Chi Phí Bảo Hành Thiết Bị (UC_FSM_049)</h1>
            <p className="text-muted-foreground text-sm mt-1">
              Thống kê tỷ lệ duyệt bảo hành, chi phí hãng/nhà phân phối chi trả và phân loại lý do từ chối bảo hành
            </p>
          </div>
          <div className="text-right">
            <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-brand-muted text-brand-strong border border-brand/30">
              ● Tiến độ 90% (UC_FSM_049)
            </span>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-4 gap-4">
        <div className="p-4 rounded-xl border border-border bg-surface">
          <div className="text-xs text-muted-foreground font-semibold">Tổng Yêu Cầu (Claims)</div>
          <div className="text-2xl font-black text-foreground mt-1">{report.totalClaims} ca</div>
        </div>
        <div className="p-4 rounded-xl border border-border bg-surface">
          <div className="text-xs text-muted-foreground font-semibold">Đã Chấp Thuận Bảo Hành</div>
          <div className="text-2xl font-black text-emerald-700 mt-1">{report.approved} ca</div>
        </div>
        <div className="p-4 rounded-xl border border-border bg-surface">
          <div className="text-xs text-muted-foreground font-semibold">Tỷ Lệ Duyệt Bảo Hành</div>
          <div className="text-2xl font-black text-brand mt-1">{formatWarrantyApprovalRate(report.rate)}</div>
        </div>
        <div className="p-4 rounded-xl border border-border bg-surface">
          <div className="text-xs text-muted-foreground font-semibold">Tổng Chi Phí Hãng Chi Trả</div>
          <div className="text-2xl font-black text-blue-700 mt-1">{formatClaimAmount(report.coveredAmount)}</div>
        </div>
      </div>

      <div className="bg-surface rounded-xl shadow-sm border border-border p-6 space-y-4">
        <h2 className="text-lg font-bold text-foreground">📑 Danh Sách Chi Tiết Yêu Cầu Bảo Hành Phát Sinh Trong Kỳ</h2>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm border-collapse">
            <thead>
              <tr className="border-b border-border bg-surface-hover text-muted-foreground font-semibold">
                <th className="p-3">Mã Khiếu Nại</th>
                <th className="p-3">Số Serial Thiết Bị</th>
                <th className="p-3">Hạng Mục / Linh Kiện Yêu Cầu</th>
                <th className="p-3 text-right">Chi Phí Bảo Hành</th>
                <th className="p-3">Giải Trình Kỹ Thuật</th>
                <th className="p-3 text-right">Kết Quả Thẩm Định</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {claims.map((c) => (
                <tr key={c.id} className="hover:bg-surface-hover/50">
                  <td className="p-3 font-mono font-bold text-brand">{c.claimNo}</td>
                  <td className="p-3 font-mono font-bold text-slate-800">{c.sn}</td>
                  <td className="p-3 font-semibold text-foreground">{c.item}</td>
                  <td className="p-3 text-right font-black text-slate-800">{c.cost.toLocaleString('vi-VN')} đ</td>
                  <td className="p-3 text-xs text-muted-foreground">{c.note}</td>
                  <td className="p-3 text-right">
                    <span className={`px-2.5 py-1 text-xs font-bold rounded-full border ${c.status === 'Approved' ? 'bg-emerald-100 text-emerald-800 border-emerald-300' : 'bg-rose-100 text-rose-800 border-rose-300'}`}>
                      {c.status === 'Approved' ? '✓ Chấp Thuận' : '✕ Từ Chối'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
