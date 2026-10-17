<template>
  <div>
    <div class="analysis-page-title d-flex align-center mb-5">
      <v-icon color="accent" class="mr-3">mdi-text-search</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Análisis Léxico</h1>
        <p class="text-body-2 text-medium-emphasis">Procesa un archivo de texto y consulta sus resultados.</p>
      </div>
    </div>

    <v-row>
      <v-col cols="12" md="4">
        <v-card class="analysis-config-card pa-5">
          <h2 class="text-subtitle-1 font-weight-bold mb-4">Archivo de texto</h2>

          <div class="upload-zone mb-4"
               :class="{ 'upload-zone--active': arrastrando }"
               @dragover.prevent="arrastrando = true"
               @dragleave="arrastrando = false"
               @drop.prevent="onDrop"
               @click="fileInput?.click()">
            <v-icon size="40" color="accent" class="mb-2">mdi-file-upload-outline</v-icon>
            <p class="text-body-2 font-weight-medium">
              {{ archivo ? archivo.name : 'Arrastra tu archivo .txt aquí' }}
            </p>
            <p class="text-caption text-medium-emphasis">o haz clic para seleccionar</p>
            <input ref="fileInput" type="file" accept=".txt" style="display:none"
                   @change="onFileSelect" />
          </div>

          <v-select
            v-model="idioma"
            label="Idioma del texto"
            prepend-inner-icon="mdi-translate"
            :items="idiomas"
            item-title="label"
            item-value="value"
            density="compact"
            hide-details
            class="mb-4"
          />

          <v-textarea
            v-if="contenido"
            :model-value="contenido.substring(0, 500) + (contenido.length > 500 ? '...' : '')"
            label="Vista previa"
            readonly
            rows="3"
            density="compact"
            class="mb-4"
          />

          <v-alert v-if="errorMsg" type="error" variant="tonal" density="compact" class="mb-4">
            {{ errorMsg }}
          </v-alert>

          <v-btn
            color="accent"
            class="umg-gold-button"
            block
            size="large"
            :loading="procesando"
            :disabled="!contenido"
            prepend-icon="mdi-play"
            @click="procesar"
          >
            Procesar análisis
          </v-btn>
        </v-card>
      </v-col>

      <v-col cols="12" md="8">
        <div v-if="!resultado" class="analisis-empty-state d-flex flex-column align-center justify-center">
          <v-icon size="80" color="grey-lighten-2" class="mb-4">mdi-text-box-search-outline</v-icon>
          <p class="text-h6 font-weight-medium">Esperando archivo</p>
          <p class="text-body-2">Carga un archivo .txt y presiona "Procesar"</p>
        </div>

        <template v-else>
          <div class="analysis-result-context mb-4">
            <div class="analysis-result-context__file">
              <v-icon size="20">mdi-file-document-outline</v-icon>
              <span>{{ resultado.nombreArchivo }}</span>
            </div>
            <div class="analysis-result-context__actions">
              <span class="analysis-result-context__language">{{ nombreIdioma(resultado.idioma) }}</span>
              <v-btn
                size="small"
                variant="outlined"
                color="primary"
                prepend-icon="mdi-download-outline"
                :loading="descargandoReporte"
                :disabled="!resultado.analisisId"
                @click="descargarReporte"
              >
                Descargar reporte
              </v-btn>
            </div>
          </div>

          <v-card class="analysis-summary mb-4">
            <v-row no-gutters>
              <v-col v-for="m in metricas" :key="m.label" cols="6" sm="3">
                <div class="analysis-metric pa-3">
                <v-icon size="18" :color="m.color">{{ m.icono }}</v-icon>
                <div>
                  <div class="analysis-metric__value">{{ m.valor }}</div>
                  <div class="analysis-metric__label">{{ m.label }}</div>
                </div>
                </div>
              </v-col>
            </v-row>
          </v-card>

          <v-card class="analysis-results-card">
            <v-tabs v-model="tabResultado" color="accent" density="compact">
              <v-tab value="frecuencia" prepend-icon="mdi-chart-bar">Frecuencia</v-tab>
              <v-tab value="clasificacion" prepend-icon="mdi-format-list-bulleted-type">Clasificación</v-tab>
              <v-tab value="patrones" prepend-icon="mdi-magnify-scan">Datos detectados</v-tab>
            </v-tabs>

            <v-divider />

            <v-window v-model="tabResultado">
              <v-window-item value="frecuencia" class="pa-4">
                <p class="analysis-tab-intro">Muestra las palabras que más se repiten y las que aparecen con menor frecuencia.</p>
                <v-row>
                  <v-col cols="12" md="6">
                    <h3 class="analysis-table-title"><v-icon size="18">mdi-arrow-up</v-icon> Más repetidas</h3>
                    <table class="tabla-frecuencia">
                      <thead>
                        <tr><th>Palabra</th><th>Apariciones</th></tr>
                      </thead>
                      <tbody>
                        <tr v-for="t in resultado.palabrasMasFrecuentes.slice(0,10)" :key="t.token">
                          <td><code>{{ t.token }}</code></td>
                          <td>
                            <v-chip size="x-small" color="accent" variant="tonal">{{ t.frecuencia }}</v-chip>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </v-col>
                  <v-col cols="12" md="6">
                    <h3 class="analysis-table-title"><v-icon size="18">mdi-arrow-down</v-icon> Menos repetidas</h3>
                    <table class="tabla-frecuencia">
                      <thead>
                        <tr><th>Palabra</th><th>Apariciones</th></tr>
                      </thead>
                      <tbody>
                        <tr v-for="t in resultado.palabrasMenosFrecuentes.slice(0,10)" :key="t.token">
                          <td><code>{{ t.token }}</code></td>
                          <td>
                            <v-chip size="x-small" color="secondary" variant="tonal">{{ t.frecuencia }}</v-chip>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </v-col>
                </v-row>
              </v-window-item>

              <v-window-item value="clasificacion" class="pa-4">
                <p class="analysis-tab-intro">Palabras agrupadas según las reglas del idioma seleccionado.</p>
                <v-row>
                  <v-col cols="12" sm="6" v-for="cat in categorias" :key="cat.key">
                    <div class="mb-4">
                      <div class="d-flex align-center mb-2">
                        <v-icon size="19" class="mr-2" :color="cat.color">{{ cat.icon }}</v-icon>
                        <span class="text-subtitle-2 font-weight-bold">{{ cat.label }}</span>
                        <v-chip size="x-small" class="ml-2" color="grey" variant="tonal">
                          {{ resultado[cat.key]?.length || 0 }}
                        </v-chip>
                      </div>
                      <div class="d-flex flex-wrap gap-1">
                        <v-chip v-for="w in (resultado[cat.key] || []).slice(0,15)" :key="w"
                                size="small" :color="cat.color" variant="tonal" class="resultado-chip">
                          {{ w }}
                        </v-chip>
                        <span v-if="!resultado[cat.key]?.length"
                              class="text-caption text-medium-emphasis">Sin resultados</span>
                      </div>
                    </div>
                  </v-col>
                </v-row>
              </v-window-item>

              <v-window-item value="patrones" class="pa-4">
                <p class="analysis-tab-intro">Elementos identificados directamente en el contenido del documento.</p>
                <div class="analysis-patterns">
                  <section class="analysis-pattern">
                    <h4><v-icon size="18">mdi-email-outline</v-icon> Correos electrónicos</h4>
                    <ul v-if="resultado.correosEncontrados?.length">
                      <li v-for="correo in resultado.correosEncontrados" :key="correo">{{ correo }}</li>
                    </ul>
                    <p v-else>Sin coincidencias.</p>
                  </section>
                  <section class="analysis-pattern">
                    <h4><v-icon size="18">mdi-link-variant</v-icon> Enlaces web</h4>
                    <ul v-if="resultado.urlsEncontradas?.length">
                      <li v-for="url in resultado.urlsEncontradas" :key="url">
                        <a :href="url" target="_blank" rel="noopener noreferrer">{{ url }}</a>
                      </li>
                    </ul>
                    <p v-else>Sin coincidencias.</p>
                  </section>
                  <section class="analysis-pattern">
                    <h4><v-icon size="18">mdi-calendar-outline</v-icon> Fechas</h4>
                    <ul v-if="resultado.fechasEncontradas?.length">
                      <li v-for="fecha in resultado.fechasEncontradas" :key="fecha">{{ fecha }}</li>
                    </ul>
                    <p v-else>Sin coincidencias.</p>
                  </section>
                  <section class="analysis-pattern">
                    <h4><v-icon size="18">mdi-numeric</v-icon> Números</h4>
                    <ul v-if="resultado.numeros?.length">
                      <li v-for="numero in resultado.numeros.slice(0, 10)" :key="numero">{{ numero }}</li>
                    </ul>
                    <p v-else>Sin coincidencias.</p>
                  </section>
                </div>
              </v-window-item>
            </v-window>
          </v-card>
        </template>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN', 'SUPERVISOR', 'ANALISTA'] })
useHead({ title: 'Análisis Léxico' })

const { api } = useApi()

const archivo    = ref<File | null>(null)
const contenido  = ref('')
const idioma     = ref('español')
const procesando = ref(false)
const errorMsg   = ref('')
const arrastrando = ref(false)
const resultado  = ref<any>(null)
const tabResultado = ref('frecuencia')
const fileInput  = ref<HTMLInputElement | null>(null)
const descargandoReporte = ref(false)

const idiomas = [
  { label: '🇪🇸 Español', value: 'español' },
  { label: '🇺🇸 Inglés',  value: 'inglés' },
  { label: '🇷🇺 Ruso',    value: 'ruso' },
  { label: '🇨🇳 Chino',   value: 'chino' },
  { label: '🇸🇦 Árabe',   value: 'árabe' }
]

const categorias = [
  { key: 'pronombres', label: 'Pronombres personales', icon: 'mdi-account-voice', color: 'indigo' },
  { key: 'verbos', label: 'Verbos', icon: 'mdi-run', color: 'green' },
  { key: 'sustantivos', label: 'Sustantivos', icon: 'mdi-tag-outline', color: 'blue' },
  { key: 'adjetivos', label: 'Adjetivos', icon: 'mdi-format-color-text', color: 'pink' },
  { key: 'nombresPersonas', label: 'Nombres de personas', icon: 'mdi-account-outline', color: 'amber' },
  { key: 'conectores', label: 'Conectores', icon: 'mdi-link-variant', color: 'cyan' },
  { key: 'preposiciones', label: 'Preposiciones', icon: 'mdi-map-marker-outline', color: 'deep-purple' },
]

const metricas = computed(() => resultado.value ? [
  { label: 'Palabras', valor: resultado.value.totalPalabras.toLocaleString(), color: 'accent', icono: 'mdi-format-letter-case' },
  { label: 'Oraciones', valor: resultado.value.totalOraciones.toLocaleString(), color: 'accent', icono: 'mdi-text-long' },
  { label: 'Párrafos', valor: resultado.value.totalParrafos.toLocaleString(), color: 'accent', icono: 'mdi-format-align-left' },
  { label: 'Promedio por palabra', valor: resultado.value.promedioLongitudPalabra + ' caracteres', color: 'accent', icono: 'mdi-ruler' }
] : [])

function nombreIdioma(valor: string) {
  return idiomas.find((item) => item.value === valor)?.label || valor
}

function onFileSelect(e: Event) {
  const f = (e.target as HTMLInputElement).files?.[0]
  if (f) cargarArchivo(f)
}

function onDrop(e: DragEvent) {
  arrastrando.value = false
  const f = e.dataTransfer?.files?.[0]
  if (f) cargarArchivo(f)
}

function cargarArchivo(f: File) {
  if (!f.name.endsWith('.txt')) {
    errorMsg.value = 'Solo se permiten archivos .txt'
    return
  }
  archivo.value  = f
  errorMsg.value = ''
  const reader = new FileReader()
  reader.onload = (e) => { contenido.value = e.target?.result as string }
  reader.readAsText(f, 'UTF-8')
}

async function procesar() {
  errorMsg.value = ''
  procesando.value = true
  try {
    const { data } = await api.post('/analisis/procesar', {
      contenido: contenido.value,
      idioma: idioma.value,
      nombreArchivo: archivo.value?.name || 'archivo.txt'
    })
    resultado.value = data
  } catch (e: any) {
    errorMsg.value = e.response?.data?.mensaje || 'Error al procesar el archivo.'
  } finally {
    procesando.value = false
  }
}

async function descargarReporte() {
  if (!resultado.value?.analisisId) return

  descargandoReporte.value = true
  try {
    const { data } = await api.get('/analisis/' + resultado.value.analisisId + '/reporte', {
      responseType: 'blob'
    })
    const url = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
    const enlace = document.createElement('a')
    enlace.href = url
    enlace.download = 'reporte-' + (resultado.value.nombreArchivo || 'analisis').replace(/\.txt$/i, '') + '.pdf'
    enlace.click()
    URL.revokeObjectURL(url)
  } catch (e: any) {
    errorMsg.value = e.response?.data?.mensaje || 'No se pudo descargar el reporte.'
  } finally {
    descargandoReporte.value = false
  }
}
</script>

<style scoped>
.upload-zone {
  border: 2px dashed #CBD5E1;
  border-radius: 12px;
  padding: 24px 16px;
  text-align: center;
  cursor: pointer;
  transition: border-color .2s, background .2s;
  background: #F8FAFC;
}
.upload-zone:hover,
.upload-zone--active {
  border-color: #B48B21;
  background: #FFF8E1;
}
.gap-1 { gap: 4px; }
.analysis-page-title { min-height: 62px; }
.analysis-config-card { box-shadow: none !important; border: 1px solid #E5EAF0 !important; }

.analysis-result-context {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 14px;
  border: 1px solid #E2E8F0;
  border-radius: 9px;
  background: #FFFFFF;
}

.analysis-result-context__file {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: 8px;
  color: #17324D;
  font-size: 13px;
  font-weight: 650;
}

.analysis-result-context__file span { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.analysis-result-context__file .v-icon { color: #B48B21; }
.analysis-result-context__actions { display: flex; flex: 0 0 auto; align-items: center; gap: 12px; }
.analysis-result-context__language {
  flex: 0 0 auto;
  color: #506174;
  font-size: 12px;
  text-transform: capitalize;
}

.analysis-summary { overflow: hidden; border: 1px solid #E2E8F0 !important; box-shadow: none !important; }
.analysis-summary :deep(.v-col) + :deep(.v-col) { border-left: 1px solid #E2E8F0; }
.analysis-metric { display: flex; min-height: 72px; align-items: center; gap: 10px; text-align: left; }
.analysis-metric__value { color: #17324D; font-size: 17px; font-weight: 700; line-height: 1.15; }
.analysis-metric__label { margin-top: 2px; color: #64748B; font-size: 11px; line-height: 1.25; }
.analysis-results-card { border: 1px solid #E2E8F0 !important; box-shadow: none !important; }
.analysis-tab-intro { margin: 0 0 18px; color: #64748B; font-size: 13px; line-height: 1.5; }
.analysis-table-title { display: flex; align-items: center; gap: 7px; margin: 0 0 10px; color: #17324D; font-size: 14px; font-weight: 700; }
.analysis-table-title .v-icon { color: #B48B21; }

.analysis-patterns {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.analysis-pattern {
  min-height: 116px;
  padding: 14px;
  border: 1px solid #E2E8F0;
  border-radius: 8px;
  background: #FFFFFF;
}

.analysis-pattern h4 {
  display: flex;
  align-items: center;
  gap: 7px;
  margin: 0 0 11px;
  color: #17324D;
  font-size: 13px;
  font-weight: 700;
}

.analysis-pattern h4 .v-icon { color: #B48B21; }
.analysis-pattern p { margin: 0; color: #7B8794; font-size: 13px; }
.analysis-pattern ul { display: grid; gap: 6px; padding: 0; margin: 0; list-style: none; }
.analysis-pattern li { overflow: hidden; color: #405064; font-family: ui-monospace, SFMono-Regular, Menlo, monospace; font-size: 12px; line-height: 1.4; text-overflow: ellipsis; white-space: nowrap; }
.analysis-pattern a { color: #1A5080; text-decoration: none; }
.analysis-pattern a:hover { text-decoration: underline; }

@media (max-width: 600px) {
  .analysis-patterns { grid-template-columns: 1fr; }
  .analysis-result-context { align-items: flex-start; flex-direction: column; }
  .analysis-result-context__actions { width: 100%; justify-content: space-between; }
  .analysis-summary :deep(.v-col):nth-child(odd) { border-left: 0; }
  .analysis-summary :deep(.v-col):nth-child(n + 3) { border-top: 1px solid #E2E8F0; }
}
</style>
