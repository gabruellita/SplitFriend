import { Construction } from 'lucide-react';

interface ComingSoonProps {
  title: string;
}

export const ComingSoon: React.FC<ComingSoonProps> = ({ title }) => (
  <div className="flex flex-col items-center justify-center rounded-2xl glass-card px-6 py-20 text-center">
    <span className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-slate-900 text-white">
      <Construction className="h-7 w-7" />
    </span>
    <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
    <p className="mt-2 max-w-sm text-sm text-slate-500">
      Această secțiune este în curs de dezvoltare și va fi disponibilă în curând.
    </p>
  </div>
);
