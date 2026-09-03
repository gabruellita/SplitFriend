// src/components/groups/GroupCard.tsx
import { Link } from 'react-router-dom';
import { Users, MessageSquare } from 'lucide-react';
import type { Group } from '@/types/group.types';
import { BalancePill } from './BalancePill';

interface GroupCardProps {
  group:       Group;
  myBalance?:  number;          // soldul userului curent (opțional; vine din balances)
  unreadCount?: number;
}

export const GroupCard: React.FC<GroupCardProps> = ({ group, myBalance, unreadCount }) => (
  <Link to={`/app/groups/${group.id}`}
    className="glass-card flex flex-col gap-3 rounded-2xl p-5 transition hover:shadow-md focus:outline-none focus:ring-2 focus:ring-brand-500 cursor-pointer">
    <div className="flex items-start justify-between gap-2">
      <h3 className="font-semibold text-slate-900">{group.name}</h3>
      {!!unreadCount && (
        <span className="flex items-center gap-1 rounded-full bg-brand-600 px-2 py-0.5 text-xs font-medium text-white">
          <MessageSquare className="h-3 w-3" aria-hidden="true" /> {unreadCount}
        </span>
      )}
    </div>
    {group.description && <p className="line-clamp-2 text-sm text-slate-500">{group.description}</p>}
    <div className="mt-auto flex items-center justify-between text-sm">
      <span className="flex items-center gap-1 text-slate-500">
        <Users className="h-4 w-4" aria-hidden="true" /> {group.memberCount} · {group.currencyCode}
      </span>
      {myBalance !== undefined && <BalancePill amount={myBalance} currencyCode={group.currencyCode} />}
    </div>
  </Link>
);
