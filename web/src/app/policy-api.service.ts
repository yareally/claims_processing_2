import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PolicyPage } from './policy-models';

@Injectable({ providedIn: 'root' })
export class PolicyApiService {
  private readonly http = inject(HttpClient);

  getPolicies(page: number, pageSize: number): Observable<PolicyPage> {
    return this.http.get<PolicyPage>('/policies', { params: { page, pageSize } });
  }
}
