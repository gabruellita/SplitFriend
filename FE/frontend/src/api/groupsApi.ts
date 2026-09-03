// src/api/groupsApi.ts
import { axiosClient } from './axiosClient';
import { API_ENDPOINTS } from './endpoints';
import type {
  Group, GroupMember, CreateGroupRequest, UpdateGroupRequest,
  InviteMemberRequest, InviteResponse,
  GroupExpense, CreateGroupExpenseRequest,
  GroupBalance, Payment, CreatePaymentRequest,
} from '@/types/group.types';

const G = API_ENDPOINTS.GROUPS;

export const groupsApi = {
  // ─── Grupuri ───────────────────────────────────────────
  list: async (): Promise<Group[]> => {
    const { data } = await axiosClient.get<Group[]>(G.BASE);
    return data;
  },
  getById: async (id: number): Promise<Group> => {
    const { data } = await axiosClient.get<Group>(G.byId(id));
    return data;
  },
  create: async (body: CreateGroupRequest): Promise<{ id: number }> => {
    const { data } = await axiosClient.post<{ id: number }>(G.BASE, body);
    return data;
  },
  update: async (id: number, body: UpdateGroupRequest): Promise<void> => {
    await axiosClient.patch(G.byId(id), body);
  },
  archive: async (id: number): Promise<void> => {
    await axiosClient.delete(G.byId(id));
  },

  // ─── Membri / invitații ────────────────────────────────
  getMembers: async (id: number): Promise<GroupMember[]> => {
    const { data } = await axiosClient.get<GroupMember[]>(G.members(id));
    return data;
  },
  invite: async (id: number, body: InviteMemberRequest): Promise<InviteResponse> => {
    const { data } = await axiosClient.post<InviteResponse>(G.invite(id), body);
    return data;
  },
  accept: async (id: number): Promise<void> => {
    await axiosClient.post(G.accept(id));
  },
  leave: async (id: number): Promise<void> => {
    await axiosClient.post(G.leave(id));
  },

  // ─── Cheltuieli ────────────────────────────────────────
  getExpenses: async (id: number): Promise<GroupExpense[]> => {
    const { data } = await axiosClient.get<GroupExpense[]>(G.expenses(id));
    return data;
  },
  getExpense: async (id: number, expenseId: number): Promise<GroupExpense> => {
    const { data } = await axiosClient.get<GroupExpense>(G.expenseById(id, expenseId));
    return data;
  },
  createExpense: async (id: number, body: CreateGroupExpenseRequest): Promise<{ id: number }> => {
    const { data } = await axiosClient.post<{ id: number }>(G.expenses(id), body);
    return data;
  },
  cancelExpense: async (id: number, expenseId: number): Promise<void> => {
    await axiosClient.delete(G.expenseById(id, expenseId));
  },

  // ─── Balanțe / plăți ───────────────────────────────────
  getBalances: async (id: number): Promise<GroupBalance[]> => {
    const { data } = await axiosClient.get<GroupBalance[]>(G.balances(id));
    return data;
  },
  getPayments: async (id: number): Promise<Payment[]> => {
    const { data } = await axiosClient.get<Payment[]>(G.payments(id));
    return data;
  },
  createPayment: async (id: number, body: CreatePaymentRequest): Promise<{ id: number }> => {
    const { data } = await axiosClient.post<{ id: number }>(G.payments(id), body);
    return data;
  },
};
