// hrm-contract-helpers.ts
// Frontend helpers cho Bước 15: UC_HRM_038 (Tạo hợp đồng lao động), UC_HRM_039 (Tạo phụ lục hợp đồng),
// UC_HRM_043 (Cảnh báo hết hạn hợp đồng), UC_HRM_046 (Lịch sử hợp đồng theo nhân sự)

export interface ContractForm {
  contractNo: string;
  contractType: 'Definite' | 'Indefinite' | 'Probation' | 'Seasonal' | 'Annex';
  startDate: string;
  endDate?: string | null;
  baseSalary?: number | null;
  parentContractId?: string | null;
}

export function validateContractForm(form: ContractForm): { valid: boolean; error?: string } {
  if (!form.contractNo || form.contractNo.trim().length === 0)
    return { valid: false, error: 'Số hợp đồng (ContractNo) không được để trống.' };

  if (!form.startDate || isNaN(Date.parse(form.startDate)))
    return { valid: false, error: 'Ngày bắt đầu hợp đồng không hợp lệ.' };

  if (form.contractType !== 'Indefinite' && (!form.endDate || form.endDate.trim().length === 0))
    return { valid: false, error: 'Hợp đồng có thời hạn bắt buộc phải có Ngày kết thúc.' };

  if (form.endDate && new Date(form.endDate) <= new Date(form.startDate))
    return { valid: false, error: 'Ngày kết thúc hợp đồng phải sau ngày bắt đầu.' };

  if (form.baseSalary !== undefined && form.baseSalary !== null && form.baseSalary < 0)
    return { valid: false, error: 'Lương cơ bản không được nhỏ hơn 0.' };

  return { valid: true };
}

export function calculateContractExpiringSeverity(daysRemaining: number): 'critical' | 'warning' | 'info' {
  if (daysRemaining <= 7) return 'critical';
  if (daysRemaining <= 15) return 'warning';
  return 'info';
}

export function generateSuggestedAnnexNo(parentContractNo: string, annexCount: number): string {
  const cleanNo = (parentContractNo || 'HD').trim().toUpperCase();
  return `${cleanNo}-PL${annexCount + 1}`;
}
