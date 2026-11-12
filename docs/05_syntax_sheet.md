# Syntax sheet

Use this in runs 1 and 2. In run 3, work without it. The examples use orders and customers on purpose, so they show the syntax without giving away the answers.

## C#

```csharp
// A list you add to, then return as IReadOnlyList<T>
var warnings = new List<string>();
if (order.ItemCount > 0)
{
    warnings.Add("has items");
}

// Null, empty, or spaces
if (string.IsNullOrWhiteSpace(order.Notes)) { }

// Comparing dates (DateOnly supports < <= > >=)
if (order.ShipDate < today) { }

// The conditional operator
var label = warnings.Count == 0 ? "clean" : "check";

// LINQ: map, sort, count, any
var totals = orders
    .Select(o => new OrderTotal(o.OrderNumber, o.Price * o.Quantity))
    .OrderBy(t => t.OrderNumber, StringComparer.Ordinal)
    .ToList();
var bigOrders = totals.Count(t => t.Amount > 100m);
var anyFree = totals.Any(t => t.Amount == 0m);

// Creating a record
public record OrderTotal(string OrderNumber, decimal Amount);
var total = new OrderTotal("A-1", 25m);

// Parsing a date exactly (needs using System.Globalization;)
var ok = DateOnly.TryParseExact("2026-12-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);

// Minimal API results
return Results.Problem(detail: "Explain what was wrong.", statusCode: StatusCodes.Status400BadRequest);
return Results.NotFound();
return Results.Ok(value);

// Awaiting a call that may return null
var customer = await store.FindAsync(id, cancellationToken);
if (customer is null) { }
```

## TypeScript

```ts
// A Map keeps insertion order and keys by any string
const byCity = new Map<string, Customer[]>();
let list = byCity.get(customer.city);
if (list === undefined) {
  list = [];
  byCity.set(customer.city, list);
}
list.push(customer);

// A switch on a string union
type Size = 'Small' | 'Large';
switch (size) {
  case 'Small':
    small++;
    break;
  case 'Large':
    large++;
    break;
}

// Map values to an array
const lists = [...byCity.values()];

// A sort compare function returns a negative number, 0, or a positive number.
// `x || y` uses y when x is 0, so it works as a tie-break.
customers.sort((a, b) =>
  a.lastName.localeCompare(b.lastName, undefined, { sensitivity: 'base' }) ||
  a.id.localeCompare(b.id));

// Copy instead of changing the input
const sortedCopy = [...customers].sort(compare);
const renamed = { ...customer, name: customer.name.trim() };
```

## Commands

```
dotnet test --filter Level1
dotnet test --filter Level2
dotnet test -- xUnit.StopOnFail=false
cd web && npm test
```

## Design questions to answer aloud before you code

1. What makes a policy excluded, and why should excluded policies never block a client?
2. Which rules can apply to one policy at the same time?
3. Where should readiness be worked out: on the server, in the browser, or both? Why?
