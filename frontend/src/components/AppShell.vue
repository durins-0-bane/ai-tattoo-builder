<template>
  <div class="shell">
    <aside class="sidebar">
      <div class="brand">
        <p class="eyebrow">Showcase Build</p>
        <h1>AI Tattoo Builder</h1>
        <p class="summary">
          A curated studio of AI artists, saved concepts, and bookable sessions.
        </p>
      </div>

      <nav class="nav-list">
        <RouterLink
          v-for="item in navItems"
          :key="item.name"
          :to="{ name: item.name }"
          class="nav-item"
        >
          {{ item.label }}
        </RouterLink>
      </nav>

      <div class="profile-card">
        <img v-if="user?.avatarUrl" :src="user.avatarUrl" alt="Profile" class="avatar" />
        <div>
          <p class="profile-name">{{ user?.displayName }}</p>
          <p class="profile-email">{{ user?.email }}</p>
        </div>
      </div>

      <button class="signout" @click="signOut">Sign out</button>
    </aside>

    <section class="content">
      <header class="page-header">
        <div>
          <p class="eyebrow">Studio Workspace</p>
          <h2>{{ pageTitle }}</h2>
        </div>
        <slot name="header-actions" />
      </header>

      <div class="page-body">
        <slot />
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { mapState, mapActions } from 'pinia'
import { useAuthStore } from '@/stores/auth'

export default defineComponent({
  name: 'AppShell',
  data() {
    return {
      navItems: [
        { name: 'chat', label: 'Chat' },
        { name: 'gallery', label: 'Gallery' },
        { name: 'artists', label: 'Artists' },
        { name: 'calendar', label: 'Calendar' },
      ],
    }
  },
  computed: {
    ...mapState(useAuthStore, ['user']),
    pageTitle(): string {
      return this.navItems.find((item) => item.name === this.$route.name)?.label ?? 'Showcase'
    },
  },
  methods: {
    ...mapActions(useAuthStore, ['logout']),
    signOut() {
      this.logout()
      this.$router.replace({ name: 'login' })
    },
  },
})
</script>

<style scoped>
.shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 300px 1fr;
  background:
    radial-gradient(circle at top left, rgba(196, 149, 86, 0.18), transparent 28%),
    linear-gradient(135deg, #090909 0%, #121212 48%, #18130f 100%);
}

.sidebar {
  padding: 2rem;
  border-right: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  background: rgba(10, 10, 10, 0.78);
  backdrop-filter: blur(18px);
}

.brand h1 {
  font-size: 1.8rem;
  margin: 0.35rem 0 0.75rem;
}

.summary,
.profile-email {
  color: rgba(255, 255, 255, 0.6);
  line-height: 1.5;
}

.eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.18em;
  font-size: 0.72rem;
  color: #d7a86e;
}

.nav-list {
  display: grid;
  gap: 0.75rem;
}

.nav-item {
  padding: 0.85rem 1rem;
  color: rgba(255, 255, 255, 0.72);
  text-decoration: none;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.03);
  transition:
    background 0.2s ease,
    color 0.2s ease,
    transform 0.2s ease;
}

.nav-item:hover,
.nav-item.router-link-active {
  background: rgba(215, 168, 110, 0.16);
  color: #fff;
  transform: translateX(4px);
}

.profile-card {
  margin-top: auto;
  display: flex;
  gap: 0.85rem;
  align-items: center;
  padding: 1rem;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.04);
}

.avatar {
  width: 48px;
  height: 48px;
  border-radius: 999px;
}

.profile-name {
  font-weight: 600;
}

.signout {
  border: 1px solid rgba(255, 255, 255, 0.12);
  background: transparent;
  color: #fff;
  padding: 0.85rem 1rem;
  border-radius: 14px;
  cursor: pointer;
}

.content {
  padding: 2rem;
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.page-header h2 {
  font-size: 2rem;
  margin-top: 0.35rem;
}

.page-body {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

@media (max-width: 980px) {
  .shell {
    grid-template-columns: 1fr;
  }

  .sidebar {
    border-right: none;
    border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  }
}
</style>
