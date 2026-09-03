import { useEffect, useState } from 'react';
import clsx from 'clsx';
import { User, ShieldCheck } from 'lucide-react';

import { authApi } from '@/api/authApi';
import { parseApiError } from '@/utils/errorParser';
import type { MeResponse } from '@/types/auth.types';

import { Alert } from '@/components/common/Alert';
import { Spinner } from '@/components/common/Spinner';
import { ProfileTab } from './ProfileTab';
import { SecurityTab } from './SecurityTab';

type AccountTab = 'profile' | 'security';

const TABS: { key: AccountTab; label: string; icon: React.ComponentType<{ className?: string }> }[] = [
  { key: 'profile',  label: 'Profil',     icon: User },
  { key: 'security', label: 'Securitate', icon: ShieldCheck },
];

export const Account: React.FC = () => {
  const [active, setActive]   = useState<AccountTab>('profile');
  const [me, setMe]           = useState<MeResponse | null>(null);
  const [isLoading, setLoad]  = useState(true);
  const [error, setError]     = useState<string | null>(null);

  useEffect(() => {
    let alive = true;
    (async () => {
      try {
        const data = await authApi.getMe();
        if (alive) setMe(data);
      } catch (err) {
        if (alive) setError(parseApiError(err).message || 'Nu am putut încărca datele contului.');
      } finally {
        if (alive) setLoad(false);
      }
    })();
    return () => { alive = false; };
  }, []);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Despre cont</h1>
        <p className="text-sm text-slate-500">Gestionează-ți datele personale și securitatea contului.</p>
      </div>

      <div role="tablist" aria-label="Setări cont"
        className="flex gap-1 overflow-x-auto border-b border-slate-900/10">
        {TABS.map(({ key, label, icon: Icon }) => (
          <button key={key} role="tab" aria-selected={active === key} type="button"
            onClick={() => setActive(key)}
            className={clsx(
              'flex items-center gap-2 whitespace-nowrap px-4 py-2.5 text-sm font-medium transition cursor-pointer',
              'focus:outline-none focus:ring-2 focus:ring-brand-500 rounded-t-lg',
              active === key
                ? 'border-b-2 border-brand-600 text-brand-700'
                : 'text-slate-500 hover:text-slate-900',
            )}>
            <Icon className="h-4 w-4" />
            {label}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="rounded-2xl glass-card p-10">
          <Spinner size="lg" />
        </div>
      ) : error ? (
        <Alert type="error" message={error} />
      ) : me ? (
        <div className="rounded-2xl glass-card p-5 sm:p-6 max-w-2xl">
          {active === 'profile'  && <ProfileTab me={me} />}
          {active === 'security' && <SecurityTab />}
        </div>
      ) : null}
    </div>
  );
};
