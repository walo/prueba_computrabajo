import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { NgxPermissionsModule } from 'ngx-permissions';
import { AdButtonComponent, AdModalComponent } from '@accionadev/ui-kit';

import { FeatureHttpService } from './service-http.example';
import { NotificationService } from '@core/services/notification.service';
import { trackLoading } from '@core/utils/loading.utils';

/**
 * FeatureListComponent — Smart Component representativo de una feature.
 * Conecta rxResource, control de acceso PBAC, y componentes oficiales de @accionadev/ui-kit (<ad-button>, <ad-modal>).
 */
@Component({
  selector: 'app-feature-list',
  standalone: true,
  imports: [
    NgxPermissionsModule,
    AdButtonComponent,
    AdModalComponent,
  ],
  template: `
    <div class="p-6 max-w-7xl mx-auto space-y-6">
      <!-- Encabezado con título y acciones protegidas por permisos -->
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold tracking-tight text-foreground">Listado de Registros</h1>
          <p class="text-sm text-foreground-muted">Gestión de recursos y operaciones del módulo</p>
        </div>

        <ad-button
          *ngxPermissionsOnly="['Features.Create']"
          variant="primary"
          size="md"
          (clicked)="openCreateModal()">
          Crear Registro
        </ad-button>
      </div>

      <!-- Estado asíncrono gestionado declarativamente vía rxResource -->
      @if (itemsResource.isLoading()) {
        <div class="h-64 flex items-center justify-center bg-surface rounded-ad-xl border border-border shadow-fluent">
          <span class="text-sm text-foreground-muted">Cargando registros...</span>
        </div>
      } @else if (itemsResource.error()) {
        <div class="p-4 bg-status-danger/10 text-status-danger border border-status-danger/20 rounded-ad-lg text-sm">
          Ocurrió un error al cargar la información.
        </div>
      } @else {
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (item of itemsResource.value()?.data ?? []; track item.id) {
            <div class="p-6 rounded-ad-xl bg-surface border border-border shadow-fluent flex flex-col justify-between gap-4">
              <div>
                <h3 class="text-base font-semibold text-foreground">{{ item.name }}</h3>
                <p class="text-sm text-foreground-muted">{{ item.description }}</p>
              </div>
              <div class="flex justify-end">
                <ad-button
                  *ngxPermissionsOnly="['Features.Delete']"
                  variant="danger"
                  size="sm"
                  (clicked)="promptDelete(item.id)">
                  Eliminar
                </ad-button>
              </div>
            </div>
          }
        </div>
      }

      <!-- Modal oficial de @accionadev/ui-kit para operaciones de confirmación -->
      <ad-modal
        [open]="isDeleteModalOpen()"
        title="Confirmar Eliminación"
        size="sm"
        (closed)="cancelDelete()">
        <p class="text-sm text-foreground-muted">
          ¿Estás seguro de que deseas eliminar este registro? Esta acción es irreversible.
        </p>

        <div modal-footer class="flex justify-end gap-3">
          <ad-button variant="secondary" size="md" [disabled]="isDeleting()" (clicked)="cancelDelete()">
            Cancelar
          </ad-button>
          <ad-button variant="danger" size="md" [loading]="isDeleting()" (clicked)="confirmDelete()">
            Eliminar
          </ad-button>
        </div>
      </ad-modal>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FeatureListComponent {
  private readonly featureService = inject(FeatureHttpService);
  private readonly notificationService = inject(NotificationService);

  readonly itemsResource = rxResource({
    loader: () => this.featureService.getItems(),
  });

  readonly isDeleteModalOpen = signal(false);
  readonly isDeleting = signal(false);
  readonly selectedItemId = signal<string | null>(null);

  openCreateModal(): void {
    // Abrir modal o navegar a formulario
  }

  promptDelete(id: string): void {
    this.selectedItemId.set(id);
    this.isDeleteModalOpen.set(true);
  }

  cancelDelete(): void {
    this.isDeleteModalOpen.set(false);
    this.selectedItemId.set(null);
  }

  confirmDelete(): void {
    const id = this.selectedItemId();
    if (!id || this.isDeleting()) return;

    this.featureService
      .deleteItem(id)
      .pipe(trackLoading(this.isDeleting))
      .subscribe({
        next: () => {
          this.notificationService.success('Registro eliminado exitosamente');
          this.isDeleteModalOpen.set(false);
          this.itemsResource.reload();
        },
      });
  }
}
