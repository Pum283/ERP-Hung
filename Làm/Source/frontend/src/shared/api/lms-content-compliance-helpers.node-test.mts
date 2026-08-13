import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateVideoDownloadPermission,
  calculateSurveyScore,
  evaluateShiftTrainingGate,
} from './lms-content-compliance-helpers.ts';

test('evaluateVideoDownloadPermission - Learner blocked video download', () => {
  const res = evaluateVideoDownloadPermission(true, 'Learner', 'NguyenVanA');
  assert.equal(res.canDownload, false);
  assert.equal(res.watermarkText.includes('NGUYENVANA'), true);
  assert.equal(res.reason.includes('chặn tải video'), true);
});

test('evaluateVideoDownloadPermission - Admin allowed even if blocked', () => {
  const res = evaluateVideoDownloadPermission(true, 'Admin', 'AdminUser');
  assert.equal(res.canDownload, true);
  assert.equal(res.watermarkText, '');
});

test('calculateSurveyScore - passing score calculation', () => {
  const answers = { q1: 10, q2: 10, q3: 8 }; // total 28 out of 30
  const res = calculateSurveyScore(answers, 3, 70);
  assert.equal(res.scorePercentage, 93);
  assert.equal(res.isPass, true);
  assert.equal(res.gradeBadge, 'Đạt tiêu chuẩn');
});

test('calculateSurveyScore - failing score calculation', () => {
  const answers = { q1: 5, q2: 2, q3: 3 }; // total 10 out of 30 => 33%
  const res = calculateSurveyScore(answers, 3, 70);
  assert.equal(res.scorePercentage, 33);
  assert.equal(res.isPass, false);
  assert.equal(res.gradeBadge, 'Chưa đạt');
});

test('evaluateShiftTrainingGate - completed training permits work entry', () => {
  const res = evaluateShiftTrainingGate(true, '2026-08-13T08:00:00Z');
  assert.equal(res.canEnterWorkShift, true);
  assert.equal(res.gateStatus, 'Passed');
});

test('evaluateShiftTrainingGate - incomplete training near shift blocks work entry', () => {
  const shiftTime = '2026-08-13T08:00:00Z';
  const currentTime = '2026-08-13T07:45:00Z'; // 15 mins before shift
  const res = evaluateShiftTrainingGate(false, shiftTime, currentTime);
  assert.equal(res.canEnterWorkShift, false);
  assert.equal(res.gateStatus, 'Blocked');
  assert.equal(res.message.includes('CHẶN VÀO CA'), true);
});
