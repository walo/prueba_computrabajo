import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { AdButtonComponent } from '@accionadev/ui-kit';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-status-page',
  standalone: true,
  imports: [AdButtonComponent],
  templateUrl: './status-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  private readonly routeData = toSignal(this.route.data, {
    initialValue: this.route.snapshot.data,
  });

  readonly code = computed(() => (this.routeData()?.['code'] as string | undefined) ?? '404');
  readonly title = computed(
    () => (this.routeData()?.['title'] as string | undefined) ?? 'Página no encontrada',
  );
  readonly message = computed(
    () =>
      (this.routeData()?.['message'] as string | undefined) ??
      'La dirección URL solicitada no existe o no tienes acceso a ella.',
  );
  readonly actionLabel = computed(() =>
    this.authService.isAuthenticated() ? 'Volver al Dashboard' : 'Ir al inicio de sesión',
  );

  navigateBack(): void {
    const target = this.authService.isAuthenticated() ? '/dashboard' : '/auth/login';
    void this.router.navigate([target]);
  }
}

