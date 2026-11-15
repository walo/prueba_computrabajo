import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { AdAppShellComponent, AdAppShellNavItem } from '@accionadev/ui-kit';
import { filter } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PERMISSIONS } from '../../core/constants/permissions.constants';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

/**
 * ShellComponent — Layout privado autenticado basado en <ad-app-shell> de @accionadev/ui-kit.
 * Incluye Sidebar colapsable, Topbar con Dropdown de Perfil (Modo Oscuro + Cerrar sesión) y Footer.
 */
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, AdAppShellComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="h-screen w-screen overflow-hidden">
      <ad-app-shell
        [appName]="appName"
        [appSubtitle]="appSubtitle"
        [logoUrl]="logoUrl"
        [logoIcon]="logoIcon"
        [pageTitle]="pageTitle()"
        [navItems]="navItems()"
        [userName]="currentUser()?.fullName ?? 'Usuario'"
        [userEmail]="currentUser()?.email ?? ''"
        userRole="Sesión Activa PBAC"
        [companyName]="companyName"
        [version]="appVersion"
        (navItemSelect)="onNavItemSelect($event)"
        (profileClick)="onProfileClick()"
        (settingsClick)="onSettingsClick()"
        (logoutClick)="logout()"
      >
        <router-outlet />
      </ad-app-shell>
    </div>
  `,
})
export class ShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notification = inject(NotificationService);

  readonly appName = environment.appName;
  readonly appSubtitle = environment.appSubtitle;
  readonly logoUrl = environment.logoUrl;
  readonly logoIcon = environment.logoIcon;
  readonly companyName = environment.companyName;
  readonly appVersion = `v${environment.version}`;

  readonly currentUser = this.authService.currentUser;
  readonly currentUrl = signal<string>(this.router.url || '/dashboard');

  readonly navItems = computed<AdAppShellNavItem[]>(() => {
    const url = this.currentUrl();
    const userPerms = this.currentUser()?.permissions ?? [];

    const items: AdAppShellNavItem[] = [
      {
        id: 'dashboard',
        label: 'Dashboard',
        route: '/dashboard',
        icon: 'dashboard',
        active: url.startsWith('/dashboard'),
      },
    ];

    if (userPerms.includes(PERMISSIONS.USERS.READ) || userPerms.includes(PERMISSIONS.ROLES.READ)) {
      items.push({
        id: 'admin',
        label: 'Administración',
        route: '/dashboard',
        icon: 'shield',
        badge: String(userPerms.length),
        active: url.startsWith('/admin'),
      });
    }

    return items;
  });

  readonly pageTitle = computed(() => {
    const active = this.navItems().find((i) => i.active);
    return active?.label ?? 'Panel Ejecutivo';
  });

  constructor() {
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e) => {
        this.currentUrl.set(e.urlAfterRedirects);
      });
  }

  onNavItemSelect(item: AdAppShellNavItem): void {
    if (item.route) {
      void this.router.navigateByUrl(item.route);
    }
  }

  onProfileClick(): void {
    this.notification.info('Módulo de Perfil de Usuario listo para configurar.');
  }

  onSettingsClick(): void {
    this.notification.info('Panel de Configuración del Sistema.');
  }

  logout(): void {
    this.authService.logout();
  }
}
