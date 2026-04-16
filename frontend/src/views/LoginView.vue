<template>
  <div class="login-view">
    <div class="login-card">
      <h1>AI Tattoo Builder</h1>
      <p class="subtitle">Design your next tattoo with AI-powered artists</p>

      <div v-if="error" class="error-message">
        {{ error }}
      </div>

      <div v-if="loading || redirecting" aria-live="polite">
        <LoadingIndicator text="Signing you in" />
      </div>

      <div v-else>
        <GoogleSignInButton />
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { mapState } from 'pinia'
import { useAuthStore } from '@/stores/auth'
import GoogleSignInButton from '@/components/GoogleSignInButton.vue'
import LoadingIndicator from '@/components/LoadingIndicator.vue'

export default defineComponent({
  name: 'LoginView',
  components: { GoogleSignInButton, LoadingIndicator },
  data() {
    return {
      redirecting: false,
    }
  },
  computed: {
    ...mapState(useAuthStore, ['isAuthenticated', 'loading', 'error']),
  },
  watch: {
    async isAuthenticated(authenticated: boolean) {
      if (authenticated) {
        this.redirecting = true
        try {
          await this.$router.replace({ name: 'chat' })
        } catch {
          this.redirecting = false
        }
      }
    },
  },
  methods: {},
})
</script>

<style scoped>
.login-view {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  background: #0f0f0f;
}

.login-card {
  background: #1a1a1a;
  border: 1px solid #333;
  border-radius: 12px;
  padding: 3rem 2.5rem;
  text-align: center;
  max-width: 380px;
  width: 100%;
}

h1 {
  color: #fff;
  font-size: 1.75rem;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: #888;
  margin-bottom: 2rem;
  font-size: 0.95rem;
}

.google-btn-container {
  display: flex;
  justify-content: center;
}

.error-message {
  background: #2d1515;
  border: 1px solid #7f1d1d;
  color: #fca5a5;
  border-radius: 6px;
  padding: 0.75rem 1rem;
  margin-bottom: 1.25rem;
  font-size: 0.875rem;
}


</style>
