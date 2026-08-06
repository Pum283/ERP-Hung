"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchFinPeriods,
  fetchFinTaxes,
  upsertFinTax,
  type FinPeriodDto,
  type FinTaxDto,
} from "@/shared/api/fin-api";
import {
  calculateFinVat,
  fetchFinVatDocuments,
  fetchFinVatSummary,
  postFinVatDocument,
  upsertFinVatDocument,
  voidFinVatDocument,
  type FinVatCalcResult,
  type FinVatDocumentDto,
  type FinVatSummaryDto,
} from "@/shared/api/fin-vat-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

type Tab = "rates" | "docs" | "summary";

export default function FinTaxPage() {
  const { can } = usePermissions();
  const canRead = can("fin.tax.read");
  const canManage = can("fin.tax.manage");

  const [tab, setTab] = useState<Tab>("docs");
  const [taxes, setTaxes] = useState<FinTaxDto[]>([]);
  const [docs, setDocs] = useState<FinVatDocumentDto[]>([]);
  const [summary, setSummary] = useState<FinVatSummaryDto | null>(null);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [periodId, setPeriodId] = useState("");
  const [calc, setCalc] = useState<FinVatCalcResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [taxCode, setTaxCode] = useState("VAT10");
  const [taxName, setTaxName] = useState("GTGT 10%");
  const [taxRate, setTaxRate] = useState("10");
  const [taxType, setTaxType] = useState("VatOutput");
  const [isDefault, setIsDefault] = useState(true);

  const [direction, setDirection] = useState("Output");
  const [taxId, setTaxId] = useState("");
  const [invoiceNo, setInvoiceNo] = useState("HD-001");
  const [partnerName, setPartnerName] = useState("");
  const [partnerTax, setPartnerTax] = useState("");
  const [taxable, setTaxable] = useState("1000000");

  const load = useCallback(async () => {
    const [t, d, s, p] = await Promise.all([
      fetchFinTaxes(),
      fetchFinVatDocuments(periodId ? { periodId } : undefined),
      fetchFinVatSummary(periodId ? { periodId } : undefined),
      fetchFinPeriods().catch(() => [] as FinPeriodDto[]),
    ]);
    setTaxes(t);
    setDocs(d);
    setSummary(s);
    setPeriods(p.filter((x) => x.status !== "Locked"));
    if (!taxId && t[0]) setTaxId(t.find((x) => x.isDefault)?.id ?? t[0].id);
    if (!periodId && p[0]) setPeriodId(p.find((x) => x.status !== "Locked")?.id ?? "");
  }, [periodId, taxId]);

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
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem thuế GTGT.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Thuế GTGT</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Cấu hình thuế suất · tính đầu ra/đầu vào · bảng kê (UC_FIN_052, 053, 056)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="flex flex-wrap gap-2">
        {([
          ["rates", "Thuế suất"],
          ["docs", "Bảng kê"],
          ["summary", "Tổng hợp"],
        ] as const).map(([k, label]) => (
          <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
            {label}
          </button>
        ))}
        <select className={`${field} ml-auto w-48`} value={periodId} onChange={(e) => setPeriodId(e.target.value)}>
          <option value="">— Kỳ KT —</option>
          {periods.map((p) => <option key={p.id} value={p.id}>{p.code}</option>)}
        </select>
      </div>

      {tab === "rates" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Cấu hình thuế suất</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Loại</th>
                    <th className={th}>%</th>
                    <th className={th}>TT</th>
                  </tr>
                </thead>
                <tbody>
                  {taxes.map((t) => (
                    <tr key={t.id}>
                      <td className={td}>
                        <div className="font-medium">{t.code}{t.isDefault ? " ★" : ""}</div>
                        <div className="text-xs text-[var(--muted)]">{t.name}</div>
                      </td>
                      <td className={td}>{t.taxType}</td>
                      <td className={td}>{t.ratePercent}%</td>
                      <td className={td}>
                        <span className={statusPill(t.status === "Active" ? "success" : "muted")}>{t.status}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Thêm / cập nhật thuế suất</h2>
              <form
                className="grid gap-2 sm:grid-cols-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => upsertFinTax({
                    code: taxCode, name: taxName, ratePercent: Number(taxRate) || 0,
                    taxType, isDefault, status: "Active",
                  }), "Đã lưu thuế suất");
                }}
              >
                <input className={field} value={taxCode} onChange={(e) => setTaxCode(e.target.value)} placeholder="Mã" required />
                <input className={field} value={taxName} onChange={(e) => setTaxName(e.target.value)} placeholder="Tên" required />
                <input className={field} value={taxRate} onChange={(e) => setTaxRate(e.target.value)} placeholder="%" />
                <select className={field} value={taxType} onChange={(e) => setTaxType(e.target.value)}>
                  <option value="VatOutput">VatOutput</option>
                  <option value="VatInput">VatInput</option>
                  <option value="Other">Other</option>
                </select>
                <label className="flex items-center gap-2 text-sm sm:col-span-2">
                  <input type="checkbox" checked={isDefault} onChange={(e) => setIsDefault(e.target.checked)} />
                  Mặc định theo loại
                </label>
                <button className={`${btn.primary} sm:col-span-2`} type="submit">Lưu thuế suất</button>
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "docs" && (
        <div className="grid gap-4 xl:grid-cols-2">
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Bảng kê hóa đơn GTGT</h2>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>HĐ</th>
                    <th className={th}>Chiều</th>
                    <th className={th}>Thuế</th>
                    <th className={th}>TT</th>
                    <th className={th}></th>
                  </tr>
                </thead>
                <tbody>
                  {docs.map((d) => (
                    <tr key={d.id}>
                      <td className={td}>
                        <div className="font-medium">{d.invoiceNo}</div>
                        <div className="text-xs text-[var(--muted)]">
                          {d.code} · {d.partnerName || d.partnerCode || "—"}
                        </div>
                      </td>
                      <td className={td}>{d.direction}</td>
                      <td className={td}>
                        <div>{money(d.taxAmount)}</div>
                        <div className="text-xs text-[var(--muted)]">{d.ratePercent}% · HT {money(d.taxableAmount)}</div>
                      </td>
                      <td className={td}>
                        <span className={statusPill(
                          d.status === "Posted" ? "success" : d.status === "Void" ? "danger" : "brand",
                        )}>{d.status}</span>
                      </td>
                      <td className={td}>
                        {canManage && d.status === "Draft" && (
                          <div className="flex gap-1">
                            <button type="button" className={btn.ghost} onClick={() => void run(() => postFinVatDocument(d.id), "Đã ghi nhận")}>Ghi nhận</button>
                            <button type="button" className={btn.ghost} onClick={() => void run(() => voidFinVatDocument(d.id, "Hủy"), "Đã hủy")}>Hủy</button>
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                  {docs.length === 0 && (
                    <tr><td className={td} colSpan={5}>Chưa có dòng bảng kê.</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          </section>
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo dòng GTGT + tính thuế</h2>
              <form
                className="grid gap-2"
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(async () => {
                    const doc = await upsertFinVatDocument({
                      direction,
                      taxId: taxId || null,
                      invoiceNo,
                      invoiceDate: new Date().toISOString(),
                      partnerName: partnerName || null,
                      partnerTaxCode: partnerTax || null,
                      taxableAmount: Number(taxable) || 0,
                      periodId: periodId || null,
                    });
                    await postFinVatDocument(doc.id);
                  }, "Đã tạo & ghi nhận GTGT");
                }}
              >
                <select className={field} value={direction} onChange={(e) => setDirection(e.target.value)}>
                  <option value="Output">Đầu ra (Output)</option>
                  <option value="Input">Đầu vào (Input)</option>
                </select>
                <select className={field} value={taxId} onChange={(e) => setTaxId(e.target.value)}>
                  <option value="">— Thuế suất —</option>
                  {taxes.filter((t) => t.status === "Active").map((t) => (
                    <option key={t.id} value={t.id}>{t.code} · {t.ratePercent}%</option>
                  ))}
                </select>
                <input className={field} value={invoiceNo} onChange={(e) => setInvoiceNo(e.target.value)} placeholder="Số HĐ" required />
                <input className={field} value={partnerName} onChange={(e) => setPartnerName(e.target.value)} placeholder="Đối tác" />
                <input className={field} value={partnerTax} onChange={(e) => setPartnerTax(e.target.value)} placeholder="MST" />
                <input className={field} value={taxable} onChange={(e) => setTaxable(e.target.value)} placeholder="Tiền trước thuế" />
                <div className="flex gap-2">
                  <button
                    type="button"
                    className={btn.ghost}
                    onClick={() => {
                      void calculateFinVat({
                        taxableAmount: Number(taxable) || 0,
                        taxId: taxId || null,
                      }).then(setCalc).catch((e: Error) => setError(e.message));
                    }}
                  >
                    Xem tính thuế
                  </button>
                  <button className={btn.primary} type="submit">Tạo & ghi nhận</button>
                </div>
                {calc && (
                  <div className="rounded-md bg-black/5 px-3 py-2 text-sm">
                    {calc.taxCode || "—"} · {calc.ratePercent}% → thuế <b>{money(calc.taxAmount)}</b> · tổng <b>{money(calc.totalAmount)}</b>
                  </div>
                )}
              </form>
            </section>
          )}
        </div>
      )}

      {tab === "summary" && summary && (
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">
            Tổng hợp GTGT {summary.periodCode ? `· ${summary.periodCode}` : ""}
          </h2>
          <div className="mb-4 grid gap-3 sm:grid-cols-4 text-sm">
            <div className="rounded-md border border-black/10 px-3 py-2">
              <div className="text-xs text-[var(--muted)]">Đầu ra ({summary.outputCount})</div>
              <div className="font-semibold">{money(summary.outputTax)}</div>
              <div className="text-xs text-[var(--muted)]">HT {money(summary.outputTaxable)}</div>
            </div>
            <div className="rounded-md border border-black/10 px-3 py-2">
              <div className="text-xs text-[var(--muted)]">Đầu vào ({summary.inputCount})</div>
              <div className="font-semibold">{money(summary.inputTax)}</div>
              <div className="text-xs text-[var(--muted)]">HT {money(summary.inputTaxable)}</div>
            </div>
            <div className="rounded-md border border-black/10 px-3 py-2 sm:col-span-2">
              <div className="text-xs text-[var(--muted)]">GTGT phải nộp (ra − vào)</div>
              <div className="text-lg font-semibold">{money(summary.netVatPayable)}</div>
            </div>
          </div>
        </section>
      )}
    </div>
  );
}
