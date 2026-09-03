import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import clsx from 'clsx';
import { Menu, X } from 'lucide-react';
import { Sidebar } from './Sidebar';
import { useRunDueOnLoad } from '@/hooks/useRunDueOnLoad';

export const DashboardLayout: React.FC = () => {
  const [drawerOpen, setDrawerOpen] = useState(false);
  useRunDueOnLoad();
  const closeDrawer = () => setDrawerOpen(false);

  return (
    <div className="min-h-dvh bg-gradient-to-br from-slate-100 via-slate-50 to-slate-100">
      {/* Sidebar fix pe desktop (>=1024px) */}
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-64 lg:block">
        <div className="m-3 h-[calc(100dvh-1.5rem)] rounded-2xl glass-card">
          <Sidebar />
        </div>
      </aside>

      {/* Drawer pe mobil */}
      {drawerOpen && (
        <>
          <div
            className="fixed inset-0 z-40 bg-slate-900/50 lg:hidden"
            onClick={closeDrawer}
            aria-hidden="true"
          />
          <aside className="fixed inset-y-0 left-0 z-50 w-64 bg-white shadow-xl lg:hidden">
            <button
              type="button"
              onClick={closeDrawer}
              aria-label="Închide meniul"
              className="absolute right-3 top-3 rounded-lg p-1 text-slate-500 hover:bg-slate-100"
            >
              <X className="h-5 w-5" />
            </button>
            <Sidebar onNavigate={closeDrawer} />
          </aside>
        </>
      )}

      {/* Zona de continut */}
      <div className="lg:pl-64">
        {/* Topbar mobil cu hamburger */}
        <header className="sticky top-0 z-20 flex items-center gap-3 border-b border-slate-900/10 bg-white/70 px-4 py-3 backdrop-blur-md lg:hidden">
          <button
            type="button"
            onClick={() => setDrawerOpen(true)}
            aria-label="Deschide meniul"
            className="rounded-lg p-1 text-slate-700 hover:bg-slate-100"
          >
            <Menu className="h-6 w-6" />
          </button>
          <span className="font-semibold text-slate-900">FinanceApp</span>
        </header>

        <main className={clsx('mx-auto w-full max-w-6xl px-4 py-6 sm:px-6 lg:px-8 lg:py-8')}>
          <Outlet />
        </main>
      </div>
    </div>
  );
};
