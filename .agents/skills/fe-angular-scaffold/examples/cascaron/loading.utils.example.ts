import { HttpContext, HttpContextToken } from '@angular/common/http';
import { WritableSignal } from '@angular/core';
import { MonoTypeOperatorFunction, Observable, defer, finalize } from 'rxjs';

/**
 * Token de HttpContext para solicitar explícitamente el bloqueo de pantalla completa (Nivel 3).
 * Por defecto es false: NINGUNA petición bloquea la pantalla completa a menos que se configure explícitamente.
 */
export const USE_GLOBAL_LOADER = new HttpContextToken<boolean>(() => false);

/**
 * Función de utilidad para crear o extender un HttpContext solicitando el loader global.
 * Uso: this.http.post('/api/export', body, { context: withGlobalLoader() })
 */
export function withGlobalLoader(context?: HttpContext): HttpContext {
  const ctx = context ?? new HttpContext();
  return ctx.set(USE_GLOBAL_LOADER, true);
}

/**
 * Operador RxJS declarativo para vincular una Signal booleana al ciclo de vida de una petición (Nivel 1).
 * Pone la signal en true al suscribirse y GARANTIZA ponerla en false al terminar
 * (ya sea por éxito, error o cancelación), evitando que los botones queden congelados.
 *
 * Uso en componentes con <ad-button [loading]="isSubmitting()">:
 *   this.service.save(data)
 *     .pipe(trackLoading(this.isSubmitting))
 *     .subscribe({ ... });
 */
export function trackLoading<T>(loadingSignal: WritableSignal<boolean>): MonoTypeOperatorFunction<T> {
  return (source: Observable<T>) =>
    defer(() => {
      loadingSignal.set(true);
      return source.pipe(
        finalize(() => {
          loadingSignal.set(false);
        })
      );
    });
}
