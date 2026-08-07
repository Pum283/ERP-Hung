import {
  canHoldStock,
  canLinkContract,
  canMergeOrders,
  canPushWarehouse,
  canReturnOrder,
  canSplitOrder,
  holdStatusTone,
  parseLogDeliveryRef,
  parseReservationRef,
  warehousePushTone,
} from "./crm-order-sync-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("canHoldStock", () => {
  it("true for Confirmed + None", () => {
    assert.equal(canHoldStock("Confirmed", "None"), true);
  });
  it("false when already Held", () => {
    assert.equal(canHoldStock("Holding", "Held"), false);
  });
  it("false for Cancelled / Delivered", () => {
    assert.equal(canHoldStock("Cancelled", "None"), false);
    assert.equal(canHoldStock("Delivered", "None"), false);
  });
  it("true again after Released", () => {
    assert.equal(canHoldStock("Confirmed", "Released"), true);
  });
});

describe("canPushWarehouse", () => {
  it("true for Confirmed + None", () => {
    assert.equal(canPushWarehouse("Confirmed", "None"), true);
  });
  it("false for Draft / Cancelled", () => {
    assert.equal(canPushWarehouse("Draft", "None"), false);
    assert.equal(canPushWarehouse("Cancelled", "None"), false);
  });
  it("false when already Pushed", () => {
    assert.equal(canPushWarehouse("Released", "Pushed"), false);
  });
  it("allows retry after Failed", () => {
    assert.equal(canPushWarehouse("Confirmed", "Failed"), true);
  });
});

describe("canReturnOrder", () => {
  it("true for Confirmed and Delivered", () => {
    assert.equal(canReturnOrder("Confirmed"), true);
    assert.equal(canReturnOrder("Delivered"), true);
  });
  it("false for Draft or Cancelled", () => {
    assert.equal(canReturnOrder("Draft"), false);
    assert.equal(canReturnOrder("Cancelled"), false);
  });
});

describe("canSplitOrder", () => {
  it("true for active order with >1 lines", () => {
    assert.equal(canSplitOrder("Confirmed", 3), true);
  });
  it("false if only 1 line or Delivered/Cancelled", () => {
    assert.equal(canSplitOrder("Confirmed", 1), false);
    assert.equal(canSplitOrder("Delivered", 3), false);
  });
});

describe("canMergeOrders", () => {
  it("true if both orders are active", () => {
    assert.equal(canMergeOrders("Draft", "Confirmed"), true);
  });
  it("false if any order is Delivered or Cancelled", () => {
    assert.equal(canMergeOrders("Delivered", "Draft"), false);
    assert.equal(canMergeOrders("Draft", "Cancelled"), false);
  });
});

describe("canLinkContract", () => {
  it("true for non-cancelled orders", () => {
    assert.equal(canLinkContract("Draft"), true);
    assert.equal(canLinkContract("Confirmed"), true);
    assert.equal(canLinkContract("Cancelled"), false);
  });
});

describe("tones", () => {
  it("holdStatusTone Held → brand, Failed → danger, None → muted", () => {
    assert.equal(holdStatusTone("Held"), "brand");
    assert.equal(holdStatusTone("Failed"), "danger");
    assert.equal(holdStatusTone("None"), "muted");
  });
  it("warehousePushTone Pushed → success, Failed → danger", () => {
    assert.equal(warehousePushTone("Pushed"), "success");
    assert.equal(warehousePushTone("Failed"), "danger");
    assert.equal(warehousePushTone("None"), "muted");
  });
});

describe("parseReservationRef", () => {
  it("extracts RV code from note", () => {
    assert.equal(
      parseReservationRef("Từ báo giá BG-01 · Giữ tồn RV-202608-0001 (2/2 dòng)"),
      "RV-202608-0001",
    );
  });
  it("null when absent", () => {
    assert.equal(parseReservationRef("ghi chú thường"), null);
    assert.equal(parseReservationRef(null), null);
  });
});

describe("parseLogDeliveryRef", () => {
  it("extracts DG code from note", () => {
    assert.equal(parseLogDeliveryRef("Giữ tồn RV-1 · LOG DG-202608-0003"), "DG-202608-0003");
  });
  it("null when absent", () => {
    assert.equal(parseLogDeliveryRef("note khác"), null);
    assert.equal(parseLogDeliveryRef(undefined), null);
  });
});

await run();
