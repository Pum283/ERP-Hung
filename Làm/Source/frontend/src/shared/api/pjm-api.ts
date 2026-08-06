import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PjmProjectTypeDto = { id: string; code: string; name: string; status: string; note?: string | null };
export type PjmProjectStatusDto = {
  id: string; code: string; name: string; sortOrder: number; isTerminal: boolean; isActive: boolean;
};
export type PjmWbsTemplateDto = {
  id: string; code: string; name: string; status: string; note?: string | null; itemCount: number;
};
export type PjmWbsTemplateItemDto = {
  id: string; templateId: string; code: string; name: string; parentItemId?: string | null; sortOrder: number;
};
export type PjmWbsTemplateDetailDto = { template: PjmWbsTemplateDto; items: PjmWbsTemplateItemDto[] };
export type PjmProjectDto = {
  id: string; code: string; name: string; projectTypeId?: string | null; projectTypeName?: string | null;
  statusCode: string; statusName?: string | null; customerName?: string | null; contractCode?: string | null;
  sourceOpportunityCode?: string | null; pmUserId?: string | null; pmName?: string | null;
  budget: number; startDate?: string | null; endDate?: string | null; note?: string | null;
  memberCount: number; wbsCount: number;
  actualCost: number; recognizedRevenue: number; margin: number; closedAt?: string | null;
};
export type PjmProjectMemberDto = {
  id: string; projectId: string; userId: string; userName?: string | null; role: string; isActive: boolean;
  allocationPct: number; fromDate?: string | null; toDate?: string | null;
};
export type PjmWbsItemDto = {
  id: string; projectId: string; code: string; name: string; parentItemId?: string | null;
  assigneeUserId?: string | null; assigneeName?: string | null; status: string; sortOrder: number; note?: string | null;
  percentComplete: number; isMilestone: boolean; dueDate?: string | null; isOverdue: boolean;
};
export type PjmExpenseDto = {
  id: string; projectId: string; code: string; category: string; description: string;
  amount: number; expenseDate: string; wbsItemId?: string | null; status: string;
  postedAt?: string | null; note?: string | null;
};
export type PjmMaterialIssueDto = {
  id: string; projectId: string; code: string; status: string; note?: string | null; postedAt?: string | null;
  totalAmount: number; lines: { id: string; productCode: string; productName: string; unit: string; qty: number; unitCost: number; amount: number }[];
};
export type PjmAcceptanceDto = {
  id: string; projectId: string; code: string; kind: string; title: string; status: string;
  signerName?: string | null; signedAt?: string | null; note?: string | null;
};
export type PjmCostSummaryDto = {
  budget: number; expenseCost: number; materialCost: number; actualCost: number;
  recognizedRevenue: number; margin: number; budgetVariance: number; hasFinalAcceptance: boolean;
};
export type PjmProjectDetailDto = {
  project: PjmProjectDto; members: PjmProjectMemberDto[]; wbsItems: PjmWbsItemDto[];
  expenses: PjmExpenseDto[]; materialIssues: PjmMaterialIssueDto[]; acceptances: PjmAcceptanceDto[];
  costSummary: PjmCostSummaryDto;
};

export async function fetchPjmTypes() {
  const { data } = await api.get<Envelope<PjmProjectTypeDto[]>>("/api/pjm/types");
  return data.data;
}
export async function upsertPjmType(body: { id?: string | null; code: string; name: string; status?: string; note?: string | null }) {
  const { data } = await api.post<Envelope<PjmProjectTypeDto>>("/api/pjm/types", body);
  return data.data;
}
export async function fetchPjmStatuses() {
  const { data } = await api.get<Envelope<PjmProjectStatusDto[]>>("/api/pjm/statuses");
  return data.data;
}
export async function upsertPjmStatus(body: {
  id?: string | null; code: string; name: string; sortOrder?: number; isTerminal?: boolean; isActive?: boolean;
}) {
  const { data } = await api.post<Envelope<PjmProjectStatusDto>>("/api/pjm/statuses", body);
  return data.data;
}
export async function fetchPjmTemplates() {
  const { data } = await api.get<Envelope<PjmWbsTemplateDto[]>>("/api/pjm/wbs-templates");
  return data.data;
}
export async function fetchPjmTemplateDetail(id: string) {
  const { data } = await api.get<Envelope<PjmWbsTemplateDetailDto>>(`/api/pjm/wbs-templates/${id}`);
  return data.data;
}
export async function upsertPjmTemplate(body: { id?: string | null; code: string; name: string; status?: string; note?: string | null }) {
  const { data } = await api.post<Envelope<PjmWbsTemplateDto>>("/api/pjm/wbs-templates", body);
  return data.data;
}
export async function upsertPjmTemplateItem(templateId: string, body: {
  id?: string | null; code: string; name: string; parentItemId?: string | null; sortOrder?: number;
}) {
  const { data } = await api.post<Envelope<PjmWbsTemplateItemDto>>(`/api/pjm/wbs-templates/${templateId}/items`, body);
  return data.data;
}
export async function fetchPjmProjects(q?: string) {
  const { data } = await api.get<Envelope<PjmProjectDto[]>>("/api/pjm/projects", { params: { q } });
  return data.data;
}
export async function fetchPjmProjectDetail(id: string) {
  const { data } = await api.get<Envelope<PjmProjectDetailDto>>(`/api/pjm/projects/${id}`);
  return data.data;
}
export async function upsertPjmProject(body: {
  id?: string | null; code?: string; name: string; projectTypeId?: string | null; statusCode?: string;
  customerName?: string | null; contractCode?: string | null; sourceOpportunityCode?: string | null;
  pmUserId?: string | null; pmName?: string | null; budget?: number;
  startDate?: string | null; endDate?: string | null; note?: string | null; applyTemplateId?: string | null;
}) {
  const { data } = await api.post<Envelope<PjmProjectDto>>("/api/pjm/projects", body);
  return data.data;
}
export async function upsertPjmMember(projectId: string, body: {
  id?: string | null; userId: string; role: string; isActive?: boolean;
  allocationPct?: number; fromDate?: string | null; toDate?: string | null;
}) {
  const { data } = await api.post<Envelope<PjmProjectMemberDto>>(`/api/pjm/projects/${projectId}/members`, body);
  return data.data;
}
export async function upsertPjmWbs(projectId: string, body: {
  id?: string | null; code: string; name: string; parentItemId?: string | null;
  assigneeUserId?: string | null; assigneeName?: string | null; status?: string; sortOrder?: number; note?: string | null;
  percentComplete?: number; isMilestone?: boolean; dueDate?: string | null;
}) {
  const { data } = await api.post<Envelope<PjmWbsItemDto>>(`/api/pjm/projects/${projectId}/wbs`, body);
  return data.data;
}
export async function upsertPjmExpense(projectId: string, body: {
  id?: string | null; category: string; description: string; amount: number;
  expenseDate?: string | null; wbsItemId?: string | null; note?: string | null; post: boolean;
}) {
  const { data } = await api.post<Envelope<PjmExpenseDto>>(`/api/pjm/projects/${projectId}/expenses`, body);
  return data.data;
}
export async function createPjmMaterialIssue(projectId: string, body: {
  note?: string | null; post: boolean;
  lines: { productCode: string; productName: string; unit?: string; qty: number; unitCost: number }[];
}) {
  const { data } = await api.post<Envelope<PjmMaterialIssueDto>>(`/api/pjm/projects/${projectId}/material-issues`, body);
  return data.data;
}
export async function createPjmAcceptance(projectId: string, body: { kind: string; title: string; note?: string | null }) {
  const { data } = await api.post<Envelope<PjmAcceptanceDto>>(`/api/pjm/projects/${projectId}/acceptances`, body);
  return data.data;
}
export async function signPjmAcceptance(projectId: string, acceptanceId: string, body: { signerName: string; note?: string | null }) {
  const { data } = await api.post<Envelope<PjmAcceptanceDto>>(
    `/api/pjm/projects/${projectId}/acceptances/${acceptanceId}/sign`, body,
  );
  return data.data;
}
export async function recognizePjmRevenue(projectId: string, body: { amount: number; note?: string | null }) {
  const { data } = await api.post<Envelope<PjmProjectDto>>(`/api/pjm/projects/${projectId}/revenue`, body);
  return data.data;
}
export async function closePjmProject(projectId: string, body?: { note?: string | null }) {
  const { data } = await api.post<Envelope<PjmProjectDto>>(`/api/pjm/projects/${projectId}/close`, body ?? {});
  return data.data;
}

