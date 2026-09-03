import { useState, useEffect, useCallback } from 'react';
import { financeApi } from '@/api/financeApi';
import type {
  Category,
  TransactionKind,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '@/types/finance.types';

interface UseCategoriesResult {
  categories:     Category[];
  isLoading:      boolean;
  error:          string | null;
  refetch:        () => Promise<void>;
  createCategory: (body: CreateCategoryRequest) => Promise<void>;
  updateCategory: (id: number, body: UpdateCategoryRequest) => Promise<void>;
  deleteCategory: (id: number) => Promise<void>;
}

/** `kind` optional → filtreaza local categoriile dupa tip (INCOME/EXPENSE). */
export const useCategories = (kind?: TransactionKind): UseCategoriesResult => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading]   = useState(true);
  const [error, setError]           = useState<string | null>(null);

  const fetchCategories = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await financeApi.getCategories();
      setCategories(kind ? data.filter(c => c.kind === kind) : data);
    } catch (err) {
      setError('Nu s-au putut încărca categoriile. Reîncearcă.');
      console.error('useCategories error:', err);
    } finally {
      setIsLoading(false);
    }
  }, [kind]);

  useEffect(() => {
    void fetchCategories();
  }, [fetchCategories]);

  const createCategory = useCallback(async (body: CreateCategoryRequest) => {
    await financeApi.createCategory(body);
    await fetchCategories();
  }, [fetchCategories]);

  const updateCategory = useCallback(async (id: number, body: UpdateCategoryRequest) => {
    await financeApi.updateCategory(id, body);
    await fetchCategories();
  }, [fetchCategories]);

  const deleteCategory = useCallback(async (id: number) => {
    await financeApi.deleteCategory(id);
    await fetchCategories();
  }, [fetchCategories]);

  return {
    categories,
    isLoading,
    error,
    refetch: fetchCategories,
    createCategory,
    updateCategory,
    deleteCategory,
  };
};
