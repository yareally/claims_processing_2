namespace Readiness.Api.Readiness;

// The order of these values is the order issues are reported in.
public enum ReadinessIssue
{
    LapsedPolicy,
    UnmappedProduct,
    OutstandingBalance,
    OpenClaims,
    MissingContactEmail,
}

public enum PolicyReadinessStatus
{
    Ready,
    Blocked,
    Excluded,
}

public record PolicyReadiness(string PolicyNumber, PolicyReadinessStatus Status, IReadOnlyList<ReadinessIssue> Issues);

public record ReadinessSummary(int Total, int Ready, int Blocked, int Excluded);

public record ClientReadinessReport(
    string ClientId,
    string ClientName,
    DateOnly CutoverDate,
    bool IsReady,
    ReadinessSummary Summary,
    IReadOnlyList<PolicyReadiness> Policies);
