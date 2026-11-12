// These types match the JSON that GET /clients/{clientId}/readiness returns.
// They mirror the C# types in src/Readiness.Api/Readiness/ReadinessModels.cs.

export type ReadinessIssue =
  'LapsedPolicy' | 'UnmappedProduct' | 'OutstandingBalance' | 'OpenClaims' | 'MissingContactEmail';

// The order issues are reported in, the same as the C# ReadinessIssue enum.
export const READINESS_ISSUE_ORDER: readonly ReadinessIssue[] = [
  'LapsedPolicy',
  'UnmappedProduct',
  'OutstandingBalance',
  'OpenClaims',
  'MissingContactEmail',
];

export type PolicyReadinessStatus = 'Ready' | 'Blocked' | 'Excluded';

export interface PolicyReadiness {
  policyNumber: string;
  status: PolicyReadinessStatus;
  issues: ReadinessIssue[];
}

export interface ReadinessSummary {
  total: number;
  ready: number;
  blocked: number;
  excluded: number;
}

export interface ClientReadinessReport {
  clientId: string;
  clientName: string;
  // A date in the form yyyy-MM-dd, such as "2026-12-01".
  cutoverDate: string;
  isReady: boolean;
  summary: ReadinessSummary;
  policies: PolicyReadiness[];
}

// How many policies have one issue. countIssues returns these.
export interface IssueCount {
  issue: ReadinessIssue;
  count: number;
}
