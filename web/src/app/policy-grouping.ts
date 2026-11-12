import { ClientGroup, PolicySummary } from './policy-models';

export function groupPoliciesByClient(policies: PolicySummary[]): ClientGroup[] {
  const groups = new Map<string, ClientGroup>();
  for (const policy of policies) {
    let group = groups.get(policy.clientId);
    if (group === undefined) {
      group = {
        clientId: policy.clientId,
        clientName: policy.clientName.trim(),
        policies: [],
        readyCount: 0,
        blockedCount: 0,
        excludedCount: 0,
      };
      groups.set(policy.clientId, group);
    }
    group.policies.push(policy);
    switch (policy.readiness) {
      case 'Ready':
        group.readyCount++;
        break;
      case 'Blocked':
        group.blockedCount++;
        break;
      case 'Excluded':
        group.excludedCount++;
        break;
    }
  }
  return [...groups.values()].sort(
    (a, b) =>
      a.clientName.localeCompare(b.clientName, undefined, { sensitivity: 'base' }) ||
      a.clientId.localeCompare(b.clientId),
  );
}
