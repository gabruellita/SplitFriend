import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from '@/context/AuthContext';

import { Login }          from '@/pages/Login/Login';
import { Register }       from '@/pages/Register/Register';
import { ForgotPassword } from '@/pages/ForgotPassword/ForgotPassword';
import { ResetPassword }  from '@/pages/ResetPassword/ResetPassword';
import { ConfirmEmail }   from '@/pages/ConfirmEmail/ConfirmEmail';
import { NotFound }     from '@/pages/NotFound';

import { Overview }   from '@/pages/Overview/Overview';
import { Expenses }   from '@/pages/Expenses/Expenses';
import { Incomes }    from '@/pages/Incomes/Incomes';
import { Grafice }    from '@/pages/Grafice/Grafice';
import { Recurente }  from '@/pages/Recurente/Recurente';
import { Account }     from '@/pages/Account/Account';
import { GroupsList }  from '@/pages/Groups/GroupsList';
import { GroupDetail } from '@/pages/Groups/GroupDetail';
import { Export }      from '@/pages/Export/Export';

import { DashboardLayout } from '@/components/layout/DashboardLayout';
import { ProtectedRoute }  from '@/components/routing/ProtectedRoute';
import { PublicOnlyRoute } from '@/components/routing/PublicOnlyRoute';

export const App: React.FC = () => (
  <BrowserRouter>
    <AuthProvider>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />

        <Route path="/login" element={
          <PublicOnlyRoute><Login /></PublicOnlyRoute>
        } />
        <Route path="/register" element={
          <PublicOnlyRoute><Register /></PublicOnlyRoute>
        } />
        <Route path="/forgot-password" element={
          <PublicOnlyRoute><ForgotPassword /></PublicOnlyRoute>
        } />
        <Route path="/reset-password" element={
          <PublicOnlyRoute><ResetPassword /></PublicOnlyRoute>
        } />
        <Route path="/confirm-email" element={<ConfirmEmail />} />

        {/* Zona autentificata — layout cu sidebar */}
        <Route path="/app" element={
          <ProtectedRoute>
            <DashboardLayout />
          </ProtectedRoute>
        }>
          <Route index           element={<Overview />} />
          <Route path="expenses" element={<Expenses />} />
          <Route path="incomes"  element={<Incomes />} />
          <Route path="charts"     element={<Grafice />} />
          <Route path="recurring"  element={<Recurente />} />
          <Route path="groups"      element={<GroupsList />} />
          <Route path="groups/:id"  element={<GroupDetail />} />
          <Route path="export"      element={<Export />} />
          <Route path="account"  element={<Account />} />
        </Route>

        {/* Compatibilitate cu vechea ruta / redirect post-login */}
        <Route path="/dashboard" element={<Navigate to="/app" replace />} />

        <Route path="*" element={<NotFound />} />
      </Routes>
    </AuthProvider>
  </BrowserRouter>
);
