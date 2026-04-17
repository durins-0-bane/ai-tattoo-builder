<template>
  <AppShell>
    <template #header-actions>
      <button class="ghost-button" @click="startFreshSession">New concept</button>
    </template>

    <div v-if="loading" class="panel empty-state">Loading your studio...</div>

    <div v-else class="chat-layout">
      <section class="panel sidebar-panel">
        <div>
          <p class="section-label">Artists</p>
          <div class="artist-list">
            <button
              v-for="artist in artists"
              :key="artist.id"
              class="artist-card"
              :class="{ active: selectedArtistId === artist.id }"
              :style="{ '--accent': artist.accentColor }"
              @click="selectedArtistId = artist.id"
            >
              <strong>{{ artist.displayName }}</strong>
              <span>{{ artist.specialties.join(' • ') }}</span>
            </button>
          </div>
        </div>

        <div>
          <p class="section-label">Sessions</p>
          <div class="session-list">
            <button
              v-for="session in sessions"
              :key="session.id"
              class="session-card"
              :class="{ active: activeSessionId === session.id }"
              @click="openSession(session)"
            >
              <strong>{{ session.title }}</strong>
              <span>{{ new Date(session.updatedAt).toLocaleString() }}</span>
            </button>
          </div>
        </div>
      </section>

      <section class="panel conversation-panel" :aria-busy="chatLoading">
        <div class="artist-hero" :style="{ '--accent': activeArtist?.accentColor ?? '#d7a86e' }">
          <div>
            <p class="section-label">Currently channeling</p>
            <h3>{{ activeArtist?.displayName ?? 'Choose an artist' }}</h3>
            <p>{{ activeArtist?.bio }}</p>
          </div>
          <div class="specialty-list">
            <span v-for="specialty in activeArtist?.specialties ?? []" :key="specialty">{{
              specialty
            }}</span>
          </div>
        </div>

        <div v-if="error" class="feedback error">{{ error }}</div>
        <div v-if="saveNotice" class="feedback success">{{ saveNotice }}</div>

        <div class="message-stack">
          <article
            v-for="(message, index) in messages"
            :key="messageKey(message, index)"
            class="message"
            :class="message.role"
          >
            <p class="message-role">
              {{ message.role === 'assistant' ? activeArtist?.displayName : 'You' }}
            </p>
            <p class="message-text">{{ message.content }}</p>

            <img
              v-if="message.imageUrl"
              :src="message.imageUrl"
              alt="Generated tattoo concept"
              class="message-image"
            />

            <button
              v-if="message.imageUrl"
              class="save-button"
              :disabled="savedKeys.includes(messageKey(message, index))"
              @click="saveGeneratedDesign(message, index)"
            >
              {{ savedKeys.includes(messageKey(message, index)) ? 'Saved' : 'Save to gallery' }}
            </button>
          </article>

          <div v-if="!messages.length && !chatLoading" class="empty-state tall">
            Start a conversation and ask one of the studio artists for a concept, a placement
            recommendation, or a booking suggestion.
          </div>

          <article v-if="chatLoading" class="message assistant thinking">
            <p class="message-role">{{ activeArtist?.displayName ?? 'Artist' }}</p>
            <LoadingIndicator text="Sketching" size="small" />
          </article>
        </div>

        <form class="composer" @submit.prevent="submitMessage">
          <textarea
            v-model="draft"
            rows="4"
            placeholder="Ask for a concept, refinement, placement note, or appointment idea..."
            :disabled="chatLoading"
          />
          <button
            class="primary-button"
            :disabled="chatLoading || !draft.trim() || !selectedArtistId"
            :aria-label="chatLoading ? 'AI is sketching your concept' : 'Send brief'"
          >
            <LoadingIndicator v-if="chatLoading" text="Sketching" size="small" />
            <span v-else>Send brief</span>
          </button>
        </form>
      </section>
    </div>
  </AppShell>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { mapState } from 'pinia'
import AppShell from '@/components/AppShell.vue'
import LoadingIndicator from '@/components/LoadingIndicator.vue'
import { useAuthStore } from '@/stores/auth'
import { fetchArtists, type ArtistProfile } from '@/services/artistService'
import {
  fetchChatMessages,
  fetchChatSessions,
  sendChatMessage,
  type ChatMessage,
  type ChatSession,
} from '@/services/chatService'
import { saveDesign } from '@/services/designService'

export default defineComponent({
  name: 'ChatView',
  components: { AppShell, LoadingIndicator },
  data() {
    return {
      artists: [] as ArtistProfile[],
      sessions: [] as ChatSession[],
      messages: [] as ChatMessage[],
      selectedArtistId: '',
      activeSessionId: null as string | null,
      draft: '',
      chatLoading: false,
      loading: true,
      error: null as string | null,
      saveNotice: null as string | null,
      savedKeys: [] as string[],
    }
  },
  computed: {
    ...mapState(useAuthStore, {
      token: (store) => store.token ?? '',
      user: 'user',
    }),
    activeArtist(): ArtistProfile | null {
      return (
        this.artists.find((artist) => artist.id === this.selectedArtistId) ??
        this.artists[0] ??
        null
      )
    },
  },
  mounted() {
    this.initialize()
  },
  methods: {
    messageKey(message: ChatMessage, index: number): string {
      return `${message.createdAt}-${index}`
    },

    async initialize() {
      if (!this.token) return

      this.loading = true
      this.error = null

      try {
        const [artistResults, sessionResults] = await Promise.all([
          fetchArtists(this.token),
          fetchChatSessions(this.token),
        ])

        this.artists = artistResults
        this.sessions = sessionResults

        if (sessionResults[0]) {
          this.activeSessionId = sessionResults[0].id
          this.selectedArtistId = sessionResults[0].artistId
          this.messages = await fetchChatMessages(this.token, sessionResults[0].id)
        } else if (artistResults[0]) {
          this.selectedArtistId = artistResults[0].id
        }
      } catch (err) {
        this.error = err instanceof Error ? err.message : 'Failed to load the studio workspace.'
      } finally {
        this.loading = false
      }
    },

    async openSession(session: ChatSession) {
      if (!this.token) return

      this.activeSessionId = session.id
      this.selectedArtistId = session.artistId
      this.messages = await fetchChatMessages(this.token, session.id)
    },

    startFreshSession() {
      this.activeSessionId = null
      this.messages = []
      this.saveNotice = null
    },

    async submitMessage() {
      if (!this.token || !this.draft.trim() || !this.selectedArtistId) return

      const userText = this.draft.trim()
      this.chatLoading = true
      this.error = null
      this.saveNotice = null

      const optimisticUser: ChatMessage = {
        id: crypto.randomUUID(),
        sessionId: this.activeSessionId ?? 'pending',
        userId: this.user?.id ?? 'current-user',
        role: 'user',
        content: userText,
        imageUrl: null,
        createdAt: new Date().toISOString(),
      }

      this.messages = [...this.messages, optimisticUser]
      this.draft = ''

      try {
        const reply = await sendChatMessage(this.token, {
          message: userText,
          artistId: this.selectedArtistId,
          sessionId: this.activeSessionId,
        })

        this.activeSessionId = reply.sessionId

        this.messages = [
          ...this.messages.map((message) =>
            message.id === optimisticUser.id ? { ...message, sessionId: reply.sessionId } : message,
          ),
          {
            id: crypto.randomUUID(),
            sessionId: reply.sessionId,
            userId: this.user?.id ?? 'current-user',
            role: 'assistant',
            content: reply.text,
            imageUrl: reply.imageUrl ?? null,
            createdAt: new Date().toISOString(),
          },
        ]
      } catch (err) {
        this.error = err instanceof Error ? err.message : 'The artist could not answer right now.'
      } finally {
        this.chatLoading = false
      }

      await this.refreshSessionsNonCritical()
    },

    async refreshSessionsNonCritical() {
      if (!this.token) return

      try {
        this.sessions = await fetchChatSessions(this.token)
      } catch (err) {
        console.warn('Session list refresh failed after sending message.', err)
      }
    },

    async saveGeneratedDesign(message: ChatMessage, index: number) {
      if (!this.token || !message.imageUrl) return

      const promptSource = [...this.messages]
        .slice(0, index)
        .reverse()
        .find((entry) => entry.role === 'user')

      try {
        await saveDesign(this.token, {
          imageUrl: message.imageUrl,
          prompt: promptSource?.content ?? message.content,
          refinedPrompt: promptSource?.content ?? message.content,
          style: this.activeArtist?.specialties[0] ?? 'Custom/Hybrid',
          artistId: this.selectedArtistId,
          sessionId: this.activeSessionId,
        })

        this.savedKeys = [...this.savedKeys, this.messageKey(message, index)]
        this.saveNotice = 'Design saved to your gallery.'
      } catch (err) {
        this.error = err instanceof Error ? err.message : 'Failed to save the design.'
      }
    },
  },
})
</script>

<style scoped>
.chat-layout {
  display: grid;
  grid-template-columns: 300px 1fr;
  gap: 1.25rem;
  height: 100%;
  min-height: 0;
  overflow: hidden;
}

.panel {
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.04);
  backdrop-filter: blur(14px);
}

.sidebar-panel {
  padding: 1.25rem;
  display: grid;
  gap: 1.5rem;
  align-content: start;
  overflow-y: auto;
}

.conversation-panel {
  padding: 1.25rem;
  display: grid;
  gap: 1rem;
  grid-template-rows: auto auto 1fr auto;
  overflow: hidden;
  min-height: 0;
}

.section-label {
  text-transform: uppercase;
  letter-spacing: 0.14em;
  font-size: 0.72rem;
  color: rgba(255, 255, 255, 0.5);
  margin-bottom: 0.75rem;
}

.artist-list,
.session-list {
  display: grid;
  gap: 0.75rem;
}

.artist-card,
.session-card {
  text-align: left;
  padding: 0.95rem;
  border-radius: 18px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.03);
  color: #fff;
  cursor: pointer;
  display: grid;
  gap: 0.35rem;
}

.artist-card.active,
.session-card.active {
  border-color: color-mix(in srgb, var(--accent, #d7a86e) 70%, white 30%);
  background: rgba(255, 255, 255, 0.08);
}

.artist-card span,
.session-card span,
.artist-hero p {
  color: rgba(255, 255, 255, 0.68);
}

.artist-hero {
  border-radius: 24px;
  padding: 1.15rem;
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.05),
    color-mix(in srgb, var(--accent) 24%, transparent 76%)
  );
  border: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  justify-content: space-between;
  gap: 1rem;
}

.artist-hero h3 {
  font-size: 1.45rem;
  margin-bottom: 0.5rem;
}

.specialty-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-content: flex-start;
}

.specialty-list span {
  padding: 0.45rem 0.75rem;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
}

.message-stack {
  min-height: 0;
  display: grid;
  gap: 0.9rem;
  align-content: start;
  overflow-y: auto;
}

.message {
  padding: 1rem;
  border-radius: 18px;
  max-width: 80%;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.message.user {
  justify-self: end;
  background: rgba(215, 168, 110, 0.16);
}

.message.assistant {
  justify-self: start;
  background: rgba(255, 255, 255, 0.05);
}

.message.thinking {
  opacity: 0.7;
}

textarea:disabled {
  opacity: 1;
  background: rgba(42, 46, 51, 0.96);
  border-color: rgba(210, 218, 226, 0.36);
  color: #d4dce4;
  cursor: not-allowed;
}

.primary-button:disabled {
  opacity: 1;
  background: linear-gradient(135deg, #7f8790, #626871);
  color: #f4f6f8;
  border: 1px solid rgba(244, 246, 248, 0.22);
  cursor: not-allowed;
}

.message-role {
  text-transform: uppercase;
  letter-spacing: 0.12em;
  font-size: 0.7rem;
  color: rgba(255, 255, 255, 0.5);
  margin-bottom: 0.45rem;
}

.message-text {
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
  overflow-wrap: break-word;
}

.message-image {
  width: min(360px, 100%);
  margin-top: 0.9rem;
  border-radius: 18px;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.composer {
  display: grid;
  gap: 0.85rem;
}

textarea {
  width: 100%;
  border-radius: 18px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(8, 8, 8, 0.7);
  color: #fff;
  padding: 1rem;
  resize: vertical;
}

.primary-button,
.ghost-button,
.save-button {
  border-radius: 14px;
  padding: 0.85rem 1rem;
  cursor: pointer;
  border: none;
}

.primary-button {
  background: linear-gradient(135deg, #d7a86e, #a86a35);
  color: #111;
  font-weight: 700;
}

.ghost-button,
.save-button {
  background: rgba(255, 255, 255, 0.06);
  color: #fff;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.save-button {
  margin-top: 0.8rem;
}

.feedback {
  padding: 0.85rem 1rem;
  border-radius: 14px;
}

.feedback.error {
  background: rgba(128, 28, 28, 0.22);
  border: 1px solid rgba(255, 114, 114, 0.4);
}

.feedback.success {
  background: rgba(54, 98, 52, 0.22);
  border: 1px solid rgba(120, 217, 116, 0.35);
}

.empty-state {
  display: grid;
  place-items: center;
  text-align: center;
  color: rgba(255, 255, 255, 0.6);
}

.empty-state.tall {
  min-height: 280px;
  border: 1px dashed rgba(255, 255, 255, 0.12);
  border-radius: 20px;
}

@media (max-width: 1200px) {
  .chat-layout {
    grid-template-columns: 1fr;
  }

  .message {
    max-width: 100%;
  }

  .artist-hero {
    flex-direction: column;
  }
}
</style>
