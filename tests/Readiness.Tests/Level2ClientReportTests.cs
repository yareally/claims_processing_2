using Readiness.Api;
using Readiness.Api.Readiness;

namespace Readiness.Tests;

public class Level2ClientReportTests
{
    private static readonly DateOnly Cutover = new(2026, 12, 1);
    private static readonly Client Northwind = new("C-1001", "Northwind Traders");
    private readonly ReadinessEvaluator _evaluator = new(new InMemoryProductCatalog(["AUTO-PERS"]));

    [Fact]
    public void EvaluateClient_sorts_policies_by_policy_number_in_ordinal_order()
    {
        PolicyRecord[] policies =
        [
            TestData.ReadyPolicy("P-1001-3"),
            TestData.ReadyPolicy("P-1001-100"),
            TestData.ReadyPolicy("P-1001-20"),
        ];

        var report = _evaluator.EvaluateClient(Northwind, policies, Cutover);

        Assert.Equal(new[] { "P-1001-100", "P-1001-20", "P-1001-3" }, report.Policies.Select(p => p.PolicyNumber));
    }

    [Fact]
    public void EvaluateClient_summary_counts_each_status_and_total_includes_excluded()
    {
        PolicyRecord[] policies =
        [
            TestData.ReadyPolicy("P-1001-01"),
            TestData.ReadyPolicy("P-1001-02") with { OutstandingBalance = 300m },
            TestData.ReadyPolicy("P-1001-03") with { Status = PolicyStatus.Cancelled },
            TestData.ReadyPolicy("P-1001-04"),
            TestData.ReadyPolicy("P-1001-05") with { ExpirationDate = new DateOnly(2026, 11, 30) },
            TestData.ReadyPolicy("P-1001-06"),
        ];

        var report = _evaluator.EvaluateClient(Northwind, policies, Cutover);

        Assert.Equal(new ReadinessSummary(Total: 6, Ready: 3, Blocked: 1, Excluded: 2), report.Summary);
    }

    [Fact]
    public void EvaluateClient_ready_and_excluded_policies_make_the_client_ready()
    {
        PolicyRecord[] policies =
        [
            TestData.ReadyPolicy("P-1001-01"),
            TestData.ReadyPolicy("P-1001-02") with { Status = PolicyStatus.Cancelled },
        ];

        var report = _evaluator.EvaluateClient(Northwind, policies, Cutover);

        Assert.True(report.IsReady, "Expected IsReady true: one policy is Ready, the other is Excluded, none is Blocked.");
    }

    [Fact]
    public void EvaluateClient_one_blocked_policy_makes_the_client_not_ready()
    {
        PolicyRecord[] policies =
        [
            TestData.ReadyPolicy("P-1001-01"),
            TestData.ReadyPolicy("P-1001-02") with { OpenClaimCount = 1 },
        ];

        var report = _evaluator.EvaluateClient(Northwind, policies, Cutover);

        Assert.False(report.IsReady, "Expected IsReady false: one policy is Blocked.");
    }

    [Fact]
    public void EvaluateClient_only_excluded_policies_make_the_client_not_ready()
    {
        PolicyRecord[] policies =
        [
            TestData.ReadyPolicy("P-1001-01") with { Status = PolicyStatus.Cancelled },
            TestData.ReadyPolicy("P-1001-02") with { ExpirationDate = new DateOnly(2026, 6, 30) },
        ];

        var report = _evaluator.EvaluateClient(Northwind, policies, Cutover);

        Assert.False(report.IsReady, "Expected IsReady false: every policy is Excluded, so no policy is Ready.");
    }

    [Fact]
    public void EvaluateClient_takes_client_id_name_and_cutover_date_from_the_inputs()
    {
        PolicyRecord[] policies = [TestData.ReadyPolicy() with { ClientName = "NORTHWIND TRADERS " }];

        var report = _evaluator.EvaluateClient(Northwind, policies, Cutover);

        Assert.Equal("C-1001", report.ClientId);
        Assert.Equal("Northwind Traders", report.ClientName);
        Assert.Equal(new DateOnly(2026, 12, 1), report.CutoverDate);
    }
}
