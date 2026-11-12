import { Component } from '@angular/core';
import { ClientReadiness } from './client-readiness/client-readiness';
import { Dashboard } from './dashboard/dashboard';

@Component({
  selector: 'app-root',
  imports: [ClientReadiness, Dashboard],
  template: `
    <h1>Client readiness</h1>
    <app-client-readiness />
    <app-dashboard />
  `,
})
export class App {}
