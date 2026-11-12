# The dashboard question

The operations team says the dashboard's numbers don't match what they see per client. Some clients look ready on the dashboard but fail the readiness check. One client appears more than once. Some clients are missing.

**Explain the general issue with how the dashboard groups policy data.** Then say how you would fix it.

Look at:

- `web/src/app/policy-grouping.ts` (the grouping function, before your Level 3 fix)
- `web/src/app/dashboard/` (how the dashboard loads the data it groups)
- `web/src/app/policy-api.service.ts`
- `src/Readiness.Api/PolicyEndpoints.cs` (what the server sends)

Answer aloud in about 2 minutes, or in 5 to 8 sentences of writing. A good answer:

1. names each problem you find, and the symptom it causes for the operations team
2. says what the problems have in common
3. says where the grouping and the counting should happen instead, and why

Don't open `docs/answers/` until you have answered.
