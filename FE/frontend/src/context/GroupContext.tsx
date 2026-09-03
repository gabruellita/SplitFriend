// src/context/GroupContext.tsx
import { createContext, useContext, useMemo } from 'react';
import type { Group, GroupMember } from '@/types/group.types';
import { buildNameMap, resolveName } from '@/utils/groupMembers';
import { tokenStorage } from '@/utils/tokenStorage';
import { jwtUtils } from '@/utils/jwtUtils';

interface GroupContextValue {
  group:         Group;
  members:       GroupMember[];
  currentUserId: number;
  nameOf:        (userId: number) => string;
  refetch:       () => Promise<void>;
}

const GroupContext = createContext<GroupContextValue | null>(null);

const readCurrentUserId = (): number => {
  const token = tokenStorage.getAccessToken();
  const payload = token ? jwtUtils.decode(token) : null;
  return payload?.sub ? Number(payload.sub) : 0;
};

interface GroupProviderProps {
  group:    Group;
  members:  GroupMember[];
  refetch:  () => Promise<void>;
  children: React.ReactNode;
}

export const GroupProvider: React.FC<GroupProviderProps> = ({ group, members, refetch, children }) => {
  const value = useMemo<GroupContextValue>(() => {
    const nameMap = buildNameMap(members);
    return {
      group, members, refetch,
      currentUserId: readCurrentUserId(),
      nameOf: (userId: number) => resolveName(nameMap, userId),
    };
  }, [group, members, refetch]);

  return <GroupContext.Provider value={value}>{children}</GroupContext.Provider>;
};

export const useGroupContext = (): GroupContextValue => {
  const ctx = useContext(GroupContext);
  if (!ctx) throw new Error('useGroupContext must be used inside <GroupProvider>');
  return ctx;
};
