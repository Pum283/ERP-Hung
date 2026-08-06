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
