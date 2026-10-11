<template>
  <div class="auth-page">
    <div class="auth-background" aria-hidden="true">
      <span class="auth-background__slide auth-background__slide--one" />
      <span class="auth-background__slide auth-background__slide--two" />
      <span class="auth-background__slide auth-background__slide--three" />
      <span class="auth-background__slide auth-background__slide--four" />
    </div>

    <v-card
      :class="['auth-card', 'auth-card--shell', 'login-card', 'pa-8', { 'login-card--success': accesoExitoso }]"
      elevation="0"
    >
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
          :disabled="!puedeEnviar"
          @click="abrirLoginFacial"
        >
          Ingresar con reconocimiento facial
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

    <v-dialog v-model="mostrarFacial" max-width="440">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Reconocimiento facial</v-card-title>
        <v-card-text class="px-0 text-center">
          <FaceScanner
            v-if="mostrarFacial"
            ref="escanerLogin"
            :ocupado="verificandoFacial"
            @capturado="verificarRostro"
            @error="errorFacial = $event"
          />
          <p class="text-body-2 text-medium-emphasis my-3">
            Coloca tu rostro dentro del marco. El escaneo comienza solo.
          </p>

          <v-alert v-if="errorFacial" type="error" variant="tonal" density="compact" class="mb-2">
            {{ errorFacial }}
          </v-alert>
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn variant="outlined" @click="mostrarFacial = false">Cancelar</v-btn>
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
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Acceso | Analizador Léxico' })

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

// Login facial
const mostrarFacial     = ref(false)
const verificandoFacial = ref(false)
const errorFacial       = ref('')
const escanerLogin      = ref<{ reiniciar: () => void } | null>(null)

const form = reactive({ identificador: '', password: '' })
const recaptchaToken = ref('')
const recaptchaSiteKey = computed(() => config.public.recaptchaSiteKey.trim())
const requiereRecaptcha = computed(() => recaptchaSiteKey.value.length > 0)
const bypassDesarrollo = computed(() => import.meta.dev && !requiereRecaptcha.value)
const puedeEnviar = computed(() => bypassDesarrollo.value || recaptchaToken.value.length > 0)
// El cambio entre acceso y registro ocurre dentro de la misma vista. No usamos
// navegación para evitar remontar la tarjeta ni recuperar un formulario anterior.
const modoRegistro = ref(route.query.modo === 'registro')

// Si se llega desde un enlace externo (por ejemplo, la barra de navegación),
// Nuxt reutiliza esta misma página al pasar de /login?modo=registro a /login.
// Sin esta sincronización quedaba visible el formulario de registro anterior.
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

  // Actualiza la URL sin añadir una entrada al historial ni recargar componentes.
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
  if (auth.isAuthenticated) navigateTo('/dashboard')
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
    accesoExitoso.value = true
    auth.login(data)
    await new Promise(resolve => setTimeout(resolve, 650))
    await navigateTo('/dashboard')
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || 'No fue posible iniciar sesión.'
  } finally {
    cargando.value = false
  }
}

function abrirLoginFacial() {
  errorFacial.value = ''
  mostrarFacial.value = true
}

async function verificarRostro(captura: { descriptor: number[] | null }) {
  if (!captura.descriptor || verificandoFacial.value) return
  errorFacial.value = ''
  verificandoFacial.value = true

  try {
    const { data } = await api.post('/auth/login-facial', {
      descriptor: captura.descriptor,
      recaptchaToken: bypassDesarrollo.value ? 'dev-bypass' : recaptchaToken.value
    })

    auth.login(data)
    mostrarFacial.value = false
    await navigateTo('/dashboard')
  } catch (e: any) {
    errorFacial.value = e.response?.data?.mensaje || 'Rostro no reconocido.'
    // Pausa breve para no saturar al servidor y luego vuelve a escanear.
    setTimeout(() => { errorFacial.value = ''; escanerLogin.value?.reiniciar() }, 1800)
  } finally {
    verificandoFacial.value = false
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
  display: flex;
  flex-direction: column;
  /* La tarjeta no cambia de tamaño: solo se intercambia su contenido. */
  height: min(760px, calc(100dvh - 48px));
  overflow: hidden;
}
.auth-access-tabs { align-self: flex-end; margin: 4px -28px 10px auto; }
.auth-card--shell :deep(.auth-form-content) { display: flex; flex: 1; min-height: 0; flex-direction: column; }
@media (max-width: 700px) { .auth-card--shell { height: calc(100dvh - 32px); } }

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
@media (prefers-reduced-motion: reduce) {
  .auth-access-tabs::before, .auth-access-tabs button { transition: none; }
  .auth-flip-enter-active, .auth-flip-leave-active { transition: none; }
}
.login-register-link button { border: 0; background: transparent; color: #7A5E12; font: inherit; font-weight: 700; text-decoration: underline; cursor: pointer; }
</style>
