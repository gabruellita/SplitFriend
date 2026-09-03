// src/components/groups/tabs/BalancesTab.tsx
import { useState } from 'react';
import { useGroupContext } from '@/context/GroupContext';
import { useGroupBalances } from '@/hooks/useGroupBalances';
import { useGroupPayments } from '@/hooks/useGroupPayments';
import { BalancePill } from '@/components/groups/BalancePill';
import { SettleUpModal } from '@/components/groups/SettleUpModal';
import { useUserCurrency } from '@/hooks/useUserCurrency';
import { useConvert } from '@/hooks/useConvert';
import { ConvertedAmount } from '@/components/groups/ConvertedAmount';
import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { Spinner } from '@/components/common/Spinner';
import { formatMoney, formatDate } from '@/utils/format';
import type { GroupBalance } from '@/types/group.types';

// Soldul PROPRIU al userului, afișat în moneda LUI (convertit din moneda ancoră a rândului).
// Un component per rând ⇒ un singur apel `useConvert` per instanță (respectă rules-of-hooks
// chiar dacă numărul de rânduri e dinamic).
const MyBalanceLine: React.FC<{ balance: GroupBalance; myCurrency?: string }> = ({ balance, myCurrency }) => {
  const net    = balance.netAmount;
  const anchor = balance.currencyCode ?? undefined;
  const same   = !myCurrency || !anchor || anchor === myCurrency;
  const conv   = useConvert(Math.abs(net), anchor, myCurrency);

  // Aceeași monedă (sau lipsă) → nu e nevoie de conversie.
  if (same) {
    return <BalancePill amount={net} currencyCode={anchor ?? myCurrency} className="text-2xl" />;
  }
  // În conversie → arătăm valoarea ancoră cu un indiciu.
  if (conv.loading) {
    return (
      <span className="flex items-baseline gap-2">
        <BalancePill amount={net} currencyCode={anchor} className="text-2xl" />
        <span className="text-xs text-slate-400">conversie…</span>
      </span>
    );
  }
  // Curs indisponibil → fallback pe moneda ancoră (nu pierdem informația).
  if (conv.error || conv.result == null) {
    return <BalancePill amount={net} currencyCode={anchor} className="text-2xl" />;
  }
  // Convertit în moneda mea, semnul păstrat din net.
  const signed = net < 0 ? -conv.result : conv.result;
  return (
    <span className="flex flex-col">
      <BalancePill amount={signed} currencyCode={myCurrency} className="text-2xl" />
      <span className="text-xs text-slate-400">
        din {formatMoney(Math.abs(net), anchor)} · curs {conv.rate?.toFixed(4)}
      </span>
    </span>
  );
};

export const BalancesTab: React.FC = () => {
  const { group, currentUserId, nameOf } = useGroupContext();
  const { balances, isLoading, error, refetch: refetchBalances } = useGroupBalances(group.id);
  const { payments, create: createPayment, refetch: refetchPayments, error: paymentsError } = useGroupPayments(group.id);

  const [settleTarget, setSettleTarget] = useState<{ userId: number; amount: number; currencyCode: string | null } | null>(null);
  const myCurrency = useUserCurrency() ?? undefined;

  // Rândurile mele (pot fi mai multe — câte unul pe monedă ancoră).
  const myRows = balances.filter(b => b.userId === currentUserId);

  // Cât datorez/mi se datorează pe fiecare monedă (currencyId → net), pentru logica butonului „Achită".
  const myNetByCurrency = new Map<number, number>();
  myRows.forEach(b => myNetByCurrency.set(b.currencyId, (myNetByCurrency.get(b.currencyId) ?? 0) + b.netAmount));

  // Ceilalți membri, grupați pe userId (păstrând ordinea de apariție).
  const byMember = new Map<number, GroupBalance[]>();
  balances.filter(b => b.userId !== currentUserId).forEach(b => {
    const arr = byMember.get(b.userId) ?? [];
    arr.push(b);
    byMember.set(b.userId, arr);
  });

  return (
    <div className="space-y-5">
      {error && <Alert type="error" message={error} />}
      {paymentsError && <Alert type="error" message={paymentsError} />}

      <div className="glass-card rounded-2xl p-5">
        <p className="text-sm text-slate-500">Soldul tău în grup</p>
        {myRows.length === 0 ? (
          <BalancePill amount={0} currencyCode={myCurrency ?? group.currencyCode} className="text-2xl" />
        ) : (
          <div className="space-y-1">
            {myRows.map(b => <MyBalanceLine key={b.currencyId} balance={b} myCurrency={myCurrency} />)}
          </div>
        )}
      </div>

      {isLoading ? (
        <div className="flex justify-center py-12"><Spinner size="lg" /></div>
      ) : (
        <div>
          <h2 className="mb-2 font-semibold text-slate-900">Per membru</h2>
          <ul className="divide-y divide-slate-900/10 rounded-2xl glass-card">
            {[...byMember.entries()].map(([userId, rows]) => (
              <li key={userId} className="px-4 py-3">
                <div className="mb-2 truncate font-medium text-slate-700">{nameOf(userId)}</div>
                <ul className="space-y-2">
                  {rows.map(b => {
                    // „Achită" apare când eu datorez în moneda b (net_meu[ccy] < 0) ȘI
                    // membrul e creditor în aceeași monedă (b.netAmount > 0).
                    const myNet     = myNetByCurrency.get(b.currencyId) ?? 0;
                    const iOwe      = myNet < 0 && b.netAmount > 0;
                    const suggested = Math.min(Math.abs(myNet), b.netAmount);
                    return (
                      <li key={b.currencyId} className="flex items-center justify-between gap-3">
                        <span className="flex flex-col items-start">
                          <BalancePill amount={b.netAmount} currencyCode={b.currencyCode} />
                          {b.netAmount !== 0 && (
                            <ConvertedAmount amount={Math.abs(b.netAmount)} from={b.currencyCode} to={myCurrency} />
                          )}
                        </span>
                        {iOwe && (
                          <Button onClick={() => setSettleTarget({
                            userId,
                            amount: Math.round(suggested * 100) / 100,
                            currencyCode: b.currencyCode,
                          })}>
                            Achită
                          </Button>
                        )}
                      </li>
                    );
                  })}
                </ul>
              </li>
            ))}
            {byMember.size === 0 && <li className="px-4 py-6 text-center text-sm text-slate-500">Niciun alt membru cu sold.</li>}
          </ul>
        </div>
      )}

      <div>
        <h2 className="mb-2 font-semibold text-slate-900">Istoric plăți</h2>
        {payments.length === 0 ? (
          <p className="rounded-2xl glass-card px-4 py-6 text-center text-sm text-slate-500">Nicio plată încă.</p>
        ) : (
          <ul className="divide-y divide-slate-900/10 rounded-2xl glass-card">
            {payments.map(p => {
              // Moneda debitorului diferă de a creditorului → arătăm „21 EUR → 100 RON".
              const sameCcy = !p.originalCurrencyCode || p.originalCurrencyCode === p.currencyCode;
              return (
                <li key={p.id} className="flex items-center justify-between px-4 py-3 text-sm">
                  <span className="text-slate-700">{nameOf(p.fromUserId)} → {nameOf(p.toUserId)}</span>
                  <span className="flex items-center gap-3 text-slate-500">
                    <span className="tabular-nums text-slate-900">
                      {sameCcy
                        ? formatMoney(p.amount, p.currencyCode)
                        : `${formatMoney(p.originalAmount, p.originalCurrencyCode)} → ${formatMoney(p.amount, p.currencyCode)}`}
                    </span>
                    <span>{formatDate(p.paidAt)}</span>
                  </span>
                </li>
              );
            })}
          </ul>
        )}
      </div>

      <SettleUpModal
        open={settleTarget !== null}
        toUserId={settleTarget?.userId ?? null}
        suggested={settleTarget?.amount ?? 0}
        currencyCode={settleTarget?.currencyCode ?? group.currencyCode}
        onClose={() => setSettleTarget(null)}
        onSubmit={async (body) => {
          await createPayment(body);
          // Refresh-ul balanțelor/plăților e în fundal — o eroare la refetch NU
          // înseamnă că plata a eșuat, deci nu o lăsăm să arunce în onSubmit.
          void Promise.all([refetchBalances(), refetchPayments()]).catch(console.error);
        }}
      />
    </div>
  );
};
