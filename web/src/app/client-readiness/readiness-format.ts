import {
  IssueCount,
  PolicyReadiness,
  READINESS_ISSUE_ORDER,
  ReadinessIssue,
} from './readiness-models';

const ISSUE_SENTENCES: Record<ReadinessIssue, string> = {
  LapsedPolicy: 'The policy has lapsed. Reinstate or cancel it.',
  UnmappedProduct: 'The product has no match in the new system.',
  OutstandingBalance: 'The policy has an unpaid balance.',
  OpenClaims: 'The policy has open claims.',
  MissingContactEmail: 'The policyholder has no email address.',
};

export function describeIssue(issue: ReadinessIssue): string {
  return ISSUE_SENTENCES[issue];
}

export function countIssues(policies: PolicyReadiness[]): IssueCount[] {
  const counts = READINESS_ISSUE_ORDER.map((issue) => ({
    issue,
    count: policies.filter((policy) => policy.issues.includes(issue)).length,
  }));
  // sort keeps equal counts in their current order, which is READINESS_ISSUE_ORDER.
  return counts.filter((item) => item.count > 0).sort((a, b) => b.count - a.count);
}
