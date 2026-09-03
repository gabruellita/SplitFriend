import { useState, useEffect } from 'react';
import { currencyApi } from '@/api/currencyApi';
import type { Currency } from '@/types/currency.types';

interface UseCurrenciesResult {
  currencies: Currency[];
  isLoading:  boolean;
  error:      string | null;
  refetch:    () => Promise<void>;
}

export const useCurrencies = (): UseCurrenciesResult => {
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [isLoading, setIsLoading]   = useState(true);
  const [error, setError]           = useState<string | null>(null);

  const fetchCurrencies = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await currencyApi.getAllActive();
      setCurrencies(data);
    } catch (err) {
      setError('Nu s-au putut încărca monedele. Reîncearcă.');
      console.error('useCurrencies error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void fetchCurrencies();
  }, []);

  return { currencies, isLoading, error, refetch: fetchCurrencies };
};
