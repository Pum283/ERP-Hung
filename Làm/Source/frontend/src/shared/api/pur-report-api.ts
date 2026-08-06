import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PurPurchaseByVendorRowDto = {
  vendorId: string; vendorCode: string; vendorName: string;
  grnCount: number; acceptedQty: number; amount: number;
};
export type PurPurchaseByProductRowDto = {
  productCode: string; productName: string;
  acceptedQty: number; amount: number; lineCount: number;
};
export type PurOpenPrAgingRowDto = {
  id: string; code: string; status: string; createdAt: string; ageDays: number;
  requestedByName?: string | null; lineCount: number; totalQty: number;
};
export type PurOpenPoAgingRowDto = {
  id: string; code: string; vendorId: string; vendorCode?: string | null; vendorName?: string | null;
  status: string; createdAt: string; ageDays: number; openQty: number; openAmount: number;
};

export async function fetchPurPurchaseByVendor(params: { from: string; to: string; vendorId?: string }) {
  const { data } = await api.get<Envelope<PurPurchaseByVendorRowDto[]>>("/api/pur/reports/by-vendor", { params });
  return data.data;
}
export async function fetchPurPurchaseByProduct(params: { from: string; to: string; vendorId?: string }) {
  const { data } = await api.get<Envelope<PurPurchaseByProductRowDto[]>>("/api/pur/reports/by-product", { params });
  return data.data;
}
export async function fetchPurOpenPrAging() {
  const { data } = await api.get<Envelope<PurOpenPrAgingRowDto[]>>("/api/pur/reports/open-pr");
  return data.data;
}
export async function fetchPurOpenPoAging(params?: { vendorId?: string }) {
  const { data } = await api.get<Envelope<PurOpenPoAgingRowDto[]>>("/api/pur/reports/open-po", { params });
  return data.data;
}
export async function downloadPurReportCsv(params: {
  report: string; from?: string; to?: string; vendorId?: string;
}) {
  const { data } = await api.get<Blob>("/api/pur/reports/export.csv", { params, responseType: "blob" });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = `pur-${params.report}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
