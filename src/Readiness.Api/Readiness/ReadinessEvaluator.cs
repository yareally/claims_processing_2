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
        // TODO: see "Level 1: one policy" in docs/01_brief.md.
        throw new NotImplementedException("TODO: Level 1. See docs/01_brief.md.");
    }

    public ClientReadinessReport EvaluateClient(Client client, IReadOnlyList<PolicyRecord> policies, DateOnly cutoverDate)
    {
        // TODO: see "Level 2: one client" in docs/01_brief.md.
        throw new NotImplementedException("TODO: Level 2. See docs/01_brief.md.");
    }
}
