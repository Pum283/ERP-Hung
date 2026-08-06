"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPosPromotions,
  fetchPosVouchers,
  upsertPosPromotion,
  upsertPosVoucher,
  type PosPromotionDto,
  type PosVoucherDto,
} from "@/shared/api/pos-promo-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PosPromosPage() {
  const { can } = usePermissions();
  const canRead = can("pos.promo.read");
  const canManage = can("pos.promo.manage");

  const [promos, setPromos] = useState<PosPromotionDto[]>([]);
  const [vouchers, setVouchers] = useState<PosVoucherDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [code, setCode] = useState("KM10");
  const [name, setName] = useState("Giảm 10%");
  const [dtype, setDtype] = useState("Percent");
  const [dval, setDval] = useState("10");
  const [minAmt, setMinAmt] = useState("0");
  const [vCode, setVCode] = useState("SAVE10");
  const [vMax, setVMax] = useState("100");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [p, v] = await Promise.all([fetchPosPromotions(), fetchPosVouchers()]);
    setPromos(p);
    setVouchers(v);
    if (!selectedId && p[0]) setSelectedId(p[0].id);
  }, [selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  const selected = promos.find((p) => p.id === selectedId);

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem khuyến mại POS.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Khuyến mại POS</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          CTKM · voucher · áp trên quầy bán (UC_POS_021–022, 024)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chương trình</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Giảm</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {promos.map((p) => (
                  <tr key={p.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(p.id)}>
                    <td className={td}>
                      <div className="font-medium">{p.code}</div>
                      <div className="text-xs text-[var(--muted)]">{p.name} · {p.voucherCount} voucher</div>
                    </td>
                    <td className={td}>
                      {p.discountType === "Percent" ? `${p.discountValue}%` : p.discountValue.toLocaleString()}
                    </td>
                    <td className={td}>
                      <span className={statusPill(p.status === "Active" ? "success" : "muted")}>{p.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {canManage && (
            <form
              className="mt-3 grid gap-2 border-t border-black/10 pt-3 sm:grid-cols-2"
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => upsertPosPromotion({
                  code, name, discountType: dtype, discountValue: Number(dval) || 0,
                  minOrderAmount: Number(minAmt) || 0, status: "Active",
                }), "Đã lưu CTKM");
              }}
            >
              <input className={field} value={code} onChange={(e) => setCode(e.target.value)} placeholder="Mã" required />
              <input className={field} value={name} onChange={(e) => setName(e.target.value)} placeholder="Tên" required />
              <select className={field} value={dtype} onChange={(e) => setDtype(e.target.value)}>
                <option value="Percent">Percent</option>
                <option value="Amount">Amount</option>
              </select>
              <input className={field} value={dval} onChange={(e) => setDval(e.target.value)} placeholder="Giá trị" />
              <input className={field} value={minAmt} onChange={(e) => setMinAmt(e.target.value)} placeholder="Đơn tối thiểu" />
              <button className={btn.primary} type="submit">Tạo / cập nhật CTKM</button>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">
            Voucher {selected ? `· ${selected.code}` : ""}
          </h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>CTKM</th>
                  <th className={th}>Lượt</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {vouchers
                  .filter((v) => !selectedId || v.promotionId === selectedId)
                  .map((v) => (
                    <tr key={v.id}>
                      <td className={td}><b>{v.code}</b></td>
                      <td className={td}>{v.promotionCode}</td>
                      <td className={td}>{v.usedCount}/{v.maxUses}</td>
                      <td className={td}>
                        <span className={statusPill(v.status === "Active" ? "success" : "muted")}>{v.status}</span>
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
          {canManage && selected && (
            <form
              className="mt-3 flex flex-wrap gap-2 border-t border-black/10 pt-3"
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => upsertPosVoucher({
                  code: vCode, promotionId: selected.id, maxUses: Number(vMax) || 1, status: "Active",
                }), "Đã lưu voucher");
              }}
            >
              <input className={field} value={vCode} onChange={(e) => setVCode(e.target.value)} placeholder="Mã voucher" required />
              <input className={`${field} w-24`} value={vMax} onChange={(e) => setVMax(e.target.value)} placeholder="Max" />
              <button className={btn.primary} type="submit">Thêm voucher</button>
            </form>
          )}
        </section>
      </div>
    </div>
  );
}
