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

    <!-- Resumen para Supervisor y Admin cuando el backend está conectado -->
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

    <section class="module-hub mt-9" aria-labelledby="modulos-disponibles">
      <div class="module-hub__header d-flex align-center justify-space-between mb-5 flex-wrap gap-3">
        <div>
          <p class="dashboard-eyebrow mb-1">ACCESOS RÁPIDOS</p>
          <h2 id="modulos-disponibles" class="dashboard-card-title mb-0">Módulos disponibles</h2>
        </div>
        <v-chip color="primary" variant="tonal" size="small">
          <v-icon start size="16">mdi-view-grid</v-icon>
          {{ modulos.length }} módulos habilitados
        </v-chip>
      </div>

      <v-row class="module-grid">
        <v-col v-for="modulo in modulos" :key="modulo.ruta" cols="12" sm="6" lg="4">
          <v-card class="module-card pa-6" :class="{ 'module-card--featured': modulo.primario }">
            <div class="d-flex align-start justify-space-between mb-5">
              <div class="module-card__icon" :class="`module-card__icon--${modulo.color}`">
                <v-icon size="28">{{ modulo.icono }}</v-icon>
              </div>
              <v-chip size="x-small" color="success" variant="tonal">
                <v-icon start size="12">mdi-check-circle</v-icon>Disponible
              </v-chip>
            </div>
            <p class="dashboard-card-kicker">{{ modulo.categoria }}</p>
            <h2 class="module-card__title">{{ modulo.titulo }}</h2>
            <p class="module-card__copy">{{ modulo.descripcion }}</p>
            <v-btn :class="modulo.primario ? 'umg-gold-button' : ''"
              :color="modulo.primario ? 'accent' : 'primary'"
              :variant="modulo.primario ? 'flat' : 'outlined'"
              prepend-icon="mdi-arrow-right" @click="navigateTo(modulo.ruta)">
              {{ modulo.accion }}
            </v-btn>
          </v-card>
        </v-col>
      </v-row>
    </section>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'Dashboard' })

const auth    = useAuthStore()
const { api } = useApi()
const stats   = ref<any>(null)

const modulos = computed(() => {
  const disponibles = [
    {
      ruta: '/dashboard/analisis', icono: 'mdi-text-search', color: 'primary', primario: true,
      categoria: 'HERRAMIENTA PRINCIPAL', titulo: 'Análisis léxico',
      descripcion: 'Carga un archivo .txt, selecciona el idioma y revisa las palabras y patrones identificados.',
      accion: 'Iniciar análisis'
    },
    {
      ruta: '/dashboard/historial', icono: 'mdi-history', color: 'accent', primario: false,
      categoria: 'CONSULTA', titulo: 'Historial de análisis',
      descripcion: 'Consulta los análisis procesados y abre el detalle de cada resultado.',
      accion: 'Ver historial'
    },
    {
      ruta: '/dashboard/perfil', icono: 'mdi-account-edit', color: 'primary', primario: false,
      categoria: 'CUENTA', titulo: 'Gestión de perfil',
      descripcion: 'Actualiza tu información, método de notificación y contraseña.',
      accion: 'Administrar perfil'
    }
  ]

  if (auth.isSupervisor) {
    disponibles.push({
      ruta: '/dashboard/reportes', icono: 'mdi-chart-bar', color: 'accent', primario: false,
      categoria: 'SUPERVISIÓN', titulo: 'Estadísticas y reportes',
      descripcion: 'Revisa indicadores de análisis y actividad general del sistema.',
      accion: 'Ver estadísticas'
    })
  }

  if (auth.isAdmin) {
    disponibles.push(
      {
        ruta: '/dashboard/usuarios', icono: 'mdi-account-group', color: 'primary', primario: false,
        categoria: 'ADMINISTRACIÓN', titulo: 'Usuarios y roles',
        descripcion: 'Gestiona cuentas, asigna roles y controla el estado de los usuarios.',
        accion: 'Gestionar usuarios'
      },
      {
        ruta: '/dashboard/bitacora', icono: 'mdi-shield-check', color: 'accent', primario: false,
        categoria: 'AUDITORÍA', titulo: 'Bitácora de accesos',
        descripcion: 'Consulta quién ingresó, cuándo lo hizo y el resultado de cada acceso.',
        accion: 'Consultar bitácora'
      }
    )
  }

  return disponibles
})

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
