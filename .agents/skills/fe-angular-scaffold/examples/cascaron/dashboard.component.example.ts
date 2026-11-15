import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AdBadgeComponent, AdButtonComponent, AdCardComponent } from '@accionadev/ui-kit';
import { NgxPermissionsModule } from 'ngx-permissions';
import { PERMISSIONS } from '../../core/constants/permissions.constants';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

/**
 * DashboardComponent — Vista ejecutiva inicial dentro de <ad-app-shell>.
 * Muestra tarjetas KPI con <ad-card> + <ad-badge> y matriz de permisos PBAC hidratados.
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [AdCardComponent, AdBadgeComponent, AdButtonComponent, NgxPermissionsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <ad-card title="Estado de Plataforma" subtitle="Arquitectura Zoneless + Onion .NET 10">
          <div card-header-actions>
            <ad-badge variant="success" [dot]="true" size="sm">Operativo</ad-badge>
          </div>
          <div class="mt-2">
            <div class="text-2xl font-bold tracking-tight text-foreground">100% Online</div>
            <p class="text-xs text-foreground-muted mt-1">
              Sesión rehidratada vía /api/v1/auth/me con rotación RTR activa.
            </p>
          </div>
        </ad-card>

        <ad-card title="Identidad Autenticada" subtitle="Usuario activo en sesión">
          <div card-header-actions>
            <ad-badge variant="primary" size="sm">Verificado</ad-badge>
          </div>
          <div class="mt-2">
            <div class="text-lg font-bold tracking-tight text-foreground truncate">
              {{ currentUser()?.fullName ?? 'Usuario' }}
            </div>
            <p class="text-xs text-foreground-muted truncate mt-1">
              {{ currentUser()?.email }}
            </p>
          </div>
        </ad-card>

        <ad-card title="Permisos PBAC Activos" subtitle="Autorización granular en memoria">
          <div card-header-actions>
            <ad-badge variant="info" size="sm">
              {{ currentUser()?.permissions?.length ?? 0 }} permisos
            </ad-badge>
          </div>
          <div class="mt-2 flex items-center justify-between gap-4">
            <div>
              <div class="text-2xl font-bold tracking-tight text-primary">
                {{ currentUser()?.permissions?.length ?? 0 }}
              </div>
              <p class="text-xs text-foreground-muted mt-1">Cero permisos en localStorage</p>
            </div>
            <ng-container *ngxPermissionsOnly="[permissions.USERS.CREATE]">
              <ad-button variant="primary" size="sm" (clicked)="onQuickAction()">
                Probar Acción PBAC
              </ad-button>
            </ng-container>
          </div>
        </ad-card>
      </div>

      <ad-card
        title="Catálogo de Permisos Hidratados (/api/v1/auth/me)"
        subtitle="Permisos cargados dinámicamente en NgxPermissionsService al iniciar o presionar F5"
      >
        <div class="flex flex-wrap gap-2 pt-1">
          @for (perm of currentUser()?.permissions ?? []; track perm) {
            <ad-badge variant="primary" [dot]="true" size="md">{{ perm }}</ad-badge>
          } @empty {
            <span class="text-sm text-foreground-muted">Sin permisos asignados.</span>
          }
        </div>
      </ad-card>
    </div>
  `,
})
export class DashboardComponent {
  private readonly authService = inject(AuthService);
  private readonly notification = inject(NotificationService);

  readonly currentUser = this.authService.currentUser;
  readonly permissions = PERMISSIONS;

  onQuickAction(): void {
    this.notification.success('Acción autorizada verificada con PBAC dinámico.');
  }
}
