<template>
  <span class="loading-indicator" role="status" :aria-label="text">
    <span class="spinner" :class="size" aria-hidden="true"></span>
    <span class="loading-text">
      {{ text
      }}<span class="dots" aria-hidden="true"><span>.</span><span>.</span><span>.</span></span>
    </span>
  </span>
</template>

<script lang="ts">
import { defineComponent } from 'vue'

export default defineComponent({
  name: 'LoadingIndicator',
  props: {
    text: {
      type: String,
      default: 'Loading',
    },
    size: {
      type: String,
      default: 'medium',
      validator: (value: string) => ['small', 'medium'].includes(value),
    },
  },
})
</script>

<style scoped>
.loading-indicator {
  display: inline-flex;
  align-items: center;
  gap: 0.6rem;
  color: #d5dbe2;
  font-size: 0.95rem;
}

.spinner {
  border: 2px solid rgba(213, 219, 226, 0.28);
  border-top-color: #e2b878;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.spinner.medium {
  width: 16px;
  height: 16px;
}

.spinner.small {
  width: 12px;
  height: 12px;
}

.dots span {
  animation: blink 1.4s infinite both;
}

.dots span:nth-child(2) {
  animation-delay: 0.2s;
}

.dots span:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@keyframes blink {
  0%,
  20% {
    opacity: 0;
  }
  50% {
    opacity: 1;
  }
  100% {
    opacity: 0;
  }
}
</style>
