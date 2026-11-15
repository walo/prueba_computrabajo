import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { LoaderService } from '../../core/services/loader.service';

/**
 * GlobalLoaderComponent — Indicador global de carga estilo Fluent 2 / Linear.
 * Combina barra superior láser de progreso indeterminado + cápsula glassmorphic
 * con doble anillo orbital concéntrico y núcleo pulsante.
 */
@Component({
  selector: 'app-global-loader',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [
    `
      @keyframes ad-progress-bar {
        0% {
          transform: translateX(-100%);
        }
        50% {
          transform: translateX(0%);
        }
        100% {
          transform: translateX(100%);
        }
      }
      @keyframes ad-spin-reverse {
        from {
          transform: rotate(360deg);
        }
        to {
          transform: rotate(0deg);
        }
      }
      .animate-ad-progress {
        animation: ad-progress-bar 1.4s cubic-bezier(0.4, 0, 0.2, 1) infinite;
      }
      .animate-ad-spin-reverse {
        animation: ad-spin-reverse 1.8s linear infinite;
      }
    `,
  ],
  template: `
    @if (loaderService.isLoading()) {
      <!-- Top Laser Progress Bar -->
      <div class="fixed top-0 left-0 right-0 z-[70] h-1 bg-primary-soft overflow-hidden pointer-events-none">
        <div class="h-full w-1/2 bg-primary rounded-full animate-ad-progress"></div>
      </div>

      <!-- Glassmorphic Orbital Loader Overlay -->
      <div
        class="fixed inset-0 z-[65] flex items-center justify-center bg-black/45 backdrop-blur-sm transition-opacity duration-200"
        role="status"
        aria-live="polite"
        aria-label="Procesando solicitud"
      >
        <div
          class="flex items-center gap-4 px-6 py-4 rounded-ad-xl bg-surface/90 backdrop-blur-md border border-border shadow-fluent-dialog"
        >
          <!-- Dual Concentric Orbital Spinner + Brand Core -->
          <div class="relative flex items-center justify-center w-11 h-11 shrink-0">
            <svg class="w-11 h-11 text-primary animate-spin" viewBox="0 0 44 44" fill="none">
              <circle
                cx="22"
                cy="22"
                r="19"
                stroke="currentColor"
                stroke-width="2.5"
                class="opacity-15"
              />
              <path
                d="M22 3a19 19 0 0 1 19 19"
                stroke="currentColor"
                stroke-width="2.5"
                stroke-linecap="round"
              />
            </svg>

            <svg
              class="absolute w-7 h-7 text-primary/60 animate-ad-spin-reverse"
              viewBox="0 0 28 28"
              fill="none"
            >
              <path
                d="M14 2a12 12 0 0 0-12 12"
                stroke="currentColor"
                stroke-width="2"
                stroke-linecap="round"
              />
            </svg>

            <span class="absolute w-2 h-2 rounded-full bg-primary animate-pulse"></span>
          </div>

          <div class="flex flex-col pr-1">
            <span class="text-sm font-semibold tracking-tight text-foreground">
              Cargando...
            </span>
          </div>
        </div>
      </div>
    }
  `,
})
export class GlobalLoaderComponent {
  readonly loaderService = inject(LoaderService);
}
