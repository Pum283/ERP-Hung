import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLmsDocumentLessonInput,
  formatLmsCoursePublishStatus,
  validateLmsExamPassConfig,
  validateLmsOfflineClassInput,
} from './hrm-step49-helpers.ts';

// ─── UC_LMS_006: validateLmsDocumentLessonInput ───

test('validateLmsDocumentLessonInput - valid input returns valid', () => {
  const res = validateLmsDocumentLessonInput('Slide Giới Thiệu', 'https://cdn.erp.vn/slide.pdf');
  assert.equal(res.valid, true);
});

test('validateLmsDocumentLessonInput - missing content URL returns error', () => {
  const res = validateLmsDocumentLessonInput('Slide Giới Thiệu', '');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('URL tài liệu'));
});

// ─── UC_LMS_009: formatLmsCoursePublishStatus ───

test('formatLmsCoursePublishStatus - returns status text with icon', () => {
  assert.ok(formatLmsCoursePublishStatus('Published').includes('Đã xuất bản'));
  assert.ok(formatLmsCoursePublishStatus('Hidden').includes('Đã ẩn'));
});

// ─── UC_LMS_014: validateLmsExamPassConfig ───

test('validateLmsExamPassConfig - valid config returns valid', () => {
  const res = validateLmsExamPassConfig(80, 3);
  assert.equal(res.valid, true);
});

test('validateLmsExamPassConfig - invalid pass score returns error', () => {
  const res = validateLmsExamPassConfig(150, 3);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('khoảng từ 0 đến 100'));
});

// ─── UC_LMS_016: validateLmsOfflineClassInput ───

test('validateLmsOfflineClassInput - valid offline class input returns valid', () => {
  const res = validateLmsOfflineClassInput({
    code: 'CLS_01',
    name: 'Lớp C# K1',
    courseTitle: 'Khóa C# Pro',
    startDate: '2026-09-01',
    endDate: '2026-09-15',
    capacity: 30,
  });
  assert.equal(res.valid, true);
});
