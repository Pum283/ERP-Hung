import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PrtAccountDto = {
  id: string; code: string; email: string; displayName: string;
  customerCode?: string | null; customerName?: string | null; status: string;
  lastLoginAt?: string | null; orderCount: number; openAr: number;
};
export type PrtOrderDto = {
  id: string; accountId: string; accountEmail?: string | null; code: string; orderDate: string;
  status: string; totalAmount: number; shippingAddress?: string | null; note?: string | null; lineCount: number;
};
export type PrtOrderLineDto = {
  id: string; orderId: string; itemCode: string; itemName: string;
  quantity: number; unitPrice: number; lineAmount: number; lineNo: number;
};
export type PrtOrderDetailDto = { order: PrtOrderDto; lines: PrtOrderLineDto[] };
export type PrtArSummaryDto = { accountId: string; openAmount: number; openInvoiceCount: number; paidYtd: number };
export type PrtInvoiceDto = {
  id: string; accountId: string; code: string; invoiceDate: string; dueDate?: string | null;
  amount: number; paidAmount: number; openAmount: number; status: string;
};
export type PrtPaymentDto = {
  id: string; accountId: string; invoiceId?: string | null; invoiceCode?: string | null;
  code: string; paidAt: string; amount: number; method: string; note?: string | null;
};
export type PrtTicketDto = {
  id: string; accountId: string; accountEmail?: string | null; code: string; subject: string;
  description?: string | null; status: string; openedAt: string; closedAt?: string | null;
};
export type PrtLoginResultDto = { account: PrtAccountDto; token: string; message: string };

export async function fetchPrtAccounts(q?: string) {
  const { data } = await api.get<Envelope<PrtAccountDto[]>>("/api/prt/accounts", { params: { q } });
  return data.data;
}
export async function upsertPrtAccount(body: {
  id?: string | null; code?: string; email: string; displayName: string; password?: string;
  customerCode?: string | null; customerName?: string | null; status?: string;
}) {
  const { data } = await api.post<Envelope<PrtAccountDto>>("/api/prt/accounts", body);
  return data.data;
}
export async function registerPrtAccount(body: {
  email: string; displayName: string; password: string; customerCode?: string;
}) {
  const { data } = await api.post<Envelope<PrtAccountDto>>("/api/prt/accounts/register", body);
  return data.data;
}
export async function loginPrtAccount(body: { email: string; password: string }) {
  const { data } = await api.post<Envelope<PrtLoginResultDto>>("/api/prt/accounts/login", body);
  return data.data;
}
export const loginPrtStub = loginPrtAccount;

export async function forgotPrtPassword(email: string) {
  const { data } = await api.post<Envelope<PrtAccountDto>>("/api/prt/accounts/forgot-password", { email });
  return data.data;
}
export async function resetPrtPassword(body: { email: string; resetToken: string; newPassword: string }) {
  const { data } = await api.post<Envelope<PrtAccountDto>>("/api/prt/accounts/reset-password", body);
  return data.data;
}
export async function linkPrtCustomer(body: { accountId: string; customerCode: string; customerName?: string }) {
  const { data } = await api.post<Envelope<PrtAccountDto>>("/api/prt/accounts/link-customer", body);
  return data.data;
}
export async function fetchPrtOrders(accountId?: string) {
  const { data } = await api.get<Envelope<PrtOrderDto[]>>("/api/prt/orders", { params: { accountId } });
  return data.data;
}
export async function fetchPrtOrderDetail(id: string) {
  const { data } = await api.get<Envelope<PrtOrderDetailDto>>(`/api/prt/orders/${id}`);
  return data.data;
}
export async function upsertPrtOrder(body: {
  id?: string | null; accountId: string; code?: string; status?: string;
  shippingAddress?: string; note?: string;
  lines?: { itemCode: string; itemName: string; quantity: number; unitPrice: number }[];
}) {
  const { data } = await api.post<Envelope<PrtOrderDto>>("/api/prt/orders", body);
  return data.data;
}
export async function fetchPrtArSummary(accountId: string) {
  const { data } = await api.get<Envelope<PrtArSummaryDto>>(`/api/prt/ar/${accountId}/summary`);
  return data.data;
}
export async function fetchPrtInvoices(accountId: string, openOnly = false) {
  const { data } = await api.get<Envelope<PrtInvoiceDto[]>>(`/api/prt/ar/${accountId}/invoices`, {
    params: { openOnly },
  });
  return data.data;
}
export async function upsertPrtInvoice(body: {
  id?: string | null; accountId: string; amount: number; paidAmount?: number; dueDate?: string;
}) {
  const { data } = await api.post<Envelope<PrtInvoiceDto>>("/api/prt/ar/invoices", body);
  return data.data;
}
export async function fetchPrtPayments(accountId: string) {
  const { data } = await api.get<Envelope<PrtPaymentDto[]>>(`/api/prt/ar/${accountId}/payments`);
  return data.data;
}
export async function upsertPrtPayment(body: {
  accountId: string; invoiceId?: string | null; amount: number; method?: string; note?: string;
}) {
  const { data } = await api.post<Envelope<PrtPaymentDto>>("/api/prt/ar/payments", body);
  return data.data;
}
export async function fetchPrtTickets(accountId?: string) {
  const { data } = await api.get<Envelope<PrtTicketDto[]>>("/api/prt/tickets", { params: { accountId } });
  return data.data;
}
export async function upsertPrtTicket(body: {
  id?: string | null; accountId: string; subject: string; description?: string; status?: string;
}) {
  const { data } = await api.post<Envelope<PrtTicketDto>>("/api/prt/tickets", body);
  return data.data;
}

export type PrtPortalPackageDto = {
  id: string; planCode: string; name: string; featuresJson: string;
  features: Record<string, boolean>; isActive: boolean; note?: string | null;
};
export type PrtEnabledFeaturesDto = { planCode: string; enabledFeatures: string[] };

export async function fetchPrtPackages() {
  const { data } = await api.get<Envelope<PrtPortalPackageDto[]>>("/api/prt/packages");
  return data.data;
}
export async function upsertPrtPackage(body: {
  id?: string | null; planCode: string; name: string;
  features?: Record<string, boolean>; featuresJson?: string;
  isActive?: boolean; note?: string | null;
}) {
  const { data } = await api.post<Envelope<PrtPortalPackageDto>>("/api/prt/packages", body);
  return data.data;
}
export async function fetchPrtEnabledFeatures(planCode?: string) {
  const { data } = await api.get<Envelope<PrtEnabledFeaturesDto>>("/api/prt/packages/enabled", {
    params: { planCode },
  });
  return data.data;
}
