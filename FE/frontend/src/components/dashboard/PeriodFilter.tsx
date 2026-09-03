import { useState } from 'react';
import type { Category } from '@/types/finance.types';
import { presetRange, type PeriodValue, type Preset } from '@/utils/period';

interface PeriodFilterProps {
  value:       PeriodValue;
  categories?: Category[];
  onChange:    (v: PeriodValue) => void;
}

const selectClass =
  'rounded-lg border border-slate-300 bg-white/70 px-3 py-1.5 text-sm text-slate-700 outline-none transition focus:ring-2 focus:ring-brand-500 cursor-pointer';

export const PeriodFilter: React.FC<PeriodFilterProps> = ({ value, categories, onChange }) => {
  const [preset, setPreset] = useState<Preset>('thisMonth');

  const handlePreset = (p: Preset) => {
    setPreset(p);
    if (p !== 'custom') onChange({ ...value, ...presetRange(p) });
  };

  return (
    <div className="flex flex-wrap items-center gap-2">
      <select className={selectClass} value={preset}
        onChange={e => handlePreset(e.target.value as Preset)} aria-label="Perioadă">
        <option value="thisMonth">Luna aceasta</option>
        <option value="lastMonth">Luna trecută</option>
        <option value="custom">Personalizat</option>
      </select>

      {preset === 'custom' && (
        <>
          <input type="date" className={selectClass} value={value.from} aria-label="De la"
            onChange={e => onChange({ ...value, from: e.target.value })} />
          <span className="text-slate-400">–</span>
          <input type="date" className={selectClass} value={value.to} aria-label="Până la"
            onChange={e => onChange({ ...value, to: e.target.value })} />
        </>
      )}

      {categories && categories.length > 0 && (
        <select className={selectClass} aria-label="Categorie"
          value={value.categoryId ?? ''}
          onChange={e => onChange({ ...value, categoryId: e.target.value ? Number(e.target.value) : undefined })}>
          <option value="">Toate categoriile</option>
          {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
      )}
    </div>
  );
};
