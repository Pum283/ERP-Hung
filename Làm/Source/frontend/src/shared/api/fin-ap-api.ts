import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FinApInvoiceDto = {
  id: string;
  code: string;
  vendorId: string;
  vendorCode?: string | null;
  vendorName?: string | null;
  vendorInvoiceNo?: string | null;
  purVendorInvoiceId?: string | null;
  invoiceDate: string;
  dueDate: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  openAmount: number;
  status: string;
  periodId?: string | null;
  periodCode?: string | null;
  finJournalId?: string | null;
  finJournalCode?: string | null;
  postedAt?: string | null;
  note?: string | null;
};

export type FinApVendorBalanceDto = {
  vendorId: string;
  vendorCode: string;
  vendorName: string;
  openInvoiceCount: number;
  totalOpen: number;
  overdueAmount: number;
  notDueAmount: number;
};

export type FinApPaymentRequestDto = {
  id: string;
  code: string;
  vendorId: string;
  vendorCode?: string | null;
  vendorName?: string | null;
  requestDate: string;
  requestAmount: number;
  payMethod: string;
  cashFundId?: string | null;
  bankAccountId?: string | null;
  status: string;
  paymentId?: string | null;
  paymentCode?: string | null;
  approvedAt?: string | null;
  note?: string | null;
  lines: { apInvoiceId: string; invoiceCode?: string | null; amount: number; invoiceOpen: number }[];
};

export type FinApPaymentDto = {
  id: string;
  code: string;
  vendorId: string;
  vendorCode?: string | null;
  vendorName?: string | null;
  payDate: string;
  amount: number;
  payMethod: string;
  status: string;
  cashVoucherId?: string | null;
  bankVoucherId?: string | null;
  paymentRequestId?: string | null;
  note?: string | null;
  allocations: { apInvoiceId: string; invoiceCode?: string | null; amount: number }[];
};

export type FinApAgingDto = {
  asOf: string;
  buckets: { bucket: string; amount: number; invoiceCount: number }[];
  rows: {
    vendorId: string;
    vendorCode: string;
    vendorName: string;
    current: number;
    d1To30: number;
    d31To60: number;
    d61To90: number;
    over90: number;
    total: number;
  }[];
};

export async function fetchFinApInvoices(params?: { vendorId?: string; status?: string }) {
  const { data } = await api.get<Envelope<FinApInvoiceDto[]>>("/api/fin/ap-invoices", { params });
  return data.data;
}

export async function upsertFinApInvoice(body: {
  id?: string | null;
  vendorId: string;
  vendorInvoiceNo?: string | null;
  invoiceDate: string;
  dueDate: string;
  subTotal: number;
  taxAmount: number;
  periodId?: string | null;
  apAccountId?: string | null;
  expenseAccountId?: string | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinApInvoiceDto>>("/api/fin/ap-invoices", body);
  return data.data;
}

export async function postFinApInvoice(id: string) {
  const { data } = await api.post<Envelope<FinApInvoiceDto>>(`/api/fin/ap-invoices/${id}/post`);
  return data.data;
}

export async function voidFinApInvoice(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinApInvoiceDto>>(`/api/fin/ap-invoices/${id}/void`, { note });
  return data.data;
}

export async function fetchFinApVendorBalances() {
  const { data } = await api.get<Envelope<FinApVendorBalanceDto[]>>("/api/fin/ap-vendor-balances");
  return data.data;
}

export async function fetchFinApPaymentRequests(params?: { vendorId?: string }) {
  const { data } = await api.get<Envelope<FinApPaymentRequestDto[]>>("/api/fin/ap-payment-requests", { params });
  return data.data;
}

export async function upsertFinApPaymentRequest(body: {
  vendorId: string;
  payMethod: string;
  cashFundId?: string | null;
  bankAccountId?: string | null;
  note?: string | null;
  lines: { apInvoiceId: string; amount: number }[];
}) {
  const { data } = await api.post<Envelope<FinApPaymentRequestDto>>("/api/fin/ap-payment-requests", body);
  return data.data;
}

export async function submitFinApPaymentRequest(id: string) {
  const { data } = await api.post<Envelope<FinApPaymentRequestDto>>(`/api/fin/ap-payment-requests/${id}/submit`);
  return data.data;
}

export async function approveFinApPaymentRequest(id: string) {
  const { data } = await api.post<Envelope<FinApPaymentRequestDto>>(`/api/fin/ap-payment-requests/${id}/approve`);
  return data.data;
}

export async function rejectFinApPaymentRequest(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinApPaymentRequestDto>>(`/api/fin/ap-payment-requests/${id}/reject`, { note });
  return data.data;
}

export async function payFinApPaymentRequest(id: string) {
  const { data } = await api.post<Envelope<FinApPaymentDto>>(`/api/fin/ap-payment-requests/${id}/pay`);
  return data.data;
}

export async function fetchFinApPayments(params?: { vendorId?: string }) {
  const { data } = await api.get<Envelope<FinApPaymentDto[]>>("/api/fin/ap-payments", { params });
  return data.data;
}

export async function fetchFinApAging() {
  const { data } = await api.get<Envelope<FinApAgingDto>>("/api/fin/ap-aging");
  return data.data;
}
