import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import errorMessages from '../config/error-messages.json';

export interface UiErrorMessage {
  title: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {
  private readonly errorCatalog = errorMessages.ERRORS;

  private readonly excludedRoutes: string[] = [
    '/auth/refresh-token',
    '/auth/validate-token',
  ];

  public resolveError(error: HttpErrorResponse, requestUrl: string): UiErrorMessage | null {
    // 1. Silencioso para rutas de background
    if (this.excludedRoutes.some((route) => requestUrl.includes(route))) {
      return null;
    }

    // 2. Error de red (status 0)
    if (error.status === 0) {
      return {
        title: 'Error de Red',
        message: 'No fue posible establecer conexión con el servidor. Verifica tu conexión a internet.',
      };
    }

    // 3. Error de autorización (status 403)
    if (error.status === 403) {
      return {
        title: 'Acceso Denegado',
        message: 'No posees los permisos necesarios para realizar esta acción.',
      };
    }

    // 4. Error interno del servidor (status 500)
    if (error.status === 500) {
      return {
        title: 'Error de Servidor',
        message: 'Ocurrió un error inesperado en el servidor. Por favor intenta más tarde.',
      };
    }

    // 5. Error de dominio controlado (RFC 7807 code)
    const errorCode = (error.error?.code || error.error?.extensions?.code) as keyof typeof this.errorCatalog;
    let message = this.errorCatalog[errorCode];

    if (message) {
      const metadata: Record<string, string> = error.error?.metadata || {};
      Object.entries(metadata).forEach(([key, value]) => {
        message = message.replace(new RegExp(`\\{${key}\\}`, 'g'), value);
      });
      return { title: 'Validación', message };
    }

    return {
      title: 'Error',
      message: error.error?.detail || this.errorCatalog['DEFAULT'] || 'Operación no completada.',
    };
  }
}
