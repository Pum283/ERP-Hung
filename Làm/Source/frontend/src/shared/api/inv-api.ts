import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type InvItemGroupDto = {
  id: string;
  code: string;
  name: string;
  sortOrder: number;
  isActive: boolean;
  skuCount: number;
};

export type InvUomDto = {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
};

export type InvUnitConversionDto = {
  id: string;
  fromUnitId: string;
  fromUnitCode?: string | null;
  toUnitId: string;
  toUnitCode?: string | null;
  factor: number;
};

export type InvSkuDto = {
  id: string;
  code: string;
  name: string;
  groupId?: string | null;
  groupName?: string | null;
  baseUnitId: string;
  baseUnitCode?: string | null;
  trackLot: boolean;
  trackSerial: boolean;
  trackExpiry: boolean;
  costingMethod: string;
  standardCost: number;
  status: string;
  minQty?: number | null;
  maxQty?: number | null;
  reorderQty?: number | null;
  note?: string | null;
};

export type InvImportResult = {
  total: number;
  success: number;
  failed: number;
  messages: string[];
};

export type InvWarehouseTypeDto = {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
};

export type InvWarehouseDto = {
  id: string;
  code: string;
  name: string;
  warehouseTypeId?: string | null;
  warehouseTypeName?: string | null;
  address?: string | null;
  status: string;
  pickPolicy: string;
  allowNegativeStock: boolean;
  keeperCount: number;
};

export type InvWarehouseKeeperDto = {
  id: string;
  warehouseId: string;
  userId: string;
  userName?: string | null;
  role: string;
  isActive: boolean;
};

export type InvWarehouseDetailDto = {
  warehouse: InvWarehouseDto;
  keepers: InvWarehouseKeeperDto[];
};

export async function fetchInvGroups() {
  const { data } = await api.get<Envelope<InvItemGroupDto[]>>("/api/inv/groups");
  return data.data;
}

export async function upsertInvGroup(body: {
  id?: string | null;
  code: string;
  name: string;
  sortOrder?: number;
  isActive?: boolean;
}) {
  const { data } = await api.post<Envelope<InvItemGroupDto>>("/api/inv/groups", body);
  return data.data;
}

export async function fetchInvUoms() {
  const { data } = await api.get<Envelope<InvUomDto[]>>("/api/inv/uoms");
  return data.data;
}

export async function upsertInvUom(body: {
  id?: string | null;
  code: string;
  name: string;
  isActive?: boolean;
}) {
  const { data } = await api.post<Envelope<InvUomDto>>("/api/inv/uoms", body);
  return data.data;
}

export async function fetchInvConversions() {
  const { data } = await api.get<Envelope<InvUnitConversionDto[]>>("/api/inv/uoms/conversions");
  return data.data;
}

export async function upsertInvConversion(body: {
  id?: string | null;
  fromUnitId: string;
  toUnitId: string;
  factor: number;
}) {
  const { data } = await api.post<Envelope<InvUnitConversionDto>>("/api/inv/uoms/conversions", body);
  return data.data;
}

export async function fetchInvSkus(q?: string) {
  const { data } = await api.get<Envelope<InvSkuDto[]>>("/api/inv/skus", { params: { q } });
  return data.data;
}

export async function upsertInvSku(body: {
  id?: string | null;
  code: string;
  name: string;
  groupId?: string | null;
  baseUnitId: string;
  trackLot?: boolean;
  trackSerial?: boolean;
  trackExpiry?: boolean;
  costingMethod?: string;
  standardCost: number;
  status?: string;
  minQty?: number | null;
  maxQty?: number | null;
  reorderQty?: number | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<InvSkuDto>>("/api/inv/skus", body);
  return data.data;
}

export async function setInvSkuStatus(id: string, status: string) {
  const { data } = await api.post<Envelope<InvSkuDto>>(`/api/inv/skus/${id}/status`, { status });
  return data.data;
}

export async function importInvSkusCsv(csvText: string) {
  const { data } = await api.post<Envelope<InvImportResult>>("/api/inv/skus/import", { csvText });
  return data.data;
}

export async function downloadInvSkusCsv() {
  const res = await api.get("/api/inv/skus/export.csv", { responseType: "blob" });
  const url = URL.createObjectURL(res.data);
  const a = document.createElement("a");
  a.href = url;
  a.download = "inv-skus.csv";
  a.click();
  URL.revokeObjectURL(url);
}

export async function fetchInvWarehouseTypes() {
  const { data } = await api.get<Envelope<InvWarehouseTypeDto[]>>("/api/inv/warehouse-types");
  return data.data;
}

export async function upsertInvWarehouseType(body: {
  id?: string | null;
  code: string;
  name: string;
  isActive?: boolean;
}) {
  const { data } = await api.post<Envelope<InvWarehouseTypeDto>>("/api/inv/warehouse-types", body);
  return data.data;
}

export async function fetchInvWarehouses() {
  const { data } = await api.get<Envelope<InvWarehouseDto[]>>("/api/inv/warehouses");
  return data.data;
}

export async function upsertInvWarehouse(body: {
  id?: string | null;
  code: string;
  name: string;
  warehouseTypeId?: string | null;
  address?: string | null;
  status?: string;
  pickPolicy?: string;
  allowNegativeStock?: boolean;
}) {
  const { data } = await api.post<Envelope<InvWarehouseDto>>("/api/inv/warehouses", body);
  return data.data;
}

export async function fetchInvWarehouseDetail(id: string) {
  const { data } = await api.get<Envelope<InvWarehouseDetailDto>>(`/api/inv/warehouses/${id}`);
  return data.data;
}

export async function upsertInvKeeper(
  warehouseId: string,
  body: { id?: string | null; userId: string; role: string; isActive?: boolean },
) {
  const { data } = await api.post<Envelope<InvWarehouseKeeperDto>>(
    `/api/inv/warehouses/${warehouseId}/keepers`,
    body,
  );
  return data.data;
}
