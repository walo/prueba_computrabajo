import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  catchError,
  finalize,
  map,
  Observable,
  shareReplay,
  switchMap,
  take,
  throwError,
} from 'rxjs';
import { API_PATHS } from '../constants/api-paths.constants';
import { AuthService } from '../services/auth.service';
import { ErrorHandlerService } from '../services/error-handler.service';
import { NotificationService } from '../services/notification.service';
import { StorageService } from '../services/storage.service';

let refreshInFlight$: Observable<string> | null = null;

function isPublicAuthEndpoint(url: string): boolean {
  return (
    url.includes(API_PATHS.AUTH.LOGIN) || url.includes(API_PATHS.AUTH.REFRESH)
  );
}

function attachBearerToken(
  request: HttpRequest<unknown>,
  token: string
): HttpRequest<unknown> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function retryWithRefresh(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
): Observable<HttpEvent<unknown>> {
  if (!refreshInFlight$) {
    refreshInFlight$ = authService.refreshTokens().pipe(
      map((tokens) => tokens.accessToken),
      catchError((refreshError) => {
        authService.clearSessionState();
        void router.navigate(['/auth/login']);

        return throwError(() => refreshError);
      }),
      finalize(() => {
        refreshInFlight$ = null;
      }),
      shareReplay(1)
    );
  }

  return refreshInFlight$.pipe(
    take(1),
    switchMap((accessToken) => next(attachBearerToken(request, accessToken)))
  );
}

function notifyHttpError(
  error: HttpErrorResponse,
  requestUrl: string,
  errorHandler: ErrorHandlerService,
  notifications: NotificationService
): void {
  const uiError = errorHandler.resolveError(error, requestUrl);
  if (uiError) {
    notifications.error(uiError.title, uiError.message);
  }
}

export const tokenInterceptor: HttpInterceptorFn = (request, next) => {
  const storage = inject(StorageService);
  const authService = inject(AuthService);
  const errorHandler = inject(ErrorHandlerService);
  const notifications = inject(NotificationService);
  const router = inject(Router);
  const token = storage.getAccessToken();
  const outbound =
    token && !isPublicAuthEndpoint(request.url)
      ? attachBearerToken(request, token)
      : request;

  return next(outbound).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }
      if (
        error.status === 401 &&
        !isPublicAuthEndpoint(request.url) &&
        storage.getRefreshToken()
      ) {
        return retryWithRefresh(request, next, authService, router);
      }
      notifyHttpError(error, request.url, errorHandler, notifications);

      return throwError(() => error);
    })
  );
};
