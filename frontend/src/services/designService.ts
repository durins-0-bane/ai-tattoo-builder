import { apiRequest } from '@/services/apiClient'

export interface TattooDesign {
  id: string
  customerId: string
  imageUrl: string
  prompt: string
  refinedPrompt: string
  style: string
  artistId?: string | null
  sessionId?: string | null
  createdAt: string
  isFavorite: boolean
}

export interface SaveTattooDesignPayload {
  imageUrl: string
  prompt: string
  refinedPrompt: string
  style: string
  artistId?: string | null
  sessionId?: string | null
}

export function fetchDesigns(token: string) {
  return apiRequest<TattooDesign[]>('/api/tattoodesigns/mine', { token })
}

export function saveDesign(token: string, payload: SaveTattooDesignPayload) {
  return apiRequest<TattooDesign>('/api/tattoodesigns', {
    method: 'POST',
    token,
    body: payload,
  })
}

export function toggleFavorite(token: string, designId: string) {
  return apiRequest<TattooDesign>(`/api/tattoodesigns/${designId}/favorite`, {
    method: 'PATCH',
    token,
  })
}
