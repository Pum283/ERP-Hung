import assert from "node:assert/strict";
import { describe, it } from "node:test";
import {
  applyThemeCssVars,
  highlightSearchSnippet,
  isEffectivelyMuted,
  isValidHexColor,
  pickBestRoleHome,
  validateLandingPath,
  validateMessageSearchQuery,
  validateMuteUntil,
  validateThemeForm,
} from "./sys-theme-role-home-msg-helpers.ts";

describe("sys-theme-role-home-msg UC_SYS_093 theme", () => {
  it("validates hex colors", () => {
    assert.equal(isValidHexColor("#0ea5e9"), true);
    assert.equal(isValidHexColor("#fff"), true);
    assert.equal(isValidHexColor("red"), false);
    assert.equal(validateThemeForm({ primaryColor: "#111111", accentColor: "x" }).isValid, false);
  });

  it("builds css vars", () => {
    const v = applyThemeCssVars("#0EA5E9", "#F59E0B");
    assert.equal(v["--brand"], "#0EA5E9");
    assert.equal(v["--accent"], "#F59E0B");
    assert.ok(v["--brand-hover"]);
    assert.ok(v["--brand-muted"]);
    assert.ok(v["--brand-strong"]);
    assert.equal(v["--ring"], "#0EA5E9");
    assert.ok(v["--accent-muted"]);
  });
});

describe("sys-theme-role-home-msg UC_SYS_094 role home", () => {
  it("validates landing path", () => {
    assert.equal(validateLandingPath("/app/hrm").isValid, true);
    assert.equal(validateLandingPath("/evil").isValid, false);
  });

  it("picks lowest priority home", () => {
    const r = pickBestRoleHome(
      [
        { roleCode: "HR", landingPath: "/app/hrm", priority: 20, isActive: true },
        { roleCode: "FIN", landingPath: "/app/fin", priority: 5, isActive: true },
      ],
      ["HR", "FIN"],
    );
    assert.equal(r.landingPath, "/app/fin");
    assert.equal(r.matchedRoleCode, "FIN");
  });

  it("inactive homes are ignored", () => {
    const r = pickBestRoleHome(
      [{ roleCode: "HR", landingPath: "/app/hrm", priority: 1, isActive: false }],
      ["HR"],
    );
    assert.equal(r.landingPath, "/app");
  });

  it("defaults when no match", () => {
    assert.equal(pickBestRoleHome([], ["ADMIN"]).landingPath, "/app");
  });
});

describe("sys-theme-role-home-msg UC_SYS_103 search", () => {
  it("validates query length", () => {
    assert.equal(validateMessageSearchQuery("a").isValid, false);
    assert.equal(validateMessageSearchQuery("ab").isValid, true);
  });

  it("highlights snippet around hit", () => {
    const s = highlightSearchSnippet("please check invoice 99 now", "invoice");
    assert.match(s, /invoice/i);
  });
});

describe("sys-theme-role-home-msg UC_SYS_104 mute", () => {
  it("effective mute with until", () => {
    const future = new Date(Date.now() + 60_000).toISOString();
    const past = new Date(Date.now() - 60_000).toISOString();
    assert.equal(isEffectivelyMuted(true, future, Date.now()), true);
    assert.equal(isEffectivelyMuted(true, past, Date.now()), false);
    assert.equal(isEffectivelyMuted(false, null, Date.now()), false);
  });

  it("validates mute until", () => {
    assert.equal(validateMuteUntil(true, null).isValid, true);
    assert.equal(validateMuteUntil(true, new Date(Date.now() - 1000).toISOString()).isValid, false);
  });
});
