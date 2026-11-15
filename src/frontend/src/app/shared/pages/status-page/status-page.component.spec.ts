import { provideZonelessChangeDetection, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { StatusPageComponent } from './status-page.component';

describe('StatusPageComponent', () => {
  const isAuthSignal = signal(true);

  beforeEach(() => {
    isAuthSignal.set(true);
    TestBed.configureTestingModule({
      imports: [StatusPageComponent],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { data: { statusCode: '403' } } } },
        { provide: AuthService, useValue: { isAuthenticated: isAuthSignal.asReadonly() } },
      ],
    });
  });

  it('should render 403 status content and navigate to /dashboard when authenticated', () => {
    const fixture = TestBed.createComponent(StatusPageComponent);
    const router = TestBed.inject(Router);
    const navigateSpy = jest.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    button.click();
    expect(fixture.nativeElement.textContent).toContain('Acceso Denegado');
    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should navigate to /auth/login when unauthenticated', () => {
    isAuthSignal.set(false);
    const fixture = TestBed.createComponent(StatusPageComponent);
    const router = TestBed.inject(Router);
    const navigateSpy = jest.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    button.click();
    expect(navigateSpy).toHaveBeenCalledWith(['/auth/login']);
  });
});
