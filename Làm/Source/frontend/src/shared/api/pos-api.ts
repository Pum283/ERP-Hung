import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PosStoreDto = {
  id: string;
  code: string;
  name: string;
  address?: string | null;
  status: string;
  warehouseId?: string | null;
  warehouseName?: string | null;
  monthlyRevenueTarget: number;
  terminalCount: number;
  printerCount: number;
  cashierCount: number;
};

export type PosTerminalDto = {
  id: string;
  storeId: string;
  code: string;
  name: string;
  status: string;
};

export type PosPrinterDto = {
  id: string;
  storeId: string;
  code: string;
  name: string;
  printerType: string;
  connectionInfo?: string | null;
  status: string;
};

export type PosCashierDto = {
  id: string;
  storeId: string;
  userId: string;
  userName?: string | null;
  role: string;
  isActive: boolean;
};

export type PosStoreDetailDto = {
  store: PosStoreDto;
  terminals: PosTerminalDto[];
  printers: PosPrinterDto[];
  cashiers: PosCashierDto[];
};

export type PosCategoryDto = {
  id: string;
  code: string;
  name: string;
  sortOrder: number;
  isActive: boolean;
  productCount: number;
};

export type PosProductDto = {
  id: string;
  categoryId?: string | null;
  categoryName?: string | null;
  code: string;
  name: string;
  unit?: string | null;
  status: string;
  sortOrder: number;
  syncedAt?: string | null;
  bomLineCount: number;
};

export type PosBomLineDto = {
  id: string;
  productId: string;
  materialCode: string;
  materialName: string;
  qty: number;
  unit: string;
};

export type PosTaxRateDto = {
  id: string;
  code: string;
  name: string;
  ratePct: number;
  isDefault: boolean;
  isActive: boolean;
};

export type PosPriceListDto = {
  id: string;
  storeId: string;
  storeName?: string | null;
  code: string;
  name: string;
  status: string;
  itemCount: number;
};

export type PosPriceItemDto = {
  id: string;
  priceListId: string;
  productId: string;
  productCode?: string | null;
  productName?: string | null;
  price: number;
  taxRateId?: string | null;
  taxCode?: string | null;
  taxRatePct?: number | null;
};

export async function fetchPosStores() {
  const { data } = await api.get<Envelope<PosStoreDto[]>>("/api/pos/stores");
  return data.data;
}

export async function upsertPosStore(body: {
  id?: string;
  code: string;
  name: string;
  address?: string;
  status?: string;
  warehouseId?: string | null;
  monthlyRevenueTarget?: number | null;
}) {
  const { data } = await api.post<Envelope<PosStoreDto>>("/api/pos/stores", body);
  return data.data;
}

export async function fetchPosStoreDetail(id: string) {
  const { data } = await api.get<Envelope<PosStoreDetailDto>>(`/api/pos/stores/${id}`);
  return data.data;
}

export async function upsertPosTerminal(
  storeId: string,
  body: { id?: string; code: string; name: string; status?: string },
) {
  const { data } = await api.post<Envelope<PosTerminalDto>>(
    `/api/pos/stores/${storeId}/terminals`,
    body,
  );
  return data.data;
}

export async function upsertPosPrinter(
  storeId: string,
  body: {
    id?: string;
    code: string;
    name: string;
    printerType: string;
    connectionInfo?: string;
    status?: string;
  },
) {
  const { data } = await api.post<Envelope<PosPrinterDto>>(
    `/api/pos/stores/${storeId}/printers`,
    body,
  );
  return data.data;
}

export async function upsertPosCashier(
  storeId: string,
  body: { id?: string; userId: string; role: string; isActive?: boolean },
) {
  const { data } = await api.post<Envelope<PosCashierDto>>(
    `/api/pos/stores/${storeId}/cashiers`,
    body,
  );
  return data.data;
}

export async function fetchPosCategories() {
  const { data } = await api.get<Envelope<PosCategoryDto[]>>("/api/pos/categories");
  return data.data;
}

export async function upsertPosCategory(body: {
  id?: string;
  code: string;
  name: string;
  sortOrder?: number;
  isActive?: boolean;
}) {
  const { data } = await api.post<Envelope<PosCategoryDto>>("/api/pos/categories", body);
  return data.data;
}

export async function fetchPosProducts(q?: string) {
  const { data } = await api.get<Envelope<PosProductDto[]>>("/api/pos/products", {
    params: { q },
  });
  return data.data;
}

export async function upsertPosProduct(body: {
  id?: string;
  categoryId?: string | null;
  code: string;
  name: string;
  unit?: string;
  status?: string;
  sortOrder?: number;
}) {
  const { data } = await api.post<Envelope<PosProductDto>>("/api/pos/products", body);
  return data.data;
}

export async function setPosProductStatus(id: string, status: string) {
  const { data } = await api.post<Envelope<PosProductDto>>(`/api/pos/products/${id}/status`, {
    status,
  });
  return data.data;
}

export async function fetchPosBom(productId: string) {
  const { data } = await api.get<Envelope<PosBomLineDto[]>>(`/api/pos/products/${productId}/bom`);
  return data.data;
}

export async function upsertPosBom(
  productId: string,
  body: {
    id?: string;
    materialCode: string;
    materialName: string;
    qty: number;
    unit?: string;
  },
) {
  const { data } = await api.post<Envelope<PosBomLineDto>>(
    `/api/pos/products/${productId}/bom`,
    body,
  );
  return data.data;
}

export type PosSyncResult = {
  productCount: number; createdCount: number; updatedCount: number;
  suspendedCount: number; syncedAt: string;
};

/** UC_POS_015 — đồng bộ catalog thật từ back-office (INV SKU). */
export async function syncPosCatalog() {
  const { data } = await api.post<Envelope<PosSyncResult>>("/api/pos/products/sync", {});
  return data.data;
}

export async function fetchPosTaxRates() {
  const { data } = await api.get<Envelope<PosTaxRateDto[]>>("/api/pos/tax-rates");
  return data.data;
}

export async function upsertPosTaxRate(body: {
  id?: string;
  code: string;
  name: string;
  ratePct: number;
  isDefault?: boolean;
  isActive?: boolean;
}) {
  const { data } = await api.post<Envelope<PosTaxRateDto>>("/api/pos/tax-rates", body);
  return data.data;
}

export async function fetchPosPriceLists() {
  const { data } = await api.get<Envelope<PosPriceListDto[]>>("/api/pos/price-lists");
  return data.data;
}

export async function upsertPosPriceList(body: {
  id?: string;
  storeId: string;
  code: string;
  name: string;
  status?: string;
}) {
  const { data } = await api.post<Envelope<PosPriceListDto>>("/api/pos/price-lists", body);
  return data.data;
}

export async function fetchPosPriceItems(priceListId: string) {
  const { data } = await api.get<Envelope<PosPriceItemDto[]>>(
    `/api/pos/price-lists/${priceListId}/items`,
  );
  return data.data;
}

export async function upsertPosPriceItem(
  priceListId: string,
  body: { id?: string; productId: string; price: number; taxRateId?: string | null },
) {
  const { data } = await api.post<Envelope<PosPriceItemDto>>(
    `/api/pos/price-lists/${priceListId}/items`,
    body,
  );
  return data.data;
}
