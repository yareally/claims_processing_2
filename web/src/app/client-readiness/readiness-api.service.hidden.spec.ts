import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ReadinessApiService } from './readiness-api.service';

let service: ReadinessApiService;
let http: HttpTestingController;

beforeEach(() => {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  service = TestBed.inject(ReadinessApiService);
  http = TestBed.inject(HttpTestingController);
});

it('getClientReadiness_encodes_a_client_id_with_a_space_or_a_slash', () => {
  service.getClientReadiness('C 1001/A', '2026-12-01').subscribe();

  // expectOne fails when no request has this exact URL, and lists the requests it saw.
  http.expectOne('/clients/C%201001%2FA/readiness?cutoverDate=2026-12-01');
});
