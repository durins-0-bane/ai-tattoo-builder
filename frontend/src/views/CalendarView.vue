<template>
  <AppShell>
    <div v-if="loading" class="panel state-card">Loading calendar...</div>

    <div v-else class="calendar-layout">
      <section class="panel booking-panel">
        <p class="section-label">Book a session</p>
        <h3>Reserve time with an artist</h3>

        <div v-if="error" class="feedback error">{{ error }}</div>
        <div v-if="success" class="feedback success">{{ success }}</div>

        <form class="booking-form" @submit.prevent="submitAppointment">
          <label>
            Artist
            <select v-model="form.artistId">
              <option v-for="artist in artists" :key="artist.id" :value="artist.id">
                {{ artist.displayName }}
              </option>
            </select>
          </label>

          <label>
            Start time
            <input v-model="form.startTimeLocal" type="datetime-local" />
          </label>

          <label>
            Duration (minutes)
            <input v-model.number="form.durationMinutes" type="number" min="60" step="30" />
          </label>

          <label>
            Service
            <select v-model="form.serviceType">
              <option>Consultation</option>
              <option>New Tattoo</option>
              <option>Touch-up</option>
              <option>Concept Review</option>
            </select>
          </label>

          <button class="primary-button" :disabled="saving">
            {{ saving ? 'Booking...' : 'Book appointment' }}
          </button>
        </form>
      </section>

      <section class="panel appointments-panel">
        <p class="section-label">Upcoming</p>
        <div v-if="!appointments.length" class="state-card small">
          You have no booked sessions yet.
        </div>

        <article v-for="appointment in appointments" :key="appointment.id" class="appointment-card">
          <div>
            <strong>{{ artistName(appointment.artistId) }}</strong>
            <p>{{ appointment.serviceType }}</p>
          </div>
          <div>
            <p>{{ new Date(appointment.startTime).toLocaleString() }}</p>
            <p>{{ appointment.durationMinutes }} min • {{ appointment.status }}</p>
          </div>
        </article>
      </section>
    </div>
  </AppShell>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { mapState } from 'pinia'
import AppShell from '@/components/AppShell.vue'
import { useAuthStore } from '@/stores/auth'
import { fetchArtists, type ArtistProfile } from '@/services/artistService'
import {
  createAppointment,
  fetchAppointments,
  type Appointment,
} from '@/services/appointmentService'

export default defineComponent({
  name: 'CalendarView',
  components: { AppShell },
  data() {
    return {
      artists: [] as ArtistProfile[],
      appointments: [] as Appointment[],
      loading: true,
      saving: false,
      error: null as string | null,
      success: null as string | null,
      form: {
        artistId: '',
        startTimeLocal: '',
        durationMinutes: 180,
        serviceType: 'Consultation',
      },
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
    artistName(artistId: string): string {
      return this.artists.find((artist) => artist.id === artistId)?.displayName ?? 'Artist'
    },

    async bootstrap() {
      if (!this.token) return

      this.loading = true
      try {
        const [artistResults, appointmentResults] = await Promise.all([
          fetchArtists(this.token),
          fetchAppointments(this.token),
        ])

        this.artists = artistResults
        this.appointments = appointmentResults
        this.form.artistId = artistResults[0]?.id ?? ''
      } catch (err) {
        this.error = err instanceof Error ? err.message : 'Failed to load calendar.'
      } finally {
        this.loading = false
      }
    },

    async submitAppointment() {
      if (!this.token || !this.form.artistId || !this.form.startTimeLocal) return

      this.saving = true
      this.error = null
      this.success = null

      try {
        const created = await createAppointment(this.token, {
          artistId: this.form.artistId,
          startTimeUtc: new Date(this.form.startTimeLocal).toISOString(),
          durationMinutes: this.form.durationMinutes,
          serviceType: this.form.serviceType,
        })

        this.appointments = [...this.appointments, created].sort((left, right) =>
          left.startTime.localeCompare(right.startTime),
        )
        this.success = 'Appointment booked.'
      } catch (err) {
        this.error = err instanceof Error ? err.message : 'Failed to create appointment.'
      } finally {
        this.saving = false
      }
    },
  },
})
</script>

<style scoped>
.calendar-layout {
  display: grid;
  grid-template-columns: 380px 1fr;
  gap: 1.25rem;
}

.panel {
  border-radius: 24px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.04);
  padding: 1.25rem;
}

.section-label {
  text-transform: uppercase;
  letter-spacing: 0.14em;
  font-size: 0.72rem;
  color: rgba(255, 255, 255, 0.5);
  margin-bottom: 0.75rem;
}

.booking-form {
  display: grid;
  gap: 0.9rem;
  margin-top: 1rem;
}

label {
  display: grid;
  gap: 0.45rem;
  color: rgba(255, 255, 255, 0.7);
}

input,
select {
  border-radius: 14px;
  padding: 0.8rem 0.9rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(8, 8, 8, 0.7);
  color: #fff;
}

.primary-button {
  border: none;
  border-radius: 14px;
  padding: 0.9rem 1rem;
  background: linear-gradient(135deg, #d7a86e, #a86a35);
  color: #111;
  font-weight: 700;
  cursor: pointer;
}

.appointments-panel {
  display: grid;
  gap: 0.85rem;
  align-content: start;
}

.appointment-card {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.03);
}

.appointment-card p,
.state-card {
  color: rgba(255, 255, 255, 0.65);
}

.feedback {
  padding: 0.85rem 1rem;
  border-radius: 14px;
  margin-top: 1rem;
}

.feedback.error {
  background: rgba(128, 28, 28, 0.22);
  border: 1px solid rgba(255, 114, 114, 0.4);
}

.feedback.success {
  background: rgba(54, 98, 52, 0.22);
  border: 1px solid rgba(120, 217, 116, 0.35);
}

.state-card {
  display: grid;
  place-items: center;
  min-height: 220px;
  text-align: center;
}

.state-card.small {
  min-height: 120px;
}

@media (max-width: 1120px) {
  .calendar-layout {
    grid-template-columns: 1fr;
  }

  .appointment-card {
    flex-direction: column;
  }
}
</style>
