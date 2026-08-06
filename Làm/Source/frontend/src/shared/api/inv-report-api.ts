import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type InvStockValueRowDto = {
  skuId: string; skuCode: string; skuName: string; warehouseId: string; warehouseName?: string | null;
  qtyOnHand: number; standardCost: number; stockValue: number;
};
export type InvMovementPeriodRowDto = {
  skuId: string; skuCode: string; skuName: string;
  qtyIn: number; qtyOut: number; qtyNet: number; valueIn: number; valueOut: number;
};
export type InvSkuCardLineDto = {
  at: string; docCode: string; docType: string; sourceType: string;
  warehouseName: string; qtySigned: number; unitCost: number; amount: number; refCode?: string | null;
};
export type InvMinMaxAlertRowDto = {
  skuId: string; skuCode: string; skuName: string; warehouseId: string; warehouseName?: string | null;
  qtyOnHand: number; minQty?: number | null; maxQty?: number | null; alertType: string;
};
export type InvStocktakeReportRowDto = {
  stocktakeId: string; stocktakeCode: string; warehouseName?: string | null; status: string;
  skuCode: string; skuName: string; systemQty: number; countedQty?: number | null; varianceQty: number;
};
export type InvDashboardDto = {
  skuCount: number; warehouseCount: number; totalQtyOnHand: number; totalStockValue: number;
  belowMinCount: number; aboveMaxCount: number; openStocktakeCount: number;
  topAlerts: InvMinMaxAlertRowDto[];
  nearExpiryCount: number; expiredCount: number; insufficientAtpCount: number;
};
export type InvNearExpiryRowDto = {
  warehouseId: string; warehouseName?: string | null; skuId: string; skuCode: string; skuName: string;
  lotCode?: string | null; expiryDate?: string | null;
  qtyOnHand: number; qtyReserved: number; qtyAvailable: number; daysToExpiry: number; alertType: string;
};

export async function fetchInvStockValue(params?: { warehouseId?: string }) {
  const { data } = await api.get<Envelope<InvStockValueRowDto[]>>("/api/inv/reports/stock-value", { params });
  return data.data;
}
export async function fetchInvMovement(params: { from: string; to: string; warehouseId?: string }) {
  const { data } = await api.get<Envelope<InvMovementPeriodRowDto[]>>("/api/inv/reports/movement", { params });
  return data.data;
}
export async function fetchInvSkuCard(params: {
  skuId: string; warehouseId?: string; from?: string; to?: string;
}) {
  const { data } = await api.get<Envelope<InvSkuCardLineDto[]>>("/api/inv/reports/sku-card", { params });
  return data.data;
}
export async function fetchInvMinMax(params?: { warehouseId?: string }) {
  const { data } = await api.get<Envelope<InvMinMaxAlertRowDto[]>>("/api/inv/reports/min-max", { params });
  return data.data;
}
export async function fetchInvStocktakeReport(params?: { stocktakeId?: string; warehouseId?: string }) {
  const { data } = await api.get<Envelope<InvStocktakeReportRowDto[]>>("/api/inv/reports/stocktake", { params });
  return data.data;
}
export async function fetchInvDashboard(params?: { warehouseId?: string }) {
  const { data } = await api.get<Envelope<InvDashboardDto>>("/api/inv/reports/dashboard", { params });
  return data.data;
}
export async function fetchInvNearExpiry(params?: { warehouseId?: string; withinDays?: number }) {
  const { data } = await api.get<Envelope<InvNearExpiryRowDto[]>>("/api/inv/reports/near-expiry", { params });
  return data.data;
}
export async function downloadInvReportCsv(params: {
  report: string; warehouseId?: string; skuId?: string; stocktakeId?: string; from?: string; to?: string; withinDays?: number;
}) {
  const { data } = await api.get<Blob>("/api/inv/reports/export.csv", { params, responseType: "blob" });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = `inv-${params.report}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
