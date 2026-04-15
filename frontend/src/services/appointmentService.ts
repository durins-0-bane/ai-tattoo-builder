import { apiRequest } from '@/services/apiClient'

export interface Appointment {
  id: string
  artistId: string
  customerUserId: string
  customerName: string
  customerEmail: string
  startTime: string
  durationMinutes: number
  serviceType: string
  status: string
}

export interface CreateAppointmentPayload {
  artistId: string
  startTimeUtc: string
  durationMinutes: number
  serviceType: string
}

export function fetchAppointments(token: string) {
  return apiRequest<Appointment[]>('/api/appointments/mine', { token })
}

export function createAppointment(token: string, payload: CreateAppointmentPayload) {
  return apiRequest<Appointment>('/api/appointments', {
    method: 'POST',
    token,
    body: payload,
  })
}
