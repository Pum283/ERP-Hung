"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchFinAccounts,
  fetchFinPeriods,
  type FinAccountDto,
  type FinPeriodDto,
} from "@/shared/api/fin-api";
import {
  approveFinBankTransfer,
  executeFinBankTransfer,
  fetchFinBankAccounts,
  fetchFinBankBook,
  fetchFinBankStatements,
  fetchFinBankTransfers,
  fetchFinBankVouchers,
  ignoreFinBankStatement,
  matchFinBankStatement,
  postFinBankVoucher,
  rejectFinBankTransfer,
  submitFinBankTransfer,
  unmatchFinBankStatement,
  upsertFinBankAccount,
  upsertFinBankStatement,
  upsertFinBankTransfer,
  upsertFinBankVoucher,
  voidFinBankTransfer,
  voidFinBankVoucher,
  type FinBankAccountDto,
  type FinBankBookDto,
  type FinBankStatementDto,
  type FinBankTransferDto,
  type FinBankVoucherDto,
} from "@/shared/api/fin-bank-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

type Tab = "accounts" | "vouchers" | "transfers" | "reconcile" | "book";

export default function FinBankPage() {
  const { can } = usePermissions();
  const canRead = can("fin.bank.read");
  const canManage = can("fin.bank.manage");

  const [tab, setTab] = useState<Tab>("accounts");
  const [accounts, setAccounts] = useState<FinBankAccountDto[]>([]);
  const [vouchers, setVouchers] = useState<FinBankVoucherDto[]>([]);
  const [transfers, setTransfers] = useState<FinBankTransferDto[]>([]);
  const [statements, setStatements] = useState<FinBankStatementDto[]>([]);
  const [glAccounts, setGlAccounts] = useState<FinAccountDto[]>([]);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [book, setBook] = useState<FinBankBookDto | null>(null);
  const [bankId, setBankId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [aCode, setACode] = useState("VCB-HQ");
  const [aName, setAName] = useState("TK thanh toán VCB");
  const [aBank, setABank] = useState("Vietcombank");
  const [aNumber, setANumber] = useState("0123456789");
  const [aBranch, setABranch] = useState("HN");
  const [aGlId, setAGlId] = useState("");
  const [aOpen, setAOpen] = useState("0");

  const [vType, setVType] = useState("Credit");
  const [vAmount, setVAmount] = useState("1000000");
  const [vDesc, setVDesc] = useState("Giấy báo Có");
  const [vRef, setVRef] = useState("");
  const [vCounterId, setVCounterId] = useState("");
  const [vPeriodId, setVPeriodId] = useState("");

  const [tBenName, setTBenName] = useState("NCC ABC");
  const [tBenAcc, setTBenAcc] = useState("9876543210");
  const [tBenBank, setTBenBank] = useState("BIDV");
  const [tAmount, setTAmount] = useState("500000");
  const [tDesc, setTDesc] = useState("Thanh toán NCC");

  const [sDir, setSDir] = useState("Credit");
  const [sAmount, setSAmount] = useState("1000000");
  const [sDesc, setSDesc] = useState("Dòng sao kê");
  const [sRef, setSRef] = useState("");
  const [matchVoucherId, setMatchVoucherId] = useState("");

  const load = useCallback(async () => {
    const [acc, v, t, s, gl, p] = await Promise.all([
      fetchFinBankAccounts(),
      fetchFinBankVouchers(bankId ? { bankAccountId: bankId } : undefined),
      fetchFinBankTransfers(bankId ? { bankAccountId: bankId } : undefined),
      fetchFinBankStatements(bankId ? { bankAccountId: bankId } : undefined),
      fetchFinAccounts().catch(() => [] as FinAccountDto[]),
      fetchFinPeriods().catch(() => [] as FinPeriodDto[]),
    ]);
    setAccounts(acc);
    setVouchers(v);
    setTransfers(t);
    setStatements(s);
    setGlAccounts(gl.filter((x) => x.isPostable && x.status === "Active"));
    setPeriods(p.filter((x) => x.status !== "Locked"));
    if (!bankId && acc[0]) setBankId(acc[0].id);
    if (!aGlId && gl[0]) setAGlId(gl.find((x) => x.isPostable)?.id ?? "");
    if (!vCounterId && gl[1]) setVCounterId(gl.filter((x) => x.isPostable)[1]?.id ?? gl[0]?.id ?? "");
    if (!vPeriodId && p[0]) setVPeriodId(p.find((x) => x.status !== "Locked")?.id ?? "");
    const id = bankId || acc[0]?.id;
    if (id) setBook(await fetchFinBankBook(id).catch(() => null));
  }, [bankId, aGlId, vCounterId, vPeriodId]);

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
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem ngân hàng.</div>;
  }

  const postedForMatch = vouchers.filter((v) => v.status === "Posted");

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Ngân hàng</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          TKNH · giấy báo Nợ/Có · đề nghị CK · đối soát · số dư (UC_FIN_024–027, 029)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="flex flex-wrap gap-2">
        {([
          ["accounts", "TKNH"],
          ["vouchers", "Giấy báo"],
          ["transfers", "Chuyển khoản"],
          ["reconcile", "Đối soát"],
          ["book", "Số dư"],
        ] as const).map(([k, label]) => (
          <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
            {label}
          </button>
        ))}
        <select className={`${field} ml-auto w-56`} value={bankId} onChange={(e) => setBankId(e.target.value)}>
          <option value="">— TKNH —</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>{a.code} · {money(a.bookBalance)}</option>
          ))}
        </select>
      </div>

      {tab === "accounts" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Danh mục TK ngân hàng</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Số TK</th>
                    <th className={th}>Số dư</th>
                    <th className={th}>TT</th>
                  </tr>
                </thead>
                <tbody>
                  {accounts.map((a) => (
                    <tr key={a.id} className="cursor-pointer hover:bg-black/5" onClick={() => setBankId(a.id)}>
                      <td className={td}>
                        <div className="font-medium">{a.code}</div>
                        <div className="text-xs text-[var(--muted)]">{a.bankName} · {a.name}</div>
                      </td>
                      <td className={td}>{a.accountNumber}</td>
                      <td className={td}>{money(a.bookBalance)}</td>
                      <td className={td}>
                        <span className={statusPill(a.status === "Active" ? "success" : "muted")}>{a.status}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo / cập nhật TKNH</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => upsertFinBankAccount({
                    code: aCode, name: aName, bankName: aBank, accountNumber: aNumber,
                    branchName: aBranch, glAccountId: aGlId,
                    openingBalance: Number(aOpen) || 0, status: "Active",
                  }), "Đã lưu TKNH");
                }}
              >
                <input className={field} value={aCode} onChange={(e) => setACode(e.target.value)} placeholder="Mã" required />
                <input className={field} value={aName} onChange={(e) => setAName(e.target.value)} placeholder="Tên" required />
                <input className={field} value={aBank} onChange={(e) => setABank(e.target.value)} placeholder="Ngân hàng" required />
                <input className={field} value={aNumber} onChange={(e) => setANumber(e.target.value)} placeholder="Số TK" required />
                <input className={field} value={aBranch} onChange={(e) => setABranch(e.target.value)} placeholder="Chi nhánh" />
                <input className={field} value={aOpen} onChange={(e) => setAOpen(e.target.value)} placeholder="Số dư đầu" />
                <select className={`${field} sm:col-span-2`} value={aGlId} onChange={(e) => setAGlId(e.target.value)} required>
                  <option value="">— TK hạch toán —</option>
                  {glAccounts.map((a) => (
                    <option key={a.id} value={a.id}>{a.code} · {a.name}</option>
                  ))}
                </select>
                <button className={`${btn.primary} sm:col-span-2`} type="submit">Lưu TKNH</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "vouchers" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Giấy báo Nợ / Có</h2>
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
                        {v.finJournalCode && <div className="text-xs text-[var(--muted)]">BT {v.finJournalCode}</div>}
                      </td>
                      <td className={td}>
                        {canManage && v.status === "Draft" && (
                          <div className="flex gap-1">
                            <button type="button" className={btn.ghost} onClick={() => void run(() => postFinBankVoucher(v.id), "Đã ghi sổ + JE Auto (auto TK/kỳ).")}>Ghi sổ</button>
                            <button type="button" className={btn.ghost} onClick={() => void run(() => voidFinBankVoucher(v.id, "Hủy"), "Đã hủy")}>Hủy</button>
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
              <h2 className="mb-3 text-sm font-semibold">Tạo giấy báo</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!bankId) { setError("Chọn TKNH"); return; }
                  void run(() => upsertFinBankVoucher({
                    bankAccountId: bankId,
                    voucherType: vType,
                    docDate: new Date().toISOString(),
                    amount: Number(vAmount) || 0,
                    description: vDesc,
                    bankRef: vRef || null,
                    counterAccountId: vCounterId || null,
                    periodId: vPeriodId || null,
                  }), "Đã tạo giấy báo Draft");
                }}
              >
                <select className={field} value={vType} onChange={(e) => setVType(e.target.value)}>
                  <option value="Credit">Báo Có (GBC)</option>
                  <option value="Debit">Báo Nợ (GBN)</option>
                </select>
                <input className={field} value={vAmount} onChange={(e) => setVAmount(e.target.value)} placeholder="Số tiền" />
                <input className={`${field} sm:col-span-2`} value={vDesc} onChange={(e) => setVDesc(e.target.value)} placeholder="Diễn giải" required />
                <input className={field} value={vRef} onChange={(e) => setVRef(e.target.value)} placeholder="Ref NH" />
                <select className={field} value={vCounterId} onChange={(e) => setVCounterId(e.target.value)}>
                  <option value="">— TK đối ứng —</option>
                  {glAccounts.map((a) => (
                    <option key={a.id} value={a.id}>{a.code} · {a.name}</option>
                  ))}
                </select>
                <select className={`${field} sm:col-span-2`} value={vPeriodId} onChange={(e) => setVPeriodId(e.target.value)}>
                  <option value="">— Kỳ KT —</option>
                  {periods.map((p) => (
                    <option key={p.id} value={p.id}>{p.code}</option>
                  ))}
                </select>
                <button className={`${btn.primary} sm:col-span-2`} type="submit">Tạo giấy báo</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "transfers" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Đề nghị chuyển khoản</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Thụ hưởng</th>
                    <th className={th}>Tiền</th>
                    <th className={th}>TT</th>
                    <th className={th}></th>
                  </tr>
                </thead>
                <tbody>
                  {transfers.map((t) => (
                    <tr key={t.id}>
                      <td className={td}>
                        <div className="font-medium">{t.code}</div>
                        <div className="text-xs text-[var(--muted)]">{t.description}</div>
                      </td>
                      <td className={td}>
                        <div>{t.beneficiaryName}</div>
                        <div className="text-xs text-[var(--muted)]">{t.beneficiaryBank} · {t.beneficiaryAccount}</div>
                      </td>
                      <td className={td}>{money(t.amount)}</td>
                      <td className={td}>
                        <span className={statusPill(
                          t.status === "Executed" ? "success"
                            : t.status === "Rejected" || t.status === "Void" ? "danger"
                              : "brand",
                        )}>{t.status}</span>
                        {t.executedVoucherCode && (
                          <div className="text-xs text-[var(--muted)]">GB {t.executedVoucherCode}</div>
                        )}
                      </td>
                      <td className={td}>
                        {canManage && (
                          <div className="flex flex-wrap gap-1">
                            {t.status === "Draft" && (
                              <>
                                <button type="button" className={btn.ghost} onClick={() => void run(() => submitFinBankTransfer(t.id), "Đã gửi duyệt")}>Gửi</button>
                                <button type="button" className={btn.ghost} onClick={() => void run(() => voidFinBankTransfer(t.id, "Hủy"), "Đã hủy")}>Hủy</button>
                              </>
                            )}
                            {t.status === "Submitted" && (
                              <>
                                <button type="button" className={btn.ghost} onClick={() => void run(() => approveFinBankTransfer(t.id), "Đã duyệt")}>Duyệt</button>
                                <button type="button" className={btn.ghost} onClick={() => void run(() => rejectFinBankTransfer(t.id, "Từ chối"), "Đã từ chối")}>Từ chối</button>
                              </>
                            )}
                            {t.status === "Approved" && (
                              <button type="button" className={btn.ghost} onClick={() => void run(() => executeFinBankTransfer(t.id), "Đã thực hiện CK")}>Thực hiện</button>
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
              <h2 className="mb-3 text-sm font-semibold">Tạo đề nghị CK</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!bankId) { setError("Chọn TKNH nguồn"); return; }
                  void run(() => upsertFinBankTransfer({
                    fromBankAccountId: bankId,
                    beneficiaryName: tBenName,
                    beneficiaryAccount: tBenAcc,
                    beneficiaryBank: tBenBank,
                    amount: Number(tAmount) || 0,
                    description: tDesc,
                    counterAccountId: vCounterId || null,
                    periodId: vPeriodId || null,
                  }), "Đã tạo đề nghị Draft");
                }}
              >
                <input className={field} value={tBenName} onChange={(e) => setTBenName(e.target.value)} placeholder="Người thụ hưởng" required />
                <input className={field} value={tBenAcc} onChange={(e) => setTBenAcc(e.target.value)} placeholder="Số TK thụ hưởng" required />
                <input className={field} value={tBenBank} onChange={(e) => setTBenBank(e.target.value)} placeholder="NH thụ hưởng" required />
                <input className={field} value={tAmount} onChange={(e) => setTAmount(e.target.value)} placeholder="Số tiền" />
                <input className={`${field} sm:col-span-2`} value={tDesc} onChange={(e) => setTDesc(e.target.value)} placeholder="Nội dung" required />
                <button className={`${btn.primary} sm:col-span-2`} type="submit">Tạo đề nghị</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "reconcile" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Sao kê / đối soát</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Ngày</th>
                    <th className={th}>Diễn giải</th>
                    <th className={th}>Tiền</th>
                    <th className={th}>TT</th>
                    <th className={th}></th>
                  </tr>
                </thead>
                <tbody>
                  {statements.map((s) => (
                    <tr key={s.id}>
                      <td className={td}>{new Date(s.stmtDate).toLocaleDateString("vi-VN")}</td>
                      <td className={td}>
                        <div>{s.description}</div>
                        <div className="text-xs text-[var(--muted)]">{s.direction} · {s.bankRef}</div>
                        {s.matchedVoucherCode && (
                          <div className="text-xs text-[var(--muted)]">Khớp {s.matchedVoucherCode}</div>
                        )}
                      </td>
                      <td className={td}>{money(s.amount)}</td>
                      <td className={td}>
                        <span className={statusPill(
                          s.status === "Matched" ? "success" : s.status === "Ignored" ? "muted" : "brand",
                        )}>{s.status}</span>
                      </td>
                      <td className={td}>
                        {canManage && s.status === "Unmatched" && (
                          <div className="flex flex-col gap-1">
                            <select className={field} value={matchVoucherId} onChange={(e) => setMatchVoucherId(e.target.value)}>
                              <option value="">— Giấy báo Posted —</option>
                              {postedForMatch
                                .filter((v) => v.voucherType === s.direction && Math.abs(v.amount - s.amount) < 0.01)
                                .map((v) => (
                                  <option key={v.id} value={v.id}>{v.code} · {money(v.amount)}</option>
                                ))}
                            </select>
                            <div className="flex gap-1">
                              <button
                                type="button"
                                className={btn.ghost}
                                onClick={() => {
                                  if (!matchVoucherId) { setError("Chọn giấy báo"); return; }
                                  void run(() => matchFinBankStatement(s.id, matchVoucherId), "Đã khớp sao kê");
                                }}
                              >
                                Khớp
                              </button>
                              <button type="button" className={btn.ghost} onClick={() => void run(() => ignoreFinBankStatement(s.id), "Đã bỏ qua")}>Bỏ qua</button>
                            </div>
                          </div>
                        )}
                        {canManage && s.status === "Matched" && (
                          <button type="button" className={btn.ghost} onClick={() => void run(() => unmatchFinBankStatement(s.id), "Đã bỏ khớp")}>Bỏ khớp</button>
                        )}
                      </td>
                    </tr>
                  ))}
                  {statements.length === 0 && (
                    <tr><td className={td} colSpan={5}>Chưa có dòng sao kê (nhập tay Cap-2).</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </section>
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Nhập dòng sao kê</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  if (!bankId) { setError("Chọn TKNH"); return; }
                  void run(() => upsertFinBankStatement({
                    bankAccountId: bankId,
                    stmtDate: new Date().toISOString(),
                    description: sDesc,
                    bankRef: sRef || null,
                    direction: sDir,
                    amount: Number(sAmount) || 0,
                  }), "Đã thêm dòng sao kê");
                }}
              >
                <select className={field} value={sDir} onChange={(e) => setSDir(e.target.value)}>
                  <option value="Credit">Có</option>
                  <option value="Debit">Nợ</option>
                </select>
                <input className={field} value={sAmount} onChange={(e) => setSAmount(e.target.value)} placeholder="Số tiền" />
                <input className={`${field} sm:col-span-2`} value={sDesc} onChange={(e) => setSDesc(e.target.value)} placeholder="Diễn giải" required />
                <input className={`${field} sm:col-span-2`} value={sRef} onChange={(e) => setSRef(e.target.value)} placeholder="Ref NH" />
                <button className={`${btn.primary} sm:col-span-2`} type="submit">Thêm dòng</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "book" && (
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">
            Số dư ngân hàng {book ? `· ${book.bankAccountCode}` : ""}
          </h2>
          {book ? (
            <>
              <div className="mb-3 grid gap-3 sm:grid-cols-4 text-sm">
                <div>Đầu kỳ: <b>{money(book.openingBalance)}</b></div>
                <div>Có: <b>{money(book.totalCredit)}</b></div>
                <div>Nợ: <b>{money(book.totalDebit)}</b></div>
                <div>Cuối kỳ: <b>{money(book.closingBalance)}</b></div>
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead>
                    <tr>
                      <th className={th}>Ngày</th>
                      <th className={th}>Phiếu</th>
                      <th className={th}>Diễn giải</th>
                      <th className={th}>Có</th>
                      <th className={th}>Nợ</th>
                      <th className={th}>Tồn</th>
                    </tr>
                  </thead>
                  <tbody>
                    {book.rows.map((r) => (
                      <tr key={`${r.voucherCode}-${r.docDate}`}>
                        <td className={td}>{new Date(r.docDate).toLocaleDateString("vi-VN")}</td>
                        <td className={td}>{r.voucherCode}</td>
                        <td className={td}>{r.description}</td>
                        <td className={td}>{r.credit ? money(r.credit) : "—"}</td>
                        <td className={td}>{r.debit ? money(r.debit) : "—"}</td>
                        <td className={td}>{money(r.balance)}</td>
                      </tr>
                    ))}
                    {book.rows.length === 0 && (
                      <tr><td className={td} colSpan={6}>Chưa có giấy báo Posted.</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
            </>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chọn TKNH để xem số dư.</p>
          )}
        </section>
      )}
    </div>
  );
}
