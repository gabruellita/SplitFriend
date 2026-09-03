// src/api/chatApi.ts
import { axiosClient } from './axiosClient';
import { API_ENDPOINTS } from './endpoints';
import type { ChatMessage, UnreadCounts } from '@/types/chat.types';

const C = API_ENDPOINTS.CHAT;

export const chatApi = {
  /** Istoric paginat descrescător. `before` = id-ul celui mai vechi mesaj deja încărcat. */
  getHistory: async (groupId: number, before?: number, limit = 50): Promise<ChatMessage[]> => {
    const { data } = await axiosClient.get<ChatMessage[]>(C.messages(groupId), { params: { before, limit } });
    return data;
  },
  getPresence: async (groupId: number): Promise<number[]> => {
    const { data } = await axiosClient.get<number[]>(C.presence(groupId));
    return data;
  },
  getUnread: async (groupIds: number[]): Promise<UnreadCounts> => {
    const { data } = await axiosClient.get<UnreadCounts>(C.UNREAD, { params: { groups: groupIds.join(',') } });
    return data;
  },
};
