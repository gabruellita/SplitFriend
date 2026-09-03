import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { X } from 'lucide-react';

import type {
  RecurringTemplate, UpdateRecurringTemplateRequest,
} from '@/types/finance.types';
import { Input }  from '@/components/common/Input';
import { Button } from '@/components/common/Button';
import { Alert }  from '@/components/common/Alert';

interface FormValues {
  amount:        number;
  frequency:     'DAILY' | 'WEEKLY' | 'MONTHLY' | 'YEARLY';
  intervalCount: number;
  endDate:       string;
  description:   string;
}

interface Props {
  open:     boolean;
  template: RecurringTemplate | null;
  onSubmit: (id: number, body: UpdateRecurringTemplateRequest) => Promise<void>;
  onClose:  () => void;
}

export const RecurringTemplateModal: React.FC<Props> = ({ open, template, onSubmit, onClose }) => {
  const {
    register, handleSubmit, reset, setError,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>();

  useEffect(() => {
    if (!open || !template) return;
    reset({
      amount:        template.amount,
      frequency:     template.frequency,
      intervalCount: template.intervalCount,
      endDate:       template.endDate ?? '',
      description:   template.description ?? '',
    });
  }, [open, template, reset]);

  if (!open || !template) return null;

  const submit = async (values: FormValues) => {
    try {
      await onSubmit(template.id, {
        amount:        values.amount,
        kind:          template.kind,
        frequency:     values.frequency,
        intervalCount: values.intervalCount,
        endDate:       values.endDate?.trim() || null,
        categoryId:    template.categoryId,
        currencyId:    template.currencyId,
        description:   values.description?.trim() || null,
      });
      onClose();
    } catch {
      setError('root', { message: 'Nu s-a putut salva șablonul. Reîncearcă.' });
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/50" onClick={onClose} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-label="Editează șablon recurent"
        className="relative w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
        <button type="button" onClick={onClose} aria-label="Închide"
          className="absolute right-4 top-4 rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700">
          <X className="h-5 w-5" />
        </button>

        <h2 className="mb-4 text-lg font-semibold text-slate-900">Editează șablon recurent</h2>

        {errors.root && <Alert type="error" message={errors.root.message ?? ''} />}

        <form onSubmit={handleSubmit(submit)} noValidate>
          <Input label="Sumă" type="number" step="0.01" min="0" inputMode="decimal"
            {...register('amount', { valueAsNumber: true, min: 0.01 })}
            error={errors.amount ? 'Suma trebuie să fie mai mare ca 0.' : undefined} />

          <div className="flex items-end gap-2">
            <div className="w-20">
              <Input label="La fiecare" type="number" min="1" step="1"
                {...register('intervalCount', { valueAsNumber: true, min: 1 })}
                error={errors.intervalCount ? 'Minim 1.' : undefined} />
            </div>
            <div className="flex-1">
              <label htmlFor="rt-frequency" className="sr-only">Frecvență</label>
              <select id="rt-frequency"
                className="mb-4 w-full rounded-md border border-gray-300 px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500"
                {...register('frequency')}>
                <option value="DAILY">zile</option>
                <option value="WEEKLY">săptămâni</option>
                <option value="MONTHLY">luni</option>
                <option value="YEARLY">ani</option>
              </select>
            </div>
          </div>

          <Input label="Se termină la (opțional)" type="date" {...register('endDate')} />

          <Input label="Descriere (opțional)" type="text" maxLength={500} {...register('description')} />

          <div className="mt-2 flex justify-end gap-3">
            <Button type="button" variant="secondary" onClick={onClose} disabled={isSubmitting}>
              Anulează
            </Button>
            <Button type="submit" loading={isSubmitting}>Salvează</Button>
          </div>
        </form>
      </div>
    </div>
  );
};
