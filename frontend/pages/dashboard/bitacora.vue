<template>
  <div>
    <div class="d-flex align-center mb-6">
      <v-icon color="accent" class="mr-3">mdi-shield-check</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Historial de accesos</h1>
        <p class="text-body-2 text-medium-emphasis">Registro de actividad de autenticación</p>
      </div>
    </div>

    <v-card>
      <v-alert v-if="error" type="error" variant="tonal" density="compact" class="ma-4 mb-0">{{ error }}</v-alert>
      <v-card-text>
        <v-text-field v-model.trim="busqueda" class="mb-4" density="compact" variant="outlined" hide-details clearable prepend-inner-icon="mdi-magnify" label="Buscar accesos" placeholder="Usuario, método o resultado" />
        <v-data-table :headers="headers" :items="registrosFiltrados" :loading="cargando"
                      no-data-text="No se encontraron accesos" density="comfortable">
          <template #item.resultado="{ item }">
            <v-chip :color="item.resultado === 'exitoso' ? 'success' : 'error'"
                    size="x-small" variant="tonal">
              {{ item.resultado }}
            </v-chip>
          </template>
          <template #item.metodo="{ item }">
            <v-chip color="info" size="x-small" variant="tonal">{{ formatearMetodo(item.metodo) }}</v-chip>
          </template>
          <template #item.fechaHora="{ item }">
            {{ new Date(item.fechaHora).toLocaleString('es-GT') }}
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN'] })
useHead({ title: 'Bitácora' })

const { api }  = useApi()
const cargando = ref(true)
const registros = ref<any[]>([])
const busqueda = ref('')
const error = ref('')

const headers = [
  { title: 'Usuario',  key: 'usuario',   sortable: true },
  { title: 'Método de acceso', key: 'metodo', sortable: true },
  { title: 'Resultado',key: 'resultado', sortable: true },
  { title: 'Fecha',    key: 'fechaHora', sortable: true }
]

const registrosFiltrados = computed(() => {
  const termino = busqueda.value.trim().toLocaleLowerCase('es-GT')
  if (!termino) return registros.value
  return registros.value.filter((registro) => [registro.usuario, formatearMetodo(registro.metodo), registro.resultado]
    .some((valor) => String(valor || '').toLocaleLowerCase('es-GT').includes(termino)))
})

function formatearMetodo(metodo: string) {
  const etiquetas: Record<string, string> = {
    password: 'Contraseña',
    google: 'Google',
    facial: 'Reconocimiento facial',
    qr: 'Código QR'
  }
  return etiquetas[metodo] || 'Otro'
}

onMounted(async () => {
  try {
    const { data } = await api.get('/dashboard/bitacora')
    registros.value = data.registros
  } catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible cargar la bitácora.' } finally { cargando.value = false }
})
</script>
