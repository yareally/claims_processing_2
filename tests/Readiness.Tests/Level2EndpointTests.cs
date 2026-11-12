using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Readiness.Api;
using Readiness.Api.Readiness;

namespace Readiness.Tests;

public class Level2EndpointTests
{
    private readonly ReadinessEvaluator _evaluator = new(new InMemoryProductCatalog(["AUTO-PERS"]));

    // Two clients with the same name. C-1001 has one blocked policy; C-1042 has one ready policy.
    private readonly InMemoryClientRepository _repository = new(
        [new Client("C-1001", "Northwind Traders"), new Client("C-1042", "Northwind Traders")],
        [
            TestData.ReadyPolicy("P-1001-02") with { OutstandingBalance = 99m },
            TestData.ReadyPolicy("P-1042-01") with { ClientId = "C-1042" },
            TestData.ReadyPolicy("P-1001-01"),
        ]);

    [Fact]
    public async Task GetClientReadinessAsync_known_client_returns_200_with_its_report()
    {
        var result = await ReadinessEndpoints.GetClientReadinessAsync(
            "C-1042", "2026-12-01", _repository, _evaluator, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        var report = Assert.IsType<ClientReadinessReport>(valueResult.Value);
        Assert.Equal("C-1042", report.ClientId);
        Assert.Equal(new DateOnly(2026, 12, 1), report.CutoverDate);
        Assert.Equal(new[] { "P-1042-01" }, report.Policies.Select(p => p.PolicyNumber));
        Assert.True(report.IsReady, "Expected IsReady true: C-1042's only policy is Ready.");
    }

    [Fact]
    public async Task GetClientReadinessAsync_client_that_is_not_ready_still_returns_200()
    {
        var result = await ReadinessEndpoints.GetClientReadinessAsync(
            "C-1001", "2026-12-01", _repository, _evaluator, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        var report = Assert.IsType<ClientReadinessReport>(valueResult.Value);
        Assert.Equal(new ReadinessSummary(Total: 2, Ready: 1, Blocked: 1, Excluded: 0), report.Summary);
        Assert.False(report.IsReady, "Expected IsReady false: P-1001-02 is Blocked.");
    }

    [Fact]
    public async Task GetClientReadinessAsync_unknown_client_returns_404()
    {
        var result = await ReadinessEndpoints.GetClientReadinessAsync(
            "C-9999", "2026-12-01", _repository, _evaluator, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("next week")]
    public async Task GetClientReadinessAsync_missing_or_malformed_date_returns_400_problem(string? cutoverDate)
    {
        var result = await ReadinessEndpoints.GetClientReadinessAsync(
            "C-1001", cutoverDate, _repository, _evaluator, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        Assert.IsAssignableFrom<ProblemDetails>(valueResult.Value);
    }

    [Fact]
    public async Task GetClientReadinessAsync_checks_the_date_before_looking_up_the_client()
    {
        var result = await ReadinessEndpoints.GetClientReadinessAsync(
            "C-9999", "not-a-date", _repository, _evaluator, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }
}
