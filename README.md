# Client Readiness Practice

A timed practice run for an assessment that asks you to:

- implement a client readiness endpoint in C# that checks policies and reports whether they are ready for cutover
- build the screen that shows the result, in Angular and TypeScript (TypeScript is JavaScript with types)
- explain a general issue with how the dashboard groups policy data
- talk through your solution aloud

The assessment itself will differ. This project copies its shape from the invitation text only. Because the role is full stack, about half the coding is front end.

## Set up before the timed run

The project, the .NET 10 SDK and Node.js are already on this PC, inside WSL (the Linux system that runs under Windows).

1. Open a WSL terminal: in Windows Terminal, type `wsl`. Then run:

   ```
   cd ~/idea_projects/client_readiness_practice
   git switch -c my-attempt
   cd web && npm install && cd ..
   dotnet test --filter Level1
   code .
   ```

2. Check that the bottom-left corner of VS Code says **WSL: Debian**. If it doesn't, close the window and run `code .` from the WSL terminal again.
3. In the Extensions panel, install **C# Dev Kit** and **Angular Language Service**. Click **Install in WSL: Debian** for each.

At the start, every given test fails, and each run prints only the first failure. Fix it, run again, and repeat, one level at a time.

The `solution` branch holds the answers. Stay off it until you finish.

## Practise under the same limits

The assessment editor is a limited online version of VS Code. Use no AI tools during a timed run.

| Run | Time limit | What you may use | Goal |
|---|---|---|---|
| 1 | None | Extensions, the hints in the brief, `docs/05_syntax_sheet.md` | Learn the design |
| 2 | 55 minutes | `docs/05_syntax_sheet.md`, extensions off | Find what slows you down |
| 3 | 55 minutes | Nothing extra, extensions off | Match the assessment |

To turn an extension off, open the Extensions panel, open its gear menu and choose **Disable (Workspace)**. Then run the tests in the terminal.

## One timed run (55 minutes, 60 at most)

The minutes are estimates. Adjust them after your first run.

| Minutes | Stage | What to do |
|---|---|---|
| 5 | Get familiar | Read `docs/01_brief.md`. Find the TODOs. Run the tests. Answer the design questions in `docs/05_syntax_sheet.md` aloud. |
| 12 | Level 1 (C#) | `EvaluatePolicy`: one policy's rules. |
| 10 | Level 2 (C#) | `EvaluateClient` and the endpoint `GetClientReadinessAsync`. |
| 8 | Level 3 (TypeScript) | `describeIssue`, `countIssues` and the HTTP call `getClientReadiness`. |
| 12 | Level 4 (Angular) | The `ClientReadiness` component: form, loading, errors, verdict, table. |
| 4 | Dashboard question | `docs/02_dashboard_question.md`. |
| 4 | Walkthrough | Record yourself answering `docs/03_walkthrough.md`. |

Level 5 (fixing `groupPoliciesByClient`) is optional. Do it after the timed run, or if time is left.

To practise only the front end, do Levels 3 and 4 in 25 minutes. Their tests don't need the C# code.

Say your reasoning aloud as you work. The assessment asks for this, and it takes practice.

If time runs short, finish a level before you start the next. A working Level 3 is worth more than a half-finished Level 4. Then say aloud what you would do next.

## Commands

```
dotnet test --filter Level1
dotnet test --filter Level2
cd web
npm run test:level3
npm run test:level4
npm run test:level5
```

Each run stops at the first failure. To see every failure, run `dotnet test -- xUnit.StopOnFail=false`, or `npm run test:all` in `web`.

### How to read a C# failure

```
Failed EvaluatePolicy_lapsed_policy_is_blocked [11 ms]
Error Message:
 Assert.Equal() Failure: Collections differ
         ↓ (pos 0)
Expected: [LapsedPolicy]
Actual:   []
Stack Trace:
   at Readiness.Tests.Level1PolicyRulesTests.EvaluatePolicy_lapsed_policy_is_blocked() in .../Level1PolicyRulesTests.cs:line 66
```

- The test name starts with the method to fix, then says what should happen.
- `Expected` and `Actual` show what the check found. The `line` is the check in the test.
- `NotImplementedException` means the method still has its TODO. The stack trace names your file and line.

### How to read a TypeScript failure

```
FAIL  src/app/client-readiness/client-readiness.spec.ts > ClientReadiness_shows_checking_only_while_the_check_runs
AssertionError: expected [ 'Checking...' ] to deeply equal []
```

- The test name starts with the function or component to fix.
- The first value is what your code produced. The second is what the test expected.
- `Error: TODO: Level 3` means the function still has its TODO.
- `No element with data-testid="client-id" on the page` means your template is missing that element, or its `data-testid` is spelled differently.

### See the whole system run

Open two WSL terminals:

```
dotnet run --project src/Readiness.Api --urls http://localhost:5100
cd web && npm start
```

Then open http://localhost:4200 in the browser. Your `ClientReadiness` screen is at the top and needs Levels 1 to 4. Try `C-1001`, `C-1003` (ready) and `C-9999` (not found). The dashboard below it needs Level 1; until then it says "Could not load policies."

To call the endpoint yourself: http://localhost:5100/clients/C-1001/readiness?cutoverDate=2026-12-01

## After the timed run

1. Commit your work, then add the extra tests. These cover cases the given tests leave out:

   ```
   git add -A
   git commit -m "My attempt"
   git checkout solution -- tests/Readiness.Tests/Hidden web/src/app/client-readiness/client-readiness.hidden.spec.ts web/src/app/policy-grouping.hidden.spec.ts
   dotnet test --filter Hidden
   cd web && npm run test:all
   ```

2. Compare your code with the reference solution:

   ```
   git diff my-attempt solution -- src/ web/src/
   ```

3. Practise `docs/04_likely_questions.md` aloud.

4. Read the answers in `docs/answers/` on the `solution` branch:

   ```
   git switch solution
   ```

To try again from a clean start, run `git switch main`, then `git switch -c attempt-2`.

## What is in this repo

| Path | What it is |
|---|---|
| `src/Readiness.Api` | The API. Your C# TODOs are in `Readiness/ReadinessEvaluator.cs` and `ReadinessEndpoints.cs`. |
| `tests/Readiness.Tests` | The C# tests, one class per level. |
| `web/src/app/client-readiness` | Your Levels 3 and 4: `readiness-format.ts`, `readiness-api.service.ts`, `client-readiness.ts` and `client-readiness.html`. The tests are the `.spec.ts` files next to them. |
| `web/src/app/dashboard`, `web/src/app/policy-grouping.ts` | The existing dashboard, for the question and the optional Level 5. |
| `docs/` | The brief, the dashboard question, the walkthrough, likely questions and the syntax sheet. |
