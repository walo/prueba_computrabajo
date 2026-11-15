import { ToastVariant } from './toast-variant.model';

export interface ToastMessage {
  id: string;
  variant: ToastVariant;
  title: string;
  message: string;
}
