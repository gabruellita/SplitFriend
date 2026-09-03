import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';

import { registerSchema, type RegisterFormValues } from '@/schemas/registerSchema';
import { authApi } from '@/api/authApi';
import { parseApiError } from '@/utils/errorParser';

import { Input }                 from '@/components/common/Input';
import { PasswordInput }         from '@/components/common/PasswordInput';
import { Button }                from '@/components/common/Button';
import { Alert }                 from '@/components/common/Alert';
import { CurrencyDropdown }      from '@/components/auth/CurrencyDropdown';
import { PasswordStrengthMeter } from '@/components/auth/PasswordStrengthMeter';
import { AuthLayout }            from '@/components/auth/AuthLayout';

export const Register: React.FC = () => {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);
  const [successMsg,  setSuccessMsg]  = useState<string | null>(null);

  const {
    register, handleSubmit, watch, setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    mode:     'onBlur',
    defaultValues: { acceptTerms: false },
  });

  const passwordValue = watch('password', '');

  const onSubmit = async (values: RegisterFormValues) => {
    setServerError(null);
    setSuccessMsg(null);

    try {
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      const { confirmPassword: _cp, acceptTerms: _at, ...rest } = values;
      const payload = {
        ...rest,
        email:     rest.email.toLowerCase().trim(),
        username:  rest.username.trim(),
        firstName: rest.firstName?.trim() || undefined,
        lastName:  rest.lastName?.trim()  || undefined,
      };
      const response = await authApi.register(payload);
      setSuccessMsg(response.message);
      setTimeout(() => navigate('/login'), 3000);
    } catch (err) {
      const parsed = parseApiError(err);

      Object.entries(parsed.fieldErrors).forEach(([field, msg]) => {
        setError(field as keyof RegisterFormValues, { type: 'server', message: msg });
      });

      if (Object.keys(parsed.fieldErrors).length === 0) {
        setServerError(parsed.message || 'Eroare la înregistrare.');
      }
    }
  };

  if (successMsg) {
    return (
      <AuthLayout title="Cont creat!">
        <Alert type="success" message={successMsg} />
        <p className="text-sm text-gray-600">Vei fi redirecționat către pagina de login...</p>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="Înregistrare">
      {serverError && <Alert type="error" message={serverError} onClose={() => setServerError(null)} />}

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <Input
          label="Email"
          type="email"
          autoComplete="email"
          {...register('email')}
          error={errors.email?.message}
        />

        <Input
          label="Username"
          type="text"
          autoComplete="username"
          {...register('username')}
          error={errors.username?.message}
        />

        <div className="grid grid-cols-2 gap-3">
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

        <CurrencyDropdown
          label="Monedă preferată"
          {...register('preferredCurrencyId', { valueAsNumber: true })}
          error={errors.preferredCurrencyId?.message}
        />

        <PasswordInput
          label="Parolă"
          autoComplete="new-password"
          {...register('password')}
          error={errors.password?.message}
        />

        <PasswordStrengthMeter password={passwordValue} />

        <PasswordInput
          label="Confirmă parola"
          autoComplete="new-password"
          {...register('confirmPassword')}
          error={errors.confirmPassword?.message}
        />

        <label className="flex items-start gap-2 text-sm mb-4">
          <input type="checkbox" {...register('acceptTerms')} className="mt-1" />
          <span>
            Accept{' '}
            <a href="/terms" className="text-blue-600 hover:underline">
              termenii și condițiile
            </a>
            .
          </span>
        </label>
        {errors.acceptTerms && (
          <span role="alert" className="text-xs text-red-600 block -mt-3 mb-3">
            {errors.acceptTerms.message}
          </span>
        )}

        <Button type="submit" loading={isSubmitting} fullWidth>
          Creează cont
        </Button>
      </form>

      <p className="mt-4 text-sm text-center text-gray-600">
        Ai deja cont?{' '}
        <Link to="/login" className="text-blue-600 hover:underline">
          Autentifică-te
        </Link>
      </p>
    </AuthLayout>
  );
};
