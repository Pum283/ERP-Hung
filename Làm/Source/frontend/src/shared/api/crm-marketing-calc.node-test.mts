/**
 * FE unit tests cho calcPromoDiscount — chạy bằng:
 * node --experimental-strip-types src/shared/api/crm-marketing-calc.node-test.mts
 */
import { calcPromoDiscount } from "./crm-marketing-calc.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("calcPromoDiscount", () => {
  it("returns 0 when subTotal below minOrderValue", () => {
    assert.equal(
      calcPromoDiscount({
        discountType: "Percentage",
        discountValue: 10,
        minOrderValue: 500_000,
        subTotal: 100_000,
      }),
      0,
    );
  });

  it("computes percentage discount", () => {
    assert.equal(
      calcPromoDiscount({ discountType: "Percentage", discountValue: 10, subTotal: 1_000_000 }),
      100_000,
    );
  });

  it("caps by maxDiscountAmount", () => {
    assert.equal(
      calcPromoDiscount({
        discountType: "Percentage",
        discountValue: 50,
        maxDiscountAmount: 80_000,
        subTotal: 1_000_000,
      }),
      80_000,
    );
  });

  it("applies fixed amount", () => {
    assert.equal(
      calcPromoDiscount({ discountType: "FixedAmount", discountValue: 25_000, subTotal: 200_000 }),
      25_000,
    );
  });

  it("does not exceed subTotal for fixed amount", () => {
    assert.equal(
      calcPromoDiscount({ discountType: "FixedAmount", discountValue: 500_000, subTotal: 120_000 }),
      120_000,
    );
  });

  it("returns 0 for FreeShipping", () => {
    assert.equal(
      calcPromoDiscount({ discountType: "FreeShipping", discountValue: 0, subTotal: 100_000 }),
      0,
    );
  });

  it("returns 0 for unknown type", () => {
    assert.equal(
      calcPromoDiscount({ discountType: "BuyXGetY", discountValue: 1, subTotal: 100_000 }),
      0,
    );
  });

  it("handles zero subTotal", () => {
    assert.equal(
      calcPromoDiscount({ discountType: "Percentage", discountValue: 10, subTotal: 0 }),
      0,
    );
  });

  it("allows exact minOrderValue", () => {
    assert.equal(
      calcPromoDiscount({
        discountType: "FixedAmount",
        discountValue: 10_000,
        minOrderValue: 100_000,
        subTotal: 100_000,
      }),
      10_000,
    );
  });

  it("rounds percentage to 2 decimals", () => {
    assert.equal(
      calcPromoDiscount({ discountType: "Percentage", discountValue: 15, subTotal: 10_000 }),
      1500,
    );
  });
});

await run();
