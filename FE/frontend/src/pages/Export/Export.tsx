import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FileDown, Calendar, CalendarRange, CheckSquare, Settings2, AlertCircle } from 'lucide-react';
import { exportSchema, type ExportFormValues } from '@/schemas/exportSchema';
import { exportApi } from '@/api/exportApi';
import type { ExportBlock } from '@/types/export.types';

// ─── Constante ────────────────────────────────────────────────────────────────

const BLOCKS: { value: ExportBlock; label: string; description: string }[] = [
  { value: 'SUMMARY',      label: 'Copertă + sumar',      description: 'KPI-uri: venituri, cheltuieli, sold net' },
  { value: 'TREND',        label: 'Grafic evoluție',       description: 'Venituri vs cheltuieli în timp' },
  { value: 'CATEGORIES',   label: 'Defalcare categorii',   description: 'Top categorii și grafic proporții' },
  { value: 'TRANSACTIONS', label: 'Extras tranzacții',     description: 'Lista completă de tranzacții' },
];

/** Ultimele 12 luni ca opțiuni "YYYY-MM". */
function lastMonths(n: number): { value: string; label: string }[] {
  const out: { value: string; label: string }[] = [];
  const d = new Date();
  for (let i = 0; i < n; i++) {
    const dt = new Date(d.getFullYear(), d.getMonth() - i, 1);
    const value = `${dt.getFullYear()}-${String(dt.getMonth() + 1).padStart(2, '0')}`;
    out.push({ value, label: dt.toLocaleDateString('ro-RO', { month: 'short', year: 'numeric' }) });
  }
  return out;
}

// ─── Componente mici ──────────────────────────────────────────────────────────

/** Card glassmorphic refolosibil pentru secțiuni. */
const Card: React.FC<{ children: React.ReactNode; className?: string }> = ({ children, className = '' }) => (
  <div className={`rounded-xl bg-white/60 backdrop-blur-md border border-slate-200/60 shadow-sm p-5 ${className}`}>
    {children}
  </div>
);

/** Titlu de secțiune cu icon. */
const SectionTitle: React.FC<{ icon: React.ReactNode; label: string }> = ({ icon, label }) => (
  <div className="flex items-center gap-2 mb-4">
    <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
      {icon}
    </span>
    <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">{label}</h2>
  </div>
);

// ─── Pagina principală ────────────────────────────────────────────────────────

export const Export: React.FC = () => {
  const [submitting, setSubmitting] = useState(false);
  const [error, setError]           = useState<string | null>(null);
  const monthOptions                 = lastMonths(12);

  const { register, handleSubmit, watch, setValue, formState: { errors } } =
    useForm<ExportFormValues>({
      resolver: zodResolver(exportSchema),
      defaultValues: {
        mode: 'MONTHS',
        months: [],
        blocks: ['SUMMARY', 'TREND', 'CATEGORIES', 'TRANSACTIONS'],
        granularity: 'DAILY',
        runningBalanceInStatement: false,
        cumulativeTotal: true,
      },
    });

  const mode   = watch('mode');
  const months = watch('months') ?? [];
  const blocks = watch('blocks');

  const toggleMonth = (m: string) =>
    setValue('months', months.includes(m) ? months.filter(x => x !== m) : [...months, m], { shouldValidate: true });

  const toggleBlock = (b: ExportBlock) =>
    setValue('blocks', blocks.includes(b) ? blocks.filter(x => x !== b) : [...blocks, b], { shouldValidate: true });

  const onSubmit = async (v: ExportFormValues) => {
    setError(null);
    setSubmitting(true);
    try {
      await exportApi.generateReport({
        mode: v.mode,
        months: v.mode === 'MONTHS' ? v.months : undefined,
        range:  v.mode === 'RANGE'  ? v.range  : undefined,
        blocks: v.blocks,
        options: {
          granularity: v.granularity ?? 'DAILY',
          runningBalanceInStatement: v.runningBalanceInStatement ?? false,
          cumulativeTotal: v.cumulativeTotal ?? true,
        },
      });
    } catch {
      setError('Nu am putut genera raportul. Verifică dacă serviciile sunt pornite și reîncearcă.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* ── Antet ──────────────────────────────────────────────────────────── */}
      <div className="sticky top-0 z-10 -mx-4 flex flex-wrap items-center justify-between gap-3 bg-slate-50/80 px-4 py-3 backdrop-blur-md lg:-mx-6 lg:px-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Export PDF</h1>
          <p className="text-sm text-slate-500">Generează un raport financiar detaliat în format PDF.</p>
        </div>
        <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand-600 text-white shadow-sm">
          <FileDown className="h-5 w-5" />
        </span>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 max-w-2xl" noValidate>

        {/* ── 1. Mod raport ───────────────────────────────────────────────── */}
        <Card>
          <SectionTitle icon={<Calendar className="h-4 w-4" />} label="Perioada" />

          {/* Segmented control */}
          <div
            role="group"
            aria-label="Mod raport"
            className="inline-flex rounded-xl bg-slate-100 p-1 gap-1 mb-5"
          >
            {([
              { value: 'MONTHS', label: 'Luni multiple',   Icon: Calendar },
              { value: 'RANGE',  label: 'Interval liber',  Icon: CalendarRange },
            ] as const).map(({ value, label, Icon }) => (
              <button
                key={value}
                type="button"
                onClick={() => setValue('mode', value)}
                aria-pressed={mode === value}
                className={[
                  'flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium transition-all duration-200 cursor-pointer',
                  mode === value
                    ? 'bg-white text-brand-600 shadow-sm border border-slate-200/80'
                    : 'text-slate-500 hover:text-slate-700',
                ].join(' ')}
              >
                <Icon className="h-4 w-4" />
                {label}
              </button>
            ))}
          </div>

          {/* Conținut condiționat */}
          {mode === 'MONTHS' ? (
            <div className="space-y-4">
              <div>
                <p className="mb-2.5 text-sm font-medium text-slate-700">
                  Alege lunile
                  {months.length > 0 && (
                    <span className="ml-2 rounded-full bg-brand-100 px-2 py-0.5 text-xs font-semibold text-brand-700">
                      {months.length} selectate
                    </span>
                  )}
                </p>
                <div className="flex flex-wrap gap-2">
                  {monthOptions.map(o => {
                    const active = months.includes(o.value);
                    return (
                      <button
                        key={o.value}
                        type="button"
                        onClick={() => toggleMonth(o.value)}
                        aria-pressed={active}
                        className={[
                          'rounded-full px-3 py-1.5 text-sm font-medium transition-all duration-150 cursor-pointer focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-1',
                          active
                            ? 'bg-brand-600 text-white shadow-sm'
                            : 'bg-slate-100 text-slate-600 hover:bg-slate-200 hover:text-slate-800',
                        ].join(' ')}
                      >
                        {o.label}
                      </button>
                    );
                  })}
                </div>
                {errors.months && (
                  <p role="alert" className="mt-2 flex items-center gap-1.5 text-sm text-rose-600">
                    <AlertCircle className="h-3.5 w-3.5 shrink-0" />
                    {errors.months.message}
                  </p>
                )}
              </div>

              <label className="flex items-center gap-2.5 cursor-pointer select-none">
                <input
                  type="checkbox"
                  {...register('cumulativeTotal')}
                  className="h-4 w-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500 cursor-pointer"
                />
                <span className="text-sm text-slate-700">Adaugă pagină „Total cumulat" la final</span>
              </label>
            </div>
          ) : (
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <label className="flex flex-col gap-1.5">
                <span className="text-sm font-medium text-slate-700">De la</span>
                <input
                  type="date"
                  {...register('range.from')}
                  className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition"
                />
              </label>
              <label className="flex flex-col gap-1.5">
                <span className="text-sm font-medium text-slate-700">Până la</span>
                <input
                  type="date"
                  {...register('range.to')}
                  className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition"
                />
              </label>
              {errors.range && (
                <p role="alert" className="col-span-full flex items-center gap-1.5 text-sm text-rose-600">
                  <AlertCircle className="h-3.5 w-3.5 shrink-0" />
                  {errors.range.message}
                </p>
              )}
            </div>
          )}
        </Card>

        {/* ── 2. Blocuri de conținut ──────────────────────────────────────── */}
        <Card>
          <SectionTitle icon={<CheckSquare className="h-4 w-4" />} label="Ce să includă raportul" />
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            {BLOCKS.map(b => {
              const active = blocks.includes(b.value);
              return (
                <label
                  key={b.value}
                  className={[
                    'flex items-start gap-3 rounded-xl border p-4 cursor-pointer transition-all duration-150 select-none',
                    'focus-within:ring-2 focus-within:ring-brand-500 focus-within:ring-offset-1',
                    active
                      ? 'border-brand-300 bg-brand-50/60'
                      : 'border-slate-200 bg-white/40 hover:border-slate-300 hover:bg-white/70',
                  ].join(' ')}
                >
                  <input
                    type="checkbox"
                    checked={active}
                    onChange={() => toggleBlock(b.value)}
                    className="mt-0.5 h-4 w-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500 cursor-pointer shrink-0"
                  />
                  <div className="min-w-0">
                    <p className={`text-sm font-semibold ${active ? 'text-brand-700' : 'text-slate-800'}`}>
                      {b.label}
                    </p>
                    <p className="text-xs text-slate-500 leading-relaxed">{b.description}</p>
                  </div>
                </label>
              );
            })}
          </div>
          {errors.blocks && (
            <p role="alert" className="mt-3 flex items-center gap-1.5 text-sm text-rose-600">
              <AlertCircle className="h-3.5 w-3.5 shrink-0" />
              {errors.blocks.message}
            </p>
          )}
        </Card>

        {/* ── 3. Opțiuni avansate ─────────────────────────────────────────── */}
        <Card>
          <SectionTitle icon={<Settings2 className="h-4 w-4" />} label="Opțiuni avansate" />
          <div className="flex flex-wrap items-center gap-6">
            <label className="flex items-center gap-2.5 cursor-pointer select-none">
              <span className="text-sm font-medium text-slate-700">Granularitate grafic</span>
              <select
                {...register('granularity')}
                className="rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition cursor-pointer"
              >
                <option value="DAILY">Zilnic</option>
                <option value="WEEKLY">Săptămânal</option>
                <option value="MONTHLY">Lunar</option>
              </select>
            </label>

            <label className="flex items-center gap-2.5 cursor-pointer select-none">
              <input
                type="checkbox"
                {...register('runningBalanceInStatement')}
                className="h-4 w-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500 cursor-pointer"
              />
              <span className="text-sm text-slate-700">Sold cumulativ în extras</span>
            </label>
          </div>
        </Card>

        {/* ── Mesaj eroare ────────────────────────────────────────────────── */}
        {error && (
          <div
            role="alert"
            className="flex items-start gap-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
          >
            <AlertCircle className="h-4 w-4 shrink-0 mt-0.5" />
            <span>{error}</span>
          </div>
        )}

        {/* ── Buton submit ────────────────────────────────────────────────── */}
        <button
          type="submit"
          disabled={submitting}
          className={[
            'flex w-full items-center justify-center gap-2.5 rounded-xl px-6 py-3.5 text-sm font-semibold text-white',
            'transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2',
            submitting
              ? 'bg-brand-400 cursor-not-allowed opacity-70'
              : 'bg-brand-600 hover:bg-brand-700 shadow-sm hover:shadow-md cursor-pointer',
          ].join(' ')}
        >
          {submitting ? (
            <>
              <svg
                className="h-4 w-4 animate-spin"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
              </svg>
              Se generează…
            </>
          ) : (
            <>
              <FileDown className="h-4 w-4" />
              Generează PDF
            </>
          )}
        </button>

      </form>
    </div>
  );
};
