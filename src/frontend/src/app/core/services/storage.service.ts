import { Injectable } from '@angular/core';

const ACCESS_TOKEN_KEY = 'redarbor_access_token';
const REFRESH_TOKEN_KEY = 'redarbor_refresh_token';

@Injectable({ providedIn: 'root' })
export class StorageService {
  public set(key: string, value: string): void {
    localStorage.setItem(key, value);
  }

  public get(key: string): string | null {
    return localStorage.getItem(key);
  }

  public remove(key: string): void {
    localStorage.removeItem(key);
  }

  public clear(): void {
    localStorage.clear();
  }

  public getAccessToken(): string | null {
    return this.get(ACCESS_TOKEN_KEY);
  }

  public getRefreshToken(): string | null {
    return this.get(REFRESH_TOKEN_KEY);
  }

  public setTokens(accessToken: string, refreshToken: string): void {
    this.set(ACCESS_TOKEN_KEY, accessToken);
    this.set(REFRESH_TOKEN_KEY, refreshToken);
  }

  public clearTokens(): void {
    this.remove(ACCESS_TOKEN_KEY);
    this.remove(REFRESH_TOKEN_KEY);
  }
}
