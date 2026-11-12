# The walkthrough

The assessment ends with a short oral explanation of your solution. Record yourself answering these in about 5 minutes in total. Play it back once: listen for long pauses and for words like "basically" and "just".

1. Walk through `EvaluatePolicy`. Why does the exclusion check come before the issue checks?
2. Why does the check report every issue, instead of stopping at the first one?
3. Why is a client with only excluded policies not ready?
4. In `GetClientReadinessAsync`, why check the date before looking up the client? Why 400 for a bad date and 404 for an unknown client?
5. What did you change in `groupPoliciesByClient`, and why?
6. What is the general issue with how the dashboard groups policy data? How would you fix it properly?
7. What would you test next, or change, if you had another hour?

Answer each in three parts: what you did, why, and what you would watch out for.
