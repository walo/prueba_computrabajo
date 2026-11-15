import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AdStatusPageComponent, StatusCode } from '@accionadev/ui-kit';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-status-page',
  standalone: true,
  imports: [AdStatusPageComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="min-h-screen bg-background flex items-center justify-center p-4">
      <ad-status-page
        [code]="statusCode()"
        [actionLabel]="actionLabel()"
        (actionClick)="navigateBack()" />
    </div>
  `,
})
export class StatusPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  protected readonly statusCode = computed<StatusCode>(() => {
    const raw = this.route.snapshot.data['statusCode'] as string | undefined;

    return raw === '403' ? '403' : '404';
  });

  protected readonly actionLabel = computed(() =>
    this.authService.isAuthenticated() ? 'Volver al tablero' : 'Ir al inicio de sesión'
  );

  protected navigateBack(): void {
    const target = this.authService.isAuthenticated() ? '/dashboard' : '/auth/login';
    void this.router.navigate([target]);
  }
}
