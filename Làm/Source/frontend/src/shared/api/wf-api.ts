import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type WfTaskDto = {
  id: string;
  instanceId: string;
  nodeId: string;
  nodeName?: string | null;
  status: string;
  dueAt?: string | null;
  sourceModule: string;
  sourceDocType: string;
  sourceDocId: string;
  docSummary?: string | null;
  assigneeUserId?: string | null;
  assigneeName?: string | null;
  viaDelegation?: boolean;
};

export type WfDelegationDto = {
  id: string;
  fromUserId: string;
  fromUserName: string;
  toUserId: string;
  toUserName: string;
  startDate: string;
  endDate: string;
  moduleCode?: string | null;
  isActive: boolean;
  note?: string | null;
  createdAt: string;
};

export type WfDashboardDto = {
  pendingTasks: number;
  overdueTasks: number;
  completedToday: number;
  rejectedToday: number;
  runningInstances: number;
  completedInstances: number;
  rejectedInstances: number;
  byModule: { moduleCode: string; pending: number; completed: number; rejected: number }[];
  last7Days: { date: string; completed: number; rejected: number }[];
  topAssignees: { userId: string; userName: string; pendingCount: number }[];
};

export async function fetchMyWfTasks() {
  const { data } = await api.get<Envelope<WfTaskDto[]>>("/api/wf/tasks/my");
  return data.data;
}

export async function actWfTask(taskId: string, action: "Approve" | "Reject", comment?: string) {
  const { data } = await api.post<Envelope<{ ok: boolean }>>(`/api/wf/tasks/${taskId}/act`, {
    action,
    comment: comment ?? null,
  });
  return data.data;
}

export async function fetchWfDelegations() {
  const { data } = await api.get<Envelope<WfDelegationDto[]>>("/api/wf/delegations");
  return data.data;
}

export async function upsertWfDelegation(body: {
  id?: string | null;
  toUserId: string;
  startDate: string;
  endDate: string;
  moduleCode?: string | null;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<WfDelegationDto>>("/api/wf/delegations", body);
  return data.data;
}

export async function deactivateWfDelegation(id: string) {
  await api.post(`/api/wf/delegations/${id}/deactivate`);
}

export async function fetchWfDashboard() {
  const { data } = await api.get<Envelope<WfDashboardDto>>("/api/wf/dashboard");
  return data.data;
}
