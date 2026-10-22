<template>
  <div>
    <div class="d-flex align-center mb-6">
      <v-icon color="accent" class="mr-3">mdi-history</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Historial de Análisis</h1>
        <p class="text-body-2 text-medium-emphasis">Todos tus análisis realizados</p>
      </div>
    </div>

    <v-card>
      <v-card-text>
        <v-text-field v-model.trim="busqueda" class="mb-4" density="compact" variant="outlined" hide-details clearable prepend-inner-icon="mdi-magnify" label="Buscar análisis" placeholder="Archivo o idioma" />
        <v-data-table
          :headers="headers"
          :items="historialFiltrado"
          :loading="cargando"
          no-data-text="No se encontraron análisis"
          density="comfortable"
        >
          <template #item.idioma="{ item }">
            <v-chip size="x-small" color="accent" variant="tonal">{{ item.idioma }}</v-chip>
          </template>
          <template #item.totalPalabras="{ item }">
            {{ item.totalPalabras.toLocaleString() }}
          </template>
          <template #item.fechaAnalisis="{ item }">
            {{ formatFecha(item.fechaAnalisis) }}
          </template>
          <template #item.acciones="{ item }">
            <v-btn icon size="small" variant="text" color="accent"
                   @click="verDetalle(item.id)">
              <v-icon size="16">mdi-eye</v-icon>
            </v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Diálogo de detalle -->
    <v-dialog v-model="dialogDetalle" max-width="700">
      <v-card v-if="detalle">
        <v-card-title class="pa-5 pb-2">
          <span class="text-subtitle-1 font-weight-bold">{{ detalle.nombreArchivo }}</span>
          <v-chip size="small" color="accent" variant="tonal" class="ml-2">{{ detalle.idioma }}</v-chip>
        </v-card-title>
        <v-card-text>
          <v-row class="mb-3">
            <v-col v-for="m in detalleMet" :key="m.l" cols="6" sm="3">
              <div class="text-center">
                <div class="text-h6 font-weight-bold" style="color:#051B2E">{{ m.v }}</div>
                <div class="text-caption text-medium-emphasis">{{ m.l }}</div>
              </div>
            </v-col>
          </v-row>
          <v-divider class="mb-3" />
          <div class="text-subtitle-2 font-weight-bold mb-2">Palabras más frecuentes</div>
          <div class="d-flex flex-wrap gap-1 mb-5">
            <v-chip v-for="t in detalle.palabrasMasFrecuentes?.slice(0, 8)" :key="t.token"
                    size="small" color="primary" variant="tonal">
              {{ t.token }} ({{ t.frecuencia }})
            </v-chip>
            <span v-if="!detalle.palabrasMasFrecuentes?.length" class="text-body-2 text-medium-emphasis">
              Sin palabras registradas.
            </span>
          </div>

          <div class="d-flex align-center justify-space-between mb-2">
            <div>
              <div class="text-subtitle-2 font-weight-bold">Clasificación</div>
              <div class="text-caption text-medium-emphasis">Selecciona una categoría para consultar sus palabras.</div>
            </div>
          </div>
          <v-list class="border rounded" density="compact" lines="one">
            <v-list-item
              v-for="categoria in categorias"
              :key="categoria.key"
              :title="categoria.label"
              :prepend-icon="categoria.icon"
              :disabled="cantidadCategoria(categoria.key) === 0"
              @click="abrirCategoria(categoria)"
            >
              <template #append>
                <v-chip size="x-small" color="primary" variant="tonal" class="mr-2">
                  {{ cantidadCategoria(categoria.key) }}
                </v-chip>
                <v-icon size="18">mdi-chevron-right</v-icon>
              </template>
            </v-list-item>
          </v-list>
          <div class="text-caption text-medium-emphasis mt-2">El número indica las palabras distintas encontradas.</div>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn @click="dialogDetalle = false">Cerrar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="dialogCategoria" max-width="560">
      <v-card>
        <v-card-title class="d-flex align-center pa-5 pb-3">
          <v-icon color="primary" class="mr-3">{{ categoriaSeleccionada?.icon }}</v-icon>
          <div>
            <div class="text-subtitle-1 font-weight-bold">{{ categoriaSeleccionada?.label }}</div>
            <div class="text-caption text-medium-emphasis">{{ palabrasCategoria.length }} palabras encontradas</div>
          </div>
        </v-card-title>
        <v-card-text>
          <div v-if="palabrasCategoria.length" class="d-flex flex-wrap gap-1">
            <v-chip v-for="palabra in palabrasCategoria" :key="palabra" color="primary" variant="tonal">
              {{ palabra }}
            </v-chip>
          </div>
          <div v-else class="text-body-2 text-medium-emphasis">No se encontraron palabras en esta categoría.</div>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn @click="dialogCategoria = false">Cerrar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN', 'SUPERVISOR', 'ANALISTA'] })
useHead({ title: 'Historial' })

const { api }  = useApi()
const cargando = ref(true)
const historial = ref<any[]>([])
const busqueda = ref('')
const dialogDetalle = ref(false)
const dialogCategoria = ref(false)
const detalle = ref<any>(null)
const categoriaSeleccionada = ref<any>(null)

const categorias = [
  { key: 'pronombres', label: 'Pronombres personales', icon: 'mdi-account-voice' },
  { key: 'verbos', label: 'Verbos', icon: 'mdi-run' },
  { key: 'sustantivos', label: 'Sustantivos', icon: 'mdi-tag-outline' },
  { key: 'adjetivos', label: 'Adjetivos', icon: 'mdi-format-color-text' },
  { key: 'nombresPersonas', label: 'Nombres de personas', icon: 'mdi-account-outline' },
  { key: 'conectores', label: 'Conectores', icon: 'mdi-link-variant' },
  { key: 'preposiciones', label: 'Preposiciones', icon: 'mdi-map-marker-outline' }
]

const headers = [
  { title: 'Archivo',    key: 'nombreArchivo', sortable: true },
  { title: 'Idioma',     key: 'idioma',        sortable: true },
  { title: 'Palabras',   key: 'totalPalabras', sortable: true },
  { title: 'Fecha',      key: 'fechaAnalisis', sortable: true },
  { title: 'Ver',        key: 'acciones',      sortable: false }
]

const historialFiltrado = computed(() => {
  const termino = busqueda.value.trim().toLocaleLowerCase('es-GT')
  if (!termino) return historial.value
  return historial.value.filter((analisis) => [analisis.nombreArchivo, analisis.idioma]
    .some((valor) => String(valor || '').toLocaleLowerCase('es-GT').includes(termino)))
})

const detalleMet = computed(() => detalle.value ? [
  { l: 'Palabras',  v: detalle.value.totalPalabras?.toLocaleString() },
  { l: 'Oraciones', v: detalle.value.totalOraciones?.toLocaleString() },
  { l: 'Párrafos',  v: detalle.value.totalParrafos?.toLocaleString() },
  { l: 'Long. prom',v: detalle.value.promedioLongitudPalabra }
] : [])

const palabrasCategoria = computed<string[]>(() => {
  const key = categoriaSeleccionada.value?.key
  return key && Array.isArray(detalle.value?.[key]) ? detalle.value[key] : []
})

onMounted(async () => {
  try {
    const { data } = await api.get('/analisis/historial')
    historial.value = data
  } catch { /* ignore */ }
  finally { cargando.value = false }
})

async function verDetalle(id: number) {
  try {
    const { data } = await api.get(`/analisis/${id}`)
    detalle.value = data
    categoriaSeleccionada.value = null
    dialogDetalle.value = true
  } catch { /* ignore */ }
}

function cantidadCategoria(key: string) {
  return Array.isArray(detalle.value?.[key]) ? detalle.value[key].length : 0
}

function abrirCategoria(categoria: any) {
  if (!cantidadCategoria(categoria.key)) return
  categoriaSeleccionada.value = categoria
  dialogCategoria.value = true
}

function formatFecha(iso: string) {
  return new Date(iso).toLocaleDateString('es-GT', {
    day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
  })
}
</script>
<style scoped>.gap-1{gap:4px;}</style>
