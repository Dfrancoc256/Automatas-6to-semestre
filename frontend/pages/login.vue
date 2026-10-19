<template>
  <div class="auth-page">
    <div class="auth-background" aria-hidden="true">
      <span class="auth-background__slide auth-background__slide--one" />
      <span class="auth-background__slide auth-background__slide--two" />
      <span class="auth-background__slide auth-background__slide--three" />
      <span class="auth-background__slide auth-background__slide--four" />
    </div>

    <v-card
      :class="['auth-card', 'auth-card--shell', 'login-card', 'pa-8', {
        'auth-card--login': !modoRegistro,
        'auth-card--register': modoRegistro,
        'login-card--success': accesoExitoso
      }]"
      elevation="0"
    >
      <v-btn
        variant="text"
        color="primary"
        size="small"
        prepend-icon="mdi-arrow-left"
        class="auth-back-home"
        @click="navigateTo('/')"
      >
        Regresar al inicio
      </v-btn>

      <div
        :class="['auth-access-tabs', modoRegistro ? 'auth-access-tabs--registro' : 'auth-access-tabs--login']"
        aria-label="Acceso y registro"
      >
        <button type="button" :class="{ 'is-active': !modoRegistro }" :aria-pressed="!modoRegistro" @click="cambiarModo('login')">Ingresar</button>
        <button type="button" :class="{ 'is-active': modoRegistro }" :aria-pressed="modoRegistro" @click="cambiarModo('registro')">Crear cuenta</button>
      </div>

      <Transition name="auth-flip" mode="out-in">
      <div v-if="!modoRegistro" key="login" class="auth-form-content">
      <!-- Logo -->
      <div class="text-center mb-6">
        <img class="auth-logo login-logo mx-auto mb-3" src="/images/logo-umg-oficial.png"
             alt="Universidad Mariano Gálvez de Guatemala" />
      </div>

      <v-form @submit.prevent="loginPassword" class="login-form">
        <label class="login-field-label" for="identificador">Correo o nickname</label>
        <v-text-field
          id="identificador"
          v-model.trim="form.identificador"
          placeholder="Ingresa tu correo o nickname"
          prepend-inner-icon="mdi-account-outline"
          autocomplete="username"
          :error="!!error"
          hide-details
          base-color="primary"
          color="primary"
          class="login-password-field mb-5"
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
          autocomplete="current-password"
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

        <RecaptchaV2
          v-if="requiereRecaptcha"
          ref="recaptcha"
          v-model:token="recaptchaToken"
          :site-key="recaptchaSiteKey"
          class="mt-6"
        />
        <v-alert v-else-if="!bypassDesarrollo" type="warning" variant="tonal" density="compact" class="mt-6">
          Falta configurar la clave pública de reCAPTCHA.
        </v-alert>

        <v-btn
          type="submit"
          color="accent"
          class="umg-gold-button login-access-button mt-10"
          block
          size="large"
          :loading="cargando"
          :disabled="!puedeEnviar"
          prepend-icon="mdi-login"
        >
          Ingresar
        </v-btn>
      </v-form>

      <div class="text-center mt-4">
        <v-divider class="mb-4" />
        <v-btn
          variant="outlined"
          color="primary"
          block
          prepend-icon="mdi-face-recognition"
          :disabled="!puedeLoginFacial"
          @click="abrirLoginFacial"
        >
          {{ intentosFacialesRestantes ? 'Ingresar con reconocimiento facial' : 'Reconocimiento facial no disponible' }}
        </v-btn>
      </div>

      <div class="text-center mt-5">
        <v-btn variant="text" color="primary" size="small" @click="mostrarReset = true">
          ¿Olvidaste tu contraseña?
        </v-btn>
        <p class="login-register-link">¿Aún no tienes cuenta? <button type="button" @click="cambiarModo('registro')">Regístrate aquí</button></p>
      </div>
      </div>

      <AuthRegistrationForm v-else key="registro" class="auth-form-content" @cambiar-modo="cambiarModo" />
      </Transition>
    </v-card>

    <v-dialog v-model="mostrarFacial" max-width="440" :persistent="verificandoFacial || rostroValidado">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Reconocimiento facial</v-card-title>
        <v-card-text class="px-0 text-center">
          <FaceScanner
            v-if="mostrarFacial && !rostroValidado"
            ref="escanerLogin"
            con-foto
            :ocupado="verificandoFacial"
            @capturado="verificarRostro"
            @error="errorFacial = $event"
          />
          <div v-else-if="rostroValidado" class="facial-success py-7" role="status" aria-live="polite">
            <v-avatar v-if="fotoLoginRecortada" size="88" class="mb-3 facial-success__photo">
              <v-img :src="fotoLoginRecortada" cover eager />
            </v-avatar>
            <v-icon v-else color="success" size="70" class="mb-3">mdi-check-circle</v-icon>
            <h2 class="text-h6 mb-2">Identidad validada</h2>
            <p class="text-body-2 text-medium-emphasis mb-0">Acceso correcto. Estamos preparando tu dashboard.</p>
          </div>
          <p v-if="!rostroValidado" class="text-body-2 text-medium-emphasis my-3">
            Coloca tu rostro dentro del marco. El escaneo comienza solo. Intentos disponibles: {{ intentosFacialesRestantes }}.
          </p>

          <v-alert v-if="errorFacial" type="error" variant="tonal" density="compact" class="mb-2">
            {{ errorFacial }}
          </v-alert>
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn variant="outlined" :disabled="verificandoFacial || rostroValidado" @click="mostrarFacial = false">Cancelar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="mostrarReset" max-width="420">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Restablecer contraseña</v-card-title>
        <v-card-text class="px-0">
          Ingresa el correo de tu cuenta para recibir las instrucciones de recuperación.
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn class="umg-gold-button" color="accent" @click="mostrarReset = false">Entendido</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="mostrarCambioPassword" max-width="440" persistent>
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Crea tu contraseña personal</v-card-title>
        <v-card-text class="px-0">
          <p class="text-body-2 text-medium-emphasis mb-4">
            Tu contraseña temporal ya fue validada. Define una contraseña personal para continuar.
          </p>
          <v-text-field
            v-model="nuevaPassword"
            label="Nueva contraseña"
            type="password"
            autocomplete="new-password"
            hint="Mínimo 8 caracteres, mayúscula, minúscula, número y símbolo"
            persistent-hint
            class="mb-3"
          />
          <v-text-field
            v-model="confirmarNuevaPassword"
            label="Confirmar nueva contraseña"
            type="password"
            autocomplete="new-password"
          />
          <v-alert v-if="errorCambioPassword" type="error" variant="tonal" density="compact" class="mt-3">
            {{ errorCambioPassword }}
          </v-alert>
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn color="accent" class="umg-gold-button" :loading="cambiandoPassword" @click="cambiarPasswordTemporal">
            Guardar y continuar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
// Conexiones necesarias para reCAPTCHA.
useHead({
  title: 'Acceso | Analizador Léxico',
  link: [
    { rel: 'preconnect', href: 'https://www.google.com' },
    { rel: 'preconnect', href: 'https://www.gstatic.com', crossorigin: 'anonymous' },
    { rel: 'dns-prefetch', href: '//www.google.com' },
    { rel: 'dns-prefetch', href: '//www.gstatic.com' }
  ]
})

const auth     = useAuthStore()
const route = useRoute()
const { api }  = useApi()
const config = useRuntimeConfig()
const { calentar } = useFacialRecognition()
const mostrarPass  = ref(false)
const cargando     = ref(false)
const error        = ref('')
const mostrarReset = ref(false)
const accesoExitoso = ref(false)
const mostrarCambioPassword = ref(false)
const nuevaPassword = ref('')
const confirmarNuevaPassword = ref('')
const errorCambioPassword = ref('')
const cambiandoPassword = ref(false)

// Login facial
const mostrarFacial     = ref(false)
const verificandoFacial = ref(false)
const errorFacial       = ref('')
const rostroValidado    = ref(false)
const fotoLoginRecortada = ref<string | null>(null)
const escanerLogin      = ref<{ reiniciar: () => void } | null>(null)
const recaptcha = ref<{ reiniciar: () => void } | null>(null)
const intentosFaciales = ref(0)
const maxIntentosFaciales = 3

const form = reactive({ identificador: '', password: '' })
const recaptchaToken = ref('')
const recaptchaSiteKey = computed(() => config.public.recaptchaSiteKey.trim())
const requiereRecaptcha = computed(() => recaptchaSiteKey.value.length > 0)
const bypassDesarrollo = computed(() => import.meta.dev && !requiereRecaptcha.value)
const puedeEnviar = computed(() => bypassDesarrollo.value || recaptchaToken.value.length > 0)
const intentosFacialesRestantes = computed(() => Math.max(0, maxIntentosFaciales - intentosFaciales.value))
const puedeLoginFacial = computed(() => puedeEnviar.value && intentosFacialesRestantes.value > 0)
// El acceso y el registro comparten esta vista.
const modoRegistro = ref(route.query.modo === 'registro')

// Sincroniza el modo al cambiar la URL.
watch(
  () => route.query.modo,
  (modo) => {
    modoRegistro.value = modo === 'registro'
  },
)

function cambiarModo(modo: 'login' | 'registro') {
  const mostrarRegistro = modo === 'registro'
  if (modoRegistro.value === mostrarRegistro) return

  modoRegistro.value = mostrarRegistro

  // Actualiza la URL sin recargar la vista.
  if (import.meta.client) {
    const url = new URL(window.location.href)
    if (mostrarRegistro) url.searchParams.set('modo', 'registro')
    else url.searchParams.delete('modo')
    window.history.replaceState(window.history.state, '', url)
  }
}

onMounted(() => {
  calentar()
  auth.restore()
  if (auth.isAuthenticated) navigateTo(rutaDestino.value)
})

// Solo permite redirecciones internas.
const rutaDestino = computed(() => {
  const destino = route.query.redirect
  return typeof destino === 'string' && destino.startsWith('/dashboard')
    ? destino
    : '/dashboard'
})

async function loginPassword() {
  error.value = ''
  if (!identificadorValido(form.identificador)) {
    error.value = 'Ingresa un correo válido o un nickname de 3 a 50 caracteres.'
    return
  }
  if (form.password.length < 8) {
    error.value = 'La contraseña debe tener al menos 8 caracteres.'
    return
  }
  if (!puedeEnviar.value) {
    error.value = 'Completa la verificación reCAPTCHA para continuar.'
    return
  }

  cargando.value = true
  try {
    const { data } = await api.post('/auth/login', {
      identificador: form.identificador,
      password: form.password,
      recaptchaToken: bypassDesarrollo.value ? 'dev-bypass' : recaptchaToken.value
    })
    if (data.debeCambiarPassword) {
      abrirCambioPassword()
      return
    }
    accesoExitoso.value = true
    auth.login(data)
    await new Promise(resolve => setTimeout(resolve, 650))
    await navigateTo(rutaDestino.value)
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'No fue posible iniciar sesión.'
    if (requiereRecaptcha.value && !bypassDesarrollo.value) {
      recaptchaToken.value = ''
      recaptcha.value?.reiniciar()
    }
  } finally {
    cargando.value = false
  }
}

function abrirLoginFacial() {
  if (!puedeLoginFacial.value) {
    error.value = intentosFacialesRestantes.value === 0
      ? 'Se agotaron los tres intentos de reconocimiento facial. Espera diez minutos antes de volver a intentarlo.'
      : 'Completa la verificación reCAPTCHA para continuar.'
    return
  }

  error.value = ''
  errorFacial.value = ''
  rostroValidado.value = false
  fotoLoginRecortada.value = null
  mostrarFacial.value = true
}

async function verificarRostro(captura: { descriptor: number[] | null, fotoRecortada: string | null }) {
  if (!captura.descriptor || !captura.fotoRecortada || verificandoFacial.value) return
  if (!puedeLoginFacial.value) {
    errorFacial.value = 'Completa nuevamente la verificación reCAPTCHA antes de escanear el rostro.'
    return
  }

  errorFacial.value = ''
  verificandoFacial.value = true

  try {
    const { data } = await api.post('/auth/login-facial', {
      descriptor: captura.descriptor,
      recaptchaToken: bypassDesarrollo.value ? 'dev-bypass' : recaptchaToken.value
    })

    if (data.debeCambiarPassword) {
      mostrarFacial.value = false
      abrirCambioPassword()
      return
    }

    auth.login(data)
    fotoLoginRecortada.value = captura.fotoRecortada
    rostroValidado.value = true
    // Muestra la validación antes de redirigir.
    await new Promise(resolve => setTimeout(resolve, 1200))
    mostrarFacial.value = false
    await navigateTo(rutaDestino.value)
  } catch (e: any) {
    const estado = e.response?.status
    if (estado === 401) intentosFaciales.value += 1

    recaptchaToken.value = ''
    recaptcha.value?.reiniciar()

    if (estado === 429 || intentosFacialesRestantes.value === 0) {
      errorFacial.value = 'Se agotaron los tres intentos de reconocimiento facial. Espera diez minutos antes de volver a intentarlo.'
    } else {
      errorFacial.value = estado === 401
        ? `Rostro no reconocido. Te quedan ${intentosFacialesRestantes.value} intento(s). reCAPTCHA se reinició para el siguiente intento.`
        : e.response?.data?.mensaje || 'No fue posible validar el rostro. reCAPTCHA se reinició para reintentar.'
    }

    window.setTimeout(() => {
      mostrarFacial.value = false
      error.value = errorFacial.value
      errorFacial.value = ''
    }, 2200)
  } finally {
    verificandoFacial.value = false
  }
}

function abrirCambioPassword() {
  nuevaPassword.value = ''
  confirmarNuevaPassword.value = ''
  errorCambioPassword.value = ''
  mostrarCambioPassword.value = true
}

async function cambiarPasswordTemporal() {
  errorCambioPassword.value = ''
  const cumpleRegla = /^(?=.{8,128}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$/.test(nuevaPassword.value)
  if (!cumpleRegla) {
    errorCambioPassword.value = 'Usa entre 8 y 128 caracteres, con mayúscula, minúscula, número y símbolo.'
    return
  }
  if (nuevaPassword.value !== confirmarNuevaPassword.value) {
    errorCambioPassword.value = 'La confirmación no coincide con la nueva contraseña.'
    return
  }

  cambiandoPassword.value = true
  try {
    const { data } = await api.post('/auth/cambiar-password-temporal', {
      nuevoPassword: nuevaPassword.value
    })
    auth.login(data)
    mostrarCambioPassword.value = false
    await navigateTo(rutaDestino.value)
  } catch (e: any) {
    errorCambioPassword.value = e.response?.data?.mensaje || 'No se pudo actualizar la contraseña.'
  } finally {
    cambiandoPassword.value = false
  }
}

function identificadorValido(valor: string) {
  const identificador = valor.trim()
  if (identificador.includes('@')) return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(identificador)
  return /^[a-zA-Z0-9_.-]{3,50}$/.test(identificador)
}
</script>

<style scoped>
/* Una única tarjeta: al cambiar de modo gira sobre el eje vertical. */
.auth-flip-enter-active,
.auth-flip-leave-active {
  transform-style: preserve-3d;
  transition: opacity .12s ease, transform .2s cubic-bezier(.22, 1, .36, 1);
}

.facial-success__photo { border: 3px solid #B48B21; }

.auth-flip-enter-from {
  opacity: 0;
  transform: perspective(1200px) rotateY(-76deg) scale(.98);
}

.auth-flip-leave-to {
  opacity: 0;
  transform: perspective(1200px) rotateY(76deg) scale(.98);
}

.auth-flip-enter-to,
.auth-flip-leave-from {
  opacity: 1;
  transform: perspective(1200px) rotateY(0) scale(1);
}

.auth-logo {
  width: 140px; height: 140px;
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

.auth-card--shell {
  position: relative;
  display: flex;
  flex-direction: column;
  /* Altura de la tarjeta. */
  height: min(820px, calc(100dvh - 32px));
  overflow: hidden;
}
.auth-access-tabs { align-self: flex-end; margin: 4px -28px 10px auto; }
.auth-back-home {
  position: absolute;
  z-index: 2;
  top: 14px;
  left: 18px;
}
.auth-card--shell :deep(.auth-form-content) {
  display: flex;
  flex: 1;
  min-height: 0;
  flex-direction: column;
  overflow-y: auto;
  overscroll-behavior: contain;
  padding: 0 5px 10px;
  scrollbar-width: thin;
  scrollbar-color: #B48B21 transparent;
}
.auth-card--shell :deep(.auth-form-content::-webkit-scrollbar) { width: 5px; }
.auth-card--shell :deep(.auth-form-content::-webkit-scrollbar-thumb) { background: #B48B21; border-radius: 999px; }
.auth-card--login :deep(.auth-form-content) { padding-top: 4px; }
@media (max-width: 700px) {
  .auth-card--shell {
    width: 100%;
    height: auto;
    min-height: calc(100dvh - 24px);
    overflow: visible;
  }
  .auth-card--shell :deep(.auth-form-content) {
    min-width: 0;
    overflow: visible;
    padding-right: 0;
  }
  .auth-back-home { top: 10px; left: 8px; }
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

.login-register-link { margin: 8px 0 0; color: #6B7280; font-size: 12px; }
.login-register-link a { color: #7A5E12; font-weight: 700; }
.auth-access-tabs { position: relative; display: inline-grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 4px; padding: 4px; background: #F5F6F7; border: 1px solid #E2E4E8; border-radius: 999px; isolation: isolate; }
.auth-access-tabs::before { content: ''; position: absolute; z-index: -1; inset: 4px auto 4px 4px; width: calc(50% - 6px); background: #fff; border-radius: 999px; box-shadow: 0 2px 7px rgba(5,27,46,.12); transition: transform .28s cubic-bezier(.22, 1, .36, 1), box-shadow .28s ease; }
.auth-access-tabs button { padding: 6px 13px; border: 0; background: transparent; color: #6B7280; border-radius: 999px; font-size: 11px; font-weight: 700; cursor: pointer; transition: color .2s ease, transform .2s ease; }
.auth-access-tabs button.is-active { color: #051B2E; }
.auth-access-tabs button:hover, .auth-access-tabs button:focus-visible { color: #051B2E; transform: translateY(-1px); outline: none; }
.auth-access-tabs--registro::before { transform: translateX(calc(100% + 4px)); }
.auth-access-tabs--login:has(button:nth-child(2):hover)::before, .auth-access-tabs--login:has(button:nth-child(2):focus-visible)::before { transform: translateX(calc(100% + 4px)); box-shadow: 0 5px 12px rgba(5,27,46,.16); }
.auth-access-tabs--registro:has(button:first-child:hover)::before, .auth-access-tabs--registro:has(button:first-child:focus-visible)::before { transform: translateX(0); box-shadow: 0 5px 12px rgba(5,27,46,.16); }
/* Ajustes móviles. */
@media (max-width: 700px) {
  .auth-access-tabs {
    align-self: center;
    width: min(100%, 264px);
    margin: 42px auto 12px;
  }

  .auth-access-tabs button {
    min-height: 40px;
    padding: 8px 12px;
    font-size: 12px;
  }
}
@media (prefers-reduced-motion: reduce) {
  .auth-access-tabs::before, .auth-access-tabs button { transition: none; }
  .auth-flip-enter-active, .auth-flip-leave-active { transition: none; }
}
.login-register-link button { border: 0; background: transparent; color: #7A5E12; font: inherit; font-weight: 700; text-decoration: underline; cursor: pointer; }
.facial-success h2 { color: #051B2E; font-weight: 700; }
</style>
