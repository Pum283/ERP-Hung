import assert from "node:assert/strict";
import { describe, it } from "node:test";
import {
  canApproveWorkOrder,
  canCancelPlan,
  canConfirmPlan,
  canPrintWorkOrder,
  canReleaseWorkOrder,
  formatWorkOrderSlip,
  validatePlanSourceOrder,
  validateWorkOrderCreate,
} from "./mfg-step111-helpers.ts";

describe("mfg-step111-helpers UC_MFG_013", () => {
  it("rejects empty SO", () => {
    const r = validatePlanSourceOrder("  ");
    assert.equal(r.isValid, false);
  });

  it("accepts SO within 40 chars", () => {
    assert.equal(validatePlanSourceOrder("SO-001").isValid, true);
  });

  it("confirm requires Draft + lines", () => {
    assert.equal(canConfirmPlan("Draft", 0).canConfirm, false);
    assert.equal(canConfirmPlan("Confirmed", 2).canConfirm, false);
    assert.equal(canConfirmPlan("Draft", 1).canConfirm, true);
  });

  it("cancel blocked when linked WO exists", () => {
    assert.equal(canCancelPlan("Confirmed", 1).canCancel, false);
    assert.equal(canCancelPlan("Draft", 0).canCancel, true);
  });
});

describe("mfg-step111-helpers UC_MFG_017/018/019", () => {
  it("validate WO create with non-confirmed plan", () => {
    const r = validateWorkOrderCreate("item-1", 5, "plan-1", "Draft");
    assert.equal(r.isValid, false);
  });

  it("validate WO create qty and confirmed plan", () => {
    assert.equal(validateWorkOrderCreate("item-1", 0, null, null).isValid, false);
    assert.equal(validateWorkOrderCreate("item-1", 2, "plan-1", "Confirmed").isValid, true);
  });

  it("approve only Draft", () => {
    assert.equal(canApproveWorkOrder("Draft").canApprove, true);
    assert.equal(canApproveWorkOrder("Approved").canApprove, false);
  });

  it("release only Approved", () => {
    assert.equal(canReleaseWorkOrder("Approved").canRelease, true);
    assert.equal(canReleaseWorkOrder("Draft").canRelease, false);
  });

  it("print after Released", () => {
    assert.equal(canPrintWorkOrder("Released").canPrint, true);
    assert.equal(canPrintWorkOrder("MaterialsIssued").canPrint, true);
    assert.equal(canPrintWorkOrder("Draft").canPrint, false);
    assert.equal(canPrintWorkOrder("Approved").canPrint, false);
  });

  it("format slip contains code and qty", () => {
    const slip = formatWorkOrderSlip({
      code: "LSX-001",
      itemCode: "TP-1",
      itemName: "Áo",
      qty: 10,
      workshopName: "WS1",
      bomCode: "BOM-1",
      status: "Released",
    });
    assert.match(slip, /LSX-001/);
    assert.match(slip, /10/);
    assert.match(slip, /PHIẾU LỆNH SẢN XUẤT/);
  });
});
