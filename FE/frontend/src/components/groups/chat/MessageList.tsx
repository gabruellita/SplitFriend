// src/components/groups/chat/MessageList.tsx
import { useEffect, useRef } from 'react';
import type { ChatMessage } from '@/types/chat.types';
import { useGroupContext } from '@/context/GroupContext';
import { MessageBubble } from './MessageBubble';

interface MessageListProps {
  messages:    ChatMessage[];
  hasMore:     boolean;
  onLoadOlder: () => Promise<void>;
  onEdit:      (messageId: number, content: string) => Promise<void>;
  onDelete:    (messageId: number) => Promise<void>;
}

export const MessageList: React.FC<MessageListProps> = ({ messages, hasMore, onLoadOlder, onEdit, onDelete }) => {
  const { currentUserId, nameOf } = useGroupContext();
  const bottomRef = useRef<HTMLDivElement>(null);

  // scroll la jos doar când apare un mesaj NOU la coadă (nu la prepend de istoric)
  const lastId = messages.length > 0 ? messages[messages.length - 1].id : 0;
  useEffect(() => { bottomRef.current?.scrollIntoView({ behavior: 'smooth' }); }, [lastId]);

  return (
    <div role="log" aria-live="polite" className="flex max-h-[55vh] min-h-[300px] flex-col gap-2 overflow-y-auto rounded-xl border border-slate-200 p-3">
      {hasMore && (
        <button type="button" onClick={onLoadOlder}
          className="mx-auto mb-1 text-xs text-brand-600 underline hover:text-brand-700 cursor-pointer">
          Încarcă mesaje mai vechi
        </button>
      )}
      {messages.map(m => (
        <MessageBubble key={m.id} message={m} mine={m.senderUserId === currentUserId}
          authorName={nameOf(m.senderUserId)} onEdit={onEdit} onDelete={onDelete} />
      ))}
      <div ref={bottomRef} />
    </div>
  );
};
