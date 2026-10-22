<template>
  <div>
    <div class="d-flex align-center justify-space-between flex-wrap ga-3 mb-6">
      <div class="d-flex align-center">
        <v-icon color="accent" class="mr-3">mdi-account-group</v-icon>
        <div>
          <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Gestión de usuarios</h1>
          <p class="text-body-2 text-medium-emphasis">Consulta, actualiza y administra las cuentas del sistema.</p>
        </div>
      </div>
      <v-btn variant="outlined" prepend-icon="mdi-refresh" :loading="cargando" @click="cargarUsuarios">Actualizar</v-btn>
    </div>

    <v-card class="users-card">
      <v-alert v-if="error" type="error" variant="tonal" density="compact" class="ma-4 mb-0" closable @click:close="error = ''">{{ error }}</v-alert>
      <v-card-text class="pa-0">
        <v-data-table :headers="headers" :items="usuarios" :loading="cargando" no-data-text="Sin usuarios registrados" density="comfortable" class="users-table">
          <template #item.usuario="{ item }">
            <div class="user-identity py-2">
              <v-avatar size="38" color="primary" class="user-identity__avatar">
                <v-img v-if="item.fotoModificada" :src="fotoUrl(item.fotoModificada)" :alt="`Fotografía de ${item.nombre}`" cover />
                <span v-else>{{ iniciales(item) }}</span>
              </v-avatar>
              <div><div class="font-weight-bold">{{ item.nombre }} {{ item.apellido }}</div><div class="text-caption text-medium-emphasis">@{{ item.nickname }}</div></div>
            </div>
          </template>
          <template #item.rol="{ item }">
            <v-menu><template #activator="{ props }"><v-btn size="small" variant="tonal" :color="colRol(item.rol)" v-bind="props" append-icon="mdi-chevron-down">{{ item.rol }}</v-btn></template><v-list density="compact"><v-list-item v-for="rol in roles" :key="rol" :title="rol" @click="cambiarRol(item, rol)" /></v-list></v-menu>
          </template>
          <template #item.activo="{ item }"><v-chip :color="item.activo ? 'success' : 'error'" size="x-small" variant="tonal">{{ item.activo ? 'Activo' : 'Inactivo' }}</v-chip></template>
          <template #item.fechaRegistro="{ item }">{{ fecha(item.fechaRegistro) }}</template>
          <template #item.acciones="{ item }">
            <div class="d-flex align-center justify-end ga-1">
              <v-tooltip text="Enviar carnet PDF por correo"><template #activator="{ props }"><v-btn v-bind="props" icon="mdi-email-outline" size="small" variant="text" color="primary" @click="abrirEnvioCredencial(item, 'email')" /></template></v-tooltip>
              <v-tooltip text="Enviar carnet PDF por WhatsApp"><template #activator="{ props }"><v-btn v-bind="props" icon="mdi-whatsapp" size="small" variant="text" color="success" @click="abrirEnvioCredencial(item, 'whatsapp')" /></template></v-tooltip>
              <v-tooltip text="Editar usuario"><template #activator="{ props }"><v-btn v-bind="props" icon="mdi-pencil-outline" size="small" variant="text" @click="abrirEdicion(item)" /></template></v-tooltip>
              <v-tooltip :text="item.activo ? 'Desactivar usuario' : 'Activar usuario'"><template #activator="{ props }"><v-btn v-bind="props" :icon="item.activo ? 'mdi-account-off-outline' : 'mdi-account-check-outline'" size="small" variant="text" :color="item.activo ? 'error' : 'success'" @click="item.activo ? pedirDesactivacion(item) : toggleUsuario(item)" /></template></v-tooltip>
            </div>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <v-dialog v-model="dialogoEdicion" max-width="620"><v-card><v-card-title class="pt-5 px-6">Editar usuario</v-card-title><v-card-text class="px-6 pb-2"><v-row><v-col cols="12" sm="6"><v-text-field v-model.trim="formEdicion.nombre" label="Nombre" /></v-col><v-col cols="12" sm="6"><v-text-field v-model.trim="formEdicion.apellido" label="Apellido" /></v-col><v-col cols="12" sm="6"><v-text-field v-model.trim="formEdicion.correo" label="Correo electrónico" type="email" /></v-col><v-col cols="12" sm="6"><v-text-field v-model.trim="formEdicion.telefono" label="Teléfono" /></v-col><v-col cols="12" sm="6"><v-select v-model="formEdicion.rol" label="Rol" :items="roles" /></v-col><v-col cols="12" sm="6"><v-select v-model="formEdicion.metodoNotificacion" label="Canal preferido" :items="canales" /></v-col></v-row></v-card-text><v-card-actions class="px-6 pb-5"><v-spacer /><v-btn variant="text" @click="dialogoEdicion = false">Cancelar</v-btn><v-btn color="accent" class="umg-gold-button" :loading="guardando" @click="guardarEdicion">Guardar cambios</v-btn></v-card-actions></v-card></v-dialog>

    <v-dialog v-model="dialogoDesactivar" max-width="430"><v-card><v-card-title class="pt-5 px-6">Desactivar usuario</v-card-title><v-card-text class="px-6">La cuenta de <strong>{{ usuarioSeleccionado?.nombre }} {{ usuarioSeleccionado?.apellido }}</strong> dejará de poder iniciar sesión. Sus registros se conservarán.</v-card-text><v-card-actions class="px-6 pb-5"><v-spacer /><v-btn variant="text" @click="dialogoDesactivar = false">Cancelar</v-btn><v-btn color="error" variant="flat" :loading="guardando" @click="desactivarUsuario">Desactivar</v-btn></v-card-actions></v-card></v-dialog>

    <v-dialog v-model="dialogoCredencial" max-width="520">
      <v-card class="notification-dialog">
        <v-card-title class="pt-5 px-6">Enviar carnet PDF</v-card-title>
        <v-card-text class="px-6 pb-2">
          <div v-if="usuarioCredencial" class="notification-recipient mb-5">
            <v-avatar size="48" color="primary"><v-img v-if="usuarioCredencial.fotoModificada" :src="fotoUrl(usuarioCredencial.fotoModificada)" cover /><span v-else>{{ iniciales(usuarioCredencial) }}</span></v-avatar>
            <div><strong>{{ usuarioCredencial.nombre }} {{ usuarioCredencial.apellido }}</strong><span>{{ destinoCredencial }}</span></div>
          </div>
          <v-btn-toggle v-model="canalCredencial" mandatory divided color="primary" class="mb-5 w-100">
            <v-btn value="email" class="flex-grow-1" prepend-icon="mdi-email-outline">Correo</v-btn>
            <v-btn value="whatsapp" class="flex-grow-1" prepend-icon="mdi-whatsapp">WhatsApp</v-btn>
          </v-btn-toggle>
          <p class="text-body-1 mb-0">¿Estás seguro de enviar el carnet PDF a <strong>{{ usuarioCredencial?.nombre }} {{ usuarioCredencial?.apellido }}</strong> al {{ canalCredencial === 'email' ? 'correo' : 'número' }} <strong>{{ destinoCredencial }}</strong>?</p>
          <v-alert v-if="errorCredencial" type="error" variant="tonal" density="compact" class="mt-4">{{ errorCredencial }}</v-alert>
        </v-card-text>
        <v-card-actions class="px-6 pb-5"><v-spacer /><v-btn variant="text" :disabled="enviandoCredencial" @click="dialogoCredencial = false">Cancelar</v-btn><v-btn color="accent" class="umg-gold-button" :loading="enviandoCredencial" @click="enviarCredencial">Enviar carnet</v-btn></v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="mensajeVisible" color="success" timeout="3200">{{ mensaje }}</v-snackbar>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN'] })
useHead({ title: 'Usuarios' })

const { api } = useApi()
const { fotoUrl } = useMediaUrl()
const cargando = ref(false)
const guardando = ref(false)
const usuarios = ref<any[]>([])
const error = ref('')
const mensaje = ref('')
const mensajeVisible = ref(false)
const dialogoEdicion = ref(false)
const dialogoDesactivar = ref(false)
const dialogoCredencial = ref(false)
const usuarioSeleccionado = ref<any>(null)
const usuarioCredencial = ref<any>(null)
const canalCredencial = ref<'email' | 'whatsapp'>('email')
const enviandoCredencial = ref(false)
const errorCredencial = ref('')
const roles = ['ADMIN', 'SUPERVISOR', 'ANALISTA']
const canales = ['email', 'whatsapp', 'ambos']
const formEdicion = reactive({ nombre: '', apellido: '', correo: '', telefono: '', rol: 'ANALISTA', metodoNotificacion: 'email' })

const headers = [
  { title: 'Usuario', key: 'usuario', sortable: false }, { title: 'Correo', key: 'correo', sortable: true }, { title: 'Teléfono', key: 'telefono', sortable: false }, { title: 'Rol', key: 'rol', sortable: true }, { title: 'Estado', key: 'activo', sortable: true }, { title: 'Registro', key: 'fechaRegistro', sortable: true }, { title: 'Acciones', key: 'acciones', sortable: false, align: 'end' as const }
]

onMounted(cargarUsuarios)

async function cargarUsuarios() {
  cargando.value = true; error.value = ''
  try { const { data } = await api.get('/dashboard/usuarios'); usuarios.value = data }
  catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible cargar los usuarios.' }
  finally { cargando.value = false }
}
async function toggleUsuario(usuario: any) {
  try { await api.patch(`/dashboard/usuarios/${usuario.id}/toggle`); usuario.activo = !usuario.activo; confirmar('Estado actualizado.') }
  catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible cambiar el estado del usuario.' }
}
async function cambiarRol(usuario: any, rol: string) {
  if (usuario.rol === rol) return
  try { await api.patch(`/dashboard/usuarios/${usuario.id}/rol`, { rol }); usuario.rol = rol; confirmar('Rol actualizado.') }
  catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible actualizar el rol.' }
}
function abrirEdicion(usuario: any) {
  usuarioSeleccionado.value = usuario
  Object.assign(formEdicion, { nombre: usuario.nombre, apellido: usuario.apellido, correo: usuario.correo, telefono: usuario.telefono, rol: usuario.rol, metodoNotificacion: usuario.metodoNotificacion })
  dialogoEdicion.value = true
}
async function guardarEdicion() {
  if (!usuarioSeleccionado.value) return
  guardando.value = true
  try { const { data } = await api.patch(`/dashboard/usuarios/${usuarioSeleccionado.value.id}`, formEdicion); Object.assign(usuarioSeleccionado.value, data); dialogoEdicion.value = false; confirmar('Datos del usuario actualizados.') }
  catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible guardar los cambios.' }
  finally { guardando.value = false }
}
function pedirDesactivacion(usuario: any) { usuarioSeleccionado.value = usuario; dialogoDesactivar.value = true }
async function desactivarUsuario() {
  if (!usuarioSeleccionado.value) return
  guardando.value = true
  try { await api.delete(`/dashboard/usuarios/${usuarioSeleccionado.value.id}`); usuarioSeleccionado.value.activo = false; dialogoDesactivar.value = false; confirmar('Usuario desactivado. Sus datos se conservaron.') }
  catch (e: any) { error.value = e.response?.data?.mensaje || 'No fue posible desactivar el usuario.' }
  finally { guardando.value = false }
}
const destinoCredencial = computed(() => {
  if (!usuarioCredencial.value) return ''
  return canalCredencial.value === 'email' ? usuarioCredencial.value.correo : usuarioCredencial.value.telefono
})
function abrirEnvioCredencial(usuario: any, canal: 'email' | 'whatsapp') {
  usuarioCredencial.value = usuario
  canalCredencial.value = canal
  errorCredencial.value = ''
  dialogoCredencial.value = true
}
async function enviarCredencial() {
  if (!usuarioCredencial.value) return
  enviandoCredencial.value = true; errorCredencial.value = ''
  try {
    const { data } = await api.post(`/dashboard/usuarios/${usuarioCredencial.value.id}/credencial/enviar`, { canal: canalCredencial.value })
    dialogoCredencial.value = false
    confirmar(data.mensaje || 'Carnet PDF enviado.')
  } catch (e: any) { errorCredencial.value = e.response?.data?.mensaje || 'No fue posible enviar el carnet PDF.' }
  finally { enviandoCredencial.value = false }
}
function confirmar(texto: string) { mensaje.value = texto; mensajeVisible.value = true }
function fecha(valor: string) { return new Date(valor).toLocaleDateString('es-GT') }
function colRol(rol: string) { return rol === 'ADMIN' ? 'error' : rol === 'SUPERVISOR' ? 'warning' : 'accent' }
function iniciales(usuario: any) { return `${usuario.nombre?.[0] || ''}${usuario.apellido?.[0] || ''}`.toUpperCase() || '?' }
</script>

<style scoped>
.users-card { overflow: hidden; border: 1px solid #E2E8F0 !important; box-shadow: none !important; }
.users-table :deep(th) { color: #405064; font-size: 12px; font-weight: 700; white-space: nowrap; }
.users-table :deep(td) { white-space: nowrap; }
.user-identity, .notification-recipient { display: flex; align-items: center; gap: .7rem; }
.user-identity__avatar { color: #fff; font-size: .75rem; font-weight: 800; }
.notification-recipient { padding: .8rem; border: 1px solid #dce5eb; border-radius: 10px; background: #f7fafc; }
.notification-recipient > div { display: grid; min-width: 0; }
.notification-recipient strong { color: #05233d; }
.notification-recipient span { overflow: hidden; color: #65788a; font-size: .82rem; text-overflow: ellipsis; white-space: nowrap; }
@media (max-width: 760px) { .users-card { overflow-x: auto; } .users-table { min-width: 780px; } }
</style>
