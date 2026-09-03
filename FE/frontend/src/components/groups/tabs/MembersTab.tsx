// src/components/groups/tabs/MembersTab.tsx
import { useState } from 'react';
import { UserPlus, LogOut } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useGroupContext } from '@/context/GroupContext';
import { groupsApi } from '@/api/groupsApi';
import { memberDisplayName } from '@/utils/groupMembers';
import { parseApiError } from '@/utils/errorParser';
import { GroupRole } from '@/types/group.types';
import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { ConfirmDialog } from '@/components/dashboard/ConfirmDialog';
import { InviteMemberModal } from '@/components/groups/InviteMemberModal';

export const MembersTab: React.FC = () => {
  const { group, members, currentUserId, refetch } = useGroupContext();
  const navigate = useNavigate();
  const [inviteOpen, setInviteOpen] = useState(false);
  const [leaveOpen, setLeaveOpen]   = useState(false);
  const [notice, setNotice]         = useState<string | null>(null);
  const [error, setError]           = useState<string | null>(null);

  const canInvite = group.myRole === GroupRole.OWNER || group.myRole === GroupRole.ADMIN;

  const handleInvite = async (email: string): Promise<string> => {
    const { outcome } = await groupsApi.invite(group.id, { email });
    setNotice(`Invitație trimisă (${outcome}).`);
    refetch().catch(console.error); // refresh în fundal; o eroare aici nu înseamnă invitație eșuată
    return outcome;
  };

  const handleLeave = async () => {
    try {
      await groupsApi.leave(group.id);
      navigate('/app/groups');
    } catch (err) {
      const parsed = parseApiError(err);
      setError(parsed.statusCode === 409
        ? 'Nu poți părăsi grupul cât timp ai solduri neachitate. Achită întâi datoriile.'
        : 'Nu s-a putut părăsi grupul. Reîncearcă.');
      setLeaveOpen(false);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="font-semibold text-slate-900">Membri ({members.length})</h2>
        {canInvite && (
          <Button onClick={() => setInviteOpen(true)}>
            <UserPlus className="h-4 w-4" /> Invită
          </Button>
        )}
      </div>

      {notice && <Alert type="success" message={notice} />}
      {error && <Alert type="error" message={error} />}

      <ul className="divide-y divide-slate-900/10 rounded-2xl glass-card">
        {members.map(m => (
          <li key={m.userId} className="flex items-center justify-between gap-3 px-4 py-3">
            <div className="min-w-0">
              <p className="truncate font-medium text-slate-900">
                {memberDisplayName(m)}
                {m.userId === currentUserId && <span className="ml-2 text-xs text-brand-600">(Tu)</span>}
              </p>
              {m.email && <p className="truncate text-sm text-slate-500">{m.email}</p>}
            </div>
            <div className="flex shrink-0 items-center gap-2 text-xs">
              <span className="rounded-full bg-slate-100 px-2 py-0.5 font-medium text-slate-600">{m.role}</span>
              {m.status !== 'ACTIVE' && (
                <span className="rounded-full bg-amber-100 px-2 py-0.5 font-medium text-amber-700">{m.status}</span>
              )}
            </div>
          </li>
        ))}
      </ul>

      <div className="pt-2">
        <Button variant="danger" onClick={() => setLeaveOpen(true)}>
          <LogOut className="h-4 w-4" /> Părăsește grupul
        </Button>
      </div>

      <InviteMemberModal open={inviteOpen} onClose={() => setInviteOpen(false)} onSubmit={handleInvite} />
      <ConfirmDialog
        open={leaveOpen}
        title="Părăsești grupul?"
        message="Vei pierde accesul la cheltuielile și chat-ul acestui grup."
        confirmLabel="Părăsește"
        onConfirm={handleLeave}
        onCancel={() => setLeaveOpen(false)}
      />
    </div>
  );
};
