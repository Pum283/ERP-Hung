import assert from "node:assert/strict";
import test from "node:test";
import { getMfgShiftLabel, groupReceiptsByShift } from "./mfg-shift-helpers.ts";

test("getMfgShiftLabel returns correct shift for hour", () => {
  assert.equal(getMfgShiftLabel(7), "Ca 1 (06:00-14:00)");
  assert.equal(getMfgShiftLabel(15), "Ca 2 (14:00-22:00)");
  assert.equal(getMfgShiftLabel(23), "Ca 3 (22:00-06:00)");
  assert.equal(getMfgShiftLabel(2), "Ca 3 (22:00-06:00)");
});

test("groupReceiptsByShift groups receipts by day and shift", () => {
  const receipts = [
    { receivedAt: "2026-08-07T08:30:00Z", qty: 100, workshopId: "ws1", workshopCode: "WS01", workOrderId: "wo1" },
    { receivedAt: "2026-08-07T10:00:00Z", qty: 50, workshopId: "ws1", workshopCode: "WS01", workOrderId: "wo1" },
    { receivedAt: "2026-08-07T16:00:00Z", qty: 80, workshopId: "ws1", workshopCode: "WS01", workOrderId: "wo2" },
  ];

  const summary = groupReceiptsByShift(receipts);
  assert.equal(summary.length, 2);
  assert.equal(summary[0].qtyFg, 150);
  assert.equal(summary[0].receiptCount, 2);
});
