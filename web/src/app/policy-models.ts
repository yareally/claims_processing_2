export type PolicyStatus = 'Active' | 'Lapsed' | 'Cancelled';

export type ReadinessStatus = 'Ready' | 'Blocked' | 'Excluded';

export interface PolicySummary {
  policyNumber: string;
  clientId: string;
  clientName: string;
  status: PolicyStatus;
  outstandingBalance: number;
  openClaimCount: number;
  readiness: ReadinessStatus;
}

export interface PolicyPage {
  items: PolicySummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ClientGroup {
  clientId: string;
  clientName: string;
  policies: PolicySummary[];
  readyCount: number;
  blockedCount: number;
  excludedCount: number;
}
