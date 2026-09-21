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
            <div class="webcam-container mb-3">
              <video ref="videoEnrolEl" autoplay playsinline
                     style="width:100%;border-radius:8px;object-fit:cover" />
            </div>
            <canvas ref="canvasEnrolEl" style="display:none" width="640" height="480" />

            <v-alert v-if="msgFacial" :type="tipoMsgFacial" variant="tonal" density="compact" class="mb-3">
              {{ msgFacial }}
            </v-alert>

            <div class="d-flex gap-2 justify-center">
              <v-btn variant="outlined" @click="cancelarEnrolamiento">Cancelar</v-btn>
              <v-btn color="accent" class="umg-gold-button" :loading="enrolando"
                     prepend-icon="mdi-camera-iris" @click="capturarYEnrolar">
                Capturar y enrolar
              </v-btn>
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
const { cargarModelos, obtenerDescriptor } = useFacialRecognition()
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
const videoEnrolEl  = ref<HTMLVideoElement | null>(null)
const canvasEnrolEl = ref<HTMLCanvasElement | null>(null)
let   streamEnrol: MediaStream | null = null

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

async function iniciarEnrolamiento() {
  msgFacial.value = ''
  camaraFacialActiva.value = true
  cargarModelos()
  await nextTick()
  try {
    streamEnrol = await navigator.mediaDevices.getUserMedia({ video: true })
    if (videoEnrolEl.value) videoEnrolEl.value.srcObject = streamEnrol
  } catch {
    msgFacial.value = 'No se pudo acceder a la cámara.'
    tipoMsgFacial.value = 'error'
  }
}

function detenerCamaraEnrol() {
  streamEnrol?.getTracks().forEach(t => t.stop())
  streamEnrol = null
}

function cancelarEnrolamiento() {
  detenerCamaraEnrol()
  camaraFacialActiva.value = false
  msgFacial.value = ''
}

async function capturarYEnrolar() {
  if (!videoEnrolEl.value || !canvasEnrolEl.value) return
  msgFacial.value = ''
  enrolando.value = true

  try {
    const ctx = canvasEnrolEl.value.getContext('2d')!
    ctx.drawImage(videoEnrolEl.value, 0, 0, 640, 480)

    const descriptor = await obtenerDescriptor(canvasEnrolEl.value)
    if (!descriptor) {
      msgFacial.value = 'No se detectó un rostro claro. Mejora la iluminación e inténtalo de nuevo.'
      tipoMsgFacial.value = 'error'
      return
    }

    const fotoRostro = canvasEnrolEl.value.toDataURL('image/jpeg', 0.8)
    await api.post('/facial/enrolar', { descriptor })
    await api.put('/auth/perfil', { fotoModificadaBase64: fotoRostro })

    const { data } = await api.get('/auth/perfil')
    perfil.value = data

    msgFacial.value = 'Rostro enrolado correctamente.'
    tipoMsgFacial.value = 'success'
    detenerCamaraEnrol()
    camaraFacialActiva.value = false
  } catch (e: any) {
    msgFacial.value = e.response?.data?.mensaje || 'No se pudo enrolar el rostro.'
    tipoMsgFacial.value = 'error'
  } finally {
    enrolando.value = false
  }
}

onUnmounted(() => detenerCamaraEnrol())
</script>

<style scoped>
.facial-enrol-foto {
  border: 3px solid #B48B21;
}
</style>
