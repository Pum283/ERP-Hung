export interface RawTrustedDevice {
  id: string;
  deviceFingerprint: string;
  deviceName: string;
  ipAddress: string;
  lastUsedAt: string;
  expiresAt: string;
  isActive: boolean;
}

export function filterActiveDevices(devices: RawTrustedDevice[], now: Date = new Date()): RawTrustedDevice[] {
  return devices.filter((d) => d.isActive && new Date(d.expiresAt) > now);
}

export function isValidDeviceFingerprint(fingerprint: string): boolean {
  return typeof fingerprint === "string" && fingerprint.trim().length >= 3;
}
