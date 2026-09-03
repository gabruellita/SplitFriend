// ─── Enums ─────────────────────────────────────────────
export const UserStatus = {
  ACTIVE:   'ACTIVE',
  INACTIVE: 'INACTIVE',
  PENDING:  'PENDING',
} as const;

export type UserStatus = typeof UserStatus[keyof typeof UserStatus];

// ─── Request DTOs ──────────────────────────────────────
export interface LoginRequest {
  email:    string;
  password: string;
}

export interface RegisterRequest {
  email:               string;
  username:            string;
  password:            string;
  firstName?:          string;
  lastName?:           string;
  preferredCurrencyId: number;
}

export interface ConfirmEmailRequest {
  token: string;
}

// ─── Response DTOs ─────────────────────────────────────
export interface AuthUser {
  id:                  number;
  email:               string;
  username:            string;
  firstName?:          string;
  lastName?:           string;
  status:              UserStatus;
  preferredCurrencyId: number;
}

export interface LoginResponse {
  accessToken:  string;
  refreshToken: string;
  expiresIn:    number;
  tokenType:    'Bearer';
  user:         AuthUser;
}

export interface RegisterResponse {
  userId:   number;
  email:    string;
  username: string;
  status:   UserStatus;
  message:  string;
}

// ─── JWT Payload ────────────────────────────────────────
export interface JwtPayload {
  sub:      string;
  email:    string;
  username: string;
  currency: string;
  status:   UserStatus;
  jti:      string;
  iat:      number;
  exp:      number;
  iss:      string;
  aud:      string;
}

// ─── Me / Profile ────────────────────────────────────────
export interface MeResponse {
  id:                    number;
  email:                 string;
  username:              string;
  firstName:             string | null;
  lastName:              string | null;
  status:                string;
  preferredCurrencyId:   number;
  preferredCurrencyCode: string | null;
  createdAt:             string;
}

export interface UpdateProfileRequest {
  firstName?:          string | null;
  lastName?:           string | null;
  preferredCurrencyId?: number | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword:     string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token:       string;
  newPassword: string;
}
