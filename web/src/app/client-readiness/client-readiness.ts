import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReadinessApiService } from './readiness-api.service';
import { describeIssue } from './readiness-format';
import { ClientReadinessReport, PolicyReadiness } from './readiness-models';

@Component({
  selector: 'app-client-readiness',
  imports: [FormsModule],
  templateUrl: './client-readiness.html',
  styleUrl: './client-readiness.css',
})
export class ClientReadiness {
  private readonly api = inject(ReadinessApiService);

  protected readonly clientId = signal('');
  protected readonly cutoverDate = signal('2026-12-01');
  protected readonly loading = signal(false);
  protected readonly result = signal<ClientReadinessReport | null>(null);
  protected readonly error = signal('');

  protected readonly canCheck = computed(() => this.clientId().trim() !== '' && !this.loading());

  protected check(): void {
    const clientId = this.clientId().trim();
    this.loading.set(true);
    this.result.set(null);
    this.error.set('');
    this.api.getClientReadiness(clientId, this.cutoverDate()).subscribe({
      next: (report) => {
        this.result.set(report);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.error.set(errorMessage(error, clientId));
        this.loading.set(false);
      },
    });
  }

  protected issueText(policy: PolicyReadiness): string {
    return policy.issues.length === 0 ? 'None' : policy.issues.map(describeIssue).join(' ');
  }
}

function errorMessage(error: HttpErrorResponse, clientId: string): string {
  if (error.status === 404) {
    return `No client found with ID ${clientId}.`;
  }
  if (error.status === 400) {
    return 'The cutover date is not valid.';
  }
  return 'The readiness check failed. Try again.';
}
