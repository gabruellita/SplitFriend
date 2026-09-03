// src/utils/groupMembers.ts
import type { GroupMember } from '@/types/group.types';

/** Numele afișabil al unui membru: "Prenume Nume" → username → email → "Utilizator #id". */
export const memberDisplayName = (m: Pick<GroupMember,
  'userId' | 'firstName' | 'lastName' | 'username' | 'email'>): string => {
  const full = [m.firstName, m.lastName].filter(Boolean).join(' ').trim();
  if (full) return full;
  if (m.username) return m.username;
  if (m.email) return m.email;
  return `Utilizator #${m.userId}`;
};

/** Hartă userId → nume afișabil, cu fallback pentru id-uri necunoscute. */
export const buildNameMap = (members: GroupMember[]): Map<number, string> => {
  const map = new Map<number, string>();
  for (const m of members) map.set(m.userId, memberDisplayName(m));
  return map;
};

/** Rezolvă un userId la nume; fallback dacă lipsește din hartă (ex. membru care a plecat). */
export const resolveName = (map: Map<number, string>, userId: number): string =>
  map.get(userId) ?? `Utilizator #${userId}`;
