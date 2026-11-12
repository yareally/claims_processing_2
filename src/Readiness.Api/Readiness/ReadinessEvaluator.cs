namespace Readiness.Api.Readiness;

public class ReadinessEvaluator
{
    private readonly IProductCatalog _catalog;

    public ReadinessEvaluator(IProductCatalog catalog)
    {
        _catalog = catalog;
    }

    public PolicyReadiness EvaluatePolicy(PolicyRecord policy, DateOnly cutoverDate)
    {
        if (policy.Status == PolicyStatus.Cancelled || policy.ExpirationDate < cutoverDate)
        {
            return new PolicyReadiness(policy.PolicyNumber, PolicyReadinessStatus.Excluded, []);
        }

        var issues = new List<ReadinessIssue>();
        if (policy.Status == PolicyStatus.Lapsed)
        {
            issues.Add(ReadinessIssue.LapsedPolicy);
        }
        if (!_catalog.IsMapped(policy.LegacyProductCode))
        {
            issues.Add(ReadinessIssue.UnmappedProduct);
        }
        if (policy.OutstandingBalance > 0)
        {
            issues.Add(ReadinessIssue.OutstandingBalance);
        }
        if (policy.OpenClaimCount > 0)
        {
            issues.Add(ReadinessIssue.OpenClaims);
        }
        if (string.IsNullOrWhiteSpace(policy.PolicyholderEmail))
        {
            issues.Add(ReadinessIssue.MissingContactEmail);
        }

        var status = issues.Count == 0 ? PolicyReadinessStatus.Ready : PolicyReadinessStatus.Blocked;
        return new PolicyReadiness(policy.PolicyNumber, status, issues);
    }

    public ClientReadinessReport EvaluateClient(Client client, IReadOnlyList<PolicyRecord> policies, DateOnly cutoverDate)
    {
        var results = policies
            .Select(policy => EvaluatePolicy(policy, cutoverDate))
            .OrderBy(result => result.PolicyNumber, StringComparer.Ordinal)
            .ToList();

        var summary = new ReadinessSummary(
            Total: results.Count,
            Ready: results.Count(result => result.Status == PolicyReadinessStatus.Ready),
            Blocked: results.Count(result => result.Status == PolicyReadinessStatus.Blocked),
            Excluded: results.Count(result => result.Status == PolicyReadinessStatus.Excluded));

        var isReady = summary.Blocked == 0 && summary.Ready > 0;
        return new ClientReadinessReport(client.ClientId, client.Name, cutoverDate, isReady, summary, results);
    }
}
