import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PosShiftDto = {
  id: string; code: string; storeId: string; storeName?: string | null;
  terminalId?: string | null; terminalName?: string | null;
  cashierUserId: string; cashierName?: string | null;
  openedAt: string; closedAt?: string | null;
  openingCash: number; closingCashCounted?: number | null;
  expectedCash?: number | null; variance?: number | null;
  status: string; reportPrintedAt?: string | null; note?: string | null;
  salesTotal: number; cashSalesTotal: number; saleCount: number; openSaleCount: number;
};
export type PosSaleDto = {
  id: string; code: string; shiftId: string; storeId: string; storeName?: string | null;
  terminalId?: string | null; status: string; areaName?: string | null;
  subTotal: number; taxAmount: number; discountAmount: number; totalAmount: number;
  paidAmount: number; returnedAmount: number;
  paidAt?: string | null; receiptPrintedAt?: string | null; note?: string | null; lineCount: number;
  discountSource: string;
  promotionId?: string | null;
  promotionCode?: string | null;
  voucherId?: string | null;
  appliedVoucherCode?: string | null;
  manualDiscountType?: string | null;
  manualDiscountValue: number;
  discountApprovalStatus: string;
  discountNote?: string | null;
};
export type PosSaleLineDto = {
  id: string; saleId: string; productId?: string | null; productCode: string; productName: string;
  quantity: number; unitPrice: number; taxRatePct: number; lineAmount: number; status: string; lineNo: number;
};
export type PosSalePaymentDto = {
  id: string; saleId: string; code: string; paidAt: string; amount: number; method: string; note?: string | null;
};
export type PosSaleDetailDto = {
  sale: PosSaleDto; lines: PosSaleLineDto[]; payments: PosSalePaymentDto[];
};
export type PosShiftDetailDto = { shift: PosShiftDto; sales: PosSaleDto[] };
export type PosReturnDto = {
  id: string; code: string; saleId: string; saleCode?: string | null; shiftId?: string | null;
  status: string; refundAmount: number; refundMethod: string; reason?: string | null;
  completedAt?: string | null; lineCount: number;
};
export type PosReturnLineDto = {
  id: string; returnId: string; saleLineId?: string | null; productCode: string; productName: string;
  quantity: number; lineAmount: number;
};
export type PosReturnDetailDto = { return: PosReturnDto; lines: PosReturnLineDto[] };

export async function fetchPosShifts(params?: { storeId?: string; status?: string }) {
  const { data } = await api.get<Envelope<PosShiftDto[]>>("/api/pos/shifts", { params });
  return data.data;
}
export async function fetchPosShiftDetail(id: string) {
  const { data } = await api.get<Envelope<PosShiftDetailDto>>(`/api/pos/shifts/${id}`);
  return data.data;
}
export async function openPosShift(body: {
  storeId: string; terminalId?: string | null; openingCash: number; note?: string;
}) {
  const { data } = await api.post<Envelope<PosShiftDto>>("/api/pos/shifts/open", body);
  return data.data;
}
export async function closePosShift(id: string, closingCashCounted: number, note?: string) {
  const { data } = await api.post<Envelope<PosShiftDto>>(`/api/pos/shifts/${id}/close`, {
    closingCashCounted, note,
  });
  return data.data;
}
export async function printPosShiftReport(id: string) {
  const { data } = await api.post<Envelope<PosShiftDto>>(`/api/pos/shifts/${id}/print-report`);
  return data.data;
}

export async function fetchPosSales(params?: { shiftId?: string; status?: string }) {
  const { data } = await api.get<Envelope<PosSaleDto[]>>("/api/pos/sales", { params });
  return data.data;
}
export async function fetchPosSaleDetail(id: string) {
  const { data } = await api.get<Envelope<PosSaleDetailDto>>(`/api/pos/sales/${id}`);
  return data.data;
}
export async function openPosSale(body: { shiftId: string; areaName?: string; note?: string }) {
  const { data } = await api.post<Envelope<PosSaleDto>>("/api/pos/sales/open", body);
  return data.data;
}
export async function upsertPosSaleLine(saleId: string, body: {
  productId?: string | null; productCode?: string; productName?: string;
  quantity: number; unitPrice?: number; taxRatePct?: number;
}) {
  const { data } = await api.post<Envelope<PosSaleLineDto>>(`/api/pos/sales/${saleId}/lines`, body);
  return data.data;
}
export async function holdPosSale(saleId: string, note?: string) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/hold`, { note });
  return data.data;
}
export async function resumePosSale(saleId: string) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/resume`);
  return data.data;
}
export async function cancelPosSaleLine(saleId: string, lineId: string) {
  const { data } = await api.post<Envelope<PosSaleLineDto>>(
    `/api/pos/sales/${saleId}/lines/${lineId}/cancel`);
  return data.data;
}
export async function cancelPosSale(saleId: string, note?: string) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/cancel`, { note });
  return data.data;
}
export async function payPosSale(saleId: string, method: string, amount: number, note?: string) {
  const { data } = await api.post<Envelope<PosSalePaymentDto>>(
    `/api/pos/sales/${saleId}/pay`, { method, amount, note });
  return data.data;
}
export async function printPosReceipt(saleId: string) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/print-receipt`);
  return data.data;
}

export async function fetchPosReturns(params?: { saleId?: string }) {
  const { data } = await api.get<Envelope<PosReturnDto[]>>("/api/pos/returns", { params });
  return data.data;
}
export async function fetchPosReturnDetail(id: string) {
  const { data } = await api.get<Envelope<PosReturnDetailDto>>(`/api/pos/returns/${id}`);
  return data.data;
}
export async function createPosReturn(saleId: string, reason?: string) {
  const { data } = await api.post<Envelope<PosReturnDto>>("/api/pos/returns", { saleId, reason });
  return data.data;
}
export async function addPosReturnLine(returnId: string, saleLineId: string, quantity: number) {
  const { data } = await api.post<Envelope<PosReturnLineDto>>(
    `/api/pos/returns/${returnId}/lines`, { saleLineId, quantity });
  return data.data;
}
export async function completePosReturn(returnId: string, refundMethod: string, reason?: string) {
  const { data } = await api.post<Envelope<PosReturnDto>>(
    `/api/pos/returns/${returnId}/complete`, { refundMethod, reason });
  return data.data;
}
