import { Component } from '@angular/core';
import { Dashboard } from './dashboard/dashboard';

@Component({
  selector: 'app-root',
  imports: [Dashboard],
  template: `
    <h1>Client readiness</h1>
    <app-dashboard />
  `,
})
export class App {}
