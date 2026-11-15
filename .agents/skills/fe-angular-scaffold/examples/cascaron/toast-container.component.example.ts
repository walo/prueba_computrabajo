import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AdAlertComponent, AlertVariant } from '@accionadev/ui-kit';
import { NotificationService, ToastType } from '../../core/services/notification.service';

/**
 * ToastContainerComponent — Contenedor flotante superior derecho para notificaciones.
 * Reutiliza <ad-alert> de @accionadev/ui-kit sobre superficie elevada bg-surface.
 */
@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [AdAlertComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      class="fixed top-20 right-6 z-50 flex flex-col gap-3 max-w-sm w-full pointer-events-none"
      aria-live="polite"
    >
      @for (toast of notificationService.toasts(); track toast.id) {
        <div class="pointer-events-auto bg-surface rounded-ad-lg shadow-fluent-lg overflow-hidden">
          <ad-alert
            [variant]="mapVariant(toast.type)"
            [dismissible]="true"
            (closed)="notificationService.dismiss(toast.id)"
          >
            {{ toast.message }}
          </ad-alert>
        </div>
      }
    </div>
  `,
})
export class ToastContainerComponent {
  readonly notificationService = inject(NotificationService);

  mapVariant(type: ToastType): AlertVariant {
    return type === 'error' ? 'danger' : type;
  }
}
