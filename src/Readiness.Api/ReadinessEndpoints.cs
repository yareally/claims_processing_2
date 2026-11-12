using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Readiness.Api.Readiness;

namespace Readiness.Api;

public static class ReadinessEndpoints
{
    public static IEndpointRouteBuilder MapReadinessEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/clients/{clientId}/readiness", GetClientReadinessAsync);
        return endpoints;
    }

    public static async Task<IResult> GetClientReadinessAsync(
        string clientId,
        [FromQuery] string? cutoverDate,
        IClientRepository repository,
        ReadinessEvaluator evaluator,
        CancellationToken cancellationToken)
    {
        // TODO: see "GetClientReadinessAsync" in docs/01_brief.md.
        throw new NotImplementedException("TODO: Level 2. See docs/01_brief.md.");
    }
}
