// src/components/groups/tabs/ChatTab.tsx
import clsx from 'clsx';
import { useGroupContext } from '@/context/GroupContext';
import { useChat } from '@/hooks/useChat';
import { MessageList } from '@/components/groups/chat/MessageList';
import { ChatComposer } from '@/components/groups/chat/ChatComposer';

const STATUS_LABEL = {
  connecting:   { text: 'Se conectează…',  tone: 'text-amber-600' },
  connected:    { text: 'Conectat',         tone: 'text-emerald-600' },
  reconnecting: { text: 'Se reconectează…', tone: 'text-amber-600' },
  disconnected: { text: 'Deconectat',       tone: 'text-rose-600' },
} as const;

export const ChatTab: React.FC = () => {
  const { group, members, currentUserId } = useGroupContext();
  const { messages, online, status, send, edit, remove, loadOlder, hasMore } = useChat(group.id);

  const onlineCount = members.filter(m => m.userId !== currentUserId && online.has(m.userId)).length;
  const s = STATUS_LABEL[status];

  return (
    <div>
      <div className="mb-2 flex items-center justify-between text-sm">
        <span className="text-slate-500">{onlineCount} online</span>
        <span className={clsx('font-medium', s.tone)}>{s.text}</span>
      </div>
      <MessageList messages={messages} hasMore={hasMore} onLoadOlder={loadOlder} onEdit={edit} onDelete={remove} />
      <ChatComposer onSend={send} disabled={status === 'disconnected'} />
    </div>
  );
};
