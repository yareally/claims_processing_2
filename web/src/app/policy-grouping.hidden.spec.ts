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

it('groupPoliciesByClient_empty_input_gives_an_empty_array', () => {
  expect(groupPoliciesByClient([])).toEqual([]);
});

it('groupPoliciesByClient_policies_in_a_group_keep_their_input_order', () => {
  const policies = [
    makePolicy({ policyNumber: 'P-1001-03' }),
    makePolicy({ policyNumber: 'P-1001-01' }),
    makePolicy({ policyNumber: 'P-1001-02' }),
  ];

  const groups = groupPoliciesByClient(policies);

  expect(groups.map((group) => group.policies.map((policy) => policy.policyNumber))).toEqual([
    ['P-1001-03', 'P-1001-01', 'P-1001-02'],
  ]);
});

it('groupPoliciesByClient_does_not_change_the_input', () => {
  const policies = [
    makePolicy({ policyNumber: 'P-2001-01', clientId: 'C-2001', clientName: 'Contoso Ltd' }),
    makePolicy({
      policyNumber: 'P-1042-01',
      clientId: 'C-1042',
      clientName: ' Northwind Traders ',
    }),
    makePolicy({ policyNumber: 'P-1003-01', clientId: 'C-1003', clientName: 'Adatum Corporation' }),
  ];
  const before = policies.map((policy) => ({ ...policy }));

  groupPoliciesByClient(policies);

  expect(policies).toEqual(before);
});
