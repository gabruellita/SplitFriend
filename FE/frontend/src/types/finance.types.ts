// ─── Enums (string literals, identice cu BE-ul Finance) ──
export const TransactionKind = {
  INCOME:  'INCOME',
  EXPENSE: 'EXPENSE',
} as const;
export type TransactionKind = typeof TransactionKind[keyof typeof TransactionKind];

export const TransactionStatus = {
  POSTED: 'POSTED',
  VOIDED: 'VOIDED',
} as const;
export type TransactionStatus = typeof TransactionStatus[keyof typeof TransactionStatus];

// ─── Categorii ───────────────────────────────────────────
export interface Category {
  id:       number;
  name:     string;
  kind:     TransactionKind;
  icon:     string | null;
  color:    string | null;
  isSystem: boolean;
  isActive: boolean;
}

export interface CreateCategoryRequest {
  name:   string;
  kind:   TransactionKind;
  icon?:  string | null;
  color?: string | null;
}

export interface UpdateCategoryRequest {
  name:   string;
  icon?:  string | null;
  color?: string | null;
}

// ─── Tranzactii ──────────────────────────────────────────
export interface Transaction {
  id:              number;
  amount:          number;
  kind:            TransactionKind;
  transactionDate: string;          // ISO date "YYYY-MM-DD"
  categoryId:      number | null;
  categoryName:    string | null;
  currencyId:      number;
  currencyCode:    string | null;
  description:     string | null;
  status:          TransactionStatus;
  templateId:      number | null;
  createdAt:       string;          // ISO datetime
}

export interface CreateTransactionRequest {
  amount:          number;
  kind:            TransactionKind;
  transactionDate: string;          // "YYYY-MM-DD"
  categoryId?:     number | null;
  currencyId?:     number | null;   // optional → BE foloseste moneda preferata
  description?:    string | null;
}

export type UpdateTransactionRequest = CreateTransactionRequest;

export interface TransactionFilters {
  from?:       string;              // "YYYY-MM-DD"
  to?:         string;              // "YYYY-MM-DD"
  categoryId?: number;
  kind?:       TransactionKind;
}

// ─── Sumar ───────────────────────────────────────────────
export interface CategoryBreakdown {
  kind:         TransactionKind;
  categoryId:   number | null;
  categoryName: string | null;
  total:        number;
  count:        number;
}

export interface TransactionSummary {
  totalIncome:  number;
  totalExpense: number;
  net:          number;
  byCategory:   CategoryBreakdown[];
}

// ─── Recurenta (sabloane) ────────────────────────────────
export const RecurrenceFrequency = {
  DAILY:   'DAILY',
  WEEKLY:  'WEEKLY',
  MONTHLY: 'MONTHLY',
  YEARLY:  'YEARLY',
} as const;
export type RecurrenceFrequency = typeof RecurrenceFrequency[keyof typeof RecurrenceFrequency];

export interface RecurringTemplate {
  id:            number;
  amount:        number;
  kind:          TransactionKind;
  frequency:     RecurrenceFrequency;
  intervalCount: number;
  startDate:     string;          // "YYYY-MM-DD"
  endDate:       string | null;
  nextRunDate:   string;
  isActive:      boolean;
  categoryId:    number | null;
  categoryName:  string | null;
  currencyId:    number;
  currencyCode:  string | null;
  description:   string | null;
}

export interface CreateRecurringTemplateRequest {
  amount:        number;
  kind:          TransactionKind;
  frequency:     RecurrenceFrequency;
  intervalCount: number;
  startDate:     string;          // "YYYY-MM-DD"
  endDate?:      string | null;
  categoryId?:   number | null;
  currencyId?:   number | null;
  description?:  string | null;
}

export interface UpdateRecurringTemplateRequest {
  amount:        number;
  kind:          TransactionKind;
  frequency:     RecurrenceFrequency;
  intervalCount: number;
  endDate?:      string | null;
  categoryId?:   number | null;
  currencyId?:   number | null;
  description?:  string | null;
}

export interface RunDueResponse {
  generatedCount: number;
}
