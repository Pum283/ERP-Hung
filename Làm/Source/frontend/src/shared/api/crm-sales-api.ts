import { api } from "@/shared/api/client";
import type { CrmQuoteDto } from "@/shared/api/crm-lead-api";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type { CrmQuoteDto };

export type CrmPriceListDto = {
  id: string; code: string; name: string; status: string; note?: string | null; itemCount: number;
};
export type CrmPriceListItemDto = {
  id: string; priceListId: string; itemCode: string; itemName: string; unitPrice: number;
};
export type CrmPriceListDetailDto = { priceList: CrmPriceListDto; items: CrmPriceListItemDto[] };

export type CrmQuoteLineDto = {
  id: string; quoteId: string; itemCode: string; itemName: string;
  quantity: number; unitPrice: number; lineAmount: number; lineNo: number;
};
export type CrmQuoteDetailDto = { quote: CrmQuoteDto; lines: CrmQuoteLineDto[] };

export type CrmSalesOrderDto = {
  id: string; code: string; quoteId?: string | null; quoteCode?: string | null;
  customerId?: string | null; customerName?: string | null; opportunityId?: string | null;
  ownerUserId?: string | null; ownerName?: string | null; orderDate: string; status: string;
  subTotal: number; discountAmount: number; totalAmount: number; paidAmount: number;
  stockHoldStatus: string; warehousePushStatus: string; cancelReason?: string | null;
  note?: string | null; lineCount: number; paymentCount: number;
};
export type CrmSalesOrderLineDto = {
  id: string; orderId: string; itemCode: string; itemName: string;
  quantity: number; unitPrice: number; lineAmount: number; lineNo: number;
};
export type CrmOrderPaymentDto = {
  id: string; orderId: string; code: string; paidAt: string; amount: number; method: string; note?: string | null;
};
export type CrmSalesOrderDetailDto = {
  order: CrmSalesOrderDto; lines: CrmSalesOrderLineDto[]; payments: CrmOrderPaymentDto[];
};

export async function fetchCrmPriceLists() {
  const { data } = await api.get<Envelope<CrmPriceListDto[]>>("/api/crm/price-lists");
  return data.data;
}
export async function upsertCrmPriceList(body: {
  id?: string | null; code: string; name: string; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<CrmPriceListDto>>("/api/crm/price-lists", body);
  return data.data;
}
export async function upsertCrmPriceListItem(priceListId: string, body: {
  itemCode: string; itemName: string; unitPrice: number;
}) {
  const { data } = await api.post<Envelope<CrmPriceListItemDto>>(
    `/api/crm/price-lists/${priceListId}/items`, body);
  return data.data;
}

export async function fetchCrmQuotes(params?: { status?: string }) {
  const { data } = await api.get<Envelope<CrmQuoteDto[]>>("/api/crm/quotes", { params });
  return data.data;
}
export async function fetchCrmQuoteDetail(id: string) {
  const { data } = await api.get<Envelope<CrmQuoteDetailDto>>(`/api/crm/quotes/${id}`);
  return data.data;
}
export async function upsertCrmQuote(body: {
  id?: string | null; opportunityId?: string | null; customerId?: string | null;
  priceListId?: string | null; validUntil?: string | null; discountPercent?: number; note?: string | null;
}) {
  const { data } = await api.post<Envelope<CrmQuoteDto>>("/api/crm/quotes", body);
  return data.data;
}
export async function upsertCrmQuoteLine(quoteId: string, body: {
  itemCode: string; itemName: string; quantity: number; unitPrice: number;
}) {
  const { data } = await api.post<Envelope<CrmQuoteLineDto>>(`/api/crm/quotes/${quoteId}/lines`, body);
  return data.data;
}
export async function applyCrmPriceList(quoteId: string, priceListId: string) {
  const { data } = await api.post<Envelope<CrmQuoteDto>>(
    `/api/crm/quotes/${quoteId}/apply-price-list/${priceListId}`);
  return data.data;
}
export async function requestCrmQuoteDiscount(quoteId: string, discountPercent: number, note?: string) {
  const { data } = await api.post<Envelope<CrmQuoteDto>>(
    `/api/crm/quotes/${quoteId}/request-discount`, { discountPercent, note });
  return data.data;
}
export async function decideCrmQuoteDiscount(quoteId: string, approved: boolean, note?: string) {
  const { data } = await api.post<Envelope<CrmQuoteDto>>(
    `/api/crm/quotes/${quoteId}/decide-discount`, { approved, note });
  return data.data;
}
export async function sendCrmQuote(quoteId: string, channel: "Email" | "Pdf") {
  const { data } = await api.post<Envelope<CrmQuoteDto>>(
    `/api/crm/quotes/${quoteId}/send`, { channel });
  return data.data;
}

/** UC_CRM_074 — tải nội dung báo giá text thật (tuỳ chọn đóng dấu Sent). */
export async function downloadCrmQuoteText(quoteId: string, filename: string, stamp = true) {
  const { data } = await api.get<Blob>(`/api/crm/quotes/${quoteId}/quote.txt`, {
    params: { stamp },
    responseType: "blob",
  });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}
export async function convertCrmQuoteToOrder(quoteId: string) {
  const { data } = await api.post<Envelope<CrmSalesOrderDto>>(
    `/api/crm/quotes/${quoteId}/convert-order`);
  return data.data;
}

export async function fetchCrmOrders(params?: { status?: string }) {
  const { data } = await api.get<Envelope<CrmSalesOrderDto[]>>("/api/crm/orders", { params });
  return data.data;
}
export async function fetchCrmOrderDetail(id: string) {
  const { data } = await api.get<Envelope<CrmSalesOrderDetailDto>>(`/api/crm/orders/${id}`);
  return data.data;
}
export async function setCrmOrderStatus(id: string, status: string) {
  const { data } = await api.post<Envelope<CrmSalesOrderDto>>(`/api/crm/orders/${id}/status`, { status });
  return data.data;
}
export async function holdCrmOrderStock(id: string) {
  const { data } = await api.post<Envelope<CrmSalesOrderDto>>(`/api/crm/orders/${id}/hold-stock`);
  return data.data;
}
export async function cancelCrmOrder(id: string, reason: string) {
  const { data } = await api.post<Envelope<CrmSalesOrderDto>>(`/api/crm/orders/${id}/cancel`, { reason });
  return data.data;
}
export async function addCrmOrderPayment(id: string, body: {
  amount: number; method: string; note?: string;
}) {
  const { data } = await api.post<Envelope<CrmOrderPaymentDto>>(`/api/crm/orders/${id}/payments`, body);
  return data.data;
}
export async function pushCrmOrderWarehouse(id: string) {
  const { data } = await api.post<Envelope<CrmSalesOrderDto>>(`/api/crm/orders/${id}/push-warehouse`);
  return data.data;
}
