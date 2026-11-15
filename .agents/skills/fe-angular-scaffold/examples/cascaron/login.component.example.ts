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
import { mapHttpErrorToMessage } from '../../core/models/problem-details.model';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { trackLoading } from '../../core/utils/loading.utils';

const AUTH_ERRORS: Record<string, string> = {
  'Auth.InvalidCredentials': 'Correo electrónico o contraseña incorrectos.',
  'Auth.UserInactive': 'Tu cuenta está desactivada. Contacta al administrador.',
};

/**
 * LoginComponent — Vista pública de autenticación usando <ad-card>, <ad-alert>, <ad-input> y <ad-button>.
 * Los campos <ad-input> detectan automáticamente Validators.required (*) y muestran errores reactivos.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AdCardComponent,
    AdAlertComponent,
    AdBrandLogoComponent,
    AdInputComponent,
    AdButtonComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <main class="min-h-screen flex items-center justify-center bg-background px-4 py-12">
      <div class="w-full max-w-md">
        <ad-card padding="lg">
          <div class="mb-6 text-center">
            <div class="mx-auto mb-3 flex justify-center">
              <ad-brand-logo
                [logoUrl]="logoUrl"
                [logoIcon]="logoIcon"
                [alt]="appName"
                size="lg"
              />
            </div>
            <h1 class="text-2xl font-bold tracking-tight text-foreground">{{ appName }}</h1>
            <p class="mt-1 text-sm text-foreground-muted">
              Inicia sesión para acceder a la plataforma
            </p>
          </div>

          @if (errorMessage(); as error) {
            <div class="mb-4">
              <ad-alert variant="danger" [dismissible]="true" (closed)="errorMessage.set(null)">
                {{ error }}
              </ad-alert>
            </div>
          }

          <form [formGroup]="form" (ngSubmit)="onSubmit()" class="flex flex-col gap-4" novalidate>
            <ad-input
              label="Correo electrónico"
              type="email"
              formControlName="email"
              placeholder="admin@enterprise.local"
            />

            <ad-input
              label="Contraseña"
              type="password"
              formControlName="password"
              placeholder="••••••••"
            />

            <div class="mt-2">
              <ad-button
                type="submit"
                variant="primary"
                size="md"
                [fullWidth]="true"
                [loading]="isSubmitting()"
              >
                Iniciar sesión
              </ad-button>
            </div>
          </form>
        </ad-card>
      </div>
    </main>
  `,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notification = inject(NotificationService);

  readonly appName = environment.appName;
  readonly logoUrl = environment.logoUrl;
  readonly logoIcon = environment.logoIcon;
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);

    const { email, password } = this.form.getRawValue();

    this.authService
      .login({ email, password })
      .pipe(trackLoading(this.isSubmitting))
      .subscribe({
        next: () => {
          this.notification.success('Sesión iniciada correctamente.');
          void this.router.navigate(['/dashboard']);
        },
        error: (err: unknown) => {
          const msg = mapHttpErrorToMessage(err, AUTH_ERRORS);
          this.errorMessage.set(msg);
        },
      });
  }
}

