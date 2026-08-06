import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PurVendorDto = {
  id: string;
  code: string;
  name: string;
  taxCode?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  paymentTerms?: string | null;
  status: string;
  contactCount: number;
  productCount: number;
};

export type PurVendorContactDto = {
  id: string;
  vendorId: string;
  fullName: string;
  title?: string | null;
  phone?: string | null;
  email?: string | null;
  isPrimary: boolean;
};

export type PurVendorProductDto = {
  id: string;
  vendorId: string;
  productCode: string;
  productName: string;
  isPreferred: boolean;
};

export type PurVendorPriceDto = {
  id: string;
  vendorId: string;
  productCode: string;
  productName: string;
  unitPrice: number;
  currency: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
};

export type PurVendorDetailDto = {
  vendor: PurVendorDto;
  contacts: PurVendorContactDto[];
  products: PurVendorProductDto[];
  prices: PurVendorPriceDto[];
};

export type PurPurchaseRequestDto = {
  id: string;
  code: string;
  requestingUnit?: string | null;
  note?: string | null;
  status: string;
  decisionNote?: string | null;
  requestedBy: string;
  requestedByName?: string | null;
  decidedBy?: string | null;
  decidedByName?: string | null;
  decidedAt?: string | null;
  lineCount: number;
};

export type PurPrLineDto = {
  id: string;
  prId: string;
  productCode: string;
  productName: string;
  qty: number;
  unit: string;
  note?: string | null;
};

export type PurPrDetailDto = {
  header: PurPurchaseRequestDto;
  lines: PurPrLineDto[];
};

export type PurPurchaseOrderDto = {
  id: string;
  code: string;
  vendorId: string;
  vendorName?: string | null;
  sourcePrId?: string | null;
  sourcePrCode?: string | null;
  status: string;
  version: number;
  totalAmount: number;
  currency: string;
  note?: string | null;
  createdByUserId: string;
  createdByName?: string | null;
  approvedBy?: string | null;
  approvedByName?: string | null;
  approvedAt?: string | null;
  sentAt?: string | null;
  printedAt?: string | null;
  closedAt?: string | null;
  cancelReason?: string | null;
  lineCount: number;
  receivedPct: number;
};

export type PurPoLineDto = {
  id: string;
  poId: string;
  productCode: string;
  productName: string;
  qty: number;
  receivedQty: number;
  invoicedQty: number;
  unitPrice: number;
  unit: string;
};

export type PurPoDetailDto = {
  header: PurPurchaseOrderDto;
  lines: PurPoLineDto[];
};

export async function fetchPurVendors(q?: string) {
  const { data } = await api.get<Envelope<PurVendorDto[]>>("/api/pur/vendors", { params: { q } });
  return data.data;
}

export async function upsertPurVendor(body: {
  id?: string;
  code: string;
  name: string;
  taxCode?: string;
  phone?: string;
  email?: string;
  address?: string;
  paymentTerms?: string;
  status?: string;
}) {
  const { data } = await api.post<Envelope<PurVendorDto>>("/api/pur/vendors", body);
  return data.data;
}

export async function fetchPurVendorDetail(id: string) {
  const { data } = await api.get<Envelope<PurVendorDetailDto>>(`/api/pur/vendors/${id}`);
  return data.data;
}

export async function upsertPurVendorContact(
  vendorId: string,
  body: {
    id?: string;
    fullName: string;
    title?: string;
    phone?: string;
    email?: string;
    isPrimary?: boolean;
  },
) {
  const { data } = await api.post<Envelope<PurVendorContactDto>>(
    `/api/pur/vendors/${vendorId}/contacts`,
    body,
  );
  return data.data;
}

export async function upsertPurVendorProduct(
  vendorId: string,
  body: { id?: string; productCode: string; productName: string; isPreferred?: boolean },
) {
  const { data } = await api.post<Envelope<PurVendorProductDto>>(
    `/api/pur/vendors/${vendorId}/products`,
    body,
  );
  return data.data;
}

export async function upsertPurVendorPrice(
  vendorId: string,
  body: {
    id?: string;
    productCode: string;
    productName: string;
    unitPrice: number;
    currency?: string;
    effectiveFrom: string;
    effectiveTo?: string | null;
  },
) {
  const { data } = await api.post<Envelope<PurVendorPriceDto>>(
    `/api/pur/vendors/${vendorId}/prices`,
    body,
  );
  return data.data;
}

export async function fetchPurPrs() {
  const { data } = await api.get<Envelope<PurPurchaseRequestDto[]>>("/api/pur/prs");
  return data.data;
}

export async function upsertPurPr(body: {
  id?: string;
  code: string;
  requestingUnit?: string;
  note?: string;
}) {
  const { data } = await api.post<Envelope<PurPurchaseRequestDto>>("/api/pur/prs", body);
  return data.data;
}

export async function fetchPurPrDetail(id: string) {
  const { data } = await api.get<Envelope<PurPrDetailDto>>(`/api/pur/prs/${id}`);
  return data.data;
}

export async function upsertPurPrLine(
  prId: string,
  body: {
    id?: string;
    productCode: string;
    productName: string;
    qty: number;
    unit?: string;
    note?: string;
  },
) {
  const { data } = await api.post<Envelope<PurPrLineDto>>(`/api/pur/prs/${prId}/lines`, body);
  return data.data;
}

export async function submitPurPr(id: string) {
  const { data } = await api.post<Envelope<PurPurchaseRequestDto>>(`/api/pur/prs/${id}/submit`, {});
  return data.data;
}

export async function approvePurPr(id: string, note?: string) {
  const { data } = await api.post<Envelope<PurPurchaseRequestDto>>(`/api/pur/prs/${id}/approve`, {
    note,
  });
  return data.data;
}

export async function rejectPurPr(id: string, note?: string) {
  const { data } = await api.post<Envelope<PurPurchaseRequestDto>>(`/api/pur/prs/${id}/reject`, {
    note,
  });
  return data.data;
}

export async function returnPurPr(id: string, note?: string) {
  const { data } = await api.post<Envelope<PurPurchaseRequestDto>>(`/api/pur/prs/${id}/return`, {
    note,
  });
  return data.data;
}

export async function createPoFromPr(
  prId: string,
  body: { code: string; vendorId: string; note?: string },
) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>(
    `/api/pur/prs/${prId}/create-po`,
    body,
  );
  return data.data;
}

export async function fetchPurPos() {
  const { data } = await api.get<Envelope<PurPurchaseOrderDto[]>>("/api/pur/pos");
  return data.data;
}

export async function upsertPurPo(body: {
  id?: string;
  code: string;
  vendorId: string;
  sourcePrId?: string | null;
  note?: string;
}) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>("/api/pur/pos", body);
  return data.data;
}

export async function fetchPurPoDetail(id: string) {
  const { data } = await api.get<Envelope<PurPoDetailDto>>(`/api/pur/pos/${id}`);
  return data.data;
}

export async function upsertPurPoLine(
  poId: string,
  body: {
    id?: string;
    productCode: string;
    productName: string;
    qty: number;
    unitPrice: number;
    unit?: string;
  },
) {
  const { data } = await api.post<Envelope<PurPoLineDto>>(`/api/pur/pos/${poId}/lines`, body);
  return data.data;
}

export async function submitPurPo(id: string) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>(`/api/pur/pos/${id}/submit`, {});
  return data.data;
}

export async function approvePurPo(id: string) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>(`/api/pur/pos/${id}/approve`, {});
  return data.data;
}

export async function sendPurPo(id: string) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>(`/api/pur/pos/${id}/send`, {});
  return data.data;
}

export async function revisePurPo(id: string) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>(`/api/pur/pos/${id}/revise`, {});
  return data.data;
}
export async function closePurPo(id: string) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>(`/api/pur/pos/${id}/close`, {});
  return data.data;
}
export async function cancelPurPo(id: string, reason: string) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>(`/api/pur/pos/${id}/cancel`, { reason });
  return data.data;
}
export async function printPurPo(id: string) {
  const { data } = await api.post<Envelope<PurPurchaseOrderDto>>(`/api/pur/pos/${id}/print`, {});
  return data.data;
}

/** UC_PUR_033 — tải PO CSV thật (kèm đóng dấu PrintedAt phía BE). */
export async function downloadPurPoCsv(id: string, filename: string) {
  const { data } = await api.get<Blob>(`/api/pur/pos/${id}/export.csv`, { responseType: "blob" });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}
