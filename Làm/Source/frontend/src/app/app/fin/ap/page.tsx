"use client";

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import {
  fetchFinAccounts,
  fetchFinPeriods,
  type FinAccountDto,
  type FinPeriodDto,
} from "@/shared/api/fin-api";
import { fetchFinCashFunds, type FinCashFundDto } from "@/shared/api/fin-cash-api";
import { fetchFinBankAccounts, type FinBankAccountDto } from "@/shared/api/fin-bank-api";
import {
  approveFinApPaymentRequest,
  fetchFinApAging,
  fetchFinApInvoices,
  fetchFinApPaymentRequests,
  fetchFinApPayments,
  fetchFinApVendorBalances,
  payFinApPaymentRequest,
  postFinApInvoice,
  rejectFinApPaymentRequest,
  submitFinApPaymentRequest,
  upsertFinApInvoice,
  upsertFinApPaymentRequest,
  voidFinApInvoice,
  type FinApAgingDto,
  type FinApInvoiceDto,
  type FinApPaymentDto,
  type FinApPaymentRequestDto,
  type FinApVendorBalanceDto,
} from "@/shared/api/fin-ap-api";
import { fetchPurVendors, type PurVendorDto } from "@/shared/api/pur-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

type Tab = "invoices" | "balances" | "requests" | "payments" | "aging";

export default function FinApPage() {
  const { can } = usePermissions();
  const canRead = can("fin.ap.read");
  const canManage = can("fin.ap.manage");

  const [tab, setTab] = useState<Tab>("invoices");
  const [invoices, setInvoices] = useState<FinApInvoiceDto[]>([]);
  const [balances, setBalances] = useState<FinApVendorBalanceDto[]>([]);
  const [requests, setRequests] = useState<FinApPaymentRequestDto[]>([]);
  const [payments, setPayments] = useState<FinApPaymentDto[]>([]);
  const [aging, setAging] = useState<FinApAgingDto | null>(null);
  const [vendors, setVendors] = useState<PurVendorDto[]>([]);
  const [accounts, setAccounts] = useState<FinAccountDto[]>([]);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [funds, setFunds] = useState<FinCashFundDto[]>([]);
  const [banks, setBanks] = useState<FinBankAccountDto[]>([]);
  const [vendorId, setVendorId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [invNo, setInvNo] = useState("NCC-001");
  const [subTotal, setSubTotal] = useState("1000000");
  const [tax, setTax] = useState("100000");
  const [dueDays, setDueDays] = useState("30");
  const [apAccId, setApAccId] = useState("");
  const [expAccId, setExpAccId] = useState("");
  const [periodId, setPeriodId] = useState("");

  const [payMethod, setPayMethod] = useState("Bank");
  const [fundId, setFundId] = useState("");
  const [bankId, setBankId] = useState("");
  const [selectedInvId, setSelectedInvId] = useState("");
  const [reqAmount, setReqAmount] = useState("");

  const openInvoices = useMemo(
    () => invoices.filter((i) => i.status === "Open" || i.status === "Partial"),
    [invoices],
  );

  const load = useCallback(async () => {
    const [inv, bal, req, pay, age, ven, acc, per, f, b] = await Promise.all([
      fetchFinApInvoices(vendorId ? { vendorId } : undefined),
      fetchFinApVendorBalances(),
      fetchFinApPaymentRequests(vendorId ? { vendorId } : undefined),
      fetchFinApPayments(vendorId ? { vendorId } : undefined),
      fetchFinApAging(),
      fetchPurVendors().catch(() => [] as PurVendorDto[]),
      fetchFinAccounts().catch(() => [] as FinAccountDto[]),
      fetchFinPeriods().catch(() => [] as FinPeriodDto[]),
      fetchFinCashFunds().catch(() => [] as FinCashFundDto[]),
      fetchFinBankAccounts().catch(() => [] as FinBankAccountDto[]),
    ]);
    setInvoices(inv);
    setBalances(bal);
    setRequests(req);
    setPayments(pay);
    setAging(age);
    setVendors(ven.filter((v) => v.status === "Active"));
    setAccounts(acc.filter((x) => x.isPostable && x.status === "Active"));
    setPeriods(per.filter((x) => x.status !== "Locked"));
    setFunds(f.filter((x) => x.status === "Active"));
    setBanks(b.filter((x) => x.status === "Active"));
    if (!vendorId && ven[0]) setVendorId(ven[0].id);
    if (!apAccId && acc[0]) setApAccId(acc.find((x) => x.isPostable)?.id ?? "");
    if (!expAccId && acc[1]) setExpAccId(acc.filter((x) => x.isPostable)[1]?.id ?? acc[0]?.id ?? "");
    if (!periodId && per[0]) setPeriodId(per.find((x) => x.status !== "Locked")?.id ?? "");
    if (!fundId && f[0]) setFundId(f[0].id);
    if (!bankId && b[0]) setBankId(b[0].id);
  }, [vendorId, apAccId, expAccId, periodId, fundId, bankId]);

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

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem công nợ phải trả.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Công nợ phải trả (AP)</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          HĐ AP · công nợ NCC · đề nghị TT · duyệt/chi · tuổi nợ (UC_FIN_039–044)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="flex flex-wrap gap-2">
        {([
          ["invoices", "Hóa đơn"],
          ["balances", "Công nợ NCC"],
          ["requests", "Đề nghị TT"],
          ["payments", "Thanh toán"],
          ["aging", "Tuổi nợ"],
        ] as const).map(([k, label]) => (
          <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
            {label}
          </button>
        ))}
        <select className={`${field} ml-auto w-56`} value={vendorId} onChange={(e) => setVendorId(e.target.value)}>
          <option value="">— Tất cả NCC —</option>
          {vendors.map((v) => (
            <option key={v.id} value={v.id}>{v.code} · {v.name}</option>
          ))}
        </select>
      </div>

      {tab === "invoices" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Hóa đơn phải trả</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>NCC</th>
                    <th className={th}>Còn lại</th>
                    <th className={th}>TT</th>
                    <th className={th}></th>
                  </tr>
                </thead>
                <tbody>
                  {invoices.map((i) => (
                    <tr key={i.id}>
                      <td className={td}>
                        <div className="font-medium">{i.code}</div>
                        <div className="text-xs text-[var(--muted)]">{i.vendorInvoiceNo} · hạn {new Date(i.dueDate).toLocaleDateString("vi-VN")}</div>
                      </td>
                      <td className={td}>{i.vendorCode}</td>
                      <td className={td}>{money(i.openAmount)}</td>
                      <td className={td}>
                        <span className={statusPill(
                          i.status === "Paid" ? "success" : i.status === "Void" ? "danger" : "brand",
                        )}>{i.status}</span>
                      </td>
                      <td className={td}>
                        {canManage && i.status === "Draft" && (
                          <div className="flex gap-1">
                            <button type="button" className={btn.ghost} onClick={() => void run(() => postFinApInvoice(i.id), "Đã ghi sổ HĐ")}>Ghi sổ</button>
                            <button type="button" className={btn.ghost} onClick={() => void run(() => voidFinApInvoice(i.id, "Hủy"), "Đã hủy")}>Hủy</button>
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo hóa đơn AP</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!vendorId) { setError("Chọn NCC"); return; }
                  const invDate = new Date();
                  const due = new Date(invDate);
                  due.setDate(due.getDate() + (Number(dueDays) || 30));
                  void run(() => upsertFinApInvoice({
                    vendorId,
                    vendorInvoiceNo: invNo,
                    invoiceDate: invDate.toISOString(),
                    dueDate: due.toISOString(),
                    subTotal: Number(subTotal) || 0,
                    taxAmount: Number(tax) || 0,
                    periodId: periodId || null,
                    apAccountId: apAccId || null,
                    expenseAccountId: expAccId || null,
                  }), "Đã tạo HĐ Draft");
                }}
              >
                <input className={field} value={invNo} onChange={(e) => setInvNo(e.target.value)} placeholder="Số HĐ NCC" />
                <input className={field} value={dueDays} onChange={(e) => setDueDays(e.target.value)} placeholder="Số ngày hạn" />
                <input className={field} value={subTotal} onChange={(e) => setSubTotal(e.target.value)} placeholder="Tiền hàng" />
                <input className={field} value={tax} onChange={(e) => setTax(e.target.value)} placeholder="Thuế" />
                <select className={field} value={apAccId} onChange={(e) => setApAccId(e.target.value)}>
                  <option value="">— TK phải trả —</option>
                  {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} · {a.name}</option>)}
                </select>
                <select className={field} value={expAccId} onChange={(e) => setExpAccId(e.target.value)}>
                  <option value="">— TK chi phí —</option>
                  {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} · {a.name}</option>)}
                </select>
                <select className={`${field} sm:col-span-2`} value={periodId} onChange={(e) => setPeriodId(e.target.value)}>
                  <option value="">— Kỳ KT (tuỳ chọn BT) —</option>
                  {periods.map((p) => <option key={p.id} value={p.id}>{p.code}</option>)}
                </select>
                <button className={`${btn.primary} sm:col-span-2`} type="submit">Tạo HĐ</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "balances" && (
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Công nợ theo nhà cung cấp</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>NCC</th>
                  <th className={th}>Số HĐ mở</th>
                  <th className={th}>Tổng còn</th>
                  <th className={th}>Quá hạn</th>
                  <th className={th}>Trong hạn</th>
                </tr>
              </thead>
              <tbody>
                {balances.map((b) => (
                  <tr key={b.vendorId} className="cursor-pointer hover:bg-black/5" onClick={() => setVendorId(b.vendorId)}>
                    <td className={td}>
                      <div className="font-medium">{b.vendorCode}</div>
                      <div className="text-xs text-[var(--muted)]">{b.vendorName}</div>
                    </td>
                    <td className={td}>{b.openInvoiceCount}</td>
                    <td className={td}>{money(b.totalOpen)}</td>
                    <td className={td}>{money(b.overdueAmount)}</td>
                    <td className={td}>{money(b.notDueAmount)}</td>
                  </tr>
                ))}
                {balances.length === 0 && (
                  <tr><td className={td} colSpan={5}>Chưa có công nợ mở.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {tab === "requests" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Đề nghị thanh toán</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>NCC</th>
                    <th className={th}>Tiền</th>
                    <th className={th}>TT</th>
                    <th className={th}></th>
                  </tr>
                </thead>
                <tbody>
                  {requests.map((r) => (
                    <tr key={r.id}>
                      <td className={td}>
                        <div className="font-medium">{r.code}</div>
                        <div className="text-xs text-[var(--muted)]">{r.payMethod} · {r.lines.map((l) => l.invoiceCode).join(", ")}</div>
                      </td>
                      <td className={td}>{r.vendorCode}</td>
                      <td className={td}>{money(r.requestAmount)}</td>
                      <td className={td}>
                        <span className={statusPill(
                          r.status === "Paid" ? "success"
                            : r.status === "Rejected" || r.status === "Void" ? "danger"
                              : "brand",
                        )}>{r.status}</span>
                        {r.paymentCode && <div className="text-xs text-[var(--muted)]">TT {r.paymentCode}</div>}
                      </td>
                      <td className={td}>
                        {canManage && (
                          <div className="flex flex-wrap gap-1">
                            {r.status === "Draft" && (
                              <button type="button" className={btn.ghost} onClick={() => void run(() => submitFinApPaymentRequest(r.id), "Đã gửi duyệt")}>Gửi</button>
                            )}
                            {r.status === "Submitted" && (
                              <>
                                <button type="button" className={btn.ghost} onClick={() => void run(() => approveFinApPaymentRequest(r.id), "Đã duyệt")}>Duyệt</button>
                                <button type="button" className={btn.ghost} onClick={() => void run(() => rejectFinApPaymentRequest(r.id, "Từ chối"), "Đã từ chối")}>Từ chối</button>
                              </>
                            )}
                            {r.status === "Approved" && (
                              <button type="button" className={btn.ghost} onClick={() => void run(() => payFinApPaymentRequest(r.id), "Đã thanh toán")}>Chi trả</button>
                            )}
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo đề nghị TT</h2>
              <form
                className="grid gap-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!vendorId) { setError("Chọn NCC"); return; }
                  if (!selectedInvId) { setError("Chọn HĐ"); return; }
                  const inv = openInvoices.find((i) => i.id === selectedInvId);
                  const amt = Number(reqAmount) || inv?.openAmount || 0;
                  void run(() => upsertFinApPaymentRequest({
                    vendorId,
                    payMethod,
                    cashFundId: payMethod === "Cash" ? fundId : null,
                    bankAccountId: payMethod === "Bank" ? bankId : null,
                    lines: [{ apInvoiceId: selectedInvId, amount: amt }],
                  }), "Đã tạo đề nghị Draft");
                }}
              >
                <select className={field} value={selectedInvId} onChange={(e) => {
                  setSelectedInvId(e.target.value);
                  const inv = openInvoices.find((i) => i.id === e.target.value);
                  if (inv) setReqAmount(String(inv.openAmount));
                }}>
                  <option value="">— HĐ Open/Partial —</option>
                  {openInvoices.filter((i) => !vendorId || i.vendorId === vendorId).map((i) => (
                    <option key={i.id} value={i.id}>{i.code} · còn {money(i.openAmount)}</option>
                  ))}
                </select>
                <input className={field} value={reqAmount} onChange={(e) => setReqAmount(e.target.value)} placeholder="Số tiền đề nghị" />
                <select className={field} value={payMethod} onChange={(e) => setPayMethod(e.target.value)}>
                  <option value="Bank">Ngân hàng</option>
                  <option value="Cash">Tiền mặt</option>
                </select>
                {payMethod === "Bank" ? (
                  <select className={field} value={bankId} onChange={(e) => setBankId(e.target.value)}>
                    <option value="">— TKNH —</option>
                    {banks.map((b) => <option key={b.id} value={b.id}>{b.code} · {money(b.bookBalance)}</option>)}
                  </select>
                ) : (
                  <select className={field} value={fundId} onChange={(e) => setFundId(e.target.value)}>
                    <option value="">— Quỹ —</option>
                    {funds.map((f) => <option key={f.id} value={f.id}>{f.code} · {money(f.bookBalance)}</option>)}
                  </select>
                )}
                <button className={btn.primary} type="submit">Tạo đề nghị</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "payments" && (
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Phiếu thanh toán đã ghi sổ</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>NCC</th>
                  <th className={th}>Phương thức</th>
                  <th className={th}>Tiền</th>
                  <th className={th}>Phân bổ</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {payments.map((p) => (
                  <tr key={p.id}>
                    <td className={td}>{p.code}</td>
                    <td className={td}>{p.vendorCode}</td>
                    <td className={td}>{p.payMethod}</td>
                    <td className={td}>{money(p.amount)}</td>
                    <td className={td}>{p.allocations.map((a) => a.invoiceCode).join(", ")}</td>
                    <td className={td}>
                      <span className={statusPill(p.status === "Posted" ? "success" : "brand")}>{p.status}</span>
                    </td>
                  </tr>
                ))}
                {payments.length === 0 && (
                  <tr><td className={td} colSpan={6}>Chưa có phiếu TT — dùng «Chi trả» trên đề nghị Approved.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {tab === "aging" && (
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">
            Bảng tuổi nợ phải trả {aging ? `· ${new Date(aging.asOf).toLocaleDateString("vi-VN")}` : ""}
          </h2>
          {aging && (
            <>
              <div className="mb-3 flex flex-wrap gap-3 text-sm">
                {aging.buckets.map((b) => (
                  <div key={b.bucket} className="rounded-md border border-black/10 px-3 py-2">
                    <div className="text-xs text-[var(--muted)]">{b.bucket} · {b.invoiceCount} HĐ</div>
                    <div className="font-semibold">{money(b.amount)}</div>
                  </div>
                ))}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead>
                    <tr>
                      <th className={th}>NCC</th>
                      <th className={th}>Current</th>
                      <th className={th}>1–30</th>
                      <th className={th}>31–60</th>
                      <th className={th}>61–90</th>
                      <th className={th}>&gt;90</th>
                      <th className={th}>Tổng</th>
                    </tr>
                  </thead>
                  <tbody>
                    {aging.rows.map((r) => (
                      <tr key={r.vendorId}>
                        <td className={td}>
                          <div className="font-medium">{r.vendorCode}</div>
                          <div className="text-xs text-[var(--muted)]">{r.vendorName}</div>
                        </td>
                        <td className={td}>{money(r.current)}</td>
                        <td className={td}>{money(r.d1To30)}</td>
                        <td className={td}>{money(r.d31To60)}</td>
                        <td className={td}>{money(r.d61To90)}</td>
                        <td className={td}>{money(r.over90)}</td>
                        <td className={td}>{money(r.total)}</td>
                      </tr>
                    ))}
                    {aging.rows.length === 0 && (
                      <tr><td className={td} colSpan={7}>Không có nợ mở.</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
            </>
          )}
        </section>
      )}
    </div>
  );
}
