"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPosBom,
  fetchPosCategories,
  fetchPosPriceItems,
  fetchPosPriceLists,
  fetchPosProducts,
  fetchPosStores,
  fetchPosTaxRates,
  setPosProductStatus,
  syncPosCatalog,
  upsertPosBom,
  upsertPosCategory,
  upsertPosPriceItem,
  upsertPosPriceList,
  upsertPosProduct,
  upsertPosTaxRate,
  type PosBomLineDto,
  type PosCategoryDto,
  type PosPriceItemDto,
  type PosPriceListDto,
  type PosProductDto,
  type PosStoreDto,
  type PosTaxRateDto,
} from "@/shared/api/pos-api";
import { formatCatalogSyncMessage } from "@/shared/api/pos-doc-helpers";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PosCatalogPage() {
  const { can } = usePermissions();
  const canRead = can("pos.catalog.read");
  const canManage = can("pos.catalog.manage");

  const [categories, setCategories] = useState<PosCategoryDto[]>([]);
  const [products, setProducts] = useState<PosProductDto[]>([]);
  const [taxes, setTaxes] = useState<PosTaxRateDto[]>([]);
  const [priceLists, setPriceLists] = useState<PosPriceListDto[]>([]);
  const [stores, setStores] = useState<PosStoreDto[]>([]);
  const [selectedProductId, setSelectedProductId] = useState("");
  const [bom, setBom] = useState<PosBomLineDto[]>([]);
  const [selectedPriceListId, setSelectedPriceListId] = useState("");
  const [priceItems, setPriceItems] = useState<PosPriceItemDto[]>([]);
  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [catCode, setCatCode] = useState("NHOM-01");
  const [catName, setCatName] = useState("");
  const [prodCode, setProdCode] = useState("SP-001");
  const [prodName, setProdName] = useState("");
  const [prodCatId, setProdCatId] = useState("");
  const [prodUnit, setProdUnit] = useState("ly");
  const [matCode, setMatCode] = useState("NVL-01");
  const [matName, setMatName] = useState("");
  const [matQty, setMatQty] = useState("1");
  const [taxCode, setTaxCode] = useState("VAT10");
  const [taxName, setTaxName] = useState("GTGT 10%");
  const [taxPct, setTaxPct] = useState("10");
  const [plCode, setPlCode] = useState("BG-01");
  const [plName, setPlName] = useState("");
  const [plStoreId, setPlStoreId] = useState("");
  const [priceProductId, setPriceProductId] = useState("");
  const [price, setPrice] = useState("0");
  const [priceTaxId, setPriceTaxId] = useState("");

  const load = useCallback(async () => {
    const [c, p, t, pl, s] = await Promise.all([
      fetchPosCategories(),
      fetchPosProducts(q || undefined),
      fetchPosTaxRates(),
      fetchPosPriceLists(),
      fetchPosStores().catch(() => [] as PosStoreDto[]),
    ]);
    setCategories(c);
    setProducts(p);
    setTaxes(t);
    setPriceLists(pl);
    setStores(s);
    if (!selectedProductId && p[0]) setSelectedProductId(p[0].id);
    if (!prodCatId && c[0]) setProdCatId(c[0].id);
    if (!selectedPriceListId && pl[0]) setSelectedPriceListId(pl[0].id);
    if (!plStoreId && s[0]) setPlStoreId(s[0].id);
    if (!priceProductId && p[0]) setPriceProductId(p[0].id);
    if (!priceTaxId && t[0]) setPriceTaxId(t[0].id);
  }, [q, selectedProductId, prodCatId, selectedPriceListId, plStoreId, priceProductId, priceTaxId]);

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
    if (!selectedProductId || !canRead) return;
    fetchPosBom(selectedProductId)
      .then(setBom)
      .catch((e: Error) => setError(e.message));
  }, [selectedProductId, canRead]);

  useEffect(() => {
    if (!selectedPriceListId || !canRead) return;
    fetchPosPriceItems(selectedPriceListId)
      .then(setPriceItems)
      .catch((e: Error) => setError(e.message));
  }, [selectedPriceListId, canRead]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function onCat(e: FormEvent) {
    e.preventDefault();
    try {
      await upsertPosCategory({ code: catCode, name: catName });
      setCatName("");
      await load();
      flash("Đã lưu nhóm SP.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onProd(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertPosProduct({
        code: prodCode,
        name: prodName,
        categoryId: prodCatId || null,
        unit: prodUnit,
        status: "Active",
      });
      setProdName("");
      await load();
      setSelectedProductId(saved.id);
      flash("Đã lưu sản phẩm.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onBom(e: FormEvent) {
    e.preventDefault();
    if (!selectedProductId) return;
    try {
      await upsertPosBom(selectedProductId, {
        materialCode: matCode,
        materialName: matName,
        qty: Number(matQty) || 1,
        unit: "g",
      });
      setMatName("");
      setBom(await fetchPosBom(selectedProductId));
      await load();
      flash("Đã thêm dòng BOM.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onTax(e: FormEvent) {
    e.preventDefault();
    try {
      await upsertPosTaxRate({
        code: taxCode,
        name: taxName,
        ratePct: Number(taxPct) || 0,
        isDefault: true,
      });
      await load();
      flash("Đã lưu thuế.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onPriceList(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertPosPriceList({
        storeId: plStoreId,
        code: plCode,
        name: plName || plCode,
      });
      setPlName("");
      await load();
      setSelectedPriceListId(saved.id);
      flash("Đã tạo bảng giá.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onPriceItem(e: FormEvent) {
    e.preventDefault();
    if (!selectedPriceListId) return;
    try {
      await upsertPosPriceItem(selectedPriceListId, {
        productId: priceProductId,
        price: Number(price) || 0,
        taxRateId: priceTaxId || null,
      });
      setPriceItems(await fetchPosPriceItems(selectedPriceListId));
      await load();
      flash("Đã gán giá SP.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function toggleSuspend(p: PosProductDto) {
    try {
      await setPosProductStatus(p.id, p.status === "Suspended" ? "Active" : "Suspended");
      await load();
      flash(p.status === "Suspended" ? "Đã mở bán lại." : "Đã ngưng bán tạm.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onSync() {
    try {
      const r = await syncPosCatalog();
      await load();
      flash(`${formatCatalogSyncMessage(r)} · ${new Date(r.syncedAt).toLocaleString("vi-VN")}`);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem catalog POS.</div>;
  }

  const selected = products.find((p) => p.id === selectedProductId) ?? null;

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Catalog & giá POS</h1>
          <p className="mt-1 text-sm text-[var(--muted)]">
            Nhóm · SP · BOM · ngưng bán · sync · bảng giá · thuế (UC_POS_009–010, 012, 014–016, 019)
          </p>
        </div>
        {canManage && (
          <button type="button" className={btn.primary} onClick={onSync}>
            Đồng bộ catalog
          </button>
        )}
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <div className="mb-3 flex flex-wrap gap-2">
            <input className={`${field} flex-1`} placeholder="Tìm SP…" value={q} onChange={(e) => setQ(e.target.value)} />
            <button type="button" className={btn.ghost} onClick={() => load().catch((e: Error) => setError(e.message))}>
              Lọc
            </button>
          </div>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>Nhóm</th>
                  <th className={th}>TT</th>
                  <th className={th} />
                </tr>
              </thead>
              <tbody>
                {products.map((p) => (
                  <tr
                    key={p.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedProductId === p.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedProductId(p.id)}
                  >
                    <td className={td}>{p.code}</td>
                    <td className={td}>{p.name}</td>
                    <td className={td}>{p.categoryName ?? "—"}</td>
                    <td className={td}>
                      <span className={statusPill(p.status === "Active" ? "success" : "warning")}>{p.status}</span>
                    </td>
                    <td className={td}>
                      {canManage && (
                        <button
                          type="button"
                          className={btn.ghost}
                          onClick={(e) => {
                            e.stopPropagation();
                            void toggleSuspend(p);
                          }}
                        >
                          {p.status === "Suspended" ? "Mở bán" : "Ngưng"}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <>
              <section className={panel}>
                <h2 className="mb-3 text-sm font-semibold">Nhóm sản phẩm</h2>
                <form onSubmit={onCat} className="grid gap-2 sm:grid-cols-3">
                  <input className={field} value={catCode} onChange={(e) => setCatCode(e.target.value)} required />
                  <input className={field} placeholder="Tên nhóm" value={catName} onChange={(e) => setCatName(e.target.value)} required />
                  <button type="submit" className={btn.primary}>Lưu nhóm</button>
                </form>
                <ul className="mt-2 text-xs text-[var(--muted)]">
                  {categories.map((c) => (
                    <li key={c.id}>{c.code} · {c.name} ({c.productCount})</li>
                  ))}
                </ul>
              </section>

              <section className={panel}>
                <h2 className="mb-3 text-sm font-semibold">Sản phẩm bán</h2>
                <form onSubmit={onProd} className="grid gap-2 sm:grid-cols-2">
                  <input className={field} value={prodCode} onChange={(e) => setProdCode(e.target.value)} required />
                  <input className={field} placeholder="Tên SP" value={prodName} onChange={(e) => setProdName(e.target.value)} required />
                  <select className={field} value={prodCatId} onChange={(e) => setProdCatId(e.target.value)}>
                    <option value="">— Nhóm —</option>
                    {categories.map((c) => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                  <input className={field} placeholder="ĐVT" value={prodUnit} onChange={(e) => setProdUnit(e.target.value)} />
                  <button type="submit" className={`${btn.primary} sm:col-span-2`}>Lưu SP</button>
                </form>
              </section>
            </>
          )}

          {selected && (
            <section className={panel}>
              <h2 className="mb-2 text-sm font-semibold">BOM · {selected.name}</h2>
              <p className="mb-2 text-xs text-[var(--muted)]">
                Sync: {selected.syncedAt ? new Date(selected.syncedAt).toLocaleString("vi-VN") : "chưa"}
              </p>
              <ul className="mb-3 space-y-1 text-sm">
                {bom.map((b) => (
                  <li key={b.id}>{b.materialCode} · {b.materialName} · {b.qty} {b.unit}</li>
                ))}
                {bom.length === 0 && <li className="text-[var(--muted)]">Chưa có định mức.</li>}
              </ul>
              {canManage && (
                <form onSubmit={onBom} className="grid gap-2 sm:grid-cols-2">
                  <input className={field} value={matCode} onChange={(e) => setMatCode(e.target.value)} required />
                  <input className={field} placeholder="Tên NVL" value={matName} onChange={(e) => setMatName(e.target.value)} required />
                  <input className={field} type="number" min={0.001} step="any" value={matQty} onChange={(e) => setMatQty(e.target.value)} />
                  <button type="submit" className={btn.primary}>Thêm BOM</button>
                </form>
              )}
            </section>
          )}

          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Thuế GTGT</h2>
            <ul className="mb-3 space-y-1 text-sm">
              {taxes.map((t) => (
                <li key={t.id}>
                  {t.code} · {t.name} · {t.ratePct}%
                  {t.isDefault ? " · mặc định" : ""}
                </li>
              ))}
            </ul>
            {canManage && (
              <form onSubmit={onTax} className="grid gap-2 sm:grid-cols-2">
                <input className={field} value={taxCode} onChange={(e) => setTaxCode(e.target.value)} required />
                <input className={field} value={taxName} onChange={(e) => setTaxName(e.target.value)} required />
                <input className={field} type="number" value={taxPct} onChange={(e) => setTaxPct(e.target.value)} />
                <button type="submit" className={btn.primary}>Lưu thuế</button>
              </form>
            )}
          </section>

          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Bảng giá theo điểm bán</h2>
            <select
              className={`${field} mb-2`}
              value={selectedPriceListId}
              onChange={(e) => setSelectedPriceListId(e.target.value)}
            >
              <option value="">— Chọn bảng giá —</option>
              {priceLists.map((pl) => (
                <option key={pl.id} value={pl.id}>
                  {pl.code} · {pl.storeName} ({pl.itemCount})
                </option>
              ))}
            </select>
            <ul className="mb-3 space-y-1 text-sm">
              {priceItems.map((i) => (
                <li key={i.id}>
                  {i.productCode} · {i.price.toLocaleString("vi-VN")}
                  {i.taxCode ? ` · ${i.taxCode}` : ""}
                </li>
              ))}
            </ul>
            {canManage && (
              <div className="space-y-3 border-t border-[var(--border)] pt-3">
                <form onSubmit={onPriceList} className="grid gap-2 sm:grid-cols-2">
                  <select className={field} value={plStoreId} onChange={(e) => setPlStoreId(e.target.value)}>
                    {stores.map((s) => (
                      <option key={s.id} value={s.id}>{s.code} · {s.name}</option>
                    ))}
                  </select>
                  <input className={field} value={plCode} onChange={(e) => setPlCode(e.target.value)} required />
                  <input className={`${field} sm:col-span-2`} placeholder="Tên bảng giá" value={plName} onChange={(e) => setPlName(e.target.value)} />
                  <button type="submit" className={`${btn.ghost} sm:col-span-2`}>Tạo bảng giá</button>
                </form>
                <form onSubmit={onPriceItem} className="grid gap-2 sm:grid-cols-2">
                  <select className={field} value={priceProductId} onChange={(e) => setPriceProductId(e.target.value)}>
                    {products.map((p) => (
                      <option key={p.id} value={p.id}>{p.code} · {p.name}</option>
                    ))}
                  </select>
                  <input className={field} type="number" min={0} value={price} onChange={(e) => setPrice(e.target.value)} />
                  <select className={field} value={priceTaxId} onChange={(e) => setPriceTaxId(e.target.value)}>
                    <option value="">— Thuế —</option>
                    {taxes.map((t) => (
                      <option key={t.id} value={t.id}>{t.code} ({t.ratePct}%)</option>
                    ))}
                  </select>
                  <button type="submit" className={btn.primary} disabled={!selectedPriceListId}>Gán giá</button>
                </form>
              </div>
            )}
          </section>
        </div>
      </div>
    </div>
  );
}
