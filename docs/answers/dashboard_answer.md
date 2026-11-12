# The dashboard question: an answer

## The short answer

The dashboard groups and counts policies in the browser, from data that is incomplete and with rules that are not the server's. It groups by a display name instead of an ID, it groups only the first page of policies, and it works out readiness again with a partial copy of the rules. Each problem makes the dashboard disagree with the readiness endpoint. The fix is to let the server group and count, with the same code the readiness endpoint uses, and let the dashboard only display the result.

## The problems, and what the operations team sees

| Problem | Where | What the team sees |
|---|---|---|
| Groups by `clientName`, not `clientId` | `groupPoliciesByClient` | Two different clients named "Northwind Traders" merge into one row. One client whose name the old system typed as "Northwind Traders" and "NORTHWIND TRADERS" splits into two rows. |
| Works out readiness again in TypeScript | `isPolicyReady` in `policy-grouping.ts` | It checks only the balance and open claims. A policy with an unmapped product, a missing email, or a lapsed status counts as ready. A policy with a credit (a negative balance) counts as blocked. An expired policy counts as ready or blocked instead of excluded. |
| Groups only page 1 | the dashboard component loads `page=1&pageSize=50` | Clients whose policies all sit on later pages are missing. Clients split across pages show counts that are too low. |
| Sorts with `a < b ? -1 : 1` | `groupPoliciesByClient` | Sorting is case-sensitive ("acme" after "Zenith"), and the compare function never returns 0, which breaks the rules `sort` expects. |

## What they have in common

The browser rebuilds a business answer (which clients are ready, and how many policies block each) from raw rows. To get it right, the browser would need all the rows, the right key, and the same rules as the server. It has none of the three.

## The fix

1. Add a server endpoint, for example `GET /clients/readiness-summary?cutoverDate=...`, that groups by `ClientId` and counts with `ReadinessEvaluator`. Then the dashboard and the readiness endpoint can never disagree, because one piece of code decides.
2. Page the summary by client, not by policy, so a client's counts never split across pages.
3. In the dashboard, display what the server returns. Keep only display work in the browser: sorting a visible table, expanding a row.
4. Until that endpoint exists, the Level 3 fix is the smaller step: group by `clientId` and use the server's `readiness` field. That still leaves the paging problem, so say so.

## Points that impress

- Name the symptom for the user, not only the code problem.
- Say that duplicated rules drift: the day someone adds a rule on the server, the dashboard is wrong, and no test catches it.
- Say that IDs are for identity and names are for display.
