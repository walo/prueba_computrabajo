import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  AdAlertComponent,
  AdBrandLogoComponent,
  AdButtonComponent,
  AdCardComponent,
  AdInputComponent,
} from '@accionadev/ui-kit';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { mapHttpErrorToMessage } from '../../core/utils/error-mapper';

const AUTH_ERROR_CATALOG: Record<string, string> = {
  'Auth.InvalidCredentials': 'Correo electrónico o contraseña incorrectos.',
};

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AdBrandLogoComponent,
    AdCardComponent,
    AdAlertComponent,
    AdInputComponent,
    AdButtonComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="min-h-screen bg-background flex items-center justify-center p-4">
      <div class="w-full max-w-md space-y-6">
        <div class="flex flex-col items-center text-center gap-3">
          <ad-brand-logo
            [logoUrl]="env.logoUrl"
            [logoIcon]="env.logoIcon"
            [alt]="env.appName"
            size="lg" />
          <div>
            <h1 class="text-2xl font-bold tracking-tight text-foreground">
              {{ env.appName }}
            </h1>
            <p class="text-sm text-foreground-muted mt-1">
              {{ env.appSubtitle }}
            </p>
          </div>
        </div>

        <ad-card title="Iniciar sesión" subtitle="Ingresa tus credenciales corporativas" padding="lg">
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-4">
            @if (errorMessage(); as error) {
              <ad-alert variant="danger" [dismissible]="true" (closed)="errorMessage.set(null)">
                {{ error }}
              </ad-alert>
            }

            <ad-input
              label="Correo electrónico"
              type="email"
              prefixIcon="mail"
              placeholder="admin@enterprise.local"
              formControlName="email"
              [required]="true" />

            <ad-input
              label="Contraseña"
              type="password"
              prefixIcon="lock"
              placeholder="••••••••"
              formControlName="password"
              [required]="true" />

            <div class="pt-2">
              <ad-button
                type="submit"
                variant="primary"
                size="md"
                [fullWidth]="true"
                [loading]="isSubmitting()"
                [disabled]="loginForm.invalid || isSubmitting()">
                Ingresar
              </ad-button>
            </div>
          </form>
        </ad-card>

        <p class="text-center text-xs text-foreground-subtle">
          {{ env.companyName }} · {{ env.version }}
        </p>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly env = environment;
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  protected onSubmit(): void {
    if (this.loginForm.invalid || this.isSubmitting()) {
      this.loginForm.markAllAsTouched();
      return;
    }
    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    const credentials = this.loginForm.getRawValue();
    this.authService
      .login(credentials)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (user) => this.handleLoginSuccess(user.fullName),
        error: (err: unknown) => this.handleLoginError(err),
      });
  }

  private handleLoginSuccess(fullName: string): void {
    this.notifications.success('Bienvenido', `Sesión iniciada como ${fullName}`);
    void this.router.navigate(['/dashboard']);
  }

  private handleLoginError(error: unknown): void {
    this.errorMessage.set(mapHttpErrorToMessage(error, AUTH_ERROR_CATALOG));
  }
}
