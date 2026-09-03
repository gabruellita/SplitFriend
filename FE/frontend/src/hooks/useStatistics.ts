import { useCallback, useEffect, useState, type DependencyList } from 'react';

interface UseStatisticsResult<T> {
  data:      T | null;
  isLoading: boolean;
  error:     string | null;
  refetch:   () => Promise<void>;
}

/**
 * Hook generic de fetch pentru StatisticsService (pattern useSummary/useTransactions).
 * `fetcher` apeleaza statisticsApi; `deps` serializate ca o singura cheie stabila declanseaza
 * re-fetch la schimbarea filtrelor (from/to/kind/granularity/limit/buckets).
 */
export function useStatistics<T>(fetcher: () => Promise<T>, deps: DependencyList): UseStatisticsResult<T> {
  const [data, setData]           = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError]         = useState<string | null>(null);

  const depsKey = JSON.stringify(deps);

  const load = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      setData(await fetcher());
    } catch (err) {
      setError('Nu s-au putut încărca datele. Reîncearcă.');
      console.error('useStatistics error:', err);
    } finally {
      setIsLoading(false);
    }
    // re-creat doar cand depsKey se schimba; `fetcher` se schimba odata cu deps (intentionat omis)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [depsKey]);

  useEffect(() => {
    void load();
  }, [load]);

  return { data, isLoading, error, refetch: load };
}
