"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchFinAccounts,
  fetchFinCostCenters,
  fetchFinFiscalYears,
  fetchFinGroups,
  fetchFinPaymentMethods,
  fetchFinPeriods,
  fetchFinTaxes,
  setFinPeriodLock,
  upsertFinAccount,
  upsertFinCostCenter,
  upsertFinFiscalYear,
  upsertFinGroup,
  upsertFinPaymentMethod,
  upsertFinTax,
  type FinAccountDto,
  type FinAccountGroupDto,
  type FinCostCenterDto,
  type FinFiscalYearDto,
  type FinPaymentMethodDto,
  type FinPeriodDto,
  type FinTaxDto,
} from "@/shared/api/fin-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function FinCatalogPage() {
  const { can } = usePermissions();
  const canRead = can("fin.master.read");
  const canManage = can("fin.master.manage");

  const [groups, setGroups] = useState<FinAccountGroupDto[]>([]);
  const [accounts, setAccounts] = useState<FinAccountDto[]>([]);
  const [years, setYears] = useState<FinFiscalYearDto[]>([]);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [ccs, setCcs] = useState<FinCostCenterDto[]>([]);
  const [pms, setPms] = useState<FinPaymentMethodDto[]>([]);
  const [taxes, setTaxes] = useState<FinTaxDto[]>([]);
  const [selectedYearId, setSelectedYearId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [gCode, setGCode] = useState("TAISAN");
  const [gName, setGName] = useState("");
  const [aCode, setACode] = useState("111");
  const [aName, setAName] = useState("");
  const [aType, setAType] = useState("Asset");
  const [aGroupId, setAGroupId] = useState("");
  const [fyCode, setFyCode] = useState(String(new Date().getFullYear()));
  const [fyName, setFyName] = useState(`Năm ${new Date().getFullYear()}`);
  const [ccCode, setCcCode] = useState("CC-HQ");
  const [ccName, setCcName] = useState("");
  const [pmCode, setPmCode] = useState("CASH");
  const [pmName, setPmName] = useState("");
  const [taxCode, setTaxCode] = useState("VAT10");
  const [taxName, setTaxName] = useState("VAT 10%");
  const [taxRate, setTaxRate] = useState("10");

  const load = useCallback(async () => {
    const [g, a, y, c, p, t] = await Promise.all([
      fetchFinGroups(), fetchFinAccounts(), fetchFinFiscalYears(),
      fetchFinCostCenters(), fetchFinPaymentMethods(), fetchFinTaxes(),
    ]);
    setGroups(g); setAccounts(a); setYears(y); setCcs(c); setPms(p); setTaxes(t);
    if (!selectedYearId && y[0]) setSelectedYearId(y[0].id);
    if (!aGroupId && g[0]) setAGroupId(g[0].id);
  }, [selectedYearId, aGroupId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedYearId || !canRead) { setPeriods([]); return; }
    fetchFinPeriods(selectedYearId).then(setPeriods).catch((e: Error) => setError(e.message));
  }, [selectedYearId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedYearId) setPeriods(await fetchFinPeriods(selectedYearId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem danh mục kế toán.</div>;
  }

  const year = new Date().getFullYear();

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Danh mục kế toán</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          COA · nhóm TK · kỳ/năm · khóa sổ · TTCP · HTTT · thuế (UC_FIN_001–004, 006, 008–009)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 lg:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Nhóm tài khoản</h2>
          {canManage && (
            <form className="mb-3 flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertFinGroup({ code: gCode, name: gName }), "Đã lưu nhóm TK");
            }}>
              <input className={field} placeholder="Mã" value={gCode} onChange={(e) => setGCode(e.target.value)} />
              <input className={field} placeholder="Tên" value={gName} onChange={(e) => setGName(e.target.value)} />
              <button className={btn.primary} type="submit">Thêm</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Tên</th><th className={th}>TK</th></tr></thead>
              <tbody>
                {groups.map((g) => (
                  <tr key={g.id}><td className={td}>{g.code}</td><td className={td}>{g.name}</td><td className={td}>{g.accountCount}</td></tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Hệ thống tài khoản (COA)</h2>
          {canManage && (
            <form className="mb-3 flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertFinAccount({
                code: aCode, name: aName, accountType: aType, groupId: aGroupId || null,
              }), "Đã lưu TK");
            }}>
              <input className={field} placeholder="Mã" value={aCode} onChange={(e) => setACode(e.target.value)} />
              <input className={field} placeholder="Tên" value={aName} onChange={(e) => setAName(e.target.value)} />
              <select className={field} value={aType} onChange={(e) => setAType(e.target.value)}>
                {["Asset", "Liability", "Equity", "Revenue", "Expense"].map((t) => <option key={t}>{t}</option>)}
              </select>
              <select className={field} value={aGroupId} onChange={(e) => setAGroupId(e.target.value)}>
                <option value="">— Nhóm —</option>
                {groups.map((g) => <option key={g.id} value={g.id}>{g.code}</option>)}
              </select>
              <button className={btn.primary} type="submit">Thêm</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Tên</th><th className={th}>Loại</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {accounts.map((a) => (
                  <tr key={a.id}>
                    <td className={td}>{a.code}</td><td className={td}>{a.name}</td>
                    <td className={td}>{a.accountType}</td>
                    <td className={td}><span className={statusPill(a.status === "Active" ? "success" : "muted")}>{a.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Năm / kỳ kế toán</h2>
          {canManage && (
            <form className="mb-3 flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              const y = Number(fyCode) || year;
              void run(() => upsertFinFiscalYear({
                code: fyCode, name: fyName, year: y,
                startDate: new Date(Date.UTC(y, 0, 1)).toISOString(),
                endDate: new Date(Date.UTC(y, 11, 31, 23, 59, 59)).toISOString(),
                generateMonths: true,
              }), "Đã tạo năm TC + 12 kỳ");
            }}>
              <input className={field} placeholder="Mã năm" value={fyCode} onChange={(e) => setFyCode(e.target.value)} />
              <input className={field} placeholder="Tên" value={fyName} onChange={(e) => setFyName(e.target.value)} />
              <button className={btn.primary} type="submit">Tạo năm + kỳ</button>
            </form>
          )}
          <div className="mb-2 flex flex-wrap gap-2">
            {years.map((y) => (
              <button key={y.id} type="button" className={selectedYearId === y.id ? btn.primary : btn.ghost}
                onClick={() => setSelectedYearId(y.id)}>
                {y.code} ({y.periodCount} kỳ)
              </button>
            ))}
          </div>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Kỳ</th><th className={th}>TT</th><th className={th}></th></tr></thead>
              <tbody>
                {periods.map((p) => (
                  <tr key={p.id}>
                    <td className={td}>{p.code} · {p.name}</td>
                    <td className={td}><span className={statusPill(p.status === "Open" ? "success" : "warning")}>{p.status}</span></td>
                    <td className={td}>
                      {canManage && (
                        <button type="button" className={btn.ghost}
                          onClick={() => void run(
                            () => setFinPeriodLock(p.id, p.status !== "Locked"),
                            p.status === "Locked" ? "Đã mở kỳ" : "Đã khóa kỳ",
                          )}>
                          {p.status === "Locked" ? "Mở lại" : "Khóa sổ"}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">TTCP · HTTT · Thuế</h2>
          {canManage && (
            <div className="mb-3 space-y-2">
              <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => upsertFinCostCenter({ code: ccCode, name: ccName }), "Đã lưu TTCP");
              }}>
                <input className={field} placeholder="Mã TTCP" value={ccCode} onChange={(e) => setCcCode(e.target.value)} />
                <input className={field} placeholder="Tên TTCP" value={ccName} onChange={(e) => setCcName(e.target.value)} />
                <button className={btn.primary} type="submit">TTCP</button>
              </form>
              <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => upsertFinPaymentMethod({ code: pmCode, name: pmName }), "Đã lưu HTTT");
              }}>
                <input className={field} placeholder="Mã HTTT" value={pmCode} onChange={(e) => setPmCode(e.target.value)} />
                <input className={field} placeholder="Tên HTTT" value={pmName} onChange={(e) => setPmName(e.target.value)} />
                <button className={btn.primary} type="submit">HTTT</button>
              </form>
              <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => upsertFinTax({
                  code: taxCode, name: taxName, ratePercent: Number(taxRate) || 0,
                  taxType: "VatOutput", isDefault: true,
                }), "Đã lưu thuế");
              }}>
                <input className={field} placeholder="Mã thuế" value={taxCode} onChange={(e) => setTaxCode(e.target.value)} />
                <input className={field} placeholder="Tên" value={taxName} onChange={(e) => setTaxName(e.target.value)} />
                <input className={field} placeholder="%" value={taxRate} onChange={(e) => setTaxRate(e.target.value)} />
                <button className={btn.primary} type="submit">Thuế</button>
              </form>
            </div>
          )}
          <div className="grid gap-3 sm:grid-cols-3 text-sm">
            <div>
              <div className="mb-1 font-medium">TTCP</div>
              {ccs.map((x) => <div key={x.id}>{x.code} — {x.name}</div>)}
            </div>
            <div>
              <div className="mb-1 font-medium">HTTT</div>
              {pms.map((x) => <div key={x.id}>{x.code} — {x.name}</div>)}
            </div>
            <div>
              <div className="mb-1 font-medium">Thuế</div>
              {taxes.map((x) => (
                <div key={x.id}>{x.code} — {x.ratePercent}% · {x.taxType}{x.isDefault ? " ★" : ""}</div>
              ))}
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}
