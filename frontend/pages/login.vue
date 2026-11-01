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

      <Transition :name="modoRegistro ? 'auth-to-register' : 'auth-to-login'" mode="out-in"
                  @after-enter="cambiandoModo = false">
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
          maxlength="30"
          counter="30"
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
          maxlength="35"
          counter="35"
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

        <v-alert v-if="!puedeEnviar" type="warning" variant="tonal" density="compact" class="mt-6">
          Falta configurar la clave pública de reCAPTCHA.
        </v-alert>

        <v-btn
          type="submit"
          color="accent"
          class="umg-gold-button login-access-button mt-10"
          block
          size="large"
          :loading="cargando"
          :disabled="cargando"
          prepend-icon="mdi-login"
        >
          Ingresar
        </v-btn>
      </v-form>

      <v-btn
        block
        variant="outlined"
        color="primary"
        class="mt-4 google-access-button"
        :loading="cargandoGoogle"
        :disabled="cargandoGoogle"
        @click="continuarConGoogle"
      >
        <span class="google-mark" aria-hidden="true">G</span>
        <span>Continuar con Google</span>
      </v-btn>

      <div class="text-center mt-4">
        <v-divider class="mb-4" />
        <v-btn
          variant="outlined"
          color="primary"
          block
          prepend-icon="mdi-face-recognition"
          :loading="preparandoFacial"
          :disabled="!puedeLoginFacial || preparandoFacial"
          @click="abrirLoginFacial"
        >
          {{ intentosFacialesRestantes ? 'Ingresar con reconocimiento facial' : 'Reconocimiento facial no disponible' }}
        </v-btn>
        <v-btn
          variant="outlined"
          color="primary"
          block
          class="mt-3"
          prepend-icon="mdi-qrcode-scan"
          :disabled="!puedeEnviar"
          @click="abrirEscanerQr"
        >
          Ingresar con carnet QR
        </v-btn>
      </div>

      <div class="text-center mt-5">
        <v-btn variant="text" color="primary" size="small" @click="abrirRecuperacion">
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
            :timeout-sin-rostro="30_000"
            @capturado="verificarRostro"
            @error="errorFacial = $event"
            @sin-rostro-timeout="cerrarPorAusenciaDeRostro"
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

    <v-dialog v-model="mostrarQr" max-width="460" :persistent="verificandoQr">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Ingresar con carnet QR</v-card-title>
        <v-card-text class="px-0 text-center">
          <p class="text-body-2 text-medium-emphasis mb-4">
            Enfoca con la cámara el código QR impreso en tu carnet institucional.
          </p>
          <QrLoginScanner
            v-if="mostrarQr && !verificandoQr"
            @detectado="ingresarConQr"
            @error="errorQr = $event"
          />
          <v-progress-circular v-else-if="verificandoQr" indeterminate color="accent" size="44" class="my-10" />
          <v-alert v-if="errorQr" type="error" variant="tonal" density="compact" class="mt-4">
            {{ errorQr }}
          </v-alert>
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn variant="outlined" :disabled="verificandoQr" @click="mostrarQr = false">Cancelar</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="mostrarReset" max-width="420">
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">Restablecer contraseña</v-card-title>
        <v-card-text class="px-0 pb-2">
          <p class="text-body-2 text-medium-emphasis mb-4">
            Te enviaremos un enlace de un solo uso para definir una nueva contraseña.
          </p>
          <v-text-field
            v-model.trim="correoReset"
            label="Correo electrónico"
            type="email"
            autocomplete="email"
            prepend-inner-icon="mdi-email-outline"
            :disabled="enviandoReset || !!mensajeReset"
          />
          <v-alert v-if="errorReset" type="error" variant="tonal" density="compact" class="mt-4">
            {{ errorReset }}
          </v-alert>
          <v-alert v-if="mensajeReset" type="success" variant="tonal" density="compact" class="mt-4">
            {{ mensajeReset }}
          </v-alert>
        </v-card-text>
        <v-card-actions class="px-0 justify-end">
          <v-btn variant="text" @click="mostrarReset = false">Cerrar</v-btn>
          <v-btn v-if="!mensajeReset" class="umg-gold-button" color="accent"
                 :loading="enviandoReset" :disabled="!puedeSolicitarReset" @click="solicitarRestablecimiento">
            Enviar enlace
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="mostrarCambioPassword" max-width="520" persistent scrollable>
      <v-card class="password-change-card">
        <div class="password-change-header">
          <div class="password-change-heading">
            <span class="password-change-icon"><v-icon icon="mdi-shield-key-outline" size="26" /></span>
            <div>
              <p class="password-change-eyebrow">SEGURIDAD DE TU CUENTA</p>
              <h2 class="password-change-title">Crea tu contraseña personal</h2>
            </div>
          </div>
          <v-btn icon="mdi-close" variant="text" size="small" aria-label="Cancelar cambio de contraseña" title="Cancelar" :disabled="cambiandoPassword" @click="cancelarCambioPassword" />
        </div>
        <v-card-text class="password-change-body">
          <p class="password-change-description">
            Tu contraseña temporal ya fue validada. Crea una nueva para proteger tu cuenta y continuar.
          </p>
          <v-text-field
            v-model="nuevaPassword"
            label="Nueva contraseña"
            :type="mostrarNuevaPassword ? 'text' : 'password'"
            autocomplete="new-password"
            variant="outlined"
            maxlength="35"
            counter="35"
            hide-details="auto"
            class="mb-4"
          >
            <template #append-inner>
              <v-btn :icon="mostrarNuevaPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" variant="text" size="small"
                     :aria-label="mostrarNuevaPassword ? 'Ocultar nueva contraseña' : 'Mostrar nueva contraseña'"
                     @click="mostrarNuevaPassword = !mostrarNuevaPassword" />
            </template>
          </v-text-field>

          <div class="password-strength" aria-live="polite">
            <div class="password-strength-label">
              <span>Seguridad estimada</span>
              <strong :style="{ color: nivelPassword.color }">{{ nivelPassword.etiqueta }}</strong>
            </div>
            <v-progress-linear :model-value="nivelPassword.valor" :color="nivelPassword.color"
                               bg-color="#E7EBEF" rounded height="8" />
            <p class="password-strength-note">Estimación basada en la longitud y los tipos de caracteres.</p>
          </div>

          <div class="password-rules" role="list" aria-label="Requisitos de la contraseña">
            <div v-for="regla in reglasPassword" :key="regla.etiqueta" role="listitem"
                 class="password-rule" :class="{ 'password-rule--met': regla.cumple }">
              <v-icon :icon="regla.cumple ? 'mdi-check-circle' : 'mdi-close-circle-outline'" size="18" />
              <span>{{ regla.etiqueta }}</span>
            </div>
          </div>

          <v-text-field
            v-model="confirmarNuevaPassword"
            label="Confirmar nueva contraseña"
            :type="mostrarConfirmacionPassword ? 'text' : 'password'"
            autocomplete="new-password"
            variant="outlined"
            maxlength="35"
            hide-details="auto"
            class="mt-5"
          >
            <template #append-inner>
              <v-btn :icon="mostrarConfirmacionPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'" variant="text" size="small"
                     :aria-label="mostrarConfirmacionPassword ? 'Ocultar confirmación' : 'Mostrar confirmación'"
                     @click="mostrarConfirmacionPassword = !mostrarConfirmacionPassword" />
            </template>
          </v-text-field>
          <p v-if="confirmarNuevaPassword" class="password-confirmation"
             :class="{ 'password-confirmation--met': contrasenasCoinciden }">
            <v-icon :icon="contrasenasCoinciden ? 'mdi-check-circle' : 'mdi-close-circle-outline'" size="17" />
            {{ contrasenasCoinciden ? 'Las contraseñas coinciden' : 'Las contraseñas no coinciden' }}
          </p>
          <v-alert v-if="errorCambioPassword" type="error" variant="tonal" density="compact" class="mt-4">
            {{ errorCambioPassword }}
          </v-alert>
        </v-card-text>
        <v-card-actions class="password-change-actions">
          <v-btn variant="text" :disabled="cambiandoPassword" @click="cancelarCambioPassword">Cancelar</v-btn>
          <v-btn color="accent" class="umg-gold-button" :loading="cambiandoPassword"
                 :disabled="!passwordLista" @click="cambiarPasswordTemporal">
            Guardar y continuar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { obtenerTokenRecaptcha, prepararRecaptcha } from '~/utils/recaptchaV3'
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
const correoReset = ref('')
const enviandoReset = ref(false)
const errorReset = ref('')
const mensajeReset = ref('')
const accesoExitoso = ref(false)
const mostrarQr = ref(false)
const verificandoQr = ref(false)
const errorQr = ref('')
const mostrarCambioPassword = ref(false)
const nuevaPassword = ref('')
const confirmarNuevaPassword = ref('')
const mostrarNuevaPassword = ref(false)
const mostrarConfirmacionPassword = ref(false)
const errorCambioPassword = ref('')
const cambiandoPassword = ref(false)
const reglasPassword = computed(() => [
  { etiqueta: 'Entre 8 y 35 caracteres', cumple: nuevaPassword.value.length >= 8 && nuevaPassword.value.length <= 35 },
  { etiqueta: 'Una letra mayúscula', cumple: /[A-Z]/.test(nuevaPassword.value) },
  { etiqueta: 'Una letra minúscula', cumple: /[a-z]/.test(nuevaPassword.value) },
  { etiqueta: 'Un número', cumple: /\d/.test(nuevaPassword.value) },
  { etiqueta: 'Un símbolo', cumple: /[^A-Za-z0-9]/.test(nuevaPassword.value) }
])
const contrasenasCoinciden = computed(() => confirmarNuevaPassword.value.length > 0 && nuevaPassword.value === confirmarNuevaPassword.value)
const passwordLista = computed(() => reglasPassword.value.every(regla => regla.cumple) && contrasenasCoinciden.value)
const nivelPassword = computed(() => {
  if (!nuevaPassword.value) return { etiqueta: 'Sin evaluar', valor: 0, color: '#94A3B8' }
  const cumplidas = reglasPassword.value.filter(regla => regla.cumple).length
  if (cumplidas < 3) return { etiqueta: 'Débil', valor: 25, color: '#B42318' }
  if (cumplidas < 5) return { etiqueta: 'En progreso', valor: 50, color: '#C47F00' }
  if (nuevaPassword.value.length < 12) return { etiqueta: 'Adecuada', valor: 75, color: '#2E7D32' }
  return { etiqueta: 'Fuerte', valor: 100, color: '#15803D' }
})
const cargandoGoogle = ref(false)

// Login facial
const mostrarFacial     = ref(false)
const verificandoFacial = ref(false)
const preparandoFacial  = ref(false)
const errorFacial       = ref('')
const rostroValidado    = ref(false)
const fotoLoginRecortada = ref<string | null>(null)
const escanerLogin      = ref<{ reiniciar: () => void } | null>(null)
const intentosFaciales = ref(0)
const maxIntentosFaciales = 3
const permisoFacialActivo = ref(false)

const form = reactive({ identificador: '', password: '' })
const recaptchaSiteKey = computed(() => config.public.recaptchaSiteKey.trim())
const requiereRecaptcha = computed(() => recaptchaSiteKey.value.length > 0)
const bypassDesarrollo = computed(() => import.meta.dev && !requiereRecaptcha.value)
const puedeEnviar = computed(() => bypassDesarrollo.value || requiereRecaptcha.value)
const puedeSolicitarReset = computed(() =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correoReset.value)
  && puedeEnviar.value
)
const tokenSeguridad = (accion: string) => bypassDesarrollo.value
  ? Promise.resolve('dev-bypass')
  : obtenerTokenRecaptcha(recaptchaSiteKey.value, accion)
const intentosFacialesRestantes = computed(() => Math.max(0, maxIntentosFaciales - intentosFaciales.value))
const puedeLoginFacial = computed(() =>
  (permisoFacialActivo.value || puedeEnviar.value) && intentosFacialesRestantes.value > 0
)
// El acceso y el registro comparten esta vista.
const modoRegistro = ref(route.query.modo === 'registro')
const cambiandoModo = ref(false)

// Sincroniza el modo al cambiar la URL.
watch(
  () => route.query.modo,
  (modo) => {
    modoRegistro.value = modo === 'registro'
  },
)

function cambiarModo(modo: 'login' | 'registro') {
  const mostrarRegistro = modo === 'registro'
  if (modoRegistro.value === mostrarRegistro || cambiandoModo.value) return

  cambiandoModo.value = true
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
  prepararRecaptcha(recaptchaSiteKey.value)
  calentar()
  auth.restore()
  if (route.query.error === 'google-existente') {
    error.value = 'Esta cuenta de Google ya está registrada. Usa “Continuar con Google” para iniciar sesión.'
  } else if (route.query.error === 'google') {
    error.value = 'No fue posible verificar la cuenta de Google. Intenta nuevamente.'
  }
  if (auth.user?.debeCambiarPassword) {
    abrirCambioPassword()
    return
  }
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
  const identificadorInvalido = !identificadorValido(form.identificador)
  const passwordInvalida = form.password.length < 8 || form.password.length > 35
  if (identificadorInvalido && passwordInvalida) {
    error.value = 'Completa tu correo o nombre de usuario y una contraseña de entre 8 y 35 caracteres.'
    return
  }
  if (identificadorInvalido) {
    error.value = 'Ingresa un correo válido de hasta 30 caracteres o un nickname de 3 a 25 caracteres.'
    return
  }
  if (passwordInvalida) {
    error.value = 'La contraseña debe tener entre 8 y 35 caracteres.'
    return
  }
  if (!puedeEnviar.value) {
    error.value = 'Falta configurar la verificación de seguridad.'
    return
  }

  cargando.value = true
  try {
    const recaptchaToken = await tokenSeguridad('login')
    const { data } = await api.post('/auth/login', {
      identificador: form.identificador,
      password: form.password,
      recaptchaToken
    })
    if (data.debeCambiarPassword) {
      // Conserva el estado pendiente en la cookie de metadatos. De esta forma,
      // si se actualiza la página, el formulario obligatorio se vuelve a abrir.
      auth.login(data)
      abrirCambioPassword()
      return
    }
    accesoExitoso.value = true
    auth.login(data)
    await new Promise(resolve => setTimeout(resolve, 650))
    await navigateTo(rutaDestino.value)
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || e.message || 'No fue posible iniciar sesión.'
  } finally {
    cargando.value = false
  }
}

async function abrirLoginFacial() {
  if (!puedeLoginFacial.value) {
    error.value = intentosFacialesRestantes.value === 0
      ? 'Se agotaron los tres intentos de reconocimiento facial.'
      : 'Falta configurar la verificación de seguridad.'
    return
  }

  error.value = ''
  errorFacial.value = ''
  rostroValidado.value = false
  fotoLoginRecortada.value = null

  if (!permisoFacialActivo.value) {
    preparandoFacial.value = true
    try {
      const recaptchaToken = await tokenSeguridad('login_facial')
      const { data } = await api.post('/auth/facial/verificar-captcha', {
        recaptchaToken
      })
      permisoFacialActivo.value = true
      intentosFaciales.value = Math.max(0, maxIntentosFaciales - Number(data.intentosRestantes || maxIntentosFaciales))
      // El permiso se conserva hasta agotar los intentos o vencer.
    } catch (e: any) {
      error.value = e.response?.data?.mensaje || e.message || 'No fue posible habilitar el reconocimiento facial.'
      return
    } finally {
      preparandoFacial.value = false
    }
  }

  mostrarFacial.value = true
}

function abrirEscanerQr() {
  if (!puedeEnviar.value) {
    error.value = 'Falta configurar la verificación de seguridad.'
    return
  }

  error.value = ''
  errorQr.value = ''
  mostrarQr.value = true
}

async function ingresarConQr(codigoQr: string) {
  if (verificandoQr.value) return

  errorQr.value = ''
  verificandoQr.value = true
  try {
    const recaptchaToken = await tokenSeguridad('login_qr')
    const { data } = await api.post('/auth/login-qr', {
      codigoQr,
      recaptchaToken
    })

    auth.login(data)
    if (data.debeCambiarPassword) {
      mostrarQr.value = false
      abrirCambioPassword()
      return
    }

    accesoExitoso.value = true
    mostrarQr.value = false
    await new Promise(resolve => setTimeout(resolve, 650))
    await navigateTo(rutaDestino.value)
  } catch (e: any) {
    errorQr.value = e.response?.data?.mensaje || e.message || 'El código QR no es válido o el carnet venció.'
  } finally {
    verificandoQr.value = false
  }
}

function cerrarPorAusenciaDeRostro() {
  if (verificandoFacial.value || rostroValidado.value) return
  mostrarFacial.value = false
  errorFacial.value = ''
  error.value = 'No se detectó un rostro durante 30 segundos. El reconocimiento facial se cerró; puedes intentarlo nuevamente cuando estés frente a la cámara.'
}

async function verificarRostro(captura: { descriptor: number[] | null, fotoRecortada: string | null }) {
  if (!captura.descriptor || !captura.fotoRecortada || verificandoFacial.value) return
  if (!permisoFacialActivo.value || intentosFacialesRestantes.value === 0) {
    errorFacial.value = 'El permiso de reconocimiento facial venció. Inicia nuevos intentos.'
    return
  }

  errorFacial.value = ''
  verificandoFacial.value = true

  try {
    const { data } = await api.post('/auth/login-facial', {
      descriptor: captura.descriptor
    })

    if (data.debeCambiarPassword) {
      mostrarFacial.value = false
      auth.login(data)
      abrirCambioPassword()
      return
    }

    auth.login(data)
    permisoFacialActivo.value = false
    fotoLoginRecortada.value = captura.fotoRecortada
    rostroValidado.value = true
    // Muestra la validación antes de redirigir.
    await new Promise(resolve => setTimeout(resolve, 1200))
    mostrarFacial.value = false
    await navigateTo(rutaDestino.value)
  } catch (e: any) {
    const estado = e.response?.status
    const restantes = Number(e.response?.data?.intentosRestantes)
    if (Number.isFinite(restantes)) {
      intentosFaciales.value = Math.max(0, maxIntentosFaciales - restantes)
    }

    if (estado === 429 || intentosFacialesRestantes.value === 0) {
      permisoFacialActivo.value = false
      errorFacial.value = 'Se agotaron los tres intentos. Cierra este diálogo e inicia nuevos intentos.'
      intentosFaciales.value = 0
    } else {
      errorFacial.value = estado === 401
        ? `Rostro no reconocido. Te quedan ${intentosFacialesRestantes.value} intento(s).`
        : e.response?.data?.mensaje || 'No fue posible validar el rostro. Intenta nuevamente.'

      if (estado === 400) {
        permisoFacialActivo.value = false
        intentosFaciales.value = 0
      }
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
  // No se restaura ni almacena una contraseña escrita. Al refrescar, los
  // campos quedan vacíos, pero el diálogo obligatorio vuelve a mostrarse.
  nuevaPassword.value = ''
  confirmarNuevaPassword.value = ''
  mostrarNuevaPassword.value = false
  mostrarConfirmacionPassword.value = false
  errorCambioPassword.value = ''
  mostrarCambioPassword.value = true
}

function abrirRecuperacion() {
  correoReset.value = ''
  errorReset.value = ''
  mensajeReset.value = ''
  mostrarReset.value = true
}

async function solicitarRestablecimiento() {
  errorReset.value = ''
  mensajeReset.value = ''
  if (!puedeSolicitarReset.value) {
    errorReset.value = 'Ingresa un correo válido y verifica que la protección de seguridad esté configurada.'
    return
  }

  enviandoReset.value = true
  try {
    const recaptchaToken = await tokenSeguridad('restablecimiento')
    const { data } = await api.post('/auth/solicitar-restablecimiento', {
      correo: correoReset.value,
      recaptchaToken
    })
    mensajeReset.value = data.mensaje
  } catch (e: any) {
    errorReset.value = e.response?.data?.mensaje || e.message || 'No fue posible enviar el enlace. Intenta nuevamente.'
  } finally {
    enviandoReset.value = false
  }
}

async function cambiarPasswordTemporal() {
  errorCambioPassword.value = ''
  if (!reglasPassword.value.every(regla => regla.cumple)) {
    errorCambioPassword.value = 'Usa entre 8 y 35 caracteres, con mayúscula, minúscula, número y símbolo.'
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
    nuevaPassword.value = ''
    confirmarNuevaPassword.value = ''
    mostrarNuevaPassword.value = false
    mostrarConfirmacionPassword.value = false
    await navigateTo(rutaDestino.value)
  } catch (e: any) {
    errorCambioPassword.value = e.response?.data?.mensaje || 'No se pudo actualizar la contraseña.'
  } finally {
    cambiandoPassword.value = false
  }
}

async function cancelarCambioPassword() {
  if (cambiandoPassword.value) return
  mostrarCambioPassword.value = false
  nuevaPassword.value = ''
  confirmarNuevaPassword.value = ''
  mostrarNuevaPassword.value = false
  mostrarConfirmacionPassword.value = false
  errorCambioPassword.value = ''
  try {
    await api.post('/auth/logout')
  } catch {
    // La limpieza local evita conservar una sesión temporal.
  }
  auth.logout()
}

function identificadorValido(valor: string) {
  const identificador = valor.trim()
  if (identificador.includes('@')) return identificador.length <= 30 && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(identificador)
  return /^[a-zA-Z0-9_.-]{3,25}$/.test(identificador)
}

async function continuarConGoogle() {
  error.value = ''
  if (!puedeEnviar.value) {
    error.value = 'Falta configurar la verificación de seguridad.'
    return
  }

  cargandoGoogle.value = true
  try {
    const recaptchaToken = await tokenSeguridad('google_login')
    await api.post('/auth/google/verificar-captcha', {
      recaptchaToken
    })
    window.location.assign(`${config.public.apiBase}/auth/google/iniciar`)
  } catch (e: any) {
    error.value = e.response?.data?.mensaje || e.message || 'No fue posible validar reCAPTCHA. Intenta nuevamente.'
    cargandoGoogle.value = false
  }
}
</script>

<style scoped>
.password-change-card {
  max-height: 92dvh;
  border: 1px solid #DCE5EC;
  border-radius: 16px;
  background: #fff;
}
.password-change-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  padding: 24px 24px 18px;
  border-bottom: 1px solid #E8ECF0;
}
.password-change-heading { display: flex; align-items: center; gap: 14px; min-width: 0; }
.password-change-icon {
  display: grid;
  flex: 0 0 44px;
  width: 44px;
  height: 44px;
  place-items: center;
  border-radius: 12px;
  color: #8A6814;
  background: #F8F1DD;
}
.password-change-eyebrow { margin: 0 0 3px; color: #8A6814; font-size: 10px; font-weight: 800; letter-spacing: .12em; }
.password-change-title { margin: 0; color: #051B2E; font-size: 20px; font-weight: 700; line-height: 1.25; }
.password-change-body { padding: 20px 24px 12px !important; }
.password-change-description { margin: 0 0 20px; color: #475467; font-size: 13px; line-height: 1.55; }
.password-strength { margin: 4px 0 18px; }
.password-strength-label { display: flex; justify-content: space-between; gap: 12px; margin-bottom: 9px; color: #344054; font-size: 12px; }
.password-strength-label strong { font-weight: 700; }
.password-strength-note { margin: 7px 0 0; color: #667085; font-size: 11px; line-height: 1.4; }
.password-rules {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px 14px;
  padding: 14px;
  border: 1px solid #E8ECF0;
  border-radius: 10px;
  background: #F8FAFC;
}
.password-rule { display: flex; align-items: flex-start; gap: 7px; color: #B42318; font-size: 12px; line-height: 1.35; }
.password-rule .v-icon { flex: 0 0 18px; }
.password-rule--met { color: #15803D; }
.password-confirmation { display: flex; align-items: center; gap: 7px; margin: 8px 0 0; color: #B42318; font-size: 12px; }
.password-confirmation--met { color: #15803D; }
.password-change-actions { justify-content: flex-end; gap: 8px; padding: 12px 24px 20px; border-top: 1px solid #E8ECF0; }
@media (max-width: 480px) {
  .password-change-header { padding: 20px 18px 16px; }
  .password-change-title { font-size: 18px; }
  .password-change-body { padding: 18px 18px 10px !important; }
  .password-rules { grid-template-columns: 1fr; }
  .password-change-actions { padding: 12px 18px 18px; }
}
/* El formulario se retira y entra en la dirección elegida, sin desmontar la tarjeta. */
.auth-to-register-enter-active,
.auth-to-register-leave-active,
.auth-to-login-enter-active,
.auth-to-login-leave-active {
  transform-style: preserve-3d;
  backface-visibility: hidden;
  will-change: opacity, transform;
}

.facial-success__photo { border: 3px solid #B48B21; }

.auth-to-register-leave-active,
.auth-to-login-leave-active {
  transition: opacity .22s ease-in, transform .28s cubic-bezier(.55, 0, .8, .2);
}

.auth-to-register-enter-active,
.auth-to-login-enter-active {
  transition: opacity .3s ease-out, transform .38s cubic-bezier(.16, 1, .3, 1);
}

.auth-to-register-leave-to,
.auth-to-login-enter-from {
  opacity: 0;
  transform: perspective(1200px) translateX(-38px) rotateY(32deg) scale(.97);
}

.auth-to-login-leave-to,
.auth-to-register-enter-from {
  opacity: 0;
  transform: perspective(1200px) translateX(38px) rotateY(-32deg) scale(.97);
}

.auth-to-register-enter-to,
.auth-to-register-leave-from,
.auth-to-login-enter-to,
.auth-to-login-leave-from {
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
.google-access-button :deep(.v-btn__content) { gap: 10px; justify-content: center; }
.google-mark {
  display: inline-grid;
  width: 20px;
  height: 20px;
  place-items: center;
  background: conic-gradient(from -42deg, #4285F4 0 25%, #34A853 25% 50%, #FBBC05 50% 75%, #EA4335 75% 100%);
  color: transparent;
  -webkit-background-clip: text;
  background-clip: text;
  font-family: Arial, sans-serif;
  font-size: 20px;
  font-weight: 700;
  line-height: 1;
}
.auth-access-tabs { position: relative; display: inline-grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 4px; padding: 4px; background: #F5F6F7; border: 1px solid #E2E4E8; border-radius: 999px; isolation: isolate; }
.auth-access-tabs::before { content: ''; position: absolute; z-index: -1; inset: 4px auto 4px 4px; width: calc(50% - 6px); background: #fff; border-radius: 999px; box-shadow: 0 2px 7px rgba(5,27,46,.12); transition: transform .66s cubic-bezier(.22, 1, .36, 1), box-shadow .3s ease; }
.auth-access-tabs button { padding: 6px 13px; border: 0; background: transparent; color: #6B7280; border-radius: 999px; font-size: 11px; font-weight: 700; cursor: pointer; transition: color .2s ease, transform .2s ease; }
.auth-access-tabs button.is-active { color: #051B2E; }
.auth-access-tabs button:hover, .auth-access-tabs button:focus-visible { color: #051B2E; transform: translateY(-1px); outline: none; }
.auth-access-tabs--registro::before { transform: translateX(calc(100% + 4px)); }
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
  .auth-to-register-enter-active, .auth-to-register-leave-active,
  .auth-to-login-enter-active, .auth-to-login-leave-active { transition: none; }
}
.login-register-link button { border: 0; background: transparent; color: #7A5E12; font: inherit; font-weight: 700; text-decoration: underline; cursor: pointer; }
.facial-success h2 { color: #051B2E; font-weight: 700; }
</style>
