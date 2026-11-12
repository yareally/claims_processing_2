# Likely questions: answers

Short model answers. The numbers match `docs/04_likely_questions.md`.

## About this solution

1. **A new rule.** Add a value to `ReadinessIssue` at the right place in the order, add one check in `EvaluatePolicy`, and add a test. If the rule needs new data, add a field to `PolicyRecord` and to the repository. Nothing in the browser changes if the browser only shows the server's answer. With many rules, each rule could become its own small class behind one interface, but five rules don't need that yet.

2. **50,000 policies.** The endpoint loads every policy into memory and returns them all in one response. First, page the `Policies` list or return only the blocked ones with counts. Then push the counting into the database query. If the check is slow, run it as a background job and store the result.

3. **`DateOnly` versus `DateTime`.** A `DateTime` carries a time and a time zone kind. "Expires before the cutover" could then depend on the hour or on the server's time zone, and a policy could flip between excluded and not excluded at midnight UTC. A date-only value says what the business means: a calendar day.

4. **Issues for non-technical readers.** Map each enum value to a sentence in the UI, for example `OpenClaims` to "This policy has open claims. Close or transfer them first." Keep the enum in the API, because codes are stable and easy to test, and translate in one place.

5. **Different answers a minute apart.** Not necessarily. The data can change between runs: someone pays a balance or closes a claim. To compare runs, the report could include the time it was made. If nothing changed and the answers differ, it is a bug.

6. **200 for a client that isn't ready.** The request worked: the server checked the client and has an answer. "Not ready" is the answer, not a failure. Error codes are for requests that could not be answered, such as a bad date (400) or an unknown client (404).

## C# and .NET

7. **`record` versus `class`.** A record compares by value (two records with the same data are equal), prints its data with `ToString`, and supports `with` to copy with changes. That suits data that shouldn't change, like a report. A class compares by reference.

8. **`async`/`await` and `CancellationToken`.** `await` frees the thread while waiting for I/O, so the server can handle other requests. The `CancellationToken` lets the server stop work when the caller disconnects, so it doesn't waste effort on an answer nobody will read.

9. **Dependency injection.** The framework creates the objects a class needs and passes them in, instead of the class creating them. Because the endpoint takes `IClientRepository`, a test can pass an in-memory version and production can pass a database version, with no change to the endpoint.

10. **`IEnumerable<T>`, `IReadOnlyList<T>`, `List<T>`.** `IEnumerable<T>` can only be looped over, and may run a query each time you loop. `IReadOnlyList<T>` has a count and an index, and can't be changed by the caller. `List<T>` can be changed. Return `IReadOnlyList<T>` for finished results; take `IEnumerable<T>` when you only loop once.

11. **`NotFound`, `Problem`, throwing.** `Results.NotFound()` is a normal 404 answer. `Results.Problem()` returns a standard error body (RFC 9457 problem details) with a status code and a message. Throwing is for things that should not happen; the framework turns an unhandled exception into a 500. Don't use exceptions for expected cases like a bad date.

## TypeScript and JavaScript

12. **`==` versus `===`.** `==` converts types before comparing (`0 == ''` is true). `===` compares without converting. Use `===`.

13. **Object versus `Map`.** An object's keys are strings, and it inherits keys such as `constructor` and `toString`, so `groups['constructor']` exists even when you never set it. A `Map` takes any key, keeps insertion order, has `size`, and has no inherited keys.

14. **`a < b ? -1 : 1`.** It never returns 0 for equal items, so the compare function is inconsistent: compare(a, b) and compare(b, a) both say "first". The result order can then depend on the browser. `<` on strings also compares character codes, so every uppercase letter sorts before every lowercase letter.

15. **Don't mutate the input.** Don't change the array or objects you were given. Other code may hold the same array, such as the component's state. Angular and other UI tools often detect changes by checking whether the reference changed; changing an object in place can leave the screen showing old data.

16. **`interface` versus `type`.** Both describe object shapes. An `interface` can be extended and merged; a `type` can also describe unions such as `'Ready' | 'Blocked'`. Pick one style for object shapes and stay consistent.

17. **`??=`, `?.`, `??`.** `a ??= b` sets `a` to `b` only when `a` is null or undefined. `a?.b` reads `b` only when `a` isn't null or undefined, and otherwise gives undefined. `a ?? b` gives `b` when `a` is null or undefined. Unlike `||`, `??` keeps `0` and `''`.

18. **Promise versus Observable.** A Promise gives one value, later, and starts at once. An Observable can give many values over time, starts only when someone subscribes, and can be cancelled by unsubscribing. Angular's `HttpClient` returns Observables.

## Angular

19. **Component and service.** A component owns a piece of the screen: its template and the state it shows. A service holds logic or data access that several components share. The HTTP call lives in a service so components stay about display, and tests can replace the service.

20. **Signals.** A signal holds one current value; reading it in a template makes Angular update the template when it changes. An Observable is a stream of values over time. Signals suit UI state; Observables suit events and HTTP. `toSignal` turns one into the other.

21. **`track` in `@for`.** It tells Angular how to recognise each item between updates, for example `track group.clientId`. Angular then reuses the existing rows instead of rebuilding the whole list. Tracking by a display name would have the same problem as grouping by one.

22. **The proxy.** `ng serve` reads `proxy.conf.json` and forwards `/policies` and `/clients` to the API on port 5100. The browser sees one origin, so no CORS setup is needed in development.

23. **Testing a component with HTTP.** Provide `provideHttpClient()` and `provideHttpClientTesting()`, then use `HttpTestingController` to answer the request with test data and check what the component shows. Or replace the service with a fake that returns fixed data.

## Working style

24. **What to test first.** The rules most likely to be wrong and most costly when wrong: the boundaries (expiry on the cutover day), and the rules that decide the final answer (`IsReady`).

25. **A bug outside your task.** Note it, tell the team, and don't fix it silently inside an unrelated change. In an assessment, say it aloud: it shows you noticed.

26. **Out of time.** Say which rule is missing, how you would write it, and which test would prove it. A clear, working part with an explained gap beats a broken whole.
