export interface RawSession {
  id: string;
  sessionKey: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  lastSeenAt: string;
  expiresAt: string;
  isRevoked: boolean;
}

export function filterActiveSessions(sessions: RawSession[], now: Date = new Date()): RawSession[] {
  return sessions.filter((s) => !s.isRevoked && new Date(s.expiresAt) > now);
}

export function isSessionLimitReached(activeSessionsCount: number, limit: number = 5): boolean {
  return activeSessionsCount >= limit;
}
