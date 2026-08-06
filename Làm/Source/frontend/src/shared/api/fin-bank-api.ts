import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FinBankAccountDto = {
  id: string;
  code: string;
  name: string;
  bankName: string;
  accountNumber: string;
  branchName?: string | null;
  glAccountId: string;
  glAccountCode?: string | null;
  glAccountName?: string | null;
  openingBalance: number;
  status: string;
  note?: string | null;
  postedCreditTotal: number;
  postedDebitTotal: number;
  bookBalance: number;
};

export type FinBankVoucherDto = {
  id: string;
  code: string;
  bankAccountId: string;
  bankAccountCode?: string | null;
  bankAccountName?: string | null;
  voucherType: string;
  docDate: string;
  amount: number;
  description: string;
  bankRef?: string | null;
  partnerCode?: string | null;
  counterAccountId?: string | null;
  counterAccountCode?: string | null;
  periodId?: string | null;
  periodCode?: string | null;
  status: string;
  finJournalId?: string | null;
  finJournalCode?: string | null;
  postedAt?: string | null;
  transferRequestId?: string | null;
  note?: string | null;
};

export type FinBankTransferDto = {
  id: string;
  code: string;
  fromBankAccountId: string;
  fromBankAccountCode?: string | null;
  beneficiaryName: string;
  beneficiaryAccount: string;
  beneficiaryBank: string;
  amount: number;
  description: string;
  requestDate: string;
  counterAccountId?: string | null;
  counterAccountCode?: string | null;
  periodId?: string | null;
  periodCode?: string | null;
  status: string;
  executedVoucherId?: string | null;
  executedVoucherCode?: string | null;
  approvedAt?: string | null;
  note?: string | null;
};

export type FinBankStatementDto = {
  id: string;
  bankAccountId: string;
  bankAccountCode?: string | null;
  stmtDate: string;
  description: string;
  bankRef?: string | null;
  direction: string;
  amount: number;
  status: string;
  matchedVoucherId?: string | null;
  matchedVoucherCode?: string | null;
  matchedAt?: string | null;
  note?: string | null;
};

export type FinBankBookDto = {
  bankAccountId: string;
  bankAccountCode: string;
  bankAccountName: string;
  openingBalance: number;
  totalCredit: number;
  totalDebit: number;
  closingBalance: number;
  rows: {
    docDate: string;
    voucherCode: string;
    voucherType: string;
    description: string;
    bankRef?: string | null;
    credit: number;
    debit: number;
    balance: number;
  }[];
};

export async function fetchFinBankAccounts() {
  const { data } = await api.get<Envelope<FinBankAccountDto[]>>("/api/fin/bank-accounts");
  return data.data;
}

export async function upsertFinBankAccount(body: {
  id?: string | null;
  code: string;
  name: string;
  bankName: string;
  accountNumber: string;
  branchName?: string | null;
  glAccountId: string;
  openingBalance?: number;
  status?: string;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinBankAccountDto>>("/api/fin/bank-accounts", body);
  return data.data;
}

export async function fetchFinBankVouchers(params?: { bankAccountId?: string; type?: string }) {
  const { data } = await api.get<Envelope<FinBankVoucherDto[]>>("/api/fin/bank-vouchers", { params });
  return data.data;
}

export async function upsertFinBankVoucher(body: {
  id?: string | null;
  bankAccountId: string;
  voucherType: string;
  docDate: string;
  amount: number;
  description: string;
  bankRef?: string | null;
  partnerCode?: string | null;
  counterAccountId?: string | null;
  periodId?: string | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinBankVoucherDto>>("/api/fin/bank-vouchers", body);
  return data.data;
}

export async function postFinBankVoucher(id: string) {
  const { data } = await api.post<Envelope<FinBankVoucherDto>>(`/api/fin/bank-vouchers/${id}/post`);
  return data.data;
}

export async function voidFinBankVoucher(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinBankVoucherDto>>(`/api/fin/bank-vouchers/${id}/void`, { note });
  return data.data;
}

export async function fetchFinBankTransfers(params?: { bankAccountId?: string }) {
  const { data } = await api.get<Envelope<FinBankTransferDto[]>>("/api/fin/bank-transfers", { params });
  return data.data;
}

export async function upsertFinBankTransfer(body: {
  id?: string | null;
  fromBankAccountId: string;
  beneficiaryName: string;
  beneficiaryAccount: string;
  beneficiaryBank: string;
  amount: number;
  description: string;
  counterAccountId?: string | null;
  periodId?: string | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinBankTransferDto>>("/api/fin/bank-transfers", body);
  return data.data;
}

export async function submitFinBankTransfer(id: string) {
  const { data } = await api.post<Envelope<FinBankTransferDto>>(`/api/fin/bank-transfers/${id}/submit`);
  return data.data;
}

export async function approveFinBankTransfer(id: string) {
  const { data } = await api.post<Envelope<FinBankTransferDto>>(`/api/fin/bank-transfers/${id}/approve`);
  return data.data;
}

export async function rejectFinBankTransfer(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinBankTransferDto>>(`/api/fin/bank-transfers/${id}/reject`, { note });
  return data.data;
}

export async function executeFinBankTransfer(id: string) {
  const { data } = await api.post<Envelope<FinBankTransferDto>>(`/api/fin/bank-transfers/${id}/execute`);
  return data.data;
}

export async function voidFinBankTransfer(id: string, note?: string) {
  const { data } = await api.post<Envelope<FinBankTransferDto>>(`/api/fin/bank-transfers/${id}/void`, { note });
  return data.data;
}

export async function fetchFinBankStatements(params?: { bankAccountId?: string; status?: string }) {
  const { data } = await api.get<Envelope<FinBankStatementDto[]>>("/api/fin/bank-statements", { params });
  return data.data;
}

export async function upsertFinBankStatement(body: {
  id?: string | null;
  bankAccountId: string;
  stmtDate: string;
  description: string;
  bankRef?: string | null;
  direction: string;
  amount: number;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<FinBankStatementDto>>("/api/fin/bank-statements", body);
  return data.data;
}

export async function matchFinBankStatement(id: string, voucherId: string) {
  const { data } = await api.post<Envelope<FinBankStatementDto>>(`/api/fin/bank-statements/${id}/match`, { voucherId });
  return data.data;
}

export async function unmatchFinBankStatement(id: string) {
  const { data } = await api.post<Envelope<FinBankStatementDto>>(`/api/fin/bank-statements/${id}/unmatch`);
  return data.data;
}

export async function ignoreFinBankStatement(id: string) {
  const { data } = await api.post<Envelope<FinBankStatementDto>>(`/api/fin/bank-statements/${id}/ignore`);
  return data.data;
}

export async function fetchFinBankBook(bankAccountId: string) {
  const { data } = await api.get<Envelope<FinBankBookDto>>("/api/fin/bank-book", { params: { bankAccountId } });
  return data.data;
}
