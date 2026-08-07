import {
  biExportActionLabel,
  biExportDownloadName,
  buildBiReportFilterJson,
  canDownloadBiExport,
  formatDatasetRefreshFlash,
  isLiveBiMetric,
  widgetValueHint,
} from "./bi-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("bi-helpers refresh/widget", () => {
  it("formatDatasetRefreshFlash includes rows + note", () => {
    const msg = formatDatasetRefreshFlash({
      rowCountEstimate: 12,
      lastRefreshNote: "Refresh từ CRM.CrmSalesOrders",
      status: "Ready",
    });
    assert.ok(msg.includes("12"));
    assert.ok(msg.includes("CRM.CrmSalesOrders"));
  });
  it("formatDatasetRefreshFlash falls back to status", () => {
    const msg = formatDatasetRefreshFlash({ rowCountEstimate: 0, status: "Ready" });
    assert.ok(msg.includes("Ready"));
  });
  it("isLiveBiMetric", () => {
    assert.equal(isLiveBiMetric("Revenue"), true);
    assert.equal(isLiveBiMetric("Profit"), true);
    assert.equal(isLiveBiMetric("Custom"), false);
  });
  it("widgetValueHint", () => {
    assert.equal(widgetValueHint("Revenue"), "Live FIN");
    assert.equal(widgetValueHint("Custom"), "Custom");
  });
});

describe("bi-helpers run/export", () => {
  it("buildBiReportFilterJson", () => {
    const j = JSON.parse(buildBiReportFilterJson("2026-01-01", "2026-01-31"));
    assert.equal(j.from, "2026-01-01");
    assert.equal(j.to, "2026-01-31");
    const empty = JSON.parse(buildBiReportFilterJson("", ""));
    assert.equal(empty.from, null);
    assert.equal(empty.to, null);
  });
  it("canDownloadBiExport", () => {
    assert.equal(canDownloadBiExport("Excel"), true);
    assert.equal(canDownloadBiExport("Pdf"), true);
    assert.equal(canDownloadBiExport("None"), false);
    assert.equal(canDownloadBiExport(null), false);
  });
  it("biExportDownloadName prefers BE filename", () => {
    assert.equal(biExportDownloadName("RPT.csv", "X", "Excel"), "RPT.csv");
    assert.equal(biExportDownloadName(null, "SALES", "Excel"), "SALES.csv");
    assert.equal(biExportDownloadName(null, "SALES", "Pdf"), "SALES.txt");
  });
  it("biExportActionLabel", () => {
    assert.ok(biExportActionLabel("Excel").includes("CSV"));
    assert.ok(biExportActionLabel("Pdf").includes("text"));
    assert.ok(!biExportActionLabel("Excel").toLowerCase().includes("stub"));
  });
});

await run();
