import assert from "node:assert/strict";
import { describe, it } from "node:test";
import {
  canDownloadFile,
  formatScanStatusLabel,
  isInfectedScanStatus,
  isValidIpOrCidr,
  shouldDeliverInApp,
  validateBulkExportRequest,
  validateIpRuleForm,
  validateQuietHours,
} from "./sys-step154-helpers.ts";

describe("sys-step154 UC_SYS_064 prefs", () => {
  it("validates quiet hours", () => {
    assert.equal(validateQuietHours(null, null).isValid, true);
    assert.equal(validateQuietHours("22:00", null).isValid, false);
    assert.equal(validateQuietHours("22:00", "06:00").isValid, true);
    assert.equal(validateQuietHours("25:00", "06:00").isValid, false);
  });

  it("security events bypass mute", () => {
    assert.equal(
      shouldDeliverInApp({
        muteAll: true,
        channelInApp: false,
        eventType: "security.account_locked",
        utcMinutes: 0,
      }),
      true,
    );
  });

  it("quiet hours blocks normal events overnight", () => {
    assert.equal(
      shouldDeliverInApp({
        muteAll: false,
        channelInApp: true,
        quietHoursStart: "22:00",
        quietHoursEnd: "06:00",
        eventType: "wf.task.assigned",
        utcMinutes: 23 * 60,
      }),
      false,
    );
    assert.equal(
      shouldDeliverInApp({
        muteAll: false,
        channelInApp: true,
        quietHoursStart: "22:00",
        quietHoursEnd: "06:00",
        eventType: "wf.task.assigned",
        utcMinutes: 10 * 60,
      }),
      true,
    );
  });
});

describe("sys-step154 UC_SYS_071 scan", () => {
  it("detects infected status", () => {
    assert.equal(isInfectedScanStatus("Infected"), true);
    assert.equal(canDownloadFile("Infected").canDownload, false);
    assert.equal(canDownloadFile("Clean").canDownload, true);
  });

  it("formats scan labels", () => {
    assert.match(formatScanStatusLabel("clean"), /Sạch/);
  });
  it("blocks download while scanning", () => {
    assert.equal(canDownloadFile("Scanning").canDownload, false);
  });
});

describe("sys-step154 UC_SYS_077 bulk export", () => {
  it("validates entity set and format", () => {
    assert.equal(validateBulkExportRequest([], "Csv").isValid, false);
    assert.equal(validateBulkExportRequest(["Users"], "Xlsx").isValid, false);
    assert.equal(validateBulkExportRequest(["Users", "Files"], "Csv").isValid, true);
    assert.equal(validateBulkExportRequest(["Products"], "Csv").isValid, false);
  });

  it("rejects more than 10 entities", () => {
    const many = Array.from({ length: 11 }, (_, i) => (i === 0 ? "Users" : "Files"));
    // duplicate Files still count as length 11 before distinct — helper checks length first
    assert.equal(validateBulkExportRequest(many, "Csv").isValid, false);
  });
});

describe("sys-step154 UC_SYS_082 ip rules", () => {
  it("validates ip/cidr", () => {
    assert.equal(isValidIpOrCidr("10.0.0.1"), true);
    assert.equal(isValidIpOrCidr("10.0.0.0/8"), true);
    assert.equal(isValidIpOrCidr("10.0.0.0/99"), false);
    assert.equal(isValidIpOrCidr("abc"), false);
  });

  it("validates rule form", () => {
    assert.equal(validateIpRuleForm({ ipAddressOrCidr: "1.2.3.4", ruleType: "Allow" }).isValid, true);
    assert.equal(validateIpRuleForm({ ipAddressOrCidr: "1.2.3.4", ruleType: "Block" }).isValid, false);
  });
});
