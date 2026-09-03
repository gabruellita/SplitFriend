// src/hooks/useUnreadCounts.ts
import { useState, useEffect } from 'react';
import { chatApi } from '@/api/chatApi';
import type { UnreadCounts } from '@/types/chat.types';

/** Necitite per grup pentru lista dată. Reîncarcă la schimbarea setului de id-uri. */
export const useUnreadCounts = (groupIds: number[]): UnreadCounts => {
  const [counts, setCounts] = useState<UnreadCounts>({});
  const key = groupIds.join(',');

  useEffect(() => {
    if (groupIds.length === 0) { setCounts({}); return; }
    let disposed = false;
    void chatApi.getUnread(groupIds)
      .then(data => { if (!disposed) setCounts(data); })
      .catch(err => console.error('useUnreadCounts error:', err));
    return () => { disposed = true; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key]);

  return counts;
};
