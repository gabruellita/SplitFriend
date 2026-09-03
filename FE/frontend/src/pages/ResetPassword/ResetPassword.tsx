import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { AlertTriangle } from 'lucide-react';

import { resetPasswordSchema, type ResetPasswordForm } from '@/schemas/profileSchema';
import { authApi } from '@/api/authApi';
import { parseApiError } from '@/utils/errorParser';

import { PasswordInput }         from '@/components/common/PasswordInput';
import { Button }                from '@/components/common/Button';
import { Alert }                 from '@/components/common/Alert';
import { PasswordStrengthMeter } from '@/components/auth/PasswordStrengthMeter';
import { AuthLayout }            from '@/components/auth/AuthLayout';

export const ResetPassword: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';

  const [serverError, setServerError] = useState<string | null>(null);

  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } =
    useForm<ResetPasswordForm>({
      resolver: zodResolver(resetPasswordSchema),
      mode:     'onBlur',
    });

  const passwordValue = watch('newPassword', '');

  const onSubmit = async (values: ResetPasswordForm) => {
    setServerError(null);
    try {
      await authApi.resetPassword({ token, newPassword: values.newPassword });
      navigate('/login', { state: { notice: 'Parola a fost resetată. Autentifică-te.' } });
    } catch (err) {
      const parsed = parseApiError(err);
      if (parsed.statusCode === 400) {
        setServerError('Token invalid sau expirat. Cere un nou link de resetare.');
      } else {
        setServerError(parsed.message || 'Eroare la resetarea parolei. Reîncearcă.');
      }
    }
  };

  // Fără token nu putem reseta nimic — afișăm un mesaj de eroare în loc de formular.
  if (!token) {
    return (
      <AuthLayout title="Link invalid">
        <div className="flex flex-col items-center text-center">
          <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-red-50">
            <AlertTriangle className="h-7 w-7 text-red-600" aria-hidden="true" />
          </div>
          <p className="text-sm text-gray-600">
            Link-ul de resetare lipsește sau este invalid. Cere unul nou.
          </p>
        </div>

        <p className="mt-6 text-sm text-center text-gray-600">
          <Link to="/forgot-password" className="text-blue-600 hover:underline">
            Cere un nou link de resetare
          </Link>
        </p>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="Setează o parolă nouă">
      {serverError && (
        <Alert type="error" message={serverError} onClose={() => setServerError(null)} />
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <PasswordInput
          label="Parolă nouă"
          autoComplete="new-password"
          {...register('newPassword')}
          error={errors.newPassword?.message}
        />

        <PasswordStrengthMeter password={passwordValue} />

        <PasswordInput
          label="Confirmă parola"
          autoComplete="new-password"
          {...register('confirm')}
          error={errors.confirm?.message}
        />

        <Button type="submit" loading={isSubmitting} fullWidth>
          Resetează parola
        </Button>
      </form>

      <p className="mt-4 text-sm text-center text-gray-600">
        <Link to="/login" className="text-blue-600 hover:underline">
          Înapoi la autentificare
        </Link>
      </p>
    </AuthLayout>
  );
};
