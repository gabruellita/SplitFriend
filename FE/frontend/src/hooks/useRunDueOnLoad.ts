import { useEffect, useRef } from 'react';
import { financeApi } from '@/api/financeApi';

const SESSION_KEY = 'runDueDone';

/**
 * Ruleaza run-due o singura data pe sesiunea de tab (guard sessionStorage + useRef,
 * robust si la dublu-mount din StrictMode). Genereaza tranzactiile scadente acumulate
 * din sesiuni anterioare. Esecul e doar logat — nu blocheaza UI-ul.
 */
export const useRunDueOnLoad = (): void => {
  const ran = useRef(false);
  useEffect(() => {
    if (ran.current) return;
    if (sessionStorage.getItem(SESSION_KEY)) return;
    ran.current = true;
    sessionStorage.setItem(SESSION_KEY, '1');
    void financeApi.runDueTemplates().catch(err => {
      console.error('run-due la încărcare a eșuat:', err);
    });
  }, []);
};
