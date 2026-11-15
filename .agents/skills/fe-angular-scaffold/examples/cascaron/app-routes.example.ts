import { Routes } from "@angular/router";
import { authGuard } from "./core/guards/auth.guard";
import { publicGuard } from "./core/guards/public.guard";
import { ShellComponent } from "./layout/shell/shell.component";

export const routes: Routes = [
  {
    path: "auth",
    canActivate: [publicGuard],
    loadChildren: () => import("./auth/auth.routes").then((m) => m.AUTH_ROUTES),
  },
  {
    path: "login",
    redirectTo: "auth/login",
    pathMatch: "full",
  },
  {
    path: "forbidden",
    loadComponent: () =>
      import("./shared/pages/status-page/status-page.component").then(
        (m) => m.StatusPageComponent,
      ),
    data: {
      code: "403",
      title: "Acceso restringido",
      message:
        "No cuentas con los permisos necesarios para acceder a esta página.",
    },
  },
  {
    path: "not-found",
    loadComponent: () =>
      import("./shared/pages/status-page/status-page.component").then(
        (m) => m.StatusPageComponent,
      ),
    data: {
      code: "404",
      title: "Página no encontrada",
      message: "La dirección URL solicitada no existe o fue movida.",
    },
  },
  {
    path: "",
    canActivate: [authGuard],
    component: ShellComponent,
    loadChildren: () =>
      import("./features/features.routes").then((m) => m.FEATURES_ROUTES),
  },
  {
    path: "**",
    redirectTo: "not-found",
  },
];
