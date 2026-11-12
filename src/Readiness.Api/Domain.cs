namespace Readiness.Api;

public record Client(string ClientId, string Name);

public enum PolicyStatus
{
    Active,
    Lapsed,
    Cancelled,
}

// One policy as the old policy system stores it. ClientName is copied onto each policy,
// so it can be typed differently from Client.Name and from the client's other policies.
public record PolicyRecord(
    string PolicyNumber,
    string ClientId,
    string ClientName,
    PolicyStatus Status,
    string LegacyProductCode,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    decimal OutstandingBalance,
    int OpenClaimCount,
    string? PolicyholderEmail);
