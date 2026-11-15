import { importProvidersFrom, provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, RouterStateSnapshot, UrlTree } from '@angular/router';
import { NgxPermissionsModule, NgxPermissionsService } from 'ngx-permissions';
import { PERMISSIONS } from '../constants/permissions.constants';
import { permissionGuard } from './permission.guard';

describe('permissionGuard', () => {
  let permissionsService: NgxPermissionsService;
  const mockState = {} as RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        importProvidersFrom(NgxPermissionsModule.forRoot()),
      ],
    });
    permissionsService = TestBed.inject(NgxPermissionsService);
  });

  it('should allow activation when user holds the required permission', () => {
    permissionsService.loadPermissions([PERMISSIONS.PRODUCTS.READ]);
    const route = { data: { permissions: [PERMISSIONS.PRODUCTS.READ] } } as unknown as ActivatedRouteSnapshot;

    const result = TestBed.runInInjectionContext(() => permissionGuard(route, mockState));

    expect(result).toBe(true);
  });

  it('should redirect to /forbidden when user lacks the required permission', () => {
    permissionsService.flushPermissions();
    const route = { data: { permissions: [PERMISSIONS.USERS.READ] } } as unknown as ActivatedRouteSnapshot;

    const result = TestBed.runInInjectionContext(() => permissionGuard(route, mockState));

    expect(result instanceof UrlTree).toBe(true);
    expect(result.toString()).toBe('/forbidden');
  });
});
