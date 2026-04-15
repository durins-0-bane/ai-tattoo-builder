<template>
  <AppShell>
    <div v-if="loading" class="panel state-card">Loading your gallery...</div>
    <div v-else class="gallery-layout">
      <div v-if="error" class="feedback error">{{ error }}</div>

      <div v-if="!designs.length" class="panel state-card">
        Your gallery is empty. Generate a concept in Chat and save it here.
      </div>

      <article v-for="design in designs" :key="design.id" class="design-card">
        <img :src="design.imageUrl" alt="Tattoo design" class="design-image" />
        <div class="design-content">
          <div class="design-meta">
            <span>{{ artistNames[design.artistId ?? ''] ?? 'Studio artist' }}</span>
            <span>{{ new Date(design.createdAt).toLocaleDateString() }}</span>
          </div>
          <h3>{{ design.style }}</h3>
          <p>{{ design.prompt }}</p>
          <button class="favorite-button" @click="onToggleFavorite(design)">
            {{ design.isFavorite ? '★ Favorited' : '☆ Mark favorite' }}
          </button>
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
import { fetchArtists } from '@/services/artistService'
import { fetchDesigns, toggleFavorite, type TattooDesign } from '@/services/designService'

export default defineComponent({
  name: 'GalleryView',
  components: { AppShell },
  data() {
    return {
      designs: [] as TattooDesign[],
      artistNames: {} as Record<string, string>,
      loading: true,
      error: null as string | null,
    }
  },
  computed: {
    ...mapState(useAuthStore, {
      token: (store) => store.token ?? '',
    }),
  },
  mounted() {
    this.bootstrap()
  },
  methods: {
    async bootstrap() {
      if (!this.token) return

      this.loading = true
      this.error = null

      try {
        const [artistResults, designResults] = await Promise.all([
          fetchArtists(this.token),
          fetchDesigns(this.token),
        ])

        this.artistNames = Object.fromEntries(
          artistResults.map((artist) => [artist.id, artist.displayName]),
        )
        this.designs = designResults
      } catch (err) {
        this.error = err instanceof Error ? err.message : 'Failed to load gallery.'
      } finally {
        this.loading = false
      }
    },

    async onToggleFavorite(design: TattooDesign) {
      if (!this.token) return

      try {
        const updated = await toggleFavorite(this.token, design.id)
        this.designs = this.designs.map((entry) => (entry.id === design.id ? updated : entry))
      } catch (err) {
        this.error = err instanceof Error ? err.message : 'Failed to update favorite state.'
      }
    },
  },
})
</script>

<style scoped>
.gallery-layout {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.25rem;
}

.design-card,
.panel {
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  overflow: hidden;
}

.design-image {
  width: 100%;
  aspect-ratio: 4 / 5;
  object-fit: cover;
  background: rgba(255, 255, 255, 0.03);
}

.design-content {
  padding: 1rem;
  display: grid;
  gap: 0.7rem;
}

.design-content p,
.design-meta {
  color: rgba(255, 255, 255, 0.66);
}

.design-meta {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  font-size: 0.82rem;
}

.favorite-button {
  border: 1px solid rgba(255, 255, 255, 0.12);
  background: transparent;
  color: #fff;
  border-radius: 14px;
  padding: 0.75rem 0.9rem;
  cursor: pointer;
}

.state-card {
  min-height: 220px;
  display: grid;
  place-items: center;
  text-align: center;
  color: rgba(255, 255, 255, 0.65);
}

.feedback.error {
  grid-column: 1 / -1;
  padding: 0.9rem 1rem;
  border-radius: 16px;
  background: rgba(128, 28, 28, 0.22);
  border: 1px solid rgba(255, 114, 114, 0.4);
}
</style>
