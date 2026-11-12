namespace Readiness.Api;

// The products the new system offers, looked up by the old system's product code.
public interface IProductCatalog
{
    bool IsMapped(string legacyProductCode);
}

public sealed class InMemoryProductCatalog(IEnumerable<string> mappedCodes) : IProductCatalog
{
    private readonly HashSet<string> _mappedCodes = new(mappedCodes, StringComparer.Ordinal);

    public bool IsMapped(string legacyProductCode) => _mappedCodes.Contains(legacyProductCode);
}
