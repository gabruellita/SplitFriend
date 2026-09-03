import { AxiosError } from 'axios';
import type { ApiError } from '@/types/api.types';

export interface ParsedError {
  message:     string;
  fieldErrors: Record<string, string>;
  statusCode:  number | null;
}

export const parseApiError = (error: unknown): ParsedError => {
  if (error instanceof AxiosError) {
    const statusCode = error.response?.status ?? null;
    const data       = error.response?.data as ApiError | undefined;

    if (data && 'errors' in data) {
      const fieldErrors: Record<string, string> = {};
      for (const [field, messages] of Object.entries(data.errors)) {
        fieldErrors[field.toLowerCase()] = messages[0] ?? '';
      }
      return { message: 'Validare eșuată.', fieldErrors, statusCode };
    }

    if (data && 'error' in data) {
      return { message: data.error, fieldErrors: {}, statusCode };
    }

    return {
      message:     error.message || 'Eroare necunoscută.',
      fieldErrors: {},
      statusCode,
    };
  }

  return {
    message:     'Eroare necunoscută.',
    fieldErrors: {},
    statusCode:  null,
  };
};
