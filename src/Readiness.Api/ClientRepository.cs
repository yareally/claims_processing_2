namespace Readiness.Api;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public interface IClientRepository
{
    Task<Client?> FindClientAsync(string clientId, CancellationToken cancellationToken);

    // The client's policies in the order they were stored, which is not sorted.
    Task<IReadOnlyList<PolicyRecord>> GetPoliciesAsync(string clientId, CancellationToken cancellationToken);

    // One page of all policies, ordered by PolicyNumber (ordinal). Page numbers start at 1.
    Task<PagedResult<PolicyRecord>> GetPolicyPageAsync(int page, int pageSize, CancellationToken cancellationToken);
}

public sealed class InMemoryClientRepository : IClientRepository
{
    private readonly Dictionary<string, Client> _clientsById;
    private readonly List<PolicyRecord> _policies;
    private readonly List<PolicyRecord> _policiesByNumber;

    public InMemoryClientRepository(IEnumerable<Client> clients, IEnumerable<PolicyRecord> policies)
    {
        _clientsById = clients.ToDictionary(client => client.ClientId, StringComparer.Ordinal);
        _policies = policies.ToList();
        _policiesByNumber = _policies.OrderBy(policy => policy.PolicyNumber, StringComparer.Ordinal).ToList();
    }

    public Task<Client?> FindClientAsync(string clientId, CancellationToken cancellationToken) =>
        Task.FromResult(_clientsById.GetValueOrDefault(clientId));

    public Task<IReadOnlyList<PolicyRecord>> GetPoliciesAsync(string clientId, CancellationToken cancellationToken)
    {
        IReadOnlyList<PolicyRecord> policies = _policies.Where(policy => policy.ClientId == clientId).ToList();
        return Task.FromResult(policies);
    }

    public Task<PagedResult<PolicyRecord>> GetPolicyPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var items = _policiesByNumber.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResult<PolicyRecord>(items, page, pageSize, _policiesByNumber.Count));
    }
}
