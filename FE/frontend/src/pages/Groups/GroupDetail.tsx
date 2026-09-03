// src/pages/Groups/GroupDetail.tsx
import { useParams, useSearchParams, Link } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { useGroup } from '@/hooks/useGroup';
import { useUnreadCounts } from '@/hooks/useUnreadCounts';
import { GroupProvider } from '@/context/GroupContext';
import { GroupTabs, type GroupTab } from '@/components/groups/GroupTabs';
import { ExpensesTab } from '@/components/groups/tabs/ExpensesTab';
import { BalancesTab } from '@/components/groups/tabs/BalancesTab';
import { MembersTab }  from '@/components/groups/tabs/MembersTab';
import { ChatTab }     from '@/components/groups/tabs/ChatTab';
import { Spinner } from '@/components/common/Spinner';
import { Alert } from '@/components/common/Alert';

const VALID_TABS: GroupTab[] = ['expenses', 'balances', 'members', 'chat'];

export const GroupDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const groupId = Number(id);

  const [searchParams, setSearchParams] = useSearchParams();
  const acceptInvite = searchParams.get('invite') === '1';
  const { group, members, isLoading, error, refetch, justJoined } = useGroup(groupId, acceptInvite);

  const tabParam = searchParams.get('tab') as GroupTab | null;
  const activeTab: GroupTab = tabParam && VALID_TABS.includes(tabParam) ? tabParam : 'expenses';

  const setTab = (tab: GroupTab) =>
    setSearchParams(prev => { prev.set('tab', tab); return prev; }, { replace: true });

  // Unread badge for Chat tab. Re-fetched whenever groupId changes.
  const unread = useUnreadCounts([groupId]);

  if (!id || Number.isNaN(groupId)) return <Alert type="error" message="ID grup invalid." />;
  if (isLoading) return <div className="flex justify-center py-20"><Spinner size="lg" /></div>;
  if (error || !group) return <Alert type="error" message={error ?? 'Grup negăsit.'} />;

  return (
    <GroupProvider group={group} members={members} refetch={refetch}>
      <div className="space-y-5">
        {justJoined && (
          <Alert type="success" message={`Ai intrat în grupul „${group.name}”.`} />
        )}
        <div>
          <Link to="/app/groups" className="mb-2 inline-flex items-center gap-1 text-sm text-slate-500 hover:text-slate-900">
            <ArrowLeft className="h-4 w-4" /> Toate grupurile
          </Link>
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div>
              <h1 className="text-2xl font-bold text-slate-900">{group.name}</h1>
              <p className="text-sm text-slate-500">
                {group.memberCount} membri · {group.currencyCode} · rolul tău: {group.myRole}
              </p>
            </div>
          </div>
        </div>

        <GroupTabs active={activeTab} onChange={setTab}
          unread={activeTab === 'chat' ? 0 : (unread[String(groupId)] ?? 0)} />

        <div>
          {activeTab === 'expenses' && <ExpensesTab />}
          {activeTab === 'balances' && <BalancesTab />}
          {activeTab === 'members'  && <MembersTab />}
          {activeTab === 'chat'     && <ChatTab />}
        </div>
      </div>
    </GroupProvider>
  );
};
