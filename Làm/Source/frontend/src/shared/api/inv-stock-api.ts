import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type InvBalanceDto = {
  id: string; warehouseId: string; warehouseName?: string | null;
  skuId: string; skuCode: string; skuName: string;
  lotCode?: string | null; expiryDate?: string | null;
  qtyOnHand: number; qtyReserved: number; qtyInTransit: number; qtyAvailable: number;
};
export type InvStockDocDto = {
  id: string; code: string; docType: string; sourceType: string;
  warehouseId: string; warehouseName?: string | null; status: string;
  refModule?: string | null; refId?: string | null; refCode?: string | null;
  postedAt?: string | null; note?: string | null; lineCount: number;
};
export type InvStockDocLineDto = {
  id: string; docId: string; skuId: string; skuCode: string; skuName: string;
  qty: number; lotCode?: string | null; expiryDate?: string | null; unitCost: number;
};
export type InvStockDocDetailDto = { header: InvStockDocDto; lines: InvStockDocLineDto[] };

export type InvTransferDto = {
  id: string; code: string; fromWarehouseId: string; fromWarehouseName?: string | null;
  toWarehouseId: string; toWarehouseName?: string | null; status: string;
  shippedAt?: string | null; receivedAt?: string | null; note?: string | null; lineCount: number;
};
export type InvTransferLineDto = {
  id: string; transferId: string; skuId: string; skuCode: string; skuName: string;
  qty: number; lotCode?: string | null; expiryDate?: string | null;
};
export type InvTransferDetailDto = { header: InvTransferDto; lines: InvTransferLineDto[] };

export type InvStocktakeDto = {
  id: string; code: string; warehouseId: string; warehouseName?: string | null;
  status: string; countedAt?: string | null; postedAt?: string | null; note?: string | null;
  lineCount: number; varianceCount: number;
};
export type InvStocktakeLineDto = {
  id: string; stocktakeId: string; skuId: string; skuCode: string; skuName: string;
  lotCode?: string | null; systemQty: number; countedQty?: number | null; varianceQty: number;
};
export type InvStocktakeDetailDto = { header: InvStocktakeDto; lines: InvStocktakeLineDto[] };

export async function fetchInvBalances(warehouseId?: string) {
  const { data } = await api.get<Envelope<InvBalanceDto[]>>("/api/inv/balances", { params: { warehouseId } });
  return data.data;
}
export async function fetchInvDocs(docType?: string) {
  const { data } = await api.get<Envelope<InvStockDocDto[]>>("/api/inv/docs", { params: { docType } });
  return data.data;
}
export async function fetchInvDocDetail(id: string) {
  const { data } = await api.get<Envelope<InvStockDocDetailDto>>(`/api/inv/docs/${id}`);
  return data.data;
}
export async function createInvDoc(body: {
  docType: string; sourceType: string; warehouseId: string; note?: string;
}) {
  const { data } = await api.post<Envelope<InvStockDocDto>>("/api/inv/docs", body);
  return data.data;
}
export async function upsertInvDocLine(docId: string, body: {
  skuId: string; qty: number; lotCode?: string; expiryDate?: string; unitCost?: number;
}) {
  const { data } = await api.post<Envelope<InvStockDocLineDto>>(`/api/inv/docs/${docId}/lines`, body);
  return data.data;
}
export async function postInvDoc(id: string) {
  const { data } = await api.post<Envelope<InvStockDocDto>>(`/api/inv/docs/${id}/post`);
  return data.data;
}
export async function suggestInvLots(body: { warehouseId: string; skuId: string; qty: number }) {
  const { data } = await api.post<Envelope<{ skuId: string; skuCode: string; lotCode?: string | null; expiryDate?: string | null; qtyAvailable: number; qtyPick: number }[]>>(
    "/api/inv/docs/suggest-lots", body,
  );
  return data.data;
}

export type InvReservationDto = {
  id: string; code: string; warehouseId: string; warehouseName?: string | null; status: string;
  refModule?: string | null; refId?: string | null; refCode?: string | null; note?: string | null;
  activatedAt?: string | null; releasedAt?: string | null; lineCount: number;
};
export type InvReservationDetailDto = {
  header: InvReservationDto;
  lines: { id: string; reservationId: string; skuId: string; skuCode: string; skuName: string; qty: number; lotCode?: string | null; expiryDate?: string | null }[];
};

export async function fetchInvReservations(status?: string) {
  const { data } = await api.get<Envelope<InvReservationDto[]>>("/api/inv/reservations", { params: { status } });
  return data.data;
}
export async function createInvReservation(body: {
  warehouseId: string; refModule?: string; refCode?: string; note?: string; activate: boolean;
  lines: { skuId: string; qty: number; lotCode?: string; expiryDate?: string }[];
}) {
  const { data } = await api.post<Envelope<InvReservationDetailDto>>("/api/inv/reservations", body);
  return data.data;
}
export async function activateInvReservation(id: string) {
  const { data } = await api.post<Envelope<InvReservationDetailDto>>(`/api/inv/reservations/${id}/activate`);
  return data.data;
}
export async function releaseInvReservation(id: string) {
  const { data } = await api.post<Envelope<InvReservationDetailDto>>(`/api/inv/reservations/${id}/release`);
  return data.data;
}
export async function fetchInvAtpAlerts(warehouseId?: string) {
  const { data } = await api.get<Envelope<{
    warehouseId: string; warehouseName?: string | null; skuId: string; skuCode: string; skuName: string;
    lotCode?: string | null; expiryDate?: string | null;
    qtyOnHand: number; qtyReserved: number; qtyAvailable: number; alertType: string;
  }[]>>("/api/inv/atp-alerts", { params: { warehouseId } });
  return data.data;
}

export async function fetchInvTransfers(status?: string) {
  const { data } = await api.get<Envelope<InvTransferDto[]>>("/api/inv/transfers", { params: { status } });
  return data.data;
}
export async function fetchInvTransferDetail(id: string) {
  const { data } = await api.get<Envelope<InvTransferDetailDto>>(`/api/inv/transfers/${id}`);
  return data.data;
}
export async function createInvTransfer(body: {
  fromWarehouseId: string; toWarehouseId: string; note?: string;
}) {
  const { data } = await api.post<Envelope<InvTransferDto>>("/api/inv/transfers", body);
  return data.data;
}
export async function upsertInvTransferLine(transferId: string, body: {
  skuId: string; qty: number; lotCode?: string; expiryDate?: string;
}) {
  const { data } = await api.post<Envelope<InvTransferLineDto>>(
    `/api/inv/transfers/${transferId}/lines`, body);
  return data.data;
}
export async function shipInvTransfer(id: string) {
  const { data } = await api.post<Envelope<InvTransferDto>>(`/api/inv/transfers/${id}/ship`);
  return data.data;
}
export async function receiveInvTransfer(id: string) {
  const { data } = await api.post<Envelope<InvTransferDto>>(`/api/inv/transfers/${id}/receive`);
  return data.data;
}

export async function fetchInvStocktakes() {
  const { data } = await api.get<Envelope<InvStocktakeDto[]>>("/api/inv/stocktakes");
  return data.data;
}
export async function fetchInvStocktakeDetail(id: string) {
  const { data } = await api.get<Envelope<InvStocktakeDetailDto>>(`/api/inv/stocktakes/${id}`);
  return data.data;
}
export async function createInvStocktake(body: { warehouseId: string; note?: string }) {
  const { data } = await api.post<Envelope<InvStocktakeDto>>("/api/inv/stocktakes", body);
  return data.data;
}
export async function countInvStocktakeLine(stocktakeId: string, lineId: string, countedQty: number) {
  const { data } = await api.post<Envelope<InvStocktakeLineDto>>(
    `/api/inv/stocktakes/${stocktakeId}/count`, { lineId, countedQty });
  return data.data;
}
export async function reviewInvStocktake(id: string) {
  const { data } = await api.post<Envelope<InvStocktakeDto>>(`/api/inv/stocktakes/${id}/review`);
  return data.data;
}
export async function postInvStocktake(id: string) {
  const { data } = await api.post<Envelope<InvStocktakeDto>>(`/api/inv/stocktakes/${id}/post`);
  return data.data;
}
