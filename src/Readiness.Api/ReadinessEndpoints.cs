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
        if (!DateOnly.TryParseExact(cutoverDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return Results.Problem(
                detail: "cutoverDate is required and must be a date in the form yyyy-MM-dd.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var client = await repository.FindClientAsync(clientId, cancellationToken);
        if (client is null)
        {
            return Results.NotFound();
        }

        var policies = await repository.GetPoliciesAsync(clientId, cancellationToken);
        return Results.Ok(evaluator.EvaluateClient(client, policies, date));
    }
}
