```vue
<template>
  <div class="auth-page">
    <div class="auth-background" aria-hidden="true">
      <span class="auth-background__slide auth-background__slide--one"></span>
      <span class="auth-background__slide auth-background__slide--two"></span>
      <span class="auth-background__slide auth-background__slide--three"></span>
      <span class="auth-background__slide auth-background__slide--four"></span>
    </div>

    <div class="auth-shell">
      <Transition
        :name="direccionGiro === 'registro' ? 'flip-register' : 'flip-login'"
        mode="out-in"
      >
        <!-- LOGIN -->
        <div
          v-if="!modoRegistro"
          key="login"
          class="auth-panel"
        >
          <v-card
            :class="[
              'auth-card',
              'login-card',
              'pa-7',
              { 'login-card--success': accesoExitoso }
            ]"
            elevation="0"
          >
            <!-- Selector Ingresar / Crear cuenta -->
            <div
              class="auth-access-tabs"
              aria-label="Acceso y registro"
            >
              <button
                type="button"
                class="is-active"
                @click="cambiarModo('login')"
              >
                Ingresar
              </button>

              <button
                type="button"
                @click="cambiarModo('registro')"
              >
                Crear cuenta
              </button>
            </div>

            <!-- Logo -->
            <div class="text-center mb-6">
              <img
                class="auth-logo login-logo mx-auto mb-3"
                src="/images/logo-umg-oficial.png"
                alt="Universidad Mariano Gálvez de Guatemala"
              />
            </div>

            <!-- Formulario de login -->
            <v-form
              @submit.prevent="loginPassword"
              class="login-form"
            >
              <label
                class="login-field-label"
                for="identificador"
              >
                Correo o nickname
              </label>

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

              <label
                class="login-field-label"
                for="clave-acceso"
              >
                Clave de acceso
              </label>

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

              <p
                v-if="error"
                class="login-field-error"
              >
                <v-icon size="15" start>
                  mdi-alert-circle-outline
                </v-icon>

                {{ error }}
              </p>

              <RecaptchaV2
                v-if="requiereRecaptcha"
                v-model:token="recaptchaToken"
                :site-key="recaptchaSiteKey"
                class="mt-6"
              />

              <v-alert
                v-else-if="!bypassDesarrollo"
                type="warning"
                variant="tonal"
                density="compact"
                class="mt-6"
              >
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

            <!-- Login facial -->
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

            <!-- Opciones inferiores -->
            <div class="text-center mt-5">
              <v-btn
                variant="text"
                color="primary"
                size="small"
                @click="mostrarReset = true"
              >
                ¿Olvidaste tu contraseña?
              </v-btn>

              <p class="login-register-link">
                ¿Aún no tienes cuenta?

                <button
                  type="button"
                  @click="cambiarModo('registro')"
                >
                  Regístrate aquí
                </button>
              </p>
            </div>
          </v-card>
        </div>

        <!-- REGISTRO -->
        <div
          v-else
          key="registro"
          class="auth-panel"
        >
          <AuthRegistrationForm
            @cambiar-modo="cambiarModo"
          />
        </div>
      </Transition>
    </div>

    <!-- Reconocimiento facial -->
    <v-dialog
      v-model="mostrarFacial"
      max-width="440"
    >
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">
          Reconocimiento facial
        </v-card-title>

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

          <v-alert
            v-if="errorFacial"
            type="error"
            variant="tonal"
            density="compact"
            class="mb-2"
          >
            {{ errorFacial }}
          </v-alert>
        </v-card-text>

        <v-card-actions class="px-0 justify-end">
          <v-btn
            variant="outlined"
            @click="mostrarFacial = false"
          >
            Cancelar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Restablecer contraseña -->
    <v-dialog
      v-model="mostrarReset"
      max-width="420"
    >
      <v-card class="pa-6">
        <v-card-title class="px-0 text-h6">
          Restablecer contraseña
        </v-card-title>

        <v-card-text class="px-0">
          Ingresa el correo de tu cuenta para recibir las instrucciones de recuperación.
        </v-card-text>

        <v-card-actions class="px-0 justify-end">
          <v-btn
            class="umg-gold-button"
            color="accent"
            @click="mostrarReset = false"
          >
            Entendido
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'default'
})

useHead({
  title: 'Acceso | Analizador Léxico'
})

const auth = useAuthStore()
const route = useRoute()
const { api } = useApi()
const config = useRuntimeConfig()
const { calentar } = useFacialRecognition()

const mostrarPass = ref(false)
const cargando = ref(false)
const error = ref('')
const mostrarReset = ref(false)
const accesoExitoso = ref(false)

// Login facial
const mostrarFacial = ref(false)
const verificandoFacial = ref(false)
const errorFacial = ref('')
const escanerLogin = ref<{ reiniciar: () => void } | null>(null)

const form = reactive({
  identificador: '',
  password: ''
})

const recaptchaToken = ref('')

const recaptchaSiteKey = computed(() =>
  config.public.recaptchaSiteKey.trim()
)

const requiereRecaptcha = computed(() =>
  recaptchaSiteKey.value.length > 0
)

const bypassDesarrollo = computed(() =>
  import.meta.dev &&
  !requiereRecaptcha.value
)

const puedeEnviar = computed(() =>
  bypassDesarrollo.value ||
  recaptchaToken.value.length > 0
)

const modoRegistro = computed(() =>
  route.query.modo === 'registro'
)

const direccionGiro =
  ref<'login' | 'registro'>(
    modoRegistro.value
      ? 'registro'
      : 'login'
  )

function cambiarModo(
  modo: 'login' | 'registro'
) {
  const modoActual =
    modoRegistro.value
      ? 'registro'
      : 'login'

  if (modoActual === modo) {
    return
  }

  direccionGiro.value = modo

  navigateTo({
    path: '/login',

    query:
      modo === 'registro'
        ? { modo: 'registro' }
        : {}
  })
}

onMounted(() => {
  // Precarga los modelos para que el escaneo arranque sin esperas
  calentar()
  auth.restore()

  if (auth.isAuthenticated) {
    navigateTo('/dashboard')
  }
})

async function loginPassword() {
  error.value = ''

  if (
    !identificadorValido(
      form.identificador
    )
  ) {
    error.value =
      'Ingresa un correo válido o un nickname de 3 a 50 caracteres.'

    return
  }

  if (
    form.password.length < 8
  ) {
    error.value =
      'La contraseña debe tener al menos 8 caracteres.'

    return
  }

  if (!puedeEnviar.value) {
    error.value =
      'Completa la verificación reCAPTCHA para continuar.'

    return
  }

  cargando.value = true

  try {
    const { data } =
      await api.post(
        '/auth/login',
        {
          identificador:
            form.identificador,

          password:
            form.password,

          recaptchaToken:
            bypassDesarrollo.value
              ? 'dev-bypass'
              : recaptchaToken.value
        }
      )

    accesoExitoso.value =
      true

    auth.login(data)

    await new Promise(
      resolve =>
        setTimeout(
          resolve,
          650
        )
    )

    await navigateTo(
      '/dashboard'
    )
  }
  catch (e: any) {
    error.value =
      e.response
        ?.data
        ?.mensaje ||
      'No fue posible iniciar sesión.'
  }
  finally {
    cargando.value =
      false
  }
}

function abrirLoginFacial() {
  errorFacial.value = ''
  mostrarFacial.value = true
}

async function verificarRostro(
  captura: { descriptor: number[] | null }
) {
  if (!captura.descriptor || verificandoFacial.value) return

  errorFacial.value = ''
  verificandoFacial.value = true

  try {
    const { data } = await api.post('/auth/login-facial', {
      descriptor: captura.descriptor,
      recaptchaToken: bypassDesarrollo.value
        ? 'dev-bypass'
        : recaptchaToken.value
    })

    auth.login(data)
    mostrarFacial.value = false
    await navigateTo('/dashboard')
  }
  catch (e: any) {
    errorFacial.value =
      e.response?.data?.mensaje || 'Rostro no reconocido.'

    // Pausa breve para no saturar al servidor y luego vuelve a escanear
    setTimeout(() => {
      errorFacial.value = ''
      escanerLogin.value?.reiniciar()
    }, 1800)
  }
  finally {
    verificandoFacial.value = false
  }
}

function identificadorValido(
  valor: string
) {
  const identificador =
    valor.trim()

  if (
    identificador.includes('@')
  ) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/
      .test(
        identificador
      )
  }

  return /^[a-zA-Z0-9_.-]{3,50}$/
    .test(
      identificador
    )
}
</script>

<style scoped>
/* =============================
   CONTENEDOR
============================= */

.auth-shell {
  position: relative;

  width:
    min(92vw, 540px);

  margin:
    0 auto;

  perspective:
    1500px;
}

.auth-panel {
  width:
    100%;

  transform-style:
    preserve-3d;

  backface-visibility:
    hidden;

  -webkit-backface-visibility:
    hidden;
}


/* =============================
   CARD LOGIN
============================= */

.login-card {
  width:
    100%;

  max-width:
    540px;

  margin:
    0 auto;

  border-radius:
    18px;
}


/* =============================
   PESTAÑAS
============================= */

.auth-access-tabs {
  display:
    inline-flex;

  align-self:
    center;

  gap:
    4px;

  padding:
    4px;

  margin:
    0 auto 24px;

  background:
    #F5F6F7;

  border:
    1px solid #E2E4E8;

  border-radius:
    999px;

  box-shadow:
    0 2px 7px
    rgba(5,27,46,.08);
}

.auth-access-tabs button {
  padding:
    8px 17px;

  border:
    0;

  background:
    transparent;

  color:
    #6B7280;

  border-radius:
    999px;

  font-size:
    12px;

  font-weight:
    700;

  cursor:
    pointer;

  transition:
    background .25s ease,
    color .25s ease,
    box-shadow .25s ease;
}

.auth-access-tabs button.is-active {
  color:
    #051B2E;

  background:
    #FFFFFF;

  box-shadow:
    0 2px 7px
    rgba(5,27,46,.12);
}


/* =============================
   TRANSICIÓN LOGIN -> REGISTRO
============================= */

.flip-register-leave-active,
.flip-register-enter-active {
  transition:
    transform .38s
      cubic-bezier(.4,0,.2,1),
    opacity .28s ease;
}

.flip-register-leave-from {
  opacity:
    1;

  transform:
    rotateY(0deg)
    scale(1);
}

.flip-register-leave-to {
  opacity:
    0;

  transform:
    rotateY(-90deg)
    scale(.97);
}

.flip-register-enter-from {
  opacity:
    0;

  transform:
    rotateY(90deg)
    scale(.97);
}

.flip-register-enter-to {
  opacity:
    1;

  transform:
    rotateY(0deg)
    scale(1);
}


/* =============================
   TRANSICIÓN REGISTRO -> LOGIN
============================= */

.flip-login-leave-active,
.flip-login-enter-active {
  transition:
    transform .38s
      cubic-bezier(.4,0,.2,1),
    opacity .28s ease;
}

.flip-login-leave-from {
  opacity:
    1;

  transform:
    rotateY(0deg)
    scale(1);
}

.flip-login-leave-to {
  opacity:
    0;

  transform:
    rotateY(90deg)
    scale(.97);
}

.flip-login-enter-from {
  opacity:
    0;

  transform:
    rotateY(-90deg)
    scale(.97);
}

.flip-login-enter-to {
  opacity:
    1;

  transform:
    rotateY(0deg)
    scale(1);
}


/* =============================
   LOGO
============================= */

.auth-logo {
  width:
    154px;

  height:
    154px;

  object-fit:
    contain;

  background:
    transparent;

  border:
    0;

  border-radius:
    50%;

  display:
    flex;

  align-items:
    center;

  justify-content:
    center;

  outline:
    none;

  box-shadow:
    0 0 18px 8px
      rgba(180,139,33,.38),

    0 14px 28px
      rgba(5,27,46,.22);
}


/* =============================
   CAMPOS LOGIN
============================= */

.login-field-label {
  display:
    block;

  margin-bottom:
    8px;

  color:
    #1A1F2B;

  font-size:
    13px;

  font-weight:
    700;

  letter-spacing:
    .01em;
}

:deep(
  .login-password-field
  .v-field
) {
  min-height:
    56px;

  border-radius:
    9px;

  background:
    #F5F6F7;
}

:deep(
  .login-password-field
  .v-field--focused
) {
  box-shadow:
    0 0 0 3px
    rgba(26,80,128,.14);
}

.login-field-error {
  display:
    flex;

  align-items:
    center;

  margin:
    8px 0 0;

  color:
    #B42318;

  font-size:
    12px;

  line-height:
    1.35;
}


/* =============================
   ENLACE REGISTRO
============================= */

.login-register-link {
  margin:
    8px 0 0;

  color:
    #6B7280;

  font-size:
    12px;
}

.login-register-link button {
  border:
    0;

  background:
    transparent;

  color:
    #7A5E12;

  font:
    inherit;

  font-weight:
    700;

  text-decoration:
    underline;

  cursor:
    pointer;
}


/* =============================
   RESPONSIVE
============================= */

@media (max-width: 600px) {
  .auth-shell {
    width:
      calc(100vw - 24px);
  }

  .auth-access-tabs {
    margin-bottom:
      18px;
  }

  .auth-access-tabs button {
    padding:
      7px 14px;

    font-size:
      11px;
  }
}
</style>
```