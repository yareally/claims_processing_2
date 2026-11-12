using Microsoft.AspNetCore.Http;
using Readiness.Api;
using Readiness.Api.Readiness;

namespace Readiness.Tests.Hidden;

// Edge cases the Level 1 and Level 2 tests leave out. Run them with: dotnet test --filter Hidden
public class HiddenTests
{
    private static readonly DateOnly Cutover = new(2026, 12, 1);
    private static readonly Client Northwind = new("C-1001", "Northwind Traders");
    private readonly ReadinessEvaluator _evaluator = new(new InMemoryProductCatalog(["AUTO-PERS"]));

    [Fact]
    public void EvaluatePolicy_all_five_issues_are_reported_in_enum_order()
    {
        var policy = TestData.ReadyPolicy() with
        {
            PolicyholderEmail = null,
            OpenClaimCount = 4,
            OutstandingBalance = 1500m,
            LegacyProductCode = "UMB-OLD",
            Status = PolicyStatus.Lapsed,
        };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(
            new[]
            {
                ReadinessIssue.LapsedPolicy,
                ReadinessIssue.UnmappedProduct,
                ReadinessIssue.OutstandingBalance,
                ReadinessIssue.OpenClaims,
                ReadinessIssue.MissingContactEmail,
            },
            result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_empty_email_is_blocked()
    {
        var policy = TestData.ReadyPolicy() with { PolicyholderEmail = "" };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(new[] { ReadinessIssue.MissingContactEmail }, result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_one_cent_balance_is_blocked()
    {
        var policy = TestData.ReadyPolicy() with { OutstandingBalance = 0.01m };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(new[] { ReadinessIssue.OutstandingBalance }, result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_lapsed_policy_that_expired_before_cutover_is_excluded()
    {
        var policy = TestData.ReadyPolicy() with
        {
            Status = PolicyStatus.Lapsed,
            ExpirationDate = new DateOnly(2026, 11, 15),
        };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Empty(result.Issues);
        Assert.Equal(PolicyReadinessStatus.Excluded, result.Status);
    }

    [Fact]
    public void EvaluateClient_client_with_no_policies_is_not_ready()
    {
        var report = _evaluator.EvaluateClient(Northwind, [], Cutover);

        Assert.Empty(report.Policies);
        Assert.Equal(new ReadinessSummary(Total: 0, Ready: 0, Blocked: 0, Excluded: 0), report.Summary);
        Assert.False(report.IsReady, "Expected IsReady false: a client with no policies has nothing to move.");
    }

    [Fact]
    public void EvaluateClient_sorts_uppercase_before_lowercase_in_ordinal_order()
    {
        PolicyRecord[] policies =
        [
            TestData.ReadyPolicy("p-1001-01"),
            TestData.ReadyPolicy("P-1001-02"),
            TestData.ReadyPolicy("P-1001-01"),
        ];

        var report = _evaluator.EvaluateClient(Northwind, policies, Cutover);

        Assert.Equal(new[] { "P-1001-01", "P-1001-02", "p-1001-01" }, report.Policies.Select(p => p.PolicyNumber));
    }

    [Fact]
    public void EvaluateClient_judges_each_policy_on_the_given_cutover_date()
    {
        PolicyRecord[] policies =
        [
            TestData.ReadyPolicy("P-1001-01") with { ExpirationDate = new DateOnly(2027, 6, 30) },
            TestData.ReadyPolicy("P-1001-02") with { ExpirationDate = new DateOnly(2027, 7, 1) },
        ];

        var report = _evaluator.EvaluateClient(Northwind, policies, new DateOnly(2027, 7, 1));

        Assert.Equal(new ReadinessSummary(Total: 2, Ready: 1, Blocked: 0, Excluded: 1), report.Summary);
        Assert.Equal(new DateOnly(2027, 7, 1), report.CutoverDate);
    }

    [Theory]
    [InlineData("2026-13-01")]
    [InlineData("2026-02-30")]
    [InlineData("12/01/2026")]
    [InlineData("2026-12-01T00:00")]
    [InlineData(" 2026-12-01")]
    public async Task GetClientReadinessAsync_rejects_dates_not_written_as_yyyy_MM_dd(string cutoverDate)
    {
        var repository = new InMemoryClientRepository([Northwind], [TestData.ReadyPolicy()]);

        var result = await ReadinessEndpoints.GetClientReadinessAsync(
            "C-1001", cutoverDate, repository, _evaluator, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetClientReadinessAsync_client_with_no_policies_returns_200_not_ready()
    {
        var repository = new InMemoryClientRepository([Northwind], []);

        var result = await ReadinessEndpoints.GetClientReadinessAsync(
            "C-1001", "2026-12-01", repository, _evaluator, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        var report = Assert.IsType<ClientReadinessReport>(valueResult.Value);
        Assert.Equal(0, report.Summary.Total);
        Assert.False(report.IsReady, "Expected IsReady false: a client with no policies has nothing to move.");
    }
}
