import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { NgxPermissionsService } from 'ngx-permissions';

export const permissionGuard: CanActivateFn = (route) => {
  const permissionsService = inject(NgxPermissionsService);
  const router = inject(Router);
  const required = route.data['permissions'] as string[] | undefined;
  if (!required || required.length === 0) {
    return true;
  }
  const granted = permissionsService.getPermissions();
  const hasAny = required.some((permission) => Boolean(granted[permission]));
  if (hasAny) {
    return true;
  }

  return router.createUrlTree(['/forbidden']);
};
