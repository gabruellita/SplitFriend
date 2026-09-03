// src/types/chat.types.ts

/** Oglindește ChatService.DTO.MessageResponse. */
export interface ChatMessage {
  id:               number;
  groupId:          number;
  senderUserId:     number;
  content:          string;        // "" când isDeleted
  replyToMessageId: number | null;
  createdAt:        string;
  editedAt:         string | null;
  isDeleted:        boolean;
}

/** Eveniment SignalR PresenceChanged. */
export interface PresenceChanged {
  userId: number;
  online: boolean;
}

/** Răspuns GET /unread → { "1": 3, "2": 0 } (chei = groupId ca string). */
export type UnreadCounts = Record<string, number>;
