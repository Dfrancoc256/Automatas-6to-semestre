<template>
  <div class="auth-page">
    <div class="auth-background" aria-hidden="true">
      <span class="auth-background__slide auth-background__slide--one" />
      <span class="auth-background__slide auth-background__slide--two" />
      <span class="auth-background__slide auth-background__slide--three" />
      <span class="auth-background__slide auth-background__slide--four" />
    </div>

    <v-card :class="['auth-card', 'login-card', 'pa-8', { 'login-card--success': accesoExitoso }]" elevation="0">
      <div class="auth-access-tabs" aria-label="Acceso y registro">
        <NuxtLink to="/login" class="is-active">Ingresar</NuxtLink>
        <NuxtLink to="/registro">Crear cuenta</NuxtLink>
      </div>
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

      <div class="text-center mt-5">
        <v-btn variant="text" color="primary" size="small" @click="mostrarReset = true">
          ¿Olvidaste tu contraseña?
        </v-btn>
        <p class="login-register-link">¿Aún no tienes cuenta? <NuxtLink to="/registro">Regístrate aquí</NuxtLink></p>
      </div>
    </v-card>

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
const { api }  = useApi()
const config = useRuntimeConfig()
const mostrarPass  = ref(false)
const cargando     = ref(false)
const error        = ref('')
const mostrarReset = ref(false)
const accesoExitoso = ref(false)

const form = reactive({ identificador: '', password: '' })
const recaptchaToken = ref('')
const recaptchaSiteKey = computed(() => config.public.recaptchaSiteKey.trim())
const requiereRecaptcha = computed(() => recaptchaSiteKey.value.length > 0)
const bypassDesarrollo = computed(() => import.meta.dev && !requiereRecaptcha.value)
const puedeEnviar = computed(() => bypassDesarrollo.value || recaptchaToken.value.length > 0)

onMounted(() => {
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

function identificadorValido(valor: string) {
  const identificador = valor.trim()
  if (identificador.includes('@')) return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(identificador)
  return /^[a-zA-Z0-9_.-]{3,50}$/.test(identificador)
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

.login-register-link { margin: 8px 0 0; color: #6B7280; font-size: 12px; }
.login-register-link a { color: #7A5E12; font-weight: 700; }
.auth-access-tabs { display: inline-flex; align-self: center; gap: 4px; padding: 4px; margin-bottom: 26px; background: #F5F6F7; border: 1px solid #E2E4E8; border-radius: 999px; }
.auth-access-tabs a { padding: 8px 17px; color: #6B7280; border-radius: 999px; font-size: 12px; font-weight: 700; text-decoration: none; }
.auth-access-tabs a.is-active { color: #051B2E; background: #fff; box-shadow: 0 2px 7px rgba(5,27,46,.12); }
</style>
