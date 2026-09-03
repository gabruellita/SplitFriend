import { useState } from 'react';
import { Repeat, Trash2, Pencil, Play } from 'lucide-react';

import { useRecurringTemplates } from '@/hooks/useRecurringTemplates';
import type { RecurringTemplate } from '@/types/finance.types';
import { formatMoney, formatDate } from '@/utils/format';

import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { Spinner } from '@/components/common/Spinner';
import { ConfirmDialog } from '@/components/dashboard/ConfirmDialog';
import { RecurringTemplateModal } from '@/components/dashboard/RecurringTemplateModal';

// Forme singular / plural pentru afisare corecta in romana ("La fiecare 3 zile").
const FREQ_LABEL: Record<RecurringTemplate['frequency'], { one: string; many: string }> = {
  DAILY:   { one: 'zi',        many: 'zile' },
  WEEKLY:  { one: 'săptămână', many: 'săptămâni' },
  MONTHLY: { one: 'lună',      many: 'luni' },
  YEARLY:  { one: 'an',        many: 'ani' },
};

const describeFrequency = (t: RecurringTemplate): string =>
  t.intervalCount === 1
    ? `La fiecare ${FREQ_LABEL[t.frequency].one}`
    : `La fiecare ${t.intervalCount} ${FREQ_LABEL[t.frequency].many}`;

export const Recurente: React.FC = () => {
  const { templates, isLoading, error, update, deactivate, runDue } = useRecurringTemplates();

  const [editing, setEditing]   = useState<RecurringTemplate | null>(null);
  const [toStop, setToStop]     = useState<RecurringTemplate | null>(null);
  const [stopping, setStopping] = useState(false);
  const [running, setRunning]   = useState(false);
  const [notice, setNotice]     = useState<string | null>(null);

  const handleRunNow = async () => {
    setRunning(true);
    setNotice(null);
    try {
      const count = await runDue();
      setNotice(count > 0 ? `${count} tranzacții generate.` : 'Nimic de generat acum.');
    } finally {
      setRunning(false);
    }
  };

  const confirmStop = async () => {
    if (!toStop) return;
    setStopping(true);
    try {
      await deactivate(toStop.id);
      setToStop(null);
    } finally {
      setStopping(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold text-slate-900">Plăți recurente</h1>
        <Button onClick={() => void handleRunNow()} loading={running}>
          <Play className="h-4 w-4" /> Generează acum
        </Button>
      </div>

      {notice && <Alert type="success" message={notice} />}
      {error && <Alert type="error" message={error} />}

      <div className="rounded-2xl glass-card p-5">
        {isLoading ? (
          <div className="flex justify-center py-10"><Spinner /></div>
        ) : templates.length === 0 ? (
          <p className="py-8 text-center text-slate-500">
            Niciun șablon recurent. Bifează „Tranzacție recurentă" când adaugi un venit sau o cheltuială.
          </p>
        ) : (
          <ul className="divide-y divide-slate-900/10">
            {templates.map(t => (
              <li key={t.id} className="flex items-center justify-between gap-4 py-3">
                <div className="flex items-center gap-3">
                  <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-slate-100 text-slate-600">
                    <Repeat className="h-5 w-5" />
                  </span>
                  <div>
                    <p className="font-medium text-slate-900">
                      {formatMoney(t.amount, t.currencyCode)}
                      <span className={t.kind === 'INCOME' ? 'ml-2 text-xs text-emerald-600' : 'ml-2 text-xs text-rose-600'}>
                        {t.kind === 'INCOME' ? 'venit' : 'cheltuială'}
                      </span>
                    </p>
                    <p className="text-sm text-slate-500">
                      {describeFrequency(t)} · {t.categoryName ?? 'Fără categorie'} · următoarea: {formatDate(t.nextRunDate)}
                    </p>
                  </div>
                </div>
                <div className="flex gap-1">
                  <button type="button" onClick={() => setEditing(t)} aria-label="Editează"
                    className="rounded-lg p-2 text-slate-500 hover:bg-slate-100 hover:text-slate-700">
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button type="button" onClick={() => setToStop(t)} aria-label="Oprește"
                    className="rounded-lg p-2 text-rose-500 hover:bg-rose-50">
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <RecurringTemplateModal
        open={!!editing}
        template={editing}
        onSubmit={update}
        onClose={() => setEditing(null)}
      />

      <ConfirmDialog
        open={!!toStop}
        title="Oprire șablon recurent"
        message="Șablonul va fi dezactivat și nu va mai genera tranzacții. Continui?"
        loading={stopping}
        onConfirm={() => void confirmStop()}
        onCancel={() => setToStop(null)}
      />
    </div>
  );
};
