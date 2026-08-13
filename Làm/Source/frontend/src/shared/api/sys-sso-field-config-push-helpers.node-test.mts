import assert from "node:assert/strict";
import { describe, it } from "node:test";
import {
  applyFieldMaskUi,
  buildDevSsoCode,
  canRollbackVersion,
  formatPushPlatformLabel,
  isAllowedFieldAccess,
  mostPermissiveAccess,
  validateConfigKey,
  validatePushDevice,
  validateSsoProviderForm,
} from "./sys-sso-field-config-push-helpers.ts";

describe("sys-sso-field-config-push UC_SYS_009 SSO", () => {
  it("rejects empty IdP code", () => {
    const r = validateSsoProviderForm({
      code: " ",
      displayName: "Google",
      clientId: "c1",
      redirectUri: "http://localhost/cb",
    });
    assert.equal(r.isValid, false);
  });

  it("accepts valid IdP form", () => {
    assert.equal(
      validateSsoProviderForm({
        code: "GOOGLE",
        displayName: "Google",
        clientId: "c1",
        redirectUri: "http://localhost/cb",
      }).isValid,
      true,
    );
  });

  it("builds dev SSO code", () => {
    assert.equal(buildDevSsoCode("A@B.com", "sub-1"), "dev:a@b.com|sub-1");
  });
});

describe("sys-sso-field-config-push UC_SYS_031 field ACL", () => {
  it("validates access tokens", () => {
    assert.equal(isAllowedFieldAccess("Write"), true);
    assert.equal(isAllowedFieldAccess("foo"), false);
  });

  it("picks most permissive access", () => {
    assert.equal(mostPermissiveAccess(["None", "Read", "Masked"]), "Read");
    assert.equal(mostPermissiveAccess(["Masked", "Write"]), "Write");
    assert.equal(mostPermissiveAccess([]), "None");
  });

  it("masks sensitive values", () => {
    assert.equal(applyFieldMaskUi("1234567890", "Masked"), "12••••••90");
    assert.equal(applyFieldMaskUi("secret", "None"), "••••");
    assert.equal(applyFieldMaskUi("secret", "Read"), "secret");
  });
});

describe("sys-sso-field-config-push UC_SYS_058 config versions", () => {
  it("validates config key", () => {
    assert.equal(validateConfigKey(" ").isValid, false);
    assert.equal(validateConfigKey("password.policy").isValid, true);
  });

  it("blocks rollback to current or missing", () => {
    const versions = [
      { versionNumber: 2, isCurrent: true },
      { versionNumber: 1, isCurrent: false },
    ];
    assert.equal(canRollbackVersion(versions, 2).canRollback, false);
    assert.equal(canRollbackVersion(versions, 9).canRollback, false);
    assert.equal(canRollbackVersion(versions, 1).canRollback, true);
  });
});

describe("sys-sso-field-config-push UC_SYS_062 push", () => {
  it("validates device registration", () => {
    assert.equal(validatePushDevice("Fcm", "short").isValid, false);
    assert.equal(validatePushDevice("Fcm", "token-123456").isValid, true);
    assert.equal(validatePushDevice("X", "token-123456").isValid, false);
  });

  it("formats platform labels", () => {
    assert.match(formatPushPlatformLabel("fcm"), /Android/);
    assert.match(formatPushPlatformLabel("apns"), /iOS/);
  });
});
