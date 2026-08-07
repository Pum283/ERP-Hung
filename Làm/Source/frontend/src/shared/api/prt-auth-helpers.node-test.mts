import {
  formatPrtAccountStatus,
  formatPrtArSummaryText,
  isValidPrtResetToken,
} from "./prt-auth-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("prt-auth-helpers", () => {
  it("formatPrtAccountStatus", () => {
    assert.equal(formatPrtAccountStatus("Active"), "Hoạt động");
    assert.equal(formatPrtAccountStatus("pending"), "Chờ kích hoạt");
    assert.equal(formatPrtAccountStatus("Locked"), "Đã khóa");
  });

  it("isValidPrtResetToken", () => {
    assert.equal(isValidPrtResetToken("ABCDEF123"), true);
    assert.equal(isValidPrtResetToken("12345"), false);
    assert.equal(isValidPrtResetToken(""), false);
    assert.equal(isValidPrtResetToken(null), false);
  });

  it("formatPrtArSummaryText", () => {
    const text = formatPrtArSummaryText({ openAmount: 1500000, openInvoiceCount: 2, paidYtd: 3000000 });
    assert.ok(text.includes("Nợ chưa trả:"));
    assert.ok(text.includes("2 HĐ"));
  });
});

await run();
