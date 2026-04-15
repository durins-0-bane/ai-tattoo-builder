// Minimal ambient types for Google Identity Services (GIS)
interface GoogleCredentialResponse {
  credential: string
  select_by: string
  clientId: string
}

interface GoogleAccountsId {
  initialize: (config: {
    client_id: string
    callback: (response: GoogleCredentialResponse) => void
    auto_select?: boolean
    cancel_on_tap_outside?: boolean
  }) => void
  renderButton: (parent: HTMLElement, options: object) => void
  prompt: () => void
  revoke: (hint: string, callback: () => void) => void
  disableAutoSelect: () => void
}

declare global {
  interface Window {
    google?: {
      accounts: {
        id: GoogleAccountsId
      }
    }
  }
}

export {}
