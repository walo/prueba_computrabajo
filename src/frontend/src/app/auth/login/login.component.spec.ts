import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  const mockAuthService = { login: jest.fn() };
  const mockNotifications = { success: jest.fn(), error: jest.fn() };

  beforeEach(() => {
    jest.clearAllMocks();
    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService },
        { provide: NotificationService, useValue: mockNotifications },
      ],
    });
  });

  it('should authenticate valid credentials and navigate to /dashboard', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const router = TestBed.inject(Router);
    const navigateSpy = jest.spyOn(router, 'navigate').mockResolvedValue(true);
    mockAuthService.login.mockReturnValue(
      of({ userId: '1', email: 'admin@enterprise.local', fullName: 'Admin User', permissions: ['Users.Read'] })
    );
    fixture.detectChanges();
    const component = fixture.componentInstance as unknown as {
      loginForm: { setValue: (v: { email: string; password: string }) => void };
      onSubmit: () => void;
    };
    component.loginForm.setValue({ email: 'admin@enterprise.local', password: 'Password123!' });
    component.onSubmit();
    expect(mockAuthService.login).toHaveBeenCalledWith({ email: 'admin@enterprise.local', password: 'Password123!' });
    expect(mockNotifications.success).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should render an alert when authentication fails', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    mockAuthService.login.mockReturnValue(throwError(() => new Error('Invalid')));
    fixture.detectChanges();
    const component = fixture.componentInstance as unknown as {
      loginForm: { setValue: (v: { email: string; password: string }) => void };
      onSubmit: () => void;
      errorMessage: () => string | null;
    };
    component.loginForm.setValue({ email: 'admin@enterprise.local', password: 'wrong-password' });
    component.onSubmit();
    fixture.detectChanges();
    expect(component.errorMessage()).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain('Se produjo un error inesperado');
  });
});
