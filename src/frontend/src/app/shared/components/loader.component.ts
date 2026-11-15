import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AdIconComponent } from '@accionadev/ui-kit';
import { LoaderService } from '../../core/services/loader.service';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [AdIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loader.isLoading()) {
      <div
        class="fixed inset-0 z-[110] bg-background/60 backdrop-blur-sm flex items-center justify-center"
        role="status"
        aria-live="polite"
        aria-label="Cargando">
        <div class="bg-surface border border-border rounded-ad-xl px-6 py-4 shadow-fluent-lg flex items-center gap-3">
          <ad-icon name="loader" size="md" class="text-primary animate-spin" />
          <span class="text-sm font-medium text-foreground">Procesando...</span>
        </div>
      </div>
    }
  `,
})
export class LoaderComponent {
  protected readonly loader = inject(LoaderService);
}
