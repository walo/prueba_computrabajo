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
  return url.includes(API_PATHS.AUTH.LOGIN) || url.includes(API_PATHS.AUTH.REFRESH);
}

function attachBearerToken(
  request: HttpRequest<unknown>,
  token: string
): HttpRequest<unknown> {
  return request.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

function createRefreshStream(authService: AuthService, router: Router): Observable<string> {
  return authService.refreshTokens().pipe(
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

function retryWithRefresh(
  invocation: { request: HttpRequest<unknown>; next: HttpHandlerFn },
  session: { authService: AuthService; router: Router }
): Observable<HttpEvent<unknown>> {
  refreshInFlight$ ??= createRefreshStream(session.authService, session.router);

  return refreshInFlight$.pipe(
    take(1),
    switchMap((token) => invocation.next(attachBearerToken(invocation.request, token)))
  );
}

function notifyHttpError(
  failure: { error: HttpErrorResponse; requestUrl: string },
  handlers: { errorHandler: ErrorHandlerService; notifications: NotificationService }
): void {
  const uiError = handlers.errorHandler.resolveError(failure.error, failure.requestUrl);
  if (uiError) {
    handlers.notifications.error(uiError.title, uiError.message);
  }
}

function handleHttpError(
  error: unknown,
  context: {
    request: HttpRequest<unknown>;
    next: HttpHandlerFn;
    storage: StorageService;
    authService: AuthService;
    errorHandler: ErrorHandlerService;
    notifications: NotificationService;
    router: Router;
  }
): Observable<HttpEvent<unknown>> {
  if (!(error instanceof HttpErrorResponse)) {
    return throwError(() => error);
  }
  const canRefresh =
    error.status === 401 && !isPublicAuthEndpoint(context.request.url) && Boolean(context.storage.getRefreshToken());
  if (canRefresh) {
    return retryWithRefresh(context, context);
  }
  notifyHttpError({ error, requestUrl: context.request.url }, context);

  return throwError(() => error);
}

export const tokenInterceptor: HttpInterceptorFn = (request, next) => {
  const context = {
    request,
    next,
    storage: inject(StorageService),
    authService: inject(AuthService),
    errorHandler: inject(ErrorHandlerService),
    notifications: inject(NotificationService),
    router: inject(Router),
  };
  const token = context.storage.getAccessToken();
  const outbound = token && !isPublicAuthEndpoint(request.url) ? attachBearerToken(request, token) : request;

  return next(outbound).pipe(catchError((error: unknown) => handleHttpError(error, context)));
};
