import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FinArInvoiceDto = {
  id: string;
  code: string;
  customerId: string;
  customerCode?: string | null;
  customerName?: string | null;
  customerInvoiceNo?: string | null;
  invoiceDate: string;
  dueDate: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  receivedAmount: number;
  openAmount: number;
  status: string;
  creditLimitWarned: boolean;
  periodId?: string | null;
  periodCode?: string | null;
  finJournalCode?: string | null;
  note?: string | null;
};

export type FinArCustomerBalanceDto = {
  customerId: string;
  customerCode: string;
  customerName: string;
  openInvoiceCount: number;
  totalOpen: number;
  overdueAmount: number;
  notDueAmount: number;
  creditLimit?: number | null;
  creditUsedPct?: number | null;
  creditStatus: string;
};

export type FinArCreditLimitDto = {
  id: string;
  customerId: string;
  customerCode?: string | null;
  customerName?: string | null;
  creditLimit: number;
  warningPercent: number;
  isActive: boolean;
  note?: string | null;
  openBalance: number;
  creditStatus: string;
};

export type FinArReceiptDto = {
  id: string;
  code: string;
  customerId: string;
  customerCode?: string | null;
  customerName?: string | null;
  receiptDate: string;
  amount: number;
  payMethod: string;
  status: string;
  cashVoucherId?: string | null;
  bankVoucherId?: string | null;
  note?: string | null;
  allocations: { arInvoiceId: string; invoiceCode?: string | null; amount: number }[];
};

export type FinArAgingDto = {
  asOf: string;
  buckets: { bucket: string; amount: number; invoiceCount: number }[];
  rows: {
    customerId: string;
    customerCode: string;
    customerName: string;
    current: number;
    d1To30: number;
    d31To60: number;
    d61To90: number;
    over90: number;
    total: number;
  }[];
};

export async function fetchFinArInvoices(params?: { customerId?: string; status?: string }) {
  const { data } = await api.get<Envelope<FinArInvoiceDto[]>>("/api/fin/ar-invoices", { params });
  return data.data;
}

export async function upsertFinArInvoice(body: {
  customerId: string;
  customerInvoiceNo?: string | null;
  invoiceDate: string;
  dueDate: string;
  subTotal: number;
  taxAmount: number;
  periodId?: string | null;
  arAccountId?: string | null;
  revenueAccountId?: string | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinArInvoiceDto>>("/api/fin/ar-invoices", body);
  return data.data;
}

export async function postFinArInvoice(id: string) {
  const { data } = await api.post<Envelope<FinArInvoiceDto>>(`/api/fin/ar-invoices/${id}/post`);
  return data.data;
}

export async function voidFinArInvoice(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinArInvoiceDto>>(`/api/fin/ar-invoices/${id}/void`, { note });
  return data.data;
}

export async function fetchFinArCustomerBalances() {
  const { data } = await api.get<Envelope<FinArCustomerBalanceDto[]>>("/api/fin/ar-customer-balances");
  return data.data;
}

export async function fetchFinArCreditLimits() {
  const { data } = await api.get<Envelope<FinArCreditLimitDto[]>>("/api/fin/ar-credit-limits");
  return data.data;
}

export async function fetchFinArCreditAlerts() {
  const { data } = await api.get<Envelope<FinArCreditLimitDto[]>>("/api/fin/ar-credit-limits/alerts");
  return data.data;
}

export async function upsertFinArCreditLimit(body: {
  customerId: string;
  creditLimit: number;
  warningPercent?: number;
  isActive?: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinArCreditLimitDto>>("/api/fin/ar-credit-limits", body);
  return data.data;
}

export async function fetchFinArReceipts(params?: { customerId?: string }) {
  const { data } = await api.get<Envelope<FinArReceiptDto[]>>("/api/fin/ar-receipts", { params });
  return data.data;
}

export async function upsertFinArReceipt(body: {
  customerId: string;
  receiptDate: string;
  payMethod: string;
  cashFundId?: string | null;
  bankAccountId?: string | null;
  periodId?: string | null;
  note?: string | null;
  allocations: { arInvoiceId: string; amount: number }[];
}) {
  const { data } = await api.post<Envelope<FinArReceiptDto>>("/api/fin/ar-receipts", body);
  return data.data;
}

export async function postFinArReceipt(id: string) {
  const { data } = await api.post<Envelope<FinArReceiptDto>>(`/api/fin/ar-receipts/${id}/post`);
  return data.data;
}

export async function fetchFinArAging() {
  const { data } = await api.get<Envelope<FinArAgingDto>>("/api/fin/ar-aging");
  return data.data;
}
