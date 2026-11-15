import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { NgxPermissionsService } from 'ngx-permissions';
import { Observable, catchError, finalize, firstValueFrom, of, switchMap, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_PATHS } from '../constants/api-paths.constants';
import { AuthTokensResponse, CurrentUser, LoginRequest } from '../models/auth.models';
import { StorageService } from './storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storage = inject(StorageService);
  private readonly permissionsService = inject(NgxPermissionsService);

  private readonly currentUserSignal = signal<CurrentUser | null>(null);

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);

  getAccessToken(): string | null {
    return this.storage.getAccessToken();
  }

  getRefreshToken(): string | null {
    return this.storage.getRefreshToken();
  }

  login(credentials: LoginRequest): Observable<CurrentUser> {
    return this.http
      .post<AuthTokensResponse>(`${environment.apiUrl}${API_PATHS.AUTH.LOGIN}`, credentials)
      .pipe(
        tap((tokens) => this.storage.setTokens(tokens.accessToken, tokens.refreshToken)),
        switchMap(() => this.loadCurrentUserProfile()),
      );
  }

  refreshSession(): Observable<AuthTokensResponse> {
    const refreshToken = this.storage.getRefreshToken();
    return this.http
      .post<AuthTokensResponse>(`${environment.apiUrl}${API_PATHS.AUTH.REFRESH}`, {
        refreshToken: refreshToken ?? '',
      })
      .pipe(tap((tokens) => this.storage.setTokens(tokens.accessToken, tokens.refreshToken)));
  }

  loadCurrentUserProfile(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${environment.apiUrl}${API_PATHS.AUTH.ME}`).pipe(
      tap((user) => {
        this.currentUserSignal.set(user);
        this.permissionsService.loadPermissions([...user.permissions]);
      }),
    );
  }

  async initializeSession(): Promise<void> {
    const hasStoredSession = Boolean(this.storage.getAccessToken() || this.storage.getRefreshToken());
    if (!hasStoredSession) {
      return;
    }

    await firstValueFrom(
      this.loadCurrentUserProfile().pipe(
        catchError(() => {
          this.clearLocalState();
          return of(null);
        }),
      ),
    );
  }

  logout(): void {
    const hasToken = Boolean(this.storage.getAccessToken());
    if (!hasToken) {
      this.clearSessionAndRedirect();
      return;
    }

    this.http
      .post<void>(`${environment.apiUrl}${API_PATHS.AUTH.LOGOUT}`, {})
      .pipe(
        catchError(() => of(undefined)),
        finalize(() => this.clearSessionAndRedirect()),
      )
      .subscribe();
  }

  clearSessionAndRedirect(): void {
    this.clearLocalState();
    void this.router.navigate(['/auth/login']);
  }

  private clearLocalState(): void {
    this.storage.clearTokens();
    this.currentUserSignal.set(null);
    this.permissionsService.flushPermissions();
  }
}
