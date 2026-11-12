import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClientReadinessReport } from './readiness-models';

@Injectable({ providedIn: 'root' })
export class ReadinessApiService {
  private readonly http = inject(HttpClient);

  getClientReadiness(clientId: string, cutoverDate: string): Observable<ClientReadinessReport> {
    throw new Error('TODO: Level 3. See docs/01_brief.md.');
  }
}
