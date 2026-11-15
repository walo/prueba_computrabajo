import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { publicGuard } from './core/guards/public.guard';

export const routes: Routes = [
  {
    path: 'auth',
    canActivate: [publicGuard],
    loadChildren: () => import('./auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'forbidden',
    data: { statusCode: '403' },
    loadComponent: () =>
      import('./shared/pages/status-page/status-page.component').then(
        (m) => m.StatusPageComponent
      ),
  },
  {
    path: 'not-found',
    data: { statusCode: '404' },
    loadComponent: () =>
      import('./shared/pages/status-page/status-page.component').then(
        (m) => m.StatusPageComponent
      ),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    loadChildren: () =>
      import('./features/features.routes').then((m) => m.FEATURES_ROUTES),
  },
  {
    path: '**',
    redirectTo: 'not-found',
  },
];
