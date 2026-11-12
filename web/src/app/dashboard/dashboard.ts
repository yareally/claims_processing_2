import { Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { PolicyApiService } from '../policy-api.service';
import { groupPoliciesByClient } from '../policy-grouping';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  private readonly api = inject(PolicyApiService);

  protected readonly policyPage = rxResource({
    stream: () => this.api.getPolicies(1, 50),
  });

  protected readonly groups = computed(() =>
    this.policyPage.hasValue() ? groupPoliciesByClient(this.policyPage.value().items) : [],
  );
}
