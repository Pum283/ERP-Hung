export type ScopeType = "Own" | "Team" | "Department" | "All";

export type MeResponse = {
  userId: string;
  tenantId: string;
  username: string;
  displayName?: string | null;
  email?: string | null;
  departmentId?: string | null;
  jobLevelId?: string | null;
  roles: string[];
  permissions: string[];
  effectiveScopeType: ScopeType;
  bypassDataScope: boolean;
  enabledModules: string[];
  tenantLogoUrl?: string | null;
  tenantName?: string | null;
};

export type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  userId: string;
  username: string;
  displayName?: string | null;
  roles: string[];
  permissions: string[];
  effectiveScopeType: ScopeType;
  bypassDataScope: boolean;
};
