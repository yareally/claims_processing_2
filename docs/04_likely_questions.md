# Likely questions

Interviewers often ask follow-up questions during or after the walkthrough. Practise answering each aloud in under a minute. Answers are in `docs/answers/likely_questions_answers.md` on the `solution` branch.

## About this solution

1. A new rule arrives: policies with a pending address change are blocked. Where does it go, and what else changes?
2. A client has 50,000 policies. What breaks first, and what would you change?
3. The cutover date is a `DateOnly`. What goes wrong if it were a `DateTime`?
4. How would you show the reason for each issue to a person who isn't technical?
5. Two people run the readiness check a minute apart and get different answers. Is that a bug?
6. Why return the report with 200 when the client isn't ready? Why not an error code?

## C# and .NET

7. What is the difference between a `record` and a `class`? Why are the models records?
8. What does `async`/`await` do? Why does the endpoint take a `CancellationToken`?
9. What is dependency injection, and why is `IClientRepository` an interface?
10. What is the difference between `IEnumerable<T>`, `IReadOnlyList<T>` and `List<T>`? When would you return each?
11. What is the difference between `Results.NotFound()`, `Results.Problem()` and throwing an exception?

## TypeScript and JavaScript

12. What is the difference between `==` and `===`?
13. What is the difference between an object used as a map and a `Map`?
14. Why does sorting with `(a, b) => a < b ? -1 : 1` cause trouble?
15. What does "don't mutate the input" mean, and why does it matter in a UI?
16. What is the difference between `interface` and `type` in TypeScript?
17. What does `??=` do? What about `?.` and `??`?
18. What is a Promise? What is an Observable, and how is it different?

## Angular

19. What is a component, and what is a service? Why does the HTTP call live in a service?
20. What is a signal? How is it different from an Observable?
21. What does `track` do in an `@for` loop?
22. How does the dashboard reach the API in development? (Look at `proxy.conf.json`.)
23. How would you test a component that calls an HTTP service?

## Working style

24. How do you decide what to test first?
25. You found a bug outside your task. What do you do?
26. Your code works but you ran out of time for one rule. What do you say?
