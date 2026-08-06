"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  assignCrmOwner,
  downloadCrmCustomersCsv,
  fetchCrmCustomer360,
  findCrmDuplicates,
  handoverCrmCustomer,
  importCrmCustomers,
  mergeCrmCustomers,
  searchCrmCustomers,
  upsertCrmContact,
  upsertCrmCustomer,
  type CrmCustomer360Dto,
  type CrmCustomerDto,
  type CrmDuplicateHitDto,
} from "@/shared/api/crm-api";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

const SEG_TONE: Record<string, "muted" | "brand" | "success" | "warning"> = {
  Lead: "muted",
  Prospect: "brand",
  Customer: "success",
  Partner: "warning",
};

export default function CrmCustomersPage() {
  const { can } = usePermissions();
  const canRead = can("crm.customer.read");
  const canManage = can("crm.customer.manage");

  const [list, setList] = useState<CrmCustomerDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<CrmCustomer360Dto | null>(null);
  const [dupHints, setDupHints] = useState<CrmDuplicateHitDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [q, setQ] = useState("");
  const [filterType, setFilterType] = useState("");
  const [filterSegment, setFilterSegment] = useState("");

  const [editingId, setEditingId] = useState<string | undefined>();
  const [code, setCode] = useState("KH-001");
  const [customerType, setCustomerType] = useState("Person");
  const [displayName, setDisplayName] = useState("");
  const [companyName, setCompanyName] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [taxCode, setTaxCode] = useState("");
  const [segment, setSegment] = useState("Prospect");
  const [ownerUserId, setOwnerUserId] = useState("");
  const [address, setAddress] = useState("");
  const [note, setNote] = useState("");

  const [contactName, setContactName] = useState("");
  const [contactPhone, setContactPhone] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [handoverTo, setHandoverTo] = useState("");
  const [handoverNote, setHandoverNote] = useState("");
  const [mergeSourceId, setMergeSourceId] = useState("");
  const [importText, setImportText] = useState(
    "Code,CustomerType,DisplayName,CompanyName,Phone,Email,TaxCode,Segment,Status,Address\n",
  );

  const loadList = useCallback(async () => {
    const [rows, dir] = await Promise.all([
      searchCrmCustomers({
        q: q || undefined,
        customerType: filterType || undefined,
        segment: filterSegment || undefined,
      }),
      fetchMsgDirectory().catch(() => [] as MsgDirectoryUserDto[]),
    ]);
    setList(rows);
    setUsers(dir);
    if (!selectedId && rows[0]) setSelectedId(rows[0].id);
    if (!ownerUserId && dir[0]) setOwnerUserId(dir[0].id);
    if (!handoverTo && dir[0]) setHandoverTo(dir[0].id);
  }, [q, filterType, filterSegment, selectedId, ownerUserId, handoverTo]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    loadList()
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, loadList]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchCrmCustomer360(selectedId)
      .then(setDetail)
      .catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  useEffect(() => {
    if (!canManage) return;
    const t = setTimeout(() => {
      if (!phone && !taxCode) {
        setDupHints([]);
        return;
      }
      findCrmDuplicates(phone || undefined, taxCode || undefined, editingId)
        .then(setDupHints)
        .catch(() => setDupHints([]));
    }, 400);
    return () => clearTimeout(t);
  }, [phone, taxCode, editingId, canManage]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  function fillForm(c: CrmCustomerDto) {
    setEditingId(c.id);
    setCode(c.code);
    setCustomerType(c.customerType);
    setDisplayName(c.displayName);
    setCompanyName(c.companyName ?? "");
    setPhone(c.phone ?? "");
    setEmail(c.email ?? "");
    setTaxCode(c.taxCode ?? "");
    setSegment(c.segment);
    setOwnerUserId(c.ownerUserId ?? "");
    setAddress(c.address ?? "");
    setNote(c.note ?? "");
    setSelectedId(c.id);
  }

  function resetForm() {
    setEditingId(undefined);
    setDisplayName("");
    setCompanyName("");
    setPhone("");
    setEmail("");
    setTaxCode("");
    setAddress("");
    setNote("");
  }

  async function onSave(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertCrmCustomer({
        id: editingId,
        code,
        customerType,
        displayName,
        companyName: companyName || undefined,
        phone: phone || undefined,
        email: email || undefined,
        taxCode: taxCode || undefined,
        segment,
        ownerUserId: ownerUserId || null,
        address: address || undefined,
        note: note || undefined,
        status: "Active",
      });
      resetForm();
      await loadList();
      setSelectedId(saved.id);
      flash(editingId ? "Đã cập nhật KH." : "Đã tạo KH.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onAddContact(e: FormEvent) {
    e.preventDefault();
    if (!selectedId) return;
    try {
      await upsertCrmContact(selectedId, {
        fullName: contactName,
        phone: contactPhone || undefined,
        email: contactEmail || undefined,
        isPrimary: true,
      });
      setContactName("");
      setContactPhone("");
      setContactEmail("");
      setDetail(await fetchCrmCustomer360(selectedId));
      await loadList();
      flash("Đã thêm liên hệ.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onAssign() {
    if (!selectedId || !ownerUserId) return;
    try {
      await assignCrmOwner(selectedId, ownerUserId);
      setDetail(await fetchCrmCustomer360(selectedId));
      await loadList();
      flash("Đã gán phụ trách.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onHandover(e: FormEvent) {
    e.preventDefault();
    if (!selectedId || !handoverTo) return;
    try {
      await handoverCrmCustomer(selectedId, handoverTo, handoverNote || undefined);
      setHandoverNote("");
      setDetail(await fetchCrmCustomer360(selectedId));
      await loadList();
      flash("Đã bàn giao.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onMerge(e: FormEvent) {
    e.preventDefault();
    if (!selectedId || !mergeSourceId) return;
    try {
      await mergeCrmCustomers(mergeSourceId, selectedId);
      setMergeSourceId("");
      await loadList();
      setDetail(await fetchCrmCustomer360(selectedId));
      flash("Đã gộp khách trùng vào KH đang chọn.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onExport() {
    try {
      const blob = await downloadCrmCustomersCsv();
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = "crm-customers.csv";
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onImport(e: FormEvent) {
    e.preventDefault();
    try {
      const r = await importCrmCustomers(importText);
      await loadList();
      flash(`Import: ${r.success}/${r.total} OK · ${r.failed} lỗi`);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem khách hàng.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Khách hàng</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          CN/DN · trùng SĐT/MST · gộp · phân loại · phụ trách · 360° · liên hệ · import/export (UC_CRM_001–015 Must)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}

      <section className={`${panel} flex flex-wrap gap-2`}>
        <input className={`${field} min-w-[180px] flex-1`} placeholder="Tìm mã / tên / SĐT / MST / email…" value={q} onChange={(e) => setQ(e.target.value)} />
        <select className={field} value={filterType} onChange={(e) => setFilterType(e.target.value)}>
          <option value="">Mọi loại</option>
          <option value="Person">Cá nhân</option>
          <option value="Organization">Doanh nghiệp</option>
        </select>
        <select className={field} value={filterSegment} onChange={(e) => setFilterSegment(e.target.value)}>
          <option value="">Mọi tệp</option>
          <option value="Lead">Lead</option>
          <option value="Prospect">Prospect</option>
          <option value="Customer">Customer</option>
          <option value="Partner">Partner</option>
        </select>
        <button type="button" className={btn.ghost} onClick={() => loadList().catch((e: Error) => setError(e.message))}>
          Lọc
        </button>
        <button type="button" className={btn.ghost} onClick={onExport}>Export CSV</button>
      </section>

      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-[1fr_1.15fr]">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách ({list.length})</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>Loại</th>
                  <th className={th}>Tệp</th>
                  <th className={th}>Phụ trách</th>
                </tr>
              </thead>
              <tbody>
                {list.map((c) => (
                  <tr
                    key={c.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedId === c.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedId(c.id)}
                  >
                    <td className={td}>{c.code}</td>
                    <td className={td}>
                      <div>{c.displayName}</div>
                      <div className="text-xs text-[var(--muted)]">{c.phone || c.taxCode || "—"}</div>
                    </td>
                    <td className={td}>{c.customerType === "Organization" ? "DN" : "CN"}</td>
                    <td className={td}>
                      <span className={statusPill(SEG_TONE[c.segment] ?? "muted")}>{c.segment}</span>
                    </td>
                    <td className={td}>{c.ownerName ?? "—"}</td>
                  </tr>
                ))}
                {list.length === 0 && (
                  <tr><td className={td} colSpan={5}>Chưa có khách.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">{editingId ? "Sửa khách hàng" : "Tạo khách hàng"}</h2>
              <form onSubmit={onSave} className="grid gap-2 sm:grid-cols-2">
                <input className={field} placeholder="Mã" value={code} onChange={(e) => setCode(e.target.value)} required />
                <select className={field} value={customerType} onChange={(e) => setCustomerType(e.target.value)}>
                  <option value="Person">Cá nhân</option>
                  <option value="Organization">Doanh nghiệp</option>
                </select>
                <input className={`${field} sm:col-span-2`} placeholder="Tên hiển thị" value={displayName} onChange={(e) => setDisplayName(e.target.value)} required />
                {customerType === "Organization" && (
                  <input className={`${field} sm:col-span-2`} placeholder="Tên công ty" value={companyName} onChange={(e) => setCompanyName(e.target.value)} />
                )}
                <input className={field} placeholder="SĐT" value={phone} onChange={(e) => setPhone(e.target.value)} />
                <input className={field} placeholder="MST" value={taxCode} onChange={(e) => setTaxCode(e.target.value)} />
                <input className={field} placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
                <select className={field} value={segment} onChange={(e) => setSegment(e.target.value)}>
                  <option value="Lead">Lead</option>
                  <option value="Prospect">Prospect</option>
                  <option value="Customer">Customer</option>
                  <option value="Partner">Partner</option>
                </select>
                <select className={`${field} sm:col-span-2`} value={ownerUserId} onChange={(e) => setOwnerUserId(e.target.value)}>
                  <option value="">— Phụ trách —</option>
                  {users.map((u) => (
                    <option key={u.id} value={u.id}>{u.displayName || u.username}</option>
                  ))}
                </select>
                <input className={`${field} sm:col-span-2`} placeholder="Địa chỉ" value={address} onChange={(e) => setAddress(e.target.value)} />
                <textarea className={`${field} sm:col-span-2`} rows={2} placeholder="Ghi chú" value={note} onChange={(e) => setNote(e.target.value)} />
                {dupHints.length > 0 && (
                  <div className="sm:col-span-2 rounded-md bg-amber-50 px-3 py-2 text-xs text-amber-800">
                    Có thể trùng: {dupHints.map((d) => `${d.code} (${d.matchField})`).join(", ")}
                  </div>
                )}
                <div className="flex flex-wrap gap-2 sm:col-span-2">
                  <button type="submit" className={btn.primary}>{editingId ? "Cập nhật" : "Tạo mới"}</button>
                  {editingId && <button type="button" className={btn.ghost} onClick={resetForm}>Hủy sửa</button>}
                  {detail && (
                    <button type="button" className={btn.ghost} onClick={() => fillForm(detail.customer)}>Sửa KH đang chọn</button>
                  )}
                </div>
              </form>
            </section>
          )}

          {detail && (
            <section className={panel}>
              <div className="mb-3">
                <h2 className="text-sm font-semibold">360° · {detail.customer.displayName}</h2>
                <p className="text-xs text-[var(--muted)]">
                  {detail.customer.code} · {detail.customer.customerType} · {detail.customer.segment}
                  {detail.customer.ownerName ? ` · PT: ${detail.customer.ownerName}` : ""}
                </p>
              </div>

              <div className="mb-4 grid gap-2 text-sm sm:grid-cols-2">
                <div>SĐT: {detail.customer.phone || "—"}</div>
                <div>MST: {detail.customer.taxCode || "—"}</div>
                <div>Email: {detail.customer.email || "—"}</div>
                <div>Liên hệ: {detail.contacts.length}</div>
                <div className="sm:col-span-2">Địa chỉ: {detail.customer.address || "—"}</div>
              </div>

              <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-[var(--muted)]">Người liên hệ</h3>
              <ul className="mb-3 space-y-1 text-sm">
                {detail.contacts.map((c) => (
                  <li key={c.id}>
                    {c.isPrimary ? "★ " : ""}{c.fullName}
                    {c.phone ? ` · ${c.phone}` : ""}
                    {c.email ? ` · ${c.email}` : ""}
                  </li>
                ))}
                {detail.contacts.length === 0 && <li className="text-[var(--muted)]">Chưa có liên hệ.</li>}
              </ul>

              {canManage && (
                <form onSubmit={onAddContact} className="mb-4 grid gap-2 sm:grid-cols-3">
                  <input className={field} placeholder="Tên liên hệ" value={contactName} onChange={(e) => setContactName(e.target.value)} required />
                  <input className={field} placeholder="SĐT" value={contactPhone} onChange={(e) => setContactPhone(e.target.value)} />
                  <input className={field} placeholder="Email" value={contactEmail} onChange={(e) => setContactEmail(e.target.value)} />
                  <button type="submit" className={`${btn.primary} sm:col-span-3`}>Thêm liên hệ</button>
                </form>
              )}

              <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-[var(--muted)]">Bàn giao</h3>
              <ul className="mb-3 space-y-1 text-xs text-[var(--muted)]">
                {detail.handovers.map((h) => (
                  <li key={h.id}>
                    {new Date(h.handedAt).toLocaleString("vi-VN")}: {h.fromUserName ?? "—"} → {h.toUserName}
                    {h.note ? ` · ${h.note}` : ""}
                  </li>
                ))}
                {detail.handovers.length === 0 && <li>Chưa có lịch sử bàn giao.</li>}
              </ul>

              {canManage && (
                <div className="space-y-3 border-t border-[var(--border)] pt-3">
                  <div className="flex flex-wrap gap-2">
                    <select className={`${field} min-w-[160px] flex-1`} value={ownerUserId} onChange={(e) => setOwnerUserId(e.target.value)}>
                      {users.map((u) => (
                        <option key={u.id} value={u.id}>{u.displayName || u.username}</option>
                      ))}
                    </select>
                    <button type="button" className={btn.ghost} onClick={onAssign}>Gán phụ trách</button>
                  </div>
                  <form onSubmit={onHandover} className="flex flex-wrap gap-2">
                    <select className={`${field} min-w-[160px] flex-1`} value={handoverTo} onChange={(e) => setHandoverTo(e.target.value)}>
                      {users.map((u) => (
                        <option key={u.id} value={u.id}>{u.displayName || u.username}</option>
                      ))}
                    </select>
                    <input className={`${field} flex-1`} placeholder="Ghi chú bàn giao" value={handoverNote} onChange={(e) => setHandoverNote(e.target.value)} />
                    <button type="submit" className={btn.primary}>Bàn giao</button>
                  </form>
                  <form onSubmit={onMerge} className="flex flex-wrap gap-2">
                    <select className={`${field} min-w-[200px] flex-1`} value={mergeSourceId} onChange={(e) => setMergeSourceId(e.target.value)}>
                      <option value="">— Gộp KH nguồn vào đang chọn —</option>
                      {list.filter((c) => c.id !== selectedId).map((c) => (
                        <option key={c.id} value={c.id}>{c.code} · {c.displayName}</option>
                      ))}
                    </select>
                    <button type="submit" className={btn.ghost} disabled={!mergeSourceId}>Gộp trùng</button>
                  </form>
                </div>
              )}

              {detail.possibleDuplicates.length > 0 && (
                <div className="mt-3 rounded-md bg-amber-50 px-3 py-2 text-xs text-amber-800">
                  Trùng khả dĩ: {detail.possibleDuplicates.map((d) => `${d.code}/${d.matchField}`).join(", ")}
                </div>
              )}
            </section>
          )}

          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Import CSV</h2>
              <form onSubmit={onImport} className="space-y-2">
                <textarea className={field} rows={5} value={importText} onChange={(e) => setImportText(e.target.value)} />
                <button type="submit" className={btn.primary}>Import</button>
              </form>
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
