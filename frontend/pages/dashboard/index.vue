<template>
  <div class="executive-dashboard">
    <section class="executive-hero" aria-labelledby="dashboard-title">
      <div>
        <p class="executive-hero__eyebrow">Panel de trabajo</p>
        <h1 id="dashboard-title">Buen día, {{ nombreUsuario }}.</h1>
        <p>Gestiona tus análisis léxicos y consulta la actividad de tu espacio académico.</p>
      </div>
      <div class="executive-hero__actions">
        <span class="executive-hero__date"><v-icon size="16">mdi-calendar-blank-outline</v-icon>{{ fechaActual }}</span>
        <v-btn class="umg-gold-button" prepend-icon="mdi-plus" @click="navigateTo('/dashboard/analisis')">Nuevo análisis</v-btn>
      </div>
    </section>

    <template v-if="auth.isSupervisor && stats">
      <section class="executive-metrics" aria-label="Indicadores generales">
        <article class="executive-metric">
          <span class="executive-metric__icon"><v-icon>mdi-file-chart-outline</v-icon></span>
          <div><strong>{{ stats.totalAnalisis.toLocaleString() }}</strong><span>Análisis procesados</span></div>
        </article>
        <article class="executive-metric">
          <span class="executive-metric__icon"><v-icon>mdi-account-group-outline</v-icon></span>
          <div><strong>{{ stats.totalUsuarios.toLocaleString() }}</strong><span>Usuarios activos</span></div>
        </article>
        <article class="executive-metric">
          <span class="executive-metric__icon"><v-icon>mdi-calendar-today</v-icon></span>
          <div><strong>{{ stats.analisisHoy.toLocaleString() }}</strong><span>Análisis de hoy</span></div>
        </article>
        <article class="executive-metric">
          <span class="executive-metric__icon"><v-icon>mdi-login-variant</v-icon></span>
          <div><strong>{{ stats.loginsHoy.toLocaleString() }}</strong><span>Accesos de hoy</span></div>
        </article>
      </section>

      <section class="executive-insights">
        <v-card class="executive-panel executive-panel--languages" elevation="0">
          <div class="executive-panel__heading">
            <div><p>Distribución</p><h2>Análisis por idioma</h2></div>
            <v-icon>mdi-translate</v-icon>
          </div>
          <div v-if="stats.porIdioma?.length" class="language-list">
            <div v-for="item in stats.porIdioma" :key="item.idioma" class="language-row">
              <div class="language-row__label"><span class="text-capitalize">{{ item.idioma }}</span><strong>{{ item.cantidad }}</strong></div>
              <v-progress-linear :model-value="(item.cantidad / (stats.totalAnalisis || 1)) * 100" color="accent" bg-color="#E7ECF0" height="5" rounded />
            </div>
          </div>
          <p v-else class="executive-empty">No hay análisis registrados todavía.</p>
        </v-card>

        <v-card class="executive-panel executive-panel--recent" elevation="0">
          <div class="executive-panel__heading">
            <div><p>Actividad reciente</p><h2>Últimos análisis</h2></div>
            <v-btn variant="text" size="small" append-icon="mdi-arrow-right" @click="navigateTo('/dashboard/historial')">Ver historial</v-btn>
          </div>
          <div v-if="stats.recientes?.length" class="recent-list">
            <div v-for="r in stats.recientes.slice(0, 5)" :key="r.id" class="recent-item">
              <span class="recent-item__icon"><v-icon size="18">mdi-file-document-outline</v-icon></span>
              <div class="recent-item__file"><strong>{{ r.nombreArchivo }}</strong><span>{{ r.usuario }} · {{ formatFecha(r.fechaAnalisis) }}</span></div>
              <v-chip size="x-small" variant="tonal" color="primary">{{ r.idioma }}</v-chip>
              <strong class="recent-item__words">{{ r.totalPalabras.toLocaleString() }}</strong>
            </div>
          </div>
          <p v-else class="executive-empty">No hay actividad reciente.</p>
        </v-card>
      </section>
    </template>

    <section class="executive-modules" aria-labelledby="modulos-disponibles">
      <div class="executive-section-heading">
        <div><p>Herramientas</p><h2 id="modulos-disponibles">Accesos de trabajo</h2></div>
        <span>{{ modulos.length }} módulos disponibles</span>
      </div>
      <div class="executive-module-grid">
        <v-card v-for="modulo in modulos" :key="modulo.ruta" class="executive-module" :class="{ 'executive-module--primary': modulo.primario }" elevation="0">
          <div class="executive-module__top">
            <span class="executive-module__icon" :class="`executive-module__icon--${modulo.color}`"><v-icon size="22">{{ modulo.icono }}</v-icon></span>
            <span class="executive-module__category">{{ modulo.categoria }}</span>
          </div>
          <h3>{{ modulo.titulo }}</h3>
          <p>{{ modulo.descripcion }}</p>
          <v-btn :class="modulo.primario ? 'umg-gold-button' : ''" :variant="modulo.primario ? 'flat' : 'text'" :color="modulo.primario ? 'accent' : 'primary'" append-icon="mdi-arrow-right" @click="navigateTo(modulo.ruta)">
            {{ modulo.accion }}
          </v-btn>
        </v-card>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'Dashboard' })

const auth    = useAuthStore()
const { api } = useApi()
const stats   = ref<any>(null)
const nombreUsuario = computed(() => auth.user?.nickname || 'usuario')
const fechaActual = computed(() => new Intl.DateTimeFormat('es-GT', {
  day: '2-digit', month: 'long', year: 'numeric'
}).format(new Date()))

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

<style scoped>
.executive-dashboard {
  max-width: 1480px;
  margin: 0 auto;
  padding: clamp(1.1rem, 2.4vw, 2.25rem);
}

.executive-hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1.5rem;
  padding: clamp(1.5rem, 3.5vw, 2.75rem);
  border-radius: 18px;
  color: #fff;
  background: linear-gradient(120deg, #061b2c 0%, #0a3452 100%);
  box-shadow: 0 12px 30px rgba(4, 31, 49, .16);
}

.executive-hero__eyebrow,
.executive-panel__heading p,
.executive-section-heading p,
.executive-module__category {
  margin: 0 0 .38rem;
  font-size: .72rem;
  font-weight: 800;
  letter-spacing: .1em;
  text-transform: uppercase;
}

.executive-hero__eyebrow { color: #e1b22a; }
.executive-hero h1 { margin: 0; font-size: clamp(1.7rem, 3vw, 2.25rem); line-height: 1.1; }
.executive-hero > div > p:not(.executive-hero__eyebrow) { max-width: 630px; margin: .7rem 0 0; color: #d9e5ec; }
.executive-hero__actions { display: flex; flex-direction: column; align-items: flex-end; gap: .8rem; flex-shrink: 0; }
.executive-hero__date { display: inline-flex; gap: .4rem; align-items: center; color: #d9e5ec; font-size: .84rem; text-transform: capitalize; }

.executive-metrics {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
  margin: 1.3rem 0;
}

.executive-metric,
.executive-panel,
.executive-module {
  border: 1px solid #dce5eb;
  background: #fff;
  box-shadow: none;
}

.executive-metric {
  display: flex;
  align-items: center;
  gap: .85rem;
  padding: 1.05rem;
  border-radius: 13px;
}

.executive-metric__icon,
.recent-item__icon,
.executive-module__icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  color: #0c4f7e;
  background: #e6f0f7;
}

.executive-metric__icon { width: 40px; height: 40px; border-radius: 10px; }
.executive-metric strong { display: block; color: #06213a; font-size: 1.27rem; line-height: 1.1; }
.executive-metric span:not(.executive-metric__icon) { display: block; margin-top: .25rem; color: #607386; font-size: .77rem; }

.executive-insights { display: grid; grid-template-columns: minmax(270px, .85fr) minmax(0, 1.65fr); gap: 1.3rem; margin-bottom: 1.8rem; }
.executive-panel { padding: 1.35rem; border-radius: 14px; }
.executive-panel__heading,
.executive-section-heading,
.executive-module__top,
.language-row__label,
.recent-item { display: flex; align-items: center; justify-content: space-between; gap: .8rem; }
.executive-panel__heading { margin-bottom: 1.25rem; }
.executive-panel__heading h2,
.executive-section-heading h2 { margin: 0; color: #08233c; font-size: 1.14rem; }
.executive-panel__heading p,
.executive-section-heading p { color: #a67a08; }
.executive-panel__heading > .v-icon { color: #b88914; }
.language-list { display: grid; gap: 1rem; }
.language-row__label { margin-bottom: .38rem; color: #41566b; font-size: .84rem; }
.language-row__label strong { color: #092842; }
.recent-list { display: grid; }
.recent-item { min-height: 57px; padding: .6rem 0; border-bottom: 1px solid #e9eef1; }
.recent-item:last-child { border-bottom: 0; }
.recent-item__icon { width: 32px; height: 32px; border-radius: 8px; }
.recent-item__file { display: grid; flex: 1; min-width: 0; }
.recent-item__file strong { overflow: hidden; color: #18334b; font-size: .88rem; text-overflow: ellipsis; white-space: nowrap; }
.recent-item__file span { margin-top: .2rem; color: #738494; font-size: .74rem; }
.recent-item__words { color: #496175; font-size: .8rem; }
.executive-empty { margin: 1.7rem 0; color: #718294; font-size: .88rem; text-align: center; }

.executive-section-heading { margin-bottom: 1rem; }
.executive-section-heading > span { color: #687a8b; font-size: .82rem; }
.executive-module-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1rem; }
.executive-module { display: flex; flex-direction: column; align-items: flex-start; min-height: 238px; padding: 1.25rem; border-radius: 14px; }
.executive-module--primary { border-color: #d5b253; background: linear-gradient(155deg, #fffdf7, #fff); }
.executive-module__icon { width: 43px; height: 43px; border-radius: 11px; }
.executive-module__icon--accent { color: #a87907; background: #fff4d9; }
.executive-module__category { color: #718294; font-size: .63rem; }
.executive-module h3 { margin: 1rem 0 .45rem; color: #06213a; font-size: 1.05rem; }
.executive-module p { flex: 1; margin: 0 0 1rem; color: #607386; font-size: .86rem; line-height: 1.55; }
.executive-module :deep(.v-btn) { min-width: 0; padding-inline: .35rem; font-size: .77rem; font-weight: 800; letter-spacing: .04em; }
.executive-module :deep(.umg-gold-button) { padding-inline: 1rem; }

@media (max-width: 1050px) {
  .executive-metrics { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .executive-module-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

@media (max-width: 760px) {
  .executive-dashboard { padding: 1rem; }
  .executive-hero, .executive-insights { grid-template-columns: 1fr; }
  .executive-hero { display: grid; }
  .executive-hero__actions { align-items: flex-start; }
  .executive-panel { padding: 1.05rem; }
}

@media (max-width: 520px) {
  .executive-metrics, .executive-module-grid { grid-template-columns: 1fr; }
  .executive-hero { border-radius: 13px; }
  .executive-section-heading { align-items: flex-start; flex-direction: column; }
  .recent-item__words { display: none; }
  .recent-item { gap: .55rem; }
}
</style>
