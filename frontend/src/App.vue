<template>
  <RouterView />
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

export default defineComponent({
  name: 'App',
  async mounted() {
    const authStore = useAuthStore()
    await authStore.restoreSession()
    if (!authStore.isAuthenticated) {
      router.replace({ name: 'login' })
    }
  },
})
</script>

<style>
*,
*::before,
*::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family:
    Inter,
    system-ui,
    -apple-system,
    sans-serif;
  background: #0f0f0f;
  color: #fff;
}
</style>
