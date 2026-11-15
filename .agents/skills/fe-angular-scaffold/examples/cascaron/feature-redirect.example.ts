import { inject } from '@angular/core';
import { RedirectFunction } from '@angular/router';
import { NgxPermissionsService } from 'ngx-permissions';

/**
 * featureRedirect — Redirección inteligente al iniciar sesión o acceder a rutas base.
 * Evalúa los permisos del usuario para dirigirlo a su módulo/feature principal.
 */
export const featureRedirect: RedirectFunction = () => {
  const permissionsService = inject(NgxPermissionsService);
  const permissions = permissionsService.getPermissions();

  if (permissions['Dashboard.View']) {
    return '/dashboard';
  }

  if (permissions['Features.Read']) {
    return '/items';
  }

  return '/unauthorized';
};
