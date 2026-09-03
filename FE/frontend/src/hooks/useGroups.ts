// src/hooks/useGroups.ts
import { useState, useEffect, useCallback } from 'react';
import { groupsApi } from '@/api/groupsApi';
import type { Group, CreateGroupRequest } from '@/types/group.types';

interface UseGroupsResult {
  groups:    Group[];
  isLoading: boolean;
  error:     string | null;
  refetch:   () => Promise<void>;
  create:    (body: CreateGroupRequest) => Promise<number>;
}

export const useGroups = (): UseGroupsResult => {
  const [groups, setGroups]     = useState<Group[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError]       = useState<string | null>(null);

  const fetchGroups = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setGroups(await groupsApi.list());
    } catch (err) {
      setError('Nu s-au putut încărca grupurile. Reîncearcă.');
      console.error('useGroups error:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { void fetchGroups(); }, [fetchGroups]);

  const create = useCallback(async (body: CreateGroupRequest): Promise<number> => {
    const { id } = await groupsApi.create(body);
    void fetchGroups();   // refresh în fundal; o eroare aici NU înseamnă că crearea a eșuat
    return id;
  }, [fetchGroups]);

  return { groups, isLoading, error, refetch: fetchGroups, create };
};
