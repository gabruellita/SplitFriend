// src/components/groups/chat/MessageBubble.tsx
import { useState, useRef } from 'react';
import clsx from 'clsx';
import { Pencil, Trash2 } from 'lucide-react';
import type { ChatMessage } from '@/types/chat.types';
import { formatDate } from '@/utils/format';

interface MessageBubbleProps {
  message:    ChatMessage;
  mine:       boolean;
  authorName: string;
  onEdit:     (messageId: number, content: string) => Promise<void>;
  onDelete:   (messageId: number) => Promise<void>;
}

export const MessageBubble: React.FC<MessageBubbleProps> = ({ message, mine, authorName, onEdit, onDelete }) => {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft]     = useState(message.content);
  const originalRef = useRef(message.content);
  const startEdit = () => { originalRef.current = message.content; setDraft(message.content); setEditing(true); };

  return (
    <div className={clsx('flex flex-col gap-0.5', mine ? 'items-end' : 'items-start')}>
      {!mine && <span className="px-1 text-xs text-slate-400">{authorName}</span>}
      <div className={clsx('max-w-[75%] rounded-2xl px-3 py-2 text-sm',
        message.isDeleted ? 'bg-slate-100 italic text-slate-400'
          : mine ? 'bg-brand-600 text-white' : 'bg-slate-100 text-slate-900')}>
        {message.isDeleted ? (
          'mesaj șters'
        ) : editing ? (
          <div className="flex flex-col gap-1">
            <input value={draft} onChange={e => setDraft(e.target.value)}
              className="rounded-md px-2 py-1 text-sm text-slate-900" />
            <div className="flex gap-2 text-xs">
              <button type="button" className="underline" onClick={async () => { await onEdit(message.id, draft); setEditing(false); }}>Salvează</button>
              <button type="button" className="underline" onClick={() => { setEditing(false); setDraft(originalRef.current); }}>Anulează</button>
            </div>
          </div>
        ) : (
          <span className="whitespace-pre-wrap break-words">{message.content}</span>
        )}
      </div>
      <div className="flex items-center gap-2 px-1 text-[10px] text-slate-400">
        <span>{formatDate(message.createdAt)}{message.editedAt && ' · editat'}</span>
        {mine && !message.isDeleted && !editing && (
          <>
            <button type="button" aria-label="Editează" onClick={startEdit} className="hover:text-slate-600">
              <Pencil aria-hidden="true" className="h-3 w-3" />
            </button>
            <button type="button" aria-label="Șterge" onClick={() => onDelete(message.id)} className="hover:text-rose-500">
              <Trash2 aria-hidden="true" className="h-3 w-3" />
            </button>
          </>
        )}
      </div>
    </div>
  );
};
