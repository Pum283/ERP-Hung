import assert from "node:assert/strict";
import test from "node:test";
import { filterActiveSessions, isSessionLimitReached } from "./sys-session-helpers.ts";

test("filterActiveSessions filters out revoked and expired sessions", () => {
  const now = new Date("2026-08-07T12:00:00Z");
  const sessions = [
    { id: "1", sessionKey: "k1", expiresAt: "2026-08-07T14:00:00Z", isRevoked: false, lastSeenAt: "2026-08-07T11:00:00Z" },
    { id: "2", sessionKey: "k2", expiresAt: "2026-08-07T10:00:00Z", isRevoked: false, lastSeenAt: "2026-08-07T09:00:00Z" },
    { id: "3", sessionKey: "k3", expiresAt: "2026-08-07T15:00:00Z", isRevoked: true, lastSeenAt: "2026-08-07T11:30:00Z" },
  ];

  const active = filterActiveSessions(sessions, now);
  assert.equal(active.length, 1);
  assert.equal(active[0].id, "1");
});

test("isSessionLimitReached correctly detects limit", () => {
  assert.equal(isSessionLimitReached(4, 5), false);
  assert.equal(isSessionLimitReached(5, 5), true);
  assert.equal(isSessionLimitReached(6, 5), true);
});
