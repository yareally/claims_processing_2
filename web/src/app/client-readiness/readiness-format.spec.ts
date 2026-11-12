import { countIssues, describeIssue } from './readiness-format';
import { PolicyReadiness, ReadinessIssue } from './readiness-models';

function makePolicy(issues: ReadinessIssue[]): PolicyReadiness {
  return { policyNumber: 'P-1001-01', status: issues.length === 0 ? 'Ready' : 'Blocked', issues };
}

it('describeIssue_returns_the_sentence_for_each_issue', () => {
  const issues: ReadinessIssue[] = [
    'LapsedPolicy',
    'UnmappedProduct',
    'OutstandingBalance',
    'OpenClaims',
    'MissingContactEmail',
  ];

  const sentences = issues.map((issue) => describeIssue(issue));

  expect(sentences).toEqual([
    'The policy has lapsed. Reinstate or cancel it.',
    'The product has no match in the new system.',
    'The policy has an unpaid balance.',
    'The policy has open claims.',
    'The policyholder has no email address.',
  ]);
});

it('countIssues_counts_the_policies_with_each_issue_highest_count_first', () => {
  const policies = [makePolicy(['OutstandingBalance', 'OpenClaims']), makePolicy(['OpenClaims'])];

  expect(countIssues(policies)).toEqual([
    { issue: 'OpenClaims', count: 2 },
    { issue: 'OutstandingBalance', count: 1 },
  ]);
});

it('countIssues_leaves_out_issues_that_never_appear', () => {
  const policies = [makePolicy(['LapsedPolicy']), makePolicy([])];

  expect(countIssues(policies)).toEqual([{ issue: 'LapsedPolicy', count: 1 }]);
});

it('countIssues_equal_counts_follow_the_issue_order', () => {
  // Each issue appears once. The input lists MissingContactEmail first, but the
  // issue order puts LapsedPolicy first and MissingContactEmail last.
  const policies = [
    makePolicy(['MissingContactEmail']),
    makePolicy(['LapsedPolicy', 'OpenClaims']),
  ];

  expect(countIssues(policies)).toEqual([
    { issue: 'LapsedPolicy', count: 1 },
    { issue: 'OpenClaims', count: 1 },
    { issue: 'MissingContactEmail', count: 1 },
  ]);
});

it('countIssues_empty_list_gives_an_empty_array', () => {
  expect(countIssues([])).toEqual([]);
});
