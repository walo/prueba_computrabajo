import { HttpErrorResponse } from '@angular/common/http';

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  extensions?: {
    code?: string;
    [key: string]: unknown;
  };
}

export type ErrorCatalog = Record<string, string>;

export const DEFAULT_FALLBACK_MESSAGE = 'Se produjo un error inesperado al procesar la solicitud.';

/**
 * Mapea un HttpErrorResponse o ProblemDetails al mensaje amigable del catálogo tipado.
 * 
 * @param error Error capturado en el interceptor o servicio HTTP.
 * @param catalog Diccionario con códigos de error de la feature (ej. AUTH_ERRORS).
 */
export function mapHttpErrorToMessage(
  error: unknown,
  catalog: ErrorCatalog = {}
): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) {
      return 'No fue posible conectar con el servidor. Verifica tu conexión a internet.';
    }

    if (error.status === 403) {
      return 'No tienes los permisos requeridos para ejecutar esta operación.';
    }

    const problem = error.error as ProblemDetails | undefined;
    const code = problem?.extensions?.code;

    if (code && catalog[code]) {
      return catalog[code];
    }

    if (problem?.detail) {
      return problem.detail;
    }
  }

  return catalog['DEFAULT'] || DEFAULT_FALLBACK_MESSAGE;
}
