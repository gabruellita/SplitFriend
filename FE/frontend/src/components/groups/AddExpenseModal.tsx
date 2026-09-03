// src/components/groups/AddExpenseModal.tsx
import { useEffect, useMemo, useState } from 'react';
import { X } from 'lucide-react';
import { useGroupContext } from '@/context/GroupContext';
import { SplitType, type CreateGroupExpenseRequest, type ExpenseParticipantInput } from '@/types/group.types';
import { computeOwed, type PreviewParticipant } from '@/utils/splitPreview';
import { memberDisplayName } from '@/utils/groupMembers';
import { formatMoney, toIsoDate } from '@/utils/format';
import { useUserCurrency } from '@/hooks/useUserCurrency';
import { Input }  from '@/components/common/Input';
import { Button } from '@/components/common/Button';
import { Alert }  from '@/components/common/Alert';

interface AddExpenseModalProps {
  open:     boolean;
  onSubmit: (body: CreateGroupExpenseRequest) => Promise<void>;
  onClose:  () => void;
}

const SPLIT_LABELS: Record<SplitType, string> = {
  EQUAL: 'Egal', EXACT: 'Sume exacte', PERCENT: 'Procente', SHARES: 'Părți',
};

export const AddExpenseModal: React.FC<AddExpenseModalProps> = ({ open, onSubmit, onClose }) => {
  const { members, currentUserId } = useGroupContext();
  const activeMembers = useMemo(() => members.filter(m => m.status === 'ACTIVE'), [members]);
  const userCurrency = useUserCurrency();

  const [title, setTitle]         = useState('');
  const [amount, setAmount]       = useState<number>(0);
  const [paidBy, setPaidBy]       = useState<number>(currentUserId);
  const [date, setDate]           = useState<string>(toIsoDate());
  const [splitType, setSplitType] = useState<SplitType>(SplitType.EQUAL);
  const [parts, setParts]         = useState<PreviewParticipant[]>([]);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitting, setSubmitting]   = useState(false);

  // (Re)inițializează la deschidere.
  useEffect(() => {
    if (!open) return;
    setTitle(''); setAmount(0); setPaidBy(currentUserId); setDate(toIsoDate());
    setSplitType(SplitType.EQUAL); setSubmitError(null);
    setParts(activeMembers.map(m => ({ userId: m.userId, selected: true, exactAmount: null, percent: null, shares: 1 })));
  }, [open, activeMembers, currentUserId]);

  const preview = useMemo(() => computeOwed(splitType, amount, parts), [splitType, amount, parts]);
  const canSubmit = title.trim().length > 0 && amount > 0 && preview.valid && !submitting;

  if (!open) return null;

  const patchPart = (userId: number, patch: Partial<PreviewParticipant>) =>
    setParts(prev => prev.map(p => p.userId === userId ? { ...p, ...patch } : p));

  const buildParticipants = (): ExpenseParticipantInput[] =>
    parts.filter(p => p.selected).map(p => ({
      userId:      p.userId,
      exactAmount: splitType === SplitType.EXACT   ? (p.exactAmount ?? 0) : null,
      percent:     splitType === SplitType.PERCENT ? (p.percent ?? 0)     : null,
      shares:      splitType === SplitType.SHARES  ? (p.shares ?? 0)      : null,
    }));

  const submit = async () => {
    setSubmitting(true);
    setSubmitError(null);
    try {
      await onSubmit({
        title: title.trim(), amount, paidByUserId: paidBy,
        splitType, expenseDate: date, participants: buildParticipants(),
      });
      onClose();
    } catch {
      setSubmitError('Nu s-a putut salva cheltuiala. Reîncearcă.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/50" onClick={onClose} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-label="Adaugă cheltuială"
        className="relative max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-2xl bg-white p-6 shadow-xl">
        <button type="button" onClick={onClose} aria-label="Închide"
          className="absolute right-4 top-4 rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700">
          <X className="h-5 w-5" />
        </button>
        <h2 className="mb-4 text-lg font-semibold text-slate-900">Adaugă cheltuială</h2>
        {submitError && <Alert type="error" message={submitError} />}

        <Input label="Titlu" value={title} onChange={e => setTitle(e.target.value)} maxLength={200} />
        <Input
          label={`Sumă${userCurrency ? ` (${userCurrency})` : ''}`}
          type="number" step="0.01" min="0" inputMode="decimal"
          value={amount || ''}
          onChange={e => setAmount(parseFloat(e.target.value) || 0)}
          helper={userCurrency ? `Cheltuiala va fi înregistrată în ${userCurrency} (moneda ta).` : undefined}
        />
        <Input label="Dată" type="date" value={date} onChange={e => setDate(e.target.value)} />

        <div className="mb-4 flex flex-col gap-1">
          <label htmlFor="exp-paidby" className="text-sm font-medium text-gray-700">Plătit de</label>
          <select id="exp-paidby" value={paidBy} onChange={e => setPaidBy(Number(e.target.value))}
            className="rounded-md border border-gray-300 px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500">
            {activeMembers.map(m => (
              <option key={m.userId} value={m.userId}>
                {memberDisplayName(m)}{m.userId === currentUserId ? ' (Tu)' : ''}
              </option>
            ))}
          </select>
        </div>

        <div className="mb-3 flex flex-col gap-1">
          <span className="text-sm font-medium text-gray-700">Mod de împărțire</span>
          <div className="flex flex-wrap gap-2">
            {(Object.keys(SPLIT_LABELS) as SplitType[]).map(t => (
              <button key={t} type="button" onClick={() => setSplitType(t)}
                className={`rounded-lg border px-3 py-1.5 text-sm transition cursor-pointer ${
                  splitType === t ? 'border-brand-600 bg-brand-50 text-brand-700' : 'border-slate-300 text-slate-600'
                }`}>
                {SPLIT_LABELS[t]}
              </button>
            ))}
          </div>
        </div>

        {/* Participanți — UI adaptat pe tipul de split */}
        <ul className="mb-3 divide-y divide-slate-100 rounded-xl border border-slate-200">
          {parts.map(p => {
            const m = activeMembers.find(am => am.userId === p.userId)!;
            const owed = preview.owed.get(p.userId);
            return (
              <li key={p.userId} className="flex items-center gap-3 px-3 py-2">
                <input type="checkbox" className="h-4 w-4 rounded border-gray-300"
                  aria-label={memberDisplayName(m)}
                  checked={p.selected} onChange={e => patchPart(p.userId, { selected: e.target.checked })} />
                <span className="min-w-0 flex-1 truncate text-sm text-slate-700">{memberDisplayName(m)}</span>

                {p.selected && splitType === SplitType.EXACT && (
                  <input type="number" step="0.01" min="0" placeholder="0.00"
                    className="w-24 rounded-md border border-gray-300 px-2 py-1 text-sm focus:ring-2 focus:ring-blue-500"
                    value={p.exactAmount ?? ''} onChange={e => patchPart(p.userId, { exactAmount: parseFloat(e.target.value) || 0 })} />
                )}
                {p.selected && splitType === SplitType.PERCENT && (
                  <input type="number" step="0.01" min="0" max="100" placeholder="%"
                    className="w-20 rounded-md border border-gray-300 px-2 py-1 text-sm focus:ring-2 focus:ring-blue-500"
                    value={p.percent ?? ''} onChange={e => patchPart(p.userId, { percent: parseFloat(e.target.value) || 0 })} />
                )}
                {p.selected && splitType === SplitType.SHARES && (
                  <input type="number" step="1" min="0" placeholder="părți"
                    className="w-20 rounded-md border border-gray-300 px-2 py-1 text-sm focus:ring-2 focus:ring-blue-500"
                    value={p.shares ?? ''} onChange={e => patchPart(p.userId, { shares: parseInt(e.target.value, 10) || 0 })} />
                )}

                <span className="w-24 shrink-0 text-right text-sm font-medium tabular-nums text-slate-900">
                  {p.selected && owed !== undefined ? formatMoney(owed, userCurrency) : '—'}
                </span>
              </li>
            );
          })}
        </ul>

        {preview.error && <p className="mb-3 text-sm text-rose-600">{preview.error}</p>}

        <div className="mt-2 flex justify-end gap-3">
          <Button type="button" variant="secondary" onClick={onClose} disabled={submitting}>Anulează</Button>
          <Button type="button" onClick={submit} loading={submitting} disabled={!canSubmit}>Adaugă</Button>
        </div>
      </div>
    </div>
  );
};
