import {
  canInviteUser,
  channelTarget,
  formatForgotFlash,
  formatInviteFlash,
  isValidOtpFormat,
  loginModeTitle,
  preferMessageChannel,
} from "./sys-channel-helpers.ts";
import assert from "node:assert/strict";
import { describe, it, run } from "node:test";

describe("sys-channel prefer/target", () => {
  it("prefers email over phone", () => {
    assert.equal(preferMessageChannel("a@b.com", "0901"), "Email");
    assert.equal(preferMessageChannel(null, "0901"), "SMS");
    assert.equal(preferMessageChannel("", ""), null);
  });
  it("channelTarget", () => {
    assert.equal(channelTarget(" a@b.com ", "0901"), "a@b.com");
    assert.equal(channelTarget(null, " 0901 "), "0901");
    assert.equal(channelTarget(null, null), null);
  });
  it("canInviteUser", () => {
    assert.equal(canInviteUser("u", "a@b.com"), true);
    assert.equal(canInviteUser("u", null, "0901"), true);
    assert.equal(canInviteUser("", "a@b.com"), false);
    assert.equal(canInviteUser("u"), false);
  });
});

describe("sys-channel flash/otp/mode", () => {
  it("formatInviteFlash", () => {
    assert.ok(formatInviteFlash({ username: "u", channel: "Email", target: "a@b.com" }).includes("u"));
    assert.equal(
      formatInviteFlash({ username: "u", channel: "Email", target: "t", message: "OK" }),
      "OK",
    );
  });
  it("formatForgotFlash", () => {
    assert.ok(formatForgotFlash().toLowerCase().includes("otp"));
    assert.ok(!formatForgotFlash().toLowerCase().includes("stub log"));
  });
  it("isValidOtpFormat", () => {
    assert.equal(isValidOtpFormat("123456"), true);
    assert.equal(isValidOtpFormat("12345"), false);
    assert.equal(isValidOtpFormat("abcdef"), false);
  });
  it("loginModeTitle", () => {
    assert.equal(loginModeTitle("login"), "Đăng nhập");
    assert.equal(loginModeTitle("forgot"), "Quên mật khẩu");
    assert.equal(loginModeTitle("reset"), "Đặt lại mật khẩu");
  });
});

await run();
