import { HttpContext, HttpContextToken, HttpRequest } from '@angular/common/http';

export const SKIP_LOADING_TOKEN = new HttpContextToken<boolean>(() => false);

export function createSilentContext(): HttpContext {
  return new HttpContext().set(SKIP_LOADING_TOKEN, true);
}

export function shouldSkipLoading(request: HttpRequest<unknown>): boolean {
  return request.context.get(SKIP_LOADING_TOKEN);
}
