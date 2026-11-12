# The walkthrough

The assessment ends with a short oral explanation of your solution. Record yourself answering these in about 5 minutes in total. Play it back once: listen for long pauses and for words like "basically" and "just".

1. Walk through `EvaluatePolicy`. Why does the exclusion check come before the issue checks?
2. Why does the check report every issue, instead of stopping at the first one?
3. Why is a client with only excluded policies not ready?
4. In `GetClientReadinessAsync`, why check the date before looking up the client? Why 400 for a bad date and 404 for an unknown client?
5. Walk through `countIssues`. How did you get the tie order right?
6. Walk through the `ClientReadiness` component. Where does its state live, and why signals?
7. What does the screen show before a check, during one, after a success and after each kind of error? Why a different message for 404 and 400?
8. Why does the HTTP call live in `ReadinessApiService` and not in the component? Why `encodeURIComponent`?
9. What is the general issue with how the dashboard groups policy data? How would you fix it properly?
10. What would you test next, or change, if you had another hour?

Answer each in three parts: what you did, why, and what you would watch out for.
