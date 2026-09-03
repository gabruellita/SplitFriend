import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { X } from 'lucide-react';

import { transactionSchema, type TransactionFormValues } from '@/schemas/transactionSchema';
import type {
  Transaction, TransactionKind, Category,
  CreateTransactionRequest, CreateRecurringTemplateRequest,
} from '@/types/finance.types';
import { toIsoDate } from '@/utils/format';

import { Input }  from '@/components/common/Input';
import { Button } from '@/components/common/Button';
import { Alert }  from '@/components/common/Alert';

interface TransactionFormModalProps {
  open:       boolean;
  kind:       TransactionKind;
  categories: Category[];
  initial?:   Transaction | null;
  onSubmit:          (body: CreateTransactionRequest) => Promise<void>;
  onSubmitRecurring: (body: CreateRecurringTemplateRequest) => Promise<void>;
  onClose:           () => void;
}

export const TransactionFormModal: React.FC<TransactionFormModalProps> = ({
  open,
  kind,
  categories,
  initial,
  onSubmit,
  onSubmitRecurring,
  onClose,
}) => {
  const isEdit = !!initial;

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setError,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<TransactionFormValues>({
    resolver: zodResolver(transactionSchema),
    mode: 'onBlur',
  });

  const isRecurring = watch('isRecurring');

  // Reincarca valorile cand se deschide modalul / se schimba tranzactia editata.
  useEffect(() => {
    if (!open) return;
    reset({
      amount:          initial?.amount ?? undefined,
      kind,
      transactionDate: initial?.transactionDate ?? toIsoDate(),
      categoryId:      initial?.categoryId ?? null,
      description:     initial?.description ?? '',
      isRecurring:     false,
      frequency:       'MONTHLY',
      intervalCount:   1,
      endDate:         '',
    });
  }, [open, initial, kind, reset]);

  if (!open) return null;

  const handleClose = () => {
    if (isDirty && !window.confirm('Ai modificări nesalvate. Închizi oricum?')) return;
    onClose();
  };

  const submit = async (values: TransactionFormValues) => {
    try {
      if (!isEdit && (values.isRecurring ?? false)) {
        await onSubmitRecurring({
          amount:        values.amount,
          kind,
          frequency:     values.frequency!,
          intervalCount: values.intervalCount ?? 1,
          startDate:     values.transactionDate,
          endDate:       values.endDate?.trim() || null,
          categoryId:    values.categoryId ?? null,
          description:   values.description?.trim() || null,
        });
      } else {
        await onSubmit({
          amount:          values.amount,
          kind,
          transactionDate: values.transactionDate,
          categoryId:      values.categoryId ?? null,
          description:     values.description?.trim() || null,
        });
      }
      onClose();
    } catch {
      setError('root', { message: 'Nu s-a putut salva. Reîncearcă.' });
    }
  };

  const title = `${isEdit ? 'Editează' : 'Adaugă'} ${kind === 'INCOME' ? 'venit' : 'cheltuială'}`;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/50" onClick={handleClose} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-label={title}
        className="relative w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
        <button type="button" onClick={handleClose} aria-label="Închide"
          className="absolute right-4 top-4 rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700">
          <X className="h-5 w-5" />
        </button>

        <h2 className="mb-4 text-lg font-semibold text-slate-900">{title}</h2>

        {errors.root && <Alert type="error" message={errors.root.message ?? ''} />}

        <form onSubmit={handleSubmit(submit)} noValidate>
          <Input
            label="Sumă"
            type="number"
            step="0.01"
            min="0"
            inputMode="decimal"
            {...register('amount', { valueAsNumber: true })}
            error={errors.amount?.message}
          />

          <Input
            label="Dată"
            type="date"
            {...register('transactionDate')}
            error={errors.transactionDate?.message}
          />

          <div className="mb-4 flex flex-col gap-1">
            <label htmlFor="tx-category" className="text-sm font-medium text-gray-700">Categorie</label>
            <select
              id="tx-category"
              className="rounded-md border border-gray-300 px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500"
              {...register('categoryId', {
                setValueAs: v => (v === '' || v == null ? null : Number(v)),
              })}
            >
              <option value="">Fără categorie</option>
              {categories.map(c => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <Input
            label="Descriere (opțional)"
            type="text"
            maxLength={500}
            {...register('description')}
            error={errors.description?.message}
          />

          {!isEdit && (
            <div className="mb-4 rounded-xl border border-slate-200 p-3">
              <label className="flex items-center gap-2 text-sm font-medium text-slate-700">
                <input type="checkbox" className="h-4 w-4 rounded border-gray-300"
                  {...register('isRecurring')} />
                Tranzacție recurentă
              </label>

              {isRecurring && (
                <div className="mt-3 space-y-3">
                  <div className="flex items-end gap-2">
                    <span className="pb-2 text-sm text-slate-600">Repetă la fiecare</span>
                    <div className="w-20">
                      <Input
                        label=""
                        type="number"
                        min="1"
                        step="1"
                        {...register('intervalCount', { valueAsNumber: true })}
                        error={errors.intervalCount?.message}
                      />
                    </div>
                    <div className="flex-1">
                      <label htmlFor="tx-frequency" className="sr-only">Frecvență</label>
                      <select
                        id="tx-frequency"
                        className="mb-4 w-full rounded-md border border-gray-300 px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500"
                        {...register('frequency')}
                      >
                        <option value="DAILY">zile</option>
                        <option value="WEEKLY">săptămâni</option>
                        <option value="MONTHLY">luni</option>
                        <option value="YEARLY">ani</option>
                      </select>
                    </div>
                  </div>
                  {errors.frequency && (
                    <span className="text-xs text-red-600">{errors.frequency.message}</span>
                  )}

                  <Input
                    label="Se termină la (opțional)"
                    type="date"
                    {...register('endDate')}
                    error={errors.endDate?.message}
                  />
                </div>
              )}
            </div>
          )}

          <div className="mt-2 flex justify-end gap-3">
            <Button type="button" variant="secondary" onClick={handleClose} disabled={isSubmitting}>
              Anulează
            </Button>
            <Button type="submit" loading={isSubmitting}>
              {isEdit ? 'Salvează' : 'Adaugă'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
