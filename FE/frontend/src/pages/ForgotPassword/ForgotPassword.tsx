import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from 'react-router-dom';
import { MailCheck } from 'lucide-react';

import { forgotPasswordSchema, type ForgotPasswordForm } from '@/schemas/profileSchema';
import { authApi } from '@/api/authApi';

import { Input }      from '@/components/common/Input';
import { Button }     from '@/components/common/Button';
import { AuthLayout } from '@/components/auth/AuthLayout';

export const ForgotPassword: React.FC = () => {
  const [submitted, setSubmitted] = useState(false);

  const { register, handleSubmit, formState: { errors, isSubmitting } } =
    useForm<ForgotPasswordForm>({
      resolver: zodResolver(forgotPasswordSchema),
      mode:     'onBlur',
    });

  // Anti-enumerare: indiferent de succes sau eroare, afișăm același mesaj neutru,
  // ca să nu dezvăluim dacă email-ul există în sistem.
  const onSubmit = async (values: ForgotPasswordForm) => {
    try {
      await authApi.forgotPassword({ email: values.email.toLowerCase().trim() });
    } catch {
      // ignorăm intenționat eroarea — nu dezvăluim existența contului
    } finally {
      setSubmitted(true);
    }
  };

  if (submitted) {
    return (
      <AuthLayout title="Verifică-ți email-ul">
        <div className="flex flex-col items-center text-center">
          <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-blue-50">
            <MailCheck className="h-7 w-7 text-blue-600" aria-hidden="true" />
          </div>
          <p className="text-sm text-gray-600">
            Dacă există un cont cu acest email, vei primi un link de resetare.
          </p>
        </div>

        <p className="mt-6 text-sm text-center text-gray-600">
          <Link to="/login" className="text-blue-600 hover:underline">
            Înapoi la autentificare
          </Link>
        </p>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="Resetare parolă">
      <p className="mb-4 text-sm text-gray-600">
        Introdu adresa de email asociată contului tău și îți vom trimite un link de resetare.
      </p>

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <Input
          label="Email"
          type="email"
          autoComplete="email"
          {...register('email')}
          error={errors.email?.message}
        />

        <Button type="submit" loading={isSubmitting} fullWidth>
          Trimite link-ul de resetare
        </Button>
      </form>

      <p className="mt-4 text-sm text-center text-gray-600">
        Ți-ai amintit parola?{' '}
        <Link to="/login" className="text-blue-600 hover:underline">
          Autentifică-te
        </Link>
      </p>
    </AuthLayout>
  );
};
