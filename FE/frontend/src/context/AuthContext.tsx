import { createContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import { authApi } from '@/api/authApi';
import { tokenStorage } from '@/utils/tokenStorage';
import { jwtUtils } from '@/utils/jwtUtils';
import type { AuthUser, LoginRequest, LoginResponse } from '@/types/auth.types';

interface AuthContextValue {
  user:            AuthUser | null;
  isAuthenticated: boolean;
  isLoading:       boolean;
  login:           (creds: LoginRequest) => Promise<LoginResponse>;
  logout:          () => Promise<void>;
  updateUser:      (patch: Partial<AuthUser>) => void;
  refreshSession:  () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser]           = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Rehydrate from localStorage on mount
  useEffect(() => {
    const token = tokenStorage.getAccessToken();
    if (token && !jwtUtils.isExpired(token)) {
      const stored = tokenStorage.getUser<AuthUser>();
      if (stored) setUser(stored);
    } else {
      tokenStorage.clear();
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (creds: LoginRequest): Promise<LoginResponse> => {
    const response = await authApi.login(creds);
    tokenStorage.setAccessToken(response.accessToken);
    tokenStorage.setRefreshToken(response.refreshToken);
    tokenStorage.setUser(response.user);
    setUser(response.user);
    return response;
  }, []);

  const logout = useCallback(async (): Promise<void> => {
    const refreshToken = tokenStorage.getRefreshToken();
    if (refreshToken) {
      await authApi.logout(refreshToken);
    }
    tokenStorage.clear();
    setUser(null);
  }, []);

  const updateUser = useCallback((patch: Partial<AuthUser>) => {
    setUser(prev => {
      if (!prev) return prev;
      const next = { ...prev, ...patch };
      tokenStorage.setUser(next);
      return next;
    });
  }, []);

  const refreshSession = useCallback(async (): Promise<void> => {
    const rt = tokenStorage.getRefreshToken();
    if (!rt) return;
    // Best-effort: reîmprospătează JWT-ul (ex. claim-ul `currency` după schimbarea profilului).
    // Un refresh eșuat nu trebuie să transforme o salvare reușită într-o eroare.
    try {
      const response = await authApi.refresh(rt);
      tokenStorage.setAccessToken(response.accessToken);
      tokenStorage.setRefreshToken(response.refreshToken);
      tokenStorage.setUser(response.user);
      setUser(response.user);
    } catch (err) {
      console.warn('refreshSession a eșuat (token expirat/revocat?):', err);
    }
  }, []);

  return (
    <AuthContext.Provider value={{
      user,
      isAuthenticated: user !== null,
      isLoading,
      login,
      logout,
      updateUser,
      refreshSession,
    }}>
      {children}
    </AuthContext.Provider>
  );
};
