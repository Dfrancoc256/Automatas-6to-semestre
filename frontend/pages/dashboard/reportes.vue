<template>
  <div>
    <div class="d-flex align-center mb-6">
      <v-icon color="accent" class="mr-3">mdi-chart-bar</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Estadísticas</h1>
        <p class="text-body-2 text-medium-emphasis">Vista general del sistema</p>
      </div>
    </div>

    <v-row v-if="stats">
      <v-col cols="12" sm="6" md="3" v-for="m in metricas" :key="m.l">
        <div class="stat-card">
          <div class="stat-icon" :style="`background:${m.bg}`">{{ m.icon }}</div>
          <div>
            <div class="stat-value">{{ m.v }}</div>
            <div class="stat-label">{{ m.l }}</div>
          </div>
        </div>
      </v-col>

      <v-col cols="12" md="5">
        <v-card class="pa-5">
          <h3 class="text-subtitle-1 font-weight-bold mb-4">Análisis por idioma</h3>
          <div v-for="item in stats.porIdioma" :key="item.idioma" class="mb-3">
            <div class="d-flex justify-space-between text-body-2 mb-1">
              <span class="text-capitalize">{{ item.idioma }}</span>
              <span class="font-weight-bold">{{ item.cantidad }}</span>
            </div>
            <v-progress-linear :model-value="(item.cantidad / (stats.totalAnalisis || 1)) * 100"
                               color="accent" height="8" rounded />
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="7">
        <v-card class="pa-5">
          <h3 class="text-subtitle-1 font-weight-bold mb-4">Actividad de hoy</h3>
          <div class="d-flex gap-4">
            <v-card variant="tonal" color="success" class="pa-4 flex-1 text-center">
              <div class="text-h4 font-weight-bold">{{ stats.loginsHoy }}</div>
              <div class="text-caption">Logins exitosos</div>
            </v-card>
            <v-card variant="tonal" color="error" class="pa-4 flex-1 text-center">
              <div class="text-h4 font-weight-bold">{{ stats.loginsFallidosHoy }}</div>
              <div class="text-caption">Logins fallidos</div>
            </v-card>
            <v-card variant="tonal" color="accent" class="pa-4 flex-1 text-center">
              <div class="text-h4 font-weight-bold">{{ stats.analisisHoy }}</div>
              <div class="text-caption">Análisis hoy</div>
            </v-card>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-skeleton-loader v-else type="card" />
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'Estadísticas' })

const { api } = useApi()
const stats   = ref<any>(null)

const metricas = computed(() => stats.value ? [
  { l: 'Usuarios',      v: stats.value.totalUsuarios,   icon: '👥', bg: '#FFF8E1' },
  { l: 'Análisis',      v: stats.value.totalAnalisis,   icon: '📊', bg: '#F0FDF4' },
  { l: 'Análisis hoy',  v: stats.value.analisisHoy,     icon: '📝', bg: '#FFFBEB' },
  { l: 'Logins hoy',    v: stats.value.loginsHoy,       icon: '🔐', bg: '#FFF1F2' }
] : [])

onMounted(async () => {
  try {
    const { data } = await api.get('/dashboard/estadisticas')
    stats.value = data
  } catch { /* ignore */ }
})
</script>
<style scoped>.gap-4{gap:16px;}.flex-1{flex:1;}</style>
