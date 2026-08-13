import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateComplaintSeverityBadge,
  calculateReconciliationMatchRate,
  validateComplaintForm,
} from './crm-field-sales-ops-dispute-helpers.ts';

test('evaluateComplaintSeverityBadge - formats severity levels correctly', () => {
  const crit = evaluateComplaintSeverityBadge('critical');
  assert.equal(crit.label.includes('Rất nghiêm trọng'), true);
  assert.equal(crit.badgeClass.includes('rose'), true);

  const med = evaluateComplaintSeverityBadge('medium');
  assert.equal(med.label.includes('Trung bình'), true);
  assert.equal(med.badgeClass.includes('blue'), true);
});

test('calculateReconciliationMatchRate - calculates percentage rate correctly', () => {
  assert.equal(calculateReconciliationMatchRate(9, 10), 90);
  assert.equal(calculateReconciliationMatchRate(0, 0), 100);
});

test('validateComplaintForm - checks required parameters', () => {
  assert.equal(validateComplaintForm('', 'Lý do dài đủ').isValid, false);
  assert.equal(validateComplaintForm('ORD-123', 'Móp').isValid, false);
  assert.equal(validateComplaintForm('ORD-123', 'Hàng vỡ hộp khi giao').isValid, true);
});
