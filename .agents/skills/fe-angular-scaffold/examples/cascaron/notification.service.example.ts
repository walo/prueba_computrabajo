import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastMessage {
  id: string;
  type: ToastType;
  message: string;
}

/**
 * NotificationService — Servicio reactivo singleton para feedback visual (Toasts).
 * Gestiona alertas transitorias mediante Signals sin acoplarse a librerías externas.
 */
@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly _toasts = signal<ToastMessage[]>([]);
  readonly toasts = this._toasts.asReadonly();

  show(message: string, type: ToastType = 'info', durationMs = 4000): void {
    const id = crypto.randomUUID();
    this._toasts.update((current) => [...current, { id, type, message }]);

    if (durationMs > 0) {
      setTimeout(() => this.dismiss(id), durationMs);
    }
  }

  success(message: string, durationMs = 4000): void {
    this.show(message, 'success', durationMs);
  }

  error(message: string, durationMs = 5000): void {
    this.show(message, 'error', durationMs);
  }

  warning(message: string, durationMs = 4500): void {
    this.show(message, 'warning', durationMs);
  }

  info(message: string, durationMs = 4000): void {
    this.show(message, 'info', durationMs);
  }

  dismiss(id: string): void {
    this._toasts.update((current) => current.filter((toast) => toast.id !== id));
  }
}
