<template>
  <v-card class="auth-card registro-card pa-8" elevation="0">
      <div class="auth-access-tabs" aria-label="Acceso y registro">
        <button type="button" @click="emit('cambiarModo', 'login')">Ingresar</button>
        <button type="button" class="is-active" @click="emit('cambiarModo', 'registro')">Crear cuenta</button>
      </div>
      <div class="text-center mb-6">
        <img class="auth-logo mx-auto mb-3" src="/images/logo-umg-oficial.png"
             alt="Universidad Mariano Gálvez de Guatemala" />
        <h1 class="text-h5 font-weight-bold" style="color:#051B2E">Crear cuenta</h1>
        <p class="text-body-2 text-medium-emphasis mt-1">Completa el formulario para registrarte</p>
      </div>

      <v-stepper v-model="paso" alt-labels flat>
        <v-stepper-header>
          <v-stepper-item title="Datos" value="1" :complete="paso > 1" />
          <v-divider />
          <v-stepper-item title="Foto" value="2" :complete="paso > 2" />
          <v-divider />
          <v-stepper-item title="Listo" value="3" />
        </v-stepper-header>

        <v-stepper-window>
          <!-- Paso 1: datos básicos -->
          <v-stepper-window-item value="1">
            <v-form @submit.prevent="siguientePaso" ref="formPaso1">
              <v-text-field v-model="form.correo" label="Correo electrónico"
                            prepend-inner-icon="mdi-email" type="email"
                            :rules="[v => !!v || 'Requerido', v => /.+@.+\..+/.test(v) || 'Correo inválido']"
                            class="mb-3" />
              <v-text-field v-model="form.telefono" label="Teléfono"
                            prepend-inner-icon="mdi-phone"
                            :rules="[v => !!v || 'Requerido']" class="mb-3" />
              <v-text-field v-model="form.fechaNacimiento" label="Fecha de nacimiento"
                            prepend-inner-icon="mdi-calendar" type="date"
                            :rules="[v => !!v || 'Requerido']" class="mb-3" />
              <v-text-field v-model="form.nickname" label="Nickname"
                            prepend-inner-icon="mdi-at"
                            :rules="[v => !!v || 'Requerido', v => v.length >= 3 || 'Mínimo 3 caracteres']"
                            class="mb-3" />
              <v-text-field v-model="form.password" label="Contraseña"
                            prepend-inner-icon="mdi-lock"
                            :type="mostrarPass ? 'text' : 'password'"
                            :append-inner-icon="mostrarPass ? 'mdi-eye-off' : 'mdi-eye'"
                            @click:append-inner="mostrarPass = !mostrarPass"
                            :rules="[v => !!v || 'Requerido', v => v.length >= 8 || 'Mínimo 8 caracteres']"
                            class="mb-3" />
              <v-select v-model="form.metodoNotificacion"
                        label="Método de notificación"
                        prepend-inner-icon="mdi-bell"
                        :items="['email','whatsapp','ambos']"
                        class="mb-4" />

              <RecaptchaV2
                v-if="requiereRecaptcha"
                v-model:token="recaptchaToken"
                :site-key="recaptchaSiteKey"
                class="mb-4"
              />
              <v-alert v-else-if="!bypassDesarrollo" type="warning" variant="tonal" density="compact" class="mb-4">
                Falta configurar la clave pública de reCAPTCHA.
              </v-alert>
              <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mb-4">
                {{ error }}
              </v-alert>

              <v-btn color="accent" class="umg-gold-button" block size="large" type="submit" :disabled="!puedeEnviar">
                Siguiente <v-icon end>mdi-arrow-right</v-icon>
              </v-btn>
            </v-form>
          </v-stepper-window-item>

          <!-- Paso 2: foto -->
          <v-stepper-window-item value="2">
            <div class="text-center mb-4">
              <p class="text-body-2 mb-4">Toma una foto para tu credencial</p>

              <!-- Preview de cámara -->
              <div class="webcam-container mb-4">
                <video ref="videoEl" autoplay playsinline
                       style="width:100%;height:100%;object-fit:cover" />
                <div class="webcam-overlay">
                  <div class="face-guide" />
                </div>
              </div>

              <!-- Foto capturada -->
              <div v-if="fotoCapturada" class="mb-4">
                <img :src="fotoCapturada" :style="{ filter: filtroActual.css }"
                     style="width:120px;height:120px;border-radius:50%;object-fit:cover;border:3px solid #B48B21" />
                <p class="text-caption mt-1 text-success">✓ Foto capturada</p>
              </div>

              <div v-if="fotoCapturada" class="mb-4">
                <p class="text-caption font-weight-bold mb-2">Personaliza tu foto para la credencial</p>
                <div class="d-flex flex-wrap justify-center gap-2">
                  <v-btn v-for="filtro in filtros" :key="filtro.id" size="small"
                         :color="filtroActual.id === filtro.id ? 'accent' : 'primary'"
                         :variant="filtroActual.id === filtro.id ? 'flat' : 'outlined'"
                         @click="filtroActual = filtro">
                    {{ filtro.nombre }}
                  </v-btn>
                </div>
              </div>

              <canvas ref="canvasEl" style="display:none" width="640" height="480" />

              <div class="d-flex gap-2 justify-center mb-4">
                <v-btn v-if="!streamActivo" color="accent" variant="outlined"
                       prepend-icon="mdi-camera" @click="iniciarCamara">
                  Activar cámara
                </v-btn>
                <v-btn v-if="streamActivo" color="accent" class="umg-gold-button"
                       prepend-icon="mdi-camera-iris" @click="capturarFoto">
                  Capturar foto
                </v-btn>
                <v-btn v-if="fotoCapturada" color="secondary" variant="outlined"
                       prepend-icon="mdi-refresh" @click="retomar">
                  Retomar
                </v-btn>
              </div>
            </div>

            <v-alert v-if="error" type="error" variant="tonal" class="mb-4" density="compact">
              {{ error }}
            </v-alert>

            <div class="d-flex gap-2">
              <v-btn variant="outlined" @click="paso = 1">
                <v-icon start>mdi-arrow-left</v-icon> Atrás
              </v-btn>
              <v-btn color="accent" class="umg-gold-button" flex-grow-1 :loading="cargando"
                     :disabled="!fotoCapturada" @click="registrar" style="flex:1">
                Registrarme
              </v-btn>
            </div>
          </v-stepper-window-item>

          <!-- Paso 3: éxito -->
          <v-stepper-window-item value="3">
            <div class="text-center py-6">
              <v-icon size="80" color="success" class="mb-4">mdi-check-circle</v-icon>
              <h2 class="text-h5 font-weight-bold mb-2">¡Registro exitoso!</h2>
              <p class="text-body-2 text-medium-emphasis mb-6">
                Tu cuenta ha sido creada. Serás redirigido al dashboard.
              </p>
              <v-btn color="accent" class="umg-gold-button" prepend-icon="mdi-arrow-right"
                     @click="navigateTo('/dashboard')">
                Ir al Dashboard
              </v-btn>
              <v-btn class="mt-3" variant="outlined" color="secondary"
                     prepend-icon="mdi-file-pdf-box" :loading="descargando"
                     @click="descargarCredencial">
                Descargar credencial PDF
              </v-btn>
            </div>
          </v-stepper-window-item>
        </v-stepper-window>
      </v-stepper>

      <div class="text-center mt-4">
        <span class="text-body-2 text-medium-emphasis">¿Ya tienes cuenta? </span>
        <NuxtLink to="/login" class="font-weight-bold" style="color:#B48B21">Inicia sesión</NuxtLink>
      </div>
  </v-card>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
const emit = defineEmits<{ cambiarModo: [modo: 'login' | 'registro'] }>()

const auth    = useAuthStore()
const { api } = useApi()
const config = useRuntimeConfig()

const paso        = ref(1)
const mostrarPass = ref(false)
const cargando    = ref(false)
const error       = ref('')
const descargando = ref(false)
const formPaso1   = ref<any>(null)

const form = reactive({
  correo: '', telefono: '', fechaNacimiento: '',
  nickname: '', password: '', metodoNotificacion: 'email'
})
const recaptchaToken = ref('')
const recaptchaSiteKey = computed(() => config.public.recaptchaSiteKey.trim())
const requiereRecaptcha = computed(() => recaptchaSiteKey.value.length > 0)
const bypassDesarrollo = computed(() => import.meta.dev && !requiereRecaptcha.value)
const puedeEnviar = computed(() => bypassDesarrollo.value || recaptchaToken.value.length > 0)

// Webcam
const videoEl       = ref<HTMLVideoElement | null>(null)
const canvasEl      = ref<HTMLCanvasElement | null>(null)
const streamActivo  = ref(false)
const fotoCapturada = ref<string | null>(null)
let   mediaStream: MediaStream | null = null

const filtros = [
  { id: 'normal', nombre: 'Natural', css: 'none', canvas: 'none' },
  { id: 'calido', nombre: 'Cálido', css: 'sepia(.28) saturate(1.15)', canvas: 'sepia(28%) saturate(115%)' },
  { id: 'azul', nombre: 'Azul UMG', css: 'hue-rotate(175deg) saturate(1.1)', canvas: 'hue-rotate(175deg) saturate(110%)' },
  { id: 'gris', nombre: 'Clásico', css: 'grayscale(1) contrast(1.08)', canvas: 'grayscale(100%) contrast(108%)' }
]
const filtroActual = ref(filtros[0]!)

async function siguientePaso() {
  error.value = ''
  if (!puedeEnviar.value) {
    error.value = 'Completa la verificación reCAPTCHA para continuar.'
    return
  }
  const { valid } = await formPaso1.value?.validate()
  if (!valid) return
  paso.value = 2
  await nextTick()
  await iniciarCamara()
}

async function iniciarCamara() {
  try {
    mediaStream = await navigator.mediaDevices.getUserMedia({ video: true })
    if (videoEl.value) {
      videoEl.value.srcObject = mediaStream
      streamActivo.value = true
    }
  } catch {
    error.value = 'No se pudo acceder a la cámara. Puedes continuar sin foto.'
  }
}

function capturarFoto() {
  if (!videoEl.value || !canvasEl.value) return
  const ctx = canvasEl.value.getContext('2d')!
  ctx.drawImage(videoEl.value, 0, 0, 640, 480)
  fotoCapturada.value = canvasEl.value.toDataURL('image/jpeg', 0.8)
  detenerCamara()
}

async function crearFotoPersonalizada() {
  if (!fotoCapturada.value) return null
  const imagen = new Image()
  imagen.src = fotoCapturada.value
  await imagen.decode()
  const lienzo = document.createElement('canvas')
  lienzo.width = 640
  lienzo.height = 480
  const contexto = lienzo.getContext('2d')!
  contexto.filter = filtroActual.value.canvas
  contexto.drawImage(imagen, 0, 0, lienzo.width, lienzo.height)
  return lienzo.toDataURL('image/jpeg', 0.85)
}

function retomar() {
  fotoCapturada.value = null
  iniciarCamara()
}

function detenerCamara() {
  mediaStream?.getTracks().forEach(t => t.stop())
  streamActivo.value = false
}

async function registrar() {
  error.value = ''
  cargando.value = true
  try {
    const payload = {
      correo:             form.correo,
      telefono:           form.telefono,
      fechaNacimiento:    form.fechaNacimiento,
      nickname:           form.nickname,
      password:           form.password,
      metodoNotificacion: form.metodoNotificacion,
      fotoBase64:         fotoCapturada.value,
      fotoModificadaBase64: await crearFotoPersonalizada(),
      recaptchaToken:     bypassDesarrollo.value ? 'dev-bypass' : recaptchaToken.value
    }
    const { data } = await api.post('/auth/registro', payload)
    auth.login(data)
    paso.value = 3
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'Error al registrarse.'
  } finally {
    cargando.value = false
  }
}

async function descargarCredencial() {
  descargando.value = true
  try {
    const { data } = await api.get('/auth/credencial', { responseType: 'blob' })
    const url = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
    const enlace = document.createElement('a')
    enlace.href = url
    enlace.download = `credencial-${auth.user?.nickname || 'usuario'}.pdf`
    enlace.click()
    URL.revokeObjectURL(url)
  } catch {
    error.value = 'No se pudo generar la credencial.'
  } finally {
    descargando.value = false
  }
}

onUnmounted(() => detenerCamara())
</script>

<style scoped>
.auth-logo {
  width: 154px; height: 154px;
  object-fit: contain;
  background: transparent;
  border: 0;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 0 18px 8px rgba(180,139,33,.38), 0 14px 28px rgba(5,27,46,.22);
}
.gap-2 { gap: 8px; }
.registro-card { position: relative; z-index: 2; width: min(100%, 650px); max-width: 650px; height: calc(100dvh - 48px); padding: 20px 26px !important; overflow: hidden; display: flex; flex-direction: column; border: 1px solid rgba(180,139,33,.42) !important; box-shadow: 0 24px 70px rgba(5,27,46,.42) !important; }
.registro-card .auth-logo { width: 98px; height: 98px; box-shadow: 0 0 14px 6px rgba(180,139,33,.3), 0 10px 22px rgba(5,27,46,.18); }
.registro-card .text-center.mb-6 { margin-bottom: 12px !important; }.registro-card .text-h5 { font-size: 23px !important; line-height: 1.15; }.registro-card .text-body-2 { font-size: 13px !important; }
.registro-card :deep(.v-stepper) { display: flex; flex: 1; min-height: 0; flex-direction: column; }
.registro-card :deep(.v-stepper-header) { min-height: 74px; }.registro-card :deep(.v-stepper-item) { padding: 8px 4px; }.registro-card :deep(.v-stepper-item__title) { font-size: 13px; }.registro-card :deep(.v-stepper-item__avatar) { width: 26px; height: 26px; font-size: 12px; }
.registro-card :deep(.v-stepper-window) { flex: 1; min-height: 0; overflow-y: auto; overscroll-behavior: contain; padding-right: 6px; }
.registro-card :deep(.v-stepper-window::-webkit-scrollbar) { width: 5px; }
.registro-card :deep(.v-stepper-window::-webkit-scrollbar-thumb) { background: #B48B21; border-radius: 99px; }
.auth-access-tabs { display: inline-flex; gap: 4px; padding: 4px; margin: 0 auto 14px; background: #F5F6F7; border: 1px solid #E2E4E8; border-radius: 999px; }
.auth-access-tabs button { padding: 8px 17px; color: #6B7280; border-radius: 999px; font-size: 12px; font-weight: 700; text-decoration: none; }
.auth-access-tabs button.is-active { color: #051B2E; background: #fff; box-shadow: 0 2px 7px rgba(5,27,46,.12); }
@media (max-width: 700px) { .registro-card { height: calc(100dvh - 32px); } }
.auth-access-tabs button { border: 0; background: transparent; cursor: pointer; }
.auth-inline-link { border: 0; background: transparent; color: #7A5E12; font: inherit; font-weight: 700; text-decoration: underline; cursor: pointer; }
</style>
