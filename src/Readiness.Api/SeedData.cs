namespace Readiness.Api;

// Fixed sample data for the running app: 15 clients and 130 policies, the same on every run.
// Each client's policies start as ready on the 2026-12-01 cutover date; SpecialCases then changes
// a few of them to cover every issue and both exclusion rules.
//
// The policy list on the dashboard is ordered by policy number, and page 1 holds the first 50:
// C-1001 to C-1006 and the first two policies of C-1007. Everything from P-1007-03 on is beyond page 1.
public static class SeedData
{
    private const string UnmappedMarineCode = "MARINE-LEGACY";
    private const string UnmappedUmbrellaCode = "UMB-OLD";
    private static readonly DateOnly CutoverDate = new(2026, 12, 1);
    private static readonly DateOnly DayBeforeCutover = new(2026, 11, 30);

    public static readonly IReadOnlyList<string> MappedProductCodes =
        ["AUTO-PERS", "AUTO-COMM", "HOME-STD", "PROP-COMM", "GL-STD", "WC-STD"];

    private static readonly IReadOnlyList<ClientSeed> ClientSeeds =
    [
        new(1001, "Northwind Traders", 3),
        new(1002, "Contoso Pharmaceuticals", 9),
        new(1003, "Fabrikam Inc", 9),
        new(1004, "Adventure Works Cycles", 10),
        new(1005, "Tailspin Toys", 9),
        new(1006, "Wide World Importers", 8),
        new(1007, "Litware Inc", 9),
        new(1008, "Proseware Inc", 10),
        new(1009, "Coho Winery", 10),
        new(1010, "Alpine Ski House", 10),
        new(1011, "Blue Yonder Airlines", 10),
        new(1012, "Margie's Travel", 10),
        new(1013, "Trey Research", 3),
        new(1014, "Woodgrove Bank", 11),
        new(1042, "Northwind Traders", 9),
    ];

    private static readonly Dictionary<string, Func<PolicyRecord, PolicyRecord>> SpecialCases = new()
    {
        // C-1001 gives the example response in docs/01_brief.md. The old system typed its name three ways.
        ["P-1001-02"] = p => p with { ClientName = "NORTHWIND TRADERS", OutstandingBalance = 1250.00m, OpenClaimCount = 1 },
        ["P-1001-03"] = p => p with { ClientName = "Northwind Traders ", Status = PolicyStatus.Cancelled, OutstandingBalance = 80.00m },

        // C-1002: three blocked policies, one issue each.
        ["P-1002-02"] = p => p with { OutstandingBalance = 342.18m },
        ["P-1002-05"] = p => p with { Status = PolicyStatus.Lapsed },
        ["P-1002-07"] = p => p with { PolicyholderEmail = null },

        // C-1003 is ready. A credit balance is not an issue.
        ["P-1003-04"] = p => p with { OutstandingBalance = -25.00m },

        // C-1004: an unmapped product, open claims, one expired policy and one cancelled policy.
        ["P-1004-03"] = p => p with { LegacyProductCode = UnmappedMarineCode },
        ["P-1004-06"] = p => p with { OpenClaimCount = 2 },
        ["P-1004-08"] = p => p with { EffectiveDate = new DateOnly(2025, 7, 1), ExpirationDate = new DateOnly(2026, 6, 30) },
        ["P-1004-10"] = p => p with { Status = PolicyStatus.Cancelled },

        // C-1005: one policy with two issues.
        ["P-1005-04"] = p => p with { Status = PolicyStatus.Lapsed, OutstandingBalance = 96.40m },

        // C-1006 is ready.
        // C-1007 straddles the end of page 1: its two policies on page 1 are ready, its blocked ones are on page 2.
        ["P-1007-05"] = p => p with { OpenClaimCount = 1 },
        ["P-1007-07"] = p => p with { PolicyholderEmail = "   " },

        // C-1008 is ready. One policy expires on the cutover date, so it is still in force and still counts.
        ["P-1008-09"] = p => p with { EffectiveDate = new DateOnly(2025, 12, 1), ExpirationDate = CutoverDate },

        // C-1009 is ready: one policy expired the day before cutover, one cancelled policy has an open claim
        // and an unmapped product, and one policy has a credit balance.
        ["P-1009-02"] = p => p with { EffectiveDate = new DateOnly(2025, 12, 1), ExpirationDate = DayBeforeCutover },
        ["P-1009-06"] = p => p with { Status = PolicyStatus.Cancelled, OpenClaimCount = 1, LegacyProductCode = UnmappedUmbrellaCode },
        ["P-1009-09"] = p => p with { OutstandingBalance = -150.00m },

        // C-1010: one policy with all five issues, one with an unmapped product.
        ["P-1010-03"] = p => p with
        {
            Status = PolicyStatus.Lapsed,
            LegacyProductCode = UnmappedUmbrellaCode,
            OutstandingBalance = 2100.00m,
            OpenClaimCount = 3,
            PolicyholderEmail = null,
        },
        ["P-1010-08"] = p => p with { LegacyProductCode = UnmappedMarineCode },

        // C-1011: two lapsed policies, one of them with an empty email.
        ["P-1011-04"] = p => p with { Status = PolicyStatus.Lapsed },
        ["P-1011-05"] = p => p with { Status = PolicyStatus.Lapsed, PolicyholderEmail = "" },

        // C-1012: a balance of one cent still blocks; one policy has an open claim and a balance.
        ["P-1012-02"] = p => p with { OutstandingBalance = 0.01m },
        ["P-1012-07"] = p => p with { OutstandingBalance = 640.00m, OpenClaimCount = 1 },

        // C-1013 has only excluded policies, so it is not ready.
        ["P-1013-01"] = p => p with { Status = PolicyStatus.Cancelled },
        ["P-1013-02"] = p => p with { EffectiveDate = new DateOnly(2025, 10, 1), ExpirationDate = new DateOnly(2026, 9, 30) },
        ["P-1013-03"] = p => p with { EffectiveDate = new DateOnly(2025, 12, 1), ExpirationDate = DayBeforeCutover },

        // C-1014: an unmapped product, and a lapsed policy that expired before cutover, so it is excluded, not blocked.
        ["P-1014-05"] = p => p with { LegacyProductCode = UnmappedUmbrellaCode },
        ["P-1014-10"] = p => p with
        {
            Status = PolicyStatus.Lapsed,
            EffectiveDate = new DateOnly(2025, 11, 15),
            ExpirationDate = new DateOnly(2026, 11, 15),
        },

        // C-1042 is a different client with the same name as C-1001.
        ["P-1042-03"] = p => p with { OutstandingBalance = 515.75m },
        ["P-1042-06"] = p => p with { PolicyholderEmail = "  " },
    };

    public static IReadOnlyList<Client> CreateClients() =>
        ClientSeeds.Select(seed => new Client(seed.ClientId, seed.Name)).ToList();

    public static IReadOnlyList<PolicyRecord> CreatePolicies() =>
        ClientSeeds
            .SelectMany(seed => Enumerable.Range(1, seed.PolicyCount).Select(index => CreatePolicy(seed, index)))
            .ToList();

    private static PolicyRecord CreatePolicy(ClientSeed seed, int index)
    {
        var effectiveDate = new DateOnly(2026, (seed.Number + index) % 12 + 1, 1);
        var policy = new PolicyRecord(
            PolicyNumber: $"P-{seed.Number}-{index:00}",
            ClientId: seed.ClientId,
            ClientName: seed.Name,
            Status: PolicyStatus.Active,
            LegacyProductCode: MappedProductCodes[(index - 1) % MappedProductCodes.Count],
            EffectiveDate: effectiveDate,
            ExpirationDate: effectiveDate.AddYears(1),
            OutstandingBalance: 0m,
            OpenClaimCount: 0,
            PolicyholderEmail: $"holder.{seed.Number}.{index:00}@example.com");
        return SpecialCases.TryGetValue(policy.PolicyNumber, out var change) ? change(policy) : policy;
    }

    private sealed record ClientSeed(int Number, string Name, int PolicyCount)
    {
        public string ClientId => $"C-{Number}";
    }
}
