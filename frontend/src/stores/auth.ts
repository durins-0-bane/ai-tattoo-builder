import { defineStore } from 'pinia'
import {
  initGoogleSignIn,
  renderGoogleButton,
  exchangeGoogleToken,
  fetchCurrentUser,
  revokeGoogleSession,
  type AppUser,
} from '@/services/authService'

const TOKEN_KEY = 'app_token'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: sessionStorage.getItem(TOKEN_KEY) as string | null,
    user: null as AppUser | null,
    loading: false,
    error: null as string | null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.token && !!state.user,
  },

  actions: {
    setupGoogleButton(element: HTMLElement) {
      initGoogleSignIn(async (googleIdToken: string) => {
        this.loading = true
        this.error = null
        try {
          const result = await exchangeGoogleToken(googleIdToken)
          this.token = result.token
          this.user = result.user
          sessionStorage.setItem(TOKEN_KEY, result.token)
        } catch (err) {
          this.error = err instanceof Error ? err.message : 'Sign in failed.'
        } finally {
          this.loading = false
        }
      })

      renderGoogleButton(element)
    },

    async restoreSession() {
      const stored = sessionStorage.getItem(TOKEN_KEY)
      if (!stored) return
      try {
        this.user = await fetchCurrentUser(stored)
        this.token = stored
      } catch {
        sessionStorage.removeItem(TOKEN_KEY)
        this.token = null
        this.user = null
      }
    },

    logout() {
      if (this.user?.email) {
        revokeGoogleSession(this.user.email)
      }
      this.token = null
      this.user = null
      sessionStorage.removeItem(TOKEN_KEY)
    },
  },
})
