import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { StorageService } from '../services/storage.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const storage = inject(StorageService);
  const router = inject(Router);
  if (authService.isAuthenticated() || storage.getAccessToken()) {
    return true;
  }

  return router.createUrlTree(['/auth/login']);
};
