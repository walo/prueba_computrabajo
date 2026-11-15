import { bootstrapApplication } from "@angular/platform-browser";
import { App } from "./app/app";
import { appConfig } from "./app/app.config";

// NOTA: La hidratación de permisos y sesión se realiza en appConfig mediante provideAppInitializer()
// para garantizar que NgxPermissionsService esté poblado ANTES de que el Router evalúe permissionGuard en F5.
bootstrapApplication(App, appConfig).catch((err) => console.error(err));
