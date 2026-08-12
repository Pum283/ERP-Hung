import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateStocktakeCreate,
  validateCountInput,
  calculateStocktakeVariance,
  validateStocktakeReview,
} from './inv-step101-helpers.ts';

test('UC_INV_049: validateStocktakeCreate', () => {
  const valid = validateStocktakeCreate('WH-01');
  assert.equal(valid.canCreate, true);

  const empty = validateStocktakeCreate('');
  assert.equal(empty.canCreate, false);
});

test('UC_INV_050: validateCountInput', () => {
  const valid = validateCountInput(15);
  assert.equal(valid.isValid, true);

  const negative = validateCountInput(-5);
  assert.equal(negative.isValid, false);
});

test('UC_INV_052: calculateStocktakeVariance', () => {
  const surplus = calculateStocktakeVariance(12, 10);
  assert.equal(surplus.varianceQty, 2);
  assert.equal(surplus.varianceType, 'Surplus');

  const shortage = calculateStocktakeVariance(8, 10);
  assert.equal(shortage.varianceQty, -2);
  assert.equal(shortage.varianceType, 'Shortage');
});

test('UC_INV_053: validateStocktakeReview', () => {
  const valid = validateStocktakeReview('Draft');
  assert.equal(valid.canReview, true);

  const reviewed = validateStocktakeReview('Reviewed');
  assert.equal(reviewed.canReview, false);
});
