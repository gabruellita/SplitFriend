const ACCESS_TOKEN_KEY  = 'finance_app_access_token';
const REFRESH_TOKEN_KEY = 'finance_app_refresh_token';
const USER_KEY          = 'finance_app_user';

export const tokenStorage = {
  getAccessToken:  (): string | null => localStorage.getItem(ACCESS_TOKEN_KEY),
  setAccessToken:  (token: string): void => localStorage.setItem(ACCESS_TOKEN_KEY, token),

  getRefreshToken: (): string | null => localStorage.getItem(REFRESH_TOKEN_KEY),
  setRefreshToken: (token: string): void => localStorage.setItem(REFRESH_TOKEN_KEY, token),

  getUser: <T>(): T | null => {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as T) : null;
  },
  setUser: <T>(user: T): void => localStorage.setItem(USER_KEY, JSON.stringify(user)),

  clear: (): void => {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },
};
