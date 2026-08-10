"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchFsmAssets, type FsmAssetDto } from "@/shared/api/fsm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function FsmAssetsPage() {
  const { can } = usePermissions();
  const canRead = can("fsm.asset.read");

  const [assets, setAssets] = useState<FsmAssetDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const list = await fetchFsmAssets();
      setAssets(list);
    } catch (e) { setError((e as Error).message); }
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  if (!canRead) return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền truy cập Thiết Bị Khách Hàng FSM.</div>;

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Thiết Bị Khách Hàng & Bảo Hành (Install Base) (UC_FSM_008–010)</h1>
          <p className="text-sm text-slate-500">Quản lý các máy móc/thiết bị đã cài đặt cho khách hàng, thời hạn bảo hành và lịch sử sửa chữa.</p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void loadData()}>🔄 Tải lại</button>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}

      <section className={panel}>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-slate-900">Danh Sách Thiết Bị Khách Hàng (Installed Base)</h2>
          <span className="text-xs font-semibold text-slate-500">{assets.length} thiết bị</span>
        </div>

        {loading ? (
          <div className="p-6 text-center text-sm text-slate-500">Đang tải danh sách thiết bị...</div>
        ) : (
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã Thiết Bị</th>
                  <th className={th}>Tên / Model</th>
                  <th className={th}>Số Serial (S/N)</th>
                  <th className={th}>Hạn Bảo Hành</th>
                  <th className={th}>Trạng Thái</th>
                </tr>
              </thead>
              <tbody>
                {assets.map((a) => (
                  <tr key={a.id} className="hover:bg-slate-50">
                    <td className={`${td} font-bold text-slate-900`}>{a.code}</td>
                    <td className={`${td} font-semibold text-slate-800`}>{a.name}</td>
                    <td className={`${td} font-mono font-medium text-indigo-900`}>{a.serialNo || "—"}</td>
                    <td className={td}>
                      {a.warrantyEndDate ? new Date(a.warrantyEndDate).toLocaleDateString("vi-VN") : "Hết bảo hành"}
                    </td>
                    <td className={td}><span className={statusPill(a.status === "Active" ? "success" : "brand")}>{a.status}</span></td>
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
