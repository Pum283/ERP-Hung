"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchFinAccounts,
  fetchFinPeriods,
  type FinAccountDto,
  type FinPeriodDto,
} from "@/shared/api/fin-api";
import {
  fetchFinCashBook,
  fetchFinCashFunds,
  fetchFinCashVouchers,
  postFinCashVoucher,
  upsertFinCashFund,
  upsertFinCashVoucher,
  voidFinCashVoucher,
  type FinCashBookDto,
  type FinCashFundDto,
  type FinCashVoucherDto,
} from "@/shared/api/fin-cash-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

export default function FinCashPage() {
  const { can } = usePermissions();
  const canRead = can("fin.cash.read");
  const canManage = can("fin.cash.manage");

  const [tab, setTab] = useState<"funds" | "vouchers" | "book">("funds");
  const [funds, setFunds] = useState<FinCashFundDto[]>([]);
  const [vouchers, setVouchers] = useState<FinCashVoucherDto[]>([]);
  const [accounts, setAccounts] = useState<FinAccountDto[]>([]);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [book, setBook] = useState<FinCashBookDto | null>(null);
  const [fundId, setFundId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [fCode, setFCode] = useState("QTM");
  const [fName, setFName] = useState("Quỹ tiền mặt HQ");
  const [fAccId, setFAccId] = useState("");
  const [fOpen, setFOpen] = useState("0");
  const [fCustodian, setFCustodian] = useState("Thủ quỹ");

  const [vType, setVType] = useState("Receipt");
  const [vAmount, setVAmount] = useState("1000000");
  const [vDesc, setVDesc] = useState("Thu tiền mặt");
  const [vCounterId, setVCounterId] = useState("");
  const [vPeriodId, setVPeriodId] = useState("");
  const [vPartner, setVPartner] = useState("");

  const load = useCallback(async () => {
    const [f, v, a, p] = await Promise.all([
      fetchFinCashFunds(),
      fetchFinCashVouchers(fundId ? { fundId } : undefined),
      fetchFinAccounts().catch(() => [] as FinAccountDto[]),
      fetchFinPeriods().catch(() => [] as FinPeriodDto[]),
    ]);
    setFunds(f);
    setVouchers(v);
    setAccounts(a.filter((x) => x.isPostable && x.status === "Active"));
    setPeriods(p.filter((x) => x.status !== "Locked"));
    if (!fundId && f[0]) setFundId(f[0].id);
    if (!fAccId && a[0]) setFAccId(a.find((x) => x.isPostable)?.id ?? "");
    if (!vCounterId && a[1]) setVCounterId(a.filter((x) => x.isPostable)[1]?.id ?? a[0]?.id ?? "");
    if (!vPeriodId && p[0]) setVPeriodId(p.find((x) => x.status !== "Locked")?.id ?? "");
    if (fundId || f[0]) {
      const id = fundId || f[0].id;
      setBook(await fetchFinCashBook(id).catch(() => null));
    }
  }, [fundId, fAccId, vCounterId, vPeriodId]);

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
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem quỹ tiền mặt.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Quỹ tiền mặt</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Danh mục quỹ · phiếu thu/chi · sổ quỹ (UC_FIN_018–020, 023)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="flex flex-wrap gap-2">
        {(["funds", "vouchers", "book"] as const).map((t) => (
          <button
            key={t}
            type="button"
            className={tab === t ? btn.primary : btn.ghost}
            onClick={() => setTab(t)}
          >
            {t === "funds" ? "Quỹ" : t === "vouchers" ? "Phiếu" : "Sổ quỹ"}
          </button>
        ))}
        <select className={`${field} ml-auto w-48`} value={fundId} onChange={(e) => setFundId(e.target.value)}>
          <option value="">— Quỹ —</option>
          {funds.map((f) => (
            <option key={f.id} value={f.id}>{f.code} · {money(f.bookBalance)}</option>
          ))}
        </select>
      </div>

      {tab === "funds" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Danh sách quỹ</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>TK</th>
                    <th className={th}>Số dư</th>
                    <th className={th}>TT</th>
                  </tr>
                </thead>
                <tbody>
                  {funds.map((f) => (
                    <tr key={f.id} className="cursor-pointer hover:bg-black/5" onClick={() => setFundId(f.id)}>
                      <td className={td}>
                        <div className="font-medium">{f.code}</div>
                        <div className="text-xs text-[var(--muted)]">{f.name} · {f.custodianName}</div>
                      </td>
                      <td className={td}>{f.cashAccountCode}</td>
                      <td className={td}>{money(f.bookBalance)}</td>
                      <td className={td}>
                        <span className={statusPill(f.status === "Active" ? "success" : "muted")}>{f.status}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo / cập nhật quỹ</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => upsertFinCashFund({
                    code: fCode, name: fName, cashAccountId: fAccId,
                    openingBalance: Number(fOpen) || 0, custodianName: fCustodian, status: "Active",
                  }), "Đã lưu quỹ");
                }}
              >
                <input className={field} value={fCode} onChange={(e) => setFCode(e.target.value)} placeholder="Mã" required />
                <input className={field} value={fName} onChange={(e) => setFName(e.target.value)} placeholder="Tên" required />
                <select className={`${field} sm:col-span-2`} value={fAccId} onChange={(e) => setFAccId(e.target.value)} required>
                  <option value="">— Tài khoản quỹ —</option>
                  {accounts.map((a) => (
                    <option key={a.id} value={a.id}>{a.code} · {a.name}</option>
                  ))}
                </select>
                <input className={field} value={fOpen} onChange={(e) => setFOpen(e.target.value)} placeholder="Số dư đầu" />
                <input className={field} value={fCustodian} onChange={(e) => setFCustodian(e.target.value)} placeholder="Thủ quỹ" />
                <button className={`${btn.primary} sm:col-span-2`} type="submit">Lưu quỹ</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "vouchers" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Phiếu thu / chi</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Loại</th>
                    <th className={th}>Tiền</th>
                    <th className={th}>TT</th>
                    <th className={th}></th>
                  </tr>
                </thead>
                <tbody>
                  {vouchers.map((v) => (
                    <tr key={v.id}>
                      <td className={td}>
                        <div className="font-medium">{v.code}</div>
                        <div className="text-xs text-[var(--muted)]">{v.description}</div>
                      </td>
                      <td className={td}>{v.voucherType}</td>
                      <td className={td}>{money(v.amount)}</td>
                      <td className={td}>
                        <span className={statusPill(v.status === "Posted" ? "success" : v.status === "Void" ? "danger" : "brand")}>
                          {v.status}
                        </span>
                        {v.finJournalCode && (
                          <div className="text-xs text-[var(--muted)]">BT {v.finJournalCode}</div>
                        )}
                      </td>
                      <td className={td}>
                        {canManage && v.status === "Draft" && (
                          <div className="flex gap-1">
                            <button type="button" className={btn.ghost} onClick={() => void run(() => postFinCashVoucher(v.id), "Đã ghi sổ phiếu")}>
                              Ghi sổ
                            </button>
                            <button type="button" className={btn.ghost} onClick={() => void run(() => voidFinCashVoucher(v.id, "Hủy"), "Đã hủy phiếu")}>
                              Hủy
                            </button>
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
              <h2 className="mb-3 text-sm font-semibold">Tạo phiếu</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!fundId) { setError("Chọn quỹ"); return; }
                  void run(() => upsertFinCashVoucher({
                    fundId,
                    voucherType: vType,
                    docDate: new Date().toISOString(),
                    amount: Number(vAmount) || 0,
                    description: vDesc,
                    partnerCode: vPartner || null,
                    counterAccountId: vCounterId || null,
                    periodId: vPeriodId || null,
                  }), "Đã tạo phiếu Draft");
                }}
              >
                <select className={field} value={vType} onChange={(e) => setVType(e.target.value)}>
                  <option value="Receipt">Thu (PT)</option>
                  <option value="Payment">Chi (PC)</option>
                </select>
                <input className={field} value={vAmount} onChange={(e) => setVAmount(e.target.value)} placeholder="Số tiền" />
                <input className={`${field} sm:col-span-2`} value={vDesc} onChange={(e) => setVDesc(e.target.value)} placeholder="Diễn giải" required />
                <select className={field} value={vCounterId} onChange={(e) => setVCounterId(e.target.value)}>
                  <option value="">— TK đối ứng (tuỳ chọn BT) —</option>
                  {accounts.map((a) => (
                    <option key={a.id} value={a.id}>{a.code} · {a.name}</option>
                  ))}
                </select>
                <select className={field} value={vPeriodId} onChange={(e) => setVPeriodId(e.target.value)}>
                  <option value="">— Kỳ KT (tuỳ chọn BT) —</option>
                  {periods.map((p) => (
                    <option key={p.id} value={p.id}>{p.code}</option>
                  ))}
                </select>
                <input className={`${field} sm:col-span-2`} value={vPartner} onChange={(e) => setVPartner(e.target.value)} placeholder="Mã đối tượng" />
                <button className={`${btn.primary} sm:col-span-2`} type="submit">Tạo phiếu</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "book" && (
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">
            Sổ quỹ {book ? `· ${book.fundCode}` : ""}
          </h2>
          {book ? (
            <>
              <div className="mb-3 grid gap-3 sm:grid-cols-4 text-sm">
                <div>Đầu kỳ: <b>{money(book.openingBalance)}</b></div>
                <div>Thu: <b>{money(book.totalReceipt)}</b></div>
                <div>Chi: <b>{money(book.totalPayment)}</b></div>
                <div>Cuối kỳ: <b>{money(book.closingBalance)}</b></div>
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead>
                    <tr>
                      <th className={th}>Ngày</th>
                      <th className={th}>Phiếu</th>
                      <th className={th}>Diễn giải</th>
                      <th className={th}>Thu</th>
                      <th className={th}>Chi</th>
                      <th className={th}>Tồn</th>
                    </tr>
                  </thead>
                  <tbody>
                    {book.rows.map((r) => (
                      <tr key={`${r.voucherCode}-${r.docDate}`}>
                        <td className={td}>{new Date(r.docDate).toLocaleDateString("vi-VN")}</td>
                        <td className={td}>{r.voucherCode}</td>
                        <td className={td}>{r.description}</td>
                        <td className={td}>{r.receipt ? money(r.receipt) : "—"}</td>
                        <td className={td}>{r.payment ? money(r.payment) : "—"}</td>
                        <td className={td}>{money(r.balance)}</td>
                      </tr>
                    ))}
                    {book.rows.length === 0 && (
                      <tr><td className={td} colSpan={6}>Chưa có phiếu Posted.</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
            </>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chọn quỹ để xem sổ.</p>
          )}
        </section>
      )}
    </div>
  );
}
