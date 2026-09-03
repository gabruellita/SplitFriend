import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import { useAuth } from '@/hooks/useAuth';
import { useCurrencies } from '@/hooks/useCurrencies';
import { authApi } from '@/api/authApi';
import { parseApiError } from '@/utils/errorParser';
import { profileSchema, type ProfileForm } from '@/schemas/profileSchema';
import type { MeResponse } from '@/types/auth.types';

import { Input } from '@/components/common/Input';
import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { CurrencyDropdown } from '@/components/auth/CurrencyDropdown';

interface ProfileTabProps {
  me: MeResponse;
}

export const ProfileTab: React.FC<ProfileTabProps> = ({ me }) => {
  const { updateUser, refreshSession } = useAuth();
  const { currencies } = useCurrencies();
  const current = currencies.find(c => c.id === me.preferredCurrencyId);
  const currentLabel = current
    ? `${current.symbol} — ${current.name} (${current.code})`
    : me.preferredCurrencyCode ?? null;
  const [serverError, setServerError] = useState<string | null>(null);
  const [successMsg,  setSuccessMsg]  = useState<string | null>(null);

  const {
    register, handleSubmit, setError,
    formState: { errors, isSubmitting },
  } = useForm<ProfileForm>({
    resolver: zodResolver(profileSchema),
    mode:     'onBlur',
    defaultValues: {
      firstName:           me.firstName ?? '',
      lastName:            me.lastName ?? '',
      preferredCurrencyId: me.preferredCurrencyId,
    },
  });

  const onSubmit = async (values: ProfileForm) => {
    setServerError(null);
    setSuccessMsg(null);

    try {
      const updated = await authApi.updateProfile({
        firstName:           values.firstName || null,
        lastName:            values.lastName || null,
        preferredCurrencyId: values.preferredCurrencyId,
      });

      // JWT-ul conține claim-ul `currency` → reîmprospătează sesiunea ca
      // Finance/Statistics să vadă moneda nouă.
      await refreshSession();

      updateUser({
        firstName:           updated.firstName ?? undefined,
        lastName:            updated.lastName ?? undefined,
        preferredCurrencyId: updated.preferredCurrencyId,
      });

      setSuccessMsg('Profil actualizat');
    } catch (err) {
      const parsed = parseApiError(err);

      Object.entries(parsed.fieldErrors).forEach(([field, msg]) => {
        setError(field as keyof ProfileForm, { type: 'server', message: msg });
      });

      if (Object.keys(parsed.fieldErrors).length === 0) {
        setServerError(parsed.message || 'Nu am putut salva profilul.');
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate>
      {successMsg  && <Alert type="success" message={successMsg}  onClose={() => setSuccessMsg(null)} />}
      {serverError && <Alert type="error"   message={serverError} onClose={() => setServerError(null)} />}

      <Input
        label="Email"
        type="email"
        value={me.email}
        readOnly
        disabled
        helper="Adresa de email nu poate fi modificată."
      />

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Input
          label="Prenume"
          type="text"
          autoComplete="given-name"
          {...register('firstName')}
          error={errors.firstName?.message}
        />
        <Input
          label="Nume"
          type="text"
          autoComplete="family-name"
          {...register('lastName')}
          error={errors.lastName?.message}
        />
      </div>

      <div className="mb-4">
        {currentLabel && (
          <p className="mb-1 text-sm text-gray-600">
            Moneda actuală: <span className="font-medium text-gray-900">{currentLabel}</span>
          </p>
        )}
        <CurrencyDropdown
          label="Monedă preferată"
          {...register('preferredCurrencyId', { valueAsNumber: true })}
          error={errors.preferredCurrencyId?.message}
        />
      </div>

      <Button type="submit" fullWidth loading={isSubmitting}>
        Salvează modificările
      </Button>
    </form>
  );
};
