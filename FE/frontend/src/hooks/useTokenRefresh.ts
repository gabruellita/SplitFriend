import { useEffect, useRef } from 'react';
import { jwtUtils } from '@/utils/jwtUtils';
import { tokenStorage } from '@/utils/tokenStorage';
import { authApi } from '@/api/authApi';
import { useAuth } from './useAuth';

/**
 * Monitors the access token and proactively refreshes it 60 s before expiry.
 * Mount once inside AuthProvider.
 */
export const useTokenRefresh = (): void => {
  const { logout } = useAuth();
  const timerRef   = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    const scheduleRefresh = () => {
      const token = tokenStorage.getAccessToken();
      if (!token) return;

      const remainingSeconds = jwtUtils.getRemainingSeconds(token);
      const delayMs = Math.max(0, (remainingSeconds - 60) * 1000);

      timerRef.current = setTimeout(async () => {
        const refreshToken = tokenStorage.getRefreshToken();
        if (!refreshToken) { await logout(); return; }

        try {
          const response = await authApi.refresh(refreshToken);
          tokenStorage.setAccessToken(response.accessToken);
          tokenStorage.setRefreshToken(response.refreshToken);
          scheduleRefresh();
        } catch {
          await logout();
        }
      }, delayMs);
    };

    scheduleRefresh();

    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [logout]);
};
