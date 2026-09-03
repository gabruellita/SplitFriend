// src/hooks/useGroup.ts
import { useState, useEffect, useCallback, useRef } from 'react';
import { groupsApi } from '@/api/groupsApi';
import type { Group, GroupMember } from '@/types/group.types';

interface UseGroupResult {
  group:      Group | null;
  members:    GroupMember[];
  isLoading:  boolean;
  error:      string | null;
  refetch:    () => Promise<void>;
  justJoined: boolean;
}

/**
 * @param acceptInvite cand vine din deep-link-ul de invitatie (?invite=1), acceptam
 *   invitatia (INVITED → ACTIVE) o singura data inainte de a incarca grupul —
 *   altfel sp_get_group_by_id intoarce 0 randuri (cere membru ACTIVE) si pagina ar da 404.
 */
export const useGroup = (id: number, acceptInvite = false): UseGroupResult => {
  const [group, setGroup]         = useState<Group | null>(null);
  const [members, setMembers]     = useState<GroupMember[]>([]);
  const [isLoading, setLoading]   = useState(true);
  const [error, setError]         = useState<string | null>(null);
  const [justJoined, setJustJoined] = useState(false);
  const acceptPromise = useRef<Promise<void> | null>(null);

  const fetchAll = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      if (acceptInvite) {
        // Memoizam promisiunea de accept: la dublu-mount din StrictMode se face UN singur
        // /accept, iar AMBELE invocari asteapta acelasi accept inainte de getById/getMembers.
        // Altfel a doua invocare ar citi grupul inainte ca membership-ul sa fie ACTIVE
        // → EnsureMemberAsync 403 / GetByIdAsync 404. (Backend-ul e si idempotent ca plasa.)
        acceptPromise.current ??= groupsApi.accept(id)
          .then(() => { setJustJoined(true); })
          .catch(() => { /* deja membru / fara invitatie — ignoram, backend idempotent */ });
        await acceptPromise.current;
      }
      const [g, m] = await Promise.all([groupsApi.getById(id), groupsApi.getMembers(id)]);
      setGroup(g);
      setMembers(m);
    } catch (err) {
      setError('Nu s-a putut încărca grupul. Reîncearcă.');
      console.error('useGroup error:', err);
    } finally {
      setLoading(false);
    }
  }, [id, acceptInvite]);

  useEffect(() => { void fetchAll(); }, [fetchAll]);

  return { group, members, isLoading, error, refetch: fetchAll, justJoined };
};
