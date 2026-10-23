<template>
  <div>
    <!-- Encabezado + filtro de periodo -->
    <div class="d-flex align-center flex-wrap mb-5" style="gap:12px">
      <v-icon color="accent" class="mr-1">mdi-chart-bar</v-icon>
      <div class="flex-1">
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Estadísticas</h1>
        <p class="text-body-2 text-medium-emphasis">
          Vista general del sistema<span v-if="det"> · {{ fechaLarga(det.desde) }} – {{ fechaLarga(det.hasta) }}</span>
        </p>
      </div>
      <v-btn-toggle v-model="dias" mandatory density="comfortable" variant="outlined" divided color="primary">
        <v-btn :value="7">7 días</v-btn>
        <v-btn :value="30">30 días</v-btn>
        <v-btn :value="90">90 días</v-btn>
      </v-btn-toggle>
      <v-btn icon="mdi-refresh" variant="text" :loading="cargando" title="Actualizar" @click="cargar" />
    </div>

    <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mb-5">{{ error }}</v-alert>

    <v-skeleton-loader v-if="cargando && !det" type="card, card" />

    <template v-if="det && stats">
      <!-- KPIs -->
      <v-row dense class="mb-2">
        <v-col v-for="k in kpis" :key="k.l" cols="6" md="3">
          <div class="kpi">
            <div class="kpi-top">
              <v-icon :color="k.color" size="20">{{ k.icon }}</v-icon>
              <span class="kpi-label">{{ k.l }}</span>
            </div>
            <div class="kpi-value">{{ k.v }}</div>
            <div class="kpi-sub">{{ k.s }}</div>
          </div>
        </v-col>
      </v-row>

      <v-row>
        <!-- Tendencia de análisis -->
        <v-col cols="12" lg="8">
          <v-card class="pa-5 h-100">
            <div class="card-head">
              <div>
                <h3 class="card-title">Análisis realizados por día</h3>
                <p class="card-sub">{{ formatNum(det.kpis.analisis) }} análisis · {{ formatNum(det.kpis.palabras) }} palabras procesadas</p>
              </div>
            </div>
            <ChartsLineChart :labels="fechasDia" :values="det.analisisPorDia.map((d: any) => d.cantidad)"
                             serie-name="Análisis" :color="C.azul" :format-x="fechaCorta" :format-tip="fechaLarga"
                             :extra="(i: number) => `${formatNum(det.analisisPorDia[i].palabras)} palabras`"
                             aria-label="Análisis realizados por día" />
          </v-card>
        </v-col>

        <!-- Actividad de hoy -->
        <v-col cols="12" lg="4">
          <v-card class="pa-5 h-100">
            <h3 class="card-title mb-4">Actividad de hoy</h3>
            <div class="hoy-grid">
              <div class="hoy ok">
                <v-icon size="18">mdi-check-circle</v-icon>
                <div class="hoy-num">{{ stats.loginsHoy }}</div>
                <div class="hoy-lbl">Logins exitosos</div>
              </div>
              <div class="hoy bad">
                <v-icon size="18">mdi-alert-circle</v-icon>
                <div class="hoy-num">{{ stats.loginsFallidosHoy }}</div>
                <div class="hoy-lbl">Logins fallidos</div>
              </div>
              <div class="hoy info">
                <v-icon size="18">mdi-text-search</v-icon>
                <div class="hoy-num">{{ stats.analisisHoy }}</div>
                <div class="hoy-lbl">Análisis hoy</div>
              </div>
              <div class="hoy neutral">
                <v-icon size="18">mdi-account-group</v-icon>
                <div class="hoy-num">{{ stats.totalUsuarios }}</div>
                <div class="hoy-lbl">Usuarios activos</div>
              </div>
            </div>
            <v-divider class="my-4" />
            <div class="d-flex justify-space-between text-body-2 mb-1">
              <span>Tasa de éxito de acceso ({{ dias }} días)</span>
              <span class="font-weight-bold">{{ det.kpis.tasaExito }}%</span>
            </div>
            <v-progress-linear :model-value="det.kpis.tasaExito" :color="det.kpis.tasaExito >= 80 ? 'success' : 'warning'"
                               height="8" rounded bg-color="#E7E9EC" bg-opacity="1" />
            <div class="d-flex justify-space-between text-body-2 mt-4 mb-1">
              <span>Usuarios con rostro registrado</span>
              <span class="font-weight-bold">{{ pctRostro }}%</span>
            </div>
            <v-progress-linear :model-value="pctRostro" color="info" height="8" rounded bg-color="#E7E9EC" bg-opacity="1" />
          </v-card>
        </v-col>

        <!-- Accesos por día -->
        <v-col cols="12" lg="8">
          <v-card class="pa-5 h-100">
            <div class="card-head">
              <div>
                <h3 class="card-title">Inicios de sesión por día</h3>
                <p class="card-sub">{{ formatNum(det.kpis.logins) }} intentos en el periodo</p>
              </div>
              <div class="leg">
                <span><i class="sw" :style="{ background: C.ok }" />Exitosos</span>
                <span><i class="sw" :style="{ background: C.bad }" />Fallidos</span>
              </div>
            </div>
            <ChartsBarChart :labels="fechasDia" :format-x="fechaCorta" :format-tip="fechaLarga"
                            :series="[
                              { name: 'Exitosos', color: C.ok,  values: det.loginsPorDia.map((d: any) => d.exitosos) },
                              { name: 'Fallidos', color: C.bad, values: det.loginsPorDia.map((d: any) => d.fallidos) }
                            ]" aria-label="Inicios de sesión por día" />
          </v-card>
        </v-col>

        <!-- Métodos de acceso -->
        <v-col cols="12" lg="4">
          <v-card class="pa-5 h-100">
            <h3 class="card-title">Método de acceso</h3>
            <p class="card-sub mb-4">Intentos por tipo de autenticación</p>
            <div v-for="m in det.loginsPorMetodo" :key="m.metodo" class="mb-4">
              <div class="d-flex align-center text-body-2 mb-1">
                <v-icon size="16" class="mr-2" color="primary">{{ iconoMetodo(m.metodo) }}</v-icon>
                <span class="flex-1">{{ nombreMetodo(m.metodo) }}</span>
                <span class="font-weight-bold">{{ m.exitosos + m.fallidos }}</span>
              </div>
              <div class="stack">
                <div :style="{ width: pct(m.exitosos, maxMetodo) + '%', background: C.ok }" :title="`${m.exitosos} exitosos`" />
                <div :style="{ width: pct(m.fallidos, maxMetodo) + '%', background: C.bad }" :title="`${m.fallidos} fallidos`" />
              </div>
              <div class="text-caption text-medium-emphasis mt-1">
                {{ m.exitosos }} exitosos · {{ m.fallidos }} fallidos
              </div>
            </div>
            <p v-if="!det.loginsPorMetodo.length" class="text-body-2 text-medium-emphasis">Sin accesos en el periodo.</p>
          </v-card>
        </v-col>

        <!-- Idiomas -->
        <v-col cols="12" md="6" lg="4">
          <v-card class="pa-5 h-100">
            <h3 class="card-title">Análisis por idioma</h3>
            <p class="card-sub mb-4">Documentos analizados en el periodo</p>
            <div v-for="it in det.analisisPorIdioma" :key="it.idioma" class="mb-3">
              <div class="d-flex justify-space-between text-body-2 mb-1">
                <span class="text-capitalize">{{ it.idioma }}</span>
                <span><b>{{ it.cantidad }}</b> <span class="text-medium-emphasis">· {{ formatNum(it.palabras) }} palabras</span></span>
              </div>
              <div class="bar-bg"><div class="bar" :style="{ width: pct(it.cantidad, maxIdioma) + '%', background: C.azul }" /></div>
            </div>
            <p v-if="!det.analisisPorIdioma.length" class="text-body-2 text-medium-emphasis">Sin análisis en el periodo.</p>
          </v-card>
        </v-col>

        <!-- Roles -->
        <v-col cols="12" md="6" lg="4">
          <v-card class="pa-5 h-100">
            <h3 class="card-title">Usuarios por rol</h3>
            <p class="card-sub mb-4">Cuentas activas</p>
            <ChartsDonutChart center-label="usuarios" aria-label="Usuarios por rol"
                              :items="det.usuariosPorRol.map((r: any, i: number) => ({ label: r.rol.toLowerCase(), value: r.cantidad, color: CAT[i % 3] }))" />
          </v-card>
        </v-col>

        <!-- Notificación -->
        <v-col cols="12" md="6" lg="4">
          <v-card class="pa-5 h-100">
            <h3 class="card-title">Canal de notificación</h3>
            <p class="card-sub mb-4">Preferencia de los usuarios activos</p>
            <ChartsDonutChart center-label="usuarios" aria-label="Canal de notificación preferido"
                              :items="det.usuariosPorNotificacion.map((r: any, i: number) => ({ label: r.metodo, value: r.cantidad, color: CAT[i % 3] }))" />
          </v-card>
        </v-col>

        <!-- Horas -->
        <v-col cols="12" md="6" lg="8">
          <v-card class="pa-5 h-100">
            <h3 class="card-title">Horario de mayor actividad</h3>
            <p class="card-sub mb-2">Intentos de inicio de sesión por hora del día (hora de Guatemala)</p>
            <ChartsBarChart :labels="det.loginsPorHora.map((h: any) => String(h.hora))" :height="200"
                            :format-x="(h: string) => `${h}h`" :format-tip="(h: string) => `${h}:00 – ${h}:59`"
                            :series="[{ name: 'Intentos', color: C.azul, values: det.loginsPorHora.map((h: any) => h.cantidad) }]"
                            aria-label="Inicios de sesión por hora" />
          </v-card>
        </v-col>

        <!-- Registros -->
        <v-col cols="12" md="6" lg="4">
          <v-card class="pa-5 h-100">
            <h3 class="card-title">Nuevos registros</h3>
            <p class="card-sub mb-2">{{ det.kpis.nuevosUsuarios }} usuarios nuevos en el periodo</p>
            <ChartsBarChart :labels="fechasDia" :height="200" :format-x="fechaCorta" :format-tip="fechaLarga"
                            :series="[{ name: 'Registros', color: C.violeta, values: det.registrosPorDia.map((d: any) => d.cantidad) }]"
                            aria-label="Nuevos registros por día" />
          </v-card>
        </v-col>

        <!-- Top analistas -->
        <v-col cols="12" md="6">
          <v-card class="pa-5 h-100">
            <h3 class="card-title">Usuarios más activos</h3>
            <p class="card-sub mb-3">Ranking por cantidad de análisis en el periodo</p>
            <v-table density="compact">
              <thead><tr><th>#</th><th>Usuario</th><th class="text-right">Análisis</th><th class="text-right">Palabras</th></tr></thead>
              <tbody>
                <tr v-for="(t, i) in det.topAnalistas" :key="t.usuario">
                  <td><span class="rank" :class="{ top: i === 0 }">{{ i + 1 }}</span></td>
                  <td class="font-weight-medium">{{ t.usuario }}</td>
                  <td class="text-right font-weight-bold">{{ t.analisis }}</td>
                  <td class="text-right">{{ formatNum(t.palabras) }}</td>
                </tr>
                <tr v-if="!det.topAnalistas.length"><td colspan="4" class="text-medium-emphasis">Sin análisis en el periodo.</td></tr>
              </tbody>
            </v-table>
          </v-card>
        </v-col>

        <!-- Seguridad -->
        <v-col cols="12" md="6">
          <v-card class="pa-5 h-100">
            <h3 class="card-title">Alertas de seguridad</h3>
            <p class="card-sub mb-3">Direcciones IP con más intentos fallidos</p>
            <v-table density="compact">
              <thead><tr><th>IP de origen</th><th class="text-right">Fallidos</th><th>Nivel</th></tr></thead>
              <tbody>
                <tr v-for="ip in det.ipsConFallos" :key="ip.ip">
                  <td class="font-weight-medium" style="font-family:monospace">{{ ip.ip }}</td>
                  <td class="text-right font-weight-bold">{{ ip.fallidos }}</td>
                  <td>
                    <v-chip size="x-small" variant="tonal" :color="ip.fallidos >= 10 ? 'error' : ip.fallidos >= 4 ? 'warning' : 'success'"
                            :prepend-icon="ip.fallidos >= 10 ? 'mdi-alert-octagon' : ip.fallidos >= 4 ? 'mdi-alert' : 'mdi-check'">
                      {{ ip.fallidos >= 10 ? 'Alto' : ip.fallidos >= 4 ? 'Medio' : 'Bajo' }}
                    </v-chip>
                  </td>
                </tr>
                <tr v-if="!det.ipsConFallos.length">
                  <td colspan="3" class="text-medium-emphasis"><v-icon size="16" color="success" class="mr-1">mdi-shield-check</v-icon>Sin intentos fallidos en el periodo.</td>
                </tr>
              </tbody>
            </v-table>
          </v-card>
        </v-col>

        <!-- Recientes -->
        <v-col cols="12">
          <v-card class="pa-5">
            <h3 class="card-title">Últimos análisis</h3>
            <p class="card-sub mb-3">Los 10 análisis más recientes del sistema</p>
            <v-table density="compact">
              <thead><tr><th>Archivo</th><th>Usuario</th><th>Idioma</th><th class="text-right">Palabras</th><th>Fecha</th></tr></thead>
              <tbody>
                <tr v-for="r in stats.recientes" :key="r.id">
                  <td class="font-weight-medium text-truncate" style="max-width:260px">{{ r.nombreArchivo }}</td>
                  <td>{{ r.usuario }}</td>
                  <td class="text-capitalize">{{ r.idioma }}</td>
                  <td class="text-right">{{ formatNum(r.totalPalabras) }}</td>
                  <td class="text-medium-emphasis">{{ fechaHora(r.fechaAnalisis) }}</td>
                </tr>
                <tr v-if="!stats.recientes?.length"><td colspan="5" class="text-medium-emphasis">Todavía no hay análisis.</td></tr>
              </tbody>
            </v-table>
          </v-card>
        </v-col>
      </v-row>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN', 'SUPERVISOR'] })
useHead({ title: 'Estadísticas' })

const { api } = useApi()
const stats    = ref<any>(null)
const det      = ref<any>(null)
const cargando = ref(true)
const error    = ref('')
const dias     = ref(30)

// Paleta categórica validada (orden fijo) + colores de estado del tema
const C = { azul: '#2a78d6', violeta: '#6250d6', ok: '#2E7D5B', bad: '#CB3332' }
const CAT = ['#2a78d6', '#eb6834', '#1baf7a']

const fechasDia = computed<string[]>(() => det.value?.analisisPorDia.map((d: any) => d.fecha) ?? [])
const maxIdioma = computed(() => Math.max(1, ...(det.value?.analisisPorIdioma.map((i: any) => i.cantidad) ?? [])))
const maxMetodo = computed(() => Math.max(1, ...(det.value?.loginsPorMetodo.map((m: any) => m.exitosos + m.fallidos) ?? [])))
const pctRostro = computed(() => {
  const k = det.value?.kpis
  return k?.usuariosActivos ? Math.round((k.usuariosConRostro / k.usuariosActivos) * 100) : 0
})

const kpis = computed(() => {
  const k = det.value?.kpis
  if (!k) return []
  return [
    { l: 'Usuarios activos',   v: formatNum(k.usuariosActivos), s: `${k.nuevosUsuarios} nuevos · ${k.usuariosInactivos} inactivos`, icon: 'mdi-account-group', color: 'primary' },
    { l: 'Análisis',           v: formatNum(k.analisis),        s: `${k.analistasActivos} usuarios analizaron`,                     icon: 'mdi-file-document-multiple', color: 'info' },
    { l: 'Palabras procesadas', v: formatNum(k.palabras),       s: `Promedio ${formatNum(k.promedioPalabras)} por análisis`,        icon: 'mdi-format-letter-case', color: 'accent' },
    { l: 'Tokens generados',   v: formatNum(k.tokens),          s: 'Total del analizador léxico',                                   icon: 'mdi-code-tags', color: 'accent' },
    { l: 'Inicios de sesión',  v: formatNum(k.logins),          s: `${k.loginsExitosos} exitosos`,                                  icon: 'mdi-login', color: 'success' },
    { l: 'Accesos fallidos',   v: formatNum(k.loginsFallidos),  s: `Tasa de éxito ${k.tasaExito}%`,                                 icon: 'mdi-shield-alert', color: 'error' },
    { l: 'Con rostro',         v: formatNum(k.usuariosConRostro), s: `${pctRostro.value}% de los usuarios activos`,                 icon: 'mdi-face-recognition', color: 'info' },
    { l: 'Con Google',         v: formatNum(k.usuariosConGoogle), s: 'Cuentas vinculadas a Google',                                  icon: 'mdi-google', color: 'primary' }
  ]
})

const pct = (v: number, max: number) => (v / (max || 1)) * 100
const iconoMetodo = (m: string) => ({ password: 'mdi-form-textbox-password', facial: 'mdi-face-recognition', qr: 'mdi-qrcode', google: 'mdi-google' } as Record<string, string>)[m] || 'mdi-login'
const nombreMetodo = (m: string) => ({ password: 'Usuario y contraseña', facial: 'Reconocimiento facial', qr: 'Código QR', google: 'Google' } as Record<string, string>)[m] || m

const MESES: string[] = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic']
function partes(iso: string) { const [a, m, d] = iso.split('-').map(Number); return { a, m, d } }
const fechaCorta = (iso: string) => { const { m, d } = partes(iso); return `${d} ${MESES[(m ?? 1) - 1]}` }
const fechaLarga = (iso: string) => { const { a, m, d } = partes(iso); return `${d} ${MESES[(m ?? 1) - 1]} ${a}` }
const fechaHora = (v: string) => new Date(v).toLocaleString('es-GT', { dateStyle: 'medium', timeStyle: 'short' })

async function cargar() {
  cargando.value = true
  error.value = ''
  try {
    const [general, detalle] = await Promise.all([
      api.get('/dashboard/estadisticas'),
      api.get('/dashboard/estadisticas/detalle', { params: { dias: dias.value } })
    ])
    stats.value = general.data
    det.value = detalle.data
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'No fue posible cargar las estadísticas.'
  } finally {
    cargando.value = false
  }
}

watch(dias, cargar)
onMounted(cargar)
</script>

<style scoped>
.flex-1 { flex: 1; }
.kpi {
  background: #fff; border: 1px solid #E7E9EC; border-radius: 12px;
  padding: 14px 16px; height: 100%;
}
.kpi-top { display: flex; align-items: center; gap: 8px; }
.kpi-label { font-size: 12px; color: #6B7280; font-weight: 600; text-transform: uppercase; letter-spacing: .03em; }
.kpi-value { font-size: 26px; font-weight: 800; color: #051B2E; margin-top: 6px; font-variant-numeric: tabular-nums; }
.kpi-sub { font-size: 12px; color: #6B7280; margin-top: 2px; }
.card-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }
.card-title { font-size: 15px; font-weight: 700; color: #051B2E; }
.card-sub { font-size: 12.5px; color: #6B7280; margin: 2px 0 0; }
.leg { display: flex; gap: 14px; font-size: 12.5px; color: #374151; }
.leg span { display: flex; align-items: center; gap: 6px; }
.sw { width: 10px; height: 10px; border-radius: 3px; display: inline-block; }
.hoy-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
.hoy { border-radius: 10px; padding: 12px; }
.hoy-num { font-size: 26px; font-weight: 800; line-height: 1.2; margin-top: 4px; }
.hoy-lbl { font-size: 12px; opacity: .85; }
.hoy.ok { background: #EAF5EF; color: #1F5A41; }
.hoy.bad { background: #FBECEC; color: #9B2423; }
.hoy.info { background: #EAF1F8; color: #1A5080; }
.hoy.neutral { background: #F3F4F6; color: #051B2E; }
.stack { display: flex; gap: 2px; height: 10px; background: #EEF0F2; border-radius: 5px; overflow: hidden; }
.stack div { height: 100%; }
.bar-bg { height: 8px; background: #EEF0F2; border-radius: 4px; overflow: hidden; }
.bar { height: 100%; border-radius: 4px; }
.rank { display: inline-flex; width: 22px; height: 22px; align-items: center; justify-content: center; border-radius: 50%; background: #F1F3F5; font-size: 12px; font-weight: 700; }
.rank.top { background: #B48B21; color: #fff; }
</style>
