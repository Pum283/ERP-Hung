import { api } from "@/shared/api/client";
import type { PosSaleDto } from "@/shared/api/pos-sales-api";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PosPromotionDto = {
  id: string;
  code: string;
  name: string;
  discountType: string;
  discountValue: number;
  minOrderAmount: number;
  startsAt?: string | null;
  endsAt?: string | null;
  status: string;
  note?: string | null;
  voucherCount: number;
};

export type PosVoucherDto = {
  id: string;
  code: string;
  promotionId: string;
  promotionCode?: string | null;
  promotionName?: string | null;
  maxUses: number;
  usedCount: number;
  status: string;
  note?: string | null;
};

export async function fetchPosPromotions(q?: string) {
  const { data } = await api.get<Envelope<PosPromotionDto[]>>("/api/pos/promotions", { params: { q } });
  return data.data;
}

export async function upsertPosPromotion(body: {
  id?: string | null;
  code: string;
  name: string;
  discountType: string;
  discountValue: number;
  minOrderAmount?: number;
  startsAt?: string | null;
  endsAt?: string | null;
  status?: string;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<PosPromotionDto>>("/api/pos/promotions", body);
  return data.data;
}

export async function fetchPosVouchers(promotionId?: string) {
  const { data } = await api.get<Envelope<PosVoucherDto[]>>("/api/pos/vouchers", {
    params: { promotionId },
  });
  return data.data;
}

export async function upsertPosVoucher(body: {
  id?: string | null;
  code: string;
  promotionId: string;
  maxUses: number;
  status?: string;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<PosVoucherDto>>("/api/pos/vouchers", body);
  return data.data;
}

export async function applyPosPromotion(saleId: string, promotionId: string) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/promo/apply`, {
    promotionId,
  });
  return data.data;
}

export async function applyPosVoucher(saleId: string, voucherCode: string) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/promo/voucher`, {
    voucherCode,
  });
  return data.data;
}

export async function requestPosManualDiscount(
  saleId: string,
  discountType: string,
  value: number,
  note?: string,
) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/promo/manual`, {
    discountType,
    value,
    note,
  });
  return data.data;
}

export async function decidePosManualDiscount(saleId: string, approved: boolean, note?: string) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/promo/decide`, {
    approved,
    note,
  });
  return data.data;
}

export async function clearPosDiscount(saleId: string) {
  const { data } = await api.post<Envelope<PosSaleDto>>(`/api/pos/sales/${saleId}/promo/clear`);
  return data.data;
}
