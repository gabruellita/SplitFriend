import { axiosClient } from './axiosClient';
import { API_ENDPOINTS } from './endpoints';
import type { Currency, ConvertResult } from '@/types/currency.types';

export const currencyApi = {
  getAllActive: async (): Promise<Currency[]> => {
    const { data } = await axiosClient.get<Currency[]>(API_ENDPOINTS.CURRENCIES);
    return data;
  },

  convert: async (from: string, to: string, amount: number): Promise<ConvertResult> => {
    const { data } = await axiosClient.get<ConvertResult>(API_ENDPOINTS.CURRENCY.CONVERT, {
      params: { from, to, amount },
    });
    return data;
  },
};
