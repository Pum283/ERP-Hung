import {
  buildReceiptFilename,
  buildShiftReportFilename,
  canPrintReceipt,
  formatCatalogSyncMessage,
} from "./pos-doc-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("formatCatalogSyncMessage", () => {
  it("only product count when nothing changed", () => {
    const msg = formatCatalogSyncMessage({
      productCount: 12, createdCount: 0, updatedCount: 0, suspendedCount: 0, syncedAt: "",
    });
    assert.equal(msg, "Đồng bộ INV→POS: 12 SP");
  });
  it("includes created and updated", () => {
    const msg = formatCatalogSyncMessage({
      productCount: 15, createdCount: 3, updatedCount: 2, suspendedCount: 0, syncedAt: "",
    });
    assert.ok(msg.includes("3 tạo mới"));
    assert.ok(msg.includes("2 cập nhật"));
  });
  it("includes suspended", () => {
    const msg = formatCatalogSyncMessage({
      productCount: 10, createdCount: 0, updatedCount: 1, suspendedCount: 1, syncedAt: "",
    });
    assert.ok(msg.includes("1 suspend"));
  });
});

describe("canPrintReceipt", () => {
  it("true for Paid and Returned", () => {
    assert.equal(canPrintReceipt("Paid"), true);
    assert.equal(canPrintReceipt("Returned"), true);
  });
  it("false for Open / Held / Cancelled", () => {
    assert.equal(canPrintReceipt("Open"), false);
    assert.equal(canPrintReceipt("Held"), false);
    assert.equal(canPrintReceipt("Cancelled"), false);
  });
});

describe("filenames", () => {
  it("receipt filename matches BE", () => {
    assert.equal(buildReceiptFilename("HD-001"), "HD-001-hoadon.txt");
  });
  it("shift report filename matches BE", () => {
    assert.equal(buildShiftReportFilename("CA-01"), "CA-01-baocao-ca.txt");
  });
});

await run();
