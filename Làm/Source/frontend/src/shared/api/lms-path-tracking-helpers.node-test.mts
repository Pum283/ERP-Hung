import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateComplianceRatePct,
  evaluatePathProgress,
  filterLearningPathsByRole,
} from './lms-path-tracking-helpers.ts';

test('calculateComplianceRatePct - good compliance status', () => {
  const res = calculateComplianceRatePct(45, 50); // 90%
  assert.equal(res.complianceRatePct, 90);
  assert.equal(res.statusBadge, 'Good');
});

test('calculateComplianceRatePct - critical compliance status', () => {
  const res = calculateComplianceRatePct(15, 30); // 50%
  assert.equal(res.complianceRatePct, 50);
  assert.equal(res.statusBadge, 'Critical');
});

test('evaluatePathProgress - fully completed learning path', () => {
  const res = evaluatePathProgress(4, 4, '2026-08-30T00:00:00Z');
  assert.equal(res.progressPct, 100);
  assert.equal(res.isCompleted, true);
  assert.equal(res.isOverdue, false);
  assert.equal(res.statusText, 'Đã hoàn thành');
});

test('evaluatePathProgress - overdue learning path', () => {
  const dueDate = '2026-08-01T00:00:00Z';
  const currentTime = '2026-08-13T00:00:00Z';
  const res = evaluatePathProgress(2, 5, dueDate, currentTime);
  assert.equal(res.progressPct, 40);
  assert.equal(res.isCompleted, false);
  assert.equal(res.isOverdue, true);
  assert.equal(res.statusText, 'Quá hạn đào tạo');
});

test('filterLearningPathsByRole - filters by job title match', () => {
  const paths = [
    { title: 'Lộ trình Kho', jobTitle: 'Warehouse Staff' },
    { title: 'Lộ trình Backend', jobTitle: 'Backend Developer' },
  ];
  const filtered = filterLearningPathsByRole(paths, 'Warehouse');
  assert.equal(filtered.length, 1);
  assert.equal(filtered[0].jobTitle, 'Warehouse Staff');
});
