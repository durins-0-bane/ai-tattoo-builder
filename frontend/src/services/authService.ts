const API_URL = import.meta.env.VITE_API_URL as string
const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID as string

export interface AppUser {
  id: string
  email: string
  displayName: string
  avatarUrl?: string
}

export interface TokenResponse {
  token: string
  user: AppUser
}

export function initGoogleSignIn(callback: (googleIdToken: string) => void): void {
  if (!window.google) {
    console.error('Google Identity Services not loaded.')
    return
  }
  window.google.accounts.id.initialize({
    client_id: GOOGLE_CLIENT_ID,
    callback: (response) => callback(response.credential),
    auto_select: false,
    cancel_on_tap_outside: true,
  })
}

export function renderGoogleButton(element: HTMLElement): void {
  window.google?.accounts.id.renderButton(element, {
    type: 'standard',
    shape: 'rectangular',
    theme: 'outline',
    text: 'sign_in_with',
    size: 'large',
    logo_alignment: 'left',
  })
}

export async function exchangeGoogleToken(googleIdToken: string): Promise<TokenResponse> {
  const response = await fetch(`${API_URL}/api/auth/google`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ idToken: googleIdToken }),
  })

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Authentication failed.' }))
    throw new Error(error.message ?? 'Authentication failed.')
  }

  return response.json() as Promise<TokenResponse>
}

export async function fetchCurrentUser(token: string): Promise<AppUser> {
  const response = await fetch(`${API_URL}/api/me`, {
    headers: { Authorization: `Bearer ${token}` },
  })

  if (!response.ok) throw new Error('Failed to fetch current user.')
  return response.json() as Promise<AppUser>
}

export function revokeGoogleSession(email: string): void {
  window.google?.accounts.id.revoke(email, () => {})
}
