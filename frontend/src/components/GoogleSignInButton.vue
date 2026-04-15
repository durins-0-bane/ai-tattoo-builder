<template>
  <div ref="container" class="google-btn-container"></div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { mapActions } from 'pinia'
import { useAuthStore } from '@/stores/auth'

export default defineComponent({
  name: 'GoogleSignInButton',
  mounted() {
    if (this.$refs.container instanceof HTMLElement) {
      this.setupGoogleButton(this.$refs.container)
    }
  },
  methods: {
    ...mapActions(useAuthStore, ['setupGoogleButton']),
  },
})
</script>

<style scoped>
.google-btn-container {
  display: flex;
  justify-content: center;
}
</style>
