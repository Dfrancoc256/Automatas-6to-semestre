<template>
  <div>
    <div class="d-flex align-center mb-6">
      <v-icon color="accent" class="mr-3">mdi-text-search</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Análisis Léxico</h1>
        <p class="text-body-2 text-medium-emphasis">Carga un archivo .txt para analizarlo</p>
      </div>
    </div>

    <v-row>
      <!-- Panel de carga -->
      <v-col cols="12" md="5">
        <v-card class="pa-5">
          <h2 class="text-subtitle-1 font-weight-bold mb-4">Configuración</h2>

          <!-- Zona de carga -->
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

          <!-- Selector de idioma -->
          <v-select
            v-model="idioma"
            label="Idioma del texto"
            prepend-inner-icon="mdi-translate"
            :items="idiomas"
            item-title="label"
            item-value="value"
            class="mb-4"
          />

          <!-- Vista previa del texto -->
          <v-textarea
            v-if="contenido"
            :model-value="contenido.substring(0, 500) + (contenido.length > 500 ? '...' : '')"
            label="Vista previa"
            readonly
            rows="4"
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

      <!-- Resultados -->
      <v-col cols="12" md="7">
        <div v-if="!resultado" class="d-flex flex-column align-center justify-center"
             style="height:400px;color:#9CA3AF;text-align:center">
          <v-icon size="80" color="grey-lighten-2" class="mb-4">mdi-text-box-search-outline</v-icon>
          <p class="text-h6 font-weight-medium">Esperando archivo</p>
          <p class="text-body-2">Carga un archivo .txt y presiona "Procesar"</p>
        </div>

        <template v-else>
          <!-- Métricas principales -->
          <v-row class="mb-4">
            <v-col v-for="m in metricas" :key="m.label" cols="6" sm="3" md="6" lg="3">
              <v-card class="pa-3 text-center" variant="tonal" :color="m.color">
                <div class="text-h5 font-weight-bold">{{ m.valor }}</div>
                <div class="text-caption">{{ m.label }}</div>
              </v-card>
            </v-col>
          </v-row>

          <!-- Tabs de resultados -->
          <v-card>
            <v-tabs v-model="tabResultado" color="accent" density="compact">
              <v-tab value="frecuencia">Frecuencia</v-tab>
              <v-tab value="clasificacion">Clasificación</v-tab>
              <v-tab value="patrones">Patrones</v-tab>
            </v-tabs>

            <v-divider />

            <v-window v-model="tabResultado">
              <!-- Frecuencia -->
              <v-window-item value="frecuencia" class="pa-4">
                <v-row>
                  <v-col cols="12" md="6">
                    <h3 class="text-subtitle-2 font-weight-bold mb-2">
                      🔼 Más frecuentes
                    </h3>
                    <table class="tabla-frecuencia">
                      <thead>
                        <tr><th>Palabra</th><th>Veces</th></tr>
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
                    <h3 class="text-subtitle-2 font-weight-bold mb-2">
                      🔽 Menos frecuentes
                    </h3>
                    <table class="tabla-frecuencia">
                      <thead>
                        <tr><th>Palabra</th><th>Veces</th></tr>
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

              <!-- Clasificación gramatical -->
              <v-window-item value="clasificacion" class="pa-4">
                <v-row>
                  <v-col cols="12" sm="6" v-for="cat in categorias" :key="cat.key">
                    <div class="mb-4">
                      <div class="d-flex align-center mb-2">
                        <span class="text-h6 mr-2">{{ cat.icon }}</span>
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

              <!-- Patrones adicionales -->
              <v-window-item value="patrones" class="pa-4">
                <v-row>
                  <v-col cols="12" sm="6">
                    <h4 class="text-subtitle-2 font-weight-bold mb-2">📧 Correos</h4>
                    <div class="d-flex flex-wrap gap-1 mb-4">
                      <v-chip v-for="e in resultado.correosEncontrados" :key="e"
                              size="small" color="blue" variant="tonal">{{ e }}</v-chip>
                      <span v-if="!resultado.correosEncontrados?.length"
                            class="text-caption text-medium-emphasis">Ninguno</span>
                    </div>

                    <h4 class="text-subtitle-2 font-weight-bold mb-2">🌐 URLs</h4>
                    <div class="d-flex flex-wrap gap-1 mb-4">
                      <v-chip v-for="u in resultado.urlsEncontradas" :key="u"
                              size="small" color="teal" variant="tonal">{{ u }}</v-chip>
                      <span v-if="!resultado.urlsEncontradas?.length"
                            class="text-caption text-medium-emphasis">Ninguna</span>
                    </div>
                  </v-col>
                  <v-col cols="12" sm="6">
                    <h4 class="text-subtitle-2 font-weight-bold mb-2">📅 Fechas</h4>
                    <div class="d-flex flex-wrap gap-1 mb-4">
                      <v-chip v-for="f in resultado.fechasEncontradas" :key="f"
                              size="small" color="orange" variant="tonal">{{ f }}</v-chip>
                      <span v-if="!resultado.fechasEncontradas?.length"
                            class="text-caption text-medium-emphasis">Ninguna</span>
                    </div>

                    <h4 class="text-subtitle-2 font-weight-bold mb-2">🔢 Números</h4>
                    <div class="d-flex flex-wrap gap-1">
                      <v-chip v-for="n in resultado.numeros?.slice(0,10)" :key="n"
                              size="small" color="purple" variant="tonal">{{ n }}</v-chip>
                      <span v-if="!resultado.numeros?.length"
                            class="text-caption text-medium-emphasis">Ninguno</span>
                    </div>
                  </v-col>
                </v-row>
              </v-window-item>
            </v-window>
          </v-card>
        </template>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth' })
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

const idiomas = [
  { label: '🇪🇸 Español', value: 'español' },
  { label: '🇺🇸 Inglés',  value: 'inglés' },
  { label: '🇷🇺 Ruso',    value: 'ruso' },
  { label: '🇨🇳 Chino',   value: 'chino' },
  { label: '🇸🇦 Árabe',   value: 'árabe' }
]

const categorias = [
  { key: 'pronombres',    label: 'Pronombres personales', icon: '🗣️',  color: 'indigo' },
  { key: 'verbos',        label: 'Verbos (raíz)',         icon: '⚡',   color: 'green' },
  { key: 'sustantivos',   label: 'Sustantivos',           icon: '🏷️',  color: 'blue' },
  { key: 'adjetivos',     label: 'Adjetivos',             icon: '✨',   color: 'pink' },
  { key: 'nombresPersonas', label: 'Nombres de personas', icon: '👤',  color: 'amber' },
  { key: 'conectores',    label: 'Conectores',            icon: '🔗',  color: 'cyan' },
  { key: 'preposiciones', label: 'Preposiciones',         icon: '📌',  color: 'deep-purple' },
]

const metricas = computed(() => resultado.value ? [
  { label: 'Palabras',   valor: resultado.value.totalPalabras.toLocaleString(),  color: 'accent' },
  { label: 'Oraciones',  valor: resultado.value.totalOraciones.toLocaleString(), color: 'success' },
  { label: 'Párrafos',   valor: resultado.value.totalParrafos.toLocaleString(),  color: 'warning' },
  { label: 'Long. prom', valor: resultado.value.promedioLongitudPalabra,         color: 'info' }
] : [])

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
      idioma:    idioma.value,
      nombreArchivo: archivo.value?.name || 'archivo.txt'
    })
    resultado.value = data
  } catch (e: any) {
    errorMsg.value = e.response?.data?.mensaje || 'Error al procesar el archivo.'
  } finally {
    procesando.value = false
  }
}
</script>

<style scoped>
.upload-zone {
  border: 2px dashed #CBD5E1;
  border-radius: 12px;
  padding: 32px 16px;
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
</style>
