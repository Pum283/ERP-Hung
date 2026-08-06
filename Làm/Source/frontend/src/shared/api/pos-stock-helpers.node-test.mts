/**
 * FE tests POS stock helpers — npm run test:pos
 */
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";
import {
  explodeBomNeed,
  rankPosStockAlert,
  summarizePosStockAlerts,
} from "./pos-stock-helpers.ts";

describe("rankPosStockAlert", () => {
  it("orders OutOfStock first", () => {
    assert.ok(rankPosStockAlert("OutOfStock") < rankPosStockAlert("BelowMin"));
    assert.ok(rankPosStockAlert("BelowMin") < rankPosStockAlert("NearReorder"));
  });
});

describe("summarizePosStockAlerts", () => {
  it("counts by type", () => {
    const s = summarizePosStockAlerts([
      { alertType: "OutOfStock" },
      { alertType: "BelowMin" },
      { alertType: "BelowMin" },
      { alertType: "NearReorder" },
    ]);
    assert.equal(s.outOfStock, 1);
    assert.equal(s.belowMin, 2);
    assert.equal(s.nearReorder, 1);
    assert.equal(s.total, 4);
  });

  it("handles empty", () => {
    assert.deepEqual(summarizePosStockAlerts([]), {
      outOfStock: 0, belowMin: 0, nearReorder: 0, total: 0,
    });
  });
});

describe("explodeBomNeed", () => {
  it("aggregates materials across lines", () => {
    const need = explodeBomNeed(
      [
        { productId: "p1", quantity: 2 },
        { productId: "p1", quantity: 1 },
        { productId: "p2", quantity: 3 },
      ],
      [
        { productId: "p1", materialCode: "NVL-A", qty: 0.5 },
        { productId: "p1", materialCode: "NVL-B", qty: 1 },
        { productId: "p2", materialCode: "nvl-a", qty: 1 },
      ],
    );
    assert.equal(need["NVL-A"], 4.5); // (2+1)*0.5 + 3*1
    assert.equal(need["NVL-B"], 3);
  });

  it("skips zero qty", () => {
    const need = explodeBomNeed(
      [{ productId: "p1", quantity: 0 }],
      [{ productId: "p1", materialCode: "X", qty: 2 }],
    );
    assert.deepEqual(need, {});
  });

  it("returns empty when no bom", () => {
    assert.deepEqual(explodeBomNeed([{ productId: "p1", quantity: 1 }], []), {});
  });
});

await run();
