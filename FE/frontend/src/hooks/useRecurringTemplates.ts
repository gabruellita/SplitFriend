import { useState, useEffect, useCallback } from 'react';
import { financeApi } from '@/api/financeApi';
import type {
  RecurringTemplate,
  UpdateRecurringTemplateRequest,
} from '@/types/finance.types';

interface UseRecurringTemplatesResult {
  templates:   RecurringTemplate[];
  isLoading:   boolean;
  error:       string | null;
  refetch:     () => Promise<void>;
  update:      (id: number, body: UpdateRecurringTemplateRequest) => Promise<void>;
  deactivate:  (id: number) => Promise<void>;
  runDue:      () => Promise<number>;
}

export const useRecurringTemplates = (): UseRecurringTemplatesResult => {
  const [templates, setTemplates] = useState<RecurringTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError]         = useState<string | null>(null);

  const fetchTemplates = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await financeApi.getRecurringTemplates();
      setTemplates(data);
    } catch (err) {
      setError('Nu s-au putut încărca șabloanele recurente. Reîncearcă.');
      console.error('useRecurringTemplates error:', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => { void fetchTemplates(); }, [fetchTemplates]);

  const update = useCallback(async (id: number, body: UpdateRecurringTemplateRequest) => {
    await financeApi.updateRecurringTemplate(id, body);
    await fetchTemplates();
  }, [fetchTemplates]);

  const deactivate = useCallback(async (id: number) => {
    await financeApi.deactivateRecurringTemplate(id);
    await fetchTemplates();
  }, [fetchTemplates]);

  const runDue = useCallback(async () => {
    const res = await financeApi.runDueTemplates();
    await fetchTemplates();
    return res.generatedCount;
  }, [fetchTemplates]);

  return { templates, isLoading, error, refetch: fetchTemplates, update, deactivate, runDue };
};
