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
        <v-data-table
          :headers="headers"
          :items="historial"
          :loading="cargando"
          no-data-text="Sin análisis registrados aún"
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
          <div class="d-flex flex-wrap gap-1">
            <v-chip v-for="t in detalle.palabrasMasFrecuentes?.slice(0,8)" :key="t.token"
                    size="small" color="accent" variant="tonal">
              {{ t.token }} ({{ t.frecuencia }})
            </v-chip>
          </div>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn @click="dialogDetalle = false">Cerrar</v-btn>
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
const dialogDetalle = ref(false)
const detalle = ref<any>(null)

const headers = [
  { title: 'Archivo',    key: 'nombreArchivo', sortable: true },
  { title: 'Idioma',     key: 'idioma',        sortable: true },
  { title: 'Palabras',   key: 'totalPalabras', sortable: true },
  { title: 'Fecha',      key: 'fechaAnalisis', sortable: true },
  { title: 'Ver',        key: 'acciones',      sortable: false }
]

const detalleMet = computed(() => detalle.value ? [
  { l: 'Palabras',  v: detalle.value.totalPalabras?.toLocaleString() },
  { l: 'Oraciones', v: detalle.value.totalOraciones?.toLocaleString() },
  { l: 'Párrafos',  v: detalle.value.totalParrafos?.toLocaleString() },
  { l: 'Long. prom',v: detalle.value.promedioLongitudPalabra }
] : [])

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
    detalle.value   = data
    dialogDetalle.value = true
  } catch { /* ignore */ }
}

function formatFecha(iso: string) {
  return new Date(iso).toLocaleDateString('es-GT', {
    day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
  })
}
</script>
<style scoped>.gap-1{gap:4px;}</style>
