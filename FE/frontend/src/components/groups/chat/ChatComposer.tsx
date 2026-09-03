// src/components/groups/chat/ChatComposer.tsx
import { useState } from 'react';
import { Send } from 'lucide-react';
import { Button } from '@/components/common/Button';

interface ChatComposerProps {
  onSend:    (content: string) => Promise<void>;
  disabled?: boolean;
}

export const ChatComposer: React.FC<ChatComposerProps> = ({ onSend, disabled }) => {
  const [text, setText] = useState('');

  const submit = async () => {
    const trimmed = text.trim();
    if (!trimmed) return;
    try {
      await onSend(trimmed);
      setText('');                 // golește doar dacă trimiterea a reușit
    } catch {
      // păstrează textul ca userul să poată reîncerca
    }
  };

  return (
    <form className="mt-3 flex gap-2"
      onSubmit={e => { e.preventDefault(); void submit(); }}>
      <input value={text} onChange={e => setText(e.target.value)} disabled={disabled}
        placeholder="Scrie un mesaj…" aria-label="Mesaj"
        className="flex-1 rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-brand-500 disabled:opacity-50" />
      <Button type="submit" disabled={disabled || !text.trim()}>
        <Send aria-hidden="true" className="h-4 w-4" />
      </Button>
    </form>
  );
};
