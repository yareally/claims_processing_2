# The walkthrough: answers

Short model answers. Say them in your own words.

1. **Exclusion first.** An excluded policy doesn't move, so its problems don't matter. A cancelled policy with an unpaid balance must not block the client. Checking exclusion first returns early and keeps the issue checks simple.

2. **Every issue, not the first.** The report is a to-do list for the operations team. If it showed only the first issue, they would fix it, run the check again, and find the next one. Showing all of them saves rounds. The fixed order makes the output predictable and easy to test.

3. **Only excluded policies means not ready.** "Nothing blocks" is not the same as "ready to move". A client with no policies to move is probably a data problem, such as policies filed under the wrong client ID. Marking it not ready makes a person look at it. Say that you followed the brief, and that you would confirm the rule with the product owner.

4. **Date before lookup, 400 versus 404.** A bad date is the caller's mistake in the request itself, so the server can reject it without touching the data store. That is cheaper, and the answer doesn't depend on which clients exist. 400 means "fix your request". 404 means "the request is fine, but that client doesn't exist".

5. **`countIssues` tie order.** Start from `READINESS_ISSUE_ORDER`, count each issue, drop the zeros, then sort by count, highest first. JavaScript's `sort` is stable (equal items keep their order), so equal counts stay in the issue order without a second comparison. If you sort in another way, add the tie-break yourself: `b.count - a.count || order.indexOf(a.issue) - order.indexOf(b.issue)`.

6. **State in `ClientReadiness`.** Five signals: the client ID, the date, `loading`, the `result` and the `error` message, plus one `computed` for whether the button is enabled. Signals tell Angular exactly what changed. The app has no zone.js, so a plain field set when the HTTP answer arrives would not update the screen. A `computed` keeps the button rule in one place.

7. **The screen's states.** Before a check: the empty form, nothing else. During: "Checking..." and a disabled button, so a double click can't send two requests. After a success: the verdict, the summary and the table. After an error: one message, chosen by status. 404 means the request was fine but the client doesn't exist, so the message names the ID. 400 means the request itself was wrong, so the message names the date. Anything else is a failure the user can't fix, so the message says to try again. A new check clears the old result and error, so the screen never mixes two answers.

8. **The service.** The component is about the screen; the service is about the server. Tests can replace the service, and another screen can reuse it. `encodeURIComponent` stops an ID with a space, `/` or `?` from changing the URL's meaning: `C/1001` would otherwise ask for a different path.

9. **The general issue.** See `dashboard_answer.md`. In one sentence: the browser rebuilds a business answer from one page of raw rows, keyed by a display name, with a partial copy of the rules; the server should group and count with the same code as the readiness endpoint. If you did Level 5, say what you changed in `groupPoliciesByClient`: group by `clientId`, count the server's `readiness` field, and sort with `localeCompare` and a `clientId` tie-break.

10. **Another hour.** Add the server summary endpoint and point the dashboard at it. Show the `countIssues` result on the screen as "what to fix first". Add a test that the dashboard and the readiness endpoint agree for a client. Ask what time zone the cutover date means. Load test a client with many policies.
