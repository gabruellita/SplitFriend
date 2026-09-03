// src/types/group.types.ts

// ─── Enums (string literals, identice cu BE Finance) ─────
export const GroupRole = {
  OWNER:  'OWNER',
  ADMIN:  'ADMIN',
  MEMBER: 'MEMBER',
} as const;
export type GroupRole = typeof GroupRole[keyof typeof GroupRole];

export const MemberStatus = {
  INVITED: 'INVITED',
  ACTIVE:  'ACTIVE',
  LEFT:    'LEFT',
  REMOVED: 'REMOVED',
} as const;
export type MemberStatus = typeof MemberStatus[keyof typeof MemberStatus];

export const GroupStatus = {
  ACTIVE:   'ACTIVE',
  ARCHIVED: 'ARCHIVED',
} as const;
export type GroupStatus = typeof GroupStatus[keyof typeof GroupStatus];

export const SplitType = {
  EQUAL:   'EQUAL',
  EXACT:   'EXACT',
  PERCENT: 'PERCENT',
  SHARES:  'SHARES',
} as const;
export type SplitType = typeof SplitType[keyof typeof SplitType];

export const ExpenseStatus = {
  OPEN:     'OPEN',
  SETTLED:  'SETTLED',
  CANCELED: 'CANCELED',
} as const;
export type ExpenseStatus = typeof ExpenseStatus[keyof typeof ExpenseStatus];

// ─── Grup ────────────────────────────────────────────────
export interface Group {
  id:           number;
  name:         string;
  description:  string | null;
  currencyId:   number;
  currencyCode: string | null;
  ownerUserId:  number;
  status:       GroupStatus;
  memberCount:  number;
  myRole:       GroupRole | null;
  createdAt:    string;
}

export interface GroupMember {
  userId:    number;
  email:     string | null;
  username:  string | null;
  firstName: string | null;
  lastName:  string | null;
  role:      GroupRole;
  status:    MemberStatus;
  joinedAt:  string | null;
}

export interface CreateGroupRequest {
  name:        string;
  description?: string | null;
  currencyId:  number;
}

export interface UpdateGroupRequest {
  // currencyId nu se poate modifica după creare (cheltuielile sunt deja în moneda grupului)
  name:        string;
  description?: string | null;
}

export interface InviteMemberRequest {
  email: string;
}

export interface InviteResponse {
  // string opac din backend (ex. user existent invitat vs. pending invitation) — doar pentru afișare
  outcome: string;
}

// ─── Cheltuieli + split ──────────────────────────────────
export interface ExpenseSplit {
  userId:     number;
  owedAmount: number;
  paidAmount: number;
  isSettled:  boolean;
}

export interface GroupExpense {
  id:           number;
  groupId:      number;
  paidByUserId: number;
  title:        string;
  amount:       number;
  currencyId:   number;
  currencyCode: string | null;
  splitType:    SplitType;
  status:       ExpenseStatus;
  expenseDate:  string;          // "YYYY-MM-DD"
  createdAt:    string;
  splits:       ExpenseSplit[];
}

/** Un participant în request-ul de creare. Câmpurile opționale depind de splitType. */
export interface ExpenseParticipantInput {
  userId:       number;
  exactAmount?: number | null;   // EXACT
  percent?:     number | null;   // PERCENT
  shares?:      number | null;   // SHARES
}

export interface CreateGroupExpenseRequest {
  title:        string;
  amount:       number;
  paidByUserId: number;
  splitType:    SplitType;
  expenseDate:  string;          // "YYYY-MM-DD"
  participants: ExpenseParticipantInput[];
}

// ─── Balanțe + plăți ─────────────────────────────────────
export interface GroupBalance {
  userId:       number;
  username:     string | null;
  currencyId:   number;          // moneda reală a sumei (a celui care a plătit / ancora datoriei)
  currencyCode: string | null;
  netAmount:    number;          // + ți se datorează, − datorezi
}

export interface Payment {
  id:                   number;
  fromUserId:           number;
  toUserId:             number;
  amount:               number;        // în moneda creditorului (currencyCode)
  currencyId:           number;
  currencyCode:         string | null;
  originalAmount:       number;        // cât a scos efectiv debitorul din buzunar
  originalCurrencyId:   number;
  originalCurrencyCode: string | null; // moneda debitorului
  exchangeRate:         number;
  rateDate:             string;        // "YYYY-MM-DD"
  paymentMethod:        string | null;
  paidAt:               string;
}

export interface CreatePaymentRequest {
  toUserId: number;
  amount:   number;
  method?:  string | null;
}
