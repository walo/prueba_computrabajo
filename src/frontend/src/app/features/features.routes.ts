import { Routes } from '@angular/router';
import { PERMISSIONS } from '../core/constants/permissions.constants';
import { permissionGuard } from '../core/guards/permission.guard';

export const FEATURES_ROUTES: Routes = [
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'categories',
    canActivate: [permissionGuard],
    data: { permissions: [PERMISSIONS.CATEGORIES.READ] },
    loadComponent: () =>
      import('./categories/pages/categories-page.component').then(
        (m) => m.CategoriesPageComponent
      ),
  },
  {
    path: 'users',
    canActivate: [permissionGuard],
    data: { permissions: [PERMISSIONS.USERS.READ] },
    loadComponent: () =>
      import('./dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
];
