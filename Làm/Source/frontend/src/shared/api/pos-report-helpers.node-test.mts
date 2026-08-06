import {
  avgTicket,
  computeStoreShare,
  costVariance,
  paceStatus,
  rankTopProducts,
  targetAttainment,
  varianceTone,
} from "./pos-report-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("rankTopProducts", () => {
  const rows = [
    { code: "A", qty: 5, revenue: 100 },
    { code: "B", qty: 10, revenue: 50 },
    { code: "C", qty: 5, revenue: 200 },
  ];
  it("ranks by qty desc, tie-break revenue", () => {
    const r = rankTopProducts(rows, "qty");
    assert.deepEqual(r.map((x) => x.code), ["B", "C", "A"]);
    assert.equal(r[0].rank, 1);
    assert.equal(r[2].rank, 3);
  });
  it("ranks by revenue", () => {
    const r = rankTopProducts(rows, "revenue");
    assert.deepEqual(r.map((x) => x.code), ["C", "A", "B"]);
  });
  it("limits to top N", () => {
    assert.equal(rankTopProducts(rows, "qty", 2).length, 2);
  });
  it("handles empty", () => {
    assert.deepEqual(rankTopProducts([], "qty"), []);
  });
});

describe("computeStoreShare", () => {
  it("computes percentage share", () => {
    assert.deepEqual(computeStoreShare([{ revenue: 750 }, { revenue: 250 }]), [75, 25]);
  });
  it("zero total → zeros", () => {
    assert.deepEqual(computeStoreShare([{ revenue: 0 }, { revenue: 0 }]), [0, 0]);
  });
});

describe("avgTicket", () => {
  it("divides and rounds", () => {
    assert.equal(avgTicket(100_000, 3), 33333.33);
  });
  it("zero sales → 0", () => {
    assert.equal(avgTicket(100_000, 0), 0);
  });
});

describe("costVariance", () => {
  it("positive when actual over theoretical", () => {
    assert.deepEqual(costVariance(100_000, 110_000), { variance: 10_000, variancePercent: 10 });
  });
  it("negative when under", () => {
    assert.deepEqual(costVariance(100_000, 90_000), { variance: -10_000, variancePercent: -10 });
  });
  it("zero theoretical → pct 0", () => {
    assert.deepEqual(costVariance(0, 5_000), { variance: 5_000, variancePercent: 0 });
  });
});

describe("varianceTone", () => {
  it("danger when over 1%", () => {
    assert.equal(varianceTone(5), "danger");
  });
  it("success when under -1%", () => {
    assert.equal(varianceTone(-3), "success");
  });
  it("muted near zero", () => {
    assert.equal(varianceTone(0.5), "muted");
  });
});

describe("targetAttainment", () => {
  it("computes percent of target", () => {
    assert.equal(targetAttainment(50_000_000, 100_000_000), 50);
  });
  it("rounds to 2 decimals", () => {
    assert.equal(targetAttainment(1, 3), 33.33);
  });
  it("zero target → 0", () => {
    assert.equal(targetAttainment(50_000_000, 0), 0);
  });
});

describe("paceStatus", () => {
  it("ahead when attainment >= elapsed", () => {
    assert.equal(paceStatus(60, 50, true), "ahead");
  });
  it("on-track within 5 points", () => {
    assert.equal(paceStatus(46, 50, true), "on-track");
  });
  it("behind when gap > 5 points", () => {
    assert.equal(paceStatus(30, 50, true), "behind");
  });
  it("none without target", () => {
    assert.equal(paceStatus(0, 50, false), "none");
  });
});

await run();
