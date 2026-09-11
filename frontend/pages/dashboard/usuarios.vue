<template>
  <div>
    <div class="d-flex align-center mb-6">
      <v-icon color="accent" class="mr-3">mdi-account-group</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Gestión de Usuarios</h1>
        <p class="text-body-2 text-medium-emphasis">Administra las cuentas del sistema</p>
      </div>
    </div>

    <v-card>
      <v-alert v-if="error" type="error" variant="tonal" density="compact" class="ma-4 mb-0">{{ error }}</v-alert>
      <v-card-text>
        <v-data-table :headers="headers" :items="usuarios" :loading="cargando"
                      no-data-text="Sin usuarios" density="comfortable">
          <template #item.rol="{ item }">
            <v-chip :color="colRol(item.rol)" size="small" variant="tonal">{{ item.rol }}</v-chip>
          </template>
          <template #item.activo="{ item }">
            <v-chip :color="item.activo ? 'success' : 'error'" size="x-small" variant="tonal">
              {{ item.activo ? 'Activo' : 'Inactivo' }}
            </v-chip>
          </template>
          <template #item.fechaRegistro="{ item }">
            {{ new Date(item.fechaRegistro).toLocaleDateString('es-GT') }}
          </template>
          <template #item.acciones="{ item }">
            <div class="d-flex gap-1">
              <v-btn icon size="x-small" variant="text"
                     :color="item.activo ? 'error' : 'success'"
                     @click="toggleUsuario(item)">
                <v-icon size="14">{{ item.activo ? 'mdi-account-off' : 'mdi-account-check' }}</v-icon>
              </v-btn>
              <v-menu>
                <template #activator="{ props }">
                  <v-btn icon size="x-small" variant="text" color="grey" v-bind="props">
                    <v-icon size="14">mdi-dots-vertical</v-icon>
                  </v-btn>
                </template>
                <v-list density="compact">
                  <v-list-item v-for="r in ['ADMIN','SUPERVISOR','ANALISTA']" :key="r"
                               :title="r" @click="cambiarRol(item, r)" />
                </v-list>
              </v-menu>
            </div>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN'] })
useHead({ title: 'Usuarios' })

const { api }   = useApi()
const cargando  = ref(true)
const usuarios  = ref<any[]>([])
const error = ref('')

const headers = [
  { title: 'Nickname', key: 'nickname', sortable: true },
  { title: 'Correo',   key: 'correo',   sortable: true },
  { title: 'Rol',      key: 'rol',      sortable: true },
  { title: 'Estado',   key: 'activo',   sortable: true },
  { title: 'Registro', key: 'fechaRegistro', sortable: true },
  { title: 'Acciones', key: 'acciones', sortable: false }
]

onMounted(async () => {
  try {
    const { data } = await api.get('/dashboard/usuarios')
    usuarios.value = data
  } catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible cargar los usuarios.' } finally { cargando.value = false }
})

async function toggleUsuario(u: any) {
  try {
    await api.patch(`/dashboard/usuarios/${u.id}/toggle`)
    u.activo = !u.activo
  } catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible cambiar el estado del usuario.' }
}

async function cambiarRol(u: any, rol: string) {
  try {
    await api.patch(`/dashboard/usuarios/${u.id}/rol`, { rol })
    u.rol = rol
  } catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible actualizar el rol.' }
}

function colRol(r: string) {
  return r === 'ADMIN' ? 'error' : r === 'SUPERVISOR' ? 'warning' : 'accent'
}
</script>
<style scoped>.gap-1{gap:4px;}</style>
