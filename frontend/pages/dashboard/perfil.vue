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
            <v-img v-if="fotoPerfilUrl" :src="fotoPerfilUrl" />
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
        <v-card class="pa-6 mb-6">
          <div class="d-flex align-center justify-space-between mb-4">
            <h2 class="text-subtitle-1 font-weight-bold mb-0">Reconocimiento facial</h2>
            <v-chip v-if="perfil?.tieneRostroEnrolado" color="success" size="small" variant="tonal">
              <v-icon start size="16">mdi-check-decagram</v-icon>Enrolado
            </v-chip>
            <v-chip v-else color="warning" size="small" variant="tonal">
              <v-icon start size="16">mdi-alert</v-icon>Sin enrolar
            </v-chip>
          </div>

          <div v-if="!camaraFacialActiva" class="text-center">
            <v-img v-if="fotoPerfilUrl" :src="fotoPerfilUrl"
                   width="120" height="120" cover
                   class="mx-auto mb-3 rounded-circle facial-enrol-foto" />
            <p class="text-body-2 text-medium-emphasis mb-3">
              Enrola tu rostro para poder iniciar sesión con reconocimiento facial.
            </p>
            <v-btn color="primary" variant="outlined" prepend-icon="mdi-face-recognition"
                   @click="iniciarEnrolamiento">
              {{ perfil?.tieneRostroEnrolado ? 'Actualizar rostro' : 'Enrolar rostro' }}
            </v-btn>
          </div>

          <div v-else class="text-center">
            <FaceScanner
              ref="escanerEnrol"
              con-foto
              :ocupado="enrolando"
              class="mb-3"
              @capturado="enrolar"
              @error="alFallarCamara"
            />
            <p class="text-body-2 text-medium-emphasis mb-3">
              Coloca tu rostro dentro del marco. Se capturará automáticamente.
            </p>

            <v-alert v-if="msgFacial" :type="tipoMsgFacial" variant="tonal" density="compact" class="mb-3">
              {{ msgFacial }}
            </v-alert>

            <div class="d-flex gap-2 justify-center">
              <v-btn variant="outlined" @click="cancelarEnrolamiento">Cancelar</v-btn>
            </div>
          </div>
        </v-card>

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
definePageMeta({ layout: 'dashboard', middleware: ['auth', 'role'], roles: ['ADMIN', 'SUPERVISOR', 'ANALISTA'] })
useHead({ title: 'Mi Perfil' })

const auth     = useAuthStore()
const { api }  = useApi()
const { calentar } = useFacialRecognition()
const { fotoUrl } = useMediaUrl()
const perfil   = ref<any>(null)
const fotoPerfilUrl = computed(() => fotoUrl(perfil.value?.fotoModificada))
const guardando = ref(false)
const msgExito = ref(''); const msgError = ref('')
const formRef  = ref<any>(null)
const form = reactive({
  telefono: '', metodoNotificacion: 'email',
  passwordActual: '', nuevoPassword: ''
})

// Enrolamiento facial
const camaraFacialActiva = ref(false)
const enrolando     = ref(false)
const msgFacial      = ref('')
const tipoMsgFacial  = ref<'success' | 'error'>('success')
const escanerEnrol  = ref<{ reiniciar: () => void } | null>(null)

onMounted(async () => {
  calentar()
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

function iniciarEnrolamiento() {
  msgFacial.value = ''
  camaraFacialActiva.value = true
}

function cancelarEnrolamiento() {
  camaraFacialActiva.value = false
  msgFacial.value = ''
}

function alFallarCamara(mensaje: string) {
  msgFacial.value = mensaje
  tipoMsgFacial.value = 'error'
}

async function enrolar(captura: { descriptor: number[] | null, foto: string | null }) {
  if (!captura.descriptor || enrolando.value) return
  msgFacial.value = ''
  enrolando.value = true

  try {
    await api.post('/facial/enrolar', { descriptor: captura.descriptor })
    if (captura.foto) await api.put('/auth/perfil', { fotoModificadaBase64: captura.foto })

    const { data } = await api.get('/auth/perfil')
    perfil.value = data

    msgFacial.value = 'Rostro enrolado correctamente.'
    tipoMsgFacial.value = 'success'
    camaraFacialActiva.value = false
  } catch (e: any) {
    msgFacial.value = e.response?.data?.mensaje || 'No se pudo enrolar el rostro.'
    tipoMsgFacial.value = 'error'
    setTimeout(() => escanerEnrol.value?.reiniciar(), 1800)
  } finally {
    enrolando.value = false
  }
}
</script>

<style scoped>
.facial-enrol-foto {
  border: 3px solid #B48B21;
}
</style>
