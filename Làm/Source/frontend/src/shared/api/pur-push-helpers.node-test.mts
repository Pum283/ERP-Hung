import {
  buildPoCsvFilename,
  canExportPo,
  canPushInvoiceToAp,
  formatApPushMessage,
  parseInvPushError,
  pushStatusTone,
} from "./pur-push-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("canPushInvoiceToAp", () => {
  it("true when matched, not pushed, total > 0", () => {
    assert.equal(canPushInvoiceToAp("Matched", "None", 100_000), true);
  });
  it("false when not matched", () => {
    assert.equal(canPushInvoiceToAp("Variance", "None", 100_000), false);
  });
  it("false when already pushed", () => {
    assert.equal(canPushInvoiceToAp("Matched", "Pushed", 100_000), false);
  });
  it("false when total 0", () => {
    assert.equal(canPushInvoiceToAp("Matched", "None", 0), false);
  });
  it("allows retry after Failed", () => {
    assert.equal(canPushInvoiceToAp("Matched", "Failed", 50_000), true);
  });
});

describe("formatApPushMessage", () => {
  it("includes code and formatted amount", () => {
    const msg = formatApPushMessage("VIN-202608-0001", 1_500_000);
    assert.ok(msg.includes("VIN-202608-0001"));
    assert.ok(msg.includes("FIN AP"));
  });
});

describe("pushStatusTone", () => {
  it("Pushed → success", () => {
    assert.equal(pushStatusTone("Pushed"), "success");
  });
  it("Failed → danger", () => {
    assert.equal(pushStatusTone("Failed"), "danger");
  });
  it("None → muted", () => {
    assert.equal(pushStatusTone("None"), "muted");
  });
});

describe("canExportPo", () => {
  it("false for Draft and Cancelled", () => {
    assert.equal(canExportPo("Draft"), false);
    assert.equal(canExportPo("Cancelled"), false);
  });
  it("true for Sent / Approved / Closed", () => {
    assert.equal(canExportPo("Sent"), true);
    assert.equal(canExportPo("Approved"), true);
    assert.equal(canExportPo("Closed"), true);
  });
});

describe("buildPoCsvFilename", () => {
  it("code + version", () => {
    assert.equal(buildPoCsvFilename("PO-001", 2), "PO-001-v2.csv");
  });
});

describe("parseInvPushError", () => {
  it("null when no note", () => {
    assert.equal(parseInvPushError(null), null);
    assert.equal(parseInvPushError(""), null);
  });
  it("null when note without error", () => {
    assert.equal(parseInvPushError("ghi chú thường"), null);
  });
  it("extracts message after INV lỗi:", () => {
    assert.equal(
      parseInvPushError("note gốc · INV lỗi: Chưa có kho Active để nhận hàng."),
      "Chưa có kho Active để nhận hàng.",
    );
  });
  it("takes the last error when appended repeatedly", () => {
    assert.equal(
      parseInvPushError("INV lỗi: lỗi cũ · INV lỗi: lỗi mới"),
      "lỗi mới",
    );
  });
});

await run();
