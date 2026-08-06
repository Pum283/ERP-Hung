import {
  finSyncCoveragePct,
  formatFinSyncFlash,
  isFinSyncComplete,
  parseFinSyncFromNote,
} from "./pos-shift-fin-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("parseFinSyncFromNote", () => {
  it("returns null for empty", () => {
    assert.equal(parseFinSyncFromNote(null), null);
    assert.equal(parseFinSyncFromNote(""), null);
  });
  it("parses tag in note", () => {
    assert.deepEqual(parseFinSyncFromNote("cash ok | FIN:2+1/3 fail=0"), {
      synced: 2,
      already: 1,
      paid: 3,
      failed: 0,
    });
  });
  it("returns null when no tag", () => {
    assert.equal(parseFinSyncFromNote("chỉ variance"), null);
  });
});

describe("formatFinSyncFlash", () => {
  it("includes counts", () => {
    const s = formatFinSyncFlash({
      syncedCount: 2,
      alreadyHadCount: 1,
      paidSaleCount: 3,
      failedCount: 0,
    });
    assert.match(s, /2 mới/);
    assert.match(s, /3 Paid/);
  });
});

describe("isFinSyncComplete", () => {
  it("true when all covered and no fail", () => {
    assert.equal(
      isFinSyncComplete({
        paidSaleCount: 3,
        syncedCount: 2,
        alreadyHadCount: 1,
        failedCount: 0,
      }),
      true,
    );
  });
  it("false when fail", () => {
    assert.equal(
      isFinSyncComplete({
        paidSaleCount: 2,
        syncedCount: 1,
        alreadyHadCount: 0,
        failedCount: 1,
      }),
      false,
    );
  });
  it("true for zero paid", () => {
    assert.equal(
      isFinSyncComplete({
        paidSaleCount: 0,
        syncedCount: 0,
        alreadyHadCount: 0,
        failedCount: 0,
      }),
      true,
    );
  });
});

describe("finSyncCoveragePct", () => {
  it("100 when no paid", () => {
    assert.equal(finSyncCoveragePct({ paidSaleCount: 0, syncedCount: 0, alreadyHadCount: 0 }), 100);
  });
  it("rounds coverage", () => {
    assert.equal(finSyncCoveragePct({ paidSaleCount: 3, syncedCount: 1, alreadyHadCount: 1 }), 67);
  });
});

await run();
