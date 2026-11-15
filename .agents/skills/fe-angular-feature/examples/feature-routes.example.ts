import { Routes } from '@angular/router';
import { permissionGuard } from '@core/guards';
import { Permissions } from '@core/constants/permissions.constants';

export const FEATURE_ROUTES: Routes = [
  {
    path: '',
    canActivate: [permissionGuard],
    data: { permissions: [Permissions.Features.Read] },
    loadComponent: () =>
      import('../components/feature-list.component').then(
        (m) => m.FeatureListComponent
      ),
  },
  {
    path: ':id',
    canActivate: [permissionGuard],
    data: { permissions: [Permissions.Features.Read] },
    loadComponent: () =>
      import('../components/feature-detail.component').then(
        (m) => m.FeatureDetailComponent
      ),
  },
];
