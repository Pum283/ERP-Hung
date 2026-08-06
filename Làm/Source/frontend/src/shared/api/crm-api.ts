import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type CrmCustomerDto = {
  id: string;
  code: string;
  customerType: string;
  displayName: string;
  companyName?: string | null;
  phone?: string | null;
  email?: string | null;
  taxCode?: string | null;
  segment: string;
  ownerUserId?: string | null;
  ownerName?: string | null;
  status: string;
  mergedIntoId?: string | null;
  address?: string | null;
  note?: string | null;
  potentialScore?: number | null;
  contactCount: number;
};

export type CrmContactDto = {
  id: string;
  customerId: string;
  fullName: string;
  title?: string | null;
  phone?: string | null;
  email?: string | null;
  isPrimary: boolean;
};

export type CrmHandoverDto = {
  id: string;
  customerId: string;
  fromUserId?: string | null;
  fromUserName?: string | null;
  toUserId: string;
  toUserName?: string | null;
  note?: string | null;
  handedAt: string;
};

export type CrmDuplicateHitDto = {
  id: string;
  code: string;
  displayName: string;
  phone?: string | null;
  taxCode?: string | null;
  matchField: string;
};

export type CrmCustomer360Dto = {
  customer: CrmCustomerDto;
  contacts: CrmContactDto[];
  handovers: CrmHandoverDto[];
  possibleDuplicates: CrmDuplicateHitDto[];
};

export type CrmImportResult = {
  total: number;
  success: number;
  failed: number;
  rows: { code: string; ok: boolean; message: string }[];
};

export async function searchCrmCustomers(params: {
  q?: string;
  customerType?: string;
  segment?: string;
  status?: string;
  ownerUserId?: string;
  phone?: string;
  taxCode?: string;
}) {
  const { data } = await api.get<Envelope<CrmCustomerDto[]>>("/api/crm/customers", { params });
  return data.data;
}

export async function upsertCrmCustomer(body: {
  id?: string;
  code: string;
  customerType: string;
  displayName: string;
  companyName?: string;
  phone?: string;
  email?: string;
  taxCode?: string;
  segment?: string;
  ownerUserId?: string | null;
  address?: string;
  note?: string;
  potentialScore?: number | null;
  status?: string;
}) {
  const { data } = await api.post<Envelope<CrmCustomerDto>>("/api/crm/customers", body);
  return data.data;
}

export async function fetchCrmCustomer360(id: string) {
  const { data } = await api.get<Envelope<CrmCustomer360Dto>>(`/api/crm/customers/${id}`);
  return data.data;
}

export async function findCrmDuplicates(phone?: string, taxCode?: string, excludeId?: string) {
  const { data } = await api.get<Envelope<CrmDuplicateHitDto[]>>("/api/crm/customers/duplicates", {
    params: { phone, taxCode, excludeId },
  });
  return data.data;
}

export async function assignCrmOwner(customerId: string, ownerUserId: string) {
  const { data } = await api.post<Envelope<CrmCustomerDto>>(
    `/api/crm/customers/${customerId}/assign-owner`,
    { ownerUserId },
  );
  return data.data;
}

export async function handoverCrmCustomer(customerId: string, toUserId: string, note?: string) {
  const { data } = await api.post<Envelope<CrmHandoverDto>>(
    `/api/crm/customers/${customerId}/handover`,
    { toUserId, note },
  );
  return data.data;
}

export async function mergeCrmCustomers(sourceCustomerId: string, targetCustomerId: string) {
  const { data } = await api.post<Envelope<CrmCustomerDto>>("/api/crm/customers/merge", {
    sourceCustomerId,
    targetCustomerId,
  });
  return data.data;
}

export async function upsertCrmContact(
  customerId: string,
  body: {
    id?: string;
    fullName: string;
    title?: string;
    phone?: string;
    email?: string;
    isPrimary?: boolean;
  },
) {
  const { data } = await api.post<Envelope<CrmContactDto>>(
    `/api/crm/customers/${customerId}/contacts`,
    body,
  );
  return data.data;
}

export async function importCrmCustomers(csvText: string) {
  const { data } = await api.post<Envelope<CrmImportResult>>("/api/crm/customers/import", {
    csvText,
  });
  return data.data;
}

export async function downloadCrmCustomersCsv() {
  const res = await api.get("/api/crm/customers/export.csv", { responseType: "blob" });
  return res.data as Blob;
}
