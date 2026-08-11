import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLmsCourseInput,
  formatLmsDeliveryMode,
  validateLmsChapterInput,
  validateLmsLessonVideoInput,
} from './hrm-step48-helpers.ts';

// ─── UC_LMS_002 & 003: validateLmsCourseInput & formatLmsDeliveryMode ───

test('validateLmsCourseInput - valid input returns valid', () => {
  const res = validateLmsCourseInput({ code: 'CRS_01', name: 'C# Pro', deliveryMode: 'Online', price: 1000000 });
  assert.equal(res.valid, true);
});

test('validateLmsCourseInput - invalid delivery mode returns error', () => {
  const res = validateLmsCourseInput({ code: 'CRS_01', name: 'C# Pro', deliveryMode: 'INVALID_MODE', price: 1000000 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Hình thức đào tạo'));
});

test('formatLmsDeliveryMode - returns label with icon', () => {
  assert.ok(formatLmsDeliveryMode('Online').includes('Trực tuyến'));
  assert.ok(formatLmsDeliveryMode('Blended').includes('Kết hợp'));
});

// ─── UC_LMS_004: validateLmsChapterInput ───

test('validateLmsChapterInput - valid title and order returns valid', () => {
  const res = validateLmsChapterInput('Chương 1: Tổng quan', 1);
  assert.equal(res.valid, true);
});

// ─── UC_LMS_005: validateLmsLessonVideoInput ───

test('validateLmsLessonVideoInput - valid video lesson returns valid', () => {
  const res = validateLmsLessonVideoInput({
    title: 'Bài 1: Hướng dẫn C#',
    lessonType: 'Video',
    videoUrl: 'https://cdn.erp.vn/video.mp4',
    durationMinutes: 20,
  });
  assert.equal(res.valid, true);
});

test('validateLmsLessonVideoInput - missing video URL for video lesson returns error', () => {
  const res = validateLmsLessonVideoInput({
    title: 'Bài 1: Hướng dẫn C#',
    lessonType: 'Video',
    videoUrl: '',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('URL video'));
});
