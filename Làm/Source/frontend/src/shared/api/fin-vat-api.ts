import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FinVatCalcResult = {
  taxableAmount: number;
  ratePercent: number;
  taxAmount: number;
  totalAmount: number;
  taxId?: string | null;
  taxCode?: string | null;
};

export type FinVatDocumentDto = {
  id: string;
  code: string;
  direction: string;
  taxId?: string | null;
  taxCode?: string | null;
  ratePercent: number;
  invoiceNo: string;
  invoiceSeries?: string | null;
  invoiceDate: string;
  partnerCode?: string | null;
  partnerName?: string | null;
  partnerTaxCode?: string | null;
  taxableAmount: number;
  taxAmount: number;
  totalAmount: number;
  periodId?: string | null;
  periodCode?: string | null;
  arInvoiceId?: string | null;
  apInvoiceId?: string | null;
  status: string;
  postedAt?: string | null;
  note?: string | null;
};

export type FinVatSummaryDto = {
  from?: string | null;
  to?: string | null;
  periodId?: string | null;
  periodCode?: string | null;
  outputTaxable: number;
  outputTax: number;
  outputCount: number;
  inputTaxable: number;
  inputTax: number;
  inputCount: number;
  netVatPayable: number;
};

export async function calculateFinVat(body: {
  taxableAmount: number;
  taxId?: string | null;
  ratePercent?: number | null;
}) {
  const { data } = await api.post<Envelope<FinVatCalcResult>>("/api/fin/vat/calculate", body);
  return data.data;
}

export async function fetchFinVatDocuments(params?: {
  direction?: string;
  periodId?: string;
  status?: string;
}) {
  const { data } = await api.get<Envelope<FinVatDocumentDto[]>>("/api/fin/vat/documents", { params });
  return data.data;
}

export async function upsertFinVatDocument(body: {
  id?: string | null;
  direction: string;
  taxId?: string | null;
  ratePercent?: number | null;
  invoiceNo: string;
  invoiceSeries?: string | null;
  invoiceDate: string;
  partnerCode?: string | null;
  partnerName?: string | null;
  partnerTaxCode?: string | null;
  taxableAmount: number;
  periodId?: string | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinVatDocumentDto>>("/api/fin/vat/documents", body);
  return data.data;
}

export async function postFinVatDocument(id: string) {
  const { data } = await api.post<Envelope<FinVatDocumentDto>>(`/api/fin/vat/documents/${id}/post`);
  return data.data;
}

export async function voidFinVatDocument(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinVatDocumentDto>>(`/api/fin/vat/documents/${id}/void`, { note });
  return data.data;
}

export async function fetchFinVatSummary(params?: { periodId?: string }) {
  const { data } = await api.get<Envelope<FinVatSummaryDto>>("/api/fin/vat/summary", { params });
  return data.data;
}
