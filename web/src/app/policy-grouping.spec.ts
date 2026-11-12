import { groupPoliciesByClient } from './policy-grouping';
import { PolicySummary } from './policy-models';

function makePolicy(overrides: Partial<PolicySummary>): PolicySummary {
  return {
    policyNumber: 'P-1001-01',
    clientId: 'C-1001',
    clientName: 'Northwind Traders',
    status: 'Active',
    outstandingBalance: 0,
    openClaimCount: 0,
    readiness: 'Ready',
    ...overrides,
  };
}

it('groupPoliciesByClient_two_clients_with_the_same_name_stay_separate', () => {
  const policies = [
    makePolicy({ policyNumber: 'P-1001-01', clientId: 'C-1001', clientName: 'Northwind Traders' }),
    makePolicy({ policyNumber: 'P-1042-01', clientId: 'C-1042', clientName: 'Northwind Traders' }),
  ];

  const groups = groupPoliciesByClient(policies);

  expect(groups.map((group) => group.clientId)).toEqual(['C-1001', 'C-1042']);
  expect(groups.map((group) => group.policies.map((policy) => policy.policyNumber))).toEqual([
    ['P-1001-01'],
    ['P-1042-01'],
  ]);
});

it('groupPoliciesByClient_one_client_with_differently_typed_names_forms_one_group', () => {
  const policies = [
    makePolicy({ policyNumber: 'P-1001-01', clientName: 'Northwind Traders' }),
    makePolicy({ policyNumber: 'P-1001-02', clientName: 'NORTHWIND TRADERS' }),
    makePolicy({ policyNumber: 'P-1001-03', clientName: 'Northwind Traders ' }),
  ];

  const groups = groupPoliciesByClient(policies);

  expect(groups.map((group) => group.clientId)).toEqual(['C-1001']);
  expect(groups.map((group) => group.policies.length)).toEqual([3]);
});

it('groupPoliciesByClient_client_name_is_the_first_policy_name_trimmed', () => {
  const policies = [
    makePolicy({ policyNumber: 'P-1001-01', clientName: '  Northwind Traders ' }),
    makePolicy({ policyNumber: 'P-1001-02', clientName: 'NORTHWIND TRADERS' }),
  ];

  const groups = groupPoliciesByClient(policies);

  expect(groups.map((group) => group.clientName)).toEqual(['Northwind Traders']);
});

it('groupPoliciesByClient_counts_come_from_the_readiness_field', () => {
  const policies = [
    makePolicy({ policyNumber: 'P-1001-01', readiness: 'Ready' }),
    // A credit (a balance below 0) does not block a policy.
    makePolicy({ policyNumber: 'P-1001-02', outstandingBalance: -25, readiness: 'Ready' }),
    makePolicy({ policyNumber: 'P-1001-03', status: 'Lapsed', readiness: 'Blocked' }),
    // Blocked for a reason not in PolicySummary, such as a missing contact email.
    makePolicy({ policyNumber: 'P-1001-04', readiness: 'Blocked' }),
    // Excluded because it expires before the cutover date.
    makePolicy({ policyNumber: 'P-1001-05', readiness: 'Excluded' }),
  ];

  const groups = groupPoliciesByClient(policies);
  const counts = groups.map((group) => [group.readyCount, group.blockedCount, group.excludedCount]);

  // [ready, blocked, excluded] for each group
  expect(counts).toEqual([[2, 2, 1]]);
});

it('groupPoliciesByClient_groups_are_sorted_by_name_ignoring_case', () => {
  const policies = [
    makePolicy({ policyNumber: 'P-1002-01', clientId: 'C-1002', clientName: 'Fabrikam Inc' }),
    makePolicy({ policyNumber: 'P-1001-01', clientId: 'C-1001', clientName: 'contoso ltd' }),
    makePolicy({ policyNumber: 'P-1003-01', clientId: 'C-1003', clientName: 'Adatum Corporation' }),
  ];

  const groups = groupPoliciesByClient(policies);

  expect(groups.map((group) => group.clientName)).toEqual([
    'Adatum Corporation',
    'contoso ltd',
    'Fabrikam Inc',
  ]);
});

it('groupPoliciesByClient_names_equal_ignoring_case_are_sorted_by_client_id', () => {
  const policies = [
    makePolicy({ policyNumber: 'P-1077-01', clientId: 'C-1077', clientName: 'northwind traders' }),
    makePolicy({ policyNumber: 'P-1042-01', clientId: 'C-1042', clientName: 'NORTHWIND TRADERS' }),
    makePolicy({ policyNumber: 'P-1001-01', clientId: 'C-1001', clientName: 'Northwind Traders' }),
  ];

  const groups = groupPoliciesByClient(policies);

  expect(groups.map((group) => group.clientId)).toEqual(['C-1001', 'C-1042', 'C-1077']);
});
