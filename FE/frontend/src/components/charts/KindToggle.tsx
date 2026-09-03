import type { TransactionKind } from '@/types/finance.types';

interface KindToggleProps {
  value:    TransactionKind;
  onChange: (k: TransactionKind) => void;
}

const base = 'px-2.5 py-1 text-xs font-medium transition cursor-pointer focus:outline-none focus:ring-2 focus:ring-brand-500';

/** Comutator segmentat Cheltuieli / Venituri pentru graficele care cer `kind`. */
export const KindToggle: React.FC<KindToggleProps> = ({ value, onChange }) => (
  <div className="inline-flex overflow-hidden rounded-lg border border-slate-300" role="group" aria-label="Tip tranzacție">
    <button
      type="button"
      onClick={() => onChange('EXPENSE')}
      aria-pressed={value === 'EXPENSE'}
      className={`${base} ${value === 'EXPENSE' ? 'bg-slate-900 text-white' : 'bg-white/70 text-slate-600 hover:bg-slate-100'}`}
    >
      Cheltuieli
    </button>
    <button
      type="button"
      onClick={() => onChange('INCOME')}
      aria-pressed={value === 'INCOME'}
      className={`${base} ${value === 'INCOME' ? 'bg-slate-900 text-white' : 'bg-white/70 text-slate-600 hover:bg-slate-100'}`}
    >
      Venituri
    </button>
  </div>
);
