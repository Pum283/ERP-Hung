import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type MfgItemDto = {
  id: string; code: string; name: string; itemType: string; unit: string;
  standardCost: number; status: string; note?: string | null;
};
export type MfgWorkshopDto = {
  id: string; code: string; name: string; workshopType: string; status: string; note?: string | null;
};
export type MfgBomDto = {
  id: string; code: string; parentItemId: string; parentItemCode?: string | null; parentItemName?: string | null;
  version: string; status: string; note?: string | null; lineCount: number;
};
export type MfgBomLineDto = {
  id: string; bomId: string; componentItemId: string; componentCode?: string | null; componentName?: string | null;
  componentType?: string | null; qty: number; unit: string; level: number; note?: string | null;
};
export type MfgBomDetailDto = { bom: MfgBomDto; lines: MfgBomLineDto[] };
export type MfgPlanDto = {
  id: string; code: string; sourceOrderCode: string; status: string; note?: string | null; lineCount: number;
};
export type MfgPlanLineDto = {
  id: string; planId: string; itemId: string; itemCode?: string | null; itemName?: string | null;
  qty: number; workshopId?: string | null; workshopName?: string | null; note?: string | null;
};
export type MfgPlanDetailDto = { plan: MfgPlanDto; lines: MfgPlanLineDto[] };
export type MfgWorkOrderDto = {
  id: string; code: string; itemId: string; itemCode?: string | null; itemName?: string | null;
  qty: number; workshopId?: string | null; workshopName?: string | null;
  bomId?: string | null; bomCode?: string | null; planId?: string | null;
  status: string; note?: string | null; qtyIssuedMaterial: number; qtyFgReceived: number; qtyScrap: number;
  approvedAt?: string | null; releasedAt?: string | null; printedAt?: string | null;
  pausedAt?: string | null; closedAt?: string | null; cancelReason?: string | null;
};
export type MfgMaterialIssueDto = {
  id: string; workOrderId: string; itemId: string; itemCode?: string | null; itemName?: string | null;
  qty: number; unitCost: number; amount: number; unit: string; issuedAt: string; note?: string | null;
};
export type MfgCostSheetDto = {
  id: string; code: string; workOrderId: string; workOrderCode?: string | null; status: string;
  materialCost: number; laborCost: number; overheadCost: number; totalCost: number;
  goodQty: number; unitCost: number;
  invSkuId?: string | null; invSkuCode?: string | null;
  finJournalId?: string | null; finJournalCode?: string | null;
  calculatedAt?: string | null; pushedAt?: string | null; note?: string | null;
  lines: {
    id: string; materialIssueId?: string | null; itemId: string;
    itemCode?: string | null; itemName?: string | null;
    source: string; qty: number; unitCost: number; amount: number; note?: string | null;
  }[];
};
export type MfgFgReceiptDto = {
  id: string; workOrderId: string; itemId: string; itemCode?: string | null; itemName?: string | null;
  qty: number; unit: string; receivedAt: string; note?: string | null;
};
export type MfgScrapDto = {
  id: string; workOrderId: string; itemId?: string | null; itemCode?: string | null; itemName?: string | null;
  qty: number; unit: string; scrapType: string; recordedAt: string; note?: string | null;
};
export type MfgWorkOrderDetailDto = {
  order: MfgWorkOrderDto;
  issues: MfgMaterialIssueDto[];
  receipts: MfgFgReceiptDto[];
  scraps: MfgScrapDto[];
  requiredMaterials: MfgBomLineDto[];
  costSheet?: MfgCostSheetDto | null;
};

export async function fetchMfgItems(type?: string, q?: string) {
  const { data } = await api.get<Envelope<MfgItemDto[]>>("/api/mfg/items", { params: { type, q } });
  return data.data;
}
export async function upsertMfgItem(body: {
  id?: string | null; code: string; name: string; itemType: string;
  unit?: string; standardCost?: number; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<MfgItemDto>>("/api/mfg/items", body);
  return data.data;
}
export async function fetchMfgWorkshops() {
  const { data } = await api.get<Envelope<MfgWorkshopDto[]>>("/api/mfg/workshops");
  return data.data;
}
export async function upsertMfgWorkshop(body: {
  id?: string | null; code: string; name: string; workshopType?: string; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<MfgWorkshopDto>>("/api/mfg/workshops", body);
  return data.data;
}
export async function fetchMfgBoms() {
  const { data } = await api.get<Envelope<MfgBomDto[]>>("/api/mfg/boms");
  return data.data;
}
export async function fetchMfgBomDetail(id: string) {
  const { data } = await api.get<Envelope<MfgBomDetailDto>>(`/api/mfg/boms/${id}`);
  return data.data;
}
export async function upsertMfgBom(body: {
  id?: string | null; code?: string; parentItemId: string; version: string; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<MfgBomDto>>("/api/mfg/boms", body);
  return data.data;
}
export async function upsertMfgBomLine(bomId: string, body: {
  id?: string | null; componentItemId: string; qty: number; unit?: string; level?: number; note?: string | null;
}) {
  const { data } = await api.post<Envelope<MfgBomLineDto>>(`/api/mfg/boms/${bomId}/lines`, body);
  return data.data;
}
export async function activateMfgBom(id: string) {
  const { data } = await api.post<Envelope<MfgBomDto>>(`/api/mfg/boms/${id}/activate`);
  return data.data;
}
export async function fetchMfgPlans() {
  const { data } = await api.get<Envelope<MfgPlanDto[]>>("/api/mfg/plans");
  return data.data;
}
export async function fetchMfgPlanDetail(id: string) {
  const { data } = await api.get<Envelope<MfgPlanDetailDto>>(`/api/mfg/plans/${id}`);
  return data.data;
}
export async function upsertMfgPlan(body: { id?: string | null; code?: string; sourceOrderCode: string; note?: string | null }) {
  const { data } = await api.post<Envelope<MfgPlanDto>>("/api/mfg/plans", body);
  return data.data;
}
export async function upsertMfgPlanLine(planId: string, body: {
  id?: string | null; itemId: string; qty: number; workshopId?: string | null; note?: string | null;
}) {
  const { data } = await api.post<Envelope<MfgPlanLineDto>>(`/api/mfg/plans/${planId}/lines`, body);
  return data.data;
}
export async function confirmMfgPlan(id: string) {
  const { data } = await api.post<Envelope<MfgPlanDto>>(`/api/mfg/plans/${id}/confirm`);
  return data.data;
}
export async function fetchMfgWorkOrders(q?: string) {
  const { data } = await api.get<Envelope<MfgWorkOrderDto[]>>("/api/mfg/work-orders", { params: { q } });
  return data.data;
}
export async function fetchMfgWorkOrderDetail(id: string) {
  const { data } = await api.get<Envelope<MfgWorkOrderDetailDto>>(`/api/mfg/work-orders/${id}`);
  return data.data;
}
export async function upsertMfgWorkOrder(body: {
  id?: string | null; code?: string; itemId: string; qty: number;
  workshopId?: string | null; bomId?: string | null; planId?: string | null; note?: string | null;
}) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>("/api/mfg/work-orders", body);
  return data.data;
}
export async function approveMfgWorkOrder(id: string) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/approve`);
  return data.data;
}
export async function releaseMfgWorkOrder(id: string) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/release`);
  return data.data;
}
export async function issueMfgMaterials(id: string, body: { itemId: string; qty: number; unit?: string; note?: string | null }) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/issue-materials`, body);
  return data.data;
}
export async function receiveMfgFg(id: string, body: { qty: number; note?: string | null }) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/receive-fg`, body);
  return data.data;
}
export async function recordMfgScrap(id: string, body: {
  itemId?: string | null; qty: number; unit?: string; scrapType: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/scraps`, body);
  return data.data;
}
export async function pauseMfgWorkOrder(id: string, note?: string) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/pause`, { note });
  return data.data;
}
export async function resumeMfgWorkOrder(id: string) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/resume`);
  return data.data;
}
export async function cancelMfgWorkOrder(id: string, reason: string) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/cancel`, { reason });
  return data.data;
}
export async function calculateMfgCost(id: string) {
  const { data } = await api.post<Envelope<MfgCostSheetDto>>(`/api/mfg/work-orders/${id}/cost-sheet/calculate`);
  return data.data;
}
export async function pushMfgCost(id: string, body?: {
  periodId?: string | null; wipAccountId?: string | null; fgAccountId?: string | null; note?: string | null;
}) {
  const { data } = await api.post<Envelope<MfgCostSheetDto>>(`/api/mfg/work-orders/${id}/cost-sheet/push`, body ?? {});
  return data.data;
}
export async function closeMfgWorkOrder(id: string, note?: string) {
  const { data } = await api.post<Envelope<MfgWorkOrderDto>>(`/api/mfg/work-orders/${id}/close`, { note });
  return data.data;
}
