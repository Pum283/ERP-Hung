import {
  canShowFinJe,
  expectedCounterPrefixes,
  finJePostedLabel,
} from "./fin-je-helpers.ts";
import {
  canAutoIntake,
  formatAutoIntakeFlash,
  formatDeviceSyncFlash,
} from "./crm-hrm-intake-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("fin-je-helpers", () => {
  it("labels JE code", () => {
    assert.ok(finJePostedLabel("JE-1").includes("JE-1"));
    assert.ok(finJePostedLabel(null).includes("chưa có mã JE"));
  });
  it("canShowFinJe", () => {
    assert.equal(canShowFinJe("x"), true);
    assert.equal(canShowFinJe(null), false);
  });
  it("expected prefixes for cash receipt / ar", () => {
    assert.deepEqual(expectedCounterPrefixes("cash-receipt"), ["131", "511"]);
    assert.ok(expectedCounterPrefixes("ap").includes("331"));
  });
});

describe("crm-hrm-intake-helpers", () => {
  it("canAutoIntake requires contact", () => {
    assert.equal(canAutoIntake("A", "0901"), true);
    assert.equal(canAutoIntake("A", null, "a@b.com"), true);
    assert.equal(canAutoIntake("A"), false);
    assert.equal(canAutoIntake("", "0901"), false);
  });
  it("formatAutoIntakeFlash", () => {
    assert.ok(formatAutoIntakeFlash({ code: "L1", isReintake: true }).includes("Re-intake"));
    assert.ok(formatAutoIntakeFlash({ code: "L1", ownerName: "An" }).includes("An"));
  });
  it("formatDeviceSyncFlash", () => {
    const msg = formatDeviceSyncFlash({
      synced: 2, skippedUnknownEmployee: 1, skippedLocked: 0,
      skippedDuplicate: 1, skippedInvalidType: 0, total: 4,
    });
    assert.ok(msg.includes("2/4"));
    assert.ok(msg.includes("NV lạ"));
    assert.ok(msg.includes("trùng"));
  });
});

await run();
