<template>
  <div>
    <div class="d-flex align-center mb-6">
      <v-icon color="accent" class="mr-3">mdi-shield-check</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Bitácora de Accesos</h1>
        <p class="text-body-2 text-medium-emphasis">Registro de actividad de autenticación</p>
      </div>
    </div>

    <v-card>
      <v-card-text>
        <v-data-table :headers="headers" :items="registros" :loading="cargando"
                      no-data-text="Sin registros" density="comfortable">
          <template #item.resultado="{ item }">
            <v-chip :color="item.resultado === 'exitoso' ? 'success' : 'error'"
                    size="x-small" variant="tonal">
              {{ item.resultado }}
            </v-chip>
          </template>
          <template #item.metodo="{ item }">
            <v-chip color="info" size="x-small" variant="tonal">{{ item.metodo }}</v-chip>
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
definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'Bitácora' })

const { api }  = useApi()
const cargando = ref(true)
const registros = ref<any[]>([])

const headers = [
  { title: 'Usuario',  key: 'usuario',   sortable: true },
  { title: 'IP',       key: 'ipOrigen',  sortable: false },
  { title: 'Método',   key: 'metodo',    sortable: true },
  { title: 'Resultado',key: 'resultado', sortable: true },
  { title: 'Fecha',    key: 'fechaHora', sortable: true }
]

onMounted(async () => {
  try {
    const { data } = await api.get('/dashboard/bitacora')
    registros.value = data.registros
  } catch { /* ignore */ } finally { cargando.value = false }
})
</script>
