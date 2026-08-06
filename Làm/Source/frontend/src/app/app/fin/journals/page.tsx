"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  createFinAutoJournal,
  fetchFinAccounts,
  fetchFinCostCenters,
  fetchFinDetailLedger,
  fetchFinJournalDetail,
  fetchFinJournals,
  fetchFinLedger,
  fetchFinPeriods,
  postFinJournal,
  reverseFinJournal,
  upsertFinJournal,
  type FinAccountDto,
  type FinCostCenterDto,
  type FinDetailLedgerRowDto,
  type FinJournalDetailDto,
  type FinJournalDto,
  type FinLedgerRowDto,
  type FinPeriodDto,
} from "@/shared/api/fin-api";
import { isAutoSource } from "@/shared/api/fin-journal-helpers";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function FinJournalsPage() {
  const { can } = usePermissions();
  const canRead = can("fin.journal.read");
  const canManage = can("fin.journal.manage");

  const [list, setList] = useState<FinJournalDto[]>([]);
  const [accounts, setAccounts] = useState<FinAccountDto[]>([]);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [ccs, setCcs] = useState<FinCostCenterDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<FinJournalDetailDto | null>(null);
  const [ledger, setLedger] = useState<FinLedgerRowDto[]>([]);
  const [detailLedger, setDetailLedger] = useState<FinDetailLedgerRowDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [sourceFilter, setSourceFilter] = useState<"" | "Manual" | "Auto">("");

  const [periodId, setPeriodId] = useState("");
  const [desc, setDesc] = useState("");
  const [partner, setPartner] = useState("");
  const [ccId, setCcId] = useState("");
  const [accDebit, setAccDebit] = useState("");
  const [accCredit, setAccCredit] = useState("");
  const [amount, setAmount] = useState("1000000");
  const [ledgerPartner, setLedgerPartner] = useState("");

  const load = useCallback(async () => {
    const [j, a, p, c] = await Promise.all([
      fetchFinJournals(undefined, sourceFilter || undefined),
      fetchFinAccounts().catch(() => [] as FinAccountDto[]),
      fetchFinPeriods().catch(() => [] as FinPeriodDto[]),
      fetchFinCostCenters().catch(() => [] as FinCostCenterDto[]),
    ]);
    setList(j); setAccounts(a); setPeriods(p); setCcs(c);
    if (!selectedId && j[0]) setSelectedId(j[0].id);
    if (!periodId) {
      const open = p.find((x) => x.status === "Open") ?? p[0];
      if (open) setPeriodId(open.id);
    }
    if (!accDebit && a[0]) setAccDebit(a[0].id);
    if (!accCredit && a[1]) setAccCredit(a[1].id);
    else if (!accCredit && a[0]) setAccCredit(a[0].id);
  }, [selectedId, periodId, accDebit, accCredit, sourceFilter]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchFinJournalDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchFinJournalDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  async function refreshLedgers() {
    try {
      const [l, d] = await Promise.all([
        fetchFinLedger({ periodId: periodId || undefined, partnerCode: ledgerPartner || undefined }),
        fetchFinDetailLedger({
          periodId: periodId || undefined,
          partnerCode: ledgerPartner || undefined,
          costCenterId: ccId || undefined,
        }),
      ]);
      setLedger(l); setDetailLedger(d);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem bút toán.</div>;
  }

  const amt = Number(amount) || 0;

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Bút toán / sổ cái</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          BT thủ công · ghi sổ · đảo · sổ cái / CT theo ĐT · BT tự động Source=Auto (UC_FIN_010, 012–015)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
            <h2 className="text-sm font-semibold">Danh sách bút toán</h2>
            <select
              className={field}
              style={{ width: 140 }}
              value={sourceFilter}
              onChange={(e) => setSourceFilter(e.target.value as "" | "Manual" | "Auto")}
            >
              <option value="">Tất cả nguồn</option>
              <option value="Manual">Manual</option>
              <option value="Auto">Auto</option>
            </select>
          </div>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th><th className={th}>Diễn giải</th>
                  <th className={th}>Nợ</th><th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {list.map((j) => (
                  <tr key={j.id} className="cursor-pointer hover:bg-black/5"
                    onClick={() => setSelectedId(j.id)}>
                    <td className={td}>{j.code}</td>
                    <td className={td}>{j.description}</td>
                    <td className={td}>{j.totalDebit.toLocaleString()}</td>
                    <td className={td}>
                      <span className={statusPill(j.status === "Posted" ? "success" : j.status === "Draft" ? "warning" : "muted")}>{j.status}</span>
                      {isAutoSource(j.source) && <span className="ml-1 text-xs text-[var(--muted)]">Auto</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết / thao tác</h2>
          {detail ? (
            <div className="mb-3 space-y-1 text-sm">
              <div><b>{detail.journal.code}</b> · {detail.journal.periodCode} · {detail.journal.source}</div>
              <div>{detail.journal.description}</div>
              <div>
                <span className={statusPill(detail.journal.status === "Posted" ? "success" : detail.journal.status === "Draft" ? "warning" : "muted")}>{detail.journal.status}</span>
                {" "}Nợ {detail.journal.totalDebit.toLocaleString()} / Có {detail.journal.totalCredit.toLocaleString()}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>TK</th><th className={th}>Nợ</th><th className={th}>Có</th><th className={th}>ĐT</th></tr></thead>
                  <tbody>
                    {detail.lines.map((l) => (
                      <tr key={l.id}>
                        <td className={td}>{l.accountCode}</td>
                        <td className={td}>{l.debit.toLocaleString()}</td>
                        <td className={td}>{l.credit.toLocaleString()}</td>
                        <td className={td}>{l.partnerCode ?? "—"}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {canManage && detail.journal.status === "Draft" && (
                <button type="button" className={btn.primary}
                  onClick={() => void run(() => postFinJournal(detail.journal.id), "Đã ghi sổ")}>
                  Ghi sổ
                </button>
              )}
              {canManage && detail.journal.status === "Posted" && (
                <button type="button" className={btn.ghost}
                  onClick={() => void run(() => reverseFinJournal(detail.journal.id), "Đã đảo BT")}>
                  Đảo bút toán
                </button>
              )}
            </div>
          ) : (
            <p className="mb-3 text-sm text-[var(--muted)]">Chọn một BT bên trái.</p>
          )}

          {canManage && (
            <form className="space-y-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              if (!periodId || !accDebit || !accCredit || amt <= 0) {
                setError("Chọn kỳ, 2 TK và số tiền > 0.");
                return;
              }
              void run(() => upsertFinJournal({
                periodId,
                entryDate: new Date().toISOString(),
                description: desc || "Bút toán thủ công",
                partnerCode: partner || null,
                costCenterId: ccId || null,
                source: "Manual",
                lines: [
                  { accountId: accDebit, debit: amt, credit: 0, partnerCode: partner || null, costCenterId: ccId || null },
                  { accountId: accCredit, debit: 0, credit: amt, partnerCode: partner || null, costCenterId: ccId || null },
                ],
              }), "Đã tạo BT Draft");
            }}>
              <div className="text-sm font-medium">Tạo BT thủ công (2 dòng)</div>
              <select className={field} value={periodId} onChange={(e) => setPeriodId(e.target.value)}>
                <option value="">— Kỳ —</option>
                {periods.map((p) => <option key={p.id} value={p.id}>{p.code} ({p.status})</option>)}
              </select>
              <input className={field} placeholder="Diễn giải" value={desc} onChange={(e) => setDesc(e.target.value)} />
              <input className={field} placeholder="Mã đối tượng" value={partner} onChange={(e) => setPartner(e.target.value)} />
              <select className={field} value={ccId} onChange={(e) => setCcId(e.target.value)}>
                <option value="">— TTCP —</option>
                {ccs.map((c) => <option key={c.id} value={c.id}>{c.code}</option>)}
              </select>
              <select className={field} value={accDebit} onChange={(e) => setAccDebit(e.target.value)}>
                <option value="">— TK Nợ —</option>
                {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} {a.name}</option>)}
              </select>
              <select className={field} value={accCredit} onChange={(e) => setAccCredit(e.target.value)}>
                <option value="">— TK Có —</option>
                {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} {a.name}</option>)}
              </select>
              <input className={field} placeholder="Số tiền" value={amount} onChange={(e) => setAmount(e.target.value)} />
              <div className="flex flex-wrap gap-2">
                <button className={btn.primary} type="submit">Tạo Draft</button>
                <button type="button" className={btn.ghost} onClick={() => {
                  if (!periodId || !accDebit || !accCredit || amt <= 0) {
                    setError("Chọn kỳ, 2 TK và số tiền > 0.");
                    return;
                  }
                  void run(() => createFinAutoJournal({
                    periodId,
                    entryDate: new Date().toISOString(),
                    description: desc || "BT tự động",
                    partnerCode: partner || null,
                    costCenterId: ccId || null,
                    lines: [
                      { accountId: accDebit, debit: amt, credit: 0 },
                      { accountId: accCredit, debit: 0, credit: amt },
                    ],
                  }), "Đã tạo BT tự động (Source=Auto).");
                }}>
                  Tạo BT tự động
                </button>
              </div>
            </form>
          )}
        </section>

        <section className={`${panel} xl:col-span-2`}>
          <div className="mb-3 flex flex-wrap items-center gap-2">
            <h2 className="text-sm font-semibold">Sổ cái / sổ chi tiết</h2>
            <input className={field} placeholder="Lọc ĐT" value={ledgerPartner}
              onChange={(e) => setLedgerPartner(e.target.value)} />
            <button type="button" className={btn.ghost} onClick={() => void refreshLedgers()}>Tải sổ</button>
          </div>
          <div className="grid gap-4 lg:grid-cols-2">
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead><tr><th className={th}>TK</th><th className={th}>Nợ</th><th className={th}>Có</th><th className={th}>SD</th></tr></thead>
                <tbody>
                  {ledger.map((r) => (
                    <tr key={r.accountId}>
                      <td className={td}>{r.accountCode} {r.accountName}</td>
                      <td className={td}>{r.debit.toLocaleString()}</td>
                      <td className={td}>{r.credit.toLocaleString()}</td>
                      <td className={td}>{r.balance.toLocaleString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead><tr><th className={th}>BT</th><th className={th}>TK</th><th className={th}>Nợ/Có</th><th className={th}>ĐT</th></tr></thead>
                <tbody>
                  {detailLedger.map((r, i) => (
                    <tr key={`${r.journalId}-${i}`}>
                      <td className={td}>{r.journalCode}</td>
                      <td className={td}>{r.accountCode}</td>
                      <td className={td}>{r.debit.toLocaleString()} / {r.credit.toLocaleString()}</td>
                      <td className={td}>{r.partnerCode ?? "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}
