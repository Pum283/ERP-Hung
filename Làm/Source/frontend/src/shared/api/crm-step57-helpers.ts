// crm-step57-helpers.ts
// Frontend helpers cho Bước 57:
//   UC_CRM_001 — Tạo khách hàng cá nhân (validateCrmCustomerInput, formatCustomerTypeLabel)
//   UC_CRM_002 — Tạo khách hàng doanh nghiệp (validateCrmCustomerInput)
//   UC_CRM_003 — Cập nhật thông tin khách hàng (validateCrmCustomerInput)
//   UC_CRM_004 — Kiểm tra trùng SĐT / MST (formatCustomerDuplicateAlert, normalizePhoneNumber)

export function normalizePhoneNumber(phone?: string): string {
  if (!phone) return '';
  let s = phone.trim().replace(/[\s\-\.\(\)]/g, '');
  if (s.startsWith('+84')) s = '0' + s.slice(3);
  else if (s.startsWith('84') && s.length >= 11) s = '0' + s.slice(2);
  return s;
}

export function validateCrmCustomerInput(input: {
  code: string;
  displayName: string;
  customerType: string;
  companyName?: string;
  phone?: string;
  email?: string;
  taxCode?: string;
}): { isValid: boolean; errors: string[] } {
  const errors: string[] = [];

  const code = (input.code || '').trim();
  if (!code || code.length > 40) {
    errors.push('Mã khách hàng là bắt buộc và tối đa 40 ký tự.');
  }

  const name = (input.displayName || '').trim();
  if (!name || name.length > 200) {
    errors.push('Tên hiển thị khách hàng là bắt buộc và tối đa 200 ký tự.');
  }

  const type = (input.customerType || '').trim();
  if (type !== 'Person' && type !== 'Organization') {
    errors.push('Loại khách hàng phải là "Person" (Cá nhân) hoặc "Organization" (Doanh nghiệp).');
  }

  if (type === 'Organization') {
    const compName = (input.companyName || '').trim();
    if (!compName && !name) {
      errors.push('Khách hàng doanh nghiệp yêu cầu tên công ty hoặc tên hiển thị.');
    }
  }

  if (input.email && input.email.trim()) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(input.email.trim())) {
      errors.push('Định dạng email không hợp lệ.');
    }
  }

  if (input.phone && input.phone.trim()) {
    const normPhone = normalizePhoneNumber(input.phone);
    if (!/^\d{9,11}$/.test(normPhone)) {
      errors.push('Số điện thoại phải chứa từ 9 đến 11 chữ số.');
    }
  }

  if (input.taxCode && input.taxCode.trim()) {
    const tax = input.taxCode.trim();
    if (!/^[0-9\-]{10,14}$/.test(tax)) {
      errors.push('Mã số thuế phải từ 10 đến 14 ký tự chữ số/gạch nối.');
    }
  }

  return {
    isValid: errors.length === 0,
    errors,
  };
}

export function formatCustomerTypeLabel(customerType: string): string {
  if (customerType === 'Organization') return '🏢 Khách hàng Doanh nghiệp';
  if (customerType === 'Person') return '👤 Khách hàng Cá nhân';
  return '❓ Không xác định';
}

export function formatCustomerDuplicateAlert(hasDuplicate: boolean, dupPhone: boolean, dupTaxCode: boolean): { isBlocked: boolean; alertMessage: string } {
  if (!hasDuplicate) {
    return { isBlocked: false, alertMessage: '✅ Không phát hiện trùng lặp SĐT hoặc MST.' };
  }

  const dupReasons: string[] = [];
  if (dupPhone) dupReasons.push('Số điện thoại');
  if (dupTaxCode) dupReasons.push('Mã số thuế (MST)');

  return {
    isBlocked: true,
    alertMessage: `⚠️ Cảnh báo trùng lặp dữ liệu khách hàng: Trùng ${dupReasons.join(' & ')}. Vui lòng kiểm tra lại hồ sơ gốc trước khi lưu!`,
  };
}
