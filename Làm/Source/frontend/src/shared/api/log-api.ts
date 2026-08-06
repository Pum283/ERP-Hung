import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type LogCarrierDto = {
  id: string;
  code: string;
  name: string;
  phone?: string | null;
  contactName?: string | null;
  note?: string | null;
  status: string;
};

export type LogDeliveryOrderDto = {
  id: string;
  code: string;
  sourceOrderCode: string;
  customerName: string;
  shipAddress?: string | null;
  phone?: string | null;
  status: string;
  carrierId?: string | null;
  carrierName?: string | null;
  driverUserId?: string | null;
  driverName?: string | null;
  parentOrderId?: string | null;
  batchNo: number;
  note?: string | null;
  failureReason?: string | null;
  waybillNo?: string | null;
  waybillPrintedAt?: string | null;
  pickedAt?: string | null;
  dispatchedAt?: string | null;
  deliveredAt?: string | null;
  promisedAt?: string | null;
  onTime?: boolean | null;
  isCod: boolean;
  codAmount: number;
  codStatus: string;
  codDueAt?: string | null;
  codCollectedAt?: string | null;
  codHandoverId?: string | null;
  codOverdue: boolean;
  lineCount: number;
};

export type LogDeliveryLineDto = {
  id: string;
  deliveryOrderId: string;
  productCode: string;
  productName: string;
  qty: number;
  qtyPicked: number;
  unit: string;
  note?: string | null;
};

export type LogShipmentEventDto = {
  id: string;
  deliveryOrderId: string;
  status: string;
  note?: string | null;
  actorUserId: string;
  actorName?: string | null;
  occurredAt: string;
};

export type LogDeliveryDetailDto = {
  order: LogDeliveryOrderDto;
  lines: LogDeliveryLineDto[];
  events: LogShipmentEventDto[];
  childBatches: LogDeliveryOrderDto[];
};

export async function fetchLogCarriers(q?: string) {
  const { data } = await api.get<Envelope<LogCarrierDto[]>>("/api/log/carriers", { params: { q } });
  return data.data;
}

export async function upsertLogCarrier(body: {
  id?: string | null;
  code: string;
  name: string;
  phone?: string | null;
  contactName?: string | null;
  note?: string | null;
  status?: string;
}) {
  const { data } = await api.post<Envelope<LogCarrierDto>>("/api/log/carriers", body);
  return data.data;
}

export async function fetchLogDeliveries(q?: string) {
  const { data } = await api.get<Envelope<LogDeliveryOrderDto[]>>("/api/log/deliveries", { params: { q } });
  return data.data;
}

export async function fetchLogDeliveryDetail(id: string) {
  const { data } = await api.get<Envelope<LogDeliveryDetailDto>>(`/api/log/deliveries/${id}`);
  return data.data;
}

export async function upsertLogDelivery(body: {
  id?: string | null;
  code?: string;
  sourceOrderCode: string;
  customerName: string;
  shipAddress?: string | null;
  phone?: string | null;
  note?: string | null;
  promisedAt?: string | null;
}) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>("/api/log/deliveries", body);
  return data.data;
}

export async function upsertLogDeliveryLine(
  orderId: string,
  body: {
    id?: string | null;
    productCode: string;
    productName: string;
    qty: number;
    unit?: string;
    note?: string | null;
  },
) {
  const { data } = await api.post<Envelope<LogDeliveryLineDto>>(
    `/api/log/deliveries/${orderId}/lines`,
    body,
  );
  return data.data;
}

export async function confirmLogDelivery(id: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/confirm`);
  return data.data;
}

export async function splitLogDelivery(
  id: string,
  body: { lines: { lineId: string; qty: number }[]; note?: string },
) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/split`, body);
  return data.data;
}

export async function startLogPick(id: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/start-pick`);
  return data.data;
}

export async function confirmLogPick(
  id: string,
  lines: { lineId: string; qtyPicked: number }[],
) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(
    `/api/log/deliveries/${id}/confirm-pick`,
    { lines },
  );
  return data.data;
}

export async function dispatchLogDelivery(id: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/dispatch`);
  return data.data;
}

export async function printLogWaybill(id: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/waybill`);
  return data.data;
}

export async function assignLogDelivery(
  id: string,
  body: { carrierId?: string | null; driverUserId?: string | null; driverName?: string | null },
) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/assign`, body);
  return data.data;
}

export async function updateLogStatus(id: string, status: string, note?: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/status`, {
    status,
    note,
  });
  return data.data;
}

export async function cancelLogDelivery(id: string, note?: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/cancel`, {
    status: "Cancelled",
    note,
  });
  return data.data;
}

export async function returnLogDelivery(id: string, note?: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/return`, {
    status: "Returned",
    note,
  });
  return data.data;
}

export async function failLogDelivery(id: string, reason: string) {
  const { data } = await api.post<Envelope<LogDeliveryOrderDto>>(`/api/log/deliveries/${id}/fail`, {
    reason,
  });
  return data.data;
}
