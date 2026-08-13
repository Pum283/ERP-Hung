import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateStudyReminder,
  formatForumTopicPreview,
  parseCertificateCode,
  evaluateCertificateStatus
} from './lms-engage-cert-helpers.ts';

test('validateStudyReminder - valid input passes', () => {
  const res = validateStudyReminder('daily', 'Hãy học bài hôm nay!');
  assert.equal(res.isValid, true);
  assert.equal(res.normalizedFreq, 'Daily');
});

test('validateStudyReminder - empty message fails', () => {
  const res = validateStudyReminder('Weekly', '   ');
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Nội dung nhắc học không được để trống.');
});

test('validateStudyReminder - invalid frequency defaults to Daily', () => {
  const res = validateStudyReminder('Monthly', 'Lưu nhắc nhở');
  assert.equal(res.isValid, true);
  assert.equal(res.normalizedFreq, 'Daily');
});

test('formatForumTopicPreview - short content does not truncate', () => {
  const res = formatForumTopicPreview('Hỏi đáp C#', 'Nội dung ngắn');
  assert.equal(res.isShort, true);
  assert.equal(res.preview, 'Nội dung ngắn');
});

test('formatForumTopicPreview - long content truncates with ellipsis', () => {
  const longContent = 'A'.repeat(100);
  const res = formatForumTopicPreview('Hỏi đáp C#', longContent, 50);
  assert.equal(res.isShort, false);
  assert.equal(res.preview.endsWith('...'), true);
  assert.equal(res.preview.length, 53);
});

test('parseCertificateCode - valid code pattern passes', () => {
  const res = parseCertificateCode('cert-2026-abc123');
  assert.equal(res.isValid, true);
  assert.equal(res.normalized, 'CERT-2026-ABC123');
});

test('parseCertificateCode - invalid code pattern fails', () => {
  const res = parseCertificateCode('INVALID_CODE_123');
  assert.equal(res.isValid, false);
});

test('evaluateCertificateStatus - active status returns success badge', () => {
  const res = evaluateCertificateStatus('Active');
  assert.equal(res.isValid, true);
  assert.equal(res.badgeColor, 'success');
  assert.equal(res.label, 'Có hiệu lực');
});

test('evaluateCertificateStatus - revoked status returns danger badge', () => {
  const res = evaluateCertificateStatus('Revoked');
  assert.equal(res.isValid, false);
  assert.equal(res.badgeColor, 'danger');
  assert.equal(res.label, 'Đã thu hồi');
});

test('evaluateCertificateStatus - unknown status returns warning badge', () => {
  const res = evaluateCertificateStatus('Unknown');
  assert.equal(res.isValid, false);
  assert.equal(res.badgeColor, 'warning');
});
