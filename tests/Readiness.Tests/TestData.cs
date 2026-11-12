using Readiness.Api;

namespace Readiness.Tests;

internal static class TestData
{
    // A policy that is ready on the 2026-12-01 cutover date when "AUTO-PERS" is mapped.
    // Tests change one field at a time with `with { ... }`.
    public static PolicyRecord ReadyPolicy(string policyNumber = "P-1001-01") => new(
        PolicyNumber: policyNumber,
        ClientId: "C-1001",
        ClientName: "Northwind Traders",
        Status: PolicyStatus.Active,
        LegacyProductCode: "AUTO-PERS",
        EffectiveDate: new DateOnly(2026, 1, 1),
        ExpirationDate: new DateOnly(2027, 1, 1),
        OutstandingBalance: 0m,
        OpenClaimCount: 0,
        PolicyholderEmail: "owner@northwind.example");
}
