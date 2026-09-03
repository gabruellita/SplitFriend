// src/api/chatHub.ts
import {
  HubConnectionBuilder, HubConnection, HttpTransportType, LogLevel,
} from '@microsoft/signalr';
import { API_ENDPOINTS } from './endpoints';
import { tokenStorage } from '@/utils/tokenStorage';
import type { ChatMessage, PresenceChanged } from '@/types/chat.types';

const BASE = import.meta.env.VITE_API_BASE_URL as string;

export interface ChatHubHandlers {
  onMessageReceived: (m: ChatMessage) => void;
  onMessageEdited:   (m: ChatMessage) => void;
  onMessageDeleted:  (m: ChatMessage) => void;
  onPresenceChanged: (p: PresenceChanged) => void;
}

/**
 * Normalizes a raw SignalR event payload to the camelCase ChatMessage shape.
 *
 * Why this exists: The SignalR JSON protocol may serialize event payloads with
 * PascalCase property names (e.g. SenderUserId, Content, IsDeleted) depending
 * on the server's JsonSerializerOptions. Since we cannot observe the live wire
 * format without a running Chat Service, we defensively check both casings so
 * the feature is correct regardless of the server's serializer setting.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const normalizeMessage = (raw: any): ChatMessage => ({
  id:               raw.id               ?? raw.Id               ?? 0,
  groupId:          raw.groupId          ?? raw.GroupId          ?? 0,
  senderUserId:     raw.senderUserId     ?? raw.SenderUserId     ?? 0,
  content:          raw.content          ?? raw.Content          ?? '',
  replyToMessageId: raw.replyToMessageId ?? raw.ReplyToMessageId ?? null,
  createdAt:        raw.createdAt        ?? raw.CreatedAt        ?? '',
  editedAt:         raw.editedAt         ?? raw.EditedAt         ?? null,
  isDeleted:        raw.isDeleted        ?? raw.IsDeleted        ?? false,
});

/**
 * Normalizes a raw SignalR PresenceChanged payload.
 * Same dual-casing rationale as normalizeMessage above.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const normalizePresence = (raw: any): PresenceChanged => ({
  userId: raw.userId ?? raw.UserId ?? 0,
  online: raw.online ?? raw.Online ?? false,
});

/** Creează (fără a porni) o conexiune la ChatHub pentru un grup. */
export const buildChatConnection = (groupId: number, handlers: ChatHubHandlers): HubConnection => {
  // groupId merge ca query-param; access_token e re-citit la fiecare (re)conectare prin accessTokenFactory.
  const url = `${BASE}${API_ENDPOINTS.CHAT.HUB}?groupId=${groupId}`;

  const connection = new HubConnectionBuilder()
    .withUrl(url, {
      accessTokenFactory: () => tokenStorage.getAccessToken() ?? '',
      transport: HttpTransportType.WebSockets,
      skipNegotiation: true,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  // Each handler wraps the raw payload through the normalizer before calling the
  // caller's handler, so the rest of the app always receives well-typed camelCase objects.
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  connection.on('MessageReceived', (raw: any) => handlers.onMessageReceived(normalizeMessage(raw)));
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  connection.on('MessageEdited',   (raw: any) => handlers.onMessageEdited(normalizeMessage(raw)));
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  connection.on('MessageDeleted',  (raw: any) => handlers.onMessageDeleted(normalizeMessage(raw)));
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  connection.on('PresenceChanged', (raw: any) => handlers.onPresenceChanged(normalizePresence(raw)));

  return connection;
};

// ─── Wrappere peste hub methods (semnături exacte din ChatHub.cs) ──
export const hubSend   = (c: HubConnection, content: string, replyToMessageId: number | null) =>
  c.invoke('SendMessage', { content, replyToMessageId });
export const hubEdit   = (c: HubConnection, messageId: number, content: string) =>
  c.invoke('EditMessage', messageId, content);
export const hubDelete = (c: HubConnection, messageId: number) =>
  c.invoke('DeleteMessage', messageId);
export const hubMarkRead = (c: HubConnection) =>
  c.invoke('MarkRead');
