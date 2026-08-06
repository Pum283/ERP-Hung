import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FinRevenueDocumentDto = {
  id: string;
  code: string;
  kind: string;
  sourceModule: string;
  sourceId?: string | null;
  sourceCode?: string | null;
  docDate: string;
  revenueAmount: number;
  taxAmount: number;
  cogsAmount: number;
  totalAmount: number;
  periodId?: string | null;
  periodCode?: string | null;
  debitAccountId?: string | null;
  debitAccountCode?: string | null;
  creditAccountId?: string | null;
  creditAccountCode?: string | null;
  finJournalId?: string | null;
  finJournalCode?: string | null;
  status: string;
  postedAt?: string | null;
  note?: string | null;
};

export type FinRevenueSummaryDto = {
  periodId?: string | null;
  periodCode?: string | null;
  posRevenue: number;
  posCount: number;
  orderRevenue: number;
  orderCount: number;
  arRevenue: number;
  arCount: number;
  cogsAmount: number;
  cogsCount: number;
  grossMargin: number;
};

export type FinRevenueRecognizeBody = {
  periodId?: string | null;
  debitAccountId?: string | null;
  creditAccountId?: string | null;
  note?: string | null;
};

export async function fetchFinRevenueDocuments(params?: {
  kind?: string;
  periodId?: string;
  status?: string;
}) {
  const { data } = await api.get<Envelope<FinRevenueDocumentDto[]>>("/api/fin/revenue/documents", { params });
  return data.data;
}

export async function fetchFinRevenueSummary(params?: { periodId?: string }) {
  const { data } = await api.get<Envelope<FinRevenueSummaryDto>>("/api/fin/revenue/summary", { params });
  return data.data;
}

export async function recognizeFinRevenueFromPos(saleId: string, body?: FinRevenueRecognizeBody) {
  const { data } = await api.post<Envelope<FinRevenueDocumentDto>>(
    `/api/fin/revenue/recognize-from-pos/${saleId}`, body ?? {});
  return data.data;
}

export async function recognizeFinRevenueFromOrder(orderId: string, body?: FinRevenueRecognizeBody) {
  const { data } = await api.post<Envelope<FinRevenueDocumentDto>>(
    `/api/fin/revenue/recognize-from-order/${orderId}`, body ?? {});
  return data.data;
}

export async function recognizeFinRevenueFromAr(arInvoiceId: string, body?: FinRevenueRecognizeBody) {
  const { data } = await api.post<Envelope<FinRevenueDocumentDto>>(
    `/api/fin/revenue/recognize-from-ar/${arInvoiceId}`, body ?? {});
  return data.data;
}

export async function recognizeFinCogs(invStockDocId: string, body?: FinRevenueRecognizeBody) {
  const { data } = await api.post<Envelope<FinRevenueDocumentDto>>(
    `/api/fin/revenue/recognize-cogs/${invStockDocId}`, body ?? {});
  return data.data;
}

export async function voidFinRevenueDocument(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinRevenueDocumentDto>>(
    `/api/fin/revenue/documents/${id}/void`, { note });
  return data.data;
}
