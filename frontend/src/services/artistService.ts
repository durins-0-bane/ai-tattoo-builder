import { apiRequest } from '@/services/apiClient'

export interface ArtistProfile {
  id: string
  slug: string
  displayName: string
  bio: string
  personaPrompt: string
  accentColor: string
  specialties: string[]
  portfolioImageUrls: string[]
  isActive: boolean
}

export function fetchArtists(token: string) {
  return apiRequest<ArtistProfile[]>('/api/artists', { token })
}
