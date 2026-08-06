import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FinCashFundDto = {
  id: string;
  code: string;
  name: string;
  cashAccountId: string;
  cashAccountCode?: string | null;
  cashAccountName?: string | null;
  custodianUserId?: string | null;
  custodianName?: string | null;
  openingBalance: number;
  status: string;
  note?: string | null;
  postedReceiptTotal: number;
  postedPaymentTotal: number;
  bookBalance: number;
};

export type FinCashVoucherDto = {
  id: string;
  code: string;
  fundId: string;
  fundCode?: string | null;
  fundName?: string | null;
  voucherType: string;
  docDate: string;
  amount: number;
  description: string;
  partnerCode?: string | null;
  counterAccountId?: string | null;
  counterAccountCode?: string | null;
  periodId?: string | null;
  periodCode?: string | null;
  status: string;
  finJournalId?: string | null;
  finJournalCode?: string | null;
  postedAt?: string | null;
  note?: string | null;
};

export type FinCashBookDto = {
  fundId: string;
  fundCode: string;
  fundName: string;
  openingBalance: number;
  totalReceipt: number;
  totalPayment: number;
  closingBalance: number;
  rows: {
    docDate: string;
    voucherCode: string;
    voucherType: string;
    description: string;
    partnerCode?: string | null;
    receipt: number;
    payment: number;
    balance: number;
  }[];
};

export async function fetchFinCashFunds() {
  const { data } = await api.get<Envelope<FinCashFundDto[]>>("/api/fin/cash-funds");
  return data.data;
}

export async function upsertFinCashFund(body: {
  id?: string | null;
  code: string;
  name: string;
  cashAccountId: string;
  custodianName?: string | null;
  openingBalance?: number;
  status?: string;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinCashFundDto>>("/api/fin/cash-funds", body);
  return data.data;
}

export async function fetchFinCashVouchers(params?: { fundId?: string; type?: string }) {
  const { data } = await api.get<Envelope<FinCashVoucherDto[]>>("/api/fin/cash-vouchers", { params });
  return data.data;
}

export async function upsertFinCashVoucher(body: {
  id?: string | null;
  fundId: string;
  voucherType: string;
  docDate: string;
  amount: number;
  description: string;
  partnerCode?: string | null;
  counterAccountId?: string | null;
  periodId?: string | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinCashVoucherDto>>("/api/fin/cash-vouchers", body);
  return data.data;
}

export async function postFinCashVoucher(id: string) {
  const { data } = await api.post<Envelope<FinCashVoucherDto>>(`/api/fin/cash-vouchers/${id}/post`);
  return data.data;
}

export async function voidFinCashVoucher(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinCashVoucherDto>>(`/api/fin/cash-vouchers/${id}/void`, { note });
  return data.data;
}

export async function fetchFinCashBook(fundId: string) {
  const { data } = await api.get<Envelope<FinCashBookDto>>("/api/fin/cash-book", { params: { fundId } });
  return data.data;
}
