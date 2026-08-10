"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchAstAssets, type AstAssetDto } from "@/shared/api/ast-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function AstDepreciationPage() {
  const { can } = usePermissions();
  const canRead = can("ast.asset.read");

  const [assets, setAssets] = useState<AstAssetDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const list = await fetchAstAssets();
      setAssets(list);
    } catch (e) { setError((e as Error).message); }
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  if (!canRead) return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền truy cập Sổ Khấu Hao Tài Sản.</div>;

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Sổ & Trích Khấu Hao Tài Sản Cố Định (UC_AST_011–013)</h1>
          <p className="text-sm text-slate-500">Tính toán giá trị hao mòn lũy kế, giá trị còn lại và tự động sinh bút toán kế toán FIN Journal Entries.</p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void loadData()}>🔄 Tải lại</button>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}

      <section className={panel}>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-slate-900">Bảng Khấu Hao Tài Sản Theo Kỳ</h2>
          <span className="text-xs font-semibold text-slate-500">{assets.length} tài sản</span>
        </div>

        {loading ? (
          <div className="p-6 text-center text-sm text-slate-500">Đang tải sổ khấu hao...</div>
        ) : (
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã Tài Sản</th>
                  <th className={th}>Tên Tài Sản</th>
                  <th className={th}>Nguyên Giá (₫)</th>
                  <th className={th}>Thời Gian KH (Tháng)</th>
                  <th className={th}>Trạng Thái</th>
                </tr>
              </thead>
              <tbody>
                {assets.map((a) => (
                  <tr key={a.id} className="hover:bg-slate-50">
                    <td className={`${td} font-bold text-slate-900`}>{a.code}</td>
                    <td className={`${td} font-semibold text-slate-800`}>{a.name}</td>
                    <td className={`${td} font-bold text-slate-900`}>{(a.originalCost ?? 0).toLocaleString("vi-VN")} ₫</td>
                    <td className={`${td} font-medium text-slate-700`}>{a.usefulLifeMonths || 60} tháng</td>
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
