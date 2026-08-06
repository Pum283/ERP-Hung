import {
  buildDepreciationJeCode,
  canPushDepreciationFin,
  depreciationFinLabel,
} from "./ast-fin-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("canPushDepreciationFin", () => {
  it("true when Posted run has amount and lines", () => {
    assert.equal(canPushDepreciationFin({ status: "Posted", lineCount: 3, totalAmount: 500 }), true);
  });
  it("false when already pushed with JE", () => {
    assert.equal(canPushDepreciationFin({ status: "Pushed", lineCount: 3, totalAmount: 500, finJournalId: "je1" }), false);
  });
  it("true for legacy stub Pushed without JE (re-push allowed)", () => {
    assert.equal(canPushDepreciationFin({ status: "Pushed", lineCount: 3, totalAmount: 500, finJournalId: null }), true);
  });
  it("false when run is empty", () => {
    assert.equal(canPushDepreciationFin({ status: "Posted", lineCount: 0, totalAmount: 0 }), false);
  });
});

describe("buildDepreciationJeCode", () => {
  it("matches BE convention JE-AST-{code}", () => {
    assert.equal(buildDepreciationJeCode("KH-2026-07"), "JE-AST-KH-2026-07");
  });
});

describe("depreciationFinLabel", () => {
  it("shows created JE label", () => {
    assert.ok(depreciationFinLabel({ status: "Pushed", lineCount: 1, totalAmount: 1, finJournalId: "je1" }).includes("Đã tạo JE FIN"));
  });
  it("marks legacy pushed without JE", () => {
    assert.ok(depreciationFinLabel({ status: "Pushed", lineCount: 1, totalAmount: 1, finJournalId: null }).includes("chưa có JE"));
  });
  it("default label when not pushed", () => {
    assert.equal(depreciationFinLabel({ status: "Posted", lineCount: 1, totalAmount: 1 }), "Chưa đẩy FIN");
  });
});

await run();
