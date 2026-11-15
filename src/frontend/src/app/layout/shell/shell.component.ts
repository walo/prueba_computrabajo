import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { AdAppShellComponent, AdAppShellNavItem } from '@accionadev/ui-kit';
import { NgxPermissionsService } from 'ngx-permissions';
import { filter, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PERMISSIONS } from '../../core/constants/permissions.constants';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, AdAppShellComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ad-app-shell
      [appName]="env.appName"
      [appSubtitle]="env.appSubtitle"
      [logoUrl]="env.logoUrl"
      [logoIcon]="env.logoIcon"
      [pageTitle]="pageTitle()"
      [navItems]="navItems()"
      [userName]="userName()"
      [userEmail]="userEmail()"
      [companyName]="env.companyName"
      [version]="env.version"
      [(collapsed)]="sidebarCollapsed"
      (navItemSelect)="onNavSelect($event)"
      (logoutClick)="onLogout()">
      <router-outlet />
    </ad-app-shell>
  `,
})
export class ShellComponent {
  private readonly authService = inject(AuthService);
  private readonly permissions = inject(NgxPermissionsService);
  private readonly router = inject(Router);
  protected readonly env = environment;
  protected readonly sidebarCollapsed = signal(false);
  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map(() => this.router.url)
    ),
    { initialValue: this.router.url }
  );
  private readonly permissionsMap = toSignal(this.permissions.permissions$, {
    initialValue: this.permissions.getPermissions(),
  });
  protected readonly userName = computed(
    () => this.authService.currentUser()?.fullName || 'Usuario'
  );
  protected readonly userEmail = computed(
    () => this.authService.currentUser()?.email || ''
  );
  protected readonly pageTitle = computed(() => {
    const url = this.currentUrl();
    if (url.startsWith('/products')) {
      return 'Catálogo de Productos';
    }
    if (url.startsWith('/categories')) {
      return 'Catálogo de Categorías';
    }
    if (url.startsWith('/inventory-movements')) {
      return 'Movimientos de Inventario';
    }

    return url.startsWith('/users') ? 'Usuarios y Permisos' : 'Tablero Principal';
  });
  protected readonly navItems = computed<AdAppShellNavItem[]>(() =>
    this.buildNavItems(this.currentUrl(), this.permissionsMap())
  );

  protected onNavSelect(item: AdAppShellNavItem): void {
    if (item.route) {
      void this.router.navigateByUrl(item.route);
    }
  }

  protected onLogout(): void {
    this.authService.logout();
  }

  private buildNavItems(
    url: string,
    granted: Record<string, unknown>
  ): AdAppShellNavItem[] {
    const dashboardItem: AdAppShellNavItem = {
      id: 'dashboard',
      label: 'Tablero',
      route: '/dashboard',
      icon: 'dashboard',
      active: url === '/dashboard' || url === '/',
    };

    return [dashboardItem, ...this.getCatalogNavItems(url, granted)];
  }

  private getCatalogNavItems(
    url: string,
    granted: Record<string, unknown>
  ): AdAppShellNavItem[] {
    const definitions = [
      { permission: PERMISSIONS.PRODUCTS.READ, id: 'products', label: 'Productos', route: '/products', icon: 'package' },
      { permission: PERMISSIONS.CATEGORIES.READ, id: 'categories', label: 'Categorías', route: '/categories', icon: 'tag' },
      { permission: PERMISSIONS.INVENTORY_MOVEMENTS.READ, id: 'inventory-movements', label: 'Movimientos', route: '/inventory-movements', icon: 'layers' },
      { permission: PERMISSIONS.USERS.READ, id: 'users', label: 'Usuarios', route: '/users', icon: 'users' },
    ];

    return definitions
      .filter((entry) => Boolean(granted[entry.permission]))
      .map((entry) => ({
        id: entry.id,
        label: entry.label,
        route: entry.route,
        icon: entry.icon,
        active: url.startsWith(entry.route),
      }));
  }
}
