# The walkthrough: answers

Short model answers. Say them in your own words.

1. **Exclusion first.** An excluded policy doesn't move, so its problems don't matter. A cancelled policy with an unpaid balance must not block the client. Checking exclusion first returns early and keeps the issue checks simple.

2. **Every issue, not the first.** The report is a to-do list for the operations team. If it showed only the first issue, they would fix it, run the check again, and find the next one. Showing all of them saves rounds. The fixed order makes the output predictable and easy to test.

3. **Only excluded policies means not ready.** "Nothing blocks" is not the same as "ready to move". A client with no policies to move is probably a data problem, such as policies filed under the wrong client ID. Marking it not ready makes a person look at it. Say that you followed the brief, and that you would confirm the rule with the product owner.

4. **Date before lookup, 400 versus 404.** A bad date is the caller's mistake in the request itself, so the server can reject it without touching the data store. That is cheaper, and the answer doesn't depend on which clients exist. 400 means "fix your request". 404 means "the request is fine, but that client doesn't exist".

5. **`groupPoliciesByClient`.** It grouped by name, so two clients with one name merged and one client with differently typed names split. It now groups by `clientId`. It worked out readiness again with only two of the rules, so it now counts the server's `readiness` field. The sort ignored case and never returned 0, so it now uses `localeCompare` with a `clientId` tie-break.

6. **The general issue.** See `dashboard_answer.md`. In one sentence: the browser rebuilds a business answer from one page of raw rows, keyed by a display name, with a partial copy of the rules; the server should group and count with the same code as the readiness endpoint.

7. **Another hour.** Add the server summary endpoint and point the dashboard at it. Add a test that the dashboard and the readiness endpoint agree for a client. Ask what time zone the cutover date means. Load test a client with many policies.
