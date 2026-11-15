import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoaderService } from '../services/loader.service';
import { shouldSkipLoading } from '../utils/loading.utils';

export const loaderInterceptor: HttpInterceptorFn = (request, next) => {
  if (shouldSkipLoading(request)) {
    return next(request);
  }
  const loader = inject(LoaderService);
  loader.show();

  return next(request).pipe(finalize(() => loader.hide()));
};
