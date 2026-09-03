import { axiosClient } from './axiosClient';
import { API_ENDPOINTS } from './endpoints';
import type {
  Transaction,
  TransactionSummary,
  TransactionFilters,
  CreateTransactionRequest,
  UpdateTransactionRequest,
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  RecurringTemplate,
  CreateRecurringTemplateRequest,
  UpdateRecurringTemplateRequest,
  RunDueResponse,
} from '@/types/finance.types';

const {
  TRANSACTIONS, TRANSACTIONS_SUMMARY, CATEGORIES,
  RECURRING_TEMPLATES, RECURRING_TEMPLATES_RUN_DUE,
} = API_ENDPOINTS.FINANCE;

export const financeApi = {
  // ─── Tranzactii ────────────────────────────────────────
  getTransactions: async (filters: TransactionFilters = {}): Promise<Transaction[]> => {
    const { data } = await axiosClient.get<Transaction[]>(TRANSACTIONS, { params: filters });
    return data;
  },

  getSummary: async (from?: string, to?: string): Promise<TransactionSummary> => {
    const { data } = await axiosClient.get<TransactionSummary>(TRANSACTIONS_SUMMARY, {
      params: { from, to },
    });
    return data;
  },

  getTransaction: async (id: number): Promise<Transaction> => {
    const { data } = await axiosClient.get<Transaction>(`${TRANSACTIONS}/${id}`);
    return data;
  },

  createTransaction: async (body: CreateTransactionRequest): Promise<{ id: number }> => {
    const { data } = await axiosClient.post<{ id: number }>(TRANSACTIONS, body);
    return data;
  },

  updateTransaction: async (id: number, body: UpdateTransactionRequest): Promise<void> => {
    await axiosClient.put(`${TRANSACTIONS}/${id}`, body);
  },

  deleteTransaction: async (id: number): Promise<void> => {
    await axiosClient.delete(`${TRANSACTIONS}/${id}`);
  },

  // ─── Categorii ─────────────────────────────────────────
  getCategories: async (): Promise<Category[]> => {
    const { data } = await axiosClient.get<Category[]>(CATEGORIES);
    return data;
  },

  createCategory: async (body: CreateCategoryRequest): Promise<{ id: number }> => {
    const { data } = await axiosClient.post<{ id: number }>(CATEGORIES, body);
    return data;
  },

  updateCategory: async (id: number, body: UpdateCategoryRequest): Promise<void> => {
    await axiosClient.put(`${CATEGORIES}/${id}`, body);
  },

  deleteCategory: async (id: number): Promise<void> => {
    await axiosClient.delete(`${CATEGORIES}/${id}`);
  },

  // ─── Sabloane recurente ────────────────────────────────
  getRecurringTemplates: async (): Promise<RecurringTemplate[]> => {
    const { data } = await axiosClient.get<RecurringTemplate[]>(RECURRING_TEMPLATES);
    return data;
  },

  getRecurringTemplate: async (id: number): Promise<RecurringTemplate> => {
    const { data } = await axiosClient.get<RecurringTemplate>(`${RECURRING_TEMPLATES}/${id}`);
    return data;
  },

  createRecurringTemplate: async (body: CreateRecurringTemplateRequest): Promise<{ id: number }> => {
    const { data } = await axiosClient.post<{ id: number }>(RECURRING_TEMPLATES, body);
    return data;
  },

  updateRecurringTemplate: async (id: number, body: UpdateRecurringTemplateRequest): Promise<void> => {
    await axiosClient.put(`${RECURRING_TEMPLATES}/${id}`, body);
  },

  deactivateRecurringTemplate: async (id: number): Promise<void> => {
    await axiosClient.delete(`${RECURRING_TEMPLATES}/${id}`);
  },

  runDueTemplates: async (): Promise<RunDueResponse> => {
    const { data } = await axiosClient.post<RunDueResponse>(RECURRING_TEMPLATES_RUN_DUE);
    return data;
  },
};
