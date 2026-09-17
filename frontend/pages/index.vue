<template>
  <div class="auth-page">
    <div class="auth-background" aria-hidden="true">
      <span class="auth-background__slide auth-background__slide--one" />
      <span class="auth-background__slide auth-background__slide--two" />
      <span class="auth-background__slide auth-background__slide--three" />
      <span class="auth-background__slide auth-background__slide--four" />
    </div>

    <v-card :class="['auth-card', 'login-card', 'pa-8', { 'login-card--success': accesoExitoso }]" elevation="0">
      <!-- Logo -->
      <div class="text-center mb-6">
        <img class="auth-logo login-logo mx-auto mb-3" src="/images/logo-umg-oficial.png"
             alt="Universidad Mariano Gálvez de Guatemala" />
      </div>

      <v-form @submit.prevent="loginPassword" class="login-form">
        <label class="login-field-label" for="identificador">Correo o nickname</label>
        <v-text-field
          id="identificador"
          v-model="form.identificador"
          placeholder="tu@correo.com o nickname"
          prepend-inner-icon="mdi-account"
          :error="!!error"
          hide-details
          base-color="primary"
          color="primary"
          class="login-password-field mb-4"
          @update:model-value="error = ''"
        />

        <label class="login-field-label" for="clave-acceso">Clave de acceso</label>
        <v-text-field
          id="clave-acceso"
          v-model="form.password"
          placeholder="Ingresa tu clave"
          prepend-inner-icon="mdi-lock"
          :type="mostrarPass ? 'text' : 'password'"
          :append-inner-icon="mostrarPass ? 'mdi-eye-off' : 'mdi-eye'"
          @click:append-inner="mostrarPass = !mostrarPass"
          :error="!!error"
          hide-details
          base-color="primary"
          color="primary"
          class="login-password-field"
          @update:model-value="error = ''"
        />

        <p v-if="error" class="login-field-error">
          <v-icon size="15" start>mdi-alert-circle-outline</v-icon>{{ error }}
        </p>

        <v-btn
          type="submit"
          color="accent"
          class="umg-gold-button login-access-button mt-10"
          block
          size="large"
          :loading="cargando"
          prepend-icon="mdi-login"
        >
          Ingresar
        </v-btn>
      </v-form>

      <div class="text-center mt-4">
        <v-divider class="my-4" />
        <v-btn
          variant="outlined"
          color="primary"
          block
          prepend-icon="mdi-face-recognition"
          @click="abrirLoginFacial"
        >
          Ingresar con reconocimiento facial
        </v-btn>
      </div>

      <div class="text-center mt-5">
        <v-btn variant="text" color="primary" size="small" @click="mostrarReset = true">
          ¿Olvidaste tu contraseña?
        </v-btn>
      </div>

      <div class="text-center mt-2">
        <span class="text-body-2 text-medium-emphasis">¿No tienes cuenta? </span>
        <NuxtLink to="/registro" class="font-weight-bold" style="color:#B48B21">Crear cuenta</NuxtLink>
      </div>
    </v-card>

    <v-dialog v-model="mostrarReset" max-width="420">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Restablecer contraseña</v-card-title>
        <v-card-text class="px-0">
          La recuperación de contraseña estará disponible al conectar la base de datos.
          Por el momento utiliza la clave temporal asignada.
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn class="umg-gold-button" color="accent" @click="mostrarReset = false">Entendido</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="mostrarFacial" max-width="440" @update:model-value="onCerrarFacial">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Reconocimiento facial</v-card-title>
        <v-card-text class="px-0 text-center">
          <div class="webcam-container mb-3">
            <video ref="videoFacialEl" autoplay playsinline
                   style="width:100%;border-radius:8px;object-fit:cover" />
          </div>
          <canvas ref="canvasFacialEl" style="display:none" width="640" height="480" />

          <p class="text-body-2 text-medium-emphasis mb-2">
            Coloca tu rostro frente a la cámara y presiona "Verificar".
          </p>

          <v-alert v-if="errorFacial" type="error" variant="tonal" density="compact" class="mb-2">
            {{ errorFacial }}
          </v-alert>
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn variant="outlined" @click="mostrarFacial = false">Cancelar</v-btn>
          <v-btn class="umg-gold-button" color="accent" :loading="verificandoFacial"
                 prepend-icon="mdi-camera-iris" @click="verificarRostro">
            Verificar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Iniciar sesión' })

const auth     = useAuthStore()
const { api }  = useApi()
const { cargarModelos, obtenerDescriptor } = useFacialRecognition()

const mostrarPass  = ref(false)
const cargando     = ref(false)
const error        = ref('')
const mostrarReset = ref(false)
const accesoExitoso = ref(false)

// Login facial
const mostrarFacial    = ref(false)
const verificandoFacial = ref(false)
const errorFacial      = ref('')
const videoFacialEl    = ref<HTMLVideoElement | null>(null)
const canvasFacialEl   = ref<HTMLCanvasElement | null>(null)
let   streamFacial: MediaStream | null = null

const form = reactive({ identificador: '', password: '' })

onMounted(() => {
  auth.restore()
  if (auth.isAuthenticated) navigateTo('/dashboard')
})

async function loginPassword() {
  error.value = ''
  if (!form.identificador.trim() || !form.password.trim()) {
    error.value = 'Ingresa tu correo/nickname y tu clave para continuar.'
    return
  }

  cargando.value = true
  try {
    const { data } = await api.post('/auth/login', {
      identificador:  form.identificador.trim(),
      password:       form.password,
      recaptchaToken: import.meta.dev ? 'dev-bypass' : ''
    })

    accesoExitoso.value = true
    auth.login(data)
    await new Promise(resolve => setTimeout(resolve, 650))
    await navigateTo('/dashboard')
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'La clave de acceso no es correcta.'
  } finally {
    cargando.value = false
  }
}

async function abrirLoginFacial() {
  errorFacial.value = ''
  mostrarFacial.value = true
  cargarModelos()
  await nextTick()
  try {
    streamFacial = await navigator.mediaDevices.getUserMedia({ video: true })
    if (videoFacialEl.value) videoFacialEl.value.srcObject = streamFacial
  } catch {
    errorFacial.value = 'No se pudo acceder a la cámara.'
  }
}

function detenerCamaraFacial() {
  streamFacial?.getTracks().forEach(t => t.stop())
  streamFacial = null
}

function onCerrarFacial(abierto: boolean) {
  if (!abierto) detenerCamaraFacial()
}

async function verificarRostro() {
  if (!videoFacialEl.value || !canvasFacialEl.value) return
  errorFacial.value = ''
  verificandoFacial.value = true

  try {
    const ctx = canvasFacialEl.value.getContext('2d')!
    ctx.drawImage(videoFacialEl.value, 0, 0, 640, 480)

    const descriptor = await obtenerDescriptor(canvasFacialEl.value)
    if (!descriptor) {
      errorFacial.value = 'No se detectó un rostro. Acércate e inténtalo de nuevo.'
      return
    }

    const { data } = await api.post('/auth/login-facial', {
      descriptor,
      recaptchaToken: import.meta.dev ? 'dev-bypass' : ''
    })

    auth.login(data)
    mostrarFacial.value = false
    detenerCamaraFacial()
    await navigateTo('/dashboard')
  } catch (e: any) {
    errorFacial.value = e.response?.data?.mensaje || 'Rostro no reconocido.'
  } finally {
    verificandoFacial.value = false
  }
}
</script>

<style scoped>
.auth-logo {
  width: 154px; height: 154px;
  object-fit: contain;
  background: transparent;
  border: 0;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  outline: none;
  box-shadow:
    0 0 18px 8px rgba(180,139,33,.38),
    0 14px 28px rgba(5,27,46,.22);
}

.login-field-label {
  display: block;
  margin-bottom: 8px;
  color: #1A1F2B;
  font-size: 13px;
  font-weight: 700;
  letter-spacing: .01em;
}

:deep(.login-password-field .v-field) {
  min-height: 56px;
  border-radius: 9px;
  background: #F5F6F7;
}

:deep(.login-password-field .v-field--focused) {
  box-shadow: 0 0 0 3px rgba(26,80,128,.14);
}

.login-field-error {
  display: flex;
  align-items: center;
  margin: 8px 0 0;
  color: #B42318;
  font-size: 12px;
  line-height: 1.35;
}
</style>
