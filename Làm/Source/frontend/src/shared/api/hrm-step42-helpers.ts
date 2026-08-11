// hrm-step42-helpers.ts
// Frontend helpers cho Bước 42:
//   UC_HRM_159 — Rule phụ cấp đặc thù (formatAllowanceRuleSummary)
//   UC_HRM_160 — Cấu hình bảo hiểm (calculateInsuranceDeductions & formatInsuranceRatesSummary)
//   UC_HRM_161 — Cấu hình thuế TNCN (calculatePersonalTax & validatePayrollPolicyInput)
//   UC_HRM_162 — Giảm trừ gia cảnh (calculatePersonalTax)

export interface PayrollPolicyInput {
  socialInsuranceEmpRate: number;
  healthInsuranceEmpRate: number;
  unemploymentEmpRate: number;
  personalDeduction: number;
  flatTaxRate: number;
  standardWorkDays: number;
  otMultiplier: number;
}

export function validatePayrollPolicyInput(p: PayrollPolicyInput): { valid: boolean; error?: string } {
  if (isNaN(p.standardWorkDays) || p.standardWorkDays < 1 || p.standardWorkDays > 31)
    return { valid: false, error: 'Ngày công chuẩn phải từ 1 đến 31 ngày.' };

  if (isNaN(p.otMultiplier) || p.otMultiplier < 1 || p.otMultiplier > 5)
    return { valid: false, error: 'Hệ số lương tăng ca (OT) phải từ 1.0 đến 5.0.' };

  if (isNaN(p.flatTaxRate) || p.flatTaxRate < 0 || p.flatTaxRate > 1)
    return { valid: false, error: 'Tỷ lệ thuế TNCN phẳng phải từ 0% đến 100% (0.0 - 1.0).' };

  if (isNaN(p.socialInsuranceEmpRate) || p.socialInsuranceEmpRate < 0 || p.socialInsuranceEmpRate > 1)
    return { valid: false, error: 'Tỷ lệ BHXH NLĐ đóng phải từ 0% đến 100% (0.0 - 1.0).' };

  if (isNaN(p.healthInsuranceEmpRate) || p.healthInsuranceEmpRate < 0 || p.healthInsuranceEmpRate > 1)
    return { valid: false, error: 'Tỷ lệ BHYT NLĐ đóng phải từ 0% đến 100% (0.0 - 1.0).' };

  if (isNaN(p.unemploymentEmpRate) || p.unemploymentEmpRate < 0 || p.unemploymentEmpRate > 1)
    return { valid: false, error: 'Tỷ lệ BHTN NLĐ đóng phải từ 0% đến 100% (0.0 - 1.0).' };

  if (isNaN(p.personalDeduction) || p.personalDeduction < 0)
    return { valid: false, error: 'Mức giảm trừ gia cảnh bản thân không được âm.' };

  return { valid: true };
}

export function calculateInsuranceDeductions(
  baseSalary: number,
  socialRate = 0.08,
  healthRate = 0.015,
  unempRate = 0.01
): { social: number; health: number; unemp: number; totalInsurance: number } {
  if (isNaN(baseSalary) || baseSalary <= 0) {
    return { social: 0, health: 0, unemp: 0, totalInsurance: 0 };
  }
  const social = Math.round(baseSalary * socialRate);
  const health = Math.round(baseSalary * healthRate);
  const unemp = Math.round(baseSalary * unempRate);
  const totalInsurance = social + health + unemp;
  return { social, health, unemp, totalInsurance };
}

export function calculatePersonalTax(
  grossPay: number,
  totalInsurance: number,
  personalDeduction = 11000000,
  flatTaxRate = 0.05
): { taxableIncome: number; taxAmount: number } {
  const taxableIncome = Math.max(0, grossPay - totalInsurance - personalDeduction);
  const taxAmount = Math.round(taxableIncome * flatTaxRate);
  return { taxableIncome, taxAmount };
}

export function formatInsuranceRatesSummary(socialRate: number, healthRate: number, unempRate: number): string {
  const totalPct = ((socialRate + healthRate + unempRate) * 100).toFixed(1);
  return `🛡️ Tổng tỷ lệ trích nộp BH người lao động: ${totalPct}% (BHXH ${socialRate * 100}%, BHYT ${healthRate * 100}%, BHTN ${unempRate * 100}%)`;
}
