// src/components/groups/GroupTabs.tsx
import clsx from 'clsx';
import { Receipt, Scale, Users, MessageSquare } from 'lucide-react';

export type GroupTab = 'expenses' | 'balances' | 'members' | 'chat';

const TABS: { key: GroupTab; label: string; icon: React.ComponentType<{ className?: string }> }[] = [
  { key: 'expenses', label: 'Cheltuieli', icon: Receipt },
  { key: 'balances', label: 'Balanțe',    icon: Scale },
  { key: 'members',  label: 'Membri',     icon: Users },
  { key: 'chat',     label: 'Chat',       icon: MessageSquare },
];

interface GroupTabsProps {
  active:   GroupTab;
  onChange: (tab: GroupTab) => void;
  unread?:  number;
}

export const GroupTabs: React.FC<GroupTabsProps> = ({ active, onChange, unread }) => (
  <div role="tablist" aria-label="Secțiuni grup"
    className="flex gap-1 overflow-x-auto border-b border-slate-900/10">
    {TABS.map(({ key, label, icon: Icon }) => (
      <button key={key} role="tab" aria-selected={active === key} type="button"
        onClick={() => onChange(key)}
        className={clsx(
          'flex items-center gap-2 whitespace-nowrap px-4 py-2.5 text-sm font-medium transition cursor-pointer',
          'focus:outline-none focus:ring-2 focus:ring-brand-500 rounded-t-lg',
          active === key
            ? 'border-b-2 border-brand-600 text-brand-700'
            : 'text-slate-500 hover:text-slate-900',
        )}>
        <Icon className="h-4 w-4" />
        {label}
        {key === 'chat' && !!unread && (
          <span className="ml-1 rounded-full bg-brand-600 px-1.5 text-xs font-semibold text-white">{unread}</span>
        )}
      </button>
    ))}
  </div>
);
