import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type CrmLeadSourceDto = {
  id: string; code: string; name: string; channelType: string; status: string; note?: string | null; leadCount: number;
};
export type CrmLeadDto = {
  id: string; code: string; name: string; phone?: string | null; email?: string | null; companyName?: string | null;
  sourceId?: string | null; sourceName?: string | null; ownerUserId?: string | null; ownerName?: string | null;
  customerId?: string | null; pipelineStatus: string; score: number; nextFollowUpAt?: string | null;
  lostReason?: string | null; opportunityId?: string | null; intakeChannel: string; note?: string | null;
  openTaskCount: number; activityCount: number;
};
export type CrmLeadTaskDto = {
  id: string; leadId: string; title: string; dueAt: string; assigneeUserId?: string | null;
  assigneeName?: string | null; status: string; isReminder: boolean; note?: string | null;
};
export type CrmLeadActivityDto = {
  id: string; leadId: string; activityType: string; content: string; createdByUserId: string;
  createdByName?: string | null; activityAt: string;
};
export type CrmLeadDetailDto = { lead: CrmLeadDto; tasks: CrmLeadTaskDto[]; activities: CrmLeadActivityDto[] };
export type CrmLeadConversionReportDto = {
  totalLeads: number; converted: number; lost: number; conversionRatePercent: number;
  byStatus: { pipelineStatus: string; count: number; conversionRatePercent: number }[];
};
export type CrmOpportunityDto = {
  id: string; code: string; name: string; leadId?: string | null; leadCode?: string | null;
  customerId?: string | null; customerName?: string | null; ownerUserId?: string | null; ownerName?: string | null;
  stage: string; estimatedValue: number; probabilityPercent: number; expectedCloseDate?: string | null;
  quoteId?: string | null; quoteCode?: string | null; lostReason?: string | null; note?: string | null; lineCount: number;
};
export type CrmOpportunityLineDto = {
  id: string; opportunityId: string; itemCode: string; itemName: string;
  quantity: number; unitPrice: number; lineAmount: number; lineNo: number;
};
export type CrmOpportunityDetailDto = { opportunity: CrmOpportunityDto; lines: CrmOpportunityLineDto[] };
export type CrmQuoteDto = {
  id: string; code: string; opportunityId?: string | null; opportunityCode?: string | null;
  customerId?: string | null; customerName?: string | null; priceListId?: string | null; priceListName?: string | null;
  quoteDate: string; validUntil?: string | null;
  subTotal: number; discountPercent: number; discountAmount: number; totalAmount: number;
  status: string; discountApprovalStatus: string; version: number;
  sentAt?: string | null; sentChannel: string; orderId?: string | null; orderCode?: string | null;
  note?: string | null; lineCount: number;
};

export async function fetchCrmLeadSources() {
  const { data } = await api.get<Envelope<CrmLeadSourceDto[]>>("/api/crm/lead-sources");
  return data.data;
}
export async function upsertCrmLeadSource(body: {
  id?: string | null; code: string; name: string; channelType: string; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<CrmLeadSourceDto>>("/api/crm/lead-sources", body);
  return data.data;
}
export async function fetchCrmLeads(params?: { q?: string; status?: string; ownerUserId?: string }) {
  const { data } = await api.get<Envelope<CrmLeadDto[]>>("/api/crm/leads", { params });
  return data.data;
}
export async function fetchCrmLeadDetail(id: string) {
  const { data } = await api.get<Envelope<CrmLeadDetailDto>>(`/api/crm/leads/${id}`);
  return data.data;
}
export async function upsertCrmLead(body: {
  id?: string | null; code?: string; name: string; phone?: string; email?: string; companyName?: string;
  sourceId?: string | null; ownerUserId?: string | null; pipelineStatus?: string; score?: number; note?: string;
}) {
  const { data } = await api.post<Envelope<CrmLeadDto>>("/api/crm/leads", body);
  return data.data;
}
export async function autoIntakeCrmLead(body: {
  name: string; phone?: string; email?: string; companyName?: string; sourceCode?: string; note?: string;
}) {
  const { data } = await api.post<Envelope<CrmLeadDto>>("/api/crm/leads/auto-intake", body);
  return data.data;
}
export async function assignCrmLead(id: string, ownerUserId: string) {
  const { data } = await api.post<Envelope<CrmLeadDto>>(`/api/crm/leads/${id}/assign`, { ownerUserId });
  return data.data;
}
export async function setCrmLeadStatus(id: string, pipelineStatus: string, note?: string) {
  const { data } = await api.post<Envelope<CrmLeadDto>>(`/api/crm/leads/${id}/status`, { pipelineStatus, note });
  return data.data;
}
export async function markCrmLeadLost(id: string, lostReason: string) {
  const { data } = await api.post<Envelope<CrmLeadDto>>(`/api/crm/leads/${id}/mark-lost`, { lostReason });
  return data.data;
}
export async function convertCrmLead(id: string) {
  const { data } = await api.post<Envelope<CrmOpportunityDto>>(`/api/crm/leads/${id}/convert`);
  return data.data;
}
export async function upsertCrmLeadTask(body: {
  id?: string | null; leadId: string; title: string; dueAt: string; assigneeUserId?: string | null;
  status?: string; isReminder?: boolean; note?: string;
}) {
  const { data } = await api.post<Envelope<CrmLeadTaskDto>>("/api/crm/leads/tasks", body);
  return data.data;
}
export async function addCrmLeadActivity(body: {
  leadId: string; activityType: string; content: string; activityAt?: string;
}) {
  const { data } = await api.post<Envelope<CrmLeadActivityDto>>("/api/crm/leads/activities", body);
  return data.data;
}
export async function importCrmLeadsCsv(csvContent: string) {
  const { data } = await api.post<Envelope<{ created: number; skipped: number; errors: string[] }>>(
    "/api/crm/leads/import", { csvContent });
  return data.data;
}
export async function fetchCrmLeadConversionReport() {
  const { data } = await api.get<Envelope<CrmLeadConversionReportDto>>("/api/crm/leads/conversion-report");
  return data.data;
}
export async function fetchCrmOpportunities(params?: { q?: string; stage?: string }) {
  const { data } = await api.get<Envelope<CrmOpportunityDto[]>>("/api/crm/opportunities", { params });
  return data.data;
}
export async function fetchCrmOpportunityDetail(id: string) {
  const { data } = await api.get<Envelope<CrmOpportunityDetailDto>>(`/api/crm/opportunities/${id}`);
  return data.data;
}
export async function upsertCrmOpportunity(body: {
  id?: string | null; code?: string; name: string; leadId?: string | null; customerId?: string | null;
  ownerUserId?: string | null; stage?: string; estimatedValue?: number; note?: string;
}) {
  const { data } = await api.post<Envelope<CrmOpportunityDto>>("/api/crm/opportunities", body);
  return data.data;
}
export async function upsertCrmOpportunityLine(opportunityId: string, body: {
  itemCode: string; itemName: string; quantity: number; unitPrice: number;
}) {
  const { data } = await api.post<Envelope<CrmOpportunityLineDto>>(`/api/crm/opportunities/${opportunityId}/lines`, body);
  return data.data;
}
export async function setCrmOpportunityStage(id: string, stage: string, lostReason?: string) {
  const { data } = await api.post<Envelope<CrmOpportunityDto>>(`/api/crm/opportunities/${id}/stage`, { stage, lostReason });
  return data.data;
}
export async function createCrmQuote(opportunityId: string) {
  const { data } = await api.post<Envelope<CrmQuoteDto>>(`/api/crm/opportunities/${opportunityId}/create-quote`);
  return data.data;
}
