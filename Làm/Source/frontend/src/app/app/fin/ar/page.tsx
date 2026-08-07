"use client";

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import { searchCrmCustomers, type CrmCustomerDto } from "@/shared/api/crm-api";
import {
  fetchFinAccounts,
  fetchFinPeriods,
  type FinAccountDto,
  type FinPeriodDto,
} from "@/shared/api/fin-api";
import { fetchFinCashFunds, type FinCashFundDto } from "@/shared/api/fin-cash-api";
import { fetchFinBankAccounts, type FinBankAccountDto } from "@/shared/api/fin-bank-api";
import {
  fetchFinArAging,
  fetchFinArCreditAlerts,
  fetchFinArCreditLimits,
  fetchFinArCustomerBalances,
  fetchFinArInvoices,
  fetchFinArReceipts,
  postFinArInvoice,
  postFinArReceipt,
  upsertFinArCreditLimit,
  upsertFinArInvoice,
  upsertFinArReceipt,
  voidFinArInvoice,
  type FinArAgingDto,
  type FinArCreditLimitDto,
  type FinArCustomerBalanceDto,
  type FinArInvoiceDto,
  type FinArReceiptDto,
} from "@/shared/api/fin-ar-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

type Tab = "invoices" | "balances" | "receipts" | "credit" | "aging";

export default function FinArPage() {
  const { can } = usePermissions();
  const canRead = can("fin.ar.read");
  const canManage = can("fin.ar.manage");

  const [tab, setTab] = useState<Tab>("invoices");
  const [invoices, setInvoices] = useState<FinArInvoiceDto[]>([]);
  const [balances, setBalances] = useState<FinArCustomerBalanceDto[]>([]);
  const [receipts, setReceipts] = useState<FinArReceiptDto[]>([]);
  const [limits, setLimits] = useState<FinArCreditLimitDto[]>([]);
  const [alerts, setAlerts] = useState<FinArCreditLimitDto[]>([]);
  const [aging, setAging] = useState<FinArAgingDto | null>(null);
  const [customers, setCustomers] = useState<CrmCustomerDto[]>([]);
  const [accounts, setAccounts] = useState<FinAccountDto[]>([]);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [funds, setFunds] = useState<FinCashFundDto[]>([]);
  const [banks, setBanks] = useState<FinBankAccountDto[]>([]);
  const [customerId, setCustomerId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [invNo, setInvNo] = useState("HD-KH-001");
  const [subTotal, setSubTotal] = useState("1000000");
  const [tax, setTax] = useState("100000");
  const [dueDays, setDueDays] = useState("30");
  const [arAccId, setArAccId] = useState("");
  const [revAccId, setRevAccId] = useState("");
  const [periodId, setPeriodId] = useState("");

  const [payMethod, setPayMethod] = useState("Bank");
  const [fundId, setFundId] = useState("");
  const [bankId, setBankId] = useState("");
  const [selectedInvId, setSelectedInvId] = useState("");
  const [recvAmount, setRecvAmount] = useState("");

  const [creditLimit, setCreditLimit] = useState("5000000");
  const [warnPct, setWarnPct] = useState("80");

  const openInvoices = useMemo(
    () => invoices.filter((i) => i.status === "Open" || i.status === "Partial"),
    [invoices],
  );

  const load = useCallback(async () => {
    const [inv, bal, rec, lim, al, age, cus, acc, per, f, b] = await Promise.all([
      fetchFinArInvoices(customerId ? { customerId } : undefined),
      fetchFinArCustomerBalances(),
      fetchFinArReceipts(customerId ? { customerId } : undefined),
      fetchFinArCreditLimits(),
      fetchFinArCreditAlerts(),
      fetchFinArAging(),
      searchCrmCustomers({}).catch(() => [] as CrmCustomerDto[]),
      fetchFinAccounts().catch(() => [] as FinAccountDto[]),
      fetchFinPeriods().catch(() => [] as FinPeriodDto[]),
      fetchFinCashFunds().catch(() => [] as FinCashFundDto[]),
      fetchFinBankAccounts().catch(() => [] as FinBankAccountDto[]),
    ]);
    setInvoices(inv);
    setBalances(bal);
    setReceipts(rec);
    setLimits(lim);
    setAlerts(al);
    setAging(age);
    setCustomers(cus.filter((c) => c.status === "Active"));
    setAccounts(acc.filter((x) => x.isPostable && x.status === "Active"));
    setPeriods(per.filter((x) => x.status !== "Locked"));
    setFunds(f.filter((x) => x.status === "Active"));
    setBanks(b.filter((x) => x.status === "Active"));
    if (!customerId && cus[0]) setCustomerId(cus[0].id);
    if (!arAccId && acc[0]) setArAccId(acc.find((x) => x.isPostable)?.id ?? "");
    if (!revAccId && acc[1]) setRevAccId(acc.filter((x) => x.isPostable)[1]?.id ?? acc[0]?.id ?? "");
    if (!periodId && per[0]) setPeriodId(per.find((x) => x.status !== "Locked")?.id ?? "");
    if (!fundId && f[0]) setFundId(f[0].id);
    if (!bankId && b[0]) setBankId(b[0].id);
  }, [customerId, arAccId, revAccId, periodId, fundId, bankId]);

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
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem công nợ phải thu.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Công nợ phải thu (AR)</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          HĐ AR · công nợ KH · thu tiền phân bổ · hạn mức · tuổi nợ (UC_FIN_030–032, 035–036)
        </p>
      </div>
      {alerts.length > 0 && (
        <div className="rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-800">
          Cảnh báo hạn mức: {alerts.map((a) => `${a.customerCode} (${a.creditStatus})`).join(", ")}
        </div>
      )}
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="flex flex-wrap gap-2">
        {([
          ["invoices", "Hóa đơn"],
          ["balances", "Công nợ KH"],
          ["receipts", "Thu tiền"],
          ["credit", "Hạn mức"],
          ["aging", "Tuổi nợ"],
        ] as const).map(([k, label]) => (
          <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
            {label}
          </button>
        ))}
        <select className={`${field} ml-auto w-56`} value={customerId} onChange={(e) => setCustomerId(e.target.value)}>
          <option value="">— Tất cả KH —</option>
          {customers.map((c) => (
            <option key={c.id} value={c.id}>{c.code} · {c.displayName}</option>
          ))}
        </select>
      </div>

      {tab === "invoices" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Hóa đơn phải thu</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>KH</th>
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
                        <div className="text-xs text-[var(--muted)]">
                          {i.customerInvoiceNo} · hạn {new Date(i.dueDate).toLocaleDateString("vi-VN")}
                          {i.creditLimitWarned ? " · vượt HM" : ""}
                        </div>
                      </td>
                      <td className={td}>{i.customerCode}</td>
                      <td className={td}>{money(i.openAmount)}</td>
                      <td className={td}>
                        <span className={statusPill(
                          i.status === "Paid" ? "success" : i.status === "Void" ? "danger" : "brand",
                        )}>{i.status}</span>
                      </td>
                      <td className={td}>
                        {canManage && i.status === "Draft" && (
                          <div className="flex gap-1">
                            <button type="button" className={btn.ghost} onClick={() => void run(() => postFinArInvoice(i.id), "Đã ghi sổ HĐ + JE Nợ 131/Có 511.")}>Ghi sổ</button>
                            <button type="button" className={btn.ghost} onClick={() => void run(() => voidFinArInvoice(i.id, "Hủy"), "Đã hủy")}>Hủy</button>
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
              <h2 className="mb-3 text-sm font-semibold">Tạo hóa đơn AR</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!customerId) { setError("Chọn khách hàng"); return; }
                  const invDate = new Date();
                  const due = new Date(invDate);
                  due.setDate(due.getDate() + (Number(dueDays) || 30));
                  void run(() => upsertFinArInvoice({
                    customerId,
                    customerInvoiceNo: invNo,
                    invoiceDate: invDate.toISOString(),
                    dueDate: due.toISOString(),
                    subTotal: Number(subTotal) || 0,
                    taxAmount: Number(tax) || 0,
                    periodId: periodId || null,
                    arAccountId: arAccId || null,
                    revenueAccountId: revAccId || null,
                  }), "Đã tạo HĐ Draft");
                }}
              >
                <input className={field} value={invNo} onChange={(e) => setInvNo(e.target.value)} placeholder="Số HĐ" />
                <input className={field} value={dueDays} onChange={(e) => setDueDays(e.target.value)} placeholder="Số ngày hạn" />
                <input className={field} value={subTotal} onChange={(e) => setSubTotal(e.target.value)} placeholder="Tiền hàng" />
                <input className={field} value={tax} onChange={(e) => setTax(e.target.value)} placeholder="Thuế" />
                <select className={field} value={arAccId} onChange={(e) => setArAccId(e.target.value)}>
                  <option value="">— TK phải thu —</option>
                  {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} · {a.name}</option>)}
                </select>
                <select className={field} value={revAccId} onChange={(e) => setRevAccId(e.target.value)}>
                  <option value="">— TK doanh thu —</option>
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
          <h2 className="mb-3 text-sm font-semibold">Công nợ theo khách / hóa đơn</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>KH</th>
                  <th className={th}>HĐ mở</th>
                  <th className={th}>Tổng còn</th>
                  <th className={th}>Quá hạn</th>
                  <th className={th}>Hạn mức</th>
                </tr>
              </thead>
              <tbody>
                {balances.map((b) => (
                  <tr key={b.customerId} className="cursor-pointer hover:bg-black/5" onClick={() => setCustomerId(b.customerId)}>
                    <td className={td}>
                      <div className="font-medium">{b.customerCode}</div>
                      <div className="text-xs text-[var(--muted)]">{b.customerName}</div>
                    </td>
                    <td className={td}>{b.openInvoiceCount}</td>
                    <td className={td}>{money(b.totalOpen)}</td>
                    <td className={td}>{money(b.overdueAmount)}</td>
                    <td className={td}>
                      {b.creditLimit != null ? (
                        <>
                          <div>{money(b.creditLimit)} · {b.creditUsedPct ?? 0}%</div>
                          <span className={statusPill(
                            b.creditStatus === "Exceeded" ? "danger"
                              : b.creditStatus === "Warning" ? "brand" : "success",
                          )}>{b.creditStatus}</span>
                        </>
                      ) : "—"}
                    </td>
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

      {tab === "receipts" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Phiếu thu & phân bổ</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>KH</th>
                    <th className={th}>Tiền</th>
                    <th className={th}>Phân bổ</th>
                    <th className={th}>TT</th>
                    <th className={th}></th>
                  </tr>
                </thead>
                <tbody>
                  {receipts.map((r) => (
                    <tr key={r.id}>
                      <td className={td}>{r.code}</td>
                      <td className={td}>{r.customerCode}</td>
                      <td className={td}>{money(r.amount)}</td>
                      <td className={td}>{r.allocations.map((a) => a.invoiceCode).join(", ")}</td>
                      <td className={td}>
                        <span className={statusPill(r.status === "Posted" ? "success" : "brand")}>{r.status}</span>
                      </td>
                      <td className={td}>
                        {canManage && r.status === "Draft" && (
                          <button type="button" className={btn.ghost} onClick={() => void run(() => postFinArReceipt(r.id), "Đã ghi sổ thu")}>Ghi sổ</button>
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
              <h2 className="mb-3 text-sm font-semibold">Tạo phiếu thu</h2>
              <form
                className="grid gap-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!customerId) { setError("Chọn khách hàng"); return; }
                  if (!selectedInvId) { setError("Chọn HĐ"); return; }
                  const inv = openInvoices.find((i) => i.id === selectedInvId);
                  const amt = Number(recvAmount) || inv?.openAmount || 0;
                  void run(async () => {
                    const r = await upsertFinArReceipt({
                      customerId,
                      receiptDate: new Date().toISOString(),
                      payMethod,
                      cashFundId: payMethod === "Cash" ? fundId : null,
                      bankAccountId: payMethod === "Bank" ? bankId : null,
                      periodId: periodId || null,
                      allocations: [{ arInvoiceId: selectedInvId, amount: amt }],
                    });
                    await postFinArReceipt(r.id);
                  }, "Đã thu & phân bổ");
                }}
              >
                <select className={field} value={selectedInvId} onChange={(e) => {
                  setSelectedInvId(e.target.value);
                  const inv = openInvoices.find((i) => i.id === e.target.value);
                  if (inv) setRecvAmount(String(inv.openAmount));
                }}>
                  <option value="">— HĐ Open/Partial —</option>
                  {openInvoices.filter((i) => !customerId || i.customerId === customerId).map((i) => (
                    <option key={i.id} value={i.id}>{i.code} · còn {money(i.openAmount)}</option>
                  ))}
                </select>
                <input className={field} value={recvAmount} onChange={(e) => setRecvAmount(e.target.value)} placeholder="Số tiền thu" />
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
                <button className={btn.primary} type="submit">Thu & ghi sổ</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "credit" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Hạn mức tín dụng</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>KH</th>
                    <th className={th}>Hạn mức</th>
                    <th className={th}>Đang nợ</th>
                    <th className={th}>TT</th>
                  </tr>
                </thead>
                <tbody>
                  {limits.map((l) => (
                    <tr key={l.id}>
                      <td className={td}>
                        <div className="font-medium">{l.customerCode}</div>
                        <div className="text-xs text-[var(--muted)]">Cảnh báo ≥ {l.warningPercent}%</div>
                      </td>
                      <td className={td}>{money(l.creditLimit)}</td>
                      <td className={td}>{money(l.openBalance)}</td>
                      <td className={td}>
                        <span className={statusPill(
                          l.creditStatus === "Exceeded" ? "danger"
                            : l.creditStatus === "Warning" ? "brand" : "success",
                        )}>{l.creditStatus}</span>
                      </td>
                    </tr>
                  ))}
                  {limits.length === 0 && (
                    <tr><td className={td} colSpan={4}>Chưa cấu hình hạn mức.</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </section>
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Gán hạn mức KH</h2>
              <form
                className="grid gap-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!customerId) { setError("Chọn khách hàng"); return; }
                  void run(() => upsertFinArCreditLimit({
                    customerId,
                    creditLimit: Number(creditLimit) || 0,
                    warningPercent: Number(warnPct) || 80,
                    isActive: true,
                  }), "Đã lưu hạn mức");
                }}
              >
                <input className={field} value={creditLimit} onChange={(e) => setCreditLimit(e.target.value)} placeholder="Hạn mức" />
                <input className={field} value={warnPct} onChange={(e) => setWarnPct(e.target.value)} placeholder="% cảnh báo" />
                <button className={btn.primary} type="submit">Lưu hạn mức</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "aging" && (
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">
            Bảng tuổi nợ phải thu {aging ? `· ${new Date(aging.asOf).toLocaleDateString("vi-VN")}` : ""}
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
                      <th className={th}>KH</th>
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
                      <tr key={r.customerId}>
                        <td className={td}>
                          <div className="font-medium">{r.customerCode}</div>
                          <div className="text-xs text-[var(--muted)]">{r.customerName}</div>
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
