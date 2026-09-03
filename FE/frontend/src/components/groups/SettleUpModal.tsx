// src/components/groups/SettleUpModal.tsx
import { useEffect, useState } from 'react';
import { X } from 'lucide-react';
import { useGroupContext } from '@/context/GroupContext';
import type { CreatePaymentRequest } from '@/types/group.types';
import { Input }  from '@/components/common/Input';
import { Button } from '@/components/common/Button';
import { Alert }  from '@/components/common/Alert';
import { useConvert } from '@/hooks/useConvert';
import { useUserCurrency } from '@/hooks/useUserCurrency';

interface SettleUpModalProps {
  open:          boolean;
  toUserId:      number | null;   // cui plătești
  suggested:     number;          // suma pre-completată (cât datorezi)
  currencyCode?: string | null;
  onSubmit:      (body: CreatePaymentRequest) => Promise<void>;
  onClose:       () => void;
}

export const SettleUpModal: React.FC<SettleUpModalProps> = ({
  open, toUserId, suggested, currencyCode, onSubmit, onClose,
}) => {
  const { nameOf } = useGroupContext();
  const [amount, setAmount] = useState<number>(suggested);
  const [error, setError]   = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  // Moneda vizualizatorului (codul real, rezolvat din preferredCurrencyId — vezi
  // useUserCurrency). `amount` e în moneda creditorului (= currencyCode); convertim
  // spre moneda vizualizatorului doar pentru afișare.
  const viewerCurrency = useUserCurrency() ?? undefined;

  // Hook-ul trebuie apelat necondiționat, ÎNAINTE de orice early-return (rules of hooks).
  const conversion = useConvert(amount, currencyCode ?? undefined, viewerCurrency);

  // reset doar la deschidere — nu suprascrie ce tastează userul dacă suggested se schimbă în fundal
  useEffect(() => { if (open) { setAmount(suggested); setError(null); } }, [open]); // eslint-disable-line react-hooks/exhaustive-deps
  if (!open || toUserId == null) return null;

  // Linia de conversie apare doar când moneda datoriei există, diferă de cea a
  // vizualizatorului și suma e > 0.
  const showConversion = !!currencyCode && !!viewerCurrency && currencyCode !== viewerCurrency && amount > 0;

  const submit = async () => {
    if (!amount || amount <= 0 || Number.isNaN(amount)) { setError('Suma trebuie să fie > 0'); return; }
    setSubmitting(true);
    setError(null);
    try {
      await onSubmit({ toUserId, amount });
      onClose();
    } catch {
      setError('Nu s-a putut înregistra plata. Reîncearcă.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/50" onClick={onClose} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-label="Achită datoria"
        className="relative w-full max-w-sm rounded-2xl bg-white p-6 shadow-xl">
        <button type="button" onClick={onClose} aria-label="Închide"
          className="absolute right-4 top-4 rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700">
          <X className="h-5 w-5" aria-hidden="true" />
        </button>
        <h2 className="mb-1 text-lg font-semibold text-slate-900">Achită datoria</h2>
        <p className="mb-4 text-sm text-slate-500">Plată către <span className="font-medium">{nameOf(toUserId)}</span></p>
        {error && <Alert type="error" message={error} />}
        <Input label={`Sumă${currencyCode ? ` (${currencyCode})` : ''}`} type="number" step="0.01" min="0"
          inputMode="decimal" value={amount || ''} onChange={e => setAmount(parseFloat(e.target.value))} />

        {showConversion && (
          <div className="mt-3 rounded-xl bg-slate-50 px-3 py-2.5 ring-1 ring-slate-900/5">
            {conversion.loading ? (
              <p className="text-xs text-slate-400">conversie…</p>
            ) : conversion.error || conversion.result == null ? (
              <p className="text-xs text-slate-400">curs indisponibil</p>
            ) : (
              <>
                <p className="text-sm text-slate-600">
                  Datorezi{' '}
                  <span className="font-semibold tabular-nums text-slate-900">{amount.toFixed(2)} {currencyCode}</span>{' '}
                  ≈{' '}
                  <span className="font-semibold tabular-nums text-brand-600">{conversion.result.toFixed(2)} {viewerCurrency}</span>
                  <span className="text-slate-400"> (curs {conversion.rate?.toFixed(4)}, {conversion.date})</span>
                </p>
                <p className="mt-1 text-xs text-slate-400">Cursul final se aplică la confirmare.</p>
              </>
            )}
          </div>
        )}

        <div className="mt-3 flex justify-end gap-3">
          <Button type="button" variant="secondary" onClick={onClose} disabled={submitting}>Anulează</Button>
          <Button type="button" onClick={submit} loading={submitting}>Înregistrează plata</Button>
        </div>
      </div>
    </div>
  );
};
