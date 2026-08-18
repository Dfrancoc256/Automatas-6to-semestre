<template>
  <div>
    <div class="dashboard-header d-flex align-center justify-space-between mb-8">
      <div>
        <p class="dashboard-eyebrow">PANEL ACADÉMICO</p>
        <h1 class="dashboard-title">Dashboard</h1>
        <p class="dashboard-subtitle">Bienvenido, {{ auth.user?.nickname }}</p>
      </div>
      <v-chip class="role-chip" color="accent" variant="tonal">
        <v-icon start size="14">mdi-circle</v-icon>
        {{ auth.user?.rol }}
      </v-chip>
    </div>

    <!-- Estadísticas (Admin/Supervisor) -->
    <template v-if="auth.isSupervisor && stats">
      <v-row class="mb-6">
        <v-col cols="12" sm="6" md="3">
          <div class="stat-card">
            <div class="stat-icon" style="background:#FFF8E1">📊</div>
            <div>
              <div class="stat-value">{{ stats.totalAnalisis }}</div>
              <div class="stat-label">Análisis totales</div>
            </div>
          </div>
        </v-col>
        <v-col cols="12" sm="6" md="3">
          <div class="stat-card">
            <div class="stat-icon" style="background:#F0FDF4">👥</div>
            <div>
              <div class="stat-value">{{ stats.totalUsuarios }}</div>
              <div class="stat-label">Usuarios activos</div>
            </div>
          </div>
        </v-col>
        <v-col cols="12" sm="6" md="3">
          <div class="stat-card">
            <div class="stat-icon" style="background:#FFFBEB">📝</div>
            <div>
              <div class="stat-value">{{ stats.analisisHoy }}</div>
              <div class="stat-label">Análisis hoy</div>
            </div>
          </div>
        </v-col>
        <v-col cols="12" sm="6" md="3">
          <div class="stat-card">
            <div class="stat-icon" style="background:#FFF1F2">🔐</div>
            <div>
              <div class="stat-value">{{ stats.loginsHoy }}</div>
              <div class="stat-label">Logins hoy</div>
            </div>
          </div>
        </v-col>
      </v-row>

      <!-- Por idioma + Recientes -->
      <v-row>
        <v-col cols="12" md="4">
          <v-card>
            <v-card-title class="text-subtitle-1 font-weight-bold pa-4 pb-2">
              Análisis por idioma
            </v-card-title>
            <v-card-text>
              <div v-for="item in stats.porIdioma" :key="item.idioma" class="mb-3">
                <div class="d-flex justify-space-between text-body-2 mb-1">
                  <span class="text-capitalize">{{ item.idioma }}</span>
                  <span class="font-weight-bold">{{ item.cantidad }}</span>
                </div>
                <v-progress-linear
                  :model-value="(item.cantidad / (stats.totalAnalisis || 1)) * 100"
                  color="accent" bg-color="#E5E7EB" rounded height="6" />
              </div>
              <div v-if="!stats.porIdioma?.length" class="text-center text-medium-emphasis py-4">
                Sin datos aún
              </div>
            </v-card-text>
          </v-card>
        </v-col>

        <v-col cols="12" md="8">
          <v-card>
            <v-card-title class="text-subtitle-1 font-weight-bold pa-4 pb-2">
              Análisis recientes
            </v-card-title>
            <v-table density="compact">
              <thead>
                <tr>
                  <th>Archivo</th>
                  <th>Idioma</th>
                  <th>Palabras</th>
                  <th>Usuario</th>
                  <th>Fecha</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="r in stats.recientes" :key="r.id">
                  <td class="text-truncate" style="max-width:140px">{{ r.nombreArchivo }}</td>
                  <td><v-chip size="x-small" color="accent" variant="tonal">{{ r.idioma }}</v-chip></td>
                  <td>{{ r.totalPalabras.toLocaleString() }}</td>
                  <td>{{ r.usuario }}</td>
                  <td class="text-caption">{{ formatFecha(r.fechaAnalisis) }}</td>
                </tr>
                <tr v-if="!stats.recientes?.length">
                  <td colspan="5" class="text-center text-medium-emphasis py-4">Sin análisis recientes</td>
                </tr>
              </tbody>
            </v-table>
          </v-card>
        </v-col>
      </v-row>
    </template>

    <!-- Vista básica para Analista -->
    <template v-else>
      <v-row>
        <v-col cols="12" md="6">
          <v-card class="dashboard-action-card dashboard-action-card--primary pa-7">
            <v-icon size="34" color="primary" class="mb-5">mdi-text-search</v-icon>
            <p class="dashboard-card-kicker">HERRAMIENTA PRINCIPAL</p>
            <h2 class="dashboard-card-title">Análisis léxico</h2>
            <p class="dashboard-card-copy">
              Carga un archivo .txt y obtén un análisis detallado de su contenido léxico.
            </p>
            <v-btn class="umg-gold-button" color="accent" prepend-icon="mdi-arrow-right"
                   @click="navigateTo('/dashboard/analisis')">
              Comenzar análisis
            </v-btn>
          </v-card>
        </v-col>
        <v-col cols="12" md="6">
          <v-card class="dashboard-action-card pa-7">
            <v-icon size="34" color="accent" class="mb-5">mdi-history</v-icon>
            <p class="dashboard-card-kicker">CONSULTA PERSONAL</p>
            <h2 class="dashboard-card-title">Mi historial</h2>
            <p class="dashboard-card-copy">
              Revisa todos tus análisis anteriores con sus resultados.
            </p>
            <v-btn color="primary" variant="outlined" prepend-icon="mdi-arrow-right"
                   @click="navigateTo('/dashboard/historial')">
              Ver historial
            </v-btn>
          </v-card>
        </v-col>
      </v-row>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'Dashboard' })

const auth    = useAuthStore()
const { api } = useApi()
const stats   = ref<any>(null)

onMounted(async () => {
  if (auth.isSupervisor) {
    try {
      const { data } = await api.get('/dashboard/estadisticas')
      stats.value = data
    } catch { /* sin permisos */ }
  }
})

function formatFecha(iso: string) {
  return new Date(iso).toLocaleDateString('es-GT', {
    day: '2-digit', month: 'short', year: 'numeric'
  })
}
</script>
