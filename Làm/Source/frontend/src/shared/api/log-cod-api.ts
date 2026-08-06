import { api } from "@/shared/api/client";
import type { LogDeliveryOrderDto } from "@/shared/api/log-api";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type LogCodHandoverDto = {
  id: string;
  code: string;
  status: string;
  driverUserId?: string | null;
  driverName?: string | null;
  expectedAmount: number;
  collectedAmount: number;
  remittedAmount: number;
  varianceAmount: number;
  note?: string | null;
  varianceNote?: string | null;
  submittedAt?: string | null;
  reconciledAt?: string | null;
  lineCount: number;
  createdAt: string;
};

export type LogCodHandoverLineDto = {
  id: string;
  handoverId: string;
  deliveryOrderId: string;
  deliveryCode: string;
  customerName: string;
  codAmount: number;
  note?: string | null;
};

export type LogCodHandoverDetailDto = {
  header: LogCodHandoverDto;
  lines: LogCodHandoverLineDto[];
};

export type LogCodReportDto = {
  pendingAmount: number;
  pendingCount: number;
  collectedAmount: number;
  collectedCount: number;
  remittedAmount: number;
  remittedCount: number;
  reconciledAmount: number;
  reconciledCount: number;
  overdueAmount: number;
  overdueCount: number;
  varianceAmount: number;
  varianceCount: number;
};

export async function fetchLogCodDeliveries(status?: string) {
  const { data } = await api.get<Envelope<LogDeliveryOrderDto[]>>("/api/log/cod", {
    params: { status },
  });
  return data.data;
}

export async function fetchLogCodOverdue() {
  const { data } = await api.get<Envelope<LogDeliveryOrderDto[]>>("/api/log/cod/overdue");
  return data.data;
}

export async function fetchLogCodReport() {
  const { data } = await api.get<Envelope<LogCodReportDto>>("/api/log/cod/report");
  return data.data;
}

export async function fetchLogCodHandovers() {
  const { data } = await api.get<Envelope<LogCodHandoverDto[]>>("/api/log/cod/handovers");
  return data.data;
}

export async function fetchLogCodHandoverDetail(id: string) {
  const { data } = await api.get<Envelope<LogCodHandoverDetailDto>>(`/api/log/cod/handovers/${id}`);
  return data.data;
}

export async function createLogCodHandover(body: {
  deliveryOrderIds: string[];
  driverUserId?: string | null;
  driverName?: string | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<LogCodHandoverDetailDto>>("/api/log/cod/handovers", body);
  return data.data;
}

export async function submitLogCodHandover(id: string) {
  const { data } = await api.post<Envelope<LogCodHandoverDetailDto>>(
    `/api/log/cod/handovers/${id}/submit`,
  );
  return data.data;
}

export async function reconcileLogCodHandover(id: string, remittedAmount: number, note?: string) {
  const { data } = await api.post<Envelope<LogCodHandoverDetailDto>>(
    `/api/log/cod/handovers/${id}/reconcile`,
    { remittedAmount, note },
  );
  return data.data;
}

export async function resolveLogCodVariance(id: string, note: string, remittedAmount?: number) {
  const { data } = await api.post<Envelope<LogCodHandoverDetailDto>>(
    `/api/log/cod/handovers/${id}/resolve-variance`,
    { note, remittedAmount },
  );
  return data.data;
}

export async function markLogCod(orderId: string, amount: number, dueDays?: number, note?: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(
    `/api/log/deliveries/${orderId}/cod/mark`,
    { amount, dueDays, note },
  );
  return data.data;
}

export async function setLogCodAmount(orderId: string, amount: number, note?: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(
    `/api/log/deliveries/${orderId}/cod/amount`,
    { amount, note },
  );
  return data.data;
}

export async function collectLogCod(orderId: string, note?: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(
    `/api/log/deliveries/${orderId}/cod/collect`,
    { note },
  );
  return data.data;
}
