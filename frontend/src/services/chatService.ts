import { apiRequest } from '@/services/apiClient'

export interface ChatSession {
  id: string
  userId: string
  artistId: string
  title: string
  createdAt: string
  updatedAt: string
}

export interface ChatMessage {
  id: string
  sessionId: string
  userId: string
  role: 'user' | 'assistant'
  content: string
  imageUrl?: string | null
  createdAt: string
}

export interface ChatReply {
  sessionId: string
  text: string
  imageUrl?: string | null
}

export function fetchChatSessions(token: string) {
  return apiRequest<ChatSession[]>('/api/agent/sessions', { token })
}

export function fetchChatMessages(token: string, sessionId: string) {
  return apiRequest<ChatMessage[]>(`/api/agent/sessions/${sessionId}/messages`, { token })
}

export function sendChatMessage(
  token: string,
  payload: { message: string; artistId: string; sessionId?: string | null },
) {
  return apiRequest<ChatReply>('/api/agent/chat', {
    method: 'POST',
    token,
    body: payload,
  })
}
