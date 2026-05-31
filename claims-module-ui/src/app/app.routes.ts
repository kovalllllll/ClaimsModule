import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'claims', pathMatch: 'full' },
  {
    path: 'claims/new',
    loadChildren: () =>
      import('./features/fnol-intake/fnol-intake.module').then((m) => m.FnolIntakeModule),
  },
  {
    path: 'claims/:id',
    loadChildren: () =>
      import('./features/claim-detail/claim-detail.module').then((m) => m.ClaimDetailModule),
  },
  {
    path: 'claims',
    loadChildren: () =>
      import('./features/claims-list/claims-list.module').then((m) => m.ClaimsListModule),
  },
  { path: '**', redirectTo: 'claims' },
];
