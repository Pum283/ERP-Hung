import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type LogReturnNoteDto = {
  id: string;
  code: string;
  deliveryOrderId: string;
  deliveryCode?: string | null;
  warehouseId: string;
  warehouseName?: string | null;
  status: string;
  reason?: string | null;
  note?: string | null;
  countedAt?: string | null;
  postedAt?: string | null;
  invStockDocId?: string | null;
  invStockDocCode?: string | null;
  lineCount: number;
  qtyExpectedTotal: number;
  qtyAcceptedTotal: number;
  createdAt: string;
};

export type LogReturnLineDto = {
  id: string;
  returnNoteId: string;
  deliveryLineId?: string | null;
  productCode: string;
  productName: string;
  unit: string;
  qtyExpected: number;
  qtyCounted: number;
  qtyAccepted: number;
  note?: string | null;
};

export type LogReturnDetailDto = {
  header: LogReturnNoteDto;
  lines: LogReturnLineDto[];
};

export type LogOpsReportDto = {
  deliveredCount: number;
  failedCount: number;
  returnedCount: number;
  inTransitCount: number;
  openCount: number;
  returnRatePct: number;
  failRatePct: number;
  returnNotesDraft: number;
  returnNotesCounted: number;
  returnNotesPosted: number;
  codOverdueCount: number;
  onTimeDeliveredCount: number;
  lateDeliveredCount: number;
  promisedDeliveredCount: number;
  onTimeRatePct: number;
};

export async function fetchLogReturns(status?: string) {
  const { data } = await api.get<Envelope<LogReturnNoteDto[]>>("/api/log/returns", { params: { status } });
  return data.data;
}

export async function fetchLogReturnDetail(id: string) {
  const { data } = await api.get<Envelope<LogReturnDetailDto>>(`/api/log/returns/${id}`);
  return data.data;
}

export async function createLogReturn(body: {
  deliveryOrderId: string;
  warehouseId: string;
  reason?: string;
  note?: string;
}) {
  const { data } = await api.post<Envelope<LogReturnDetailDto>>("/api/log/returns", body);
  return data.data;
}

export async function countLogReturnLine(
  noteId: string,
  body: { lineId: string; qtyCounted: number; qtyAccepted?: number; note?: string },
) {
  const { data } = await api.post<Envelope<LogReturnLineDto>>(`/api/log/returns/${noteId}/count`, body);
  return data.data;
}

export async function confirmLogReturnCount(noteId: string) {
  const { data } = await api.post<Envelope<LogReturnDetailDto>>(`/api/log/returns/${noteId}/confirm-count`);
  return data.data;
}

export async function postLogReturn(noteId: string) {
  const { data } = await api.post<Envelope<LogReturnDetailDto>>(`/api/log/returns/${noteId}/post`);
  return data.data;
}

export async function cancelLogReturn(noteId: string, note?: string) {
  const { data } = await api.post<Envelope<LogReturnDetailDto>>(`/api/log/returns/${noteId}/cancel`, {
    status: "Cancelled",
    note,
  });
  return data.data;
}

export async function fetchLogOpsReport() {
  const { data } = await api.get<Envelope<LogOpsReportDto>>("/api/log/reports/ops");
  return data.data;
}
