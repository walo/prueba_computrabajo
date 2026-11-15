import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import {
  AdBadgeComponent,
  AdCardComponent,
  AdIconComponent,
} from '@accionadev/ui-kit';
import { NgxPermissionsModule } from 'ngx-permissions';
import { PERMISSIONS } from '../../core/constants/permissions.constants';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    NgxPermissionsModule,
    AdCardComponent,
    AdBadgeComponent,
    AdIconComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <ad-card title="Panel de Control" subtitle="Estado de plataforma e inventario" padding="lg">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div class="space-y-1">
            <p class="text-sm text-foreground-muted">Sesión activa</p>
            <p class="text-lg font-semibold text-foreground">
              {{ authService.currentUser()?.fullName || 'Usuario autenticado' }}
            </p>
            <p class="text-xs text-foreground-subtle">
              {{ authService.currentUser()?.email }}
            </p>
          </div>
          <div class="flex flex-wrap items-center gap-2">
            <ad-badge variant="success" [dot]="true">PBAC Activo</ad-badge>
            <ad-badge variant="primary">
              {{ permissionsCount() }} permisos asignados
            </ad-badge>
          </div>
        </div>
      </ad-card>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <ad-card *ngxPermissionsOnly="[permissions.PRODUCTS.READ]" title="Productos" subtitle="Catálogo base" padding="md" [hoverable]="true">
          <div class="flex items-center justify-between">
            <span class="text-sm text-foreground-muted">Gestión de SKU y stock</span>
            <ad-icon name="package" size="lg" class="text-primary" />
          </div>
        </ad-card>

        <ad-card *ngxPermissionsOnly="[permissions.CATEGORIES.READ]" title="Categorías" subtitle="Clasificación" padding="md" [hoverable]="true">
          <div class="flex items-center justify-between">
            <span class="text-sm text-foreground-muted">Agrupación de catálogo</span>
            <ad-icon name="tag" size="lg" class="text-primary" />
          </div>
        </ad-card>

        <ad-card *ngxPermissionsOnly="[permissions.INVENTORY_MOVEMENTS.READ]" title="Movimientos" subtitle="Kardex y entradas/salidas" padding="md" [hoverable]="true">
          <div class="flex items-center justify-between">
            <span class="text-sm text-foreground-muted">Trazabilidad de inventario</span>
            <ad-icon name="activity" size="lg" class="text-primary" />
          </div>
        </ad-card>

        <ad-card *ngxPermissionsOnly="[permissions.USERS.READ]" title="Usuarios" subtitle="Seguridad y roles" padding="md" [hoverable]="true">
          <div class="flex items-center justify-between">
            <span class="text-sm text-foreground-muted">Control de acceso PBAC</span>
            <ad-icon name="users" size="lg" class="text-primary" />
          </div>
        </ad-card>
      </div>
    </div>
  `,
})
export class DashboardComponent {
  protected readonly authService = inject(AuthService);
  protected readonly permissions = PERMISSIONS;
  protected readonly permissionsCount = computed(
    () => this.authService.currentUser()?.permissions.length ?? 0
  );
}
