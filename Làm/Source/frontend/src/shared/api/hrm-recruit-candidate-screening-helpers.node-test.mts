import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateCandidateForm,
  validateCvFile,
  validateScreenForm,
  isValidChannel,
  canScreen,
} from './hrm-recruit-candidate-screening-helpers.ts';

// ─── UC_HRM_055: isValidChannel ───

test('isValidChannel - LinkedIn is valid', () => {
  assert.equal(isValidChannel('LinkedIn'), true);
});

test('isValidChannel - TikTok is invalid', () => {
  assert.equal(isValidChannel('TikTok'), false);
});

// ─── UC_HRM_056: validateCandidateForm ───

test('validateCandidateForm - valid full data returns true', () => {
  const res = validateCandidateForm({
    jobPostingId: 'JP_001',
    fullName: 'Nguyễn Văn A',
    email: 'a@example.com',
    phone: '0901234567',
  });
  assert.equal(res.valid, true);
});

test('validateCandidateForm - empty name returns error', () => {
  const res = validateCandidateForm({ jobPostingId: 'JP_001', fullName: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Họ tên'));
});

test('validateCandidateForm - invalid email returns error', () => {
  const res = validateCandidateForm({
    jobPostingId: 'JP_001',
    fullName: 'Trần Thị B',
    email: 'not-an-email',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('email'));
});

test('validateCandidateForm - phone too short returns error', () => {
  const res = validateCandidateForm({
    jobPostingId: 'JP_001',
    fullName: 'Lê Văn C',
    phone: '123',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('điện thoại'));
});

test('validateCandidateForm - empty email/phone (optional) is valid', () => {
  const res = validateCandidateForm({ jobPostingId: 'JP_001', fullName: 'Phạm Thị D' });
  assert.equal(res.valid, true);
});

// ─── UC_HRM_057: validateCvFile ───

function makeFile(name: string, size: number, type: string): File {
  const buf = new Uint8Array(size);
  return new File([buf], name, { type });
}

test('validateCvFile - valid PDF under 10MB returns valid', () => {
  const f = makeFile('cv.pdf', 1024 * 1024, 'application/pdf');
  const res = validateCvFile(f);
  assert.equal(res.valid, true);
});

test('validateCvFile - exe file returns error', () => {
  const f = makeFile('malware.exe', 512, 'application/octet-stream');
  const res = validateCvFile(f);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('.pdf'));
});

test('validateCvFile - file over 10MB returns error', () => {
  const f = makeFile('big.pdf', 11 * 1024 * 1024, 'application/pdf');
  const res = validateCvFile(f);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('10MB'));
});

// ─── UC_HRM_059: validateScreenForm ───

test('validateScreenForm - valid Screen with note returns true', () => {
  const res = validateScreenForm({ action: 'Screen', screeningNote: 'CV phù hợp vị trí.' });
  assert.equal(res.valid, true);
});

test('validateScreenForm - valid ScreenReject with note returns true', () => {
  const res = validateScreenForm({ action: 'ScreenReject', screeningNote: 'Kinh nghiệm chưa đủ.' });
  assert.equal(res.valid, true);
});

test('validateScreenForm - Screen without note returns error', () => {
  const res = validateScreenForm({ action: 'Screen', screeningNote: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('ghi chú sơ loại'));
});

test('validateScreenForm - ScreenReject without note returns reject-specific error', () => {
  const res = validateScreenForm({ action: 'ScreenReject', screeningNote: '' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('từ chối'));
});

test('canScreen - New status returns true', () => {
  assert.equal(canScreen('New'), true);
});

test('canScreen - Accepted status returns false', () => {
  assert.equal(canScreen('Accepted'), false);
});
