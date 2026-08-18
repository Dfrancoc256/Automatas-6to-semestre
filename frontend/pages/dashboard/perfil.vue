<template>
  <div>
    <div class="d-flex align-center mb-6">
      <v-icon color="accent" class="mr-3">mdi-account-edit</v-icon>
      <div>
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Mi Perfil</h1>
        <p class="text-body-2 text-medium-emphasis">Gestiona tu información personal</p>
      </div>
    </div>

    <v-row>
      <!-- Avatar y datos rápidos -->
      <v-col cols="12" md="4">
        <v-card class="pa-6 text-center">
          <v-avatar size="96" class="mb-4" color="accent">
            <v-img v-if="perfil?.fotoModificada" :src="`/uploads/${perfil.fotoModificada}`" />
            <span v-else class="text-white text-h4 font-weight-bold">
              {{ perfil?.nickname?.charAt(0)?.toUpperCase() }}
            </span>
          </v-avatar>
          <h2 class="text-h6 font-weight-bold mb-1">{{ perfil?.nickname }}</h2>
          <v-chip color="accent" size="small" variant="tonal" class="mb-3">{{ perfil?.rol }}</v-chip>
          <p class="text-caption text-medium-emphasis">
            Miembro desde {{ perfil ? formatFecha(perfil.fechaRegistro) : '—' }}
          </p>
        </v-card>
      </v-col>

      <!-- Formulario de edición -->
      <v-col cols="12" md="8">
        <v-card class="pa-6">
          <h2 class="text-subtitle-1 font-weight-bold mb-4">Actualizar información</h2>

          <v-form @submit.prevent="guardar" ref="formRef">
            <v-text-field :model-value="perfil?.correo" label="Correo"
                          prepend-inner-icon="mdi-email" readonly
                          variant="filled" class="mb-3" />
            <v-text-field v-model="form.telefono" label="Teléfono"
                          prepend-inner-icon="mdi-phone" class="mb-3" />
            <v-select v-model="form.metodoNotificacion"
                      label="Método de notificación"
                      prepend-inner-icon="mdi-bell"
                      :items="['email','whatsapp','ambos']"
                      class="mb-4" />

            <v-divider class="mb-4" />
            <h3 class="text-subtitle-2 font-weight-bold mb-3">Cambiar contraseña</h3>

            <v-text-field v-model="form.passwordActual" label="Contraseña actual"
                          prepend-inner-icon="mdi-lock-outline" type="password" class="mb-3" />
            <v-text-field v-model="form.nuevoPassword"
                          label="Nueva contraseña (mín. 8 caracteres)"
                          prepend-inner-icon="mdi-lock" type="password" class="mb-4"
                          :rules="form.nuevoPassword ? [v => v.length >= 8 || 'Mínimo 8'] : []" />

            <v-alert v-if="msgExito" type="success" variant="tonal" density="compact" class="mb-4">
              {{ msgExito }}
            </v-alert>
            <v-alert v-if="msgError" type="error" variant="tonal" density="compact" class="mb-4">
              {{ msgError }}
            </v-alert>

            <v-btn type="submit" color="accent" :loading="guardando"
                   prepend-icon="mdi-content-save" block>
              Guardar cambios
            </v-btn>
          </v-form>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'Mi Perfil' })

const auth     = useAuthStore()
const { api }  = useApi()
const perfil   = ref<any>(null)
const guardando = ref(false)
const msgExito = ref(''); const msgError = ref('')
const formRef  = ref<any>(null)
const form = reactive({
  telefono: '', metodoNotificacion: 'email',
  passwordActual: '', nuevoPassword: ''
})

onMounted(async () => {
  try {
    const { data } = await api.get('/auth/perfil')
    perfil.value = data
    form.telefono = data.telefono
    form.metodoNotificacion = data.metodoNotificacion
  } catch { /* ignore */ }
})

async function guardar() {
  msgExito.value = ''; msgError.value = ''
  guardando.value = true
  try {
    const payload: any = {
      telefono: form.telefono,
      metodoNotificacion: form.metodoNotificacion
    }
    if (form.passwordActual && form.nuevoPassword) {
      payload.passwordActual = form.passwordActual
      payload.nuevoPassword  = form.nuevoPassword
    }
    await api.put('/auth/perfil', payload)
    msgExito.value = 'Perfil actualizado correctamente.'
    form.passwordActual = ''; form.nuevoPassword = ''
  } catch (e: any) {
    msgError.value = e.response?.data?.mensaje || 'Error al guardar.'
  } finally { guardando.value = false }
}

function formatFecha(iso: string) {
  return new Date(iso).toLocaleDateString('es-GT', { day: '2-digit', month: 'long', year: 'numeric' })
}
</script>
