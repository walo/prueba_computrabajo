import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  importProvidersFrom,
  inject,
  provideAppInitializer,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { NgxPermissionsModule } from 'ngx-permissions';
import { routes } from './app.routes';
import { loaderInterceptor } from './core/interceptors/loader.interceptor';
import { tokenInterceptor } from './core/interceptors/token.interceptor';
import { AuthService } from './core/services/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([loaderInterceptor, tokenInterceptor])),
    importProvidersFrom(NgxPermissionsModule.forRoot()),
    provideAppInitializer(() => inject(AuthService).initializeSession()),
  ],
};
