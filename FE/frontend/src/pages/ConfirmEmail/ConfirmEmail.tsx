import { useEffect, useRef, useState } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { authApi } from '@/api/authApi';
import { Alert }       from '@/components/common/Alert';
import { Spinner }     from '@/components/common/Spinner';
import { AuthLayout }  from '@/components/auth/AuthLayout';

type Status = 'loading' | 'success' | 'error';

export const ConfirmEmail: React.FC = () => {
  const [params]  = useSearchParams();
  const navigate  = useNavigate();
  const [status,  setStatus]  = useState<Status>('loading');
  const [message, setMessage] = useState<string>('');
  // Token-ul de confirmare e single-use; previne dublul apel din StrictMode (dev),
  // altfel al doilea apel esueaza (token deja consumat) si suprascrie succesul.
  const confirmedRef = useRef(false);

  useEffect(() => {
    if (confirmedRef.current) return;
    confirmedRef.current = true;

    const token = params.get('token');
    if (!token) {
      setStatus('error');
      setMessage('Token lipsă din URL.');
      return;
    }

    authApi
      .confirmEmail({ token })
      .then(res => {
        setStatus('success');
        setMessage(res.message);
        setTimeout(() => navigate('/login'), 3000);
      })
      .catch(() => {
        setStatus('error');
        setMessage('Token invalid sau expirat. Cere un nou link de confirmare.');
      });
  }, [params, navigate]);

  return (
    <AuthLayout title="Confirmare email">
      {status === 'loading' && <Spinner />}
      {status === 'success' && (
        <>
          <Alert type="success" message={message} />
          <p className="text-sm text-gray-600">Vei fi redirecționat către login...</p>
        </>
      )}
      {status === 'error' && (
        <>
          <Alert type="error" message={message} />
          <Link to="/login" className="text-blue-600 hover:underline text-sm">
            Înapoi la login
          </Link>
        </>
      )}
    </AuthLayout>
  );
};
