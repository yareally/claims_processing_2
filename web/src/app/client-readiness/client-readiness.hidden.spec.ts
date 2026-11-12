import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ClientReadiness } from './client-readiness';
import { ClientReadinessReport } from './readiness-models';

let fixture: ComponentFixture<ClientReadiness>;
let http: HttpTestingController;

beforeEach(async () => {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  fixture = TestBed.createComponent(ClientReadiness);
  http = TestBed.inject(HttpTestingController);
  await fixture.whenStable();
});

// The report from the brief's example: one ready, one blocked and one excluded policy.
function makeReport(): ClientReadinessReport {
  return {
    clientId: 'C-1001',
    clientName: 'Northwind Traders',
    cutoverDate: '2026-12-01',
    isReady: false,
    summary: { total: 3, ready: 1, blocked: 1, excluded: 1 },
    policies: [
      { policyNumber: 'P-1001-01', status: 'Ready', issues: [] },
      {
        policyNumber: 'P-1001-02',
        status: 'Blocked',
        issues: ['OutstandingBalance', 'OpenClaims'],
      },
      { policyNumber: 'P-1001-03', status: 'Excluded', issues: [] },
    ],
  };
}

// The element with this data-testid. Stops the test with a clear message when there is none.
function element<T extends HTMLElement>(testId: string): T {
  const page = fixture.nativeElement as HTMLElement;
  const found = page.querySelector<T>(`[data-testid="${testId}"]`);
  if (found === null) {
    throw new Error(`No element with data-testid="${testId}" on the page.`);
  }
  return found;
}

// The text of every element with this data-testid, extra spaces removed. [] when there is none.
function textsOf(testId: string): string[] {
  const page = fixture.nativeElement as HTMLElement;
  const found = page.querySelectorAll(`[data-testid="${testId}"]`);
  return Array.from(found, (item) => (item.textContent ?? '').replace(/\s+/g, ' ').trim());
}

// Types a value into an input the way a person does, then waits for the page to update.
async function typeInto(testId: string, value: string): Promise<void> {
  const input = element<HTMLInputElement>(testId);
  input.value = value;
  input.dispatchEvent(new Event('input', { bubbles: true }));
  input.dispatchEvent(new Event('change', { bubbles: true }));
  await fixture.whenStable();
}

it('ClientReadiness_shows_the_general_message_for_a_500', async () => {
  await typeInto('client-id', 'C-1001');

  element('check').click();
  await fixture.whenStable();
  http
    .expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01')
    .flush(null, { status: 500, statusText: 'Internal Server Error' });
  await fixture.whenStable();

  expect(textsOf('error')).toEqual(['The readiness check failed. Try again.']);
});

it('ClientReadiness_not_found_message_uses_the_trimmed_client_id', async () => {
  await typeInto('client-id', '  C-9999  ');

  element('check').click();
  await fixture.whenStable();
  http
    .expectOne('/clients/C-9999/readiness?cutoverDate=2026-12-01')
    .flush(null, { status: 404, statusText: 'Not Found' });
  await fixture.whenStable();

  expect(textsOf('error')).toEqual(['No client found with ID C-9999.']);
});

it('ClientReadiness_check_button_is_disabled_while_a_check_runs', async () => {
  await typeInto('client-id', 'C-1001');

  element('check').click();
  await fixture.whenStable();
  expect(element<HTMLButtonElement>('check').disabled).toBe(true);

  http.expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01').flush(makeReport());
  await fixture.whenStable();
  expect(element<HTMLButtonElement>('check').disabled).toBe(false);
});

it('ClientReadiness_a_second_check_clears_the_old_error', async () => {
  await typeInto('client-id', 'C-9999');
  element('check').click();
  await fixture.whenStable();
  http
    .expectOne('/clients/C-9999/readiness?cutoverDate=2026-12-01')
    .flush(null, { status: 404, statusText: 'Not Found' });
  await fixture.whenStable();
  expect(textsOf('error')).toEqual(['No client found with ID C-9999.']);

  await typeInto('client-id', 'C-1001');
  element('check').click();
  await fixture.whenStable();

  expect(textsOf('error')).toEqual([]);
});

it('ClientReadiness_a_second_check_clears_the_old_result', async () => {
  await typeInto('client-id', 'C-1001');
  element('check').click();
  await fixture.whenStable();
  http.expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01').flush(makeReport());
  await fixture.whenStable();
  expect(textsOf('verdict')).toEqual(['Not ready for cutover']);

  element('check').click();
  await fixture.whenStable();

  expect(textsOf('verdict')).toEqual([]);
  expect(textsOf('summary')).toEqual([]);
  expect(textsOf('policy-row')).toEqual([]);
});
