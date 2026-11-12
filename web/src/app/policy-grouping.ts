import { ClientGroup, PolicySummary } from './policy-models';

export function groupPoliciesByClient(policies: PolicySummary[]): ClientGroup[] {
  const groups: Record<string, ClientGroup> = {};
  for (const policy of policies) {
    const key = policy.clientName;
    groups[key] ??= {
      clientId: policy.clientId,
      clientName: policy.clientName,
      policies: [],
      readyCount: 0,
      blockedCount: 0,
      excludedCount: 0,
    };
    const group = groups[key];
    group.policies.push(policy);
    if (policy.status === 'Cancelled') {
      group.excludedCount++;
    } else if (isPolicyReady(policy)) {
      group.readyCount++;
    } else {
      group.blockedCount++;
    }
  }
  return Object.values(groups).sort((a, b) => (a.clientName < b.clientName ? -1 : 1));
}

function isPolicyReady(policy: PolicySummary): boolean {
  return policy.outstandingBalance === 0 && policy.openClaimCount === 0;
}
