import { inject } from '@angular/core';
import { RedirectFunction } from '@angular/router';
import { NgxPermissionsService } from 'ngx-permissions';
import { PERMISSIONS } from '../constants/permissions.constants';

export const featureRedirectGuard: RedirectFunction = () => {
  const permissionsService = inject(NgxPermissionsService);
  const permissions = permissionsService.getPermissions();
  if (
    permissions[PERMISSIONS.PRODUCTS.READ] ||
    permissions[PERMISSIONS.CATEGORIES.READ] ||
    permissions[PERMISSIONS.USERS.READ]
  ) {
    return '/dashboard';
  }

  return '/forbidden';
};
