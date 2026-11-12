# The brief

## Background

The company is moving its clients' insurance policies from an old policy system to a new one. Each client moves on a set day, the **cutover date**. Before that day, the operations team checks whether every policy of the client can move.

Your job has two parts:

1. Write the readiness check in C#, and the endpoint that returns it.
2. Fix the TypeScript function that groups policies on the dashboard, and explain what was wrong with how the dashboard groups policy data (`docs/02_dashboard_question.md`).

## What you need to do

| Level | File | What to write | Command |
|---|---|---|---|
| 1 | `src/Readiness.Api/Readiness/ReadinessEvaluator.cs` | `EvaluatePolicy`: the readiness of one policy | `dotnet test --filter Level1` |
| 2 | `src/Readiness.Api/Readiness/ReadinessEvaluator.cs` | `EvaluateClient`: the report for one client | `dotnet test --filter Level2` |
| 2 | `src/Readiness.Api/ReadinessEndpoints.cs` | `GetClientReadinessAsync`: the endpoint | `dotnet test --filter Level2` |
| 3 | `web/src/app/policy-grouping.ts` | Fix `groupPoliciesByClient` | `cd web && npm test` |

Everything else is already written, including the tests, which check your code. Do not change public method signatures or the types in `Readiness/ReadinessModels.cs`, because the tests depend on them. Add private helper methods as you need them.

## Level 1: one policy

`EvaluatePolicy(PolicyRecord policy, DateOnly cutoverDate)` returns a `PolicyReadiness`.

**Step 1: excluded policies.** Some policies do not move at all. A policy is `Excluded` when either is true:

- its `Status` is `Cancelled`
- its `ExpirationDate` is before `cutoverDate` (it has ended by the cutover day)

An excluded policy has no issues. A policy that expires **on** the cutover date is still in force that day, so it is not excluded.

**Step 2: issues.** For every other policy, check each rule below. Report **every** issue that applies, in the order of this table, which is also the order of the `ReadinessIssue` enum.

| Issue | When it applies |
|---|---|
| `LapsedPolicy` | `Status` is `Lapsed` (the client stopped paying; someone must reinstate or cancel it first) |
| `UnmappedProduct` | The new system has no product for `LegacyProductCode`. Ask `IProductCatalog.IsMapped`. |
| `OutstandingBalance` | `OutstandingBalance` is above 0. A balance of 0 or below (a credit) is fine. |
| `OpenClaims` | `OpenClaimCount` is above 0 |
| `MissingContactEmail` | `PolicyholderEmail` is null, empty, or only spaces |

**Step 3: status.** A policy with no issues is `Ready`. A policy with one or more issues is `Blocked`.

## Level 2: one client

### `EvaluateClient(Client client, IReadOnlyList<PolicyRecord> policies, DateOnly cutoverDate)`

Returns a `ClientReadinessReport`:

1. `Policies`: the result of `EvaluatePolicy` for every policy, sorted by `PolicyNumber` (ordinal string order).
2. `Summary`: `Total` counts every policy, excluded ones too. `Ready`, `Blocked` and `Excluded` count each status.
3. `IsReady` is true only when no policy is `Blocked` **and** at least one policy is `Ready`. A client with no policies, or with only excluded policies, has nothing to move, so it is not ready. A person must look at it.
4. `ClientId`, `ClientName` and `CutoverDate` come from the inputs.

### `GetClientReadinessAsync`

The endpoint is `GET /clients/{clientId}/readiness?cutoverDate=2026-12-01`. Check in this order:

| Case | Response |
|---|---|
| `cutoverDate` is missing, or is not a date in the form `yyyy-MM-dd` | 400, with `Results.Problem(...)` |
| No client has this `clientId` (`IClientRepository.FindClientAsync` returns null) | 404, with `Results.NotFound()` |
| Otherwise | 200, with `Results.Ok(report)` |

Check the date before you look up the client. Parse it with `DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)`.

An example 200 response:

```json
{
  "clientId": "C-1001",
  "clientName": "Northwind Traders",
  "cutoverDate": "2026-12-01",
  "isReady": false,
  "summary": { "total": 3, "ready": 1, "blocked": 1, "excluded": 1 },
  "policies": [
    { "policyNumber": "P-1001-01", "status": "Ready", "issues": [] },
    { "policyNumber": "P-1001-02", "status": "Blocked", "issues": ["OutstandingBalance", "OpenClaims"] },
    { "policyNumber": "P-1001-03", "status": "Excluded", "issues": [] }
  ]
}
```

## Level 3: the dashboard grouping

The dashboard in `web/` lists policies grouped by client. The function `groupPoliciesByClient` in `web/src/app/policy-grouping.ts` builds those groups, and it has bugs. Fix it so that:

1. It groups by `clientId`. Two clients can share a name, and the old system typed one client's name in different ways.
2. Each group's `clientName` is the first policy's `clientName` for that client, with spaces trimmed from both ends.
3. `readyCount`, `blockedCount` and `excludedCount` come from each policy's `readiness` field, which the server sets with your Level 1 code. Do not work readiness out again in TypeScript.
4. The policies in a group keep their input order.
5. The groups are sorted by `clientName` ignoring upper and lower case, then by `clientId`.
6. It does not change the input array or the policy objects.
7. An empty input gives an empty array.

Then answer `docs/02_dashboard_question.md`. The fix above is one part of that answer. Read `web/src/app/dashboard/` for the rest.

## Hints

Read these only if you are stuck.

- `string.IsNullOrWhiteSpace` covers null, empty and spaces.
- `List<ReadinessIssue>` lets you add issues as you check each rule, in order.
- `policies.Select(...).OrderBy(p => p.PolicyNumber, StringComparer.Ordinal)` sorts in ordinal order.
- In TypeScript, a `Map<string, ClientGroup>` keeps insertion order and keys by any string.
- `a.localeCompare(b, undefined, { sensitivity: 'base' })` compares strings ignoring case.
