import { countIssues } from './readiness-format';
import { PolicyReadiness, ReadinessIssue } from './readiness-models';

it('countIssues_equal_counts_follow_the_issue_order_for_all_five_issues', () => {
  const issueLists: ReadinessIssue[][] = [
    ['MissingContactEmail'],
    ['OpenClaims', 'MissingContactEmail'],
    ['OutstandingBalance'],
    ['UnmappedProduct', 'OpenClaims'],
    ['LapsedPolicy'],
  ];
  const policies: PolicyReadiness[] = issueLists.map((issues) => ({
    policyNumber: 'P-1001-01',
    status: 'Blocked',
    issues,
  }));

  expect(countIssues(policies)).toEqual([
    { issue: 'OpenClaims', count: 2 },
    { issue: 'MissingContactEmail', count: 2 },
    { issue: 'LapsedPolicy', count: 1 },
    { issue: 'UnmappedProduct', count: 1 },
    { issue: 'OutstandingBalance', count: 1 },
  ]);
});

it('countIssues_does_not_change_the_input', () => {
  // Both issue lists are out of the issue order and out of alphabetical order,
  // so sorting either list in place changes it.
  const policies: PolicyReadiness[] = [
    {
      policyNumber: 'P-1001-01',
      status: 'Blocked',
      issues: ['MissingContactEmail', 'LapsedPolicy'],
    },
    { policyNumber: 'P-1001-02', status: 'Blocked', issues: ['UnmappedProduct', 'LapsedPolicy'] },
  ];
  const before = structuredClone(policies);

  countIssues(policies);

  expect(policies).toEqual(before);
});
