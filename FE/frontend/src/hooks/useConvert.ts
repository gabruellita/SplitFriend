import { useEffect, useState } from 'react';
import { currencyApi } from '@/api/currencyApi';
import type { ConvertResult } from '@/types/currency.types';

// cache rate per from|to for 10 min
const rateCache = new Map<string, { rate: number; date: string; at: number }>();
const TTL = 10 * 60 * 1000;

export function useConvert(amount: number, from?: string, to?: string) {
  const [state, setState] = useState<{ result?: number; rate?: number; date?: string; loading: boolean; error?: string }>({ loading: false });

  useEffect(() => {
    if (!from || !to || amount <= 0) { setState({ loading: false }); return; }
    if (from === to) { setState({ result: amount, rate: 1, date: new Date().toISOString().slice(0, 10), loading: false }); return; }

    const key = `${from}|${to}`;
    const cached = rateCache.get(key);
    if (cached && Date.now() - cached.at < TTL) {
      setState({ result: Math.round(amount * cached.rate * 100) / 100, rate: cached.rate, date: cached.date, loading: false });
      return;
    }

    let active = true;
    setState({ loading: true });
    currencyApi.convert(from, to, amount)
      .then((r: ConvertResult) => {
        rateCache.set(key, { rate: r.rate, date: r.date, at: Date.now() });
        if (active) setState({ result: r.result, rate: r.rate, date: r.date, loading: false });
      })
      .catch(() => { if (active) setState({ loading: false, error: 'Curs indisponibil' }); });
    return () => { active = false; };
  }, [amount, from, to]);

  return state;
}
