import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import errorMessages from '../config/error-messages.json';
import { UiErrorMessage } from '../models/ui-error-message.model';

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {
  private readonly errorCatalog: Record<string, string> = errorMessages.ERRORS;
  private readonly excludedRoutes: string[] = ['/auth/refresh', '/auth/me'];

  public resolveError(
    error: HttpErrorResponse,
    requestUrl: string
  ): UiErrorMessage | null {
    if (this.excludedRoutes.some((route) => requestUrl.includes(route))) {
      return null;
    }
    const statusError = this.resolveStatusError(error.status);
    if (statusError) {
      return statusError;
    }

    return this.resolveDomainError(error);
  }

  private resolveStatusError(status: number): UiErrorMessage | null {
    if (status === 0) {
      return {
        title: 'Error de Red',
        message: 'No fue posible establecer conexión con el servidor. Verifica tu conexión a internet.',
      };
    }
    if (status === 403) {
      return {
        title: 'Acceso Denegado',
        message: 'No posees los permisos necesarios para realizar esta acción.',
      };
    }
    if (status >= 500) {
      return {
        title: 'Error de Servidor',
        message: 'Ocurrió un error inesperado en el servidor. Por favor intenta más tarde.',
      };
    }

    return null;
  }

  private resolveDomainError(error: HttpErrorResponse): UiErrorMessage {
    const rawError = error.error as
      | {
          code?: string;
          title?: string;
          detail?: string;
          extensions?: { code?: string };
          metadata?: Record<string, string>;
        }
      | undefined;
    const code = rawError?.code || rawError?.extensions?.code || rawError?.title || '';
    const template = this.errorCatalog[code];
    if (template) {
      return {
        title: 'Validación',
        message: this.interpolateMetadata(template, rawError?.metadata),
      };
    }

    return {
      title: 'Error',
      message: rawError?.detail || this.errorCatalog['DEFAULT'] || 'Operación no completada.',
    };
  }

  private interpolateMetadata(
    template: string,
    metadata?: Record<string, string>
  ): string {
    if (!metadata) {
      return template;
    }
    let resolved = template;
    for (const [key, value] of Object.entries(metadata)) {
      resolved = resolved.replace(new RegExp(`\\{${key}\\}`, 'g'), value);
    }

    return resolved;
  }
}
