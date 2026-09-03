import { jwtDecode } from 'jwt-decode';
import type { JwtPayload } from '@/types/auth.types';

export const jwtUtils = {
  decode: (token: string): JwtPayload | null => {
    try {
      return jwtDecode<JwtPayload>(token);
    } catch {
      return null;
    }
  },

  isExpired: (token: string): boolean => {
    const payload = jwtUtils.decode(token);
    if (!payload?.exp) return true;
    return Date.now() >= payload.exp * 1000;
  },

  getRemainingSeconds: (token: string): number => {
    const payload = jwtUtils.decode(token);
    if (!payload?.exp) return 0;
    return Math.max(0, payload.exp - Math.floor(Date.now() / 1000));
  },
};
