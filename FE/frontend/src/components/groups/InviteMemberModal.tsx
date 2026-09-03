// src/components/groups/InviteMemberModal.tsx
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X } from 'lucide-react';
import { parseApiError } from '@/utils/errorParser';
import { Input }  from '@/components/common/Input';
import { Button } from '@/components/common/Button';
import { Alert }  from '@/components/common/Alert';

const inviteSchema = z.object({ email: z.string().trim().email('Email invalid') });
type InviteValues = z.infer<typeof inviteSchema>;

interface InviteMemberModalProps {
  open:     boolean;
  onSubmit: (email: string) => Promise<string>;   // întoarce outcome-ul
  onClose:  () => void;
}

export const InviteMemberModal: React.FC<InviteMemberModalProps> = ({ open, onSubmit, onClose }) => {
  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } =
    useForm<InviteValues>({ resolver: zodResolver(inviteSchema), mode: 'onBlur' });

  useEffect(() => { if (open) reset({ email: '' }); }, [open, reset]);
  if (!open) return null;

  const submit = async (values: InviteValues) => {
    try {
      await onSubmit(values.email.trim());
      onClose();
    } catch (err) {
      const parsed = parseApiError(err);
      setError('root', { message: parsed.message || 'Nu s-a putut trimite invitația. Reîncearcă.' });
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/50" onClick={onClose} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-label="Invită membru"
        className="relative w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
        <button type="button" onClick={onClose} aria-label="Închide"
          className="absolute right-4 top-4 rounded-lg p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700">
          <X className="h-5 w-5" />
        </button>
        <h2 className="mb-1 text-lg font-semibold text-slate-900">Invită un membru</h2>
        <p className="mb-4 text-sm text-slate-500">
          Dacă persoana are deja cont, intră ca invitat; altfel primește un email de înregistrare.
        </p>
        {errors.root && <Alert type="error" message={errors.root.message ?? ''} />}
        <form onSubmit={handleSubmit(submit)} noValidate>
          <Input label="Email" type="email" autoComplete="email"
            {...register('email')} error={errors.email?.message} />
          <div className="mt-2 flex justify-end gap-3">
            <Button type="button" variant="secondary" onClick={onClose} disabled={isSubmitting}>Anulează</Button>
            <Button type="submit" loading={isSubmitting}>Trimite invitația</Button>
          </div>
        </form>
      </div>
    </div>
  );
};
