import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type AstAssetGroupDto = {
  id: string; code: string; name: string; defaultUsefulLifeMonths: number;
  defaultDepreciationRate: number; status: string; note?: string | null; assetCount: number;
};
export type AstLocationDto = {
  id: string; code: string; name: string; branchName?: string | null; status: string; note?: string | null;
};
export type AstDepreciationMethodDto = {
  id: string; code: string; name: string; methodType: string;
  defaultUsefulLifeMonths: number; defaultRatePercent: number; status: string; note?: string | null;
};
export type AstAssetDto = {
  id: string; code: string; name: string; groupId?: string | null; groupName?: string | null;
  locationId?: string | null; locationName?: string | null;
  depreciationMethodId?: string | null; methodName?: string | null;
  assignedEmployeeId?: string | null; assignedEmployeeName?: string | null;
  originalCost: number; capitalizeDate?: string | null; usefulLifeMonths: number;
  depreciationRatePercent: number; accumulatedDepreciation: number; bookValue: number;
  status: string; disposedAt?: string | null; disposalAmount?: number | null;
  purchaseRef?: string | null; note?: string | null;
};
export type AstMovementDocDto = {
  id: string; code: string; docType: string; docDate: string;
  assetId: string; assetCode?: string | null; assetName?: string | null;
  fromLocationId?: string | null; fromLocationName?: string | null;
  toLocationId?: string | null; toLocationName?: string | null;
  fromEmployeeId?: string | null; fromEmployeeName?: string | null;
  toEmployeeId?: string | null; toEmployeeName?: string | null;
  disposalKind?: string | null; disposalAmount?: number | null; bookValueSnapshot?: number | null;
  status: string; postedAt?: string | null; note?: string | null;
};
export type AstDepreciationLineDto = {
  id: string; runId: string; assetId: string; assetCode?: string | null; assetName?: string | null;
  amount: number; bookValueBefore: number; bookValueAfter: number; lineNo: number;
};
export type AstDepreciationRunDto = {
  id: string; code: string; year: number; month: number; periodStart: string; periodEnd: string;
  status: string; totalAmount: number; lineCount: number; finJournalId?: string | null; postedAt?: string | null;
};
export type AstDepreciationRunDetailDto = { run: AstDepreciationRunDto; lines: AstDepreciationLineDto[] };

export async function fetchAstGroups() {
  const { data } = await api.get<Envelope<AstAssetGroupDto[]>>("/api/ast/groups");
  return data.data;
}
export async function upsertAstGroup(body: {
  id?: string | null; code: string; name: string; defaultUsefulLifeMonths?: number;
  defaultDepreciationRate?: number; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<AstAssetGroupDto>>("/api/ast/groups", body);
  return data.data;
}
export async function fetchAstLocations() {
  const { data } = await api.get<Envelope<AstLocationDto[]>>("/api/ast/locations");
  return data.data;
}
export async function upsertAstLocation(body: {
  id?: string | null; code: string; name: string; branchName?: string | null; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<AstLocationDto>>("/api/ast/locations", body);
  return data.data;
}
export async function fetchAstMethods() {
  const { data } = await api.get<Envelope<AstDepreciationMethodDto[]>>("/api/ast/depreciation-methods");
  return data.data;
}
export async function upsertAstMethod(body: {
  id?: string | null; code: string; name: string; methodType: string;
  defaultUsefulLifeMonths?: number; defaultRatePercent?: number; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<AstDepreciationMethodDto>>("/api/ast/depreciation-methods", body);
  return data.data;
}
export async function fetchAstAssets(q?: string) {
  const { data } = await api.get<Envelope<AstAssetDto[]>>("/api/ast/assets", { params: { q } });
  return data.data;
}
export async function upsertAstAsset(body: {
  id?: string | null; code?: string; name: string; groupId?: string | null; locationId?: string | null;
  depreciationMethodId?: string | null; originalCost: number; capitalizeDate?: string | null;
  usefulLifeMonths?: number; depreciationRatePercent?: number; status?: string;
  purchaseRef?: string | null; note?: string | null; capitalizeFromPurchase?: boolean;
}) {
  const { data } = await api.post<Envelope<AstAssetDto>>("/api/ast/assets", body);
  return data.data;
}
export async function fetchAstRuns() {
  const { data } = await api.get<Envelope<AstDepreciationRunDto[]>>("/api/ast/depreciation-runs");
  return data.data;
}
export async function fetchAstRunDetail(id: string) {
  const { data } = await api.get<Envelope<AstDepreciationRunDetailDto>>(`/api/ast/depreciation-runs/${id}`);
  return data.data;
}
export async function calculateAstDepreciation(year: number, month: number) {
  const { data } = await api.post<Envelope<AstDepreciationRunDto>>("/api/ast/depreciation-runs/calculate", { year, month });
  return data.data;
}
export async function pushAstDepreciationToFin(id: string, body?: {
  expenseAccountId?: string | null; accumAccountId?: string | null; periodId?: string | null;
}) {
  const { data } = await api.post<Envelope<AstDepreciationRunDto>>(`/api/ast/depreciation-runs/${id}/push-fin`, body ?? {});
  return data.data;
}

export async function fetchAstMovements(params?: { docType?: string; status?: string }) {
  const { data } = await api.get<Envelope<AstMovementDocDto[]>>("/api/ast/movements", { params });
  return data.data;
}
export async function upsertAstMovement(body: {
  id?: string | null; code?: string | null; docType: string; docDate?: string | null; assetId: string;
  toLocationId?: string | null; toEmployeeId?: string | null; toEmployeeName?: string | null;
  disposalKind?: string | null; disposalAmount?: number | null; note?: string | null;
}) {
  const { data } = await api.post<Envelope<AstMovementDocDto>>("/api/ast/movements", body);
  return data.data;
}
export async function postAstMovement(id: string) {
  const { data } = await api.post<Envelope<AstMovementDocDto>>(`/api/ast/movements/${id}/post`);
  return data.data;
}
export async function voidAstMovement(id: string, note?: string) {
  const { data } = await api.post<Envelope<AstMovementDocDto>>(`/api/ast/movements/${id}/void`, { note });
  return data.data;
}

export type AstStocktakeDto = {
  id: string; code: string; locationId?: string | null; locationName?: string | null;
  status: string; lineCount: number; countedCount: number; varianceCount: number;
  countedAt?: string | null; reviewedAt?: string | null; note?: string | null;
};
export type AstStocktakeLineDto = {
  id: string; stocktakeId: string; assetId: string; assetCode: string; assetName: string;
  locationId?: string | null; locationName?: string | null;
  expectedPresent: number; countedPresent?: boolean | null; variance: number; note?: string | null;
};
export type AstStocktakeDetailDto = { header: AstStocktakeDto; lines: AstStocktakeLineDto[] };

export type AstRegisterRowDto = {
  id: string; code: string; name: string; groupName?: string | null; locationName?: string | null;
  methodName?: string | null; assignedEmployeeName?: string | null;
  originalCost: number; accumulatedDepreciation: number; bookValue: number;
  status: string; capitalizeDate?: string | null; disposedAt?: string | null;
};
export type AstByLocationRowDto = {
  locationId?: string | null; locationName: string; assetCount: number;
  originalCost: number; accumulatedDepreciation: number; bookValue: number;
};
export type AstDepreciationReportDto = {
  runId?: string | null; runCode?: string | null; year: number; month: number;
  status?: string | null; totalAmount: number; lineCount: number; lines: AstDepreciationLineDto[];
};

export async function fetchAstStocktakes() {
  const { data } = await api.get<Envelope<AstStocktakeDto[]>>("/api/ast/stocktakes");
  return data.data;
}
export async function fetchAstStocktakeDetail(id: string) {
  const { data } = await api.get<Envelope<AstStocktakeDetailDto>>(`/api/ast/stocktakes/${id}`);
  return data.data;
}
export async function fetchAstStocktakeVariances(id: string) {
  const { data } = await api.get<Envelope<AstStocktakeLineDto[]>>(`/api/ast/stocktakes/${id}/variances`);
  return data.data;
}
export async function createAstStocktake(body: { locationId?: string | null; note?: string | null }) {
  const { data } = await api.post<Envelope<AstStocktakeDto>>("/api/ast/stocktakes", body);
  return data.data;
}
export async function countAstStocktakeLine(id: string, body: {
  lineId: string; countedPresent: boolean; note?: string | null;
}) {
  const { data } = await api.post<Envelope<AstStocktakeLineDto>>(`/api/ast/stocktakes/${id}/count`, body);
  return data.data;
}
export async function reviewAstStocktake(id: string) {
  const { data } = await api.post<Envelope<AstStocktakeDto>>(`/api/ast/stocktakes/${id}/review`);
  return data.data;
}
export async function closeAstStocktake(id: string) {
  const { data } = await api.post<Envelope<AstStocktakeDto>>(`/api/ast/stocktakes/${id}/close`);
  return data.data;
}

export async function fetchAstRegister(params?: { status?: string; locationId?: string; groupId?: string }) {
  const { data } = await api.get<Envelope<AstRegisterRowDto[]>>("/api/ast/reports/register", { params });
  return data.data;
}
export async function fetchAstDepreciationReport(year: number, month: number) {
  const { data } = await api.get<Envelope<AstDepreciationReportDto>>("/api/ast/reports/depreciation", {
    params: { year, month },
  });
  return data.data;
}
export async function fetchAstByLocation(params?: { locationId?: string }) {
  const { data } = await api.get<Envelope<AstByLocationRowDto[]>>("/api/ast/reports/by-location", { params });
  return data.data;
}
export async function downloadAstReportCsv(params: {
  report: string; status?: string; locationId?: string; groupId?: string; year?: number; month?: number;
}) {
  const { data } = await api.get<Blob>("/api/ast/reports/export.csv", {
    params,
    responseType: "blob",
  });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = `ast-${params.report}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
