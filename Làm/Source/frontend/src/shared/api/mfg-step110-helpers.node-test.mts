import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateWorkshopUpsert,
  validateBomCreate,
  validateBomActivation,
  validateBomLineUpsert,
} from './mfg-step110-helpers.ts';

test('UC_MFG_003: validateWorkshopUpsert', () => {
  const valid = validateWorkshopUpsert('WS-01', 'Xưởng May 1', 'Workshop');
  assert.equal(valid.isValid, true);

  const invalidType = validateWorkshopUpsert('WS-02', 'Dây Chuyền 2', 'InvalidType');
  assert.equal(invalidType.isValid, false);
});

test('UC_MFG_006: validateBomCreate', () => {
  const valid = validateBomCreate('PARENT-ID-01', '1.0');
  assert.equal(valid.canCreate, true);

  const noVersion = validateBomCreate('PARENT-ID-01', '');
  assert.equal(noVersion.canCreate, false);
});

test('UC_MFG_007: validateBomActivation', () => {
  const valid = validateBomActivation('Draft', 3);
  assert.equal(valid.canActivate, true);

  const emptyLines = validateBomActivation('Draft', 0);
  assert.equal(emptyLines.canActivate, false);
  assert.match(emptyLines.reason!, /ít nhất 1 dòng/);
});

test('UC_MFG_008: validateBomLineUpsert', () => {
  const valid = validateBomLineUpsert('COMP-ID-01', 'PARENT-ID-01', 2.5);
  assert.equal(valid.isValid, true);

  const selfRef = validateBomLineUpsert('PARENT-ID-01', 'PARENT-ID-01', 1);
  assert.equal(selfRef.isValid, false);
  assert.match(selfRef.error!, /tự tham chiếu/);
});
