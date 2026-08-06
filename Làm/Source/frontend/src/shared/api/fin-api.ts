import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FinAccountGroupDto = {
  id: string; code: string; name: string; sortOrder: number; isActive: boolean; accountCount: number;
};
export type FinAccountDto = {
  id: string; code: string; name: string; groupId?: string | null; groupName?: string | null;
  accountType: string; isPostable: boolean; status: string; note?: string | null;
};
export type FinFiscalYearDto = {
  id: string; code: string; name: string; year: number; startDate: string; endDate: string;
  isActive: boolean; periodCount: number;
};
export type FinPeriodDto = {
  id: string; fiscalYearId: string; code: string; name: string;
  startDate: string; endDate: string; status: string; lockedAt?: string | null;
};
export type FinCostCenterDto = { id: string; code: string; name: string; status: string; note?: string | null };
export type FinPaymentMethodDto = { id: string; code: string; name: string; status: string };
export type FinTaxDto = {
  id: string; code: string; name: string; ratePercent: number;
  taxType: string; isDefault: boolean;
  effectiveFrom?: string | null; effectiveTo?: string | null;
  status: string; note?: string | null;
};
export type FinJournalLineDto = {
  id: string; journalId: string; accountId: string; accountCode?: string | null; accountName?: string | null;
  debit: number; credit: number; partnerCode?: string | null; costCenterId?: string | null;
  costCenterName?: string | null; note?: string | null; lineNo: number;
};
export type FinJournalDto = {
  id: string; code: string; periodId: string; periodCode?: string | null; entryDate: string;
  description: string; status: string; source: string; reversedFromId?: string | null; reversalId?: string | null;
  partnerCode?: string | null; costCenterId?: string | null; costCenterName?: string | null;
  totalDebit: number; totalCredit: number; lineCount: number; postedAt?: string | null;
};
export type FinJournalDetailDto = { journal: FinJournalDto; lines: FinJournalLineDto[] };
export type FinLedgerRowDto = {
  accountId: string; accountCode: string; accountName: string; debit: number; credit: number; balance: number;
};
export type FinDetailLedgerRowDto = {
  journalId: string; journalCode: string; entryDate: string; description: string;
  accountId: string; accountCode: string; debit: number; credit: number;
  partnerCode?: string | null; costCenterId?: string | null; costCenterName?: string | null;
};

export async function fetchFinGroups() {
  const { data } = await api.get<Envelope<FinAccountGroupDto[]>>("/api/fin/account-groups");
  return data.data;
}
export async function upsertFinGroup(body: {
  id?: string | null; code: string; name: string; sortOrder?: number; isActive?: boolean;
}) {
  const { data } = await api.post<Envelope<FinAccountGroupDto>>("/api/fin/account-groups", body);
  return data.data;
}
export async function fetchFinAccounts(q?: string) {
  const { data } = await api.get<Envelope<FinAccountDto[]>>("/api/fin/accounts", { params: { q } });
  return data.data;
}
export async function upsertFinAccount(body: {
  id?: string | null; code: string; name: string; groupId?: string | null;
  accountType: string; isPostable?: boolean; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinAccountDto>>("/api/fin/accounts", body);
  return data.data;
}
export async function fetchFinFiscalYears() {
  const { data } = await api.get<Envelope<FinFiscalYearDto[]>>("/api/fin/fiscal-years");
  return data.data;
}
export async function upsertFinFiscalYear(body: {
  id?: string | null; code: string; name: string; year: number;
  startDate: string; endDate: string; isActive?: boolean; generateMonths?: boolean;
}) {
  const { data } = await api.post<Envelope<FinFiscalYearDto>>("/api/fin/fiscal-years", body);
  return data.data;
}
export async function fetchFinPeriods(fiscalYearId?: string) {
  const { data } = await api.get<Envelope<FinPeriodDto[]>>("/api/fin/periods", {
    params: { fiscalYearId: fiscalYearId || undefined },
  });
  return data.data;
}
export async function setFinPeriodLock(id: string, lock: boolean) {
  const { data } = await api.post<Envelope<FinPeriodDto>>(`/api/fin/periods/${id}/lock`, { lock });
  return data.data;
}
export async function fetchFinCostCenters() {
  const { data } = await api.get<Envelope<FinCostCenterDto[]>>("/api/fin/cost-centers");
  return data.data;
}
export async function upsertFinCostCenter(body: {
  id?: string | null; code: string; name: string; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinCostCenterDto>>("/api/fin/cost-centers", body);
  return data.data;
}
export async function fetchFinPaymentMethods() {
  const { data } = await api.get<Envelope<FinPaymentMethodDto[]>>("/api/fin/payment-methods");
  return data.data;
}
export async function upsertFinPaymentMethod(body: {
  id?: string | null; code: string; name: string; status?: string;
}) {
  const { data } = await api.post<Envelope<FinPaymentMethodDto>>("/api/fin/payment-methods", body);
  return data.data;
}
export async function fetchFinTaxes() {
  const { data } = await api.get<Envelope<FinTaxDto[]>>("/api/fin/taxes");
  return data.data;
}
export async function upsertFinTax(body: {
  id?: string | null; code: string; name: string; ratePercent: number;
  taxType?: string; isDefault?: boolean;
  effectiveFrom?: string | null; effectiveTo?: string | null;
  status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinTaxDto>>("/api/fin/taxes", body);
  return data.data;
}
export async function fetchFinJournals(q?: string) {
  const { data } = await api.get<Envelope<FinJournalDto[]>>("/api/fin/journals", { params: { q } });
  return data.data;
}
export async function fetchFinJournalDetail(id: string) {
  const { data } = await api.get<Envelope<FinJournalDetailDto>>(`/api/fin/journals/${id}`);
  return data.data;
}
export async function upsertFinJournal(body: {
  id?: string | null; code?: string; periodId: string; entryDate: string; description: string;
  partnerCode?: string | null; costCenterId?: string | null; source?: string;
  lines?: { accountId: string; debit: number; credit: number; partnerCode?: string | null; costCenterId?: string | null; note?: string | null }[];
}) {
  const { data } = await api.post<Envelope<FinJournalDto>>("/api/fin/journals", body);
  return data.data;
}
export async function createFinAutoJournalStub(body: {
  periodId: string; entryDate: string; description: string;
  partnerCode?: string | null; costCenterId?: string | null;
  lines?: { accountId: string; debit: number; credit: number }[];
}) {
  const { data } = await api.post<Envelope<FinJournalDto>>("/api/fin/journals/auto-stub", body);
  return data.data;
}
export async function postFinJournal(id: string) {
  const { data } = await api.post<Envelope<FinJournalDto>>(`/api/fin/journals/${id}/post`);
  return data.data;
}
export async function reverseFinJournal(id: string) {
  const { data } = await api.post<Envelope<FinJournalDto>>(`/api/fin/journals/${id}/reverse`);
  return data.data;
}
export async function fetchFinLedger(params?: {
  accountId?: string; partnerCode?: string; costCenterId?: string; periodId?: string;
}) {
  const { data } = await api.get<Envelope<FinLedgerRowDto[]>>("/api/fin/ledgers", { params });
  return data.data;
}
export async function fetchFinDetailLedger(params?: {
  accountId?: string; partnerCode?: string; costCenterId?: string; periodId?: string;
}) {
  const { data } = await api.get<Envelope<FinDetailLedgerRowDto[]>>("/api/fin/ledgers/detail", { params });
  return data.data;
}
