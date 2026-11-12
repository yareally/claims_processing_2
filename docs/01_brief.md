# The brief

## Background

The company is moving its clients' insurance policies from an old policy system to a new one. Each client moves on a set day, the **cutover date**. Before that day, the operations team checks whether every policy of the client can move.

Your job has three parts:

1. **Back end (C#):** write the readiness check and the endpoint that returns it.
2. **Front end (TypeScript and Angular):** write the screen that calls your endpoint and shows the result.
3. **Explain** what is wrong with how the existing dashboard groups policy data (`docs/02_dashboard_question.md`).

## What you need to do

| Level | File | What to write | Command |
|---|---|---|---|
| 1 | `src/Readiness.Api/Readiness/ReadinessEvaluator.cs` | `EvaluatePolicy`: the readiness of one policy | `dotnet test --filter Level1` |
| 2 | `src/Readiness.Api/Readiness/ReadinessEvaluator.cs` | `EvaluateClient`: the report for one client | `dotnet test --filter Level2` |
| 2 | `src/Readiness.Api/ReadinessEndpoints.cs` | `GetClientReadinessAsync`: the endpoint | `dotnet test --filter Level2` |
| 3 | `web/src/app/client-readiness/readiness-format.ts` | `describeIssue` and `countIssues` | `cd web && npm run test:level3` |
| 3 | `web/src/app/client-readiness/readiness-api.service.ts` | `getClientReadiness`: the HTTP call | `cd web && npm run test:level3` |
| 4 | `web/src/app/client-readiness/client-readiness.ts` and `.html` | The `ClientReadiness` component | `cd web && npm run test:level4` |
| 5 (optional) | `web/src/app/policy-grouping.ts` | Fix `groupPoliciesByClient` | `cd web && npm run test:level5` |

Everything else is already written, including the tests, which check your code. Do not change public method signatures, the C# types in `Readiness/ReadinessModels.cs`, or the TypeScript types in `client-readiness/readiness-models.ts`, because the tests depend on them. Add private helper methods as you need them.

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
| Otherwise | Get the client's policies with `IClientRepository.GetPoliciesAsync`, pass them to `EvaluateClient`, and return 200 with `Results.Ok(report)` |

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

## Level 3: TypeScript functions and the HTTP call

All files are in `web/src/app/client-readiness/`. The types (`ClientReadinessReport`, `PolicyReadiness`, `ReadinessIssue`, `IssueCount`) and the constant `READINESS_ISSUE_ORDER` are in `readiness-models.ts`. They match the JSON your endpoint returns.

### `describeIssue(issue: ReadinessIssue): string`

Returns the sentence a person reads for each issue:

| Issue | Sentence |
|---|---|
| `LapsedPolicy` | `The policy has lapsed. Reinstate or cancel it.` |
| `UnmappedProduct` | `The product has no match in the new system.` |
| `OutstandingBalance` | `The policy has an unpaid balance.` |
| `OpenClaims` | `The policy has open claims.` |
| `MissingContactEmail` | `The policyholder has no email address.` |

### `countIssues(policies: PolicyReadiness[]): IssueCount[]`

Counts how many policies have each issue, so the team sees what to fix first.

1. Return one `{ issue, count }` for each issue that appears at least once. Leave out issues that never appear.
2. Sort by `count`, highest first. When two counts are equal, use the order of `READINESS_ISSUE_ORDER`.
3. An empty list gives an empty array. Do not change the input.

For example, two policies with `["OutstandingBalance", "OpenClaims"]` and `["OpenClaims"]` give `[{ issue: "OpenClaims", count: 2 }, { issue: "OutstandingBalance", count: 1 }]`.

### `ReadinessApiService.getClientReadiness(clientId: string, cutoverDate: string): Observable<ClientReadinessReport>`

Sends `GET /clients/{clientId}/readiness?cutoverDate={cutoverDate}` with Angular's `HttpClient`. Put the client ID in the path with `encodeURIComponent`, so an ID with a space or a slash can't break the URL. Pass the date as a query parameter.

## Level 4: the `ClientReadiness` component

Write the component in `client-readiness.ts` and its template `client-readiness.html`. It already appears on the page, above the dashboard. The tests find each element by its `data-testid` attribute, so use exactly these:

| `data-testid` | Element | What it does |
|---|---|---|
| `client-id` | text `<input>` | Starts empty |
| `cutover-date` | `<input type="date">` | Starts as `2026-12-01` |
| `check` | `<button>` | Disabled while the client ID is empty or only spaces, and while a check is running. A click calls `getClientReadiness` with the client ID (spaces trimmed) and the date. |
| `loading` | any | Shown only while a check is running. Text: `Checking...` |
| `verdict` | any | After a successful check: `Ready for cutover` or `Not ready for cutover` |
| `summary` | any | After a successful check: `3 policies: 1 ready, 1 blocked, 1 excluded` (the numbers from `summary`) |
| `policy-row` | `<tr>` | One row per policy, in the order the server sent them |
| `policy-number`, `policy-status`, `policy-issues` | `<td>` in each row | The policy number; the status; the `describeIssue` sentences joined with one space, or `None` when there are no issues |
| `error` | any | After a failed check. 404: `No client found with ID C-9999.` (the trimmed ID). 400: `The cutover date is not valid.` Any other error: `The readiness check failed. Try again.` |

A new check clears the last result and the last error. Show nothing from the table, verdict or summary while there is no result.

**Use signals for the component's state** (`signal()`, `computed()`). This app runs without zone.js, Angular's older way of noticing changes. If the component keeps its state in plain fields, the screen does not update when the server's answer arrives.

## Level 5 (optional): fix the dashboard grouping

The dashboard in `web/` lists policies grouped by client. It needs your Level 1 code, because the server sets each policy's readiness with `EvaluatePolicy`. Until Level 1 works, the dashboard shows "Could not load policies." The function `groupPoliciesByClient` in `web/src/app/policy-grouping.ts` builds those groups, and it has bugs. Fix it so that:

1. It groups by `clientId`. Two clients can share a name, and the old system typed one client's name in different ways.
2. Each group's `clientName` is the first policy's `clientName` for that client, with spaces trimmed from both ends.
3. `readyCount`, `blockedCount` and `excludedCount` come from each policy's `readiness` field, which the server sets with your Level 1 code. Do not work readiness out again in TypeScript.
4. The policies in a group keep their input order.
5. The groups are sorted by `clientName` ignoring upper and lower case, then by `clientId`.
6. It does not change the input array or the policy objects.
7. An empty input gives an empty array.

Whether or not you do Level 5, answer `docs/02_dashboard_question.md`. Read `web/src/app/policy-grouping.ts` and `web/src/app/dashboard/` for it.

## Hints

Read these only if you are stuck.

- `string.IsNullOrWhiteSpace` covers null, empty and spaces.
- `List<ReadinessIssue>` lets you add issues as you check each rule, in order.
- `policies.Select(...).OrderBy(p => p.PolicyNumber, StringComparer.Ordinal)` sorts in ordinal order.
- A TypeScript `Record<ReadinessIssue, string>` maps each issue to its sentence, and the compiler tells you if one is missing.
- `this.http.get<T>(url, { params: { cutoverDate } })` adds a query parameter.
- `HttpErrorResponse` has a `status` field: 404, 400, and so on.
- In a template, `@if (result(); as report) { ... }` shows a block only when there is a result, and `@for (policy of report.policies; track policy.policyNumber) { ... }` repeats rows.
- In TypeScript, a `Map<string, ClientGroup>` keeps insertion order and keys by any string.
- `a.localeCompare(b, undefined, { sensitivity: 'base' })` compares strings ignoring case.
