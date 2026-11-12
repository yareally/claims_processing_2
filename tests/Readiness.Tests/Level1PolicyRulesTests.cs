using Readiness.Api;
using Readiness.Api.Readiness;

namespace Readiness.Tests;

public class Level1PolicyRulesTests
{
    private static readonly DateOnly Cutover = new(2026, 12, 1);
    private readonly ReadinessEvaluator _evaluator = new(new InMemoryProductCatalog(["AUTO-PERS"]));

    [Fact]
    public void EvaluatePolicy_clean_active_policy_is_ready()
    {
        var result = _evaluator.EvaluatePolicy(TestData.ReadyPolicy("P-1001-01"), Cutover);

        Assert.Equal("P-1001-01", result.PolicyNumber);
        Assert.Empty(result.Issues);
        Assert.Equal(PolicyReadinessStatus.Ready, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_cancelled_policy_is_excluded_with_no_issues()
    {
        var policy = TestData.ReadyPolicy() with
        {
            Status = PolicyStatus.Cancelled,
            OutstandingBalance = 250m,
            OpenClaimCount = 1,
        };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Empty(result.Issues);
        Assert.Equal(PolicyReadinessStatus.Excluded, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_expired_before_cutover_is_excluded()
    {
        var policy = TestData.ReadyPolicy() with { ExpirationDate = new DateOnly(2026, 11, 30), OpenClaimCount = 2 };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Empty(result.Issues);
        Assert.Equal(PolicyReadinessStatus.Excluded, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_expiring_on_cutover_date_is_not_excluded()
    {
        var policy = TestData.ReadyPolicy() with { ExpirationDate = new DateOnly(2026, 12, 1) };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Empty(result.Issues);
        Assert.Equal(PolicyReadinessStatus.Ready, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_lapsed_policy_is_blocked()
    {
        var policy = TestData.ReadyPolicy() with { Status = PolicyStatus.Lapsed };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(new[] { ReadinessIssue.LapsedPolicy }, result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_unmapped_product_is_blocked()
    {
        var policy = TestData.ReadyPolicy() with { LegacyProductCode = "MARINE-LEGACY" };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(new[] { ReadinessIssue.UnmappedProduct }, result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_positive_balance_is_blocked()
    {
        var policy = TestData.ReadyPolicy() with { OutstandingBalance = 120.50m };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(new[] { ReadinessIssue.OutstandingBalance }, result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-75)]
    public void EvaluatePolicy_zero_or_negative_balance_is_not_an_issue(int balance)
    {
        var policy = TestData.ReadyPolicy() with { OutstandingBalance = balance };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Empty(result.Issues);
        Assert.Equal(PolicyReadinessStatus.Ready, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_open_claims_are_blocked()
    {
        var policy = TestData.ReadyPolicy() with { OpenClaimCount = 1 };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(new[] { ReadinessIssue.OpenClaims }, result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void EvaluatePolicy_missing_or_blank_email_is_blocked(string? email)
    {
        var policy = TestData.ReadyPolicy() with { PolicyholderEmail = email };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(new[] { ReadinessIssue.MissingContactEmail }, result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }

    [Fact]
    public void EvaluatePolicy_reports_every_issue_in_enum_order()
    {
        var policy = TestData.ReadyPolicy() with
        {
            PolicyholderEmail = null,
            OpenClaimCount = 3,
            LegacyProductCode = "MARINE-LEGACY",
        };

        var result = _evaluator.EvaluatePolicy(policy, Cutover);

        Assert.Equal(
            new[] { ReadinessIssue.UnmappedProduct, ReadinessIssue.OpenClaims, ReadinessIssue.MissingContactEmail },
            result.Issues);
        Assert.Equal(PolicyReadinessStatus.Blocked, result.Status);
    }
}
