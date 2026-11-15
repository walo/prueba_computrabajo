import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { importProvidersFrom, provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NgxPermissionsModule, NgxPermissionsService } from 'ngx-permissions';
import { environment } from '../../../environments/environment';
import { API_PATHS } from '../constants/api-paths.constants';
import { AuthService } from './auth.service';
import { StorageService } from './storage.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let storage: StorageService;
  let permissionsService: NgxPermissionsService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        importProvidersFrom(NgxPermissionsModule.forRoot()),
      ],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    storage = TestBed.inject(StorageService);
    permissionsService = TestBed.inject(NgxPermissionsService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should login, store tokens, fetch current user, and load permissions', () => {
    let resolvedEmail = '';
    service.login({ email: 'admin@enterprise.local', password: 'Password123!' }).subscribe((user) => {
      resolvedEmail = user.email;
    });

    const loginReq = httpMock.expectOne(`${environment.apiUrl}${API_PATHS.AUTH.LOGIN}`);
    expect(loginReq.request.method).toBe('POST');
    loginReq.flush({
      accessToken: 'jwt-token',
      refreshToken: 'refresh-token',
      expiresAtUtc: '2026-12-01T00:00:00Z',
    });

    const meReq = httpMock.expectOne(`${environment.apiUrl}${API_PATHS.AUTH.ME}`);
    expect(meReq.request.method).toBe('GET');
    meReq.flush({
      userId: '11111111-1111-1111-1111-111111111111',
      email: 'admin@enterprise.local',
      fullName: 'System Administrator',
      permissions: ['Products.Read', 'Users.Read'],
    });

    expect(resolvedEmail).toBe('admin@enterprise.local');
    expect(storage.getAccessToken()).toBe('jwt-token');
    expect(service.isAuthenticated()).toBe(true);
    expect(permissionsService.getPermission('Products.Read')).toBeDefined();
  });

  it('should clear session when initializeSession runs without tokens', async () => {
    await service.initializeSession();

    expect(service.isAuthenticated()).toBe(false);
    expect(service.currentUser()).toBeNull();
  });
});
