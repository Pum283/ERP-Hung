import {
  canPushMfgCost,
  formatAutoJournalFlash,
  formatMfgCostPushFlash,
  isAutoSource,
} from "./fin-journal-helpers.ts";
import {
  buildQuoteFilename,
  canSendQuote,
  formatQuoteSendFlash,
  parseQuoteSendLog,
} from "./crm-quote-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("isAutoSource / formatAutoJournalFlash", () => {
  it("detects Auto source", () => {
    assert.equal(isAutoSource("Auto"), true);
    assert.equal(isAutoSource("Manual"), false);
  });
  it("formats auto flash", () => {
    assert.ok(formatAutoJournalFlash("JE-1", "Auto").includes("Source=Auto"));
  });
});

describe("canPushMfgCost / formatMfgCostPushFlash", () => {
  it("only Calculated", () => {
    assert.equal(canPushMfgCost("Calculated"), true);
    assert.equal(canPushMfgCost("Pushed"), false);
    assert.equal(canPushMfgCost("Draft"), false);
  });
  it("flash includes FIN JE when present", () => {
    const msg = formatMfgCostPushFlash({ invSkuCode: "FG-01", finJournalCode: "JE-9", unitCost: 50000 });
    assert.ok(msg.includes("INV FG-01"));
    assert.ok(msg.includes("FIN JE-9"));
  });
  it("flash notes missing JE", () => {
    assert.ok(formatMfgCostPushFlash({ unitCost: 0 }).includes("chưa JE"));
  });
});

describe("crm quote helpers", () => {
  it("canSendQuote blocks converted/pending", () => {
    assert.equal(canSendQuote("Draft", "None"), true);
    assert.equal(canSendQuote("Converted", "None"), false);
    assert.equal(canSendQuote("Draft", "Pending"), false);
  });
  it("filename matches BE", () => {
    assert.equal(buildQuoteFilename("BG-001"), "BG-001-baogia.txt");
  });
  it("send flash differs by channel", () => {
    assert.ok(formatQuoteSendFlash("Email", "BG-1").includes("email"));
    assert.ok(formatQuoteSendFlash("Pdf", "BG-1").includes("PDF"));
  });
  it("parseQuoteSendLog extracts email", () => {
    assert.equal(parseQuoteSendLog("EMAIL→a@b.com @ 2026"), "Email → a@b.com");
    assert.equal(parseQuoteSendLog("PDF/TEXT BG-1"), "Đã xuất file text/PDF");
    assert.equal(parseQuoteSendLog(null), null);
  });
});

await run();
