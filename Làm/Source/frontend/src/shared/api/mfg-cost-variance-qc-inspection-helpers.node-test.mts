import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatVariancePercentage,
  getQcResultBadge,
} from './mfg-cost-variance-qc-inspection-helpers.ts';

test('formatVariancePercentage - formats positive and negative percentage', () => {
  assert.equal(formatVariancePercentage(15.5), '+15.5% Lệch');
  assert.equal(formatVariancePercentage(-4.2), '-4.2% Lệch');
});

test('getQcResultBadge - maps result badge styles', () => {
  const pass = getQcResultBadge('Pass');
  assert.equal(pass.label, 'Đạt Tiêu Chuẩn (Pass)');
  assert.match(pass.colorClass, /bg-emerald/);

  const fail = getQcResultBadge('Fail');
  assert.equal(fail.label, 'Không Đạt (Fail)');
  assert.match(fail.colorClass, /bg-rose/);
});
