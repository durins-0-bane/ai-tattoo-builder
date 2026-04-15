<template>
  <AppShell>
    <div v-if="loading" class="state-card">Loading artist roster...</div>
    <div v-else class="artists-grid">
      <div v-if="error" class="feedback error">{{ error }}</div>

      <article
        v-for="artist in artists"
        :key="artist.id"
        class="artist-card"
        :style="{ '--accent': artist.accentColor }"
      >
        <header>
          <p class="eyebrow">{{ artist.slug }}</p>
          <h3>{{ artist.displayName }}</h3>
          <p>{{ artist.bio }}</p>
        </header>

        <div class="tag-list">
          <span v-for="specialty in artist.specialties" :key="specialty">{{ specialty }}</span>
        </div>

        <p class="prompt-preview">{{ artist.personaPrompt }}</p>

        <div class="portfolio-grid">
          <img
            v-for="image in artist.portfolioImageUrls"
            :key="image"
            :src="image"
            alt="Artist portfolio reference"
          />
        </div>
      </article>
    </div>
  </AppShell>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { mapState } from 'pinia'
import AppShell from '@/components/AppShell.vue'
import { useAuthStore } from '@/stores/auth'
import { fetchArtists, type ArtistProfile } from '@/services/artistService'

export default defineComponent({
  name: 'ArtistsView',
  components: { AppShell },
  data() {
    return {
      artists: [] as ArtistProfile[],
      loading: true,
      error: null as string | null,
    }
  },
  computed: {
    ...mapState(useAuthStore, {
      token: (store) => store.token ?? '',
    }),
  },
  async mounted() {
    if (!this.token) return

    try {
      this.artists = await fetchArtists(this.token)
    } catch (err) {
      this.error = err instanceof Error ? err.message : 'Failed to load artists.'
    } finally {
      this.loading = false
    }
  },
})
</script>

<style scoped>
.artists-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 1.25rem;
}

.artist-card {
  border-radius: 24px;
  padding: 1.25rem;
  background: linear-gradient(
    160deg,
    rgba(255, 255, 255, 0.05),
    color-mix(in srgb, var(--accent) 16%, transparent 84%)
  );
  border: 1px solid rgba(255, 255, 255, 0.08);
  display: grid;
  gap: 1rem;
}

.artist-card p {
  color: rgba(255, 255, 255, 0.72);
  line-height: 1.55;
}

.eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.16em;
  font-size: 0.72rem;
  color: var(--accent);
}

.tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.tag-list span {
  padding: 0.45rem 0.7rem;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
}

.prompt-preview {
  font-style: italic;
}

.portfolio-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
}

.portfolio-grid img {
  width: 100%;
  aspect-ratio: 4 / 5;
  object-fit: cover;
  border-radius: 18px;
}

.state-card,
.feedback.error {
  padding: 1rem;
  border-radius: 18px;
}

.feedback.error {
  background: rgba(128, 28, 28, 0.22);
  border: 1px solid rgba(255, 114, 114, 0.4);
}
</style>
