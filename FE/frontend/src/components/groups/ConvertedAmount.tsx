// src/components/groups/ConvertedAmount.tsx
import { ArrowLeftRight } from 'lucide-react';
import { useConvert } from '@/hooks/useConvert';

interface ConvertedAmountProps {
  amount: number;
  from?:  string | null;
  to?:    string;
}

// Badge inline cu echivalentul live în moneda vizualizatorului.
// Nu se afișează când moneda lipsește sau coincide cu cea a vizualizatorului.
export const ConvertedAmount: React.FC<ConvertedAmountProps> = ({ amount, from, to }) => {
  const { result, rate, date, loading, error } = useConvert(amount, from ?? undefined, to);

  // Garda e intenționat redundantă cu useConvert: hook-ul întoarce {rate:1, result:amount}
  // când from===to, deci fără verificarea de mai jos s-ar afișa "≈ X · curs 1.0000". O păstrăm.
  if (!from || !to || from === to) return null;

  if (loading) {
    return <span className="text-xs text-slate-400">conversie…</span>;
  }

  if (error || result == null) {
    return <span className="text-xs text-slate-400">curs indisponibil</span>;
  }

  return (
    <span className="inline-flex items-center gap-1 text-xs text-slate-400">
      <ArrowLeftRight className="h-3 w-3 shrink-0" aria-hidden="true" />
      <span className="tabular-nums text-slate-500">≈ {result.toFixed(2)} {to}</span>
      <span className="text-slate-300" aria-hidden="true">·</span>
      <span className="tabular-nums">curs {rate?.toFixed(4)}</span>
      <span className="text-slate-300" aria-hidden="true">·</span>
      <span>{date}</span>
    </span>
  );
};
