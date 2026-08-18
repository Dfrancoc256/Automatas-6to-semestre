<template>
  <div class="auth-page">
    <v-card class="auth-card pa-8" elevation="0" style="max-width:520px">
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

              <v-btn color="accent" class="umg-gold-button" block size="large" type="submit">
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
                <img :src="fotoCapturada"
                     style="width:120px;height:120px;border-radius:50%;object-fit:cover;border:3px solid #B48B21" />
                <p class="text-caption mt-1 text-success">✓ Foto capturada</p>
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
        <NuxtLink to="/" class="font-weight-bold" style="color:#B48B21">Inicia sesión</NuxtLink>
      </div>
    </v-card>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Registro' })

const auth    = useAuthStore()
const { api } = useApi()

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

// Webcam
const videoEl       = ref<HTMLVideoElement | null>(null)
const canvasEl      = ref<HTMLCanvasElement | null>(null)
const streamActivo  = ref(false)
const fotoCapturada = ref<string | null>(null)
let   mediaStream: MediaStream | null = null

async function siguientePaso() {
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
      recaptchaToken:     import.meta.dev ? 'dev-bypass' : ''
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
  width: 72px; height: 72px;
  object-fit: contain;
  background: white;
  border: 3px solid #B48B21;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 10px 28px rgba(15,72,102,.30);
}
.gap-2 { gap: 8px; }
</style>
