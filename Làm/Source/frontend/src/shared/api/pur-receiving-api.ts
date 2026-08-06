import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PurGrnDto = {
  id: string; code: string; poId: string; poCode?: string | null;
  vendorId: string; vendorName?: string | null; status: string;
  receivedAt: string; qualityNote?: string | null;
  inventoryPushStatus: string; note?: string | null; lineCount: number;
  totalReceivedQty: number; totalAcceptedQty: number; totalRejectedQty: number;
};
export type PurGrnLineDto = {
  id: string; grnId: string; poLineId?: string | null;
  productCode: string; productName: string;
  orderedQty: number; receivedQty: number; acceptedQty: number; rejectedQty: number;
  unit: string; unitPrice: number;
};
export type PurGrnDetailDto = { header: PurGrnDto; lines: PurGrnLineDto[] };

export type PurInvoiceDto = {
  id: string; code: string; vendorId: string; vendorName?: string | null;
  poId?: string | null; poCode?: string | null;
  invoiceNumber: string; invoiceDate: string; status: string;
  subTotal: number; taxAmount: number; totalAmount: number;
  matchStatus: string; matchNote?: string | null;
  apPushStatus: string; note?: string | null; lineCount: number;
};
export type PurInvoiceLineDto = {
  id: string; invoiceId: string; poLineId?: string | null; grnLineId?: string | null;
  productCode: string; productName: string; qty: number; unitPrice: number; lineAmount: number;
};
export type PurInvoiceDetailDto = { header: PurInvoiceDto; lines: PurInvoiceLineDto[] };

export async function fetchPurGrns(poId?: string) {
  const { data } = await api.get<Envelope<PurGrnDto[]>>("/api/pur/grns", { params: { poId } });
  return data.data;
}
export async function fetchPurGrnDetail(id: string) {
  const { data } = await api.get<Envelope<PurGrnDetailDto>>(`/api/pur/grns/${id}`);
  return data.data;
}
export async function createPurGrn(body: { poId: string; note?: string; qualityNote?: string }) {
  const { data } = await api.post<Envelope<PurGrnDto>>("/api/pur/grns", body);
  return data.data;
}
export async function updatePurGrnLine(grnId: string, body: {
  lineId: string; receivedQty: number; acceptedQty: number; rejectedQty: number;
}) {
  const { data } = await api.post<Envelope<PurGrnLineDto>>(`/api/pur/grns/${grnId}/lines`, body);
  return data.data;
}
export async function postPurGrn(id: string) {
  const { data } = await api.post<Envelope<PurGrnDto>>(`/api/pur/grns/${id}/post`);
  return data.data;
}
export async function pushPurGrnInventory(id: string) {
  const { data } = await api.post<Envelope<PurGrnDto>>(`/api/pur/grns/${id}/push-inventory`);
  return data.data;
}

export async function fetchPurInvoices(vendorId?: string) {
  const { data } = await api.get<Envelope<PurInvoiceDto[]>>("/api/pur/invoices", { params: { vendorId } });
  return data.data;
}
export async function fetchPurInvoiceDetail(id: string) {
  const { data } = await api.get<Envelope<PurInvoiceDetailDto>>(`/api/pur/invoices/${id}`);
  return data.data;
}
export async function createPurInvoice(body: {
  vendorId: string; poId?: string | null; invoiceNumber: string;
  invoiceDate?: string; taxAmount?: number; note?: string;
}) {
  const { data } = await api.post<Envelope<PurInvoiceDto>>("/api/pur/invoices", body);
  return data.data;
}
export async function matchPurInvoice(id: string) {
  const { data } = await api.post<Envelope<PurInvoiceDto>>(`/api/pur/invoices/${id}/match`);
  return data.data;
}
export async function pushPurInvoiceAp(id: string) {
  const { data } = await api.post<Envelope<PurInvoiceDto>>(`/api/pur/invoices/${id}/push-ap`);
  return data.data;
}
