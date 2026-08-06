"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchPurPos, fetchPurVendors, type PurPurchaseOrderDto, type PurVendorDto } from "@/shared/api/pur-api";
import {
  createPurInvoice,
  fetchPurInvoiceDetail,
  fetchPurInvoices,
  matchPurInvoice,
  pushPurInvoiceAp,
  type PurInvoiceDetailDto,
  type PurInvoiceDto,
} from "@/shared/api/pur-receiving-api";
import { canPushInvoiceToAp, formatApPushMessage, pushStatusTone } from "@/shared/api/pur-push-helpers";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PurInvoicesPage() {
  const { can } = usePermissions();
  const canRead = can("pur.invoice.read");
  const canManage = can("pur.invoice.manage");

  const [vendors, setVendors] = useState<PurVendorDto[]>([]);
  const [pos, setPos] = useState<PurPurchaseOrderDto[]>([]);
  const [list, setList] = useState<PurInvoiceDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<PurInvoiceDetailDto | null>(null);
  const [vendorId, setVendorId] = useState("");
  const [poId, setPoId] = useState("");
  const [invoiceNumber, setInvoiceNumber] = useState("HD-001");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [v, p, inv] = await Promise.all([fetchPurVendors(), fetchPurPos(), fetchPurInvoices()]);
    setVendors(v);
    setPos(p.filter((x) => x.status === "Sent" || x.status === "Closed" || x.receivedPct > 0));
    setList(inv);
    if (!vendorId && v[0]) setVendorId(v[0].id);
    if (!selectedId && inv[0]) setSelectedId(inv[0].id);
  }, [vendorId, selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchPurInvoiceDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  useEffect(() => {
    if (!poId) return;
    const po = pos.find((p) => p.id === poId);
    if (po) setVendorId(po.vendorId);
  }, [poId, pos]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchPurInvoiceDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem hóa đơn NCC.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Hóa đơn nhà cung cấp</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Nhập HĐ · đối soát 3 chiều PO–GRN–Invoice · đẩy FIN AP thật (UC_PUR_040–041, 043)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách HĐ</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>HĐ</th><th className={th}>Tổng</th><th className={th}>Match</th></tr></thead>
              <tbody>
                {list.map((i) => (
                  <tr key={i.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(i.id)}>
                    <td className={td}>
                      <div className="font-medium">{i.code}</div>
                      <div className="text-xs text-[var(--muted)]">{i.invoiceNumber} · {i.poCode ?? "—"}</div>
                    </td>
                    <td className={td}>{i.totalAmount.toLocaleString()}</td>
                    <td className={td}>
                      <span className={statusPill(
                        i.matchStatus === "Matched" ? "success" : i.matchStatus === "Variance" ? "danger" : "brand",
                      )}>{i.matchStatus}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {canManage && (
            <form className="mt-3 space-y-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(async () => {
                const inv = await createPurInvoice({
                  vendorId, poId: poId || undefined, invoiceNumber,
                });
                setSelectedId(inv.id);
              }, "Đã tạo HĐ (prefill từ GRN nếu có)");
            }}>
              <div className="flex flex-wrap gap-2">
                <select className={field} value={poId} onChange={(e) => setPoId(e.target.value)}>
                  <option value="">— Chọn PO (khuyến nghị) —</option>
                  {pos.map((p) => (
                    <option key={p.id} value={p.id}>{p.code} · {p.vendorName}</option>
                  ))}
                </select>
                <select className={field} value={vendorId} onChange={(e) => setVendorId(e.target.value)}>
                  {vendors.map((v) => (
                    <option key={v.id} value={v.id}>{v.code} — {v.name}</option>
                  ))}
                </select>
                <input className={field} value={invoiceNumber} onChange={(e) => setInvoiceNumber(e.target.value)} />
                <button className={btn.primary} type="submit">Tạo HĐ</button>
              </div>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.header.code}</b> · {detail.header.invoiceNumber}
                <div className="text-xs text-[var(--muted)]">
                  {detail.header.vendorName} · PO {detail.header.poCode ?? "—"} · AP{" "}
                  <span className={statusPill(pushStatusTone(detail.header.apPushStatus))}>
                    {detail.header.apPushStatus}
                  </span>
                </div>
                <div className="mt-1">
                  Sub {detail.header.subTotal.toLocaleString()} + thuế {detail.header.taxAmount.toLocaleString()}
                  = <b>{detail.header.totalAmount.toLocaleString()}</b>
                </div>
                {detail.header.matchNote && (
                  <div className="mt-1 text-xs text-[var(--muted)]">{detail.header.matchNote}</div>
                )}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>SP</th><th className={th}>SL</th><th className={th}>TT</th></tr></thead>
                  <tbody>
                    {detail.lines.map((l) => (
                      <tr key={l.id}>
                        <td className={td}>{l.productCode} {l.productName}</td>
                        <td className={td}>{l.qty}</td>
                        <td className={td}>{l.lineAmount.toLocaleString()}</td>
                      </tr>
                    ))}
                    {detail.lines.length === 0 && (
                      <tr><td className={td} colSpan={3}>Chưa có dòng — post GRN rồi tạo HĐ từ PO</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
              {canManage && (
                <div className="flex flex-wrap gap-2">
                  {(detail.header.status === "Draft" || detail.header.status === "Disputed") && (
                    <button type="button" className={btn.primary} onClick={() => void run(
                      () => matchPurInvoice(detail.header.id), "Đã chạy đối soát 3 chiều",
                    )}>Đối soát 3 chiều</button>
                  )}
                  {canPushInvoiceToAp(detail.header.matchStatus, detail.header.apPushStatus, detail.header.totalAmount) && (
                    <button type="button" className={btn.ghost} onClick={() => void run(
                      () => pushPurInvoiceAp(detail.header.id),
                      formatApPushMessage(detail.header.code, detail.header.totalAmount),
                    )}>Đẩy FIN AP</button>
                  )}
                </div>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Tạo HĐ từ PO đã nhận hàng.</p>
          )}
        </section>
      </div>
    </div>
  );
}
