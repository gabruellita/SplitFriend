import { Link } from 'react-router-dom';

export const NotFound: React.FC = () => (
  <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center text-center px-4">
    <h1 className="text-6xl font-bold text-gray-800 mb-4">404</h1>
    <p className="text-xl text-gray-600 mb-6">Pagina nu a fost găsită.</p>
    <Link
      to="/login"
      className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
    >
      Înapoi la login
    </Link>
  </div>
);
