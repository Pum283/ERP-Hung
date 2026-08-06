import {
  canSyncPromoToPos,
  formatSyncToPosMessage,
  mapCrmDiscountToPos,
  rankUsageRows,
  summarizeVoucherUsageReport,
} from "./crm-promo-sync-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("mapCrmDiscountToPos", () => {
  it("maps Percentage → Percent", () => {
    assert.equal(mapCrmDiscountToPos("Percentage"), "Percent");
  });
  it("maps FixedAmount → Amount", () => {
    assert.equal(mapCrmDiscountToPos("FixedAmount"), "Amount");
  });
  it("rejects FreeShipping", () => {
    assert.equal(mapCrmDiscountToPos("FreeShipping"), null);
  });
  it("rejects BuyXGetY", () => {
    assert.equal(mapCrmDiscountToPos("BuyXGetY"), null);
  });
});

describe("canSyncPromoToPos", () => {
  it("true for Percentage > 0", () => {
    assert.equal(canSyncPromoToPos("Percentage", 10), true);
  });
  it("false when value 0", () => {
    assert.equal(canSyncPromoToPos("Percentage", 0), false);
  });
  it("false for FreeShipping", () => {
    assert.equal(canSyncPromoToPos("FreeShipping", 1), false);
  });
});

describe("formatSyncToPosMessage", () => {
  it("create wording", () => {
    assert.match(
      formatSyncToPosMessage({
        posPromotionCode: "PROMO-1",
        created: true,
        vouchersSynced: 3,
        vouchersSkipped: 1,
      }),
      /Tạo POS PROMO-1/,
    );
  });
  it("update wording", () => {
    assert.match(
      formatSyncToPosMessage({
        posPromotionCode: "PROMO-1",
        created: false,
        vouchersSynced: 0,
        vouchersSkipped: 2,
      }),
      /Cập nhật/,
    );
  });
});

describe("summarizeVoucherUsageReport", () => {
  it("aggregates empty", () => {
    assert.deepEqual(summarizeVoucherUsageReport([]), {
      voucherCount: 0,
      redeemTotal: 0,
      discountTotal: 0,
    });
  });
  it("sums redeem and discount", () => {
    const s = summarizeVoucherUsageReport([
      { redeemCount: 2, totalDiscount: 10_000.5 },
      { redeemCount: 1, totalDiscount: 5_000.25 },
    ]);
    assert.equal(s.voucherCount, 2);
    assert.equal(s.redeemTotal, 3);
    assert.equal(s.discountTotal, 15_000.75);
  });
});

describe("rankUsageRows", () => {
  it("sorts by redeem desc then code", () => {
    const ranked = rankUsageRows([
      { redeemCount: 1, voucherCode: "B" },
      { redeemCount: 3, voucherCode: "A" },
      { redeemCount: 1, voucherCode: "A" },
    ]);
    assert.deepEqual(
      ranked.map((x) => x.voucherCode),
      ["A", "A", "B"],
    );
    assert.equal(ranked[0].redeemCount, 3);
  });
});

await run();
