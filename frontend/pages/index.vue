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

      <div class="text-center mt-5">
        <v-btn variant="text" color="primary" size="small" @click="mostrarReset = true">
          ¿Olvidaste tu contraseña?
        </v-btn>
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
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
useHead({ title: 'Iniciar sesión' })

const auth     = useAuthStore()
const mostrarPass  = ref(false)
const cargando     = ref(false)
const error        = ref('')
const mostrarReset = ref(false)
const accesoExitoso = ref(false)

// Acceso local provisional. Debe reemplazarse al conectar el backend y PostgreSQL.
const CLAVE_TEMPORAL = 'UMG2026'
const form = reactive({ password: '' })

onMounted(() => {
  auth.restore()
  if (auth.isAuthenticated) navigateTo('/dashboard')
})

async function loginPassword() {
  error.value = ''
  if (!form.password.trim()) {
    error.value = 'Ingresa la clave para continuar.'
    return
  }

  cargando.value = true
  if (form.password !== CLAVE_TEMPORAL) {
    error.value = 'La clave de acceso no es correcta.'
    cargando.value = false
    return
  }

  accesoExitoso.value = true
  auth.login({
    token: 'acceso-local-temporal',
    rol: 'ADMIN',
    nickname: 'Administrador',
    fotoModificada: null,
    expiracion: new Date(Date.now() + 8 * 60 * 60 * 1000).toISOString()
  })
  await new Promise(resolve => setTimeout(resolve, 650))
  await navigateTo('/dashboard')
  cargando.value = false
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
