"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchLogDeliveries, type LogDeliveryDto } from "@/shared/api/log-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function LogTrackingPage() {
  const { can } = usePermissions();
  const canRead = can("log.delivery.read");

  const [deliveries, setDeliveries] = useState<LogDeliveryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const list = await fetchLogDeliveries();
      setDeliveries(list);
    } catch (e) { setError((e as Error).message); }
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  if (!canRead) return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền truy cập Giám sát Vận đơn.</div>;

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Giám Sát Vận Đơn & Trạng Thái Giao Hàng (UC_LOG_011–015)</h1>
          <p className="text-sm text-slate-500">Theo dõi hành trình đơn vận chuyển, hãng vận chuyển (Carrier) và trạng thái thu hộ COD.</p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void loadData()}>🔄 Tải lại</button>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}

      <section className={panel}>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-slate-900">Danh Sách Vận Đơn Đang Theo Dõi</h2>
          <span className="text-xs font-semibold text-slate-500">{deliveries.length} vận đơn</span>
        </div>

        {loading ? (
          <div className="p-6 text-center text-sm text-slate-500">Đang tải trạng thái giao hàng...</div>
        ) : (
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã Vận Đơn</th>
                  <th className={th}>Số Tracking / Waybill</th>
                  <th className={th}>Địa Chỉ Giao Hàng</th>
                  <th className={th}>Thu Hộ COD (₫)</th>
                  <th className={th}>Trạng Thái Chặng</th>
                </tr>
              </thead>
              <tbody>
                {deliveries.map((d) => (
                  <tr key={d.id} className="hover:bg-slate-50">
                    <td className={`${td} font-bold text-slate-900`}>{d.code}</td>
                    <td className={`${td} font-mono font-medium text-indigo-900`}>{d.waybillNo || "—"}</td>
                    <td className={`${td} text-slate-700 max-w-xs truncate`}>{d.recipientAddress || "Mặc định"}</td>
                    <td className={`${td} font-bold text-amber-700`}>{(d.codAmount ?? 0).toLocaleString("vi-VN")} ₫</td>
                    <td className={td}>
                      <span className={statusPill(d.status === "Delivered" ? "success" : "brand")}>{d.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
