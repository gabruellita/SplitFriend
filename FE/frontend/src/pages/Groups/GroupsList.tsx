// src/pages/Groups/GroupsList.tsx
import { useState } from 'react';
import { Plus, Users } from 'lucide-react';
import { useGroups } from '@/hooks/useGroups';
import { useUnreadCounts } from '@/hooks/useUnreadCounts';
import { GroupCard } from '@/components/groups/GroupCard';
import { CreateGroupModal } from '@/components/groups/CreateGroupModal';
import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { Spinner } from '@/components/common/Spinner';

export const GroupsList: React.FC = () => {
  const { groups, isLoading, error, create } = useGroups();
  const unread = useUnreadCounts(groups.map(g => g.id));
  const [modalOpen, setModalOpen] = useState(false);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Grupuri</h1>
        <Button onClick={() => setModalOpen(true)}>
          <Plus className="h-4 w-4" /> Grup nou
        </Button>
      </div>

      {error && <Alert type="error" message={error} />}

      {isLoading ? (
        <div className="flex justify-center py-20"><Spinner size="lg" /></div>
      ) : groups.length === 0 ? (
        <div className="glass-card flex flex-col items-center rounded-2xl px-6 py-16 text-center">
          <span className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-slate-900 text-white">
            <Users className="h-7 w-7" />
          </span>
          <h2 className="text-lg font-semibold text-slate-900">Niciun grup încă</h2>
          <p className="mt-1 max-w-sm text-sm text-slate-500">
            Creează un grup ca să împarți cheltuieli cu prietenii și să ții socoteala automat.
          </p>
          <Button className="mt-4" onClick={() => setModalOpen(true)}>
            <Plus className="h-4 w-4" /> Creează primul grup
          </Button>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {groups.map(g => <GroupCard key={g.id} group={g} unreadCount={unread[String(g.id)] ?? 0} />)}
        </div>
      )}

      <CreateGroupModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmit={async (body) => { await create(body); }}
      />
    </div>
  );
};
