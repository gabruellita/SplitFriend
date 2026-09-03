import type { ReactNode } from 'react';
import { AlertTriangle, RotateCw } from 'lucide-react';

interface ChartCardProps {
  title:         string;
  description?:  string;
  controls?:     ReactNode;
  isLoading?:    boolean;
  error?:        string | null;
  isEmpty?:      boolean;
  emptyMessage?: string;
  onRetry?:      () => void;
  className?:    string;
  children:      ReactNode;
}

/** Container glass-card pentru un grafic, cu stari skeleton / empty / error+retry. */
export const ChartCard: React.FC<ChartCardProps> = ({
  title, description, controls, isLoading, error, isEmpty,
  emptyMessage = 'Nu există date pentru această perioadă.', onRetry, className, children,
}) => (
  <section className={`rounded-2xl glass-card p-5 ${className ?? ''}`} aria-label={title}>
    <header className="mb-4 flex flex-wrap items-start justify-between gap-3">
      <div>
        <h3 className="text-base font-semibold text-slate-900">{title}</h3>
        {description && <p className="text-xs text-slate-500">{description}</p>}
      </div>
      {controls && <div className="flex flex-wrap items-center gap-2">{controls}</div>}
    </header>

    {isLoading ? (
      <div className="h-64 animate-pulse rounded-xl bg-slate-200/60" />
    ) : error ? (
      <div className="flex h-64 flex-col items-center justify-center gap-3 text-center text-slate-500">
        <AlertTriangle className="h-8 w-8 text-rose-500" aria-hidden="true" />
        <p className="text-sm">{error}</p>
        {onRetry && (
          <button
            type="button"
            onClick={onRetry}
            className="inline-flex items-center gap-1.5 rounded-lg border border-slate-300 bg-white/70 px-3 py-1.5 text-sm text-slate-700 transition hover:bg-white cursor-pointer focus:outline-none focus:ring-2 focus:ring-brand-500"
          >
            <RotateCw className="h-4 w-4" /> Reîncearcă
          </button>
        )}
      </div>
    ) : isEmpty ? (
      <div className="flex h-64 flex-col items-center justify-center text-center text-slate-500">
        <p className="text-sm">{emptyMessage}</p>
      </div>
    ) : (
      children
    )}
  </section>
);
