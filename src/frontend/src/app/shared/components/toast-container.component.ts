import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AdToast, AdToastComponent, ToastVariant as AdToastVariant } from '@accionadev/ui-kit';
import { ToastMessage, ToastVariant } from '../../core/models/toast-message.model';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [AdToastComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      class="fixed top-5 right-5 z-[100] flex flex-col gap-2.5 max-w-sm w-full pointer-events-none px-4 sm:px-0"
      aria-live="polite"
      aria-atomic="false">
      @for (item of notifications.toasts(); track item.id) {
        <ad-toast
          [toast]="toAdToast(item)"
          (dismiss)="notifications.dismiss($event)" />
      }
    </div>
  `,
})
export class ToastContainerComponent {
  protected readonly notifications = inject(NotificationService);

  protected toAdToast(item: ToastMessage): AdToast {
    const variants: Record<ToastVariant, AdToastVariant> = {
      success: 'success',
      error: 'danger',
      warning: 'warning',
      info: 'info',
    };

    return {
      id: item.id,
      title: item.title,
      message: item.message,
      variant: variants[item.variant],
      duration: 5000,
      dismissible: true,
      createdAt: Date.now(),
    };
  }
}
