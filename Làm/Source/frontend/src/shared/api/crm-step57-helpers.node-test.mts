import test from 'node:test';
import assert from 'node:assert/strict';
import {
  normalizePhoneNumber,
  validateCrmCustomerInput,
  formatCustomerTypeLabel,
  formatCustomerDuplicateAlert,
} from './crm-step57-helpers.ts';

// ─── UC_CRM_004: normalizePhoneNumber ───

test('normalizePhoneNumber - formats country code and removes special characters', () => {
  assert.equal(normalizePhoneNumber('+84 909 123 456'), '0909123456');
  assert.equal(normalizePhoneNumber('84909.123.456'), '0909123456');
  assert.equal(normalizePhoneNumber('0909-123-456'), '0909123456');
});

// ─── UC_CRM_001 & UC_CRM_002: validateCrmCustomerInput ───

test('validateCrmCustomerInput - valid person customer returns no errors', () => {
  const res = validateCrmCustomerInput({
    code: 'CUST_P001',
    displayName: 'Nguyễn Văn A',
    customerType: 'Person',
    phone: '0909123456',
    email: 'nva@gmail.com',
  });
  assert.equal(res.isValid, true);
  assert.equal(res.errors.length, 0);
});

test('validateCrmCustomerInput - corporate customer missing name returns error', () => {
  const res = validateCrmCustomerInput({
    code: 'CUST_O001',
    displayName: '',
    customerType: 'Organization',
    companyName: '',
  });
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('Tên hiển thị')));
});

test('validateCrmCustomerInput - invalid tax code and email returns validation errors', () => {
  const res = validateCrmCustomerInput({
    code: 'CUST_ERR',
    displayName: 'Công ty lỗi',
    customerType: 'Organization',
    companyName: 'Công ty lỗi',
    email: 'invalid-email-format',
    taxCode: '123',
  });
  assert.equal(res.isValid, false);
  assert.ok(res.errors.some(e => e.includes('email')));
  assert.ok(res.errors.some(e => e.includes('Mã số thuế')));
});

// ─── UC_CRM_001 & UC_CRM_002: formatCustomerTypeLabel ───

test('formatCustomerTypeLabel - returns correct icons and labels', () => {
  assert.ok(formatCustomerTypeLabel('Person').includes('Cá nhân'));
  assert.ok(formatCustomerTypeLabel('Organization').includes('Doanh nghiệp'));
});

// ─── UC_CRM_004: formatCustomerDuplicateAlert ───

test('formatCustomerDuplicateAlert - blocked when duplicate phone or tax code detected', () => {
  const alert = formatCustomerDuplicateAlert(true, true, true);
  assert.equal(alert.isBlocked, true);
  assert.ok(alert.alertMessage.includes('Số điện thoại'));
  assert.ok(alert.alertMessage.includes('Mã số thuế'));
});
