import assert from "node:assert/strict";
import test from "node:test";
import { filterActiveDevices, isValidDeviceFingerprint } from "./sys-device-helpers.ts";

test("filterActiveDevices filters out inactive and expired devices", () => {
  const now = new Date("2026-08-07T12:00:00Z");
  const devices = [
    { id: "1", deviceFingerprint: "fp1", deviceName: "MacBook", ipAddress: "127.0.0.1", lastUsedAt: "2026-08-07T10:00:00Z", expiresAt: "2026-09-01T00:00:00Z", isActive: true },
    { id: "2", deviceFingerprint: "fp2", deviceName: "iPhone", ipAddress: "127.0.0.1", lastUsedAt: "2026-08-07T10:00:00Z", expiresAt: "2026-08-01T00:00:00Z", isActive: true },
    { id: "3", deviceFingerprint: "fp3", deviceName: "PC", ipAddress: "127.0.0.1", lastUsedAt: "2026-08-07T10:00:00Z", expiresAt: "2026-09-01T00:00:00Z", isActive: false },
  ];

  const active = filterActiveDevices(devices, now);
  assert.equal(active.length, 1);
  assert.equal(active[0].id, "1");
});

test("isValidDeviceFingerprint validates length", () => {
  assert.equal(isValidDeviceFingerprint(""), false);
  assert.equal(isValidDeviceFingerprint("a"), false);
  assert.equal(isValidDeviceFingerprint("fp-12345"), true);
});
