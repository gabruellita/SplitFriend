import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate, useLocation } from 'react-router-dom';

import { loginSchema, type LoginFormValues } from '@/schemas/loginSchema';
import { useAuth } from '@/hooks/useAuth';
import { parseApiError } from '@/utils/errorParser';

import { Input }         from '@/components/common/Input';
import { PasswordInput } from '@/components/common/PasswordInput';
import { Button }        from '@/components/common/Button';
import { Alert }         from '@/components/common/Alert';
import { AuthLayout }    from '@/components/auth/AuthLayout';

export const Login: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const [serverError, setServerError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(
    (location.state as { notice?: string } | null)?.notice ?? null,
  );

  const { register, handleSubmit, formState: { errors, isSubmitting } } =
    useForm<LoginFormValues>({
      resolver: zodResolver(loginSchema),
      mode:     'onBlur',
    });

  const onSubmit = async (values: LoginFormValues) => {
    setServerError(null);
    try {
      await login({ ...values, email: values.email.toLowerCase().trim() });
      // Pastram si query string-ul (ex. ?invite=1 din deep-link-ul de invitatie),
      // nu doar pathname-ul — altfel accept-ul invitatiei nu se mai declanseaza dupa login.
      const from = (location.state as { from?: { pathname: string; search?: string } } | null)?.from;
      const redirectTo = from ? `${from.pathname}${from.search ?? ''}` : '/app';
      navigate(redirectTo, { replace: true });
    } catch (err) {
      const parsed = parseApiError(err);
      if (parsed.statusCode === 403) {
        setServerError('Contul nu este confirmat. Verifică email-ul pentru link-ul de activare.');
      } else if (parsed.statusCode === 401) {
        setServerError('Email sau parolă incorectă.');
      } else {
        setServerError(parsed.message || 'Eroare la autentificare. Reîncearcă.');
      }
    }
  };

  return (
    <AuthLayout title="Autentificare">
      {notice && (
        <Alert type="success" message={notice} onClose={() => setNotice(null)} />
      )}

      {serverError && (
        <Alert type="error" message={serverError} onClose={() => setServerError(null)} />
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <Input
          label="Email"
          type="email"
          autoComplete="email"
          {...register('email')}
          error={errors.email?.message}
        />

        <PasswordInput
          label="Parolă"
          autoComplete="current-password"
          {...register('password')}
          error={errors.password?.message}
        />

        <div className="mb-4 -mt-1 text-right text-sm">
          <Link to="/forgot-password" className="text-blue-600 hover:underline">
            Ai uitat parola?
          </Link>
        </div>

        <Button type="submit" loading={isSubmitting} fullWidth>
          Intră în cont
        </Button>
      </form>

      <p className="mt-4 text-sm text-center text-gray-600">
        Nu ai cont?{' '}
        <Link to="/register" className="text-blue-600 hover:underline">
          Înregistrează-te
        </Link>
      </p>
    </AuthLayout>
  );
};
