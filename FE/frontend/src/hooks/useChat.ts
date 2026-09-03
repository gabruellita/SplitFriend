// src/hooks/useChat.ts
import { useState, useEffect, useRef, useCallback } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { chatApi } from '@/api/chatApi';
import {
  buildChatConnection, hubSend, hubEdit, hubDelete, hubMarkRead,
} from '@/api/chatHub';
import type { ChatMessage } from '@/types/chat.types';

export type ChatStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

interface UseChatResult {
  messages:  ChatMessage[];
  online:    Set<number>;
  status:    ChatStatus;
  send:      (content: string) => Promise<void>;
  edit:      (messageId: number, content: string) => Promise<void>;
  remove:    (messageId: number) => Promise<void>;
  loadOlder: () => Promise<void>;
  hasMore:   boolean;
}

const PAGE = 50;

export const useChat = (groupId: number): UseChatResult => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [online, setOnline]     = useState<Set<number>>(new Set());
  const [status, setStatus]     = useState<ChatStatus>('connecting');
  const [hasMore, setHasMore]   = useState(true);
  const connRef = useRef<HubConnection | null>(null);
  const loadingOlderRef = useRef(false);

  // Upsert un mesaj în listă (păstrând ordinea crescătoare după id).
  const upsert = useCallback((m: ChatMessage) => {
    setMessages(prev => {
      const idx = prev.findIndex(x => x.id === m.id);
      if (idx >= 0) { const copy = [...prev]; copy[idx] = m; return copy; }
      return [...prev, m].sort((a, b) => a.id - b.id);
    });
  }, []);

  useEffect(() => {
    let disposed = false;

    // Istoric inițial (descrescător din API → îl întoarcem crescător pentru afișare).
    void chatApi.getHistory(groupId, undefined, PAGE).then(hist => {
      if (disposed) return;
      const asc = [...hist].sort((a, b) => a.id - b.id);
      setMessages(asc);
      setHasMore(hist.length === PAGE);
    });
    void chatApi.getPresence(groupId).then(ids => { if (!disposed) setOnline(new Set(ids)); });

    const conn = buildChatConnection(groupId, {
      onMessageReceived: upsert,
      onMessageEdited:   upsert,
      onMessageDeleted:  upsert,
      onPresenceChanged: ({ userId, online: isOnline }) =>
        setOnline(prev => {
          const next = new Set(prev);
          if (isOnline) next.add(userId); else next.delete(userId);
          return next;
        }),
    });
    connRef.current = conn;

    conn.onreconnecting(() => { if (!disposed) setStatus('reconnecting'); });
    conn.onreconnected(() => { if (!disposed) { setStatus('connected'); void hubMarkRead(conn); } });
    conn.onclose(() => { if (!disposed) setStatus('disconnected'); });

    void conn.start()
      .then(() => { if (!disposed) { setStatus('connected'); return hubMarkRead(conn); } })
      .catch(err => { if (!disposed) { setStatus('disconnected'); console.error('chat connect error:', err); } });

    return () => {
      disposed = true;
      connRef.current = null;
      void conn.stop();
    };
  }, [groupId, upsert]);

  const send = useCallback(async (content: string) => {
    const c = connRef.current;
    if (c && content.trim()) await hubSend(c, content.trim(), null);
  }, []);

  const edit = useCallback(async (messageId: number, content: string) => {
    const c = connRef.current;
    if (c && content.trim()) await hubEdit(c, messageId, content.trim());
  }, []);

  const remove = useCallback(async (messageId: number) => {
    const c = connRef.current;
    if (c) await hubDelete(c, messageId);
  }, []);

  const loadOlder = useCallback(async () => {
    if (loadingOlderRef.current || messages.length === 0) return;
    loadingOlderRef.current = true;
    try {
      const oldest = messages[0].id;
      const older = await chatApi.getHistory(groupId, oldest, PAGE);
      if (older.length < PAGE) setHasMore(false);
      setMessages(prev => {
        const ids = new Set(prev.map(m => m.id));
        const merged = [...older.filter(m => !ids.has(m.id)), ...prev];
        return merged.sort((a, b) => a.id - b.id);
      });
    } finally {
      loadingOlderRef.current = false;
    }
  }, [groupId, messages]);

  return { messages, online, status, send, edit, remove, loadOlder, hasMore };
};
