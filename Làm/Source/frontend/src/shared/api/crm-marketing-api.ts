import { api } from "@/shared/api/client";
import { calcPromoDiscount } from "@/shared/api/crm-marketing-calc";
import {
  canSyncPromoToPos,
  formatSyncToPosMessage,
  summarizeVoucherUsageReport,
} from "@/shared/api/crm-promo-sync-helpers";

export { calcPromoDiscount, canSyncPromoToPos, formatSyncToPosMessage, summarizeVoucherUsageReport };

type Envelope<T> = { success: boolean; message?: string; data: T };

export type CrmCampaignDto = {
  id: string; code: string; name: string; description?: string | null;
  channel: string; status: string;
  startDate?: string | null; endDate?: string | null;
  budgetAmount: number; spentAmount: number;
  ownerUserId?: string | null; leadCount: number; revenueGenerated: number;
  closedAt?: string | null; closedReason?: string | null;
};

export type CrmCampaignExpenseDto = {
  id: string; campaignId: string; expenseType: string;
  description?: string | null; amount: number;
  expenseDate: string; invoiceRef?: string | null;
};

export type CrmWebLeadDto = {
  id: string; sourceUrl?: string | null; landingPage?: string | null;
  utmSource?: string | null; utmMedium?: string | null; utmCampaign?: string | null;
  contactName: string; phone?: string | null; email?: string | null;
  syncStatus: string; leadId?: string | null; campaignId?: string | null;
};

export type CrmMarketingMetricsDto = {
  campaignId: string; campaignName: string;
  leadCount: number; totalSpent: number; revenue: number;
  costPerLead: number; customerAcquisitionCost: number;
  roas: number; roiPercent: number;
};

export type CrmMarketingDashboardDto = {
  totalCampaigns: number; activeCampaigns: number;
  totalBudget: number; totalSpent: number; totalRevenue: number;
  overallRoi: number; campaignMetrics: CrmMarketingMetricsDto[];
};

export type CrmPromotionConditionDto = {
  id: string; promotionId: string; conditionType: string; conditionValue: string; operator: string;
};

export type CrmPromotionDto = {
  id: string; code: string; name: string; description?: string | null;
  discountType: string; discountValue: number;
  maxDiscountAmount?: number | null; minOrderValue?: number | null;
  status: string; startDate?: string | null; endDate?: string | null;
  maxUsageTotal?: number | null; maxUsagePerCustomer?: number | null;
  currentUsageCount: number; campaignId?: string | null;
  conditions: CrmPromotionConditionDto[];
};

export type CrmVoucherDto = {
  id: string; promotionId: string; voucherCode: string; status: string;
  expiresAt?: string | null; usageCount: number; maxUsage: number;
  assignedCustomerId?: string | null;
};

export type CrmApplyPromotionResult = {
  applied: boolean; discountAmount: number; message?: string | null;
};

export type CrmSyncPromoToPosResult = {
  crmPromotionId: string; posPromotionId: string; posPromotionCode: string;
  created: boolean; vouchersSynced: number; vouchersSkipped: number; message: string;
};

export type CrmVoucherUsageReportRowDto = {
  voucherId: string; voucherCode: string; promotionId: string;
  promotionCode: string; promotionName: string;
  redeemCount: number; totalDiscount: number; lastUsedAt?: string | null;
};

export async function fetchCrmCampaigns() {
  const res = await api.get<Envelope<CrmCampaignDto[]>>("/api/crm/campaigns");
  return res.data.data;
}

export async function upsertCrmCampaign(body: {
  id?: string; code: string; name: string; description?: string;
  channel: string; startDate?: string; endDate?: string;
  budgetAmount: number; ownerUserId?: string;
}) {
  const res = await api.post<Envelope<CrmCampaignDto>>("/api/crm/campaigns", body);
  return res.data.data;
}

export async function closeCrmCampaign(id: string, reason?: string) {
  const res = await api.post<Envelope<CrmCampaignDto>>(`/api/crm/campaigns/${id}/close`, { reason });
  return res.data.data;
}

export async function fetchCrmCampaignExpenses(campaignId: string) {
  const res = await api.get<Envelope<CrmCampaignExpenseDto[]>>(`/api/crm/campaigns/${campaignId}/expenses`);
  return res.data.data;
}

export async function upsertCrmCampaignExpense(
  campaignId: string,
  body: { id?: string; expenseType: string; description?: string; amount: number; expenseDate?: string; invoiceRef?: string },
) {
  const res = await api.post<Envelope<CrmCampaignExpenseDto>>(`/api/crm/campaigns/${campaignId}/expenses`, body);
  return res.data.data;
}

export async function syncCrmWebLead(body: {
  contactName: string; phone?: string; email?: string;
  sourceUrl?: string; landingPage?: string;
  utmSource?: string; utmMedium?: string; utmCampaign?: string;
  campaignId?: string;
}) {
  const res = await api.post<Envelope<CrmWebLeadDto>>("/api/crm/campaigns/web-leads/sync", body);
  return res.data.data;
}

export async function fetchCrmWebLeads(syncStatus?: string) {
  const res = await api.get<Envelope<CrmWebLeadDto[]>>("/api/crm/campaigns/web-leads", {
    params: syncStatus ? { syncStatus } : undefined,
  });
  return res.data.data;
}

export async function fetchCrmCampaignMetrics(id: string) {
  const res = await api.get<Envelope<CrmMarketingMetricsDto>>(`/api/crm/campaigns/${id}/metrics`);
  return res.data.data;
}

export async function fetchCrmMarketingDashboard() {
  const res = await api.get<Envelope<CrmMarketingDashboardDto>>("/api/crm/campaigns/dashboard");
  return res.data.data;
}

export async function fetchCrmPromotions() {
  const res = await api.get<Envelope<CrmPromotionDto[]>>("/api/crm/promotions");
  return res.data.data;
}

export async function upsertCrmPromotion(body: {
  id?: string; code: string; name: string; description?: string;
  discountType: string; discountValue: number;
  maxDiscountAmount?: number; minOrderValue?: number;
  startDate?: string; endDate?: string;
  maxUsageTotal?: number; maxUsagePerCustomer?: number; campaignId?: string;
  conditions?: { conditionType: string; conditionValue: string; operator: string }[];
}) {
  const res = await api.post<Envelope<CrmPromotionDto>>("/api/crm/promotions", body);
  return res.data.data;
}

export async function generateCrmVouchers(
  promotionId: string,
  body: { quantity: number; prefix?: string; maxUsagePerVoucher: number; expiresAt?: string },
) {
  const res = await api.post<Envelope<CrmVoucherDto[]>>(
    `/api/crm/promotions/${promotionId}/vouchers/generate`,
    { ...body, promotionId },
  );
  return res.data.data;
}

export async function fetchCrmVouchers(promotionId: string) {
  const res = await api.get<Envelope<CrmVoucherDto[]>>(`/api/crm/promotions/${promotionId}/vouchers`);
  return res.data.data;
}

export async function applyCrmPromotionOnQuote(body: {
  quoteId: string; promotionId?: string; voucherCode?: string;
}) {
  const res = await api.post<Envelope<CrmApplyPromotionResult>>("/api/crm/promotions/apply", body);
  return res.data.data;
}

export async function syncCrmPromotionToPos(promotionId: string) {
  const res = await api.post<Envelope<CrmSyncPromoToPosResult>>(
    `/api/crm/promotions/${promotionId}/sync-pos`,
  );
  return res.data.data;
}

export async function fetchCrmVoucherUsageReport(params?: {
  promotionId?: string; from?: string; to?: string;
}) {
  const res = await api.get<Envelope<CrmVoucherUsageReportRowDto[]>>(
    "/api/crm/promotions/voucher-usage-report",
    { params },
  );
  return res.data.data;
}
