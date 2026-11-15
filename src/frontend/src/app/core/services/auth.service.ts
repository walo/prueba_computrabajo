import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { NgxPermissionsService } from 'ngx-permissions';
import { catchError, firstValueFrom, Observable, of, switchMap, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_PATHS } from '../constants/api-paths.constants';
import { AuthTokensResponse } from '../models/auth-tokens-response.model';
import { CurrentUserResponse } from '../models/current-user-response.model';
import { LoginRequest } from '../models/login-request.model';
import { createSilentContext } from '../utils/loading.utils';
import { StorageService } from './storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(StorageService);
  private readonly permissions = inject(NgxPermissionsService);
  private readonly router = inject(Router);
  private readonly currentUserSignal = signal<CurrentUserResponse | null>(null);
  public readonly currentUser = this.currentUserSignal.asReadonly();
  public readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);

  public async initializeSession(): Promise<void> {
    if (!this.storage.getAccessToken() && !this.storage.getRefreshToken()) {
      this.clearSessionState();
      return;
    }
    try {
      await firstValueFrom(this.fetchCurrentUser());
    } catch {
      this.clearSessionState();
    }
  }

  public login(credentials: LoginRequest): Observable<CurrentUserResponse> {
    return this.http
      .post<AuthTokensResponse>(
        `${environment.apiUrl}${API_PATHS.AUTH.LOGIN}`,
        credentials
      )
      .pipe(
        tap((tokens) => this.storage.setTokens(tokens.accessToken, tokens.refreshToken)),
        switchMap(() => this.fetchCurrentUser())
      );
  }

  public fetchCurrentUser(): Observable<CurrentUserResponse> {
    return this.http
      .get<CurrentUserResponse>(`${environment.apiUrl}${API_PATHS.AUTH.ME}`, {
        context: createSilentContext(),
      })
      .pipe(tap((user) => this.applyUserSession(user)));
  }

  public refreshTokens(): Observable<AuthTokensResponse> {
    const refreshToken = this.storage.getRefreshToken() ?? '';

    return this.http
      .post<AuthTokensResponse>(
        `${environment.apiUrl}${API_PATHS.AUTH.REFRESH}`,
        { refreshToken },
        { context: createSilentContext() }
      )
      .pipe(
        tap((tokens) => this.storage.setTokens(tokens.accessToken, tokens.refreshToken))
      );
  }

  public logout(): void {
    this.http
      .post<void>(`${environment.apiUrl}${API_PATHS.AUTH.LOGOUT}`, {}, {
        context: createSilentContext(),
      })
      .pipe(catchError(() => of(void 0)))
      .subscribe(() => this.completeLogout());
  }

  public clearSessionState(): void {
    this.storage.clearTokens();
    this.currentUserSignal.set(null);
    this.permissions.flushPermissions();
  }

  private applyUserSession(user: CurrentUserResponse): void {
    this.currentUserSignal.set(user);
    this.permissions.loadPermissions(user.permissions);
  }

  private completeLogout(): void {
    this.clearSessionState();
    void this.router.navigate(['/auth/login']);
  }
}
