import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClientReadinessReport } from './readiness-models';

@Injectable({ providedIn: 'root' })
export class ReadinessApiService {
  private readonly http = inject(HttpClient);

  getClientReadiness(clientId: string, cutoverDate: string): Observable<ClientReadinessReport> {
    return this.http.get<ClientReadinessReport>(
      `/clients/${encodeURIComponent(clientId)}/readiness`,
      { params: { cutoverDate } },
    );
  }
}
