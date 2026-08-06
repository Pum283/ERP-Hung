import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FsmServiceTypeDto = { id: string; code: string; name: string; status: string; note?: string | null };
export type FsmFaultCodeDto = { id: string; code: string; name: string; severity: string; status: string; note?: string | null };
export type FsmPartDto = { id: string; code: string; name: string; unit: string; status: string; note?: string | null };
export type FsmSlaPolicyDto = {
  id: string; code: string; name: string; priority: string;
  responseHours: number; resolveHours: number; isActive: boolean; note?: string | null;
};
export type FsmAssetDto = {
  id: string; code: string; customerName: string; customerPhone?: string | null;
  serialNo: string; model?: string | null; activatedAt?: string | null; warrantyEndAt?: string | null;
  status: string; address?: string | null; note?: string | null; warrantyExpiringSoon: boolean;
};
export type FsmAssetHistoryDto = {
  id: string; assetId: string; eventType: string; summary: string; ticketId?: string | null;
  actorUserId: string; actorName?: string | null; occurredAt: string;
};
export type FsmAssetDetailDto = { asset: FsmAssetDto; history: FsmAssetHistoryDto[] };
export type FsmTicketDto = {
  id: string; code: string; channel: string; subject: string; description?: string | null;
  customerName: string; customerPhone?: string | null;
  serviceTypeId?: string | null; serviceTypeName?: string | null;
  faultCodeId?: string | null; faultCodeName?: string | null;
  assetId?: string | null; assetCode?: string | null; serialNo?: string | null;
  slaPolicyId?: string | null; slaPolicyName?: string | null;
  priority: string; status: string;
  assignedTechUserId?: string | null; assignedTechName?: string | null;
  dueResponseAt?: string | null; dueResolveAt?: string | null;
  escalateReason?: string | null; createdAt: string;
  appointmentAt?: string | null; appointmentNote?: string | null;
  rootCause?: string | null; resolutionNote?: string | null;
  checkedOutAt?: string | null;
  acceptanceSignedAt?: string | null; acceptanceSignerName?: string | null; acceptanceNote?: string | null;
  resolvedAt?: string | null; closedAt?: string | null;
  slaResponseMet?: boolean | null; slaResolveMet?: boolean | null;
};

export async function fetchFsmServiceTypes() {
  const { data } = await api.get<Envelope<FsmServiceTypeDto[]>>("/api/fsm/service-types");
  return data.data;
}
export async function upsertFsmServiceType(body: { id?: string | null; code: string; name: string; status?: string; note?: string | null }) {
  const { data } = await api.post<Envelope<FsmServiceTypeDto>>("/api/fsm/service-types", body);
  return data.data;
}
export async function fetchFsmFaultCodes() {
  const { data } = await api.get<Envelope<FsmFaultCodeDto[]>>("/api/fsm/fault-codes");
  return data.data;
}
export async function upsertFsmFaultCode(body: {
  id?: string | null; code: string; name: string; severity?: string; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<FsmFaultCodeDto>>("/api/fsm/fault-codes", body);
  return data.data;
}
export async function fetchFsmParts() {
  const { data } = await api.get<Envelope<FsmPartDto[]>>("/api/fsm/parts");
  return data.data;
}
export async function upsertFsmPart(body: {
  id?: string | null; code: string; name: string; unit?: string; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<FsmPartDto>>("/api/fsm/parts", body);
  return data.data;
}
export async function fetchFsmSlaPolicies() {
  const { data } = await api.get<Envelope<FsmSlaPolicyDto[]>>("/api/fsm/sla-policies");
  return data.data;
}
export async function upsertFsmSlaPolicy(body: {
  id?: string | null; code: string; name: string; priority: string;
  responseHours: number; resolveHours: number; isActive?: boolean; note?: string | null;
}) {
  const { data } = await api.post<Envelope<FsmSlaPolicyDto>>("/api/fsm/sla-policies", body);
  return data.data;
}
export async function fetchFsmAssets(q?: string) {
  const { data } = await api.get<Envelope<FsmAssetDto[]>>("/api/fsm/assets", { params: { q } });
  return data.data;
}
export async function fetchFsmAssetDetail(id: string) {
  const { data } = await api.get<Envelope<FsmAssetDetailDto>>(`/api/fsm/assets/${id}`);
  return data.data;
}
export async function upsertFsmAsset(body: {
  id?: string | null; code?: string; customerName: string; customerPhone?: string | null;
  serialNo: string; model?: string | null; activatedAt?: string | null; warrantyEndAt?: string | null;
  status?: string; address?: string | null; note?: string | null;
}) {
  const { data } = await api.post<Envelope<FsmAssetDto>>("/api/fsm/assets", body);
  return data.data;
}
export async function addFsmAssetHistory(id: string, body: { eventType: string; summary: string }) {
  const { data } = await api.post<Envelope<FsmAssetHistoryDto>>(`/api/fsm/assets/${id}/history`, body);
  return data.data;
}
export async function fetchFsmTickets(q?: string) {
  const { data } = await api.get<Envelope<FsmTicketDto[]>>("/api/fsm/tickets", { params: { q } });
  return data.data;
}
export async function upsertFsmTicket(body: {
  id?: string | null; channel: string; subject: string; description?: string | null;
  customerName: string; customerPhone?: string | null;
  serviceTypeId?: string | null; faultCodeId?: string | null; assetId?: string | null; priority?: string;
}) {
  const { data } = await api.post<Envelope<FsmTicketDto>>("/api/fsm/tickets", body);
  return data.data;
}
export async function assignFsmTicket(id: string, body: { techUserId: string; techName?: string | null }) {
  const { data } = await api.post<Envelope<FsmTicketDto>>(`/api/fsm/tickets/${id}/assign`, body);
  return data.data;
}
export async function escalateFsmTicket(id: string, body: {
  newTechUserId: string; newTechName?: string | null; reason: string;
}) {
  const { data } = await api.post<Envelope<FsmTicketDto>>(`/api/fsm/tickets/${id}/escalate`, body);
  return data.data;
}
export async function setFsmTicketStatus(id: string, status: string, note?: string) {
  const { data } = await api.post<Envelope<FsmTicketDto>>(`/api/fsm/tickets/${id}/status`, { status, note });
  return data.data;
}
export async function setFsmAppointment(id: string, body: { appointmentAt: string; note?: string }) {
  const { data } = await api.post<Envelope<FsmTicketDto>>(`/api/fsm/tickets/${id}/appointment`, body);
  return data.data;
}
export async function workLogFsmTicket(id: string, body: { rootCause: string; resolutionNote: string; faultCodeId?: string | null }) {
  const { data } = await api.post<Envelope<FsmTicketDto>>(`/api/fsm/tickets/${id}/work-log`, body);
  return data.data;
}
export async function checkoutFsmTicket(id: string, note?: string) {
  const { data } = await api.post<Envelope<FsmTicketDto>>(`/api/fsm/tickets/${id}/checkout`, { note });
  return data.data;
}
export async function acceptFsmTicket(id: string, body: { signerName: string; note?: string }) {
  const { data } = await api.post<Envelope<FsmTicketDto>>(`/api/fsm/tickets/${id}/accept`, body);
  return data.data;
}
export async function closeFsmTicket(id: string, note?: string) {
  const { data } = await api.post<Envelope<FsmTicketDto>>(`/api/fsm/tickets/${id}/close`, { note });
  return data.data;
}

export type FsmTicketPartLineDto = {
  id: string; ticketId: string; partId: string; partCode: string; partName: string;
  qty: number; unitCost: number; amount: number; source: string;
  techUserId?: string | null; techName?: string | null; issuedAt: string; note?: string | null;
};
export type FsmPartStockDto = {
  id: string; partId: string; partCode: string; partName: string; unit: string;
  locationType: string; techUserId?: string | null; techName?: string | null;
  qtyOnHand: number; unitCost: number; amount: number;
};
export type FsmPartIssueDocDto = {
  id: string; code: string; techUserId: string; techName: string; status: string;
  note?: string | null; postedAt?: string | null; createdAt: string;
  lines: { id: string; partId: string; partCode: string; partName: string; qty: number; unitCost: number }[];
};
export type FsmPartReconcileDocDto = {
  id: string; code: string; scope: string; techUserId?: string | null; techName?: string | null;
  status: string; note?: string | null; postedAt?: string | null; createdAt: string;
  lines: {
    id: string; partId: string; partCode: string; partName: string;
    systemQty: number; countedQty: number; diffQty: number; unitCost: number;
  }[];
};

export async function fetchFsmPartStock(locationType?: string, techUserId?: string) {
  const { data } = await api.get<Envelope<FsmPartStockDto[]>>("/api/fsm/part-stock", {
    params: { locationType, techUserId },
  });
  return data.data;
}
export async function receiptFsmPartStock(body: { partId: string; qty: number; unitCost?: number | null }) {
  const { data } = await api.post<Envelope<FsmPartStockDto>>("/api/fsm/part-stock/receipt", body);
  return data.data;
}
export async function fetchFsmPartIssues() {
  const { data } = await api.get<Envelope<FsmPartIssueDocDto[]>>("/api/fsm/part-issues");
  return data.data;
}
export async function createFsmPartIssue(body: {
  techUserId: string; techName?: string | null; note?: string | null;
  lines: { partId: string; qty: number; unitCost?: number | null }[];
}) {
  const { data } = await api.post<Envelope<FsmPartIssueDocDto>>("/api/fsm/part-issues", body);
  return data.data;
}
export async function fetchFsmPartReconciles() {
  const { data } = await api.get<Envelope<FsmPartReconcileDocDto[]>>("/api/fsm/part-reconciles");
  return data.data;
}
export async function createFsmPartReconcile(body: {
  scope: string; techUserId?: string | null; techName?: string | null; note?: string | null;
  lines: { partId: string; countedQty: number }[];
}) {
  const { data } = await api.post<Envelope<FsmPartReconcileDocDto>>("/api/fsm/part-reconciles", body);
  return data.data;
}
export async function fetchFsmTicketParts(ticketId: string) {
  const { data } = await api.get<Envelope<FsmTicketPartLineDto[]>>(`/api/fsm/tickets/${ticketId}/parts`);
  return data.data;
}
export async function consumeFsmTicketPart(ticketId: string, body: {
  partId: string; qty: number; unitCost?: number | null; source?: string; techUserId?: string | null; note?: string | null;
}) {
  const { data } = await api.post<Envelope<FsmTicketPartLineDto>>(`/api/fsm/tickets/${ticketId}/parts`, body);
  return data.data;
}

