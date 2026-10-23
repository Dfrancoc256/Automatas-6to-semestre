<template>
  <div class="stats-page">
    <header class="stats-hero">
      <div><p>Supervisión académica</p><h1>Estadísticas del sistema</h1><span>Indicadores operativos, uso de la plataforma y actividad reciente.</span></div>
      <div class="stats-hero__actions">
        <v-btn-toggle v-model="dias" mandatory density="comfortable" variant="outlined" divided><v-btn :value="7">7 días</v-btn><v-btn :value="30">30 días</v-btn><v-btn :value="90">90 días</v-btn></v-btn-toggle>
        <v-btn icon="mdi-refresh" variant="tonal" :loading="cargando" aria-label="Actualizar estadísticas" @click="cargar" />
      </div>
    </header>

    <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mt-5">{{ error }}</v-alert>
    <v-skeleton-loader v-else-if="cargando && !detalle" class="mt-5" type="card, card, card" />

    <template v-else-if="detalle">
      <section class="stats-kpis" aria-label="Indicadores del periodo">
        <article v-for="kpi in kpis" :key="kpi.etiqueta" class="stats-kpi"><span class="stats-kpi__icon"><v-icon>{{ kpi.icono }}</v-icon></span><div><strong>{{ kpi.valor }}</strong><span>{{ kpi.etiqueta }}</span><small>{{ kpi.descripcion }}</small></div></article>
      </section>

      <section class="stats-grid stats-grid--main">
        <v-card class="stats-card stats-card--wide" elevation="0">
          <div class="stats-card__header"><div><p>Tendencia</p><h2>Análisis realizados</h2></div><span>{{ rango }}</span></div>
          <div class="trend-chart" role="img" aria-label="Tendencia de análisis por día"><div v-for="punto in tendencia" :key="punto.fecha" class="trend-chart__bar" :title="`${punto.etiqueta}: ${punto.cantidad} análisis`"><span :style="{ height: `${punto.porcentaje}%` }" /></div></div>
          <div class="trend-chart__footer"><span>{{ tendencia[0]?.etiqueta }}</span><span>{{ tendencia.at(-1)?.etiqueta }}</span></div>
        </v-card>

        <v-card class="stats-card" elevation="0">
          <div class="stats-card__header"><div><p>Seguridad</p><h2>Actividad de acceso</h2></div><v-icon color="accent">mdi-shield-check-outline</v-icon></div>
          <div class="access-summary"><strong>{{ detalle.indicadores.tasaExito }}%</strong><span>Tasa de accesos exitosos</span></div>
          <v-progress-linear :model-value="detalle.indicadores.tasaExito" color="accent" bg-color="#e6edf1" height="8" rounded />
          <div class="access-summary__items"><span><b>{{ detalle.indicadores.accesosExitosos }}</b> exitosos</span><span><b>{{ detalle.indicadores.accesosFallidos }}</b> fallidos</span></div>
          <div class="mini-access-chart"><i v-for="punto in accesos" :key="punto.fecha" :style="{ height: `${punto.porcentaje}%` }" /></div>
        </v-card>

        <v-card class="stats-card" elevation="0">
          <div class="stats-card__header"><div><p>Distribución</p><h2>Análisis por idioma</h2></div><v-icon color="primary">mdi-translate</v-icon></div>
          <div v-if="detalle.analisisPorIdioma.length" class="stats-list"><div v-for="idioma in detalle.analisisPorIdioma" :key="idioma.idioma" class="stats-list__item"><div><span class="text-capitalize">{{ idioma.idioma }}</span><b>{{ idioma.cantidad }}</b></div><v-progress-linear :model-value="porcentaje(idioma.cantidad, maxIdioma)" color="primary" bg-color="#e6edf1" height="6" rounded /></div></div>
          <p v-else class="stats-empty">Sin análisis en este periodo.</p>
        </v-card>

        <v-card class="stats-card" elevation="0">
          <div class="stats-card__header"><div><p>Usuarios</p><h2>Cuentas activas por rol</h2></div><v-icon color="accent">mdi-account-group-outline</v-icon></div>
          <div v-if="detalle.usuariosPorRol.length" class="stats-list"><div v-for="rol in detalle.usuariosPorRol" :key="rol.rol" class="stats-list__item"><div><span class="text-capitalize">{{ rol.rol.toLowerCase() }}</span><b>{{ rol.cantidad }}</b></div><v-progress-linear :model-value="porcentaje(rol.cantidad, maxRol)" color="accent" bg-color="#e6edf1" height="6" rounded /></div></div>
          <p v-else class="stats-empty">No hay usuarios activos.</p><div class="face-coverage"><v-icon size="18">mdi-face-recognition</v-icon><span>{{ detalle.indicadores.usuariosConRostro }} cuentas con rostro registrado</span></div>
        </v-card>
      </section>

      <section class="stats-grid stats-grid--tables">
        <v-card class="stats-card" elevation="0"><div class="stats-card__header"><div><p>Rendimiento</p><h2>Usuarios más activos</h2></div><v-icon color="primary">mdi-trophy-outline</v-icon></div><div v-if="detalle.topAnalistas.length" class="ranking-list"><div v-for="(usuario, indice) in detalle.topAnalistas" :key="usuario.usuario" class="ranking-list__item"><b>{{ indice + 1 }}</b><span>{{ usuario.usuario }}</span><strong>{{ usuario.analisis }} análisis</strong></div></div><p v-else class="stats-empty">No hay actividad para clasificar.</p></v-card>
        <v-card class="stats-card" elevation="0"><div class="stats-card__header"><div><p>Monitoreo</p><h2>Alertas de seguridad</h2></div><v-icon color="error">mdi-shield-alert-outline</v-icon></div><div v-if="detalle.alertasIp.length" class="ranking-list"><div v-for="alerta in detalle.alertasIp" :key="alerta.ip" class="ranking-list__item"><v-icon size="18" color="warning">mdi-alert-outline</v-icon><span class="stats-ip">{{ alerta.ip }}</span><strong>{{ alerta.fallidos }} fallidos</strong></div></div><p v-else class="stats-empty"><v-icon color="success" size="18">mdi-check-circle-outline</v-icon> No se detectaron fallos de acceso.</p></v-card>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN', 'SUPERVISOR'] })
useHead({ title: 'Estadísticas' })
const { api } = useApi()
const dias = ref(30), cargando = ref(true), error = ref(''), detalle = ref<any>(null)
const formato = new Intl.NumberFormat('es-GT', { maximumFractionDigits: 1 })
const fechaCorta = new Intl.DateTimeFormat('es-GT', { day: '2-digit', month: 'short' })
const rango = computed(() => detalle.value ? `${fechaCorta.format(new Date(detalle.value.desde))} — ${fechaCorta.format(new Date(detalle.value.hasta))}` : '')
const maxIdioma = computed(() => Math.max(1, ...((detalle.value?.analisisPorIdioma || []).map((item: any) => item.cantidad))))
const maxRol = computed(() => Math.max(1, ...((detalle.value?.usuariosPorRol || []).map((item: any) => item.cantidad))))
const tendencia = computed(() => { const puntos = detalle.value?.analisisPorDia || []; const maximo = Math.max(1, ...puntos.map((item: any) => item.cantidad)); return puntos.map((item: any) => ({ ...item, etiqueta: fechaCorta.format(new Date(item.fecha)), porcentaje: Math.max(4, item.cantidad * 100 / maximo) })) })
const accesos = computed(() => { const puntos = detalle.value?.accesosPorDia || []; const maximo = Math.max(1, ...puntos.map((item: any) => item.exitosos + item.fallidos)); return puntos.map((item: any) => ({ ...item, porcentaje: Math.max(3, (item.exitosos + item.fallidos) * 100 / maximo) })) })
const kpis = computed(() => detalle.value ? [
  { etiqueta: 'Análisis procesados', valor: formato.format(detalle.value.indicadores.analisis), descripcion: `${formato.format(detalle.value.indicadores.palabras)} palabras`, icono: 'mdi-file-chart-outline' },
  { etiqueta: 'Usuarios activos', valor: formato.format(detalle.value.indicadores.usuarios), descripcion: `${detalle.value.indicadores.nuevosUsuarios} nuevos en el periodo`, icono: 'mdi-account-group-outline' },
  { etiqueta: 'Accesos registrados', valor: formato.format(detalle.value.indicadores.accesos), descripcion: `${detalle.value.indicadores.tasaExito}% de éxito`, icono: 'mdi-login-variant' },
  { etiqueta: 'Cuentas con rostro', valor: formato.format(detalle.value.indicadores.usuariosConRostro), descripcion: 'Listas para acceso facial', icono: 'mdi-face-recognition' }
] : [])
function porcentaje(valor: number, maximo: number) { return Math.max(3, valor * 100 / maximo) }
async function cargar() { cargando.value = true; error.value = ''; try { const { data } = await api.get('/dashboard/estadisticas/detalle', { params: { dias: dias.value } }); detalle.value = data } catch (e: any) { error.value = e.response?.data?.mensaje || 'No se pudieron cargar las estadísticas.' } finally { cargando.value = false } }
watch(dias, cargar)
onMounted(cargar)
</script>

<style scoped>
.stats-page{max-width:1480px;margin:0 auto;padding:clamp(1rem,2vw,2rem);color:#102c42}.stats-hero{display:flex;justify-content:space-between;align-items:center;gap:1.5rem;padding:clamp(1.4rem,3vw,2.1rem);border-radius:18px;color:#fff;background:linear-gradient(120deg,#061b2c,#0b3d60)}.stats-hero p,.stats-card__header p{margin:0 0 .35rem;color:#e0b333;font-size:.72rem;font-weight:800;letter-spacing:.1em;text-transform:uppercase}.stats-hero h1{margin:0;font-size:clamp(1.5rem,3vw,2.1rem)}.stats-hero span{display:block;margin-top:.55rem;color:#d6e5ee}.stats-hero__actions{display:flex;gap:.6rem;align-items:center}.stats-hero :deep(.v-btn){color:#fff;border-color:rgba(255,255,255,.42)}.stats-hero :deep(.v-btn--active){color:#082a43;background:#fff}.stats-kpis{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:1rem;margin:1.25rem 0}.stats-kpi,.stats-card{border:1px solid #dce6ed;background:#fff;box-shadow:none}.stats-kpi{display:flex;gap:.8rem;align-items:center;padding:1rem;border-radius:13px}.stats-kpi__icon{display:grid;place-items:center;width:42px;height:42px;border-radius:11px;color:#075e93;background:#e7f1f8}.stats-kpi strong,.stats-kpi span,.stats-kpi small{display:block}.stats-kpi strong{font-size:1.25rem;line-height:1.1}.stats-kpi span{margin-top:.25rem;font-size:.78rem;font-weight:700}.stats-kpi small{margin-top:.16rem;color:#6a7d8c;font-size:.7rem}.stats-grid{display:grid;gap:1.25rem;margin-top:1.25rem}.stats-grid--main{grid-template-columns:repeat(3,minmax(0,1fr))}.stats-grid--tables{grid-template-columns:repeat(2,minmax(0,1fr))}.stats-card{padding:1.25rem;border-radius:14px}.stats-card--wide{grid-column:span 2}.stats-card__header{display:flex;justify-content:space-between;gap:.75rem;margin-bottom:1.2rem}.stats-card__header h2{margin:0;font-size:1.03rem}.trend-chart{display:flex;gap:clamp(2px,.5vw,7px);align-items:end;height:165px;padding:8px 0 3px;border-bottom:1px solid #dce6ed}.trend-chart__bar{flex:1;height:100%;display:flex;align-items:end;min-width:3px}.trend-chart__bar span{width:100%;min-height:3px;border-radius:5px 5px 0 0;background:linear-gradient(#1881bd,#0b4d7c)}.trend-chart__footer,.access-summary__items{display:flex;justify-content:space-between;margin-top:.6rem;color:#708292;font-size:.73rem}.access-summary{display:grid;gap:.1rem;margin:.55rem 0 .85rem}.access-summary strong{font-size:2rem;color:#082a43}.access-summary span{color:#607688;font-size:.83rem}.access-summary__items b{color:#16364d}.mini-access-chart{display:flex;align-items:end;gap:3px;height:48px;margin-top:.9rem}.mini-access-chart i{flex:1;min-width:2px;background:#c49318;border-radius:3px 3px 0 0}.stats-list{display:grid;gap:.9rem}.stats-list__item>div{display:flex;justify-content:space-between;margin-bottom:.35rem;color:#42596c;font-size:.83rem}.stats-list__item b{color:#0a2b43}.face-coverage{display:flex;align-items:center;gap:.45rem;margin-top:1.2rem;padding-top:.9rem;border-top:1px solid #e8eef2;color:#64798a;font-size:.79rem}.face-coverage .v-icon{color:#b88710}.ranking-list{display:grid}.ranking-list__item{display:flex;align-items:center;gap:.7rem;min-height:38px;border-bottom:1px solid #e9eef1;font-size:.84rem}.ranking-list__item:last-child{border:0}.ranking-list__item>:first-child{color:#b88710}.ranking-list__item span{flex:1}.ranking-list__item strong{color:#19394e;font-size:.78rem}.stats-ip{font-family:ui-monospace,SFMono-Regular,Menlo,monospace}.stats-empty{display:flex;align-items:center;justify-content:center;gap:.35rem;min-height:100px;margin:0;color:#738697;font-size:.84rem;text-align:center}@media(max-width:1050px){.stats-kpis{grid-template-columns:repeat(2,minmax(0,1fr))}.stats-grid--main{grid-template-columns:repeat(2,minmax(0,1fr))}.stats-card--wide{grid-column:span 2}}@media(max-width:700px){.stats-page{padding:1rem}.stats-hero{display:grid}.stats-hero__actions{align-items:flex-start;flex-wrap:wrap}.stats-grid--main,.stats-grid--tables{grid-template-columns:1fr}.stats-card--wide{grid-column:auto}.stats-kpis{grid-template-columns:1fr}.trend-chart{height:130px}.stats-hero :deep(.v-btn){font-size:.72rem}}
</style>
