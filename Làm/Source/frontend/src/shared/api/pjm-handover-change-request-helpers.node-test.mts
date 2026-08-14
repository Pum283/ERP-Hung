import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatEcrImpactSummary,
  formatAttachmentSize,
} from './pjm-handover-change-request-helpers.ts';

test('formatEcrImpactSummary - formats cost and days impact', () => {
  assert.equal(formatEcrImpactSummary(85000000, 5), '+85.000.000 đ (Gia hạn: +5 ngày)');
});

test('formatAttachmentSize - converts bytes to MB string', () => {
  assert.equal(formatAttachmentSize(2450000), '2.34 MB');
});
