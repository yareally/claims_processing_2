using Microsoft.AspNetCore.Mvc;
using Readiness.Api.Readiness;

namespace Readiness.Api;

public record PolicySummaryDto(
    string PolicyNumber,
    string ClientId,
    string ClientName,
    PolicyStatus Status,
    decimal OutstandingBalance,
    int OpenClaimCount,
    PolicyReadinessStatus Readiness);

// The cutover date the policy list uses, read from Cutover:DefaultDate in appsettings.json.
public record CutoverSettings(DateOnly DefaultDate);

public static class PolicyEndpoints
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    public static IEndpointRouteBuilder MapPolicyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/policies", ListPoliciesAsync);
        return endpoints;
    }

    // GET /policies?page=1&pageSize=50: one page of policies, each with its readiness on the default cutover date.
    public static async Task<IResult> ListPoliciesAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        IClientRepository repository,
        ReadinessEvaluator evaluator,
        CutoverSettings cutover,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(page ?? 1, 1);
        var size = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
        var policyPage = await repository.GetPolicyPageAsync(pageNumber, size, cancellationToken);

        var items = policyPage.Items
            .Select(policy => ToSummary(policy, evaluator.EvaluatePolicy(policy, cutover.DefaultDate).Status))
            .ToList();
        return Results.Ok(new PagedResult<PolicySummaryDto>(items, policyPage.Page, policyPage.PageSize, policyPage.TotalCount));
    }

    private static PolicySummaryDto ToSummary(PolicyRecord policy, PolicyReadinessStatus readiness) => new(
        policy.PolicyNumber,
        policy.ClientId,
        policy.ClientName,
        policy.Status,
        policy.OutstandingBalance,
        policy.OpenClaimCount,
        readiness);
}
