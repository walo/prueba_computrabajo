// NOTA: Generar 1 archivo físico por Guard en src/app/core/guards/:
// - auth.guard.ts
// - public.guard.ts
// - permission.guard.ts

import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { NgxPermissionsService } from 'ngx-permissions';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  return authService.isAuthenticated() ? true : router.createUrlTree(['/auth/login']);
};

export const publicGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  return authService.isAuthenticated() ? router.createUrlTree(['/dashboard']) : true;
};

export const permissionGuard: CanActivateFn = async (route: ActivatedRouteSnapshot) => {
  const permissionsService = inject(NgxPermissionsService);
  const router = inject(Router);

  const requiredPermissions = route.data['permissions'] as string[] | undefined;
  if (!requiredPermissions || requiredPermissions.length === 0) {
    return true;
  }

  const hasPermission = await permissionsService.hasPermission(requiredPermissions);
  return hasPermission ? true : router.createUrlTree(['/forbidden']);
};
