import { NavLink } from 'react-router-dom';
import clsx from 'clsx';
import {
  LayoutDashboard,
  ArrowDownCircle,
  ArrowUpCircle,
  BarChart3,
  Users,
  UserCircle,
  LogOut,
  Wallet,
  Repeat,
  FileDown,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';

interface NavItem {
  to:    string;
  label: string;
  icon:  React.ComponentType<{ className?: string }>;
  end?:  boolean;
}

const NAV_ITEMS: NavItem[] = [
  { to: '/app',          label: 'Acasă',      icon: LayoutDashboard, end: true },
  { to: '/app/expenses', label: 'Cheltuieli', icon: ArrowDownCircle },
  { to: '/app/incomes',  label: 'Venituri',   icon: ArrowUpCircle },
  { to: '/app/charts',     label: 'Grafice',    icon: BarChart3 },
  { to: '/app/recurring',  label: 'Recurente',  icon: Repeat },
  { to: '/app/groups',     label: 'Grupuri',    icon: Users },
  { to: '/app/export',     label: 'Export PDF', icon: FileDown },
];

interface SidebarProps {
  /** Apelat la click pe un link — folosit pentru a inchide drawer-ul pe mobil. */
  onNavigate?: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ onNavigate }) => {
  const { user, logout } = useAuth();

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    clsx(
      'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition cursor-pointer',
      'focus:outline-none focus:ring-2 focus:ring-brand-500',
      isActive
        ? 'bg-brand-600 text-white shadow-sm'
        : 'text-slate-600 hover:bg-slate-900/5 hover:text-slate-900'
    );

  return (
    <nav className="flex h-full flex-col p-4" aria-label="Navigație principală">
      {/* Logo */}
      <div className="flex items-center gap-2 px-2 py-3 mb-4">
        <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-slate-900 text-white">
          <Wallet className="h-5 w-5" />
        </span>
        <span className="text-lg font-bold text-slate-900">FinanceApp</span>
      </div>

      {/* Linkuri principale */}
      <ul className="flex flex-col gap-1">
        {NAV_ITEMS.map(({ to, label, icon: Icon, end }) => (
          <li key={to}>
            <NavLink to={to} end={end} className={linkClass} onClick={onNavigate}>
              <Icon className="h-5 w-5 shrink-0" />
              <span>{label}</span>
            </NavLink>
          </li>
        ))}
      </ul>

      {/* Jos: cont + logout (separat vizual de navigatie) */}
      <div className="mt-auto flex flex-col gap-1 border-t border-slate-900/10 pt-3">
        <NavLink to="/app/account" className={linkClass} onClick={onNavigate}>
          <UserCircle className="h-5 w-5 shrink-0" />
          <span className="truncate">{user?.firstName ?? user?.username ?? 'Despre cont'}</span>
        </NavLink>
        <button
          type="button"
          onClick={() => { void logout(); }}
          className={clsx(
            'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition cursor-pointer',
            'text-rose-600 hover:bg-rose-50 focus:outline-none focus:ring-2 focus:ring-rose-400'
          )}
        >
          <LogOut className="h-5 w-5 shrink-0" />
          <span>Deconectare</span>
        </button>
      </div>
    </nav>
  );
};
