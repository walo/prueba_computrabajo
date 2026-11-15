import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models/problem-details.model';

export type ErrorCatalog = Record<string, string>;

export const DEFAULT_FALLBACK_MESSAGE =
  'Se produjo un error inesperado al procesar la solicitud.';

function extractProblemMessage(
  problem: ProblemDetails | undefined,
  catalog: ErrorCatalog
): string | null {
  const code = problem?.extensions?.code;
  if (code && catalog[code]) {
    return catalog[code];
  }
  if (problem?.detail) {
    return problem.detail;
  }

  return null;
}

export function mapHttpErrorToMessage(
  error: unknown,
  catalog: ErrorCatalog = {}
): string {
  if (!(error instanceof HttpErrorResponse)) {
    return catalog['DEFAULT'] || DEFAULT_FALLBACK_MESSAGE;
  }
  if (error.status === 0) {
    return 'No fue posible conectar con el servidor. Verifica tu conexión a internet.';
  }
  if (error.status === 403) {
    return 'No tienes los permisos requeridos para ejecutar esta operación.';
  }
  const problem = error.error as ProblemDetails | undefined;

  return extractProblemMessage(problem, catalog) ?? (catalog['DEFAULT'] || DEFAULT_FALLBACK_MESSAGE);
}
