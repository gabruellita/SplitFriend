// src/utils/splitPreview.ts
import { SplitType } from '@/types/group.types';

export interface PreviewParticipant {
  userId:       number;
  selected:     boolean;        // EQUAL: participă sau nu
  exactAmount?: number | null;  // EXACT
  percent?:     number | null;  // PERCENT
  shares?:      number | null;  // SHARES
}

export interface OwedResult {
  owed:  Map<number, number>;   // userId → owedAmount (2 zecimale)
  total: number;                // suma owed (ar trebui == amount)
  valid: boolean;
  error: string | null;
}

const round2 = (n: number) => Math.round(n * 100) / 100;

/**
 * Calculează owed per participant. Ultimul participant absoarbe eroarea de rotunjire,
 * exact ca backend-ul → preview-ul reflectă fidel ce va salva serverul.
 */
export const computeOwed = (
  splitType: SplitType,
  amount: number,
  participants: PreviewParticipant[],
): OwedResult => {
  const active = participants.filter(p => p.selected);
  const owed = new Map<number, number>();

  if (!amount || amount <= 0) return { owed, total: 0, valid: false, error: 'Suma trebuie să fie > 0' };
  if (active.length === 0)    return { owed, total: 0, valid: false, error: 'Alege cel puțin un participant' };

  const assignWithRounding = (raw: number[]): void => {
    let acc = 0;
    active.forEach((p, i) => {
      const v = i === active.length - 1 ? round2(amount - acc) : round2(raw[i]);
      acc = round2(acc + v);
      owed.set(p.userId, v);
    });
  };

  if (splitType === SplitType.EQUAL) {
    const per = amount / active.length;
    assignWithRounding(active.map(() => per));
    return { owed, total: amount, valid: true, error: null };
  }

  if (splitType === SplitType.EXACT) {
    const sum = round2(active.reduce((s, p) => s + (p.exactAmount ?? 0), 0));
    active.forEach(p => owed.set(p.userId, round2(p.exactAmount ?? 0)));
    const valid = sum === round2(amount);
    return { owed, total: sum, valid, error: valid ? null : `Suma alocată (${sum}) trebuie să fie ${round2(amount)}` };
  }

  if (splitType === SplitType.PERCENT) {
    const sumPct = round2(active.reduce((s, p) => s + (p.percent ?? 0), 0));
    const valid = sumPct === 100;
    assignWithRounding(active.map(p => (amount * (p.percent ?? 0)) / 100));
    return { owed, total: amount, valid, error: valid ? null : `Procentele însumează ${sumPct}%, trebuie 100%` };
  }

  // SHARES
  const totalShares = active.reduce((s, p) => s + (p.shares ?? 0), 0);
  if (totalShares <= 0) return { owed, total: 0, valid: false, error: 'Numărul total de părți trebuie să fie > 0' };
  assignWithRounding(active.map(p => (amount * (p.shares ?? 0)) / totalShares));
  return { owed, total: amount, valid: true, error: null };
};
