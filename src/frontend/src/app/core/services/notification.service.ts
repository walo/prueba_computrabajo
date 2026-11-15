import { Injectable, signal } from '@angular/core';
import { ToastMessage } from '../models/toast-message.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly toastsSignal = signal<ToastMessage[]>([]);
  private nextId = 0;
  public readonly toasts = this.toastsSignal.asReadonly();

  public show(
    notification: Omit<ToastMessage, 'id'>,
    durationMs = 5000
  ): void {
    const id = `toast-${++this.nextId}-${Date.now()}`;
    const entry: ToastMessage = { id, ...notification };
    this.toastsSignal.update((items) => [...items, entry]);
    if (durationMs > 0) {
      setTimeout(() => this.dismiss(id), durationMs);
    }
  }

  public success(title: string, message: string): void {
    this.show({ variant: 'success', title, message });
  }

  public error(title: string, message: string): void {
    this.show({ variant: 'error', title, message }, 7000);
  }

  public warning(title: string, message: string): void {
    this.show({ variant: 'warning', title, message });
  }

  public info(title: string, message: string): void {
    this.show({ variant: 'info', title, message });
  }

  public dismiss(id: string): void {
    this.toastsSignal.update((items) => items.filter((item) => item.id !== id));
  }
}
