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
function makeReport(overrides: Partial<ClientReadinessReport> = {}): ClientReadinessReport {
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
    ...overrides,
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

it('ClientReadiness_starts_with_an_empty_client_id_and_the_date_2026_12_01', () => {
  expect(element<HTMLInputElement>('client-id').value).toBe('');
  expect(element<HTMLInputElement>('cutover-date').value).toBe('2026-12-01');
});

it('ClientReadiness_check_button_is_disabled_while_the_client_id_is_blank', async () => {
  expect(element<HTMLButtonElement>('check').disabled).toBe(true);

  await typeInto('client-id', '   ');
  expect(element<HTMLButtonElement>('check').disabled).toBe(true);

  await typeInto('client-id', 'C-1001');
  expect(element<HTMLButtonElement>('check').disabled).toBe(false);
});

it('ClientReadiness_check_sends_the_trimmed_client_id_and_the_chosen_date', async () => {
  await typeInto('client-id', '  C-1001  ');
  await typeInto('cutover-date', '2027-01-15');

  element('check').click();
  await fixture.whenStable();

  // expectOne fails when no request has this exact URL, and lists the requests it saw.
  http.expectOne('/clients/C-1001/readiness?cutoverDate=2027-01-15');
});

it('ClientReadiness_shows_checking_only_while_the_check_runs', async () => {
  await typeInto('client-id', 'C-1001');
  expect(textsOf('loading')).toEqual([]);

  element('check').click();
  await fixture.whenStable();
  expect(textsOf('loading')).toEqual(['Checking...']);

  http.expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01').flush(makeReport());
  await fixture.whenStable();
  expect(textsOf('loading')).toEqual([]);
});

it('ClientReadiness_shows_the_verdict_and_the_summary_after_a_check', async () => {
  await typeInto('client-id', 'C-1001');
  expect(textsOf('verdict')).toEqual([]);
  expect(textsOf('summary')).toEqual([]);

  element('check').click();
  await fixture.whenStable();
  http.expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01').flush(makeReport());
  await fixture.whenStable();

  expect(textsOf('verdict')).toEqual(['Not ready for cutover']);
  expect(textsOf('summary')).toEqual(['3 policies: 1 ready, 1 blocked, 1 excluded']);
});

it('ClientReadiness_shows_ready_for_cutover_when_the_client_is_ready', async () => {
  const report = makeReport({
    isReady: true,
    summary: { total: 2, ready: 2, blocked: 0, excluded: 0 },
    policies: [
      { policyNumber: 'P-1001-01', status: 'Ready', issues: [] },
      { policyNumber: 'P-1001-02', status: 'Ready', issues: [] },
    ],
  });
  await typeInto('client-id', 'C-1001');

  element('check').click();
  await fixture.whenStable();
  http.expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01').flush(report);
  await fixture.whenStable();

  expect(textsOf('verdict')).toEqual(['Ready for cutover']);
  expect(textsOf('summary')).toEqual(['2 policies: 2 ready, 0 blocked, 0 excluded']);
});

it('ClientReadiness_shows_one_row_per_policy_in_the_order_the_server_sent', async () => {
  const report = makeReport({
    policies: [
      { policyNumber: 'P-1001-03', status: 'Excluded', issues: [] },
      { policyNumber: 'P-1001-01', status: 'Ready', issues: [] },
      {
        policyNumber: 'P-1001-02',
        status: 'Blocked',
        issues: ['OutstandingBalance', 'OpenClaims'],
      },
    ],
  });
  await typeInto('client-id', 'C-1001');

  element('check').click();
  await fixture.whenStable();
  http.expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01').flush(report);
  await fixture.whenStable();

  expect(textsOf('policy-row')).toHaveLength(3);
  expect(textsOf('policy-number')).toEqual(['P-1001-03', 'P-1001-01', 'P-1001-02']);
  expect(textsOf('policy-status')).toEqual(['Excluded', 'Ready', 'Blocked']);
  expect(textsOf('policy-issues')).toEqual([
    'None',
    'None',
    'The policy has an unpaid balance. The policy has open claims.',
  ]);
});

it('ClientReadiness_shows_the_not_found_message_for_a_404', async () => {
  await typeInto('client-id', 'C-9999');

  element('check').click();
  await fixture.whenStable();
  http
    .expectOne('/clients/C-9999/readiness?cutoverDate=2026-12-01')
    .flush(null, { status: 404, statusText: 'Not Found' });
  await fixture.whenStable();

  expect(textsOf('error')).toEqual(['No client found with ID C-9999.']);
});

it('ClientReadiness_shows_the_date_message_for_a_400', async () => {
  await typeInto('client-id', 'C-1001');

  element('check').click();
  await fixture.whenStable();
  // The body is the problem details that Results.Problem sends.
  const problem = { status: 400, detail: 'cutoverDate must be a date in the form yyyy-MM-dd.' };
  http
    .expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01')
    .flush(problem, { status: 400, statusText: 'Bad Request' });
  await fixture.whenStable();

  expect(textsOf('error')).toEqual(['The cutover date is not valid.']);
});
