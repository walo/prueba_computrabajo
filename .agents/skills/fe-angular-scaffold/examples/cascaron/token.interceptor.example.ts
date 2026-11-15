import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { API_PATHS } from '../constants/api-paths.constants';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

const isPublicAuthEndpoint = (url: string): boolean =>
  url.includes(API_PATHS.AUTH.LOGIN) || url.includes(API_PATHS.AUTH.REFRESH);

const addBearerHeader = (req: HttpRequest<unknown>, token: string): HttpRequest<unknown> =>
  req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

export const tokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const accessToken = authService.getAccessToken();

  const outgoingReq =
    accessToken && !isPublicAuthEndpoint(req.url) ? addBearerHeader(req, accessToken) : req;

  return next(outgoingReq).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !isPublicAuthEndpoint(req.url) &&
        !req.url.includes(API_PATHS.AUTH.LOGOUT)
      ) {
        return handleUnauthorizedWithSilentRefresh(outgoingReq, next, authService, error);
      }

      return throwError(() => error);
    }),
  );
};

function handleUnauthorizedWithSilentRefresh(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  originalError: HttpErrorResponse,
): Observable<never> | ReturnType<HttpHandlerFn> {
  if (!authService.getRefreshToken()) {
    authService.clearSessionAndRedirect();
    return throwError(() => originalError);
  }

  if (isRefreshing) {
    return refreshTokenSubject.pipe(
      filter((token): token is string => token !== null),
      take(1),
      switchMap((newToken) => next(addBearerHeader(req, newToken))),
    );
  }

  isRefreshing = true;
  refreshTokenSubject.next(null);

  return authService.refreshSession().pipe(
    switchMap((tokens) => {
      isRefreshing = false;
      refreshTokenSubject.next(tokens.accessToken);
      return next(addBearerHeader(req, tokens.accessToken));
    }),
    catchError((refreshErr: unknown) => {
      isRefreshing = false;
      authService.clearSessionAndRedirect();
      return throwError(() => refreshErr);
    }),
  );
}
