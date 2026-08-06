"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPurVendorDetail,
  fetchPurVendors,
  upsertPurVendor,
  upsertPurVendorContact,
  upsertPurVendorPrice,
  upsertPurVendorProduct,
  type PurVendorDetailDto,
  type PurVendorDto,
} from "@/shared/api/pur-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function today() {
  return new Date().toISOString().slice(0, 10);
}

export default function PurVendorsPage() {
  const { can } = usePermissions();
  const canRead = can("pur.vendor.read");
  const canManage = can("pur.vendor.manage");

  const [list, setList] = useState<PurVendorDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<PurVendorDetailDto | null>(null);
  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [code, setCode] = useState("NCC-001");
  const [name, setName] = useState("");
  const [taxCode, setTaxCode] = useState("");
  const [phone, setPhone] = useState("");
  const [terms, setTerms] = useState("Net 30");

  const [contactName, setContactName] = useState("");
  const [contactPhone, setContactPhone] = useState("");
  const [prodCode, setProdCode] = useState("SP-001");
  const [prodName, setProdName] = useState("");
  const [price, setPrice] = useState("0");

  const load = useCallback(async () => {
    const rows = await fetchPurVendors(q || undefined);
    setList(rows);
    if (!selectedId && rows[0]) setSelectedId(rows[0].id);
  }, [q, selectedId]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    load()
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchPurVendorDetail(selectedId)
      .then(setDetail)
      .catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function onSave(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertPurVendor({
        code,
        name,
        taxCode: taxCode || undefined,
        phone: phone || undefined,
        paymentTerms: terms || undefined,
      });
      setName("");
      setTaxCode("");
      setPhone("");
      await load();
      setSelectedId(saved.id);
      flash("Đã lưu NCC.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function refresh() {
    if (!selectedId) return;
    setDetail(await fetchPurVendorDetail(selectedId));
    await load();
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem NCC.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Nhà cung cấp</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          NCC · liên hệ & điều khoản · SP–NCC · giá mua (UC_PUR_001, 003, 009, 010)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}

      <section className={`${panel} flex gap-2`}>
        <input className={`${field} flex-1`} placeholder="Tìm NCC…" value={q} onChange={(e) => setQ(e.target.value)} />
        <button type="button" className={btn.ghost} onClick={() => load().catch((e: Error) => setError(e.message))}>
          Lọc
        </button>
      </section>

      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-[1fr_1.2fr]">
        <section className={panel}>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>LH</th>
                  <th className={th}>SP</th>
                </tr>
              </thead>
              <tbody>
                {list.map((v) => (
                  <tr
                    key={v.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedId === v.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedId(v.id)}
                  >
                    <td className={td}>{v.code}</td>
                    <td className={td}>
                      <div>{v.name}</div>
                      <span className={statusPill(v.status === "Active" ? "success" : "muted")}>{v.status}</span>
                    </td>
                    <td className={td}>{v.contactCount}</td>
                    <td className={td}>{v.productCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo / cập nhật NCC</h2>
              <form onSubmit={onSave} className="grid gap-2 sm:grid-cols-2">
                <input className={field} value={code} onChange={(e) => setCode(e.target.value)} required />
                <input className={field} placeholder="Tên NCC" value={name} onChange={(e) => setName(e.target.value)} required />
                <input className={field} placeholder="MST" value={taxCode} onChange={(e) => setTaxCode(e.target.value)} />
                <input className={field} placeholder="SĐT" value={phone} onChange={(e) => setPhone(e.target.value)} />
                <input className={`${field} sm:col-span-2`} placeholder="Điều khoản TT" value={terms} onChange={(e) => setTerms(e.target.value)} />
                <button type="submit" className={`${btn.primary} sm:col-span-2`}>Lưu NCC</button>
              </form>
            </section>
          )}

          {detail && (
            <section className={panel}>
              <h2 className="mb-1 text-sm font-semibold">{detail.vendor.name}</h2>
              <p className="mb-3 text-xs text-[var(--muted)]">
                {detail.vendor.code} · {detail.vendor.paymentTerms || "—"} · {detail.vendor.phone || "—"}
              </p>

              <h3 className="mb-1 text-xs font-semibold uppercase text-[var(--muted)]">Liên hệ</h3>
              <ul className="mb-2 space-y-1 text-sm">
                {detail.contacts.map((c) => (
                  <li key={c.id}>{c.isPrimary ? "★ " : ""}{c.fullName}{c.phone ? ` · ${c.phone}` : ""}</li>
                ))}
              </ul>
              {canManage && (
                <form
                  className="mb-4 grid gap-2 sm:grid-cols-3"
                  onSubmit={async (e) => {
                    e.preventDefault();
                    try {
                      await upsertPurVendorContact(selectedId, {
                        fullName: contactName,
                        phone: contactPhone || undefined,
                        isPrimary: true,
                      });
                      setContactName("");
                      setContactPhone("");
                      await refresh();
                      flash("Đã thêm liên hệ.");
                    } catch (err) {
                      setError((err as Error).message);
                    }
                  }}
                >
                  <input className={field} placeholder="Tên LH" value={contactName} onChange={(e) => setContactName(e.target.value)} required />
                  <input className={field} placeholder="SĐT" value={contactPhone} onChange={(e) => setContactPhone(e.target.value)} />
                  <button type="submit" className={btn.ghost}>Thêm LH</button>
                </form>
              )}

              <h3 className="mb-1 text-xs font-semibold uppercase text-[var(--muted)]">SP – NCC / giá</h3>
              <ul className="mb-2 space-y-1 text-sm">
                {detail.products.map((p) => (
                  <li key={p.id}>{p.productCode} · {p.productName}{p.isPreferred ? " · ưu tiên" : ""}</li>
                ))}
                {detail.prices.map((p) => (
                  <li key={p.id} className="text-[var(--muted)]">
                    Giá {p.productCode}: {p.unitPrice.toLocaleString("vi-VN")} {p.currency} (từ {p.effectiveFrom})
                  </li>
                ))}
              </ul>
              {canManage && (
                <form
                  className="grid gap-2 sm:grid-cols-2"
                  onSubmit={async (e) => {
                    e.preventDefault();
                    try {
                      await upsertPurVendorProduct(selectedId, {
                        productCode: prodCode,
                        productName: prodName,
                        isPreferred: true,
                      });
                      await upsertPurVendorPrice(selectedId, {
                        productCode: prodCode,
                        productName: prodName,
                        unitPrice: Number(price) || 0,
                        effectiveFrom: today(),
                      });
                      setProdName("");
                      await refresh();
                      flash("Đã gắn SP + giá mua.");
                    } catch (err) {
                      setError((err as Error).message);
                    }
                  }}
                >
                  <input className={field} value={prodCode} onChange={(e) => setProdCode(e.target.value)} required />
                  <input className={field} placeholder="Tên SP" value={prodName} onChange={(e) => setProdName(e.target.value)} required />
                  <input className={field} type="number" min={0} value={price} onChange={(e) => setPrice(e.target.value)} />
                  <button type="submit" className={btn.primary}>Gắn SP + giá</button>
                </form>
              )}
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
