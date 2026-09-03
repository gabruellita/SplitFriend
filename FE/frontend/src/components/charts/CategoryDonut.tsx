import { useMemo } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import { PieChart as PieIcon } from 'lucide-react';
import type { CategoryBreakdown, TransactionKind } from '@/types/finance.types';
import { formatMoney } from '@/utils/format';

interface CategoryDonutProps {
  data:         CategoryBreakdown[];
  kind:         TransactionKind;   // afiseaza doar categoriile de acest tip
  currencyCode?: string | null;
}

// Paleta accesibila (contrast suficient, distincta fara a depinde doar de culoare).
const PALETTE = ['#2563eb', '#059669', '#e11d48', '#d97706', '#7c3aed', '#0891b2', '#64748b'];

export const CategoryDonut: React.FC<CategoryDonutProps> = ({ data, kind, currencyCode }) => {
  const slices = useMemo(() => {
    const filtered = data
      .filter(d => d.kind === kind && d.total > 0)
      .sort((a, b) => b.total - a.total)
      .map(d => ({ name: d.categoryName ?? 'Fără categorie', value: d.total }));

    // Top 6 + "Altele" (regula no-pie-overuse pentru >5-6 categorii).
    if (filtered.length <= 6) return filtered;
    const top = filtered.slice(0, 6);
    const rest = filtered.slice(6).reduce((sum, s) => sum + s.value, 0);
    return [...top, { name: 'Altele', value: rest }];
  }, [data, kind]);

  const total = slices.reduce((s, x) => s + x.value, 0);

  if (slices.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center text-slate-500">
        <PieIcon className="mb-2 h-8 w-8" />
        <p className="text-sm">Nu există date pentru această perioadă.</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
      <div className="h-48 w-full sm:w-48">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie data={slices} dataKey="value" nameKey="name" innerRadius={50} outerRadius={80} paddingAngle={2}>
              {slices.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
            </Pie>
            <Tooltip formatter={(v) => formatMoney(Number(v), currencyCode)} />
          </PieChart>
        </ResponsiveContainer>
      </div>

      {/* Legenda = tabel fallback accesibil (valori mereu vizibile, nu doar culoare) */}
      <ul className="flex-1 space-y-1.5 text-sm">
        {slices.map((s, i) => (
          <li key={s.name} className="flex items-center justify-between gap-3">
            <span className="flex items-center gap-2">
              <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: PALETTE[i % PALETTE.length] }} aria-hidden="true" />
              <span className="text-slate-700">{s.name}</span>
            </span>
            <span className="tabular-nums text-slate-500">
              {total > 0 ? Math.round((s.value / total) * 100) : 0}% · {formatMoney(s.value, currencyCode)}
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
};
