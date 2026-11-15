import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoaderService } from '../services/loader.service';
import { USE_GLOBAL_LOADER } from '../utils/loading.utils';

/**
 * loaderInterceptor — Interceptor de carga global Opt-In (Nivel 3).
 *
 * REGLA ARQUITECTÓNICA DE LOS 3 NIVELES:
 * - Nivel 1 (Formularios/Botones): Se maneja localmente con trackLoading(signal) en <ad-button [loading]="...">.
 * - Nivel 2 (Listados/Tablas): Se maneja localmente con rxResource.isLoading() en <ad-table [loading]="...">.
 * - Nivel 3 (Bloqueo Global de Pantalla): SOLO se activa si la petición lleva USE_GLOBAL_LOADER = true
 *   o el header 'x-global-loader'. Por defecto NINGUNA petición bloquea la pantalla completa.
 */
export const loaderInterceptor: HttpInterceptorFn = (req, next) => {
  const loaderService = inject(LoaderService);

  const shouldTriggerGlobal =
    req.context.get(USE_GLOBAL_LOADER) ||
    req.headers.has('x-global-loader');

  if (!shouldTriggerGlobal) {
    return next(req);
  }

  const finalReq = req.headers.has('x-global-loader')
    ? req.clone({ headers: req.headers.delete('x-global-loader') })
    : req;

  loaderService.show();

  return next(finalReq).pipe(finalize(() => loaderService.hide()));
};
