interface AuthLayoutProps {
  title:    string;
  children: React.ReactNode;
}

export const AuthLayout: React.FC<AuthLayoutProps> = ({ title, children }) => (
  <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
    <div className="w-full max-w-md bg-white rounded-xl shadow-md p-8">
      <h1 className="text-2xl font-bold text-gray-800 mb-6 text-center">{title}</h1>
      {children}
    </div>
  </div>
);
