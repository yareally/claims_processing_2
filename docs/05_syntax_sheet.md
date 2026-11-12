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

## Angular

```ts
// A component with signals for its state
import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-order-lookup',
  templateUrl: './order-lookup.html',
})
export class OrderLookup {
  private readonly api = inject(OrderApiService);

  protected readonly orderId = signal('');
  protected readonly order = signal<Order | null>(null);
  protected readonly message = signal('');
  protected readonly busy = signal(false);
  protected readonly canSearch = computed(() => this.orderId().trim() !== '' && !this.busy());

  protected search(): void {
    this.order.set(null);
    this.message.set('');
    this.busy.set(true);
    this.api.getOrder(this.orderId().trim()).subscribe({
      next: (order) => {
        this.order.set(order);
        this.busy.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.message.set(error.status === 404 ? 'No such order.' : 'Something went wrong.');
        this.busy.set(false);
      },
    });
  }
}
```

```html
<!-- An input bound to a signal, without forms -->
<input [value]="orderId()" (input)="orderId.set($any($event.target).value)" />
<button [disabled]="!canSearch()" (click)="search()">Search</button>

@if (busy()) {
  <p>Searching...</p>
}
@if (order(); as found) {
  <p>{{ found.items.length }} items</p>
  <table>
    @for (item of found.items; track item.sku) {
      <tr>
        <td>{{ item.sku }}</td>
        <td>{{ item.notes.length === 0 ? 'None' : item.notes.join(' ') }}</td>
      </tr>
    }
  </table>
}
```

```ts
// A service that calls the API
@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);

  getOrder(orderId: string): Observable<Order> {
    return this.http.get<Order>(`/orders/${encodeURIComponent(orderId)}`, { params: { expand: 'items' } });
  }
}

// A lookup table the compiler checks for missing keys
type Size = 'Small' | 'Large';
const sizeLabels: Record<Size, string> = { Small: 'Fits in a pocket', Large: 'Needs a bag' };
```

## Commands

```
dotnet test --filter Level1
dotnet test --filter Level2
dotnet test -- xUnit.StopOnFail=false
cd web
npm run test:level3
npm run test:level4
npm run test:all
```

## Design questions to answer aloud before you code

1. What makes a policy excluded, and why should excluded policies never block a client?
2. Which rules can apply to one policy at the same time?
3. Where should readiness be worked out: on the server, in the browser, or both? Why?
4. What does the `ClientReadiness` screen show before a check, during one, after a success, and after a failure?
