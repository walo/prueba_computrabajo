export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface AuthTokensResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
}

export interface CurrentUserResponse {
  userId: string;
  email: string;
  fullName: string;
  permissions: string[];
}
