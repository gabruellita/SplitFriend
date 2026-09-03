// src/components/groups/CreateGroupModal.tsx
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { X } from 'lucide-react';

import { groupSchema, type GroupFormValues } from '@/schemas/groupSchema';
import type { CreateGroupRequest } from '@/types/group.types';
import { useCurrencies } from '@/hooks/useCurrencies';
import { Input }  from '@/components/common/Input';
import { Button } from '@/components/common/Button';
import { Alert }  from '@/components/common/Alert';

interface CreateGroupModalProps {
  open:     boolean;
  onSubmit: (body: CreateGroupRequest) => Promise<void>;
  onClose:  () => void;
}

export const CreateGroupModal: React.FC<CreateGroupModalProps> = ({ open, onSubmit, onClose }) => {
  const { currencies, isLoading } = useCurrencies();
  const {
    register, handleSubmit, reset, setError, getValues, setValue,
    formState: { errors, isSubmitting },
  } = useForm<GroupFormValues>({ resolver: zodResolver(groupSchema), mode: 'onBlur' });

  // reset name/descriere DOAR la deschidere (nu la fiecare schimbare a listei de monede)
  useEffect(() => {
    if (!open) return;
    reset({ name: '', description: '', currencyId: undefined });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  // pune prima monedă ca default odată ce lista s-a încărcat, doar dacă nu e deja aleasă
  useEffect(() => {
    if (open && currencies.length > 0 && !getValues('currencyId')) {
      setValue('currencyId', currencies[0].id);
    }
  }, [open, currencies, getValues, setValue]);

  if (!open) return null;

  const submit = async (values: GroupFormValues) => {
    try {
      await onSubmit({
        name:        values.name.trim(),
        description: values.description?.trim() || null,
        currencyId:  values.currencyId,
      });
      onClose();
    } catch {
      setError('root', { message: 'Nu s-a putut crea grupul. Reîncearcă.' });
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/50" onClick={onClose} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-label="Grup nou"
        className="relative w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
        <button type="button" onClick={onClose} aria-label="Închide"
          className="absolute right-4 top-4 rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700">
          <X className="h-5 w-5" />
        </button>
        <h2 className="mb-4 text-lg font-semibold text-slate-900">Grup nou</h2>
        {errors.root && <Alert type="error" message={errors.root.message ?? ''} />}
        <form onSubmit={handleSubmit(submit)} noValidate>
          <Input label="Nume grup" {...register('name')} error={errors.name?.message} />
          <Input label="Descriere (opțional)" {...register('description')} error={errors.description?.message} />
          <div className="mb-4 flex flex-col gap-1">
            <label htmlFor="grp-currency" className="text-sm font-medium text-gray-700">Monedă</label>
            <select id="grp-currency"
              className="rounded-md border border-gray-300 px-3 py-2 outline-none transition focus:ring-2 focus:ring-blue-500"
              disabled={isLoading}
              {...register('currencyId', { setValueAs: v => (v === '' || v == null ? undefined : Number(v)) })}>
              {isLoading && <option value="">Se încarcă monedele…</option>}
              {currencies.map(c => <option key={c.id} value={c.id}>{c.code} — {c.name}</option>)}
            </select>
            {errors.currencyId && <span role="alert" className="text-xs text-red-600">{errors.currencyId.message}</span>}
          </div>
          <div className="mt-2 flex justify-end gap-3">
            <Button type="button" variant="secondary" onClick={onClose} disabled={isSubmitting}>Anulează</Button>
            <Button type="submit" loading={isSubmitting}>Creează</Button>
          </div>
        </form>
      </div>
    </div>
  );
};
