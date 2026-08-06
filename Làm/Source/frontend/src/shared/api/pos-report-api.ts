import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PosRevenueByTimeRowDto = {
  bucket: string; bucketStart?: string | null; shiftId?: string | null; shiftCode?: string | null;
  saleCount: number; revenue: number; discount: number;
};
export type PosRevenueByProductRowDto = {
  productCode: string; productName: string; qty: number; revenue: number; lineCount: number;
};
export type PosRevenueByCashierRowDto = {
  cashierUserId: string; cashierName: string; saleCount: number; revenue: number; discount: number;
};
export type PosCancelDiscountReportDto = {
  totalSales: number; paidSales: number; cancelledSales: number; discountedSales: number;
  cancelRatePercent: number; discountRatePercent: number; totalRevenue: number; totalDiscount: number;
};
export type PosTopProductRowDto = {
  rank: number; productCode: string; productName: string;
  qty: number; revenue: number; lineCount: number;
};
export type PosStoreCompareRowDto = {
  storeId: string; storeCode: string; storeName: string;
  saleCount: number; revenue: number; discount: number;
  avgTicket: number; revenueSharePercent: number;
};
export type PosCostVarianceRowDto = {
  materialCode: string; materialName: string;
  theoreticalQty: number; actualQty: number;
  standardCost: number; theoreticalCost: number; actualCost: number;
  varianceCost: number; variancePercent: number;
};
export type PosChainLiveRowDto = {
  storeId: string; storeCode: string; storeName: string; status: string;
  openShiftCount: number; todaySaleCount: number; todayRevenue: number;
  monthRevenue: number; monthlyTarget: number;
  targetAttainmentPercent: number; monthElapsedPercent: number;
};
export type PosChainLiveReportDto = {
  asOf: string; storeCount: number; openShiftCount: number;
  totalTodayRevenue: number; totalMonthRevenue: number; totalTarget: number;
  totalAttainmentPercent: number;
  rows: PosChainLiveRowDto[];
};
export type PosCostVarianceReportDto = {
  totalTheoreticalCost: number; totalActualCost: number;
  totalVarianceCost: number; totalVariancePercent: number;
  rows: PosCostVarianceRowDto[];
};

export async function fetchPosRevenueByTime(params: {
  from: string; to: string; grain?: string; storeId?: string;
}) {
  const { data } = await api.get<Envelope<PosRevenueByTimeRowDto[]>>("/api/pos/reports/by-time", { params });
  return data.data;
}
export async function fetchPosRevenueByProduct(params: { from: string; to: string; storeId?: string }) {
  const { data } = await api.get<Envelope<PosRevenueByProductRowDto[]>>("/api/pos/reports/by-product", { params });
  return data.data;
}
export async function fetchPosRevenueByCashier(params: { from: string; to: string; storeId?: string }) {
  const { data } = await api.get<Envelope<PosRevenueByCashierRowDto[]>>("/api/pos/reports/by-cashier", { params });
  return data.data;
}
export async function fetchPosCancelDiscount(params: { from: string; to: string; storeId?: string }) {
  const { data } = await api.get<Envelope<PosCancelDiscountReportDto>>("/api/pos/reports/cancel-discount", { params });
  return data.data;
}
export async function fetchPosTopProducts(params: {
  from: string; to: string; top?: number; by?: "qty" | "revenue"; storeId?: string;
}) {
  const { data } = await api.get<Envelope<PosTopProductRowDto[]>>("/api/pos/reports/top-products", { params });
  return data.data;
}
export async function fetchPosStoreCompare(params: { from: string; to: string }) {
  const { data } = await api.get<Envelope<PosStoreCompareRowDto[]>>("/api/pos/reports/store-compare", { params });
  return data.data;
}
export async function fetchPosChainLive() {
  const { data } = await api.get<Envelope<PosChainLiveReportDto>>("/api/pos/reports/chain-live");
  return data.data;
}
export async function fetchPosCostVariance(params: { from: string; to: string; storeId?: string }) {
  const { data } = await api.get<Envelope<PosCostVarianceReportDto>>("/api/pos/reports/cost-variance", { params });
  return data.data;
}
export async function downloadPosReportCsv(params: {
  report: string; from: string; to: string; grain?: string; storeId?: string;
}) {
  const { data } = await api.get<Blob>("/api/pos/reports/export.csv", { params, responseType: "blob" });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = `pos-${params.report}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
