import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';

import { useAuth } from '@/hooks/useAuth';
import { authApi } from '@/api/authApi';
import { parseApiError } from '@/utils/errorParser';
import { changePasswordSchema, type ChangePasswordForm } from '@/schemas/profileSchema';

import { PasswordInput } from '@/components/common/PasswordInput';
import { Button } from '@/components/common/Button';
import { Alert } from '@/components/common/Alert';
import { PasswordStrengthMeter } from '@/components/auth/PasswordStrengthMeter';

export const SecurityTab: React.FC = () => {
  const { logout } = useAuth();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register, handleSubmit, watch, setError,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordForm>({
    resolver: zodResolver(changePasswordSchema),
    mode:     'onBlur',
  });

  const newPasswordValue = watch('newPassword', '');

  const onSubmit = async (values: ChangePasswordForm) => {
    setServerError(null);

    try {
      await authApi.changePassword({
        currentPassword: values.currentPassword,
        newPassword:     values.newPassword,
      });

      // Serverul a revocat toate sesiunile → delogare + redirect spre login.
      await logout();
      navigate('/login', {
        state: { notice: 'Parola a fost schimbată. Autentifică-te din nou.' },
      });
    } catch (err) {
      const parsed = parseApiError(err);

      if (parsed.statusCode === 401) {
        setError('currentPassword', {
          type: 'server',
          message: 'Parola curentă este incorectă',
        });
        return;
      }

      Object.entries(parsed.fieldErrors).forEach(([field, msg]) => {
        setError(field as keyof ChangePasswordForm, { type: 'server', message: msg });
      });

      if (Object.keys(parsed.fieldErrors).length === 0) {
        setServerError(parsed.message || 'Nu am putut schimba parola.');
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate>
      {serverError && <Alert type="error" message={serverError} onClose={() => setServerError(null)} />}

      <PasswordInput
        label="Parola curentă"
        autoComplete="current-password"
        {...register('currentPassword')}
        error={errors.currentPassword?.message}
      />

      <PasswordInput
        label="Parolă nouă"
        autoComplete="new-password"
        {...register('newPassword')}
        error={errors.newPassword?.message}
      />

      <PasswordStrengthMeter password={newPasswordValue} />

      <PasswordInput
        label="Confirmă parola nouă"
        autoComplete="new-password"
        {...register('confirm')}
        error={errors.confirm?.message}
      />

      <p className="-mt-1 mb-4 text-xs text-slate-500">
        După schimbarea parolei vei fi delogat de pe toate dispozitivele.
      </p>

      <Button type="submit" fullWidth loading={isSubmitting}>
        Schimbă parola
      </Button>
    </form>
  );
};
