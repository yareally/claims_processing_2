import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ReadinessApiService } from './readiness-api.service';
import { ClientReadinessReport } from './readiness-models';

let service: ReadinessApiService;
let http: HttpTestingController;

beforeEach(() => {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  service = TestBed.inject(ReadinessApiService);
  http = TestBed.inject(HttpTestingController);
});

it('getClientReadiness_sends_a_get_with_the_client_id_and_the_date', () => {
  service.getClientReadiness('C-2002', '2027-01-15').subscribe();

  // expectOne fails when no request has this exact URL, and lists the requests it saw.
  const request = http.expectOne('/clients/C-2002/readiness?cutoverDate=2027-01-15');
  expect(request.request.method).toBe('GET');
});

it('getClientReadiness_returns_the_report_from_the_server', () => {
  const report: ClientReadinessReport = {
    clientId: 'C-1001',
    clientName: 'Northwind Traders',
    cutoverDate: '2026-12-01',
    isReady: true,
    summary: { total: 1, ready: 1, blocked: 0, excluded: 0 },
    policies: [{ policyNumber: 'P-1001-01', status: 'Ready', issues: [] }],
  };
  let received: ClientReadinessReport | undefined;

  service.getClientReadiness('C-1001', '2026-12-01').subscribe((value) => (received = value));
  http.expectOne('/clients/C-1001/readiness?cutoverDate=2026-12-01').flush(report);

  expect(received).toEqual(report);
});
